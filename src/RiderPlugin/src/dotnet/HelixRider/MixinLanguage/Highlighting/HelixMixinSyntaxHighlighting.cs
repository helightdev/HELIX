#if RIDER
using HelixRider.MixinLanguage.Parsing;
using JetBrains.ReSharper.Daemon.Syntax;
using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;

namespace HelixRider.MixinLanguage.Highlighting;

[Language(typeof(HelixMixinLanguage))]
internal sealed class HelixMixinSyntaxHighlightingManager : SyntaxHighlightingManager
{
    public override SyntaxHighlightingProcessor CreateProcessor(IPsiSourceFile sourceFile, IFile psiFile) =>
        new HelixMixinSyntaxHighlightingProcessor();
}

internal sealed class HelixMixinSyntaxHighlightingProcessor : SyntaxHighlightingProcessor
{
    public override void ProcessBeforeInterior(ITreeNode element, IHighlightingConsumer context)
    {
        if (element is not ITokenNode tokenNode) return;
        var tokenType = tokenNode.GetTokenType();
        if (tokenType.IsWhitespace) return;
        var range = tokenNode.GetDocumentRange();
        if (range.TextRange.IsEmpty) return;

        var attributeId = GetMixinAttributeId(tokenType);
        if (attributeId != null)
            context.AddHighlighting(new ReSharperSyntaxHighlighting(attributeId, null, range));
    }

    private static string GetMixinAttributeId(TokenNodeType tokenType)
    {
        if (tokenType == HelixMixinTokenNodeTypes.Directive) return HelixMixinHighlightingAttributeIds.Directive;
        if (tokenType == HelixMixinTokenNodeTypes.Value) return HelixMixinHighlightingAttributeIds.Value;
        if (tokenType == HelixMixinTokenNodeTypes.EnclosedReferenceParenthesis) return HelixMixinHighlightingAttributeIds.Value;
        if (tokenType == HelixMixinTokenNodeTypes.Path) return HelixMixinHighlightingAttributeIds.Path;
        if (tokenType == HelixMixinTokenNodeTypes.Function) return HelixMixinHighlightingAttributeIds.Function;
        if (tokenType == HelixMixinTokenNodeTypes.DirectiveArgumentDelimiter) return HelixMixinHighlightingAttributeIds.Directive;
        if (tokenType == HelixMixinTokenNodeTypes.ExpressionArgumentDelimiter) return HelixMixinHighlightingAttributeIds.Function;
        if (tokenType == HelixMixinTokenNodeTypes.Argument) return HelixMixinHighlightingAttributeIds.Argument;
        if (tokenType == HelixMixinTokenNodeTypes.ArgumentDelimiter ||
            tokenType == HelixMixinTokenNodeTypes.Parenthesis) return HelixMixinHighlightingAttributeIds.Parenthesis;
        if (tokenType == HelixMixinTokenNodeTypes.Operator ||
            tokenType == HelixMixinTokenNodeTypes.Continuation) return HelixMixinHighlightingAttributeIds.Operator;
        if (tokenType == HelixMixinTokenNodeTypes.Escape) return HelixMixinHighlightingAttributeIds.Escape;
        if (tokenType == HelixMixinTokenNodeTypes.Comment) return HelixMixinHighlightingAttributeIds.Comment;
        return null;
    }
}
#endif
