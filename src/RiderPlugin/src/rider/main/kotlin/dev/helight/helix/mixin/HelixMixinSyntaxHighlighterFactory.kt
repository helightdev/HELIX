package dev.helight.helix.mixin

import com.intellij.openapi.fileTypes.SyntaxHighlighter
import com.intellij.openapi.fileTypes.SyntaxHighlighterFactory
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.jetbrains.rider.ideaInterop.fileTypes.RiderDummySyntaxHighlighter

class HelixMixinSyntaxHighlighterFactory : SyntaxHighlighterFactory() {
    override fun getSyntaxHighlighter(project: Project?, virtualFile: VirtualFile?): SyntaxHighlighter =
        RiderDummySyntaxHighlighter(HelixMixinLanguage)
}
