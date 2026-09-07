package dev.helight.helix.mixin

import com.intellij.codeInsight.completion.CompletionContributor
import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.CompletionType
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext
import dev.helight.helix.mixin.generated.MixinLexer

/** Syntax completion is local and uses the same generated token stream as PSI. */
class HelixMixinCompletionContributor : CompletionContributor() {
    init {
        extend(CompletionType.BASIC, PlatformPatterns.psiElement().withLanguage(HelixMixinLanguage),
            object : CompletionProvider<CompletionParameters>() {
                override fun addCompletions(parameters: CompletionParameters, context: ProcessingContext,
                                            result: CompletionResultSet) {
                    val source = parameters.originalFile.text
                    val offset = parameters.offset.coerceIn(0, source.length)
                    val parsed = HelixMixinAntlrSyntax.parse(source)
                    val token = parsed.tokens.firstOrNull { it.start < offset && offset <= it.end }
                    if (token?.type in setOf(MixinLexer.ARGUMENT_TEXT, MixinLexer.CONTENT_TEXT, MixinLexer.COMMENT,
                            MixinLexer.SLASH_COMMENT, MixinLexer.ESCAPE_LITERAL, MixinLexer.ESCAPE_HEX)) return
                    val names = linkedSetOf<String>()
                    for (index in 1..MixinLexer.VOCABULARY.maxTokenType) {
                        val name = MixinLexer.VOCABULARY.getSymbolicName(index).orEmpty()
                        if (name.startsWith("KEYWORD_"))
                            MixinLexer.VOCABULARY.getLiteralName(index)?.trim('\'')?.let(names::add)
                    }
                    names += listOf("this", "target", "attr", "local", "var", "tar", "carry", "param",
                        "true", "false", "null", "table", "tuple", "string", "number", "bool", "error", "symbol",
                        "kind", "function")
                    HelixMixinAntlrSyntax.declarationNames(parsed.tree).forEach { names += it.text }
                    names.forEach { result.addElement(LookupElementBuilder.create(it)) }
                }
            })
    }
}
