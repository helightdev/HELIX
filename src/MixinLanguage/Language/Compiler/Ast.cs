using System.Collections.Generic;
using System.Linq;

namespace MixinLanguage.Compiler;

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
  Error
}

public abstract class MixinAst(
  MixinSyntaxKind kind = MixinSyntaxKind.Directive,
  MixinSourceRange sourceRange = default, IReadOnlyList<MixinAst> children = null
) {
  public int Line => SourceRange.Line;
  public MixinSourceRange SourceRange { get; internal set; } = sourceRange;
  public MixinSyntaxKind Kind { get; internal set; } = kind;
  public IReadOnlyList<MixinAst> Children { get; internal set; } = children ?? [];
  public IReadOnlyList<MixinToken> Tokens { get; internal set; } = [];
}

public sealed class LeafAst(
  MixinSyntaxKind kind, MixinSourceRange sourceRange,
  IReadOnlyList<MixinAst> children = null,
  MixinDirectiveArgumentMetadata argumentMetadata = null
) : MixinAst(kind, sourceRange, children) {
  public MixinDirectiveArgumentMetadata ArgumentMetadata { get; } = argumentMetadata;
}

public sealed class ProgramAst : MixinAst {
  private readonly DirectiveAst[] _instructions;

  internal ProgramAst(string expression) : base(MixinSyntaxKind.Document) {
    Source = expression ?? "";
    var parsed = MixinParser.ParseProgram(expression ?? "");
    _instructions = parsed.Instructions;
    Diagnostics = parsed.Diagnostics;
    Tokens = parsed.Tokens;
    SourceRange = new MixinSourceRange(0, Source.Length, 1, 0);
    Children = _instructions;
  }

  internal ProgramAst(IEnumerable<DirectiveAst> instructions)
    : base(MixinSyntaxKind.Document, new MixinSourceRange(0, 0, 1, 0)) {
    _instructions = [.. instructions ?? []];
    Diagnostics = [];
    Source = "";
    Children = _instructions;
  }

  internal int Count => _instructions.Length;
  public IReadOnlyList<DirectiveAst> Instructions => _instructions;
  public IReadOnlyList<MixinParseDiagnostic> Diagnostics { get; }
  public string Source { get; }
  public ProgramAst Root => this;
  public ProgramAst Program => this;

  internal DirectiveAst Get(int index) {
    return _instructions[index];
  }

  internal IEnumerable<DirectiveAst> AvailableInstructions() {
    return _instructions;
  }

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var item in _instructions) {
      pool.Intern(MixinSyntaxFacts.Command(item));
      item.CollectConstants(pool);
    }
  }
}

public abstract class DirectiveAst(MixinSourceRange sourceRange)
  : MixinAst(sourceRange: sourceRange) {
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

public abstract class ValueDirectiveAst(MixinSourceRange sourceRange, IReadOnlyList<ValueAst> expression)
  : DirectiveAst(sourceRange) {
  public IReadOnlyList<ValueAst> Expression { get; } = expression ?? [];

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    Collect(pool, null, Expression);
  }
}

public abstract class BooleanDirectiveAst(MixinSourceRange sourceRange,
  IReadOnlyList<MixinExpressionReference> expression
)
  : DirectiveAst(sourceRange) {
  public IReadOnlyList<MixinExpressionReference> Expression { get; } = expression ?? [];

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var item in Expression) item.CollectConstants(pool);
  }
}

public sealed class EmptyDirectiveAst(MixinSourceRange sourceRange) : DirectiveAst(sourceRange) {
  internal override void CollectConstants(MixinStringPoolBuilder pool) { }
}

public sealed class UnknownDirectiveAst(MixinSourceRange sourceRange, string command) : DirectiveAst(sourceRange) {
  public string Name { get; } = command;

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Name);
  }
}

