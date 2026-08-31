package dev.helight.helix.mixin

import com.intellij.lang.Language
import com.intellij.psi.codeStyle.CommonCodeStyleSettings
import com.intellij.psi.codeStyle.LanguageCodeStyleSettingsProvider

class HelixMixinCodeStyleSettingsProvider : LanguageCodeStyleSettingsProvider() {
    override fun getLanguage(): Language = HelixMixinLanguage

    override fun getCodeSample(settingsType: SettingsType): String = """@FUNC<Build>
  @SCOPE
    @RETURN @param
@END
"""

    override fun customizeDefaults(commonSettings: CommonCodeStyleSettings,
                                   indentOptions: CommonCodeStyleSettings.IndentOptions) {
        indentOptions.INDENT_SIZE = 2
        indentOptions.CONTINUATION_INDENT_SIZE = 2
        indentOptions.TAB_SIZE = 2
        indentOptions.USE_TAB_CHARACTER = false
    }
}
