package dev.helight.helix.hal

import com.intellij.codeInsight.completion.*
import com.intellij.codeInsight.daemon.DaemonCodeAnalyzer
import com.intellij.codeInsight.editorActions.SimpleTokenSetQuoteHandler
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.extapi.psi.ASTWrapperPsiElement
import com.intellij.extapi.psi.PsiFileBase
import com.intellij.lang.*
import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.lexer.Lexer
import com.intellij.lexer.LexerBase
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.editor.EditorFactory
import com.intellij.openapi.editor.event.DocumentEvent
import com.intellij.openapi.editor.event.DocumentListener
import com.intellij.openapi.fileEditor.FileDocumentManager
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.editor.DefaultLanguageHighlighterColors
import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.openapi.fileTypes.*
import com.intellij.openapi.project.Project
import com.intellij.openapi.util.TextRange
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.patterns.PlatformPatterns
import com.intellij.psi.*
import com.intellij.psi.tree.*
import com.intellij.util.ProcessingContext
import dev.helight.helix.hal.generated.HalLexer as GeneratedHalLexer
import dev.helight.helix.hal.generated.HalParser
import dev.helight.helix.hix.HixColors
import org.antlr.v4.runtime.ParserRuleContext
import java.util.regex.Pattern

object HalLanguage : Language("Hal")
object HalFileType : LanguageFileType(HalLanguage) {
    override fun getName() = "Hal"
    override fun getDescription() = "Hix Asset Language"
    override fun getDefaultExtension() = "hal"
    override fun getIcon() = null
}

class HalLexer : LexerBase() {
    private var buffer: CharSequence = ""; private var end = 0; private var tokens = emptyList<HalAntlrToken>(); private var index = 0
    override fun start(buffer: CharSequence, startOffset: Int, endOffset: Int, initialState: Int) {
        this.buffer = buffer; end = endOffset
        tokens = HalAntlrSyntax.lex(buffer.subSequence(0, endOffset)).filter { it.end > startOffset }
            .map { it.copy(start = maxOf(startOffset, it.start), end = minOf(endOffset, it.end)) }
        index = 0
    }
    override fun getState() = 0
    override fun getTokenType(): IElementType? = tokens.getOrNull(index)?.let { HalAntlrTypes.tokens[it.type] }
    override fun getTokenStart() = tokens.getOrNull(index)?.start ?: end
    override fun getTokenEnd() = tokens.getOrNull(index)?.end ?: end
    override fun advance() { if (index < tokens.size) index++ }
    override fun getBufferSequence() = buffer
    override fun getBufferEnd() = end
}

private val FILE = IFileElementType(HalLanguage)
class HalFile(viewProvider: FileViewProvider) : PsiFileBase(viewProvider, HalLanguage) {
    override fun getFileType() = HalFileType
    override fun toString() = "Hal File"
}
class HalParserDefinition : ParserDefinition {
    override fun createLexer(project: Project?) = HalLexer()
    override fun createParser(project: Project): PsiParser = HalPsiParser(project)
    override fun getFileNodeType() = FILE
    override fun getWhitespaceTokens() = TokenSet.create(HalAntlrTypes.tokens[0])
    override fun getCommentTokens() = TokenSet.create(HalAntlrTypes.tokens[GeneratedHalLexer.LINE_COMMENT])
    override fun getStringLiteralElements() = TokenSet.create(HalAntlrTypes.tokens[GeneratedHalLexer.STRING])
    override fun createElement(node: ASTNode): PsiElement = ASTWrapperPsiElement(node)
    override fun createFile(viewProvider: FileViewProvider) = HalFile(viewProvider)
}