public sealed class DirectiveInvocationAst : ValueDirectiveAst {
  public DirectiveInvocationAst(
    MixinSourceRange sourceRange, DirectiveDefinition definition,
    IReadOnlyList<DirectiveArgumentAst> arguments, IReadOnlyList<ValueAst> operand
  ) : base(sourceRange, operand) {
    Definition = definition;
    ParsedArguments = arguments;
    Arguments = [.. arguments.Select(MixinSyntaxRenderer.RenderArgument)];
  }

  public IReadOnlyList<DirectiveArgumentAst> ParsedArguments { get; }
  public IReadOnlyList<string> Arguments { get; }
  public string Argument => Arguments.Count == 0 ? null : Arguments[0];

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Definition.Name);
    foreach (var item in Arguments) pool.Intern(item);
    base.CollectConstants(pool);
  }
}

public sealed class ScopeAst(MixinSourceRange sourceRange, string label) : DirectiveAst(sourceRange) {
  internal string Label { get; } = label;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Label);
  }
}

public sealed class LabelAst(MixinSourceRange sourceRange, string name) : DirectiveAst(sourceRange) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class FunctionAst(MixinSourceRange sourceRange, string name) : DirectiveAst(sourceRange) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class CallDirectiveAst(MixinSourceRange sourceRange, string function, string returnLocal,
  IReadOnlyList<ValueAst> parameter
) : ValueDirectiveAst(sourceRange, parameter) {
  internal string Function { get; } = function;
  internal string ReturnLocal { get; } = returnLocal;
}

public sealed class InlineDirectiveAst(MixinSourceRange sourceRange, string name) : DirectiveAst(sourceRange) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class EndAst(MixinSourceRange sourceRange) : DirectiveAst(sourceRange) {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class MatchDirectiveAst(MixinSourceRange sourceRange, string failureLabel,
  IReadOnlyList<MixinExpressionReference> condition
)
  : BooleanDirectiveAst(sourceRange, condition) {
  internal string FailureLabel { get; } = failureLabel;
}

public sealed class AssertDirectiveAst(MixinSourceRange sourceRange, IReadOnlyList<MixinExpressionReference> condition)
  : BooleanDirectiveAst(sourceRange, condition);

public sealed class CodeDirectiveAst(MixinSourceRange sourceRange, MixinExpressionOutputTarget target,
  string injectionTarget,
  IReadOnlyList<ValueAst> code
) : ValueDirectiveAst(sourceRange, code) {
  internal MixinExpressionOutputTarget Target { get; } = target;
  internal string InjectionTarget { get; } = injectionTarget;
}

public sealed class TargetedCodeDirectiveAst(MixinSourceRange sourceRange, DirectiveArgumentAst target,
  DirectiveArgumentAst priority,
  IReadOnlyList<ValueAst> code
) : ValueDirectiveAst(sourceRange, code) {
  internal DirectiveArgumentAst Target { get; } = target;
  internal DirectiveArgumentAst Priority { get; } = priority;
}

public sealed class UsingDirectiveSyntax(MixinSourceRange sourceRange, IReadOnlyList<ValueAst> value)
  : ValueDirectiveAst(sourceRange, value);

public sealed class LogDirectiveSyntax(MixinSourceRange sourceRange, IReadOnlyList<ValueAst> message)
  : ValueDirectiveAst(sourceRange, message);

public sealed class LocalDirectiveSyntax(MixinSourceRange sourceRange, string name, IReadOnlyList<ValueAst> value)
  : ValueDirectiveAst(sourceRange, value) {
  internal string Name { get; } = name;
}

public sealed class VariableDirectiveAst(MixinSourceRange sourceRange, string name, IReadOnlyList<ValueAst> value)
  : ValueDirectiveAst(sourceRange, value) {
  internal string Name { get; } = name;
}

public sealed class TargetVariableDirectiveAst(MixinSourceRange sourceRange, string name,
  IReadOnlyList<ValueAst> value
)
  : ValueDirectiveAst(sourceRange, value) {
  internal string Name { get; } = name;
}

public sealed class CarryDirectiveAst(MixinSourceRange sourceRange, string label, IReadOnlyList<ValueAst> value)
  : ValueDirectiveAst(sourceRange, value) {
  internal string Label { get; } = label;
}

public sealed class ReturnDirectiveAst(MixinSourceRange sourceRange, IReadOnlyList<ValueAst> value)
  : ValueDirectiveAst(sourceRange, value);

public sealed class GotoDirectiveAst(MixinSourceRange sourceRange, string label) : DirectiveAst(sourceRange) {
  internal string Label { get; } = label;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Label);
  }
}

