#if RIDER
using System;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace HelixRider.MixinLanguage;

public static class ChildRole
{
    public const short NONE = 0;
    public const short LAST = 100;
}

public interface IHelixMixinTreeNode : ITreeNode
{
    void Accept(TreeNodeVisitor visitor);
    void Accept<TContext>(TreeNodeVisitor<TContext> visitor, TContext context);
    TReturn Accept<TContext, TReturn>(TreeNodeVisitor<TContext, TReturn> visitor, TContext context);
}

public partial interface IHelixMixinFile : IFile { }

public abstract class HelixMixinCompositeNodeType : CompositeNodeType
{
    protected HelixMixinCompositeNodeType(string name, int index, Type nodeType) : base(name, index, nodeType) { }
}

public abstract class HelixMixinCompositeElement : CompositeElement, IHelixMixinTreeNode
{
    public override PsiLanguageType Language => HelixMixinLanguage.Instance;
    public virtual void Accept(TreeNodeVisitor visitor) => visitor.VisitNode(this);
    public virtual void Accept<TContext>(TreeNodeVisitor<TContext> visitor, TContext context) => visitor.VisitNode(this, context);
    public virtual TReturn Accept<TContext, TReturn>(TreeNodeVisitor<TContext, TReturn> visitor, TContext context) =>
        visitor.VisitNode(this, context);
}

public abstract class HelixMixinFileElement : FileElementBase, IHelixMixinTreeNode
{
    public override PsiLanguageType Language => HelixMixinLanguage.Instance;
    public abstract void Accept(TreeNodeVisitor visitor);
    public abstract void Accept<TContext>(TreeNodeVisitor<TContext> visitor, TContext context);
    public abstract TReturn Accept<TContext, TReturn>(TreeNodeVisitor<TContext, TReturn> visitor, TContext context);
}
#endif
