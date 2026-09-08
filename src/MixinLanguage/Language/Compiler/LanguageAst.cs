using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler;

/// <summary>Semantic syntax shared by compilation and editor consumers; punctuation stays in Tokens.</summary>
public abstract class LanguageAst : HixAst {
  protected LanguageAst(IEnumerable<HixAst> children = null) {
    Children = children?.ToArray() ?? [];
  }
}

public sealed class CompilationUnitAst : LanguageAst {
  internal CompilationUnitAst(string source, IReadOnlyList<LanguageAst> declarations,
    IReadOnlyList<HixParseDiagnostic> diagnostics, IReadOnlyList<HixToken> tokens) : base(declarations) {
    Source = source;
    Declarations = declarations;
    Diagnostics = diagnostics;
    Tokens = tokens;
    Kind = HixSyntaxKind.Document;
    SourceRange = new HixSourceRange(0, source.Length, 1, 0);
  }
  public string Source { get; }
  public IReadOnlyList<LanguageAst> Declarations { get; }
  public IReadOnlyList<HixParseDiagnostic> Diagnostics { get; }
}

public sealed class MixinDeclarationAst(string name, bool derivation, IReadOnlyList<LanguageAst> declarations)
  : LanguageAst(declarations) {
  public string Name { get; } = name;
  public bool IsDerivation { get; } = derivation;
  public IReadOnlyList<LanguageAst> Declarations { get; } = declarations;
}

public sealed class ExpressionDeclarationAst(bool prelude, bool strict, BlockStatementAst body)
  : LanguageAst([body]) {
  public bool IsPrelude { get; } = prelude;
  public bool IsStrict { get; } = strict;
  public BlockStatementAst Body { get; } = body;
}

public sealed record SignatureField(string Name, string Kind, bool Variadic);
public sealed record FunctionSignature(string InputKind, IReadOnlyList<SignatureField> Inputs,
  string OutputKind, IReadOnlyList<SignatureField> Outputs);

public sealed class FunctionDeclarationAst(string name, bool pure, bool inline, bool noinline,
  IReadOnlyList<FunctionSignature> signatures, BlockStatementAst body) : LanguageAst([body]) {
  public string Name { get; } = name;
  public bool IsPure { get; } = pure;
  public bool IsInline { get; } = inline;
  public bool IsNoinline { get; } = noinline;
  public IReadOnlyList<FunctionSignature> Signatures { get; } = signatures;
  public BlockStatementAst Body { get; } = body;
}

public abstract class StatementAst(IEnumerable<HixAst> children = null) : LanguageAst(children);

public sealed class BlockStatementAst(IReadOnlyList<StatementAst> statements, string label = null)
  : StatementAst(statements) {
  public IReadOnlyList<StatementAst> Statements { get; } = statements;
  public string Label { get; } = label;
}

public enum StorageSpace { Local, Variable, Target, Carry }

public sealed class AssignmentStatementAst(StorageSpace storage, string name, ExpressionAst value)
  : StatementAst([value]) {
  public StorageSpace Storage { get; } = storage;
  public string Name { get; } = name;
  public ExpressionAst Value { get; } = value;
}

public sealed class InvocationStatementAst(CallExpressionAst call) : StatementAst([call]) {
  public CallExpressionAst Call { get; } = call;
}

public enum ControlFlowKind { Return, Goto, Break, Continue, Label }

public sealed class ControlFlowStatementAst(ControlFlowKind operation, string label,
  IReadOnlyList<ExpressionAst> values) : StatementAst(values) {
  public ControlFlowKind Operation { get; } = operation;
  public string Label { get; } = label;
  public IReadOnlyList<ExpressionAst> Values { get; } = values;
}

public sealed class SelectionStatementAst(SelectionExpressionAst selection) : StatementAst([selection]) {
  public SelectionExpressionAst Selection { get; } = selection;
}

public abstract class ExpressionAst(IEnumerable<HixAst> children = null) : LanguageAst(children);

public sealed class StringExpressionAst(string value) : ExpressionAst {
  public string Value { get; } = value;
}

public sealed class RootExpressionAst(string name, bool smart = false) : ExpressionAst {
  public string Name { get; } = name;
  public bool IsSmart { get; } = smart;
}

public sealed class MemberExpressionAst(ExpressionAst receiver, string member) : ExpressionAst([receiver]) {
  public ExpressionAst Receiver { get; } = receiver;
  public string Member { get; } = member;
}

public sealed class CallExpressionAst(string name, IReadOnlyList<ExpressionAst> arguments,
  bool coerceBoolean = false) : ExpressionAst(arguments) {
  public string Name { get; } = name;
  public IReadOnlyList<ExpressionAst> Arguments { get; } = arguments;
  public bool CoerceBoolean { get; } = coerceBoolean;
}

public enum UnaryOperation { Not, Check }
public sealed class UnaryExpressionAst(UnaryOperation operation, ExpressionAst value) : ExpressionAst([value]) {
  public UnaryOperation Operation { get; } = operation;
  public ExpressionAst Value { get; } = value;
}

public sealed class FallbackExpressionAst(ExpressionAst value, ExpressionAst fallback)
  : ExpressionAst([value, fallback]) {
  public ExpressionAst Value { get; } = value;
  public ExpressionAst Fallback { get; } = fallback;
}

public sealed class TupleExpressionAst(IReadOnlyList<ExpressionAst> values) : ExpressionAst(values) {
  public IReadOnlyList<ExpressionAst> Values { get; } = values;
}

public sealed class TableExpressionAst(IReadOnlyList<KeyValuePair<string, ExpressionAst>> entries)
  : ExpressionAst(entries.Select(entry => entry.Value)) {
  public IReadOnlyList<KeyValuePair<string, ExpressionAst>> Entries { get; } = entries;
}

public sealed class InterpolationExpressionAst(IReadOnlyList<ExpressionAst> parts) : ExpressionAst(parts) {
  public IReadOnlyList<ExpressionAst> Parts { get; } = parts;
}

public sealed class SelectionBranchAst(IReadOnlyList<ExpressionAst> conditions, LanguageAst result,
  bool transformation = false) : LanguageAst(conditions.Cast<HixAst>().Concat([result])) {
  public IReadOnlyList<ExpressionAst> Conditions { get; } = conditions;
  public LanguageAst Result { get; } = result;
  public bool IsTransformation { get; } = transformation;
}

public sealed class SelectionExpressionAst(ExpressionAst selector, IReadOnlyList<SelectionBranchAst> branches,
  LanguageAst fallback) : ExpressionAst(
    (selector is null ? Enumerable.Empty<HixAst>() : [selector]).Concat(branches)
      .Concat(fallback is null ? [] : [fallback])) {
  public ExpressionAst Selector { get; } = selector;
  public IReadOnlyList<SelectionBranchAst> Branches { get; } = branches;
  public LanguageAst Fallback { get; } = fallback;
}
