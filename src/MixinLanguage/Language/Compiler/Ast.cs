using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;

namespace Mixins.Compiler;

public enum MixinSyntaxKind {
  Document,
  Directive,
  DirectiveName,
  DirectiveArgument,
  DeclarationDirectiveArgument,
  ReferenceDirectiveArgument,
  DeclarationReferenceDirectiveArgument,
  Operand,
  Reference,
  ParenthesizedReference,
  Root,
  Member,
  Path,
  FunctionCall,
  LiteralArgument,
  ExpressionArgument,
  Comment,
  Continuation,
  Escape,
  Punctuation,
  Error
}

public abstract class MixinAst {
  private IReadOnlyList<MixinAst> _children = [];
  private MixinSourceRange _sourceRange;
  private MixinAst _rangeOwner;

  protected MixinAst(
    MixinSyntaxKind kind = MixinSyntaxKind.Directive,
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

  public ProgramAst Program {
    get {
      for (MixinAst current = this; current is not null; current = current.Parent)
        if (current is ProgramAst program)
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

public class LeafAst(
  MixinSyntaxKind kind, MixinSourceRange sourceRange,
  IReadOnlyList<MixinAst> children = null,
  MixinDirectiveArgumentMetadata argumentMetadata = null
) : MixinAst(kind, sourceRange, children) {
  public MixinDirectiveArgumentMetadata ArgumentMetadata { get; } = argumentMetadata;
}

public sealed class TriviaAst(MixinSyntaxKind kind, MixinSourceRange sourceRange) : LeafAst(kind, sourceRange) {
  public override bool IsTrivia => true;
}

public sealed class ProgramAst : MixinAst {
  private readonly InstructionAst[] _instructions;

  internal ProgramAst(string expression) : base(MixinSyntaxKind.Document) {
    Source = expression ?? "";
    var parsed = MixinParser.ParseProgram(expression ?? "");
    _instructions = parsed.Instructions;
    Diagnostics = parsed.Diagnostics;
    Tokens = parsed.Tokens;
    SourceRange = new MixinSourceRange(0, Source.Length, 1, 0);
    Children = _instructions;
  }

  internal ProgramAst(IEnumerable<InstructionAst> instructions)
    : base(MixinSyntaxKind.Document, new MixinSourceRange(0, 0, 1, 0)) {
    _instructions = [.. instructions ?? []];
    Diagnostics = [];
    Source = "";
    Children = _instructions;
  }

  internal int Count => _instructions.Length;
  public IReadOnlyList<InstructionAst> Instructions => _instructions;
  public IReadOnlyList<MixinParseDiagnostic> Diagnostics { get; }
  public string Source { get; }
  public ProgramAst Root => this;

  public ValidationResult FailureOrDefault() => Diagnostics.Count == 0
    ? null
    : new ValidationResult(false, Diagnostics[0].Message, Diagnostics[0].Line);

  public bool TryGetFailure(out ValidationResult failure) {
    failure = FailureOrDefault();
    return failure != null;
  }

  internal InstructionAst Get(int index) {
    return _instructions[index];
  }

  internal IEnumerable<InstructionAst> AvailableInstructions() {
    return _instructions;
  }

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var item in _instructions) {
      pool.Intern(MixinSyntaxFacts.Command(item));
      item.CollectConstants(pool);
    }
  }
}

public abstract class InstructionAst : MixinAst {
  public DirectiveDefinition Definition { get; internal set; }
  /// <summary>Range of the leading <c>@</c> marker in the original source.</summary>
  public MixinSourceRange MarkerRange { get; internal set; }
  /// <summary>Range of the directive identifier, excluding the leading <c>@</c>.</summary>
  public MixinSourceRange NameRange { get; internal set; }
  /// <summary>Ranges of complete <c>&lt;...&gt;</c> arguments.</summary>
  public IReadOnlyList<MixinSourceRange> ArgumentRanges { get; internal set; } = [];
  /// <summary>Ranges of argument contents, excluding delimiters and trimmed outer whitespace.</summary>
  public IReadOnlyList<MixinSourceRange> ArgumentContentRanges { get; internal set; } = [];
  /// <summary>Range of the operand after directive arguments and separating whitespace.</summary>
  public MixinSourceRange OperandRange { get; internal set; }
  public IReadOnlyList<DirectiveArgumentAst> ParsedArguments { get; internal set; } = [];

  internal InstructionAst InheritFrom(InstructionAst origin) {
    InheritRangeFrom(origin);
    return this;
  }

  internal abstract void CollectConstants(MixinStringPoolBuilder pool);

  private protected static void Collect(
    MixinStringPoolBuilder pool, string value, IReadOnlyList<ValueAst> expression = null
  ) {
    pool.Intern(value);
    foreach (var part in expression ?? []) {
      if (part.Reference is null) pool.Intern(part.Literal);
      else part.Reference.CollectConstants(pool);
    }
  }
}

/// <summary>A built-in language statement such as CALL, SCOPE, LOCAL, or CODE.</summary>
/// <summary>A catalog-defined executable directive.</summary>
public abstract class DirectiveAst : InstructionAst;

public abstract class StatementAst : InstructionAst;

/// <summary>A catalog-defined executable directive invocation.</summary>
public abstract class ExecutableDirectiveAst : DirectiveAst;

public abstract class ValueStatementAst : StatementAst {
  protected ValueStatementAst(IReadOnlyList<ValueAst> expression) {
    Expression = expression ?? [];
    Adopt(Expression);
  }

  public IReadOnlyList<ValueAst> Expression { get; }

  internal override void CollectConstants(MixinStringPoolBuilder pool) => Collect(pool, null, Expression);
}

public abstract class BooleanStatementAst : StatementAst {
  protected BooleanStatementAst(
    IReadOnlyList<MixinExpressionReference> expression
  ) {
    Expression = expression ?? [];
    Adopt(Expression);
  }

  public IReadOnlyList<MixinExpressionReference> Expression { get; }

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var item in Expression) item.CollectConstants(pool);
  }
}

