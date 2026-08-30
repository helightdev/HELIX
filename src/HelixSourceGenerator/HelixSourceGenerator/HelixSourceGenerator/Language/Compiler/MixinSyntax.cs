using System.Collections.Generic;
using System.Linq;

namespace HelixSourceGenerator.Language.Compiler;

public abstract class MixinSyntaxNode(int line) {
  internal int Line { get; } = line;
}

public sealed class MixinProgramSyntax {
  private readonly DirectiveInstruction[] _instructions;

  internal MixinProgramSyntax(string expression) {
    var parsed = MixinExpressionParser.ParseProgram(expression ?? "");
    _instructions = parsed.Instructions;
    Diagnostics = parsed.Diagnostics;
  }

  internal MixinProgramSyntax(IEnumerable<DirectiveInstruction> instructions) {
    _instructions = (instructions ?? []).ToArray();
    Diagnostics = [];
  }

  internal int Count => _instructions.Length;
  internal IReadOnlyList<MixinParseDiagnostic> Diagnostics { get; }

  internal DirectiveInstruction Get(int index) {
    return _instructions[index];
  }

  internal IEnumerable<DirectiveInstruction> AvailableInstructions() {
    return _instructions;
  }

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var item in _instructions) {
      pool.Intern(MixinSyntaxFacts.Command(item));
      item.CollectConstants(pool);
    }
  }
}

public abstract class DirectiveInstruction(int line) : MixinSyntaxNode(line) {
  internal abstract void CollectConstants(MixinStringPoolBuilder pool);

  private protected static void Collect(
    MixinStringPoolBuilder pool, string value, IReadOnlyList<IMixinValue> expression = null
  ) {
    pool.Intern(value);
    foreach (var part in expression ?? []) {
      if (part.Reference is null) pool.Intern(part.Literal);
      else part.Reference.CollectConstants(pool);
    }
  }
}

public abstract class ValueDirectiveSyntax(int line, IReadOnlyList<IMixinValue> expression)
  : DirectiveInstruction(line) {
  internal IReadOnlyList<IMixinValue> Expression { get; } = expression ?? [];

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    Collect(pool, null, Expression);
  }
}

public abstract class BooleanDirectiveSyntax(int line, IReadOnlyList<MixinExpressionReference> expression)
  : DirectiveInstruction(line) {
  internal IReadOnlyList<MixinExpressionReference> Expression { get; } = expression ?? [];

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var item in Expression) item.CollectConstants(pool);
  }
}

public sealed class EmptyDirectiveSyntax(int line) : DirectiveInstruction(line) {
  internal override void CollectConstants(MixinStringPoolBuilder pool) { }
}

public sealed class UnknownDirectiveSyntax(int line, string command) : DirectiveInstruction(line) {
  internal string Name { get; } = command;

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Name);
  }
}

public sealed class DirectiveInvocationSyntax(int line, DirectiveDefinition definition,
  IReadOnlyList<DirectiveArgumentSyntax> arguments, IReadOnlyList<IMixinValue> operand
) : ValueDirectiveSyntax(line, operand) {
  internal DirectiveDefinition Definition { get; } = definition;
  internal IReadOnlyList<DirectiveArgumentSyntax> ParsedArguments { get; } = arguments;
  internal IReadOnlyList<string> Arguments { get; } = [.. arguments.Select(MixinSyntaxRenderer.RenderArgument)];
  internal string Argument => Arguments.Count == 0 ? null : Arguments[0];

  internal override void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Definition.Name);
    foreach (var item in Arguments) pool.Intern(item);
    base.CollectConstants(pool);
  }
}

public sealed class ScopeDirectiveSyntax(int l, string label) : DirectiveInstruction(l) {
  internal string Label { get; } = label;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Label);
  }
}

public sealed class LabelDirectiveSyntax(int l, string name) : DirectiveInstruction(l) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class FunctionDirectiveSyntax(int l, string name) : DirectiveInstruction(l) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class CallDirectiveSyntax(int l, string function, string returnLocal,
  IReadOnlyList<IMixinValue> parameter
) : ValueDirectiveSyntax(l, parameter) {
  internal string Function { get; } = function;
  internal string ReturnLocal { get; } = returnLocal;
}

public sealed class InlineDirectiveSyntax(int l, string name) : DirectiveInstruction(l) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class EndDirectiveSyntax(int l) : DirectiveInstruction(l) {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class MatchDirectiveSyntax(int l, string failureLabel, IReadOnlyList<MixinExpressionReference> condition)
  : BooleanDirectiveSyntax(l, condition) {
  internal string FailureLabel { get; } = failureLabel;
}

public sealed class AssertDirectiveSyntax(int l, IReadOnlyList<MixinExpressionReference> condition)
  : BooleanDirectiveSyntax(l, condition);

public sealed class CodeDirectiveSyntax(int l, MixinExpressionOutputTarget target, string injectionTarget,
  IReadOnlyList<IMixinValue> code
) : ValueDirectiveSyntax(l, code) {
  internal MixinExpressionOutputTarget Target { get; } = target;
  internal string InjectionTarget { get; } = injectionTarget;
}

public sealed class MixinDirectiveSyntax(int l, DirectiveArgumentSyntax target, DirectiveArgumentSyntax priority,
  IReadOnlyList<IMixinValue> code
) : ValueDirectiveSyntax(l, code) {
  internal DirectiveArgumentSyntax Target { get; } = target;
  internal DirectiveArgumentSyntax Priority { get; } = priority;
}

public sealed class UsingDirectiveSyntax(int l, IReadOnlyList<IMixinValue> value)
  : ValueDirectiveSyntax(l, value);

public sealed class LogDirectiveSyntax(int l, IReadOnlyList<IMixinValue> message)
  : ValueDirectiveSyntax(l, message);

public sealed class LocalDirectiveSyntax(int l, string name, IReadOnlyList<IMixinValue> value)
  : ValueDirectiveSyntax(l, value) {
  internal string Name { get; } = name;
}

public sealed class VariableDirectiveSyntax(int l, string name, IReadOnlyList<IMixinValue> value)
  : ValueDirectiveSyntax(l, value) {
  internal string Name { get; } = name;
}

public sealed class CarryDirectiveSyntax(int l, string label, IReadOnlyList<IMixinValue> value)
  : ValueDirectiveSyntax(l, value) {
  internal string Label { get; } = label;
}

public sealed class ReturnDirectiveSyntax(int l, IReadOnlyList<IMixinValue> value)
  : ValueDirectiveSyntax(l, value);

public sealed class GotoDirectiveSyntax(int l, string label) : DirectiveInstruction(l) {
  internal string Label { get; } = label;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Label);
  }
}

