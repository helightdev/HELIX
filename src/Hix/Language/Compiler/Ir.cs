using System;
using Antlr4.Runtime;
using System.Collections.Generic;
using System.Linq;
using Hix.Runtime;

namespace Hix.Compiler;

public abstract class HixIrNode {
  private IReadOnlyList<HixIrNode> _children = [];
  private HixSourceRange _sourceRange;
  private HixIrNode _rangeOwner;

  protected HixIrNode(
    HixSourceRange sourceRange = default, IReadOnlyList<HixIrNode> children = null
  ) {
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
  public HixIrNode Parent { get; private set; }
  public IReadOnlyList<HixIrNode> Children {
    get => _children;
    set {
      _children = value ?? [];
      foreach (var child in _children)
        if (child is not null)
          child.Parent = this;
    }
  }
  public IReadOnlyList<IToken> Tokens { get; set; } = [];
  public IEnumerable<HixIrNode> SemanticChildren => Children.Where(child => child is not MetadataIr);

  public CompilationUnitIr Program {
    get {
      for (HixIrNode current = this; current is not null; current = current.Parent)
        if (current is CompilationUnitIr program)
          return program;
      return null;
    }
  }

  protected void Adopt(params IEnumerable<HixIrNode>[] groups) {
    Children = groups.Where(group => group is not null).SelectMany(group => group)
      .Where(child => child is not null).ToArray();
  }

  protected void InheritRangeFrom(HixIrNode owner) {
    _rangeOwner = owner;
  }
}

public sealed class MetadataIr(string name, IReadOnlyList<ExpressionIr> values) : HixIrNode(children: values) {
  public string Name { get; } = name;
  public IReadOnlyList<ExpressionIr> Values { get; } = values;
}

public sealed record HixParseDiagnostic(int Line, string Message);

public sealed class CompilationUnitIr : HixIrNode {
  public CompilationUnitIr(
    string source, IReadOnlyList<HixIrNode> declarations,
    IReadOnlyList<HixParseDiagnostic> diagnostics, IReadOnlyList<IToken> tokens,
    IReadOnlyList<MetadataIr> metadata = null
  ) : base(children: (metadata ?? []).Cast<HixIrNode>().Concat(declarations).ToArray()) {
    Source = source;
    Declarations = declarations;
    Diagnostics = diagnostics;
    Metadata = metadata ?? [];
    Tokens = tokens;
    SourceRange = new HixSourceRange(0, source.Length, 1, 0);
  }

