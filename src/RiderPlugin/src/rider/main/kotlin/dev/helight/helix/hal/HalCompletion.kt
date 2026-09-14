package dev.helight.helix.hal

import dev.helight.helix.hal.generated.HalLexer
import dev.helight.helix.hal.generated.HalParser
import dev.helight.helix.hix.HixCompletionCompatibility
import dev.helight.helix.hix.HixEditorCalls
import dev.helight.helix.hix.HixLookup
import org.antlr.v4.runtime.ParserRuleContext

/** Syntax-scoped HAL completions for data constructors and explicit references. */
internal object HalCompletion {
    data class Item(val name: String, val insert: String, val kind: String, val type: String, val priority: Double)
    private fun rules(rule: ParserRuleContext): Sequence<ParserRuleContext> = sequence {
        yield(rule)
        rule.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { yieldAll(rules(it)) }
    }
    fun prefix(source: String, offset: Int): String = source.take(offset.coerceIn(0, source.length))
        .takeLastWhile { it.isLetterOrDigit() || it in "_.-" }

    fun items(source: String, offset: Int, schema: String, isManifest: Boolean): List<Item> {
        val parsed = HalAntlrSyntax.parse(source)
        val token = parsed.tokens.firstOrNull { it.start < offset && offset <= it.end }
        if (token?.type in setOf(HalLexer.LINE_COMMENT, HalLexer.STRING)) return emptyList()
        val before = source.substring(0, offset)
        val line = before.substringAfterLast('\n').trimStart()
        val types = (listOf("table", "tuple") + HalManifest.types(schema) + if (isManifest) listOf("Manifest") else emptyList()).distinct()
        val shapes = HixLookup.patterns(dev.helight.helix.hix.HixAntlrSyntax.parse(schema)).associateBy { it.name }
        if (line.startsWith("---")) {
            if ('&' in line) return emptyList()
            return types.map { Item(it, it, "Pattern", "Hix pattern", if (shapes[it] != null) 1000.0 else 350.0) }
        }
        val call = HixEditorCalls.at(HixEditorCalls.hal(source), offset)
        if (call != null) {
            if (call.name == "asset" && call.currentArgument(offset) == 0)
                return listOf(Item("path", "path(\"\")", "Function", "asset file path", 1000.0),
                    Item("uid", "uid(\"\")", "Function", "persistent asset UID", 1000.0))
            if (call.name != "ref" || call.currentArgument(offset) > 0) return emptyList()
            return HalStructure.sections(source).mapNotNull { section -> section.id?.let {
                val name = it.removeSurrounding("\"")
                Item(name, HixLookup.fieldName(name), "Reference", section.type, 1000.0)
            } }.distinctBy { it.name }
        }
        val nodes = rules(parsed.tree).toList()
        val selection = nodes.filterIsInstance<HalParser.SelectionContext>().firstOrNull {
            offset >= it.start.startIndex && offset <= it.stop.stopIndex + 1
        }
        if (selection != null || before.trimEnd().endsWith('#')) {
            val value = selection?.parent as? HalParser.ValueContext
            val reference = value?.call()?.takeIf { it.IDENTIFIER()?.text == "ref" }
            val id = reference?.argumentList()?.argument()?.singleOrNull()?.value()?.text?.removeSurrounding("\"")
            val type = value?.IDENTIFIER()?.text ?: HalStructure.sections(source).firstOrNull { it.id?.removeSurrounding("\"") == id }?.type
            return type?.let { HalManifest.fieldDefinitions(schema, it).filter { field -> Regex("[A-Za-z_][A-Za-z0-9_.-]*").matches(field.name) }.map { field ->
                Item(field.name, field.name, "Field", field.type, 1200.0)
            } }.orEmpty()
        }
        val scope = HalStructure.completionScope(source, offset) ?: return emptyList()
        fun fields(type: String) = when {
            isManifest && type == "Manifest" -> HalManifest.manifestFields
            isManifest && type == "ManifestSources" -> HalManifest.sourceFields
            else -> HalManifest.fieldDefinitions(schema, type)
        }
        // A list is a value scope, even on blank lines. Only a nested table
        // may restore field completion inside it.
        val container = nodes.filter { it is HalParser.ListContext || it is HalParser.TableContext }.filter {
            val closing = when (it) {
                is HalParser.ListContext -> it.RBRACKET()?.symbol
                is HalParser.TableContext -> it.RBRACE()?.symbol
                else -> null
            }
            it.start.startIndex <= offset && (offset < (closing?.startIndex ?: -1) ||
                closing == null || closing.startIndex < 0)
        }.maxByOrNull { it.start.startIndex }
        val list = container as? HalParser.ListContext
        val owner = list?.let { generateSequence(it.parent as? ParserRuleContext) { node -> node.parent as? ParserRuleContext }
            .firstOrNull { node -> node is HalParser.FieldContext || node is HalParser.TableEntryContext } }
        val listKey = when (owner) {
            is HalParser.FieldContext -> owner.fieldKey()
            is HalParser.TableEntryContext -> owner.fieldKey()
            else -> null
        }
        val listScope = listKey?.let { HalStructure.completionScope(source, it.start.startIndex) }
        val listField = listScope?.let { fields(it.first).firstOrNull { field -> field.name == listKey?.text?.removeSurrounding("\"") } }
        val elementType = listField?.let { if (it.many) it.type else HalManifest.manyElement(schema, it.type) }
            ?: (list?.parent as? HalParser.SectionContext)?.let { HalManifest.manyElement(schema, it.IDENTIFIER().text) }
        val key = nodes.filterIsInstance<HalParser.FieldKeyContext>().firstOrNull {
            offset >= it.start.startIndex && offset <= it.stop.stopIndex + 1
        }
        val fieldPosition = key != null || line.isBlank() || line.takeLastWhile { it !in "{,=" }.isBlank() && !line.endsWith('=')
        if (fieldPosition && list == null) return fields(scope.first).filter { it.name !in scope.second || it.name == key?.text?.removeSurrounding("\"") }
            .map { Item(it.name, HixLookup.fieldName(it.name) + " = ", "Field", it.type, if (it.required) 1200.0 else 950.0) }
        val field = nodes.filter { it is HalParser.FieldContext || it is HalParser.TableEntryContext }.filter {
            offset >= it.start.startIndex && offset <= (it.stop?.stopIndex ?: source.length) + 1
        }.minByOrNull { (it.stop?.stopIndex ?: source.length) - it.start.startIndex }
        val fieldKey = when (field) { is HalParser.FieldContext -> field.fieldKey(); is HalParser.TableEntryContext -> field.fieldKey(); else -> null }
        val expected = elementType ?: fields(scope.first).firstOrNull { it.name == fieldKey?.text?.removeSurrounding("\"") }?.type ?: "any"
        val alternatives = HalManifest.unionMembers(schema, expected).toSet() + expected.split('|').map { it.trim().removeSuffix("?") }
        val results = arrayListOf<Item>()
        if (scope.first == "ManifestDependencies") {
            listOf("ModuleDependency" to "ModuleDependency {}", "PathDependency" to "PathDependency { uri =  }",
                "GitDependency" to "GitDependency { repository =  }").forEach { (name, insert) ->
                results += Item(name, insert, "Dependency", "Hix dependency", 1000.0)
            }
            return results
        }
        types.forEach { type ->
            if (HalManifest.unionMembers(schema, type).isNotEmpty()) return@forEach
            val exact = type in alternatives
            val tuple = shapes[type]?.tuple == true || HalManifest.manyElement(schema, type) != null
            val match = if (exact) HixCompletionCompatibility.Match.Exact else HixCompletionCompatibility.match(if (tuple) "tuple" else "table", expected)
            if (match == HixCompletionCompatibility.Match.Impossible || (HalManifest.unionMembers(schema, expected).isNotEmpty() && !exact)) return@forEach
            val required = fields(expected).filter { it.required }.map { it.name }.toSet()
            val supplied = fields(type).map { it.name }.toSet()
            if (!exact && required.isNotEmpty() && !supplied.containsAll(required)) return@forEach
            results += Item(type, "$type " + if (tuple) "[]" else "{}", "Constructor", type, if (exact) 1000.0 else 350.0)
        }
        if (expected in setOf("bool", "bool?", "any")) listOf("true", "false").forEach {
            results += Item(it, it, "Keyword", "bool", if (expected == "any") 300.0 else 1000.0)
        }
        if (expected == "any" || expected.endsWith('?') || "null" in alternatives)
            results += Item("null", "null", "Keyword", "null", 300.0)
        // A reference may be followed by #field; offer it below direct schema constructors.
        results += Item("ref", "ref()", "Function", "local section", 250.0)
        results += Item("asset", "asset()", "Function", "foreign asset", 250.0)
        return results.distinctBy { it.insert }
    }
}
