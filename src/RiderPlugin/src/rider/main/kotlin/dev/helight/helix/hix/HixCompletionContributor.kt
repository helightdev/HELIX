package dev.helight.helix.hix

import com.intellij.codeInsight.completion.CompletionContributor
import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.CompletionType
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext
import dev.helight.helix.hix.generated.HixLexer

/** Syntax completion is local and uses the same generated token stream as PSI. */
class HixCompletionContributor : CompletionContributor() {
    init {
        extend(CompletionType.BASIC, PlatformPatterns.psiElement().withLanguage(HixLanguage),
            object : CompletionProvider<CompletionParameters>() {
                override fun addCompletions(parameters: CompletionParameters, context: ProcessingContext,
                                            result: CompletionResultSet) {
                    val source = parameters.originalFile.text
                    val offset = parameters.offset.coerceIn(0, source.length)
                    val parsed = HixAntlrSyntax.parse(source)
                    val token = parsed.tokens.firstOrNull { it.start < offset && offset <= it.end }
                    if (token?.type in setOf(HixLexer.ARGUMENT_TEXT, HixLexer.CONTENT_TEXT, HixLexer.COMMENT,
                            HixLexer.SLASH_COMMENT, HixLexer.ESCAPE_LITERAL, HixLexer.ESCAPE_HEX)) return
                    val names = linkedSetOf<String>()
                    for (index in 1..HixLexer.VOCABULARY.maxTokenType) {
                        val name = HixLexer.VOCABULARY.getSymbolicName(index).orEmpty()
                        if (name.startsWith("KEYWORD_"))
                            HixLexer.VOCABULARY.getLiteralName(index)?.trim('\'')?.let(names::add)
                    }
                    names += listOf("this", "target", "attr", "local", "var", "tar", "param",
                        "true", "false", "null", "table", "tuple", "string", "number", "bool", "error", "symbol",
                        "kind", "function")
                    HixAntlrSyntax.declarationNames(parsed.tree).forEach { names += it.text }
                    names.forEach { result.addElement(LookupElementBuilder.create(it)) }
                }
            })
    }
}
