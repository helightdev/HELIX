package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.*
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rd.generator.nova.kotlin.Kotlin11Generator
import com.jetbrains.rider.model.nova.ide.SolutionModel

@Suppress("unused")
object HelixExpressionModel : Ext(SolutionModel.Solution) {
    init {
        setting(CSharp50Generator.Namespace, "HelixRider.Protocol")
        setting(Kotlin11Generator.Namespace, "dev.helight.helix.protocol")

        val sourceRange = structdef("mixinExpressionRange") {
            field("startOffset", int)
            field("endOffset", int)
        }

        val request = structdef("mixinExpressionRequest") {
            field("filePath", string)
        }

        val response = structdef("mixinExpressionResponse") {
            field("projectFileFound", bool)
            field("csharpFileFound", bool)
            field("attributeCount", int)
            field("matchedAttributeCount", int)
            field("ranges", array(sourceRange))
        }

        call("getMixinExpressionRanges", request, response).async
    }
}