class HalSyntaxHighlighterFactory : SyntaxHighlighterFactory() {
    override fun getSyntaxHighlighter(project: Project?, virtualFile: VirtualFile?) = object : SyntaxHighlighterBase() {
        override fun getHighlightingLexer(): Lexer = HalLexer()
        override fun getTokenHighlights(type: IElementType) = pack(when ((type as? HalAntlrTokenType)?.antlrType) {
            GeneratedHalLexer.SECTION -> HixColors.SEPARATOR; GeneratedHalLexer.PERCENT -> HixColors.METADATA
            GeneratedHalLexer.STRING -> HixColors.ARGUMENT; GeneratedHalLexer.NUMBER -> HixColors.NUMBER
            GeneratedHalLexer.TRUE, GeneratedHalLexer.FALSE, GeneratedHalLexer.NULL -> HixColors.KEYWORD
            GeneratedHalLexer.LINE_COMMENT -> HixColors.COMMENT
            GeneratedHalLexer.EQUALS, GeneratedHalLexer.COMMA, GeneratedHalLexer.AMP, GeneratedHalLexer.HASH -> HixColors.OPERATOR
            GeneratedHalLexer.ERROR_TOKEN -> HixColors.BAD; else -> null
        })
    }
}

class HalBraceMatcher : PairedBraceMatcher {
    override fun getPairs() = arrayOf(
        BracePair(HalAntlrTypes.tokens[GeneratedHalLexer.LBRACE], HalAntlrTypes.tokens[GeneratedHalLexer.RBRACE], true),
        BracePair(HalAntlrTypes.tokens[GeneratedHalLexer.LBRACKET], HalAntlrTypes.tokens[GeneratedHalLexer.RBRACKET], true),
        BracePair(HalAntlrTypes.tokens[GeneratedHalLexer.LPAREN], HalAntlrTypes.tokens[GeneratedHalLexer.RPAREN], false))
    override fun isPairedBracesAllowedBeforeType(lbraceType: IElementType, contextType: IElementType?) = true
    override fun getCodeConstructStart(file: PsiFile, openingBraceOffset: Int) = openingBraceOffset
}
class HalQuoteHandler : SimpleTokenSetQuoteHandler(HalAntlrTypes.tokens[GeneratedHalLexer.STRING])

private object HalColors {
    val SECTION_ANCHOR: TextAttributesKey = TextAttributesKey.createTextAttributesKey(
        "HAL_SECTION_ANCHOR", DefaultLanguageHighlighterColors.MARKUP_ENTITY)
}

internal fun fieldName(context: HalParser.FieldKeyContext): String =
    context.IDENTIFIER()?.text ?: unescapeString(context.STRING().text)

private fun unescapeString(text: String): String = buildString {
    var index = 1
    while (index < text.lastIndex) {
        val current = text[index++]
        if (current != '\\' || index >= text.lastIndex) append(current)
        else append(when (val escaped = text[index++]) {
            'n' -> '\n'; 'r' -> '\r'; 't' -> '\t'; else -> escaped
        })
    }
}

