using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Runtime;

namespace Hix.Compiler;

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
    set => _sourceRange = value;
  }
  public HixSyntaxKind Kind { get; set; }
  public virtual bool IsTrivia => false;
  public HixAst Parent { get; private set; }
  public IReadOnlyList<HixAst> Children {
    get => _children;
    set {
      _children = value ?? [];
      foreach (var child in _children)
        if (child is not null)
          child.Parent = this;
    }
  }
  public IReadOnlyList<HixToken> Tokens { get; set; } = [];
  public IEnumerable<HixAst> SemanticChildren => Children.Where(child => !child.IsTrivia && child is not MetadataAst);

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

public sealed class MetadataAst(string name, IReadOnlyList<ExpressionAst> values) : HixAst(children: values) {
  public string Name { get; } = name;
  public IReadOnlyList<ExpressionAst> Values { get; } = values;
}

public sealed record HixParseDiagnostic(int Line, string Message);

public sealed class CompilationUnitAst : HixAst {
  public CompilationUnitAst(
    string source, IReadOnlyList<HixAst> declarations,
    IReadOnlyList<HixParseDiagnostic> diagnostics, IReadOnlyList<HixToken> tokens,
    IReadOnlyList<MetadataAst> metadata = null
  ) : base(children: (metadata ?? []).Cast<HixAst>().Concat(declarations).ToArray()) {
    Source = source;
    Declarations = declarations;
    Diagnostics = diagnostics;
    Metadata = metadata ?? [];
    Tokens = tokens;
    Kind = HixSyntaxKind.Document;
    SourceRange = new HixSourceRange(0, source.Length, 1, 0);
  }

  public string Source { get; }
  public IReadOnlyList<HixAst> Declarations { get; }
  public IReadOnlyList<HixParseDiagnostic> Diagnostics { get; }
  public IReadOnlyList<MetadataAst> Metadata { get; }
}

public sealed class MixinDeclarationAst(string name, bool derivation, IReadOnlyList<HixAst> declarations,
  IReadOnlyList<MetadataAst> metadata = null)
  : HixAst(children: (metadata ?? []).Cast<HixAst>().Concat(declarations).ToArray()) {
  public string Name { get; } = name;
  public bool IsDerivation { get; } = derivation;
  public IReadOnlyList<HixAst> Declarations { get; } = declarations;
  public IReadOnlyList<MetadataAst> Metadata { get; } = metadata ?? [];
}

public sealed class TypeDeclarationAst(string name, HixPattern pattern,
  IReadOnlyList<MetadataAst> metadata = null) : HixAst(children: metadata?.Cast<HixAst>().ToArray()) {
  public string Name { get; } = name;
  public HixPattern Pattern { get; } = pattern ?? HixPattern.Any;
  public IReadOnlyList<MetadataAst> Metadata { get; } = metadata ?? [];
}

public sealed class ExpressionDeclarationAst(bool prelude, bool strict, BlockStatementAst body)
  : HixAst(children: [body]) {
  public bool IsPrelude { get; } = prelude;
  public bool IsStrict { get; } = strict;
  public BlockStatementAst Body { get; } = body;
}

public sealed record SignatureField {
  public SignatureField(string name, HixPattern pattern, bool variadic = false,
    IReadOnlyList<MetadataAst> metadata = null, bool optional = false) {
    Name = name; Pattern = pattern ?? HixPattern.Any; Variadic = variadic; Metadata = metadata ?? []; Optional = optional;
  }
  public SignatureField(string name, string kind, bool variadic,
    IReadOnlyList<MetadataAst> metadata = null) : this(name, HixPatterns.Named(kind), variadic, metadata) { }
  public string Name { get; }
  public HixPattern Pattern { get; }
  public string Kind => Pattern.Display;
  public bool Variadic { get; }
  public bool Optional { get; }
  public IReadOnlyList<MetadataAst> Metadata { get; }
  public HixPatternField AsPatternField() => new(Name, Pattern, Optional);
}

public sealed record FunctionSignature {
  public FunctionSignature(HixPattern inputPattern, IReadOnlyList<SignatureField> inputs,
    HixPattern outputPattern, IReadOnlyList<SignatureField> outputs, bool patternSyntax = true) {
    InputPattern = inputPattern; Inputs = inputs; OutputPattern = outputPattern; Outputs = outputs;
    IsPatternSyntax = patternSyntax;
  }
  public FunctionSignature(string inputKind, IReadOnlyList<SignatureField> inputs,
    string outputKind, IReadOnlyList<SignatureField> outputs) : this(
      inputs == null ? HixPatterns.Named(inputKind) : null, inputs,
      outputs == null ? HixPatterns.Named(outputKind) : null, outputs, false) { }
  public HixPattern InputPattern { get; }
  public IReadOnlyList<SignatureField> Inputs { get; }
  public HixPattern OutputPattern { get; }
  public IReadOnlyList<SignatureField> Outputs { get; }
  public bool IsPatternSyntax { get; }
  public string InputKind => InputPattern?.Display;
  public string OutputKind => OutputPattern?.Display;
  public SignatureHixPattern Constant(string name) => new(name,
    Inputs == null ? [new HixPatternField(null, InputPattern ?? HixPattern.Any)] :
      Inputs.Select(field => field.AsPatternField()).ToArray(),
    Outputs == null ? OutputPattern ?? HixPattern.Any : new TableHixPattern(Outputs.Select(field => field.AsPatternField()).ToArray()));
}

