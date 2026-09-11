package dev.helight.helix.hix.project

import com.intellij.ide.util.projectWizard.WizardContext
import com.intellij.ide.wizard.AbstractNewProjectWizardStep
import com.intellij.ide.wizard.GeneratorNewProjectWizard
import com.intellij.ide.wizard.NewProjectWizardBaseStep
import com.intellij.ide.wizard.NewProjectWizardChainStep.Companion.nextStep
import com.intellij.ide.wizard.NewProjectWizardStep
import com.intellij.ide.wizard.RootNewProjectWizardStep
import com.intellij.icons.AllIcons
import com.intellij.openapi.application.runWriteAction
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VfsUtil
import dev.helight.helix.hix.HixFileType
import java.nio.file.Files
import java.nio.file.Path

class HixProjectWizard : GeneratorNewProjectWizard {
    override val id: String = "hix"
    override val name: String = "Hix"
    override val icon = HixFileType.icon ?: AllIcons.FileTypes.Custom
    override val description: String = "Create a standalone manifest-based Hix application"
    override val groupName: String = "Hix"

    override fun createStep(context: WizardContext): NewProjectWizardStep =
        RootNewProjectWizardStep(context)
            .nextStep(::NewProjectWizardBaseStep)
            .nextStep(::HixProjectFilesStep)
}

private class HixProjectFilesStep(parent: NewProjectWizardStep) : AbstractNewProjectWizardStep(parent) {
    override fun setupProject(project: Project) {
        val root = project.basePath?.let(Path::of) ?: return
        val projectName = project.name.ifBlank { "hix-project" }

        Files.createDirectories(root.resolve("src"))
        Files.createDirectories(root.resolve("test"))
        Files.writeString(
            root.resolve(MANIFEST_FILE),
            "--- Manifest\nname = ${manifestIdentifier(projectName)}\nversion = 0.1.0\n",
        )
        Files.writeString(
            root.resolve("src/main.hix"),
            "func main { print(<Hello from Hix!>) }\n",
        )
        Files.writeString(
            root.resolve("test/main_test.hix"),
            "%backend<Test>\n---\n%test\nfunc mainWorks { assert(true) }\n",
        )
        runWriteAction { VfsUtil.markDirtyAndRefresh(false, true, true, root.toFile()) }
    }

    private fun manifestIdentifier(value: String): String = value
        .trim()
        .lowercase()
        .replace(Regex("[^a-z0-9_-]+"), "-")
        .trim('-')
        .ifBlank { "hix-project" }
}

internal const val MANIFEST_FILE = "manifest.hal"
