@file:Suppress("EXPERIMENTAL_API_USAGE","EXPERIMENTAL_UNSIGNED_LITERALS","PackageDirectoryMismatch","UnusedImport","unused","LocalVariableName","CanBeVal","PropertyName","EnumEntryName","ClassName","ObjectPropertyName","UnnecessaryVariable","SpellCheckingInspection")
package dev.helight.helix.protocol

import com.jetbrains.rd.framework.*
import com.jetbrains.rd.framework.base.*
import com.jetbrains.rd.framework.impl.*

import com.jetbrains.rd.util.lifetime.*
import com.jetbrains.rd.util.reactive.*
import com.jetbrains.rd.util.string.*
import com.jetbrains.rd.util.*
import kotlin.time.Duration
import kotlin.reflect.KClass
import kotlin.jvm.JvmStatic



/**
 * #### Generated from [HelixExpressionModel.kt:16]
 */
class HelixExpressionModel private constructor(
    private val _parseMixinFiles: RdCall<MixinParseRequest, MixinParseResponse>,
    private val _getMixinLanguageCatalog: RdCall<Boolean, MixinLanguageCatalog>,
    private val _isHelixEnabled: RdOptionalProperty<Boolean>
) : RdExtBase() {
    //companion

    companion object : ISerializersOwner {

        override fun registerSerializersCore(serializers: ISerializers)  {
            val classLoader = javaClass.classLoader
            serializers.register(LazyCompanionMarshaller(RdId(5218098678922832084), classLoader, "dev.helight.helix.protocol.MixinSourceRange"))
            serializers.register(LazyCompanionMarshaller(RdId(4113226335682446112), classLoader, "dev.helight.helix.protocol.MixinFileInput"))
            serializers.register(LazyCompanionMarshaller(RdId(-4347416270738964566), classLoader, "dev.helight.helix.protocol.MixinParseRequest"))
            serializers.register(LazyCompanionMarshaller(RdId(-1616834679433244845), classLoader, "dev.helight.helix.protocol.MixinSyntaxNode"))
            serializers.register(LazyCompanionMarshaller(RdId(17701739849713259), classLoader, "dev.helight.helix.protocol.MixinToken"))
            serializers.register(LazyCompanionMarshaller(RdId(5205524339893740652), classLoader, "dev.helight.helix.protocol.MixinDeclaration"))
            serializers.register(LazyCompanionMarshaller(RdId(4113236455037010621), classLoader, "dev.helight.helix.protocol.MixinReference"))
            serializers.register(LazyCompanionMarshaller(RdId(-1617245288858810763), classLoader, "dev.helight.helix.protocol.MixinDiagnostic"))
            serializers.register(LazyCompanionMarshaller(RdId(-1838348744103421731), classLoader, "dev.helight.helix.protocol.MixinCompletionItem"))
            serializers.register(LazyCompanionMarshaller(RdId(-1838348744103133935), classLoader, "dev.helight.helix.protocol.MixinCompletionSite"))
            serializers.register(LazyCompanionMarshaller(RdId(-7603046265752210668), classLoader, "dev.helight.helix.protocol.MixinTypeFact"))
            serializers.register(LazyCompanionMarshaller(RdId(-4595115062107102546), classLoader, "dev.helight.helix.protocol.MixinFileSnapshot"))
            serializers.register(LazyCompanionMarshaller(RdId(-559598091605421159), classLoader, "dev.helight.helix.protocol.MixinLanguageDefinition"))
            serializers.register(LazyCompanionMarshaller(RdId(-5642695876888106362), classLoader, "dev.helight.helix.protocol.MixinParseResponse"))
            serializers.register(LazyCompanionMarshaller(RdId(-5239733744395790445), classLoader, "dev.helight.helix.protocol.MixinLanguageCatalog"))
        }





        const val serializationHash = -3499272926308586000L

    }
    override val serializersOwner: ISerializersOwner get() = HelixExpressionModel
    override val serializationHash: Long get() = HelixExpressionModel.serializationHash

    //fields
    val parseMixinFiles: IRdCall<MixinParseRequest, MixinParseResponse> get() = _parseMixinFiles
    val getMixinLanguageCatalog: IRdCall<Boolean, MixinLanguageCatalog> get() = _getMixinLanguageCatalog
    val isHelixEnabled: IOptProperty<Boolean> get() = _isHelixEnabled
    //methods
    //initializer
    init {
        _isHelixEnabled.optimizeNested = true
    }

    init {
        _parseMixinFiles.async = true
        _getMixinLanguageCatalog.async = true
    }

    init {
        bindableChildren.add("parseMixinFiles" to _parseMixinFiles)
        bindableChildren.add("getMixinLanguageCatalog" to _getMixinLanguageCatalog)
        bindableChildren.add("isHelixEnabled" to _isHelixEnabled)
    }

    //secondary constructor
    internal constructor(
    ) : this(
        RdCall<MixinParseRequest, MixinParseResponse>(MixinParseRequest, MixinParseResponse),
        RdCall<Boolean, MixinLanguageCatalog>(FrameworkMarshallers.Bool, MixinLanguageCatalog),
        RdOptionalProperty<Boolean>(FrameworkMarshallers.Bool)
    )

    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("HelixExpressionModel (")
        printer.indent {
            print("parseMixinFiles = "); _parseMixinFiles.print(printer); println()
            print("getMixinLanguageCatalog = "); _getMixinLanguageCatalog.print(printer); println()
            print("isHelixEnabled = "); _isHelixEnabled.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): HelixExpressionModel   {
        return HelixExpressionModel(
            _parseMixinFiles.deepClonePolymorphic(),
            _getMixinLanguageCatalog.deepClonePolymorphic(),
            _isHelixEnabled.deepClonePolymorphic()
        )
    }
    //contexts
    //threading
    override val extThreading: ExtThreadingKind get() = ExtThreadingKind.Default
}
val com.jetbrains.rd.ide.model.Solution.helixExpressionModel get() = getOrCreateExtension("helixExpressionModel", ::HelixExpressionModel)