public sealed class FunctionDeclarationAst(string name, bool pure, bool inline, bool noinline,
  IReadOnlyList<FunctionSignature> signatures, BlockStatementAst body, IReadOnlyList<MetadataAst> metadata = null
) : HixAst(children: (metadata ?? []).Cast<HixAst>().Concat([body]).ToArray()) {
  public string Name { get; } = name;
  public bool IsPure { get; } = pure;
  public bool IsInline { get; } = inline;
  public bool IsNoinline { get; } = noinline;
  public IReadOnlyList<FunctionSignature> Signatures { get; } = signatures;
  public BlockStatementAst Body { get; } = body;
  public IReadOnlyList<MetadataAst> Metadata { get; } = metadata ?? [];
}

public abstract class StatementAst(IEnumerable<HixAst> children = null) : HixAst(children: children?.ToArray());

public sealed class BlockStatementAst(IReadOnlyList<StatementAst> statements, string label = null)
  : StatementAst(statements) {
  public IReadOnlyList<StatementAst> Statements { get; } = statements;
  public string Label { get; } = label;
}

public enum StorageSpace { Local, Variable, Target }

public sealed class AssignmentStatementAst(StorageSpace storage, string name, ExpressionAst value, bool carried = false)
  : StatementAst([value]) {
  public StorageSpace Storage { get; } = storage;
  public string Name { get; } = name;
  public ExpressionAst Value { get; } = value;
  public bool IsCarried { get; } = carried;
}

public sealed class InvocationStatementAst(CallExpressionAst call) : StatementAst([call]) {
  public CallExpressionAst Call { get; } = call;
}

public enum ControlFlowKind { Return, Goto, Break, Continue, Label }

public sealed class ControlFlowStatementAst(ControlFlowKind operation, string label,
  IReadOnlyList<ExpressionAst> values
) : StatementAst(values) {
  public ControlFlowKind Operation { get; } = operation;
  public string Label { get; } = label;
  public IReadOnlyList<ExpressionAst> Values { get; } = values;
}

public sealed class SelectionStatementAst(SelectionExpressionAst selection) : StatementAst([selection]) {
  public SelectionExpressionAst Selection { get; } = selection;
}

public abstract class ExpressionAst(IEnumerable<HixAst> children = null) : HixAst(children: children?.ToArray());

public sealed class StringExpressionAst(string value) : ExpressionAst {
  public string Value { get; } = value;
}

public sealed class NumberExpressionAst(double value) : ExpressionAst {
  public double Value { get; } = value;
}

public sealed class BooleanExpressionAst(bool value) : ExpressionAst {
  public bool Value { get; } = value;
}

public sealed class NullExpressionAst : ExpressionAst;

/// <summary>The current selection value used by a when transformation condition.</summary>
public sealed class SelectorExpressionAst : ExpressionAst;

public sealed class RootExpressionAst(string name, bool smart = false) : ExpressionAst {
  public string Name { get; } = name;
  public bool IsSmart { get; } = smart;
}

public sealed class MemberExpressionAst(ExpressionAst receiver, string member) : ExpressionAst([receiver]) {
  public ExpressionAst Receiver { get; } = receiver;
  public string Member { get; } = member;
}

public sealed class CallExpressionAst(string name, IReadOnlyList<ExpressionAst> arguments,
  bool coerceBoolean = false, SignatureHixPattern signature = null
) : ExpressionAst(arguments) {
  public string Name { get; } = name;
  public IReadOnlyList<ExpressionAst> Arguments { get; } = arguments;
  public bool CoerceBoolean { get; } = coerceBoolean;
  public SignatureHixPattern Signature { get; } = signature;
}

public sealed class InlineExpressionAst(BlockStatementAst body, string resultLocal) : ExpressionAst([body]) {
  public BlockStatementAst Body { get; } = body;
  public string ResultLocal { get; } = resultLocal;
}

public sealed class LambdaExpressionAst(BlockStatementAst body) : ExpressionAst([body]) {
  public BlockStatementAst Body { get; } = body;
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

public sealed class TableExpressionAst(IReadOnlyList<KeyValuePair<string, ExpressionAst>> entries,
  IReadOnlyList<KeyValuePair<string, IReadOnlyList<MetadataAst>>> fieldMetadata = null)
  : ExpressionAst((fieldMetadata?.SelectMany(value => value.Value).Cast<HixAst>() ?? [])
    .Concat(entries.Select(entry => entry.Value)).ToArray()) {
  public IReadOnlyList<KeyValuePair<string, ExpressionAst>> Entries { get; } = entries;
  public IReadOnlyList<KeyValuePair<string, IReadOnlyList<MetadataAst>>> FieldMetadata { get; } = fieldMetadata ?? [];
}

public sealed class InterpolationExpressionAst(IReadOnlyList<ExpressionAst> parts) : ExpressionAst(parts) {
  public IReadOnlyList<ExpressionAst> Parts { get; } = parts;
}

public sealed class SelectionBranchAst(IReadOnlyList<ExpressionAst> conditions, HixAst result,
  bool transformation = false
) : HixAst(children: conditions.Concat([result]).ToArray()) {
  public IReadOnlyList<ExpressionAst> Conditions { get; } = conditions;
  public HixAst Result { get; } = result;
  public bool IsTransformation { get; } = transformation;
}

public sealed class SelectionExpressionAst(ExpressionAst selector, IReadOnlyList<SelectionBranchAst> branches,
  HixAst fallback
) : ExpressionAst(
  (selector is null ? Enumerable.Empty<HixAst>() : [selector])
  .Concat(branches)
  .Concat(fallback is null ? [] : [fallback])
) {
  public ExpressionAst Selector { get; } = selector;
  public IReadOnlyList<SelectionBranchAst> Branches { get; } = branches;
  public HixAst Fallback { get; } = fallback;
}
