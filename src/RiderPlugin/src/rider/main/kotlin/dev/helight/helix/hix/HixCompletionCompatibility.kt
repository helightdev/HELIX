package dev.helight.helix.hix

/** Conservative UI ranking. Unknown schemas stay available; impossible known kinds are excluded. */
internal object HixCompletionCompatibility {
    enum class Match(val priority: Double) { Exact(1000.0), Compatible(800.0), Unknown(400.0), Conversion(200.0), Impossible(0.0) }
    private val primitives = setOf("any", "string", "number", "bool", "null", "missing", "opaque", "tuple", "table", "function", "error", "pattern")
    private fun kind(type: String): String? {
        val value = type.trim()
        if (value.lowercase() in primitives) return value.lowercase()
        if (value == "{}" || value.startsWith("@{") || value.startsWith("%map")) return "table"
        if (value == "[]" || value.startsWith("@[") || value.startsWith("%many")) return "tuple"
        if (value.startsWith("delegate")) return "function"
        if (value.startsWith("task<")) return "opaque"
        return null
    }
    fun match(actual: String, expected: String, bounds: Map<String, String> = emptyMap()): Match {
        val left = actual.trim(); val right = expected.trim()
        if (left == right) return Match.Exact
        if (left in bounds && bounds[left] != left) return match(bounds.getValue(left), right, bounds - left)
        if (left.isEmpty() || left == "any") return Match.Unknown
        if (right == "any") return Match.Compatible
        if (right in bounds) {
            val bound = bounds.getValue(right)
            return if (bound == "any" || bound == right) Match.Compatible else match(left, bound, bounds - right)
        }
        if (kind(left) != null && kind(left) == kind(right) && left.lowercase() in primitives && right.lowercase() in primitives) return Match.Exact
        if (right.endsWith('?')) return if (left == "null") Match.Compatible else match(left, right.dropLast(1), bounds)
        if ('|' in right) return right.removeSurrounding("(", ")").split('|').map { match(left, it, bounds) }.maxBy { it.priority }
        if (left.endsWith('?') || '|' in left) return Match.Unknown
        val actualKind = kind(left); val expectedKind = kind(right)
        if (actualKind == null || expectedKind == null) return Match.Unknown
        if (actualKind == expectedKind) return Match.Compatible
        if (actualKind in setOf("string", "number", "bool") && expectedKind in setOf("string", "number", "bool")) return Match.Conversion
        return Match.Impossible
    }
}