public sealed class SkipDirectiveSyntax(int l) : DirectiveInstruction(l) {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class FailDirectiveSyntax(int l, IReadOnlyList<IMixinValue> message)
  : ValueDirectiveSyntax(l, message);

public sealed class AnnotationDirectiveSyntax(int l, string name) : DirectiveInstruction(l) {
  internal string Name { get; } = name;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
  }
}

public sealed class PreludeDirectiveSyntax(int l) : DirectiveInstruction(l) {
  internal override void CollectConstants(MixinStringPoolBuilder p) { }
}

public sealed class DefineTargetDirectiveSyntax(int l, string name, string value) : DirectiveInstruction(l) {
  internal string Name { get; } = name;
  internal string Value { get; } = value;

  internal override void CollectConstants(MixinStringPoolBuilder p) {
    p.Intern(Name);
    p.Intern(Value);
  }
}

public sealed record IMixinValue(
  string Literal, MixinExpressionReference Reference, bool Verbatim = false
);

public sealed record DirectiveArgumentSyntax(string Literal, IReadOnlyList<IMixinValue> Expression) {
  internal bool IsDynamic => Expression is not null;
}

public sealed record MixinPropertyArgumentSyntax(
  string Literal,
  IReadOnlyList<IMixinValue> ValueExpression,
  IReadOnlyList<MixinExpressionReference> BooleanExpression
);

internal static class MixinSyntaxFacts {
  internal static string Command(DirectiveInstruction n) {
    return n switch {
      EmptyDirectiveSyntax => null, UnknownDirectiveSyntax x => x.Name,
      DirectiveInvocationSyntax x => x.Definition.Name,
      ScopeDirectiveSyntax => "SCOPE", LabelDirectiveSyntax => "LABEL", FunctionDirectiveSyntax => "FUNC",
      CallDirectiveSyntax => "CALL", InlineDirectiveSyntax => "INLINE", EndDirectiveSyntax => "END",
      MatchDirectiveSyntax => "MATCH", AssertDirectiveSyntax => "ASSERT", CodeDirectiveSyntax => "CODE",
      MixinDirectiveSyntax => "MIXIN", UsingDirectiveSyntax => "USING", LogDirectiveSyntax => "LOG",
      LocalDirectiveSyntax => "LOCAL", VariableDirectiveSyntax => "VAR", CarryDirectiveSyntax => "CARRY",
      ReturnDirectiveSyntax => "RETURN", GotoDirectiveSyntax => "GOTO", SkipDirectiveSyntax => "SKIP",
      FailDirectiveSyntax => "FAIL", AnnotationDirectiveSyntax => "ANNOTATION", PreludeDirectiveSyntax => "PRELUDE",
      DefineTargetDirectiveSyntax => "DEFINE_TARGET", _ => null
    };
  }

  internal static IReadOnlyList<string> Arguments(DirectiveInstruction n) {
    return n switch {
      DirectiveInvocationSyntax x => x.Arguments,
      ScopeDirectiveSyntax { Label: not null } x => [x.Label], LabelDirectiveSyntax x => [x.Name],
      FunctionDirectiveSyntax x => [x.Name], InlineDirectiveSyntax x => [x.Name],
      CallDirectiveSyntax { ReturnLocal: not null } x => [x.ReturnLocal, x.Function],
      CallDirectiveSyntax x => [x.Function],
      MatchDirectiveSyntax { FailureLabel: not null } x => [x.FailureLabel],
      CodeDirectiveSyntax { Target: MixinExpressionOutputTarget.Injection } x => [x.InjectionTarget],
      CodeDirectiveSyntax { Target: not MixinExpressionOutputTarget.Target } x => [
        x.Target.ToString().ToUpperInvariant()
      ],
      MixinDirectiveSyntax { Priority: not null } x => [
        MixinSyntaxRenderer.RenderArgument(x.Target), MixinSyntaxRenderer.RenderArgument(x.Priority)
      ],
      MixinDirectiveSyntax x => [MixinSyntaxRenderer.RenderArgument(x.Target)],
      LocalDirectiveSyntax x => [x.Name], VariableDirectiveSyntax x => [x.Name], CarryDirectiveSyntax x => [x.Label],
      GotoDirectiveSyntax x => [x.Label], AnnotationDirectiveSyntax x => [x.Name],
      DefineTargetDirectiveSyntax x => [x.Name, x.Value], _ => []
    };
  }

  internal static string Operand(DirectiveInstruction n) {
    return n switch {
      ValueDirectiveSyntax x => MixinSyntaxRenderer.RenderValue(x.Expression),
      BooleanDirectiveSyntax x => MixinSyntaxRenderer.RenderBoolean(x.Expression), _ => ""
    };
  }
}