public sealed class EmptyDirectiveAst : StatementAst {
  internal override void CollectConstants(MixinStringPoolBuilder pool) { }
}

public sealed class UnknownDirectiveAst(string command) : DirectiveAst {
  public string Name { get; } = command;

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Name);
  }
}

public sealed class DirectiveInvocationAst : ExecutableDirectiveAst {
  public DirectiveInvocationAst(
    DirectiveDefinition definition,
    IReadOnlyList<DirectiveArgumentAst> arguments, IReadOnlyList<ValueAst> operand
  ) {
    Definition = definition;
    Expression = operand ?? [];
    ParsedArguments = arguments ?? [];
    Arguments = [.. arguments.Select(MixinSyntaxRenderer.RenderArgument)];
    Adopt(arguments, Expression);
  }

  public IReadOnlyList<ValueAst> Expression { get; }
  public IReadOnlyList<string> Arguments { get; }
  public string Argument => Arguments.Count == 0 ? null : Arguments[0];

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Definition.Name);
    foreach (var item in Arguments) pool.Intern(item);
    Collect(pool, null, Expression);
  }
}

public sealed class ScopeAst(string label) : StatementAst {
  internal string Label { get; } = label;

  internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Label);
}

public sealed class LabelAst(string name) : StatementAst {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Name);
}

public sealed class FunctionAst(string name) : StatementAst {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Name);
}

public sealed class CallAst(string function, string returnLocal,
  IReadOnlyList<ValueAst> parameter
) : ValueStatementAst(parameter) {
  internal string Function { get; } = function;
  internal string ReturnLocal { get; } = returnLocal;
}

