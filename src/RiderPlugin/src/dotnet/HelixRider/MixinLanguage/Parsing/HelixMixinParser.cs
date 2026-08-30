#if RIDER
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi;
using HelixRider.MixinLanguage.Parsing.Gen;
using MixinLanguage.Compiler;

namespace HelixRider.MixinLanguage.Parsing;

internal sealed class HelixMixinParser : HelixMixinParserGenerated, IParser
{
    internal HelixMixinParser(ILexer<int> lexer) { SetLexer(lexer); }

    public IFile ParseFile() => (IFile)ParseHelixMixinFile();

    public override TreeElement ParseHelixMixinFile()
    {
        var file = TreeElementFactory.CreateCompositeElement(ElementType.HELIX_MIXIN_FILE);
        var document = TreeElementFactory.CreateCompositeElement(ElementType.DOCUMENT_NODE);
        file.AppendNewChild(document);

        var source = myLexer.Buffer.GetText(new JetBrains.Util.TextRange(0, myLexer.Buffer.Length));
        var syntax = MixinExpressionParser.ParseEditorSyntax(source).Root;
        AppendChildren(document, syntax.Children, syntax.SourceRange.End);
        while (myLexer.TokenType != null) document.AppendNewChild(CreateToken());
        return file;
    }

    private void AppendChildren(CompositeElement parent,
        System.Collections.Generic.IReadOnlyList<MixinEditorSyntaxNode> children, int parentEnd)
    {
        foreach (var child in children)
        {
            AppendTokensUntil(parent, child.SourceRange.Start);
            if (child.SourceRange.IsEmpty)
            {
                parent.AppendNewChild(TreeElementFactory.CreateCompositeElement(NodeType(child.Kind)));
                continue;
            }
            if (child.SourceRange.End < child.SourceRange.Start) continue;
            if (myLexer.TokenType == null || myLexer.TokenStart >= child.SourceRange.End) continue;

            var composite = TreeElementFactory.CreateCompositeElement(NodeType(child.Kind));
            parent.AppendNewChild(composite);
            AppendChildren(composite, child.Children, child.SourceRange.End);
            AppendTokensUntil(composite, child.SourceRange.End);
        }
        AppendTokensUntil(parent, parentEnd);
    }

    private void AppendTokensUntil(CompositeElement parent, int end)
    {
        while (myLexer.TokenType != null && myLexer.TokenStart < end)
            parent.AppendNewChild(CreateToken());
    }

    private static CompositeNodeType NodeType(MixinEditorSyntaxKind kind) => kind switch
    {
        MixinEditorSyntaxKind.Directive => ElementType.DIRECTIVE_NODE,
        MixinEditorSyntaxKind.DirectiveName => ElementType.DIRECTIVE_NAME_NODE,
        MixinEditorSyntaxKind.DirectiveArgument => ElementType.DIRECTIVE_ARGUMENT_NODE,
        MixinEditorSyntaxKind.Operand => ElementType.OPERAND_NODE,
        MixinEditorSyntaxKind.Reference => ElementType.REFERENCE_NODE,
        MixinEditorSyntaxKind.ParenthesizedReference => ElementType.PARENTHESIZED_REFERENCE_NODE,
        MixinEditorSyntaxKind.Root => ElementType.ROOT_NODE,
        MixinEditorSyntaxKind.Member => ElementType.MEMBER_NODE,
        MixinEditorSyntaxKind.Path => ElementType.PATH_NODE,
        MixinEditorSyntaxKind.FunctionCall => ElementType.FUNCTION_CALL_NODE,
        MixinEditorSyntaxKind.LiteralArgument => ElementType.LITERAL_ARGUMENT_NODE,
        MixinEditorSyntaxKind.ExpressionArgument => ElementType.EXPRESSION_ARGUMENT_NODE,
        MixinEditorSyntaxKind.Comment => ElementType.COMMENT_NODE,
        MixinEditorSyntaxKind.Continuation => ElementType.CONTINUATION_NODE,
        MixinEditorSyntaxKind.Escape => ElementType.ESCAPE_NODE,
        MixinEditorSyntaxKind.Error => ElementType.INVALID_NODE,
        _ => ElementType.INVALID_NODE
    };

    protected override TreeElement CreateToken()
    {
        var type = myLexer.TokenType;
        var element = type.Create(myLexer.Buffer, new TreeOffset(myLexer.TokenStart), new TreeOffset(myLexer.TokenEnd));
        myLexer.Advance();
        return element;
    }
}
#endif
