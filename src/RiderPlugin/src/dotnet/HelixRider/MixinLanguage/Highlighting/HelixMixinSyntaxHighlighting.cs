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

        var attributeId = GetMixinAttributeId(tokenNode);
        if (attributeId != null)
            context.AddHighlighting(new ReSharperSyntaxHighlighting(attributeId, null, range));
    }

    internal static string GetMixinAttributeId(ITokenNode token)
    {
        var text = token.GetText();
        for (var parent = token.Parent; parent != null; parent = parent.Parent)
        {
            if (parent is ICommentNode) return HelixMixinHighlightingAttributeIds.Comment;
            if (parent is IDirectiveNameNode) return HelixMixinHighlightingAttributeIds.Directive;
            if (parent is ILiteralArgumentNode)
                return IsBoundaryDelimiter(token, parent, text)
                    ? HelixMixinHighlightingAttributeIds.Function
                    : HelixMixinHighlightingAttributeIds.Argument;
            if (parent is IExpressionArgumentNode)
                return IsBoundaryDelimiter(token, parent, text)
                    ? HelixMixinHighlightingAttributeIds.Function
                    : IsExpressionBoundaryParenthesis(token, parent, text)
                        ? HelixMixinHighlightingAttributeIds.Value
                        : null;
            if (parent is IDirectiveArgumentNode)
                return IsBoundaryDelimiter(token, parent, text)
                    ? HelixMixinHighlightingAttributeIds.Directive
                    : HelixMixinHighlightingAttributeIds.Argument;
            if (parent is IPathNode) return HelixMixinHighlightingAttributeIds.Path;
            if (parent is IFunctionCallNode)
                return text is ":" or "!" or "?"
                    ? HelixMixinHighlightingAttributeIds.Operator
                    : HelixMixinHighlightingAttributeIds.Function;
            if (parent is IRootNode) return HelixMixinHighlightingAttributeIds.Value;
            if (parent is IMemberNode) return HelixMixinHighlightingAttributeIds.Path;
            if (parent is IParenthesizedReferenceNode)
                return text == "#"
                    ? HelixMixinHighlightingAttributeIds.Path
                    : text is "@" or "(" or ")" ? HelixMixinHighlightingAttributeIds.Value : null;
            if (parent is IReferenceNode)
                return text == "#"
                    ? HelixMixinHighlightingAttributeIds.Path
                    : text == "@" ? HelixMixinHighlightingAttributeIds.Value : null;
            if (parent is IContinuationNode)
            {
                if (text is "@" or "+" or "\\") return HelixMixinHighlightingAttributeIds.Operator;
                continue;
            }
            if (parent is IEscapeNode) return HelixMixinHighlightingAttributeIds.Escape;
            if (parent is IInvalidNode) return null;
        }
        return null;
    }

    private static bool IsBoundaryDelimiter(ITokenNode token, ITreeNode parent, string text)
    {
        if (text is not ("<" or ">")) return false;
        var tokenRange = token.GetTreeTextRange();
        var parentRange = parent.GetTreeTextRange();
        return text == "<"
            ? tokenRange.StartOffset == parentRange.StartOffset
            : tokenRange.EndOffset == parentRange.EndOffset;
    }

    private static bool IsExpressionBoundaryParenthesis(ITokenNode token, ITreeNode parent, string text)
    {
        if (text is not ("(" or ")")) return false;
        var tokenRange = token.GetTreeTextRange();
        var parentRange = parent.GetTreeTextRange();
        return text == "("
            ? tokenRange.StartOffset.Offset == parentRange.StartOffset.Offset + 1
            : tokenRange.EndOffset.Offset == parentRange.EndOffset.Offset - 1;
    }
}
#endif
