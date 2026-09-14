package dev.helight.helix.hix.run

import com.intellij.execution.DefaultExecutionResult
import com.intellij.execution.ExecutionException
import com.intellij.execution.Executor
import com.intellij.execution.configurations.*
import com.intellij.execution.process.OSProcessHandler
import com.intellij.execution.process.ProcessHandler
import com.intellij.execution.process.ProcessOutputTypes
import com.intellij.execution.runners.ExecutionEnvironment
import com.intellij.execution.testframework.sm.SMTestRunnerConnectionUtil
import com.intellij.execution.testframework.sm.runner.SMTRunnerConsoleProperties
import com.intellij.openapi.fileChooser.FileChooserDescriptorFactory
import com.intellij.openapi.options.SettingsEditor
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.TextFieldWithBrowseButton
import com.intellij.openapi.util.Key
import com.intellij.openapi.util.NlsContexts
import com.intellij.util.ui.FormBuilder
import com.intellij.util.execution.ParametersListUtil
import com.jetbrains.rd.framework.RdTaskResult
import com.jetbrains.rd.util.reactive.adviseOnce
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.plugins.unity.UnityProjectLifetimeService
import dev.helight.helix.protocol.HixExecutionRequest
import dev.helight.helix.protocol.helixExpressionModel
import java.awt.BorderLayout
import javax.swing.JComponent
import javax.swing.JPanel
import javax.swing.JTextField
import javax.swing.Icon

class HixRunConfigurationOptions : RunConfigurationOptions() {
    var manifestPath by string("")
    var entry by string("")
    var arguments by string("")
    var workingDirectory by string("")
    var toolchainPath by string("")
    var test by property(false)
}

class HixRunConfiguration(project: Project, factory: ConfigurationFactory, name: String) :
    RunConfigurationBase<HixRunConfigurationOptions>(project, factory, name) {
    override fun getOptions() = super.getOptions() as HixRunConfigurationOptions
    val data: HixRunConfigurationOptions get() = options
    override fun getConfigurationEditor() = HixRunSettingsEditor()
    override fun checkConfiguration() {
        if (options.manifestPath.isNullOrBlank()) throw RuntimeConfigurationError("Manifest path is required")
    }
    override fun getState(executor: Executor, environment: ExecutionEnvironment): RunProfileState =
        HixRunState(environment, this)
}

class HixRunConfigurationType : ConfigurationTypeBase(
    "HixRunConfiguration", "Hix", "Run a manifest-based Hix project", null as Icon?
) {
    init {
        addFactory(object : ConfigurationFactory(this) {
            override fun getId() = "HixApplication"
            override fun getName() = "Hix Application"
            override fun createTemplateConfiguration(project: Project) = HixRunConfiguration(project, this, "Hix").also { it.data.test = false }
            override fun getOptionsClass() = HixRunConfigurationOptions::class.java
        })
        addFactory(object : ConfigurationFactory(this) {
            override fun getId() = "HixTests"
            override fun getName() = "Hix Tests"
            override fun createTemplateConfiguration(project: Project) = HixRunConfiguration(project, this, "Hix Tests").also { it.data.test = true }
            override fun getOptionsClass() = HixRunConfigurationOptions::class.java
        })
    }
}

private class HixRunState(environment: ExecutionEnvironment, private val configuration: HixRunConfiguration) :
    CommandLineState(environment) {
    override fun execute(executor: Executor, runner: com.intellij.execution.runners.ProgramRunner<*>): com.intellij.execution.ExecutionResult {
        val handler = startProcess()
        val console = if (configuration.data.test) {
            val properties = object : SMTRunnerConsoleProperties(configuration, "Hix", executor) {
                init { isIdBasedTestTree = false }
            }
            SMTestRunnerConnectionUtil.createAndAttachConsole("Hix", handler, properties)
        } else createConsole(executor)
        console?.attachToProcess(handler)
        return DefaultExecutionResult(console, handler)
    }
    override fun startProcess(): ProcessHandler {
        val options = configuration.data
        val toolchain = options.toolchainPath.orEmpty().ifBlank { dev.helight.helix.workspace.WorkspaceSettings.getInstance(configuration.project).toolchainPath }
        val manifest = options.manifestPath.orEmpty()
        val entry = options.entry.orEmpty()
        val arguments = options.arguments.orEmpty()
        if (toolchain.isNotBlank()) {
            val command = GeneralCommandLine(toolchain)
                .withWorkDirectory(options.workingDirectory.orEmpty().ifBlank { configuration.project.basePath })
            if (options.test) command.addParameter("test")
            command.addParameter(manifest)
            if (!options.test && entry.isNotBlank()) command.addParameters("--entry", entry)
            if (!options.test && arguments.isNotBlank()) command.addParameters("--", *ParametersListUtil.parse(arguments).toTypedArray())
            if (options.test) command.addParameter("--ide-events")
            return OSProcessHandler(command)
        }
        throw com.intellij.execution.ExecutionException("Configure the Hix toolchain executable in HELIX settings.")
    }
}

class HixRunSettingsEditor : SettingsEditor<HixRunConfiguration>() {
    private val manifest = TextFieldWithBrowseButton()
    private val workingDirectory = TextFieldWithBrowseButton()
    private val entry = JTextField()
    private val arguments = JTextField()
    private val toolchain = TextFieldWithBrowseButton()
    private val panel = FormBuilder.createFormBuilder()
        .addLabeledComponent("Manifest:", manifest)
        .addLabeledComponent("Working directory:", workingDirectory)
        .addLabeledComponent("Entry override:", entry)
        .addLabeledComponent("Arguments:", arguments)
        .addLabeledComponent("Toolchain override (optional):", toolchain).panel
    override fun createEditor(): JComponent = panel
    override fun resetEditorFrom(configuration: HixRunConfiguration) {
        manifest.text = configuration.data.manifestPath.orEmpty(); workingDirectory.text = configuration.data.workingDirectory.orEmpty()
        entry.text = configuration.data.entry.orEmpty(); arguments.text = configuration.data.arguments.orEmpty(); toolchain.text = configuration.data.toolchainPath.orEmpty()
        manifest.addBrowseFolderListener(configuration.project, FileChooserDescriptorFactory.createSingleFileDescriptor("hal"))
        workingDirectory.addBrowseFolderListener(configuration.project, FileChooserDescriptorFactory.createSingleFolderDescriptor())
        toolchain.addBrowseFolderListener(configuration.project, FileChooserDescriptorFactory.createSingleFileNoJarsDescriptor())
    }
    override fun applyEditorTo(configuration: HixRunConfiguration) {
        configuration.data.manifestPath = manifest.text; configuration.data.workingDirectory = workingDirectory.text
        configuration.data.entry = entry.text; configuration.data.arguments = arguments.text; configuration.data.toolchainPath = toolchain.text
    }
}
