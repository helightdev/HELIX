package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.*
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rd.generator.nova.kotlin.Kotlin11Generator
import com.jetbrains.rider.model.nova.ide.SolutionModel

/**
 * Versioned language-service protocol for mixin additional files.
 *
 * The backend is deliberately an analysis server, not the owner of editor PSI. The frontend
 * sends a complete same-directory file set and receives diagnostics and semantic facts. IntelliJ
 * syntax and PSI are built directly from the shared ANTLR grammar in the frontend.
 */
@Suppress("unused")
object HelixExpressionModel : Ext(SolutionModel.Solution) {
    init {
        setting(CSharp50Generator.Namespace, "HelixRider.Protocol")
        setting(Kotlin11Generator.Namespace, "dev.helight.helix.protocol")

        val sourceRange = structdef("mixinSourceRange") {
            field("startOffset", int)
            field("endOffset", int)
        }

        val fileInput = structdef("mixinFileInput") {
            field("filePath", string)
            field("sourceText", string)
            field("revision", long)
        }

        val parseRequest = structdef("mixinParseRequest") {
            field("files", array(fileInput))
            field("backend", string)
        }

        val declaration = structdef("mixinDeclaration") {
            field("name", string)
            field("kind", string)
            field("range", sourceRange)
            field("scope", sourceRange)
        }

        val reference = structdef("mixinReference") {
            field("name", string)
            field("kind", string)
            field("range", sourceRange)
            field("scope", sourceRange)
            field("targetFilePath", string)
            field("targetRange", sourceRange)
        }

        val diagnostic = structdef("mixinDiagnostic") {
            field("message", string)
            field("severity", string)
            field("range", sourceRange)
        }

        val completionItem = structdef("mixinCompletionItem") {
            field("name", string)
            field("insertText", string)
            field("kind", string)
            field("documentation", string)
            field("targetFilePath", string)
            field("targetRange", sourceRange)
        }

        val completionRequest = structdef("mixinCompletionRequest") {
            field("kind", string)
            field("prefix", string)
            field("backend", string)
        }

        val completionResponse = structdef("mixinCompletionResponse") {
            field("items", array(completionItem))
        }

        val typeFact = structdef("mixinTypeFact") {
            field("range", sourceRange)
            field("type", string)
            field("documentation", string)
            field("inlay", bool)
            field("kind", string)
        }

        val fileSnapshot = structdef("mixinFileSnapshot") {
            field("filePath", string)
            field("revision", long)
            field("sourceHash", long)
            field("declarations", array(declaration))
            field("references", array(reference))
            field("diagnostics", array(diagnostic))
            field("typeFacts", array(typeFact))
        }

        val languageDefinition = structdef("mixinLanguageDefinition") {
            field("name", string)
            field("kind", string)
            field("argumentCount", int)
            field("variadic", bool)
            field("operandType", string)
            field("receiverType", string)
            field("resultType", string)
            field("argumentTypes", array(string))
            field("documentation", string)
        }

        val definitionQuery = structdef("mixinDefinitionQuery") {
            field("kind", string)
            field("receiverType", string)
            field("operandType", string)
            field("prefix", string)
            field("backend", string)
        }

        val definitionResponse = structdef("mixinDefinitionResponse") {
            field("definitions", array(languageDefinition))
        }

        val parseResponse = structdef("mixinParseResponse") {
            field("files", array(fileSnapshot))
        }

        val languageCatalog = structdef("mixinLanguageCatalog") {
            field("definitions", array(languageDefinition))
        }

        call("parseMixinFiles", parseRequest, parseResponse).async
        call("completeMixin", completionRequest, completionResponse).async
        call("queryMixinDefinitions", definitionQuery, definitionResponse).async
        call("getMixinLanguageCatalog", string, languageCatalog).async
        property("isHelixEnabled", bool)
    }
}