/**
 * #### Generated from [HelixExpressionModel.kt:73]
 */
data class MixinCompletionItem (
    val name: String,
    val insertText: String,
    val kind: String,
    val documentation: String,
    val targetFilePath: String,
    val targetRange: MixinSourceRange
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(name)
        buffer.writeString(insertText)
        buffer.writeString(kind)
        buffer.writeString(documentation)
        buffer.writeString(targetFilePath)
        MixinSourceRange.write(ctx, buffer, targetRange)
    }
    //companion

    companion object : IMarshaller<MixinCompletionItem> {
        override val _type: KClass<MixinCompletionItem> = MixinCompletionItem::class
        override val id: RdId get() = RdId(-1838348744103421731)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinCompletionItem  {
            val name = buffer.readString()
            val insertText = buffer.readString()
            val kind = buffer.readString()
            val documentation = buffer.readString()
            val targetFilePath = buffer.readString()
            val targetRange = MixinSourceRange.read(ctx, buffer)
            return MixinCompletionItem(name, insertText, kind, documentation, targetFilePath, targetRange)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinCompletionItem)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinCompletionItem

        if (name != other.name) return false
        if (insertText != other.insertText) return false
        if (kind != other.kind) return false
        if (documentation != other.documentation) return false
        if (targetFilePath != other.targetFilePath) return false
        if (targetRange != other.targetRange) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + insertText.hashCode()
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + documentation.hashCode()
        __r = __r*31 + targetFilePath.hashCode()
        __r = __r*31 + targetRange.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinCompletionItem (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("insertText = "); insertText.print(printer); println()
            print("kind = "); kind.print(printer); println()
            print("documentation = "); documentation.print(printer); println()
            print("targetFilePath = "); targetFilePath.print(printer); println()
            print("targetRange = "); targetRange.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:82]
 */
data class MixinCompletionSite (
    val kind: String,
    val activationRange: MixinSourceRange,
    val replacementRange: MixinSourceRange,
    val receiverType: String,
    val items: Array<MixinCompletionItem>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(kind)
        MixinSourceRange.write(ctx, buffer, activationRange)
        MixinSourceRange.write(ctx, buffer, replacementRange)
        buffer.writeString(receiverType)
        buffer.writeArray(items) { MixinCompletionItem.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<MixinCompletionSite> {
        override val _type: KClass<MixinCompletionSite> = MixinCompletionSite::class
        override val id: RdId get() = RdId(-1838348744103133935)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinCompletionSite  {
            val kind = buffer.readString()
            val activationRange = MixinSourceRange.read(ctx, buffer)
            val replacementRange = MixinSourceRange.read(ctx, buffer)
            val receiverType = buffer.readString()
            val items = buffer.readArray {MixinCompletionItem.read(ctx, buffer)}
            return MixinCompletionSite(kind, activationRange, replacementRange, receiverType, items)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinCompletionSite)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinCompletionSite

        if (kind != other.kind) return false
        if (activationRange != other.activationRange) return false
        if (replacementRange != other.replacementRange) return false
        if (receiverType != other.receiverType) return false
        if (!(items contentDeepEquals other.items)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + activationRange.hashCode()
        __r = __r*31 + replacementRange.hashCode()
        __r = __r*31 + receiverType.hashCode()
        __r = __r*31 + items.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinCompletionSite (")
        printer.indent {
            print("kind = "); kind.print(printer); println()
            print("activationRange = "); activationRange.print(printer); println()
            print("replacementRange = "); replacementRange.print(printer); println()
            print("receiverType = "); receiverType.print(printer); println()
            print("items = "); items.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:51]
 */
data class MixinDeclaration (
    val name: String,
    val kind: String,
    val range: MixinSourceRange,
    val scope: MixinSourceRange
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(name)
        buffer.writeString(kind)
        MixinSourceRange.write(ctx, buffer, range)
        MixinSourceRange.write(ctx, buffer, scope)
    }
    //companion

    companion object : IMarshaller<MixinDeclaration> {
        override val _type: KClass<MixinDeclaration> = MixinDeclaration::class
        override val id: RdId get() = RdId(5205524339893740652)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinDeclaration  {
            val name = buffer.readString()
            val kind = buffer.readString()
            val range = MixinSourceRange.read(ctx, buffer)
            val scope = MixinSourceRange.read(ctx, buffer)
            return MixinDeclaration(name, kind, range, scope)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinDeclaration)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinDeclaration

        if (name != other.name) return false
        if (kind != other.kind) return false
        if (range != other.range) return false
        if (scope != other.scope) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + range.hashCode()
        __r = __r*31 + scope.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinDeclaration (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("kind = "); kind.print(printer); println()
            print("range = "); range.print(printer); println()
            print("scope = "); scope.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:67]
 */
data class MixinDiagnostic (
    val message: String,
    val severity: String,
    val range: MixinSourceRange
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(message)
        buffer.writeString(severity)
        MixinSourceRange.write(ctx, buffer, range)
    }
    //companion

    companion object : IMarshaller<MixinDiagnostic> {
        override val _type: KClass<MixinDiagnostic> = MixinDiagnostic::class
        override val id: RdId get() = RdId(-1617245288858810763)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinDiagnostic  {
            val message = buffer.readString()
            val severity = buffer.readString()
            val range = MixinSourceRange.read(ctx, buffer)
            return MixinDiagnostic(message, severity, range)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinDiagnostic)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinDiagnostic

        if (message != other.message) return false
        if (severity != other.severity) return false
        if (range != other.range) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + message.hashCode()
        __r = __r*31 + severity.hashCode()
        __r = __r*31 + range.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinDiagnostic (")
        printer.indent {
            print("message = "); message.print(printer); println()
            print("severity = "); severity.print(printer); println()
            print("range = "); range.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:27]
 */
data class MixinFileInput (
    val filePath: String,
    val sourceText: String,
    val revision: Long
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(filePath)
        buffer.writeString(sourceText)
        buffer.writeLong(revision)
    }
    //companion

    companion object : IMarshaller<MixinFileInput> {
        override val _type: KClass<MixinFileInput> = MixinFileInput::class
        override val id: RdId get() = RdId(4113226335682446112)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinFileInput  {
            val filePath = buffer.readString()
            val sourceText = buffer.readString()
            val revision = buffer.readLong()
            return MixinFileInput(filePath, sourceText, revision)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinFileInput)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinFileInput

        if (filePath != other.filePath) return false
        if (sourceText != other.sourceText) return false
        if (revision != other.revision) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + filePath.hashCode()
        __r = __r*31 + sourceText.hashCode()
        __r = __r*31 + revision.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinFileInput (")
        printer.indent {
            print("filePath = "); filePath.print(printer); println()
            print("sourceText = "); sourceText.print(printer); println()
            print("revision = "); revision.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:98]
 */
data class MixinFileSnapshot (
    val filePath: String,
    val revision: Long,
    val sourceHash: Long,
    val syntaxNodes: Array<MixinSyntaxNode>,
    val tokens: Array<MixinToken>,
    val declarations: Array<MixinDeclaration>,
    val references: Array<MixinReference>,
    val diagnostics: Array<MixinDiagnostic>,
    val completionSites: Array<MixinCompletionSite>,
    val typeFacts: Array<MixinTypeFact>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(filePath)
        buffer.writeLong(revision)
        buffer.writeLong(sourceHash)
        buffer.writeArray(syntaxNodes) { MixinSyntaxNode.write(ctx, buffer, it) }
        buffer.writeArray(tokens) { MixinToken.write(ctx, buffer, it) }
        buffer.writeArray(declarations) { MixinDeclaration.write(ctx, buffer, it) }
        buffer.writeArray(references) { MixinReference.write(ctx, buffer, it) }
        buffer.writeArray(diagnostics) { MixinDiagnostic.write(ctx, buffer, it) }
        buffer.writeArray(completionSites) { MixinCompletionSite.write(ctx, buffer, it) }
        buffer.writeArray(typeFacts) { MixinTypeFact.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<MixinFileSnapshot> {
        override val _type: KClass<MixinFileSnapshot> = MixinFileSnapshot::class
        override val id: RdId get() = RdId(-4595115062107102546)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinFileSnapshot  {
            val filePath = buffer.readString()
            val revision = buffer.readLong()
            val sourceHash = buffer.readLong()
            val syntaxNodes = buffer.readArray {MixinSyntaxNode.read(ctx, buffer)}
            val tokens = buffer.readArray {MixinToken.read(ctx, buffer)}
            val declarations = buffer.readArray {MixinDeclaration.read(ctx, buffer)}
            val references = buffer.readArray {MixinReference.read(ctx, buffer)}
            val diagnostics = buffer.readArray {MixinDiagnostic.read(ctx, buffer)}
            val completionSites = buffer.readArray {MixinCompletionSite.read(ctx, buffer)}
            val typeFacts = buffer.readArray {MixinTypeFact.read(ctx, buffer)}
            return MixinFileSnapshot(filePath, revision, sourceHash, syntaxNodes, tokens, declarations, references, diagnostics, completionSites, typeFacts)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinFileSnapshot)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinFileSnapshot

        if (filePath != other.filePath) return false
        if (revision != other.revision) return false
        if (sourceHash != other.sourceHash) return false
        if (!(syntaxNodes contentDeepEquals other.syntaxNodes)) return false
        if (!(tokens contentDeepEquals other.tokens)) return false
        if (!(declarations contentDeepEquals other.declarations)) return false
        if (!(references contentDeepEquals other.references)) return false
        if (!(diagnostics contentDeepEquals other.diagnostics)) return false
        if (!(completionSites contentDeepEquals other.completionSites)) return false
        if (!(typeFacts contentDeepEquals other.typeFacts)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + filePath.hashCode()
        __r = __r*31 + revision.hashCode()
        __r = __r*31 + sourceHash.hashCode()
        __r = __r*31 + syntaxNodes.contentDeepHashCode()
        __r = __r*31 + tokens.contentDeepHashCode()
        __r = __r*31 + declarations.contentDeepHashCode()
        __r = __r*31 + references.contentDeepHashCode()
        __r = __r*31 + diagnostics.contentDeepHashCode()
        __r = __r*31 + completionSites.contentDeepHashCode()
        __r = __r*31 + typeFacts.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinFileSnapshot (")
        printer.indent {
            print("filePath = "); filePath.print(printer); println()
            print("revision = "); revision.print(printer); println()
            print("sourceHash = "); sourceHash.print(printer); println()
            print("syntaxNodes = "); syntaxNodes.print(printer); println()
            print("tokens = "); tokens.print(printer); println()
            print("declarations = "); declarations.print(printer); println()
            print("references = "); references.print(printer); println()
            print("diagnostics = "); diagnostics.print(printer); println()
            print("completionSites = "); completionSites.print(printer); println()
            print("typeFacts = "); typeFacts.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:127]
 */
data class MixinLanguageCatalog (
    val definitions: Array<MixinLanguageDefinition>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeArray(definitions) { MixinLanguageDefinition.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<MixinLanguageCatalog> {
        override val _type: KClass<MixinLanguageCatalog> = MixinLanguageCatalog::class
        override val id: RdId get() = RdId(-5239733744395790445)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinLanguageCatalog  {
            val definitions = buffer.readArray {MixinLanguageDefinition.read(ctx, buffer)}
            return MixinLanguageCatalog(definitions)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinLanguageCatalog)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinLanguageCatalog

        if (!(definitions contentDeepEquals other.definitions)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + definitions.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinLanguageCatalog (")
        printer.indent {
            print("definitions = "); definitions.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:111]
 */
data class MixinLanguageDefinition (
    val name: String,
    val kind: String,
    val argumentCount: Int,
    val variadic: Boolean,
    val operandType: String,
    val receiverType: String,
    val resultType: String,
    val argumentTypes: Array<String>,
    val documentation: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(name)
        buffer.writeString(kind)
        buffer.writeInt(argumentCount)
        buffer.writeBool(variadic)
        buffer.writeString(operandType)
        buffer.writeString(receiverType)
        buffer.writeString(resultType)
        buffer.writeArray(argumentTypes) { buffer.writeString(it) }
        buffer.writeString(documentation)
    }
    //companion

    companion object : IMarshaller<MixinLanguageDefinition> {
        override val _type: KClass<MixinLanguageDefinition> = MixinLanguageDefinition::class
        override val id: RdId get() = RdId(-559598091605421159)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinLanguageDefinition  {
            val name = buffer.readString()
            val kind = buffer.readString()
            val argumentCount = buffer.readInt()
            val variadic = buffer.readBool()
            val operandType = buffer.readString()
            val receiverType = buffer.readString()
            val resultType = buffer.readString()
            val argumentTypes = buffer.readArray {buffer.readString()}
            val documentation = buffer.readString()
            return MixinLanguageDefinition(name, kind, argumentCount, variadic, operandType, receiverType, resultType, argumentTypes, documentation)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinLanguageDefinition)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinLanguageDefinition

        if (name != other.name) return false
        if (kind != other.kind) return false
        if (argumentCount != other.argumentCount) return false
        if (variadic != other.variadic) return false
        if (operandType != other.operandType) return false
        if (receiverType != other.receiverType) return false
        if (resultType != other.resultType) return false
        if (!(argumentTypes contentDeepEquals other.argumentTypes)) return false
        if (documentation != other.documentation) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + argumentCount.hashCode()
        __r = __r*31 + variadic.hashCode()
        __r = __r*31 + operandType.hashCode()
        __r = __r*31 + receiverType.hashCode()
        __r = __r*31 + resultType.hashCode()
        __r = __r*31 + argumentTypes.contentDeepHashCode()
        __r = __r*31 + documentation.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinLanguageDefinition (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("kind = "); kind.print(printer); println()
            print("argumentCount = "); argumentCount.print(printer); println()
            print("variadic = "); variadic.print(printer); println()
            print("operandType = "); operandType.print(printer); println()
            print("receiverType = "); receiverType.print(printer); println()
            print("resultType = "); resultType.print(printer); println()
            print("argumentTypes = "); argumentTypes.print(printer); println()
            print("documentation = "); documentation.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:33]
 */
data class MixinParseRequest (
    val files: Array<MixinFileInput>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeArray(files) { MixinFileInput.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<MixinParseRequest> {
        override val _type: KClass<MixinParseRequest> = MixinParseRequest::class
        override val id: RdId get() = RdId(-4347416270738964566)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinParseRequest  {
            val files = buffer.readArray {MixinFileInput.read(ctx, buffer)}
            return MixinParseRequest(files)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinParseRequest)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinParseRequest

        if (!(files contentDeepEquals other.files)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + files.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinParseRequest (")
        printer.indent {
            print("files = "); files.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:123]
 */
data class MixinParseResponse (
    val files: Array<MixinFileSnapshot>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeArray(files) { MixinFileSnapshot.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<MixinParseResponse> {
        override val _type: KClass<MixinParseResponse> = MixinParseResponse::class
        override val id: RdId get() = RdId(-5642695876888106362)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinParseResponse  {
            val files = buffer.readArray {MixinFileSnapshot.read(ctx, buffer)}
            return MixinParseResponse(files)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinParseResponse)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinParseResponse

        if (!(files contentDeepEquals other.files)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + files.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinParseResponse (")
        printer.indent {
            print("files = "); files.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:58]
 */
data class MixinReference (
    val name: String,
    val kind: String,
    val range: MixinSourceRange,
    val scope: MixinSourceRange,
    val targetFilePath: String,
    val targetRange: MixinSourceRange
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(name)
        buffer.writeString(kind)
        MixinSourceRange.write(ctx, buffer, range)
        MixinSourceRange.write(ctx, buffer, scope)
        buffer.writeString(targetFilePath)
        MixinSourceRange.write(ctx, buffer, targetRange)
    }
    //companion

    companion object : IMarshaller<MixinReference> {
        override val _type: KClass<MixinReference> = MixinReference::class
        override val id: RdId get() = RdId(4113236455037010621)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinReference  {
            val name = buffer.readString()
            val kind = buffer.readString()
            val range = MixinSourceRange.read(ctx, buffer)
            val scope = MixinSourceRange.read(ctx, buffer)
            val targetFilePath = buffer.readString()
            val targetRange = MixinSourceRange.read(ctx, buffer)
            return MixinReference(name, kind, range, scope, targetFilePath, targetRange)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinReference)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinReference

        if (name != other.name) return false
        if (kind != other.kind) return false
        if (range != other.range) return false
        if (scope != other.scope) return false
        if (targetFilePath != other.targetFilePath) return false
        if (targetRange != other.targetRange) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + range.hashCode()
        __r = __r*31 + scope.hashCode()
        __r = __r*31 + targetFilePath.hashCode()
        __r = __r*31 + targetRange.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinReference (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("kind = "); kind.print(printer); println()
            print("range = "); range.print(printer); println()
            print("scope = "); scope.print(printer); println()
            print("targetFilePath = "); targetFilePath.print(printer); println()
            print("targetRange = "); targetRange.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:22]
 */
data class MixinSourceRange (
    val startOffset: Int,
    val endOffset: Int
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeInt(startOffset)
        buffer.writeInt(endOffset)
    }
    //companion

    companion object : IMarshaller<MixinSourceRange> {
        override val _type: KClass<MixinSourceRange> = MixinSourceRange::class
        override val id: RdId get() = RdId(5218098678922832084)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinSourceRange  {
            val startOffset = buffer.readInt()
            val endOffset = buffer.readInt()
            return MixinSourceRange(startOffset, endOffset)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinSourceRange)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinSourceRange

        if (startOffset != other.startOffset) return false
        if (endOffset != other.endOffset) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + startOffset.hashCode()
        __r = __r*31 + endOffset.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinSourceRange (")
        printer.indent {
            print("startOffset = "); startOffset.print(printer); println()
            print("endOffset = "); endOffset.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:39]
 */
data class MixinSyntaxNode (
    val parentIndex: Int,
    val kind: String,
    val range: MixinSourceRange,
    val name: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeInt(parentIndex)
        buffer.writeString(kind)
        MixinSourceRange.write(ctx, buffer, range)
        buffer.writeString(name)
    }
    //companion

    companion object : IMarshaller<MixinSyntaxNode> {
        override val _type: KClass<MixinSyntaxNode> = MixinSyntaxNode::class
        override val id: RdId get() = RdId(-1616834679433244845)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinSyntaxNode  {
            val parentIndex = buffer.readInt()
            val kind = buffer.readString()
            val range = MixinSourceRange.read(ctx, buffer)
            val name = buffer.readString()
            return MixinSyntaxNode(parentIndex, kind, range, name)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinSyntaxNode)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinSyntaxNode

        if (parentIndex != other.parentIndex) return false
        if (kind != other.kind) return false
        if (range != other.range) return false
        if (name != other.name) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + parentIndex.hashCode()
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + range.hashCode()
        __r = __r*31 + name.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinSyntaxNode (")
        printer.indent {
            print("parentIndex = "); parentIndex.print(printer); println()
            print("kind = "); kind.print(printer); println()
            print("range = "); range.print(printer); println()
            print("name = "); name.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:46]
 */
data class MixinToken (
    val kind: String,
    val range: MixinSourceRange
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(kind)
        MixinSourceRange.write(ctx, buffer, range)
    }
    //companion

    companion object : IMarshaller<MixinToken> {
        override val _type: KClass<MixinToken> = MixinToken::class
        override val id: RdId get() = RdId(17701739849713259)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinToken  {
            val kind = buffer.readString()
            val range = MixinSourceRange.read(ctx, buffer)
            return MixinToken(kind, range)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinToken)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinToken

        if (kind != other.kind) return false
        if (range != other.range) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + range.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinToken (")
        printer.indent {
            print("kind = "); kind.print(printer); println()
            print("range = "); range.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:90]
 */
data class MixinTypeFact (
    val range: MixinSourceRange,
    val type: String,
    val documentation: String,
    val inlay: Boolean,
    val kind: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        MixinSourceRange.write(ctx, buffer, range)
        buffer.writeString(type)
        buffer.writeString(documentation)
        buffer.writeBool(inlay)
        buffer.writeString(kind)
    }
    //companion

    companion object : IMarshaller<MixinTypeFact> {
        override val _type: KClass<MixinTypeFact> = MixinTypeFact::class
        override val id: RdId get() = RdId(-7603046265752210668)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinTypeFact  {
            val range = MixinSourceRange.read(ctx, buffer)
            val type = buffer.readString()
            val documentation = buffer.readString()
            val inlay = buffer.readBool()
            val kind = buffer.readString()
            return MixinTypeFact(range, type, documentation, inlay, kind)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinTypeFact)  {
            value.write(ctx, buffer)
        }


    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false

        other as MixinTypeFact

        if (range != other.range) return false
        if (type != other.type) return false
        if (documentation != other.documentation) return false
        if (inlay != other.inlay) return false
        if (kind != other.kind) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + range.hashCode()
        __r = __r*31 + type.hashCode()
        __r = __r*31 + documentation.hashCode()
        __r = __r*31 + inlay.hashCode()
        __r = __r*31 + kind.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinTypeFact (")
        printer.indent {
            print("range = "); range.print(printer); println()
            print("type = "); type.print(printer); println()
            print("documentation = "); documentation.print(printer); println()
            print("inlay = "); inlay.print(printer); println()
            print("kind = "); kind.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}
