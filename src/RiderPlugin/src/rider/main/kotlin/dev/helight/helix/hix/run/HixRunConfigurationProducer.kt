package dev.helight.helix.hix.run

import com.intellij.execution.actions.ConfigurationContext
import com.intellij.execution.actions.LazyRunConfigurationProducer
import com.intellij.execution.configurations.ConfigurationFactory
import com.intellij.execution.configurations.ConfigurationTypeUtil
import com.intellij.openapi.util.Ref
import com.intellij.psi.PsiElement
import java.io.File

abstract class HixRunConfigurationProducer(private val tests: Boolean) :
    LazyRunConfigurationProducer<HixRunConfiguration>() {
    override fun getConfigurationFactory(): ConfigurationFactory {
        val type = ConfigurationTypeUtil.findConfigurationType(HixRunConfigurationType::class.java)
        return type.configurationFactories.single { it.id == if (tests) "HixTests" else "HixApplication" }
    }
    override fun setupConfigurationFromContext(configuration: HixRunConfiguration, context: ConfigurationContext,
                                               sourceElement: Ref<PsiElement>): Boolean {
        val virtualFile = context.psiLocation?.containingFile?.virtualFile ?: return false
        val manifest = closestManifest(File(virtualFile.path)) ?: return false
        val isTest = virtualFile.extension.equals("hix", true) && context.psiLocation?.containingFile?.text?.contains("%test") == true
        if (tests != isTest && !(virtualFile.name == "manifest.hal" && !tests)) return false
        configuration.data.manifestPath = manifest.path
        configuration.data.workingDirectory = manifest.parent
        configuration.data.test = tests
        configuration.name = if (tests) "Hix Tests: ${manifest.parentFile.name}" else "Hix: ${manifest.parentFile.name}"
        return true
    }
    override fun isConfigurationFromContext(configuration: HixRunConfiguration, context: ConfigurationContext): Boolean {
        val file = context.psiLocation?.containingFile?.virtualFile ?: return false
        val manifest = closestManifest(File(file.path)) ?: return false
        return configuration.data.test == tests && File(configuration.data.manifestPath.orEmpty()).absoluteFile == manifest.absoluteFile
    }
    private fun closestManifest(input: File): File? {
        var directory = if (input.isDirectory) input else input.parentFile
        while (directory != null) { val manifest = File(directory, "manifest.hal"); if (manifest.isFile) return manifest; directory = directory.parentFile }
        return null
    }
}

class HixApplicationRunConfigurationProducer : HixRunConfigurationProducer(false)
class HixTestRunConfigurationProducer : HixRunConfigurationProducer(true)
