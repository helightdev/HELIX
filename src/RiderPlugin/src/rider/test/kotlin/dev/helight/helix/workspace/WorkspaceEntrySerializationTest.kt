package dev.helight.helix.workspace

import com.intellij.util.xmlb.XmlSerializer
import kotlin.test.Test
import kotlin.test.assertEquals

class WorkspaceEntrySerializationTest {
    @Test
    fun `all workspace entry fields survive XML serialization`() {
        val expected = WorkspaceEntry(
            typeId = "directory",
            name = "Project Scripts",
            showPath = false,
            options = mapOf("path" to "Assets/_Project", "useFileNesting" to "true"),
        )

        val serialized = XmlSerializer.serialize(expected)
        val restored = XmlSerializer.deserialize(serialized, WorkspaceEntry::class.java)

        assertEquals(expected, restored)
    }

    @Test
    fun `workspace settings preserve multiple typed entries`() {
        val expected = WorkspaceSettingsState(
            entries = listOf(
                WorkspaceEntry(
                    typeId = "directory",
                    name = "Project Scripts",
                    showPath = false,
                    options = mapOf("path" to "Assets/_Project", "useFileNesting" to "true"),
                ),
                WorkspaceEntry(typeId = "unity-packages", name = "Unity Packages", showPath = false),
            ),
        )

        val serialized = XmlSerializer.serialize(expected)
        val restored = XmlSerializer.deserialize(serialized, WorkspaceSettingsState::class.java)

        assertEquals(expected, restored)
    }
}