public sealed class SkipDirectiveAst(MixinSourceRange sourceRange) : DirectiveAst(sourceRange) {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class FailDirectiveAst(MixinSourceRange sourceRange, IReadOnlyList<ValueAst> message)
  : ValueDirectiveAst(sourceRange, message);

public sealed class AnnotationAst(MixinSourceRange sourceRange, string name) : DirectiveAst(sourceRange) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class PreludeAst(MixinSourceRange sourceRange) : DirectiveAst(sourceRange) {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class DefineTargetAst(MixinSourceRange sourceRange, string name, string value)
  : DirectiveAst(sourceRange) {
  internal string Name { get; } = name;
  internal string Value { get; } = value;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
    p.Intern(Value);
  }
}

public sealed class DirectiveArgumentAst(
  string literal, IReadOnlyList<ValueAst> expression, MixinSourceRange sourceRange = default
) {
  public string Literal { get; } = literal;
  public IReadOnlyList<ValueAst> Expression { get; } = expression;
  public MixinSourceRange SourceRange { get; internal set; } = sourceRange;
  internal bool IsDynamic => Expression is not null;
}

public sealed class ValueAst(
  string literal, MixinExpressionReference reference, bool verbatim = false,
  MixinSourceRange sourceRange = default
) {
  public string Literal { get; } = literal;
  public MixinExpressionReference Reference { get; } = reference;
  public bool Verbatim { get; } = verbatim;
  public MixinSourceRange SourceRange { get; internal set; } = sourceRange;
}

public sealed record MixinDirectiveSyntaxData(
  MixinSourceRange SourceRange, IReadOnlyList<string> Arguments,
  IReadOnlyList<DirectiveArgumentAst> ParsedArguments,
  IReadOnlyList<ValueAst> ValueOperand,
  IReadOnlyList<MixinExpressionReference> BooleanOperand
);

public sealed class MixinPropertyArgumentAst(
  string literal,
  IReadOnlyList<ValueAst> valueExpression,
  IReadOnlyList<MixinExpressionReference> booleanExpression,
  MixinSourceRange sourceRange = default
) {
  public string Literal { get; } = literal;
  public IReadOnlyList<ValueAst> ValueExpression { get; } = valueExpression;
  public IReadOnlyList<MixinExpressionReference> BooleanExpression { get; } = booleanExpression;
  public MixinSourceRange SourceRange { get; internal set; } = sourceRange;
}

internal static class MixinSyntaxFacts {
  internal static string Command(DirectiveAst n) {
    if (n.Definition is not null) return n.Definition.Name;
    return n switch {
      EmptyDirectiveAst => null, UnknownDirectiveAst x => x.Name,
      DirectiveInvocationAst x => x.Definition.Name,
      ScopeAst => "SCOPE", LabelAst => "LABEL", FunctionAst => "FUNC",
      CallDirectiveAst => "CALL", InlineDirectiveAst => "INLINE", EndAst => "END",
      MatchDirectiveAst => "MATCH", AssertDirectiveAst => "ASSERT", CodeDirectiveAst => "CODE",
      TargetedCodeDirectiveAst => "MIXIN", UsingDirectiveSyntax => "USING", LogDirectiveSyntax => "LOG",
      LocalDirectiveSyntax => "LOCAL", VariableDirectiveAst => "VAR", TargetVariableDirectiveAst => "TAR",
      CarryDirectiveAst => "CARRY",
      ReturnDirectiveAst => "RETURN", GotoDirectiveAst => "GOTO", SkipDirectiveAst => "SKIP",
      FailDirectiveAst => "FAIL", AnnotationAst => "ANNOTATION", PreludeAst => "PRELUDE",
      DefineTargetAst => "DEFINE_TARGET", _ => null
    };
  }