internal object HalManifest {
    data class Field(val type: String, val name: String, val required: Boolean, val many: Boolean = false)
    fun closest(file: VirtualFile?): VirtualFile? {
        var directory = file?.parent
        while (directory != null) { directory.findChild("manifest.hix")?.let { return it }; directory = directory.parent }
        return null
    }
    fun text(file: VirtualFile?): String {
        val manifest = closest(file) ?: return ""
        val sources = arrayListOf<String>(); val visited = hashSetOf<String>()
        fun visit(sourceFile: VirtualFile) {
            if (!visited.add(sourceFile.path)) return
            val source = FileDocumentManager.getInstance().getCachedDocument(sourceFile)?.text
                ?: runCatching { String(sourceFile.contentsToByteArray()) }.getOrNull() ?: return
            Regex("%import<([^>]+)>").findAll(source).forEach { match ->
                imported(sourceFile.parent, match.groupValues[1]).forEach(::visit)
            }
            sources += source
        }
        visit(manifest); return sources.joinToString("\n")
    }
    private fun imported(base: VirtualFile, pattern: String): List<VirtualFile> {
        val normalized = pattern.replace('\\', '/')
        val wildcard = normalized.indexOf('*')
        if (wildcard < 0) return listOfNotNull(base.findFileByRelativePath(normalized))
        val rootPath = normalized.substring(0, wildcard).substringBeforeLast('/', "")
        val root = if (rootPath.isEmpty()) base else base.findFileByRelativePath(rootPath) ?: return emptyList()
        val recursive = normalized.contains("**")
        val files = arrayListOf<VirtualFile>()
        fun collect(directory: VirtualFile) {
            directory.children.sortedBy { it.name.lowercase() }.forEach {
                if (it.isDirectory && recursive) collect(it)
                else if (!it.isDirectory && it.extension.equals("hix", true)) files += it
            }
        }
        collect(root); return files
    }
    fun types(text: String) = Regex("\\btype\\s+([A-Za-z_][\\w.]*)\\s*=").findAll(text)
        .map { it.groupValues[1] }.distinct().toList()
    fun fieldDefinitions(text: String, type: String): List<Field> {
        val body = Pattern.compile("(?s)\\btype\\s+" + Pattern.quote(type) + "\\s*=\\s*(?:%[^{\\n]+\\s+)*@?\\{(.*?)\\}")
            .matcher(text).let { if (it.find()) it.group(1) else return emptyList() }
        return body.split(',').mapNotNull { source ->
            val entry = source.trim()
            val metadata = Regex("^((?:%\\S+\\s+)*)").find(entry)?.groupValues?.get(1).orEmpty()
            val declaration = entry.removePrefix(metadata).substringBefore('=').trim()
            val words = Regex("[A-Za-z_][\\w.]*").findAll(declaration).map { it.value }.toList()
            val manyType = Regex("%many<([^>]+)>").find(metadata)?.groupValues?.get(1)
            val many = manyType != null || Regex("(?:^|\\s)%many(?:\\s|$)").containsMatchIn(metadata)
            val name = words.lastOrNull() ?: return@mapNotNull null
            val fieldType = manyType ?: words.getOrNull(words.lastIndex - 1) ?: if (many) "any" else return@mapNotNull null
            val optional = Regex("(?:^|\\s)%optional(?:\\s|$)").containsMatchIn(metadata)
            Field(fieldType, name, !entry.contains('=') && !optional, many)
        }
            .distinctBy { it.name }.toList()
    }
    fun manyElement(text: String, type: String): String? {
        val declaration = Regex("(?s)\\btype\\s+" + Pattern.quote(type) + "\\s*=\\s*(.*?)(?=\\n\\s*(?:%|type\\s|func\\s|mixin\\s)|\\z)")
            .find(text)?.groupValues?.get(1) ?: return null
        return Regex("%many<([^>]+)>").find(declaration)?.groupValues?.get(1)
    }
    fun unionMembers(text: String, type: String): List<String> {
        val declaration = Regex("(?s)\\btype\\s+" + Pattern.quote(type) + "\\s*=\\s*(.*?)(?=;|\\n\\s*(?:%|type\\s|func\\s|mixin\\s)|\\z)")
            .find(text)?.groupValues?.get(1) ?: return emptyList()
        val arguments = Regex("%union((?:<[^>]+>)+)").find(declaration)?.groupValues?.get(1) ?: return emptyList()
        return Regex("<([^>]+)>").findAll(arguments).map { it.groupValues[1] }.toList()
    }
    fun fields(text: String, type: String) = fieldDefinitions(text, type).map { it.name }
}

