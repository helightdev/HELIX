package dev.helight.helix.hix

import java.nio.file.Files
import kotlin.test.*

class HixToolchainDetectionTest {
    @Test fun `linux detection skips non executable files and respects path order`() {
        val root = Files.createTempDirectory("hix-path").toFile()
        try {
            val first = root.resolve("first").apply { mkdir() }
            val second = root.resolve("with spaces").apply { mkdir() }
            first.resolve("hix").writeText("not executable")
            val executable = second.resolve("hix").apply { writeText("#!/bin/sh\n"); setExecutable(true) }
            assertEquals(executable.absolutePath, HixToolchainService.detectExecutable("$first:$second", false))
            assertNull(HixToolchainService.detectExecutable(first.path, false))
        } finally { root.deleteRecursively() }
    }
    @Test fun `windows detection handles semicolon path and exe names`() {
        val root = Files.createTempDirectory("hix-path").toFile()
        try {
            val executable = root.resolve("Hix.Toolchain.exe").apply { writeText("fixture") }
            assertEquals(executable.absolutePath, HixToolchainService.detectExecutable("missing;\"$root\"", true))
        } finally { root.deleteRecursively() }
    }
}