public sealed class InlineAst(string name) : StatementAst {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class EndAst : StatementAst {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class MatchAst(string failureLabel,
  IReadOnlyList<MixinExpressionReference> condition
)
  : BooleanStatementAst(condition) {
  internal string FailureLabel { get; } = failureLabel;
}

public sealed class AssertAst(IReadOnlyList<MixinExpressionReference> condition)
  : BooleanStatementAst(condition);

public sealed class CodeAst(MixinExpressionOutputTarget target,
  string injectionTarget,
  IReadOnlyList<ValueAst> code
) : ValueStatementAst(code) {
  internal MixinExpressionOutputTarget Target { get; } = target;
  internal string InjectionTarget { get; } = injectionTarget;
}

public sealed class TargetedCodeAst(DirectiveArgumentAst target,
  DirectiveArgumentAst priority,
  IReadOnlyList<ValueAst> code
) : ValueStatementAst(code) {
  internal DirectiveArgumentAst Target { get; } = target;
  internal DirectiveArgumentAst Priority { get; } = priority;
}

public sealed class UsingAst(IReadOnlyList<ValueAst> value) : ValueStatementAst(value);

public sealed class LogAst(IReadOnlyList<ValueAst> message) : ValueStatementAst(message);

public sealed class LocalAst(string name, IReadOnlyList<ValueAst> value) : ValueStatementAst(value) {
  internal string Name { get; } = name;
}

public sealed class VariableAst(string name, IReadOnlyList<ValueAst> value) : ValueStatementAst(value) {
  internal string Name { get; } = name;
}

public sealed class TargetVariableAst(string name,
  IReadOnlyList<ValueAst> value
) : ValueStatementAst(value) {
  internal string Name { get; } = name;
}

public sealed class CarryAst(string label, IReadOnlyList<ValueAst> value) : ValueStatementAst(value) {
  internal string Label { get; } = label;
}

public sealed class ReturnAst(IReadOnlyList<ValueAst> value) : ValueStatementAst(value);

public sealed class GotoAst(string label) : StatementAst {
  internal string Label { get; } = label;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Label);
  }
}

public sealed class SkipAst : StatementAst {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class FailAst(IReadOnlyList<ValueAst> message) : ValueStatementAst(message);

public sealed class AnnotationAst(string name) : StatementAst {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class PreludeAst : StatementAst {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class DefineTargetAst(string name, string value) : StatementAst {
  internal string Name { get; } = name;
  internal string Value { get; } = value;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
    p.Intern(Value);
  }
}

public sealed class DirectiveArgumentAst(
  string literal, IReadOnlyList<ValueAst> expression, MixinSourceRange sourceRange = default
) : MixinAst(MixinSyntaxKind.DirectiveArgument, sourceRange, expression?.Cast<MixinAst>().ToArray()) {
  public string Literal { get; } = literal;
  public IReadOnlyList<ValueAst> Expression { get; } = expression;
  public MixinDirectiveArgumentMetadata ArgumentMetadata { get; internal set; }
  internal bool IsDynamic => Expression is not null;
}

public sealed class ValueAst(
  string literal, MixinExpressionReference reference, bool verbatim = false,
  MixinSourceRange sourceRange = default
) : MixinAst(
  reference is null ? MixinSyntaxKind.LiteralArgument : MixinSyntaxKind.ExpressionArgument,
  sourceRange, reference is null ? null : [reference]
) {
  public string Literal { get; } = literal;
  public MixinExpressionReference Reference { get; } = reference;
  public bool Verbatim { get; } = verbatim;
}

public sealed record MixinDirectiveSyntaxData(
  IReadOnlyList<string> Arguments,
  IReadOnlyList<DirectiveArgumentAst> ParsedArguments,
  IReadOnlyList<ValueAst> ValueOperand,
  IReadOnlyList<MixinExpressionReference> BooleanOperand
);

public sealed class MixinPropertyArgumentAst(
  string literal,
  IReadOnlyList<ValueAst> valueExpression,
  IReadOnlyList<MixinExpressionReference> booleanExpression,
  MixinSourceRange sourceRange = default
) : MixinAst(
  literal is not null ? MixinSyntaxKind.LiteralArgument : MixinSyntaxKind.ExpressionArgument,
  sourceRange,
  valueExpression is not null
    ? valueExpression.Cast<MixinAst>().ToArray()
    : booleanExpression?.Cast<MixinAst>().ToArray()
) {
  public string Literal { get; } = literal;
  public IReadOnlyList<ValueAst> ValueExpression { get; } = valueExpression;
  public IReadOnlyList<MixinExpressionReference> BooleanExpression { get; } = booleanExpression;
}

public readonly record struct MixinSourceRange(
  int Start, int End, int Line = 0, int Column = 0
) {
  public int Length => End - Start;
  public bool IsEmpty => Length == 0;

  public static MixinSourceRange Compose(IEnumerable<MixinSourceRange> ranges) {
    var values = (ranges ?? []).Where(range => !range.IsEmpty).ToArray();
    if (values.Length == 0) return default;
    var first = values.OrderBy(range => range.Start).First();
    return new MixinSourceRange(
      values.Min(range => range.Start), values.Max(range => range.End), first.Line, first.Column
    );
  }
}

/// <summary>Parser-only reference syntax. This type never crosses the lowering boundary.</summary>
public sealed class MixinExpressionReference(
  MixinExpressionRoot root, string member, IReadOnlyList<MixinExpressionProperty> properties,
  bool parenthesized = false, MixinSourceRange sourceRange = default,
  MixinSourceRange rootRange = default, MixinSourceRange memberRange = default
) : MixinAst(
  parenthesized ? MixinSyntaxKind.ParenthesizedReference : MixinSyntaxKind.Reference,
  children: MixinAstLayout.Reference(sourceRange, rootRange, memberRange, properties)
) {
  public MixinExpressionRoot Root { get; } = root;
  public string Member { get; } = member;
  public IReadOnlyList<MixinExpressionProperty> Properties { get; } = properties ?? [];
  public bool Parenthesized { get; } = parenthesized;
  public MixinSourceRange RootRange { get; internal set; } = rootRange;
  public MixinSourceRange MemberRange { get; internal set; } = memberRange;

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Member);
    foreach (var property in Properties) property.CollectConstants(pool);
  }
}