@Service(Service.Level.PROJECT)
internal class HalDependencyService(private val project: Project) {
    init {
        EditorFactory.getInstance().eventMulticaster.addDocumentListener(object : DocumentListener {
            override fun documentChanged(event: DocumentEvent) {
                val changed = FileDocumentManager.getInstance().getFile(event.document) ?: return
                if (!changed.extension.equals("hix", true)) return
                ApplicationManager.getApplication().invokeLater {
                    if (project.isDisposed) return@invokeLater
                    val psiManager = PsiManager.getInstance(project)
                    val daemon = DaemonCodeAnalyzer.getInstance(project)
                    FileEditorManager.getInstance(project).openFiles
                        .filter { it.extension.equals("hal", true) }
                        .mapNotNull(psiManager::findFile)
                        .forEach(daemon::restart)
                }
            }
        }, project)
    }

    companion object {
        fun ensure(project: Project) { project.service<HalDependencyService>() }
    }
}

internal data class HalSection(val type: String, val id: String?, val headerStart: Int, val headerEnd: Int,
                               val contentStart: Int, val contentEnd: Int)
internal data class HalAssignment(val name: String, val start: Int, val end: Int)

internal object HalStructure {
    fun sections(source: String): List<HalSection> {
        val parsed = HalAntlrSyntax.parse(source).tree.sectionBlock().map { it.section() }
        return parsed.mapIndexed { index, section ->
            val lineEnd = section.lineEnd().stop.stopIndex + 1
            HalSection(section.IDENTIFIER().text, section.scalarAtom()?.text ?: if (index == 0) "0" else null,
                section.start.startIndex, lineEnd, lineEnd,
                parsed.getOrNull(index + 1)?.start?.startIndex ?: source.length)
        }
    }

    fun assignments(source: String, section: HalSection): List<HalAssignment> {
        val parsed = HalAntlrSyntax.parse(source).tree.sectionBlock().map { it.section() }.firstOrNull {
            it.start.startIndex == section.headerStart
        } ?: return emptyList()
        return parsed.sectionEntry().mapNotNull { it.field()?.fieldKey() }.map {
            HalAssignment(fieldName(it), it.start.startIndex, it.stop.stopIndex + 1)
        }
    }

    fun completionScope(source: String, offset: Int): Pair<String, Set<String>>? {
        val tree = HalAntlrSyntax.parse(source).tree
        val table = rules(tree).filterIsInstance<HalParser.TableContext>().filter {
            it.start.startIndex <= offset && it.stop.stopIndex + 1 >= offset
        }.maxByOrNull { it.start.startIndex }
        if (table != null) {
            val typedContainer = table.parent as? HalParser.TypedContainerContext
            val type = when (val parent = typedContainer?.parent) {
                is HalParser.ValueContext -> parent.IDENTIFIER()?.text
                is HalParser.CollectionValueContext -> parent.IDENTIFIER()?.text
                else -> null
            }
            type?.let {
                return it to table.tableEntry().map { entry -> fieldName(entry.fieldKey()) }.toSet()
            }
        }
        val section = tree.sectionBlock().map { it.section() }.lastOrNull { it.start.startIndex <= offset } ?: return null
        return section.IDENTIFIER().text to section.sectionEntry().mapNotNull { it.field()?.fieldKey() }
            .map(::fieldName).toSet()
    }

    private fun rules(context: ParserRuleContext): Sequence<ParserRuleContext> = sequence {
        yield(context)
        context.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { yieldAll(rules(it)) }
    }
}

