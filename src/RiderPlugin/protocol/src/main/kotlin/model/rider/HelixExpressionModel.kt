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
            field("sourceText", string)
            field("revision", long)
        }

        val response = structdef("mixinExpressionResponse") {
            field("projectFileFound", bool)
            field("csharpFileFound", bool)
            field("attributeCount", int)
            field("matchedAttributeCount", int)
            field("ranges", array(sourceRange))
        }

        val contribution = structdef("mixinContribution") {
            field("offset", int)
            field("target", string)
            field("method", string)
            field("mixin", string)
            field("priority", int)
            field("sourceType", string)
            field("sourceMember", string)
            field("sourceKind", string)
            field("sourceParameterCount", int)
        }

        val contributionsResponse = structdef("mixinContributionsResponse") {
            field("contributions", array(contribution))
        }

        call("getMixinExpressionRanges", request, response).async
        call("getMixinContributions", request, contributionsResponse).async
        property("isHelixEnabled", bool)
    }
}
