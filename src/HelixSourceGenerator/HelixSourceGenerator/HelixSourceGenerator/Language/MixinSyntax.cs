using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HelixSourceGenerator.Language;

public abstract class MixinSyntaxNode {
  protected MixinSyntaxNode(int line) {
    Line = line;
  }

  internal int Line { get; }
}

/// <summary>A typed invocation in a reference pipeline (for example <c>:replace&lt;a&gt;&lt;b&gt;</c>).</summary>
public sealed class MixinProgramSyntax {
  private readonly DirectiveInstruction[] _instructions;
  private readonly string[] _lines;

  internal MixinProgramSyntax(string expression) {
    Source = expression ?? "";
    _lines = MixinExpressionParser.SplitLines(expression ?? "");
    _instructions = new DirectiveInstruction[_lines.Length];
  }

  internal int Count => _lines.Length;
  internal string Source { get; }

  internal DirectiveInstruction Get(int index) {
    var instruction = Volatile.Read(ref _instructions[index]);
    if (instruction is not null) return instruction;
    var parsed = MixinExpressionParser.ParseDirective(_lines[index], index + 1);
    return Interlocked.CompareExchange(
      ref _instructions[index], parsed, null
    ) ?? parsed;
  }

  internal void ParseAll() {
    for (var index = 0; index < Count; index++) Get(index);
  }

  internal IEnumerable<DirectiveInstruction> AvailableInstructions() {
    return _instructions.Where(instruction => instruction is not null);
  }

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Source);
    for (var index = 0; index < Count; index++) Get(index).CollectConstants(pool);
  }
}

public enum DirectiveOpcode {
  None,
  Scope,
  Function,
  Call,
  Inline,
  End,
  Match,
  Assert,
  Code,
  Mixin,
  ResolveMixin,
  Using,
  Log,
  Local,
  Variable,
  Carry,
  PropStruct,
  AugmentStruct,
  Put,
  Push,
  Return,
  Goto,
  Skip,
  Fail,
  Annotation,
  Prelude,
  DefineTarget
}

public sealed class DirectiveInstruction : MixinSyntaxNode {
  internal DirectiveInstruction(
    int line,
    DirectiveDefinition directive,
    IReadOnlyList<string> arguments,
    string operand,
    string error
  ) : base(line) {
    Directive = directive;
    Arguments = arguments ?? [];
    Operand = operand ?? "";
    Error = error;
    if (error is null && directive?.OperandKind == DirectiveOperandKind.Boolean)
      BooleanExpression = MixinExpressionParser.ParseBooleanExpression(Operand);
    if (error is null && directive?.OperandKind == DirectiveOperandKind.Value)
      ValueExpression = MixinExpressionParser.ParseValueExpression(Operand);
  }

  internal DirectiveDefinition Directive { get; }
  internal DirectiveOpcode Opcode => Directive?.Opcode ?? DirectiveOpcode.None;
  internal string Command => Directive?.Name;
  internal IReadOnlyList<string> Arguments { get; }
  internal string Argument => Arguments.Count == 0 ? null : Arguments[0];
  internal string Operand { get; }
  internal string Error { get; }
  internal IReadOnlyList<MixinExpressionReference> BooleanExpression { get; }
  internal IReadOnlyList<ValueExpressionPart> ValueExpression { get; }

  internal void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Command);
    pool.Intern(Operand);
    foreach (var argument in Arguments) pool.Intern(argument);
    foreach (var reference in BooleanExpression ?? [])
      reference.CollectConstants(pool);
    foreach (var part in ValueExpression ?? []) {
      if (part.Reference is null) pool.Intern(part.Literal);
      else part.Reference.CollectConstants(pool);
    }
  }
}

internal sealed record ValueExpressionPart(string Literal, MixinExpressionReference Reference);