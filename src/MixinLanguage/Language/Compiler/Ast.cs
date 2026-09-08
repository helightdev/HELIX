using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;

namespace Mixins.Compiler;

public abstract class HixAst {
  private IReadOnlyList<HixAst> _children = [];
  private HixSourceRange _sourceRange;
  private HixAst _rangeOwner;

  protected HixAst(
    HixSyntaxKind kind = HixSyntaxKind.Statement,
    HixSourceRange sourceRange = default, IReadOnlyList<HixAst> children = null
  ) {
    Kind = kind;
    _sourceRange = sourceRange;
    Children = children;
  }

  public int Line => SourceRange.Line;
  public HixSourceRange SourceRange {
    get {
      if (!_sourceRange.IsEmpty) return _sourceRange;
      if (_rangeOwner is not null) return _rangeOwner.SourceRange;
      var composed = HixSourceRange.Compose(_children.Select(child => child.SourceRange));
      return composed;
    }
    internal set => _sourceRange = value;
  }
  public HixSyntaxKind Kind { get; internal set; }
  public virtual bool IsTrivia => false;
  public HixAst Parent { get; private set; }
  public IReadOnlyList<HixAst> Children {
    get => _children;
    internal set {
      _children = value ?? [];
      foreach (var child in _children)
        if (child is not null)
          child.Parent = this;
    }
  }
  public IReadOnlyList<HixToken> Tokens { get; internal set; } = [];
  public IEnumerable<HixAst> SemanticChildren => Children.Where(child => !child.IsTrivia);

  public CompilationUnitAst Program {
    get {
      for (HixAst current = this; current is not null; current = current.Parent)
        if (current is CompilationUnitAst program)
          return program;
      return null;
    }
  }

  protected void Adopt(params IEnumerable<HixAst>[] groups) {
    Children = groups.Where(group => group is not null).SelectMany(group => group)
      .Where(child => child is not null).ToArray();
  }

  protected void InheritRangeFrom(HixAst owner) {
    _rangeOwner = owner;
  }
}


public sealed class TriviaAst(HixSyntaxKind kind, HixSourceRange range) : HixAst(kind, range) {
  public override bool IsTrivia => true;
}
public sealed record HixParseDiagnostic(int Line, string Message);
