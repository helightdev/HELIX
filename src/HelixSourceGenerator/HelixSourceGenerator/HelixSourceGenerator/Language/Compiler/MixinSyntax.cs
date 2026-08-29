using System.Collections.Generic;
namespace HelixSourceGenerator.Language;

public abstract class MixinSyntaxNode { protected MixinSyntaxNode(int line) => Line = line; internal int Line { get; } }

public sealed class MixinProgramSyntax {
  private readonly DirectiveInstruction[] _instructions;
  internal MixinProgramSyntax(string expression) {
    Source = expression ?? "";
    var parsed = MixinExpressionParser.ParseProgram(Source);
    _instructions = parsed.Instructions;
    Diagnostics = parsed.Diagnostics;
  }
  internal int Count => _instructions.Length;
  internal string Source { get; }
  internal IReadOnlyList<MixinParseDiagnostic> Diagnostics { get; }
  internal DirectiveInstruction Get(int index) => _instructions[index];
  internal IEnumerable<DirectiveInstruction> AvailableInstructions() => _instructions;
  internal void CollectConstants(MixinStringPoolBuilder pool) { pool.Intern(Source); foreach (var instruction in _instructions) instruction.CollectConstants(pool); }
}

public abstract class DirectiveInstruction : MixinSyntaxNode {
  protected DirectiveInstruction(int line, string command, IReadOnlyList<string> arguments, string operand, DirectiveOperandKind operandKind) : base(line) {
    Command = command; Arguments = arguments ?? []; Operand = operand ?? "";
    if (operandKind == DirectiveOperandKind.Boolean) BooleanExpression = MixinExpressionParser.ParseBooleanExpression(Operand);
    if (operandKind == DirectiveOperandKind.Value) ValueExpression = MixinExpressionParser.ParseValueExpression(Operand);
  }
  internal string Command { get; }
  internal IReadOnlyList<string> Arguments { get; }
  internal string Argument => Arguments.Count == 0 ? null : Arguments[0];
  internal string Operand { get; }
  internal IReadOnlyList<MixinExpressionReference> BooleanExpression { get; }
  internal IReadOnlyList<ValueExpressionPart> ValueExpression { get; }
  internal void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Command); pool.Intern(Operand); foreach (var argument in Arguments) pool.Intern(argument);
    foreach (var reference in BooleanExpression ?? []) reference.CollectConstants(pool);
    foreach (var part in ValueExpression ?? []) { if (part.Reference is null) pool.Intern(part.Literal); else part.Reference.CollectConstants(pool); }
  }
}

public sealed class EmptyDirectiveSyntax(int line) : DirectiveInstruction(line,null,null,"",DirectiveOperandKind.None);
public sealed class UnknownDirectiveSyntax(int line, string command, IReadOnlyList<string> arguments, string operand) : DirectiveInstruction(line,command,arguments,operand,DirectiveOperandKind.None);
public sealed class DirectiveInvocationSyntax(int line, DirectiveDefinition directive, IReadOnlyList<string> arguments, string operand) : DirectiveInstruction(line,directive.Name,arguments,operand,directive.OperandKind) {
  internal DirectiveDefinition Definition { get; } = directive;
}

public sealed class ScopeDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"SCOPE",a,o,DirectiveOperandKind.None);
public sealed class LabelDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"LABEL",a,o,DirectiveOperandKind.None);
public sealed class FunctionDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"FUNC",a,o,DirectiveOperandKind.None);
public sealed class CallDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"CALL",a,o,DirectiveOperandKind.Value);
public sealed class InlineDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"INLINE",a,o,DirectiveOperandKind.None);
public sealed class EndDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"END",a,o,DirectiveOperandKind.None);
public sealed class MatchDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"MATCH",a,o,DirectiveOperandKind.Boolean);
public sealed class AssertDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"ASSERT",a,o,DirectiveOperandKind.Boolean);
public sealed class CodeDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"CODE",a,o,DirectiveOperandKind.Value);
public sealed class MixinDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"MIXIN",a,o,DirectiveOperandKind.Value);
public sealed class UsingDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"USING",a,o,DirectiveOperandKind.Value);
public sealed class LogDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"LOG",a,o,DirectiveOperandKind.Value);
public sealed class LocalDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"LOCAL",a,o,DirectiveOperandKind.Value);
public sealed class VariableDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"VAR",a,o,DirectiveOperandKind.Value);
public sealed class CarryDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"CARRY",a,o,DirectiveOperandKind.Value);
public sealed class ReturnDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"RETURN",a,o,DirectiveOperandKind.Value);
public sealed class GotoDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"GOTO",a,o,DirectiveOperandKind.None);
public sealed class SkipDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"SKIP",a,o,DirectiveOperandKind.None);
public sealed class FailDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"FAIL",a,o,DirectiveOperandKind.Value);
public sealed class AnnotationDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"ANNOTATION",a,o,DirectiveOperandKind.None);
public sealed class PreludeDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"PRELUDE",a,o,DirectiveOperandKind.None);
public sealed class DefineTargetDirectiveSyntax(int l, IReadOnlyList<string> a, string o) : DirectiveInstruction(l,"DEFINE_TARGET",a,o,DirectiveOperandKind.None);

internal sealed record ValueExpressionPart(string Literal, MixinExpressionReference Reference);
