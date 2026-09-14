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
 * #### Generated from [HelixExpressionModel.kt:11]
 */
class HelixExpressionModel private constructor(
    private val _completeCSharpTypes: RdCall<String, MixinCompletionResponse>,
    private val _resolveCSharpTypes: RdCall<Array<String>, MixinCompletionResponse>,
    private val _getCSharpHostSchema: RdCall<String, String>,
    private val _isHelixEnabled: RdOptionalProperty<Boolean>
) : RdExtBase() {
    //companion

    companion object : ISerializersOwner {

        override fun registerSerializersCore(serializers: ISerializers)  {
            val classLoader = javaClass.classLoader
            serializers.register(LazyCompanionMarshaller(RdId(5218098678922832084), classLoader, "dev.helight.helix.protocol.MixinSourceRange"))
            serializers.register(LazyCompanionMarshaller(RdId(4113226335682446112), classLoader, "dev.helight.helix.protocol.MixinFileInput"))
            serializers.register(LazyCompanionMarshaller(RdId(-4347416270738964566), classLoader, "dev.helight.helix.protocol.MixinParseRequest"))
            serializers.register(LazyCompanionMarshaller(RdId(5205524339893740652), classLoader, "dev.helight.helix.protocol.MixinDeclaration"))
            serializers.register(LazyCompanionMarshaller(RdId(4113236455037010621), classLoader, "dev.helight.helix.protocol.MixinReference"))
            serializers.register(LazyCompanionMarshaller(RdId(-1617245288858810763), classLoader, "dev.helight.helix.protocol.MixinDiagnostic"))
            serializers.register(LazyCompanionMarshaller(RdId(-1838348744103421731), classLoader, "dev.helight.helix.protocol.MixinCompletionItem"))
            serializers.register(LazyCompanionMarshaller(RdId(2135719266191476869), classLoader, "dev.helight.helix.protocol.MixinCompletionRequest"))
            serializers.register(LazyCompanionMarshaller(RdId(-7579679042849489653), classLoader, "dev.helight.helix.protocol.MixinCompletionResponse"))
            serializers.register(LazyCompanionMarshaller(RdId(-559598091605421159), classLoader, "dev.helight.helix.protocol.MixinLanguageDefinition"))
            serializers.register(LazyCompanionMarshaller(RdId(-7603046265752210668), classLoader, "dev.helight.helix.protocol.MixinTypeFact"))
            serializers.register(LazyCompanionMarshaller(RdId(-4595115062107102546), classLoader, "dev.helight.helix.protocol.MixinFileSnapshot"))
            serializers.register(LazyCompanionMarshaller(RdId(7368049600385922343), classLoader, "dev.helight.helix.protocol.MixinDefinitionQuery"))
            serializers.register(LazyCompanionMarshaller(RdId(3757912040769169282), classLoader, "dev.helight.helix.protocol.MixinDefinitionResponse"))
            serializers.register(LazyCompanionMarshaller(RdId(-5642695876888106362), classLoader, "dev.helight.helix.protocol.MixinParseResponse"))
            serializers.register(LazyCompanionMarshaller(RdId(-5239733744395790445), classLoader, "dev.helight.helix.protocol.MixinLanguageCatalog"))
            serializers.register(LazyCompanionMarshaller(RdId(7639943946241625851), classLoader, "dev.helight.helix.protocol.HixExecutionRequest"))
            serializers.register(LazyCompanionMarshaller(RdId(6904821828525885683), classLoader, "dev.helight.helix.protocol.HixTestResult"))
            serializers.register(LazyCompanionMarshaller(RdId(-2969410624680835755), classLoader, "dev.helight.helix.protocol.HixExecutionResponse"))
        }




        private val __StringArraySerializer = FrameworkMarshallers.String.array()

        const val serializationHash = -3308294366828357265L

    }
    override val serializersOwner: ISerializersOwner get() = HelixExpressionModel
    override val serializationHash: Long get() = HelixExpressionModel.serializationHash

    //fields
    val completeCSharpTypes: IRdCall<String, MixinCompletionResponse> get() = _completeCSharpTypes
    val resolveCSharpTypes: IRdCall<Array<String>, MixinCompletionResponse> get() = _resolveCSharpTypes
    val getCSharpHostSchema: IRdCall<String, String> get() = _getCSharpHostSchema
    val isHelixEnabled: IOptProperty<Boolean> get() = _isHelixEnabled
    //methods
    //initializer
    init {
        _isHelixEnabled.optimizeNested = true
    }

    init {
        _completeCSharpTypes.async = true
        _resolveCSharpTypes.async = true
        _getCSharpHostSchema.async = true
    }

    init {
        bindableChildren.add("completeCSharpTypes" to _completeCSharpTypes)
        bindableChildren.add("resolveCSharpTypes" to _resolveCSharpTypes)
        bindableChildren.add("getCSharpHostSchema" to _getCSharpHostSchema)
        bindableChildren.add("isHelixEnabled" to _isHelixEnabled)
    }

    //secondary constructor
    internal constructor(
    ) : this(
        RdCall<String, MixinCompletionResponse>(FrameworkMarshallers.String, MixinCompletionResponse),
        RdCall<Array<String>, MixinCompletionResponse>(__StringArraySerializer, MixinCompletionResponse),
        RdCall<String, String>(FrameworkMarshallers.String, FrameworkMarshallers.String),
        RdOptionalProperty<Boolean>(FrameworkMarshallers.Bool)
    )

    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("HelixExpressionModel (")
        printer.indent {
            print("completeCSharpTypes = "); _completeCSharpTypes.print(printer); println()
            print("resolveCSharpTypes = "); _resolveCSharpTypes.print(printer); println()
            print("getCSharpHostSchema = "); _getCSharpHostSchema.print(printer); println()
            print("isHelixEnabled = "); _isHelixEnabled.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): HelixExpressionModel   {
        return HelixExpressionModel(
            _completeCSharpTypes.deepClonePolymorphic(),
            _resolveCSharpTypes.deepClonePolymorphic(),
            _getCSharpHostSchema.deepClonePolymorphic(),
            _isHelixEnabled.deepClonePolymorphic()
        )
    }
    //contexts
    //threading
    override val extThreading: ExtThreadingKind get() = ExtThreadingKind.Default
}
val com.jetbrains.rd.ide.model.Solution.helixExpressionModel get() = getOrCreateExtension("helixExpressionModel", ::HelixExpressionModel)