  internal static IReadOnlyList<string> Arguments(DirectiveAst n) {
    return n switch {
      DirectiveInvocationAst x => x.Arguments,
      ScopeAst { Label: not null } x => [x.Label], LabelAst x => [x.Name],
      FunctionAst x => [x.Name], InlineDirectiveAst x => [x.Name],
      CallDirectiveAst { ReturnLocal: not null } x => [x.ReturnLocal, x.Function],
      CallDirectiveAst x => [x.Function],
      MatchDirectiveAst { FailureLabel: not null } x => [x.FailureLabel],
      CodeDirectiveAst { Target: MixinExpressionOutputTarget.Injection } x => [x.InjectionTarget],
      CodeDirectiveAst { Target: not MixinExpressionOutputTarget.Target } x => [x.Target.ToString().ToUpperInvariant()],
      TargetedCodeDirectiveAst { Priority: not null } x => [
        MixinSyntaxRenderer.RenderArgument(x.Target), MixinSyntaxRenderer.RenderArgument(x.Priority)
      ],
      TargetedCodeDirectiveAst x => [MixinSyntaxRenderer.RenderArgument(x.Target)],
      LocalDirectiveSyntax x => [x.Name], VariableDirectiveAst x => [x.Name],
      TargetVariableDirectiveAst x => [x.Name], CarryDirectiveAst x => [x.Label],
      GotoDirectiveAst x => [x.Label], AnnotationAst x => [x.Name],
      DefineTargetAst x => [x.Name, x.Value], _ => []
    };
  }

  internal static string Operand(DirectiveAst n) {
    return n switch {
      ValueDirectiveAst x => MixinSyntaxRenderer.RenderValue(x.Expression),
      BooleanDirectiveAst x => MixinSyntaxRenderer.RenderBoolean(x.Expression), _ => ""
    };
  }
}

public readonly record struct MixinSourceRange(
  int Start, int End, int Line = 0, int Column = 0
) {
  public int Length => End - Start;
  public bool IsEmpty => Length == 0;
}

/// <summary>Parser-only reference syntax. This type never crosses the lowering boundary.</summary>
public sealed class MixinExpressionReference(
  MixinExpressionRoot root, string member, IReadOnlyList<MixinExpressionProperty> properties,
  bool parenthesized = false, MixinSourceRange sourceRange = default,
  MixinSourceRange rootRange = default, MixinSourceRange memberRange = default
) {
  public MixinExpressionRoot Root { get; } = root;
  public string Member { get; } = member;
  public IReadOnlyList<MixinExpressionProperty> Properties { get; } = properties ?? [];
  public bool Parenthesized { get; } = parenthesized;
  public MixinSourceRange SourceRange { get; internal set; } = sourceRange;
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
) {
  internal MixinExpressionProperty(string name, string argument) : this(
    name, argument is null ? [] : [new MixinPropertyArgumentAst(argument, null, null)]
  ) { }

  public string Name { get; } = name;
  public IReadOnlyList<MixinPropertyArgumentAst> ParsedArguments { get; } = arguments ?? [];
  public IReadOnlyList<string> Arguments { get; } = [.. (arguments ?? []).Select(item => item.Literal)];
  public string Argument => Arguments.Count == 0 ? null : Arguments[0];
  public bool Negated { get; } = negated;
  public MixinSourceRange SourceRange { get; internal set; } = sourceRange;
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