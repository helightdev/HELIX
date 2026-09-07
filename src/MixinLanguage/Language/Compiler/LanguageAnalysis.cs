using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler;

/// <summary>Definition and reference analysis over the canonical semantic AST.</summary>
public sealed class LanguageAnalysis {
  public sealed record Symbol(string Name, string Kind, MixinSourceRange Range, MixinSourceRange Scope,
    MixinAst Node);
  public sealed record Reference(string Name, string Kind, MixinSourceRange Range, MixinAst Node);

  public LanguageAnalysis(string source) {
    Program = AntlrSyntax.Parse(source ?? "");
    var declarations = new List<Symbol>();
    var references = new List<Reference>();
    Visit(Program, declarations, references);
    Declarations = declarations;
    References = references;
  }

  public CompilationUnitAst Program { get; }
  public IReadOnlyList<Symbol> Declarations { get; }
  public IReadOnlyList<Reference> References { get; }

  public Symbol Resolve(Reference reference, IEnumerable<LanguageAnalysis> analyses,
    out LanguageAnalysis owner) {
    owner = null;
    if (reference is null) return null;
    var candidates = (analyses ?? []).SelectMany(analysis => analysis.Declarations
      .Where(symbol => symbol.Name == reference.Name && Compatible(symbol.Kind, reference.Kind))
      .Select(symbol => (Analysis: analysis, Symbol: symbol))).ToArray();
    if (candidates.Length == 0) return null;

    // Prefer the innermost declaration visible at the reference, then declarations in this file,
    // followed by stable directory order supplied by the caller.
    var local = candidates.Where(candidate => ReferenceEquals(candidate.Analysis, this) &&
      Contains(candidate.Symbol.Scope, reference.Range)).OrderBy(candidate => candidate.Symbol.Scope.Length)
      .ThenByDescending(candidate => candidate.Symbol.Range.Start).FirstOrDefault();
    var selected = local.Symbol is not null ? local : candidates.FirstOrDefault(candidate =>
      ReferenceEquals(candidate.Analysis, this));
    if (selected.Symbol is null) selected = candidates[0];
    owner = selected.Analysis;
    return selected.Symbol;
  }

  public static MixinSourceRange Scope(MixinAst node) {
    for (var current = node; current is not null; current = current.Parent)
      if (current is FunctionDeclarationAst or MixinDeclarationAst or BlockStatementAst)
        return current.SourceRange;
    return node?.Program?.SourceRange ?? default;
  }

  private void Visit(MixinAst node, ICollection<Symbol> declarations,
    ICollection<Reference> references) {
    switch (node) {
      case MixinDeclarationAst mixin:
        declarations.Add(new Symbol(mixin.Name, "Mixin", IdentifierRange(mixin, mixin.Name),
          Scope(mixin.Parent), mixin));
        break;
      case FunctionDeclarationAst function:
        declarations.Add(new Symbol(function.Name, "Function", IdentifierRange(function, function.Name),
          Scope(function.Parent), function));
        break;
      case AssignmentStatementAst assignment:
        declarations.Add(new Symbol(assignment.Name, assignment.Storage switch {
          StorageSpace.Local => "Local", StorageSpace.Variable => "Variable",
          StorageSpace.Target => "TargetVariable", _ => "Carry"
        }, IdentifierRange(assignment, assignment.Name), Scope(assignment), assignment));
        break;
      case ControlFlowStatementAst {Operation: ControlFlowKind.Label} label:
        declarations.Add(new Symbol(label.Label, "Label", IdentifierRange(label, label.Label),
          Scope(label), label));
        break;
      case ControlFlowStatementAst {Operation: ControlFlowKind.Goto} jump:
        references.Add(new Reference(jump.Label, "Label", IdentifierRange(jump, jump.Label), jump));
        break;
      case MemberExpressionAst member when StorageReference(member) is { } storage:
        references.Add(new Reference(member.Member, storage, TrailingIdentifierRange(member, member.Member), member));
        break;
      case CallExpressionAst call:
        AddCallReferences(call, references);
        break;
      case RootExpressionAst root when IsFunctionReference(root.Name):
        references.Add(new Reference(root.Name, "Function", IdentifierRange(root, root.Name), root));
        break;
    }
    foreach (var child in node.Children) Visit(child, declarations, references);
  }

  private void AddCallReferences(CallExpressionAst call, ICollection<Reference> references) {
    if (!FunctionLibrary.TryGet(call.Name, call.Arguments.Count, out var definition))
      references.Add(new Reference(call.Name, "Function", TrailingIdentifierRange(call, call.Name), call));
    else foreach (var index in definition.CSharpTypeArguments) {
      if (index < 0 || index >= call.Arguments.Count || call.Arguments[index] is not StringExpressionAst text) continue;
      var range = StringContentRange(call.Arguments[index]);
      references.Add(new Reference(text.Value, "CSharpType", range, call.Arguments[index]));
    }
  }

  private static string StorageReference(MemberExpressionAst member) {
    if (member.Receiver is not RootExpressionAst root) return null;
    return root.Name switch {
      "local" => "Local", "var" => "Variable", "tar" => "TargetVariable", "carry" => "Carry", _ => null
    };
  }

  private static bool IsFunctionReference(string name) => name is not
    ("this" or "target" or "attr" or "local" or "var" or "tar" or "carry" or
     "param" or "true" or "false" or "null" or "string" or "bool" or "number" or
     "tuple" or "table" or "symbol" or "function" or "error" or "kind");

  private MixinSourceRange IdentifierRange(MixinAst node, string name) {
    var token = node.Tokens.FirstOrDefault(item => item.Kind == MixinTokenKind.Identifier && item.Text == name);
    return token?.SourceRange ?? TextRange(node.SourceRange, name, false);
  }

  private MixinSourceRange TrailingIdentifierRange(MixinAst node, string name) {
    var token = node.Tokens.LastOrDefault(item => item.Kind == MixinTokenKind.Identifier && item.Text == name);
    return token?.SourceRange ?? TextRange(node.SourceRange, name, true);
  }

  private MixinSourceRange TextRange(MixinSourceRange within, string text, bool last) {
    var source = Program.Source;
    var start = last
      ? source.LastIndexOf(text, Math.Max(within.Start, within.End - 1), within.Length, StringComparison.Ordinal)
      : source.IndexOf(text, within.Start, within.Length, StringComparison.Ordinal);
    return start < 0 ? within : new MixinSourceRange(start, start + text.Length, within.Line, within.Column);
  }

  private static MixinSourceRange StringContentRange(ExpressionAst expression) {
    var range = expression.SourceRange;
    return range.Length >= 2 ? range with {Start = range.Start + 1, End = range.End - 1} : range;
  }

  private static bool Compatible(string declaration, string reference) => declaration == reference;
  private static bool Contains(MixinSourceRange scope, MixinSourceRange range) =>
    scope.Start <= range.Start && scope.End >= range.End;
}
