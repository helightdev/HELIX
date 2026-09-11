package dev.helight.helix.hix.project

import com.intellij.icons.AllIcons
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.PlatformProjectOpenProcessor
import com.intellij.projectImport.ProjectOpenProcessor
import dev.helight.helix.hix.HixFileType

/** Opens a manifest directory as a standalone IDE project instead of asking Rider for a solution. */
class HixProjectOpenProcessor : ProjectOpenProcessor() {
    override val name: String = "Hix"

    override fun getIcon(file: VirtualFile) = HixFileType.icon ?: AllIcons.FileTypes.Custom

    override fun canOpenProject(file: VirtualFile): Boolean = projectDirectory(file) != null

    override fun isProjectFile(file: VirtualFile): Boolean =
        !file.isDirectory && file.name.equals(MANIFEST_FILE, ignoreCase = false)

    override fun lookForProjectsInDirectory(): Boolean = true

    override suspend fun openProjectAsync(
        virtualFile: VirtualFile,
        projectOpenOptions: ProjectOpenOptions,
    ): Project? {
        val directory = projectDirectory(virtualFile) ?: return null
        return PlatformProjectOpenProcessor.getInstance().openProjectAsync(
            directory,
            projectOpenOptions,
        )
    }

    private fun projectDirectory(file: VirtualFile): VirtualFile? = when {
        file.isDirectory && file.findChild(MANIFEST_FILE)?.isDirectory == false -> file
        !file.isDirectory && file.name == MANIFEST_FILE -> file.parent
        else -> null
    }
}