/**
 * #### Generated from [HelixExpressionModel.kt:134]
 */
data class HixExecutionRequest (
    val manifestPath: String,
    val test: Boolean,
    val entry: String,
    val arguments: Array<String>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(manifestPath)
        buffer.writeBool(test)
        buffer.writeString(entry)
        buffer.writeArray(arguments) { buffer.writeString(it) }
    }
    //companion

    companion object : IMarshaller<HixExecutionRequest> {
        override val _type: KClass<HixExecutionRequest> = HixExecutionRequest::class
        override val id: RdId get() = RdId(7639943946241625851)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): HixExecutionRequest  {
            val manifestPath = buffer.readString()
            val test = buffer.readBool()
            val entry = buffer.readString()
            val arguments = buffer.readArray {buffer.readString()}
            return HixExecutionRequest(manifestPath, test, entry, arguments)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: HixExecutionRequest)  {
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

        other as HixExecutionRequest

        if (manifestPath != other.manifestPath) return false
        if (test != other.test) return false
        if (entry != other.entry) return false
        if (!(arguments contentDeepEquals other.arguments)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + manifestPath.hashCode()
        __r = __r*31 + test.hashCode()
        __r = __r*31 + entry.hashCode()
        __r = __r*31 + arguments.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("HixExecutionRequest (")
        printer.indent {
            print("manifestPath = "); manifestPath.print(printer); println()
            print("test = "); test.print(printer); println()
            print("entry = "); entry.print(printer); println()
            print("arguments = "); arguments.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:146]
 */
data class HixExecutionResponse (
    val exitCode: Int,
    val standardOutput: String,
    val standardError: String,
    val tests: Array<HixTestResult>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeInt(exitCode)
        buffer.writeString(standardOutput)
        buffer.writeString(standardError)
        buffer.writeArray(tests) { HixTestResult.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<HixExecutionResponse> {
        override val _type: KClass<HixExecutionResponse> = HixExecutionResponse::class
        override val id: RdId get() = RdId(-2969410624680835755)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): HixExecutionResponse  {
            val exitCode = buffer.readInt()
            val standardOutput = buffer.readString()
            val standardError = buffer.readString()
            val tests = buffer.readArray {HixTestResult.read(ctx, buffer)}
            return HixExecutionResponse(exitCode, standardOutput, standardError, tests)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: HixExecutionResponse)  {
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

        other as HixExecutionResponse

        if (exitCode != other.exitCode) return false
        if (standardOutput != other.standardOutput) return false
        if (standardError != other.standardError) return false
        if (!(tests contentDeepEquals other.tests)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + exitCode.hashCode()
        __r = __r*31 + standardOutput.hashCode()
        __r = __r*31 + standardError.hashCode()
        __r = __r*31 + tests.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("HixExecutionResponse (")
        printer.indent {
            print("exitCode = "); exitCode.print(printer); println()
            print("standardOutput = "); standardOutput.print(printer); println()
            print("standardError = "); standardError.print(printer); println()
            print("tests = "); tests.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:140]
 */
data class HixTestResult (
    val `file`: String,
    val name: String,
    val success: Boolean,
    val message: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(`file`)
        buffer.writeString(name)
        buffer.writeBool(success)
        buffer.writeString(message)
    }
    //companion

    companion object : IMarshaller<HixTestResult> {
        override val _type: KClass<HixTestResult> = HixTestResult::class
        override val id: RdId get() = RdId(6904821828525885683)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): HixTestResult  {
            val `file` = buffer.readString()
            val name = buffer.readString()
            val success = buffer.readBool()
            val message = buffer.readString()
            return HixTestResult(`file`, name, success, message)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: HixTestResult)  {
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

        other as HixTestResult

        if (`file` != other.`file`) return false
        if (name != other.name) return false
        if (success != other.success) return false
        if (message != other.message) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + `file`.hashCode()
        __r = __r*31 + name.hashCode()
        __r = __r*31 + success.hashCode()
        __r = __r*31 + message.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("HixTestResult (")
        printer.indent {
            print("file = "); `file`.print(printer); println()
            print("name = "); name.print(printer); println()
            print("success = "); success.print(printer); println()
            print("message = "); message.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:55]
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
 * #### Generated from [HelixExpressionModel.kt:64]
 */
data class MixinCompletionRequest (
    val kind: String,
    val prefix: String,
    val backend: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(kind)
        buffer.writeString(prefix)
        buffer.writeString(backend)
    }
    //companion

    companion object : IMarshaller<MixinCompletionRequest> {
        override val _type: KClass<MixinCompletionRequest> = MixinCompletionRequest::class
        override val id: RdId get() = RdId(2135719266191476869)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinCompletionRequest  {
            val kind = buffer.readString()
            val prefix = buffer.readString()
            val backend = buffer.readString()
            return MixinCompletionRequest(kind, prefix, backend)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinCompletionRequest)  {
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

        other as MixinCompletionRequest

        if (kind != other.kind) return false
        if (prefix != other.prefix) return false
        if (backend != other.backend) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + prefix.hashCode()
        __r = __r*31 + backend.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinCompletionRequest (")
        printer.indent {
            print("kind = "); kind.print(printer); println()
            print("prefix = "); prefix.print(printer); println()
            print("backend = "); backend.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:70]
 */
data class MixinCompletionResponse (
    val items: Array<MixinCompletionItem>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeArray(items) { MixinCompletionItem.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<MixinCompletionResponse> {
        override val _type: KClass<MixinCompletionResponse> = MixinCompletionResponse::class
        override val id: RdId get() = RdId(-7579679042849489653)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinCompletionResponse  {
            val items = buffer.readArray {MixinCompletionItem.read(ctx, buffer)}
            return MixinCompletionResponse(items)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinCompletionResponse)  {
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

        other as MixinCompletionResponse

        if (!(items contentDeepEquals other.items)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + items.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinCompletionResponse (")
        printer.indent {
            print("items = "); items.print(printer); println()
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
 * #### Generated from [HelixExpressionModel.kt:114]
 */
data class MixinDefinitionQuery (
    val kind: String,
    val receiverType: String,
    val operandType: String,
    val prefix: String,
    val backend: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(kind)
        buffer.writeString(receiverType)
        buffer.writeString(operandType)
        buffer.writeString(prefix)
        buffer.writeString(backend)
    }
    //companion

    companion object : IMarshaller<MixinDefinitionQuery> {
        override val _type: KClass<MixinDefinitionQuery> = MixinDefinitionQuery::class
        override val id: RdId get() = RdId(7368049600385922343)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinDefinitionQuery  {
            val kind = buffer.readString()
            val receiverType = buffer.readString()
            val operandType = buffer.readString()
            val prefix = buffer.readString()
            val backend = buffer.readString()
            return MixinDefinitionQuery(kind, receiverType, operandType, prefix, backend)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinDefinitionQuery)  {
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

        other as MixinDefinitionQuery

        if (kind != other.kind) return false
        if (receiverType != other.receiverType) return false
        if (operandType != other.operandType) return false
        if (prefix != other.prefix) return false
        if (backend != other.backend) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + kind.hashCode()
        __r = __r*31 + receiverType.hashCode()
        __r = __r*31 + operandType.hashCode()
        __r = __r*31 + prefix.hashCode()
        __r = __r*31 + backend.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinDefinitionQuery (")
        printer.indent {
            print("kind = "); kind.print(printer); println()
            print("receiverType = "); receiverType.print(printer); println()
            print("operandType = "); operandType.print(printer); println()
            print("prefix = "); prefix.print(printer); println()
            print("backend = "); backend.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:122]
 */
data class MixinDefinitionResponse (
    val definitions: Array<MixinLanguageDefinition>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeArray(definitions) { MixinLanguageDefinition.write(ctx, buffer, it) }
    }
    //companion

    companion object : IMarshaller<MixinDefinitionResponse> {
        override val _type: KClass<MixinDefinitionResponse> = MixinDefinitionResponse::class
        override val id: RdId get() = RdId(3757912040769169282)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinDefinitionResponse  {
            val definitions = buffer.readArray {MixinLanguageDefinition.read(ctx, buffer)}
            return MixinDefinitionResponse(definitions)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinDefinitionResponse)  {
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

        other as MixinDefinitionResponse

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
        printer.println("MixinDefinitionResponse (")
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
 * #### Generated from [HelixExpressionModel.kt:49]
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
 * #### Generated from [HelixExpressionModel.kt:22]
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
 * #### Generated from [HelixExpressionModel.kt:104]
 */
data class MixinFileSnapshot (
    val filePath: String,
    val revision: Long,
    val sourceHash: Long,
    val declarations: Array<MixinDeclaration>,
    val references: Array<MixinReference>,
    val diagnostics: Array<MixinDiagnostic>,
    val typeFacts: Array<MixinTypeFact>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(filePath)
        buffer.writeLong(revision)
        buffer.writeLong(sourceHash)
        buffer.writeArray(declarations) { MixinDeclaration.write(ctx, buffer, it) }
        buffer.writeArray(references) { MixinReference.write(ctx, buffer, it) }
        buffer.writeArray(diagnostics) { MixinDiagnostic.write(ctx, buffer, it) }
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
            val declarations = buffer.readArray {MixinDeclaration.read(ctx, buffer)}
            val references = buffer.readArray {MixinReference.read(ctx, buffer)}
            val diagnostics = buffer.readArray {MixinDiagnostic.read(ctx, buffer)}
            val typeFacts = buffer.readArray {MixinTypeFact.read(ctx, buffer)}
            return MixinFileSnapshot(filePath, revision, sourceHash, declarations, references, diagnostics, typeFacts)
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
        if (!(declarations contentDeepEquals other.declarations)) return false
        if (!(references contentDeepEquals other.references)) return false
        if (!(diagnostics contentDeepEquals other.diagnostics)) return false
        if (!(typeFacts contentDeepEquals other.typeFacts)) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + filePath.hashCode()
        __r = __r*31 + revision.hashCode()
        __r = __r*31 + sourceHash.hashCode()
        __r = __r*31 + declarations.contentDeepHashCode()
        __r = __r*31 + references.contentDeepHashCode()
        __r = __r*31 + diagnostics.contentDeepHashCode()
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
            print("declarations = "); declarations.print(printer); println()
            print("references = "); references.print(printer); println()
            print("diagnostics = "); diagnostics.print(printer); println()
            print("typeFacts = "); typeFacts.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:130]
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
 * #### Generated from [HelixExpressionModel.kt:74]
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
    val documentation: String,
    val argumentNames: Array<String>,
    val optionalArguments: BooleanArray,
    val argumentDefaults: Array<String>,
    val typeParameters: Array<String>,
    val typeParameterBounds: Array<String>,
    val declaredArgumentTypes: Array<String>,
    val obviousArguments: BooleanArray
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
        buffer.writeArray(argumentNames) { buffer.writeString(it) }
        buffer.writeBooleanArray(optionalArguments)
        buffer.writeArray(argumentDefaults) { buffer.writeString(it) }
        buffer.writeArray(typeParameters) { buffer.writeString(it) }
        buffer.writeArray(typeParameterBounds) { buffer.writeString(it) }
        buffer.writeArray(declaredArgumentTypes) { buffer.writeString(it) }
        buffer.writeBooleanArray(obviousArguments)
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
            val argumentNames = buffer.readArray {buffer.readString()}
            val optionalArguments = buffer.readBooleanArray()
            val argumentDefaults = buffer.readArray {buffer.readString()}
            val typeParameters = buffer.readArray {buffer.readString()}
            val typeParameterBounds = buffer.readArray {buffer.readString()}
            val declaredArgumentTypes = buffer.readArray {buffer.readString()}
            val obviousArguments = buffer.readBooleanArray()
            return MixinLanguageDefinition(name, kind, argumentCount, variadic, operandType, receiverType, resultType, argumentTypes, documentation, argumentNames, optionalArguments, argumentDefaults, typeParameters, typeParameterBounds, declaredArgumentTypes, obviousArguments)
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
        if (!(argumentNames contentDeepEquals other.argumentNames)) return false
        if (!(optionalArguments contentEquals other.optionalArguments)) return false
        if (!(argumentDefaults contentDeepEquals other.argumentDefaults)) return false
        if (!(typeParameters contentDeepEquals other.typeParameters)) return false
        if (!(typeParameterBounds contentDeepEquals other.typeParameterBounds)) return false
        if (!(declaredArgumentTypes contentDeepEquals other.declaredArgumentTypes)) return false
        if (!(obviousArguments contentEquals other.obviousArguments)) return false

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
        __r = __r*31 + argumentNames.contentDeepHashCode()
        __r = __r*31 + optionalArguments.contentHashCode()
        __r = __r*31 + argumentDefaults.contentDeepHashCode()
        __r = __r*31 + typeParameters.contentDeepHashCode()
        __r = __r*31 + typeParameterBounds.contentDeepHashCode()
        __r = __r*31 + declaredArgumentTypes.contentDeepHashCode()
        __r = __r*31 + obviousArguments.contentHashCode()
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
            print("argumentNames = "); argumentNames.print(printer); println()
            print("optionalArguments = "); optionalArguments.print(printer); println()
            print("argumentDefaults = "); argumentDefaults.print(printer); println()
            print("typeParameters = "); typeParameters.print(printer); println()
            print("typeParameterBounds = "); typeParameterBounds.print(printer); println()
            print("declaredArgumentTypes = "); declaredArgumentTypes.print(printer); println()
            print("obviousArguments = "); obviousArguments.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:28]
 */
data class MixinParseRequest (
    val files: Array<MixinFileInput>,
    val backend: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeArray(files) { MixinFileInput.write(ctx, buffer, it) }
        buffer.writeString(backend)
    }
    //companion

    companion object : IMarshaller<MixinParseRequest> {
        override val _type: KClass<MixinParseRequest> = MixinParseRequest::class
        override val id: RdId get() = RdId(-4347416270738964566)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinParseRequest  {
            val files = buffer.readArray {MixinFileInput.read(ctx, buffer)}
            val backend = buffer.readString()
            return MixinParseRequest(files, backend)
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
        if (backend != other.backend) return false

        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + files.contentDeepHashCode()
        __r = __r*31 + backend.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinParseRequest (")
        printer.indent {
            print("files = "); files.print(printer); println()
            print("backend = "); backend.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:126]
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
 * #### Generated from [HelixExpressionModel.kt:40]
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
 * #### Generated from [HelixExpressionModel.kt:17]
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
 * #### Generated from [HelixExpressionModel.kt:93]
 */
data class MixinTypeFact (
    val range: MixinSourceRange,
    val type: String,
    val documentation: String,
    val inlay: Boolean,
    val kind: String,
    val callSignature: MixinLanguageDefinition?,
    val memberNames: Array<String>,
    val memberTypes: Array<String>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        MixinSourceRange.write(ctx, buffer, range)
        buffer.writeString(type)
        buffer.writeString(documentation)
        buffer.writeBool(inlay)
        buffer.writeString(kind)
        buffer.writeNullable(callSignature) { MixinLanguageDefinition.write(ctx, buffer, it) }
        buffer.writeArray(memberNames) { buffer.writeString(it) }
        buffer.writeArray(memberTypes) { buffer.writeString(it) }
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
            val callSignature = buffer.readNullable { MixinLanguageDefinition.read(ctx, buffer) }
            val memberNames = buffer.readArray {buffer.readString()}
            val memberTypes = buffer.readArray {buffer.readString()}
            return MixinTypeFact(range, type, documentation, inlay, kind, callSignature, memberNames, memberTypes)
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
        if (callSignature != other.callSignature) return false
        if (!(memberNames contentDeepEquals other.memberNames)) return false
        if (!(memberTypes contentDeepEquals other.memberTypes)) return false

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
        __r = __r*31 + if (callSignature != null) callSignature.hashCode() else 0
        __r = __r*31 + memberNames.contentDeepHashCode()
        __r = __r*31 + memberTypes.contentDeepHashCode()
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
            print("callSignature = "); callSignature.print(printer); println()
            print("memberNames = "); memberNames.print(printer); println()
            print("memberTypes = "); memberTypes.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}
