package dev.helight.helix.hix

import com.intellij.codeInsight.hints.InlayParameterHintsProvider
import com.intellij.codeInsight.hints.declarative.InlayHintsProvider
import com.intellij.codeInsight.hints.declarative.InlayHintsProviderExtensionBean
import com.intellij.lang.parameterInfo.ParameterInfoHandler
import com.intellij.openapi.project.DumbAware
import com.intellij.util.xmlb.XmlSerializer
import org.jdom.input.SAXBuilder
import java.util.Properties
import kotlin.test.*

class HixEditorExtensionsTest {
    @Test
    fun `packaged parameter info and parameter hints use the platform extension points`() {
        val root = SAXBuilder().build(javaClass.getResourceAsStream("/META-INF/plugin.xml")).rootElement
        val extensions = root.getChildren("extensions").flatMap { it.children }
        for (language in listOf("Hix", "Hal")) {
            val info = extensions.single { it.name == "codeInsight.parameterInfo" && it.getAttributeValue("language") == language }
            val handler = Class.forName(info.getAttributeValue("implementationClass")).getDeclaredConstructor().newInstance()
            assertIs<ParameterInfoHandler<*, *>>(handler)
            assertIs<DumbAware>(handler)
            val hints = extensions.single { it.name == "codeInsight.parameterNameHints" && it.getAttributeValue("language") == language }
            assertIs<InlayParameterHintsProvider>(Class.forName(hints.getAttributeValue("implementationClass")).getDeclaredConstructor().newInstance())
        }
    }
    @Test
    fun `declarative hints deserialize their options bundles and previews`() {
        val root = SAXBuilder().build(javaClass.getResourceAsStream("/META-INF/plugin.xml")).rootElement
        val entries = root.getChildren("extensions").flatMap { it.children }.filter { it.name == "codeInsight.declarativeInlayProvider" }
        val messages = Properties().apply { this@HixEditorExtensionsTest.javaClass.getResourceAsStream("/messages/HelixMessagesBundle.properties")!!.use(::load) }
        assertEquals(2, entries.size)
        entries.forEach { entry ->
            val bean = XmlSerializer.deserialize(entry, InlayHintsProviderExtensionBean::class.java)
            assertNotNull(bean.requiredGroup())
            assertTrue(messages.containsKey(bean.nameKey))
            assertTrue(messages.containsKey(bean.descriptionKey))
            val extension = if (bean.language == "Hix") "hix" else "hal"
            assertNotNull(javaClass.getResource("/inlayProviders/${bean.providerId}/preview.$extension"))
            assertIs<InlayHintsProvider>(Class.forName(bean.implementationClass).getDeclaredConstructor().newInstance())
            bean.options.forEach { assertTrue(messages.containsKey(it.nameKey)); assertNotNull(it.requireOptionId()) }
            if (bean.language == "Hix") {
                assertTrue(bean.isEnabledByDefault)
                assertEquals(listOf("inferredTypes", "conversions"), bean.options.map { it.optionId })
            } else assertFalse(bean.isEnabledByDefault)
        }
    }
}