public sealed class MixinExpressionProperty(
  string name, IReadOnlyList<MixinPropertyArgumentAst> arguments, bool negated = false,
  FunctionDefinition definition = null, MixinSourceRange sourceRange = default,
  MixinSourceRange nameRange = default
) : MixinAst(
  name == "path" ? MixinSyntaxKind.Path : MixinSyntaxKind.FunctionCall,
  children: MixinAstLayout.Property(sourceRange, nameRange, arguments)
) {
  internal MixinExpressionProperty(string name, string argument) : this(
    name, argument is null ? [] : [new MixinPropertyArgumentAst(argument, null, null)]
  ) { }

  public string Name { get; } = name;
  public IReadOnlyList<MixinPropertyArgumentAst> ParsedArguments { get; } = arguments ?? [];
  public IReadOnlyList<string> Arguments { get; } = [.. (arguments ?? []).Select(item => item.Literal)];
  public string Argument => Arguments.Count == 0 ? null : Arguments[0];
  public bool Negated { get; } = negated;
  public MixinSourceRange NameRange { get; internal set; } = nameRange;
  internal FunctionDefinition Definition { get; } = definition;

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Name);
    foreach (var argument in ParsedArguments) {
      pool.Intern(argument.Literal);
      foreach (var part in argument.ValueExpression ?? []) {
        if (part.Reference is null) pool.Intern(part.Literal);
        else part.Reference.CollectConstants(pool);
      }
      foreach (var reference in argument.BooleanExpression ?? []) reference.CollectConstants(pool);
    }
  }
}

internal static class MixinAstLayout {
  internal static IReadOnlyList<MixinAst> Reference(
    MixinSourceRange range, MixinSourceRange rootRange, MixinSourceRange memberRange,
    IReadOnlyList<MixinExpressionProperty> properties
  ) {
    var children = new List<MixinAst>();
    AddGap(children, range, range.Start, rootRange.Start);
    children.Add(new LeafAst(MixinSyntaxKind.Root, rootRange));
    if (!memberRange.IsEmpty) {
      AddGap(children, range, rootRange.End, memberRange.Start);
      children.Add(new LeafAst(MixinSyntaxKind.Member, memberRange));
    }
    children.AddRange(properties ?? []);
    var end = children.Count == 0 ? range.Start : children.Max(child => child.SourceRange.End);
    AddGap(children, range, end, range.End);
    return children;
  }

