package dev.helight.helix.hix

import com.intellij.lang.Language
import com.intellij.psi.codeStyle.CommonCodeStyleSettings
import com.intellij.psi.codeStyle.LanguageCodeStyleSettingsProvider

class HixCodeStyleSettingsProvider : LanguageCodeStyleSettingsProvider() {
    override fun getLanguage(): Language = HixLanguage

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
