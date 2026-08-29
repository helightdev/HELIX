using System.Collections.Generic;

namespace HelixSourceGenerator.Language.Compiler;

public abstract class MixinSyntaxNode(int line) { internal int Line { get; } = line; }

public sealed class MixinProgramSyntax {
  private readonly DirectiveInstruction[] _instructions;
  internal MixinProgramSyntax(string expression) {
    Source = expression ?? ""; var parsed = MixinExpressionParser.ParseProgram(Source);
    _instructions = parsed.Instructions; Diagnostics = parsed.Diagnostics;
  }
  internal int Count => _instructions.Length;
  internal string Source { get; }
  internal IReadOnlyList<MixinParseDiagnostic> Diagnostics { get; }
  internal DirectiveInstruction Get(int index) => _instructions[index];
  internal IEnumerable<DirectiveInstruction> AvailableInstructions() => _instructions;
  internal void CollectConstants(MixinStringPoolBuilder pool) { pool.Intern(Source); foreach (var item in _instructions) { pool.Intern(item.Command); item.CollectConstants(pool); } }
}

public abstract class DirectiveInstruction(int line) : MixinSyntaxNode(line) {
  // Transitional compiler view. These values are projected from typed fields; built-in nodes do not store them.
  internal string Command => MixinSyntaxFacts.Command(this);
  internal IReadOnlyList<string> Arguments => MixinSyntaxFacts.Arguments(this);
  internal string Argument => Arguments.Count == 0 ? null : Arguments[0];
  internal string Operand => MixinSyntaxFacts.Operand(this);
  internal IReadOnlyList<ValueExpressionPart> ValueExpression => (this as ValueDirectiveSyntax)?.Expression;
  internal IReadOnlyList<MixinExpressionReference> BooleanExpression => (this as BooleanDirectiveSyntax)?.Expression;
  internal abstract void CollectConstants(MixinStringPoolBuilder pool);
  private protected static void Collect(MixinStringPoolBuilder pool, string value, IReadOnlyList<ValueExpressionPart> expression = null) {
    pool.Intern(value);
    foreach (var part in expression ?? []) { if (part.Reference is null) pool.Intern(part.Literal); else part.Reference.CollectConstants(pool); }
  }
}
public abstract class ValueDirectiveSyntax(int line, string value) : DirectiveInstruction(line) {
  internal string Value { get; } = value ?? "";
  internal IReadOnlyList<ValueExpressionPart> Expression { get; } = MixinExpressionParser.ParseValueExpression(value ?? "");
  internal override void CollectConstants(MixinStringPoolBuilder pool) => Collect(pool, Value, Expression);
}
public abstract class BooleanDirectiveSyntax(int line, string value) : DirectiveInstruction(line) {
  internal string Value { get; } = value ?? "";
  internal IReadOnlyList<MixinExpressionReference> Expression { get; } = MixinExpressionParser.ParseBooleanExpression(value ?? "");
  internal override void CollectConstants(MixinStringPoolBuilder pool) { pool.Intern(Value); foreach (var item in Expression) item.CollectConstants(pool); }
}

public sealed class EmptyDirectiveSyntax(int line) : DirectiveInstruction(line) { internal override void CollectConstants(MixinStringPoolBuilder pool) { } }
public sealed class UnknownDirectiveSyntax(int line, string command, IReadOnlyList<string> arguments, string operand) : DirectiveInstruction(line) {
  internal new string Command { get; } = command; internal new IReadOnlyList<string> Arguments { get; } = arguments; internal new string Operand { get; } = operand;
  internal override void CollectConstants(MixinStringPoolBuilder pool) { pool.Intern(Command); Collect(pool, Operand); foreach (var item in Arguments) pool.Intern(item); }
}
public sealed class DirectiveInvocationSyntax(int line, DirectiveDefinition definition, IReadOnlyList<string> arguments, string operand) : ValueDirectiveSyntax(line, operand) {
  internal DirectiveDefinition Definition { get; } = definition; internal new IReadOnlyList<string> Arguments { get; } = arguments;
  internal new string Argument => Arguments.Count == 0 ? null : Arguments[0];
  internal override void CollectConstants(MixinStringPoolBuilder pool) { pool.Intern(Definition.Name); foreach (var item in Arguments) pool.Intern(item); base.CollectConstants(pool); }
}