  internal static IReadOnlyList<MixinAst> Property(
    MixinSourceRange range, MixinSourceRange nameRange,
    IReadOnlyList<MixinPropertyArgumentAst> arguments
  ) {
    var children = new List<MixinAst>();
    AddGap(children, range, range.Start, nameRange.Start);
    children.Add(new LeafAst(MixinSyntaxKind.FunctionCall, nameRange));
    children.AddRange(arguments ?? []);
    return children;
  }

  private static void AddGap(
    ICollection<MixinAst> children, MixinSourceRange owner, int start, int end
  ) {
    if (end <= start) return;
    children.Add(
      new LeafAst(
        MixinSyntaxKind.Punctuation, new MixinSourceRange(
          start, end, owner.Line, owner.Column + start - owner.Start
        )
      )
    );
  }
}

internal static class MixinSyntaxFacts {
  internal static string Command(InstructionAst n) {
    if (n.Definition is not null) return n.Definition.Name;
    return n switch {
      EmptyDirectiveAst => null, UnknownDirectiveAst x => x.Name,
      DirectiveInvocationAst x => x.Definition.Name,
      ScopeAst => "SCOPE", LabelAst => "LABEL", FunctionAst => "FUNC",
      CallAst => "CALL", InlineAst => "INLINE", EndAst => "END",
      MatchAst => "MATCH", AssertAst => "ASSERT", CodeAst => "CODE",
      TargetedCodeAst => "MIXIN", UsingAst => "USING", LogAst => "LOG",
      LocalAst => "LOCAL", VariableAst => "VAR", TargetVariableAst => "TAR",
      CarryAst => "CARRY",
      ReturnAst => "RETURN", GotoAst => "GOTO", SkipAst => "SKIP",
      FailAst => "FAIL", AnnotationAst => "ANNOTATION", PreludeAst => "PRELUDE",
      DefineTargetAst => "DEFINE_TARGET", _ => null
    };
  }

  internal static IReadOnlyList<string> Arguments(InstructionAst n) {
    return n switch {
      DirectiveInvocationAst x => x.Arguments,
      ScopeAst { Label: not null } x => [x.Label], LabelAst x => [x.Name],
      FunctionAst x => [x.Name], InlineAst x => [x.Name],
      CallAst { ReturnLocal: not null } x => [x.ReturnLocal, x.Function],
      CallAst x => [x.Function],
      MatchAst { FailureLabel: not null } x => [x.FailureLabel],
      CodeAst { Target: MixinExpressionOutputTarget.Injection } x => [x.InjectionTarget],
      CodeAst { Target: not MixinExpressionOutputTarget.Target } x => [x.Target.ToString().ToUpperInvariant()],
      TargetedCodeAst { Priority: not null } x => [
        MixinSyntaxRenderer.RenderArgument(x.Target), MixinSyntaxRenderer.RenderArgument(x.Priority)
      ],
      TargetedCodeAst x => [MixinSyntaxRenderer.RenderArgument(x.Target)],
      LocalAst x => [x.Name], VariableAst x => [x.Name],
      TargetVariableAst x => [x.Name], CarryAst x => [x.Label],
      GotoAst x => [x.Label], AnnotationAst x => [x.Name],
      DefineTargetAst x => [x.Name, x.Value], _ => []
    };
  }

  internal static string Operand(InstructionAst n) {
    return n switch {
      DirectiveInvocationAst x => MixinSyntaxRenderer.RenderValue(x.Expression),
      ValueStatementAst x => MixinSyntaxRenderer.RenderValue(x.Expression),
      BooleanStatementAst x => MixinSyntaxRenderer.RenderBoolean(x.Expression), _ => ""
    };
  }
}