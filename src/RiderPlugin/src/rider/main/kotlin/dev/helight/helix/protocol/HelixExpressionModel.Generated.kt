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
 * #### Generated from [HelixExpressionModel.kt:9]
 */
class HelixExpressionModel private constructor(
    private val _getMixinExpressionRanges: RdCall<MixinExpressionRequest, MixinExpressionResponse>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers)  {
            val classLoader = javaClass.classLoader
            serializers.register(LazyCompanionMarshaller(RdId(411702552799312759), classLoader, "dev.helight.helix.protocol.MixinExpressionRange"))
            serializers.register(LazyCompanionMarshaller(RdId(8264527692356685385), classLoader, "dev.helight.helix.protocol.MixinExpressionRequest"))
            serializers.register(LazyCompanionMarshaller(RdId(-2054058568823541817), classLoader, "dev.helight.helix.protocol.MixinExpressionResponse"))
        }
        
        
        
        
        
        const val serializationHash = 6376851667827411014L
        
    }
    override val serializersOwner: ISerializersOwner get() = HelixExpressionModel
    override val serializationHash: Long get() = HelixExpressionModel.serializationHash
    
    //fields
    val getMixinExpressionRanges: IRdCall<MixinExpressionRequest, MixinExpressionResponse> get() = _getMixinExpressionRanges
    //methods
    //initializer
    init {
        _getMixinExpressionRanges.async = true
    }
    
    init {
        bindableChildren.add("getMixinExpressionRanges" to _getMixinExpressionRanges)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdCall<MixinExpressionRequest, MixinExpressionResponse>(MixinExpressionRequest, MixinExpressionResponse)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("HelixExpressionModel (")
        printer.indent {
            print("getMixinExpressionRanges = "); _getMixinExpressionRanges.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): HelixExpressionModel   {
        return HelixExpressionModel(
            _getMixinExpressionRanges.deepClonePolymorphic()
        )
    }
    //contexts
    //threading
    override val extThreading: ExtThreadingKind get() = ExtThreadingKind.Default
}
val com.jetbrains.rd.ide.model.Solution.helixExpressionModel get() = getOrCreateExtension("helixExpressionModel", ::HelixExpressionModel)



/**
 * #### Generated from [HelixExpressionModel.kt:15]
 */
data class MixinExpressionRange (
    val startOffset: Int,
    val endOffset: Int
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeInt(startOffset)
        buffer.writeInt(endOffset)
    }
    //companion
    
    companion object : IMarshaller<MixinExpressionRange> {
        override val _type: KClass<MixinExpressionRange> = MixinExpressionRange::class
        override val id: RdId get() = RdId(411702552799312759)
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinExpressionRange  {
            val startOffset = buffer.readInt()
            val endOffset = buffer.readInt()
            return MixinExpressionRange(startOffset, endOffset)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinExpressionRange)  {
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
        
        other as MixinExpressionRange
        
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
        printer.println("MixinExpressionRange (")
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
 * #### Generated from [HelixExpressionModel.kt:20]
 */
data class MixinExpressionRequest (
    val filePath: String
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeString(filePath)
    }
    //companion
    
    companion object : IMarshaller<MixinExpressionRequest> {
        override val _type: KClass<MixinExpressionRequest> = MixinExpressionRequest::class
        override val id: RdId get() = RdId(8264527692356685385)
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinExpressionRequest  {
            val filePath = buffer.readString()
            return MixinExpressionRequest(filePath)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinExpressionRequest)  {
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
        
        other as MixinExpressionRequest
        
        if (filePath != other.filePath) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + filePath.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinExpressionRequest (")
        printer.indent {
            print("filePath = "); filePath.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [HelixExpressionModel.kt:24]
 */
data class MixinExpressionResponse (
    val projectFileFound: Boolean,
    val csharpFileFound: Boolean,
    val attributeCount: Int,
    val matchedAttributeCount: Int,
    val ranges: Array<MixinExpressionRange>
) : IPrintable {
    //write-marshaller
    private fun write(ctx: SerializationCtx, buffer: AbstractBuffer)  {
        buffer.writeBool(projectFileFound)
        buffer.writeBool(csharpFileFound)
        buffer.writeInt(attributeCount)
        buffer.writeInt(matchedAttributeCount)
        buffer.writeArray(ranges) { MixinExpressionRange.write(ctx, buffer, it) }
    }
    //companion
    
    companion object : IMarshaller<MixinExpressionResponse> {
        override val _type: KClass<MixinExpressionResponse> = MixinExpressionResponse::class
        override val id: RdId get() = RdId(-2054058568823541817)
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MixinExpressionResponse  {
            val projectFileFound = buffer.readBool()
            val csharpFileFound = buffer.readBool()
            val attributeCount = buffer.readInt()
            val matchedAttributeCount = buffer.readInt()
            val ranges = buffer.readArray {MixinExpressionRange.read(ctx, buffer)}
            return MixinExpressionResponse(projectFileFound, csharpFileFound, attributeCount, matchedAttributeCount, ranges)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MixinExpressionResponse)  {
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
        
        other as MixinExpressionResponse
        
        if (projectFileFound != other.projectFileFound) return false
        if (csharpFileFound != other.csharpFileFound) return false
        if (attributeCount != other.attributeCount) return false
        if (matchedAttributeCount != other.matchedAttributeCount) return false
        if (!(ranges contentDeepEquals other.ranges)) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + projectFileFound.hashCode()
        __r = __r*31 + csharpFileFound.hashCode()
        __r = __r*31 + attributeCount.hashCode()
        __r = __r*31 + matchedAttributeCount.hashCode()
        __r = __r*31 + ranges.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MixinExpressionResponse (")
        printer.indent {
            print("projectFileFound = "); projectFileFound.print(printer); println()
            print("csharpFileFound = "); csharpFileFound.print(printer); println()
            print("attributeCount = "); attributeCount.print(printer); println()
            print("matchedAttributeCount = "); matchedAttributeCount.print(printer); println()
            print("ranges = "); ranges.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}