class HalCompletionContributor : CompletionContributor() {
    init { extend(CompletionType.BASIC, PlatformPatterns.psiElement().withLanguage(HalLanguage), Provider) }
    private object Provider : CompletionProvider<CompletionParameters>() {
        override fun addCompletions(parameters: CompletionParameters, context: ProcessingContext, result: CompletionResultSet) {
            HalDependencyService.ensure(parameters.originalFile.project)
            val source = parameters.originalFile.text; val offset = parameters.offset.coerceIn(0, source.length)
            val before = source.substring(0, offset); val manifest = HalManifest.text(parameters.originalFile.virtualFile)
            val line = before.substringAfterLast('\n'); val types = HalManifest.types(manifest)
            if (line.trimStart().startsWith("---") || line.substringBeforeLast('{', "").isNotEmpty() && line.trim().isEmpty())
                types.forEach { result.addElement(LookupElementBuilder.create(it).withTypeText("Hix pattern")) }
            val scope = HalStructure.completionScope(source, offset)
            if (line.isBlank() || !line.contains('=')) scope?.let { (type, assigned) ->
                HalManifest.fieldDefinitions(manifest, type).filter { it.name !in assigned }.forEach {
                    result.addElement(LookupElementBuilder.create("${it.name} = ").withPresentableText(it.name).withTypeText(it.type))
                }
            }
            if (line.contains("ref(") || line.trimStart().startsWith("ref"))
                Regex("(?m)^---\\s+\\S+\\s+&([A-Za-z0-9_.-]+)").findAll(source).map { it.groupValues[1] }.forEach {
                    result.addElement(LookupElementBuilder.create(it).withTypeText("local section"))
                }
        }
    }
}

class HalAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        if (element !is PsiFile) return
        HalDependencyService.ensure(element.project)
        val source = element.text; val parsed = HalAntlrSyntax.parse(source); val sections = HalStructure.sections(source)
        parsed.diagnostics.forEach {
            holder.newAnnotation(HighlightSeverity.ERROR, it.message).range(TextRange(it.start, it.end)).create()
        }
        style(parsed.tree, holder)
        if (sections.isEmpty()) holder.newAnnotation(HighlightSeverity.ERROR, "Hal requires a typed primary section").range(element.textRange).create()
        sections.drop(1).filter { it.id == null }.forEach {
            holder.newAnnotation(HighlightSeverity.ERROR, "Only the first section may omit its local id")
                .range(TextRange(it.headerStart, it.headerEnd)).create()
        }
        sections.mapNotNull(HalSection::id).groupBy { it }.filterValues { it.size > 1 }.keys.forEach { id ->
            val match = sections.last { it.id == id }
            holder.newAnnotation(HighlightSeverity.ERROR, "Duplicate local section id '$id'")
                .range(TextRange(match.headerStart, match.headerEnd)).create()
        }
        val manifest = HalManifest.closest(element.virtualFile)
        if (manifest == null)
            holder.newAnnotation(HighlightSeverity.WARNING, "No manifest.hix found in this directory or its ancestors")
                .range(TextRange(0, minOf(source.length, source.indexOf('\n').let { if (it < 0) source.length else it }))).create()
        else {
            val manifestText = HalManifest.text(element.virtualFile)
            val known = HalManifest.types(manifestText).toSet() +
                setOf("any", "null", "bool", "number", "string", "tuple", "table")
            sections.filter { it.type !in known }.forEach {
                holder.newAnnotation(HighlightSeverity.ERROR, "Unknown Hix pattern '${it.type}'")
                    .range(TextRange(it.headerStart + 4, it.headerStart + 4 + it.type.length)).create()
            }
            parsed.tree.sectionBlock().forEach { block ->
                val section = block.section()
                if (section.IDENTIFIER().text in known) validateFields(holder, manifestText, section.IDENTIFIER().text,
                    section.sectionEntry().mapNotNull { entry -> entry.field()?.let(::fieldSite) },
                    TextRange(section.start.startIndex, section.lineEnd().stop.stopIndex + 1), known)
            }
        }
    }

    private data class HalValueSite(val value: HalParser.ValueContext?, val collection: HalParser.CollectionValueContext?) {
        val context: ParserRuleContext get() = value ?: collection!!
        val explicitType: String? get() = value?.IDENTIFIER()?.text ?: collection?.IDENTIFIER()?.text
        val table: HalParser.TableContext? get() = value?.typedContainer()?.table() ?: value?.table()
            ?: collection?.typedContainer()?.table()
        val list: HalParser.ListContext? get() = value?.typedContainer()?.list() ?: value?.list()
            ?: collection?.typedContainer()?.list()
        val scalar: HalParser.LooseScalarContext? get() = value?.looseScalar()
    }

    private data class HalFieldSite(val token: org.antlr.v4.runtime.Token, val name: String, val value: HalValueSite)

    private fun fieldSite(field: HalParser.FieldContext) = HalFieldSite(field.fieldKey().start, fieldName(field.fieldKey()),
        HalValueSite(field.value(), field.collectionValue()))
    private fun fieldSite(field: HalParser.TableEntryContext) = HalFieldSite(field.fieldKey().start, fieldName(field.fieldKey()),
        HalValueSite(field.value(), field.collectionValue()))

    private fun validateFields(holder: AnnotationHolder, manifest: String, type: String,
                               values: List<HalFieldSite>,
                               containerRange: TextRange, known: Set<String>) {
        val definitions = HalManifest.fieldDefinitions(manifest, type)
        if (definitions.isEmpty()) return
        val byName = definitions.associateBy(HalManifest.Field::name)
        values.groupBy(HalFieldSite::name).forEach { (name, occurrences) ->
            if (name !in byName) occurrences.forEach { error(holder, it.token, "Unknown field '$name' for $type") }
            occurrences.drop(1).forEach { error(holder, it.token, "Duplicate field '$name'") }
        }
        val assigned = values.map(HalFieldSite::name).toSet()
        definitions.filter { it.required && it.name !in assigned }.forEach {
            holder.newAnnotation(HighlightSeverity.ERROR, "Missing required field '${it.name}' for $type").range(containerRange).create()
        }
        values.forEach { site -> byName[site.name]?.let { validateValue(holder, manifest, it, site.value, known) } }
    }

    private fun validateValue(holder: AnnotationHolder, manifest: String, field: HalManifest.Field,
                              value: HalValueSite, known: Set<String>) {
        val aliasElement = HalManifest.manyElement(manifest, field.type)
        val elementType = if (field.many) field.type else aliasElement
        if (elementType != null) {
            val list = value.list
            if (list == null) {
                error(holder, value.context, "Field '${field.name}' expects a list of $elementType")
                return
            }
            value.context.start.takeIf { aliasElement != null && value.explicitType != null && value.explicitType != field.type }?.let {
                error(holder, it, "Expected typed list ${field.type}, found '${it.text}'")
            }
            list.value().forEach { validateExpected(holder, manifest, elementType, HalValueSite(it, null), known) }
            return
        }
        validateExpected(holder, manifest, field.type, value, known, field.name)
    }

    private fun validateExpected(holder: AnnotationHolder, manifest: String, expected: String,
                                 value: HalValueSite, known: Set<String>, fieldName: String? = null) {
        val explicitType = value.explicitType
        val union = HalManifest.unionMembers(manifest, expected)
        if (union.isNotEmpty()) {
            if (explicitType != null && explicitType in union)
                validateExpected(holder, manifest, explicitType, value, known, fieldName)
            else error(holder, value.context, "Expected a typed ${union.joinToString(" or ")} value for $expected")
            return
        }
        if (explicitType != null && explicitType != expected)
            error(holder, value.context.start, "Expected $expected, found typed $explicitType")
        val actualType = explicitType ?: expected
        val tableDefinitions = HalManifest.fieldDefinitions(manifest, actualType)
        if (tableDefinitions.isNotEmpty()) {
            val table = value.table
            if (table == null) {
                error(holder, value.context, (fieldName?.let { "Field '$it' " } ?: "Value ") + "expects an object of $expected")
                return
            }
            validateFields(holder, manifest, actualType,
                table.tableEntry().map(::fieldSite), range(table), known)
            return
        }
        HalManifest.manyElement(manifest, actualType)?.let { element ->
            val list = value.list
            if (list == null) error(holder, value.context, "Expected a list of $element")
            else list.value().forEach { validateExpected(holder, manifest, element, HalValueSite(it, null), known) }
            return
        }
        if (actualType !in known) {
            value.context.start.takeIf { explicitType != null }?.let { error(holder, it, "Unknown Hix pattern '$actualType'") }
            return
        }
        val atoms = value.scalar?.scalarAtom() ?: return
        if (actualType == "string") {
            if (atoms.isEmpty()) error(holder, value.context, "Expected string value")
            return
        }
        val scalar = atoms.singleOrNull()
        if (scalar == null) {
            error(holder, value.context, "Expected $actualType value")
            return
        }
        val valid = when (actualType) {
            "number" -> scalar.NUMBER() != null
            "bool" -> scalar.TRUE() != null || scalar.FALSE() != null
            "null" -> scalar.NULL() != null
            else -> true
        }
        if (!valid) error(holder, value.context, "Expected $actualType value")
    }

    private fun style(tree: ParserRuleContext, holder: AnnotationHolder) {
        val all = rules(tree).toList()
        all.filterIsInstance<HalParser.FieldKeyContext>().forEach { highlight(holder, it, HixColors.FIELD) }
        all.filterIsInstance<HalParser.SectionContext>().forEach { section ->
            highlight(holder, section.IDENTIFIER().symbol, HixColors.TYPE)
            section.AMP()?.symbol?.let { amp ->
                val end = section.scalarAtom()?.stop?.stopIndex ?: amp.stopIndex
                highlight(holder, TextRange(amp.startIndex, end + 1), HalColors.SECTION_ANCHOR)
            }
        }
        all.filterIsInstance<HalParser.ValueContext>().filter { it.typedContainer() != null }
            .mapNotNull { it.IDENTIFIER()?.symbol }.forEach { highlight(holder, it, HixColors.TYPE) }
        all.filterIsInstance<HalParser.CollectionValueContext>().mapNotNull { it.IDENTIFIER()?.symbol }
            .forEach { highlight(holder, it, HixColors.TYPE) }
        all.filterIsInstance<HalParser.CallContext>().forEach { highlight(holder, it.IDENTIFIER().symbol, HixColors.FUNCTION) }
        all.filterIsInstance<HalParser.LooseScalarContext>().filter { scalar ->
            val atoms = scalar.scalarAtom()
            atoms.size != 1 || atoms.single().let { it.NUMBER() == null && it.TRUE() == null && it.FALSE() == null && it.NULL() == null }
        }.forEach { highlight(holder, it, HixColors.ARGUMENT) }
        all.filterIsInstance<HalParser.MetadataContext>().forEach { highlight(holder, it, HixColors.METADATA) }
    }

    private fun highlight(holder: AnnotationHolder, token: org.antlr.v4.runtime.Token, attributes: TextAttributesKey) =
        highlight(holder, TextRange(token.startIndex, token.stopIndex + 1), attributes)
    private fun highlight(holder: AnnotationHolder, context: ParserRuleContext, attributes: TextAttributesKey) =
        highlight(holder, range(context), attributes)
    private fun highlight(holder: AnnotationHolder, range: TextRange, attributes: TextAttributesKey) {
        holder.newSilentAnnotation(HighlightSeverity.INFORMATION).range(range).textAttributes(attributes).create()
    }

    private fun error(holder: AnnotationHolder, token: org.antlr.v4.runtime.Token, message: String) {
        holder.newAnnotation(HighlightSeverity.ERROR, message)
            .range(TextRange(token.startIndex, token.stopIndex + 1)).create()
    }
    private fun error(holder: AnnotationHolder, context: ParserRuleContext, message: String) {
        holder.newAnnotation(HighlightSeverity.ERROR, message).range(range(context)).create()
    }
    private fun range(context: ParserRuleContext) = TextRange(context.start.startIndex, context.stop.stopIndex + 1)

    private fun rules(context: ParserRuleContext): Sequence<ParserRuleContext> = sequence {
        yield(context)
        context.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { yieldAll(rules(it)) }
    }
}
