package dev.helight.helix.hal

import com.intellij.psi.codeStyle.CommonCodeStyleSettings
import com.intellij.psi.codeStyle.LanguageCodeStyleSettingsProvider

class HalCodeStyleSettingsProvider : LanguageCodeStyleSettingsProvider() {
    override fun getLanguage() = HalLanguage
    override fun getCodeSample(settingsType: SettingsType) =
        "--- Example\nvalue = {\n  first = 1,\n  second = 2\n}\n"
    override fun getDefaultCommonSettings(): CommonCodeStyleSettings =
        CommonCodeStyleSettings(HalLanguage).apply {
            initIndentOptions()
            indentOptions?.apply { INDENT_SIZE = 2; CONTINUATION_INDENT_SIZE = 2; TAB_SIZE = 2; USE_TAB_CHARACTER = false }
        }
    override fun customizeDefaults(commonSettings: CommonCodeStyleSettings,
                                   indentOptions: CommonCodeStyleSettings.IndentOptions) {
        indentOptions.INDENT_SIZE = 2
        indentOptions.CONTINUATION_INDENT_SIZE = 2
        indentOptions.TAB_SIZE = 2
        indentOptions.USE_TAB_CHARACTER = false
    }
}
