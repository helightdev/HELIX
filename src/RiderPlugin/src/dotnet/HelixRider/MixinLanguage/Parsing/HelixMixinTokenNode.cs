#if RIDER
using System.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Text;

namespace HelixRider.MixinLanguage.Parsing;

internal sealed class HelixMixinTokenNode : LeafElementBase, ITokenNode, IHelixMixinTreeNode
{
    private readonly TokenNodeType _type;
    private readonly string _text;

    internal HelixMixinTokenNode(TokenNodeType type, string text) { _type = type; _text = text; }
    public override PsiLanguageType Language => LanguageFromParent;
    public override NodeType NodeType => _type;
    public override int GetTextLength() => _text.Length;
    public override string GetText() => _text;
    public override StringBuilder GetText(StringBuilder to) { to.Append(_text); return to; }
    public override IBuffer GetTextAsBuffer() => new StringBuffer(_text);
    public TokenNodeType GetTokenType() => _type;
    public void Accept(TreeNodeVisitor visitor) => visitor.VisitNode(this);
    public void Accept<TContext>(TreeNodeVisitor<TContext> visitor, TContext context) => visitor.VisitNode(this, context);
    public TReturn Accept<TContext, TReturn>(TreeNodeVisitor<TContext, TReturn> visitor, TContext context) =>
        visitor.VisitNode(this, context);
}
#endif
