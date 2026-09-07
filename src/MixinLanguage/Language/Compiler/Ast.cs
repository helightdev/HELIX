using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;

namespace Mixins.Compiler;

public abstract class MixinAst {
  private IReadOnlyList<MixinAst> _children = [];
  private MixinSourceRange _sourceRange;
  private MixinAst _rangeOwner;

  protected MixinAst(
    MixinSyntaxKind kind = MixinSyntaxKind.Statement,
    MixinSourceRange sourceRange = default, IReadOnlyList<MixinAst> children = null
  ) {
    Kind = kind;
    _sourceRange = sourceRange;
    Children = children;
  }

  public int Line => SourceRange.Line;
  public MixinSourceRange SourceRange {
    get {
      if (!_sourceRange.IsEmpty) return _sourceRange;
      if (_rangeOwner is not null) return _rangeOwner.SourceRange;
      var composed = MixinSourceRange.Compose(_children.Select(child => child.SourceRange));
      return composed;
    }
    internal set => _sourceRange = value;
  }
  public MixinSyntaxKind Kind { get; internal set; }
  public virtual bool IsTrivia => false;
  public MixinAst Parent { get; private set; }
  public IReadOnlyList<MixinAst> Children {
    get => _children;
    internal set {
      _children = value ?? [];
      foreach (var child in _children)
        if (child is not null)
          child.Parent = this;
    }
  }
  public IReadOnlyList<MixinToken> Tokens { get; internal set; } = [];
  public IEnumerable<MixinAst> SemanticChildren => Children.Where(child => !child.IsTrivia);

  public CompilationUnitAst Program {
    get {
      for (MixinAst current = this; current is not null; current = current.Parent)
        if (current is CompilationUnitAst program)
          return program;
      return null;
    }
  }

  protected void Adopt(params IEnumerable<MixinAst>[] groups) {
    Children = groups.Where(group => group is not null).SelectMany(group => group)
      .Where(child => child is not null).ToArray();
  }

  protected void InheritRangeFrom(MixinAst owner) {
    _rangeOwner = owner;
  }
}


public sealed class TriviaAst(MixinSyntaxKind kind, MixinSourceRange range) : MixinAst(kind, range) {
  public override bool IsTrivia => true;
}
public sealed record MixinParseDiagnostic(int Line, string Message);