public sealed class ScopeDirectiveSyntax(int l, string label) : DirectiveInstruction(l) { internal string Label { get; } = label; internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Label); }
public sealed class LabelDirectiveSyntax(int l, string name) : DirectiveInstruction(l) { internal string Name { get; } = name; internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Name); }
public sealed class FunctionDirectiveSyntax(int l, string name) : DirectiveInstruction(l) { internal string Name { get; } = name; internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Name); }
public sealed class CallDirectiveSyntax(int l, string function, string returnLocal, string parameter) : ValueDirectiveSyntax(l,parameter) { internal string Function { get; } = function; internal string ReturnLocal { get; } = returnLocal; }
public sealed class InlineDirectiveSyntax(int l, string name) : DirectiveInstruction(l) { internal string Name { get; } = name; internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Name); }
public sealed class EndDirectiveSyntax(int l) : DirectiveInstruction(l) { internal override void CollectConstants(MixinStringPoolBuilder p) { } }
public sealed class MatchDirectiveSyntax(int l, string failureLabel, string condition) : BooleanDirectiveSyntax(l,condition) { internal string FailureLabel { get; } = failureLabel; }
public sealed class AssertDirectiveSyntax(int l, string condition) : BooleanDirectiveSyntax(l,condition);
public sealed class CodeDirectiveSyntax(int l, string target, string code) : ValueDirectiveSyntax(l,code) { internal string Target { get; } = target; }
public sealed class MixinDirectiveSyntax(int l, string target, string priority, string code) : ValueDirectiveSyntax(l,code) { internal string Target { get; } = target; internal string Priority { get; } = priority; }
public sealed class UsingDirectiveSyntax(int l, string value) : ValueDirectiveSyntax(l,value);
public sealed class LogDirectiveSyntax(int l, string message) : ValueDirectiveSyntax(l,message);
public sealed class LocalDirectiveSyntax(int l, string name, string value) : ValueDirectiveSyntax(l,value) { internal string Name { get; } = name; }
public sealed class VariableDirectiveSyntax(int l, string name, string value) : ValueDirectiveSyntax(l,value) { internal string Name { get; } = name; }
public sealed class CarryDirectiveSyntax(int l, string label, string value) : ValueDirectiveSyntax(l,value) { internal string Label { get; } = label; }
public sealed class ReturnDirectiveSyntax(int l, string value) : ValueDirectiveSyntax(l,value);
public sealed class GotoDirectiveSyntax(int l, string label) : DirectiveInstruction(l) { internal string Label { get; } = label; internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Label); }
public sealed class SkipDirectiveSyntax(int l) : DirectiveInstruction(l) { internal override void CollectConstants(MixinStringPoolBuilder p) { } }
public sealed class FailDirectiveSyntax(int l, string message) : ValueDirectiveSyntax(l,message);
public sealed class AnnotationDirectiveSyntax(int l, string name) : DirectiveInstruction(l) { internal string Name { get; } = name; internal override void CollectConstants(MixinStringPoolBuilder p) => p.Intern(Name); }
public sealed class PreludeDirectiveSyntax(int l) : DirectiveInstruction(l) { internal override void CollectConstants(MixinStringPoolBuilder p) { } }
public sealed class DefineTargetDirectiveSyntax(int l, string name, string value) : DirectiveInstruction(l) { internal string Name { get; } = name; internal string Value { get; } = value; internal override void CollectConstants(MixinStringPoolBuilder p) { p.Intern(Name); p.Intern(Value); } }

internal sealed record ValueExpressionPart(string Literal, MixinExpressionReference Reference);

internal static class MixinSyntaxFacts {
  internal static string Command(DirectiveInstruction n) => n switch {
    EmptyDirectiveSyntax => null, UnknownDirectiveSyntax x => x.Command, DirectiveInvocationSyntax x => x.Definition.Name,
    ScopeDirectiveSyntax => "SCOPE", LabelDirectiveSyntax => "LABEL", FunctionDirectiveSyntax => "FUNC",
    CallDirectiveSyntax => "CALL", InlineDirectiveSyntax => "INLINE", EndDirectiveSyntax => "END",
    MatchDirectiveSyntax => "MATCH", AssertDirectiveSyntax => "ASSERT", CodeDirectiveSyntax => "CODE",
    MixinDirectiveSyntax => "MIXIN", UsingDirectiveSyntax => "USING", LogDirectiveSyntax => "LOG",
    LocalDirectiveSyntax => "LOCAL", VariableDirectiveSyntax => "VAR", CarryDirectiveSyntax => "CARRY",
    ReturnDirectiveSyntax => "RETURN", GotoDirectiveSyntax => "GOTO", SkipDirectiveSyntax => "SKIP",
    FailDirectiveSyntax => "FAIL", AnnotationDirectiveSyntax => "ANNOTATION", PreludeDirectiveSyntax => "PRELUDE",
    DefineTargetDirectiveSyntax => "DEFINE_TARGET", _ => null
  };
  internal static IReadOnlyList<string> Arguments(DirectiveInstruction n) => n switch {
    UnknownDirectiveSyntax x => x.Arguments, DirectiveInvocationSyntax x => x.Arguments,
    ScopeDirectiveSyntax { Label: not null } x => [x.Label], LabelDirectiveSyntax x => [x.Name], FunctionDirectiveSyntax x => [x.Name], InlineDirectiveSyntax x => [x.Name],
    CallDirectiveSyntax { ReturnLocal: not null } x => [x.ReturnLocal, x.Function], CallDirectiveSyntax x => [x.Function],
    MatchDirectiveSyntax { FailureLabel: not null } x => [x.FailureLabel], CodeDirectiveSyntax { Target: not null } x => [x.Target],
    MixinDirectiveSyntax { Priority: not null } x => [x.Target, x.Priority], MixinDirectiveSyntax x => [x.Target],
    LocalDirectiveSyntax x => [x.Name], VariableDirectiveSyntax x => [x.Name], CarryDirectiveSyntax x => [x.Label],
    GotoDirectiveSyntax x => [x.Label], AnnotationDirectiveSyntax x => [x.Name],
    DefineTargetDirectiveSyntax x => [x.Name, x.Value], _ => []
  };
  internal static string Operand(DirectiveInstruction n) => n switch {
    UnknownDirectiveSyntax x => x.Operand, ValueDirectiveSyntax x => x.Value, BooleanDirectiveSyntax x => x.Value, _ => ""
  };
}