  public string Source { get; }
  public IReadOnlyList<HixIrNode> Declarations { get; }
  public IReadOnlyList<HixParseDiagnostic> Diagnostics { get; }
  public IReadOnlyList<MetadataIr> Metadata { get; }
}

public sealed class MixinDeclarationIr(string name, bool derivation, IReadOnlyList<HixIrNode> declarations,
  IReadOnlyList<MetadataIr> metadata = null)
  : HixIrNode(children: (metadata ?? []).Cast<HixIrNode>().Concat(declarations).ToArray()) {
  public string Name { get; } = name;
  public bool IsDerivation { get; } = derivation;
  public IReadOnlyList<HixIrNode> Declarations { get; } = declarations;
  public IReadOnlyList<MetadataIr> Metadata { get; } = metadata ?? [];
}

public sealed class TypeDeclarationIr(string name, HixPattern pattern,
  IReadOnlyList<MetadataIr> metadata = null) : HixIrNode(children: metadata?.Cast<HixIrNode>().ToArray()) {
  public string Name { get; } = name;
  public HixPattern Pattern { get; } = pattern ?? HixPattern.Any;
  public IReadOnlyList<MetadataIr> Metadata { get; } = metadata ?? [];
}

public sealed class ExpressionDeclarationIr(bool prelude, bool strict, BlockStatementIr body)
  : HixIrNode(children: [body]) {
  public bool IsPrelude { get; } = prelude;
  public bool IsStrict { get; } = strict;
  public BlockStatementIr Body { get; } = body;
}

public sealed record SignatureField {
  public SignatureField(string name, HixPattern pattern, bool variadic = false,
    IReadOnlyList<MetadataIr> metadata = null, bool optional = false) {
    Name = name; Pattern = pattern ?? HixPattern.Any; Variadic = variadic; Metadata = metadata ?? []; Optional = optional;
  }
  public SignatureField(string name, string kind, bool variadic,
    IReadOnlyList<MetadataIr> metadata = null) : this(name, HixPatterns.Named(kind), variadic, metadata) { }
  public string Name { get; }
  public HixPattern Pattern { get; }
  public string Kind => Pattern.Display;
  public bool Variadic { get; }
  public bool Optional { get; }
  public IReadOnlyList<MetadataIr> Metadata { get; }
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

public sealed class FunctionDeclarationIr(string name, bool pure, bool inline, bool noinline,
  IReadOnlyList<FunctionSignature> signatures, BlockStatementIr body, IReadOnlyList<MetadataIr> metadata = null
) : HixIrNode(children: (metadata ?? []).Cast<HixIrNode>().Concat([body]).ToArray()) {
  public string Name { get; } = name;
  public bool IsPure { get; } = pure;
  public bool IsInline { get; } = inline;
  public bool IsNoinline { get; } = noinline;
  public IReadOnlyList<FunctionSignature> Signatures { get; } = signatures;
  public BlockStatementIr Body { get; } = body;
  public IReadOnlyList<MetadataIr> Metadata { get; } = metadata ?? [];
}

public abstract class StatementIr(IEnumerable<HixIrNode> children = null) : HixIrNode(children: children?.ToArray());

public sealed class BlockStatementIr(IReadOnlyList<StatementIr> statements, string label = null)
  : StatementIr(statements) {
  public IReadOnlyList<StatementIr> Statements { get; } = statements;
  public string Label { get; } = label;
}

public enum StorageSpace { Local, Variable, Target }

public sealed class AssignmentStatementIr(StorageSpace storage, string name, ExpressionIr value, bool carried = false)
  : StatementIr([value]) {
  public StorageSpace Storage { get; } = storage;
  public string Name { get; } = name;
  public ExpressionIr Value { get; } = value;
  public bool IsCarried { get; } = carried;
  public HixVariableSymbol Symbol { get; set; }
}

public sealed class InvocationStatementIr(CallExpressionIr call) : StatementIr([call]) {
  public CallExpressionIr Call { get; } = call;
}

public enum ControlFlowKind { Return, Goto, Break, Continue, Label }

public sealed class ControlFlowStatementIr(ControlFlowKind operation, string label,
  IReadOnlyList<ExpressionIr> values
) : StatementIr(values) {
  public ControlFlowKind Operation { get; } = operation;
  public string Label { get; } = label;
  public IReadOnlyList<ExpressionIr> Values { get; } = values;
}

public sealed class SelectionStatementIr(SelectionExpressionIr selection) : StatementIr([selection]) {
  public SelectionExpressionIr Selection { get; } = selection;
}

public abstract class ExpressionIr(IEnumerable<HixIrNode> children = null) : HixIrNode(children: children?.ToArray()) {
  public HixPattern InferredPattern { get; set; } = HixPattern.Any;
}

public sealed class StringExpressionIr(string value) : ExpressionIr {
  public string Value { get; } = value;
}

public sealed class NumberExpressionIr(double value) : ExpressionIr {
  public double Value { get; } = value;
}

public sealed class BooleanExpressionIr(bool value) : ExpressionIr {
  public bool Value { get; } = value;
}

public sealed class NullExpressionIr : ExpressionIr;

/// <summary>The current selection value used by a when transformation condition.</summary>
public sealed class SelectorExpressionIr : ExpressionIr;

public sealed class RootExpressionIr(string name, bool smart = false) : ExpressionIr {
  public string Name { get; } = name;
  public bool IsSmart { get; } = smart;
  public HixReferenceBinding Binding { get; set; } = HixReferenceBinding.Unbound;
}

public sealed class MemberExpressionIr(ExpressionIr receiver, string member) : ExpressionIr([receiver]) {
  public ExpressionIr Receiver { get; } = receiver;
  public string Member { get; } = member;
  public HixReferenceBinding Binding { get; set; } = HixReferenceBinding.Unbound;
}

public sealed class CallExpressionIr(string name, IReadOnlyList<ExpressionIr> arguments,
  bool coerceBoolean = false, HixCallBinding binding = null
) : ExpressionIr(arguments) {
  public string Name { get; } = name;
  public IReadOnlyList<ExpressionIr> Arguments { get; } = arguments;
  public bool CoerceBoolean { get; } = coerceBoolean;
  public HixCallBinding Binding { get; set; } = binding ?? HixCallBinding.Unbound;
}

public sealed class InlineExpressionIr(BlockStatementIr body, string resultLocal) : ExpressionIr([body]) {
  public BlockStatementIr Body { get; } = body;
  public string ResultLocal { get; } = resultLocal;
}

public sealed class LambdaExpressionIr(BlockStatementIr body) : ExpressionIr([body]) {
  public BlockStatementIr Body { get; } = body;
}

public enum UnaryOperation { Not, Check }

public sealed class UnaryExpressionIr(UnaryOperation operation, ExpressionIr value) : ExpressionIr([value]) {
  public UnaryOperation Operation { get; } = operation;
  public ExpressionIr Value { get; } = value;
}

public sealed class FallbackExpressionIr(ExpressionIr value, ExpressionIr fallback)
  : ExpressionIr([value, fallback]) {
  public ExpressionIr Value { get; } = value;
  public ExpressionIr Fallback { get; } = fallback;
}

public sealed class TupleExpressionIr(IReadOnlyList<ExpressionIr> values) : ExpressionIr(values) {
  public IReadOnlyList<ExpressionIr> Values { get; } = values;
}

public sealed class TableExpressionIr(IReadOnlyList<KeyValuePair<string, ExpressionIr>> entries,
  IReadOnlyList<KeyValuePair<string, IReadOnlyList<MetadataIr>>> fieldMetadata = null)
  : ExpressionIr((fieldMetadata?.SelectMany(value => value.Value).Cast<HixIrNode>() ?? [])
    .Concat(entries.Select(entry => entry.Value)).ToArray()) {
  public IReadOnlyList<KeyValuePair<string, ExpressionIr>> Entries { get; } = entries;
  public IReadOnlyList<KeyValuePair<string, IReadOnlyList<MetadataIr>>> FieldMetadata { get; } = fieldMetadata ?? [];
}

public sealed class InterpolationExpressionIr(IReadOnlyList<ExpressionIr> parts) : ExpressionIr(parts) {
  public IReadOnlyList<ExpressionIr> Parts { get; } = parts;
}

public sealed class SelectionBranchIr(IReadOnlyList<ExpressionIr> conditions, HixIrNode result,
  bool transformation = false
) : HixIrNode(children: conditions.Concat([result]).ToArray()) {
  public IReadOnlyList<ExpressionIr> Conditions { get; } = conditions;
  public HixIrNode Result { get; } = result;
  public bool IsTransformation { get; } = transformation;
}

public sealed class SelectionExpressionIr(ExpressionIr selector, IReadOnlyList<SelectionBranchIr> branches,
  HixIrNode fallback
) : ExpressionIr(
  (selector is null ? Enumerable.Empty<HixIrNode>() : [selector])
  .Concat(branches)
  .Concat(fallback is null ? [] : [fallback])
) {
  public ExpressionIr Selector { get; } = selector;
  public IReadOnlyList<SelectionBranchIr> Branches { get; } = branches;
  public HixIrNode Fallback { get; } = fallback;
}
