using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixSourceGenerator.Language;

public enum DirectiveOperandKind { None, Value, Boolean }

/// <summary>Definition and compiler contract for a source-language directive.</summary>
public abstract class DirectiveDefinition {
  protected DirectiveDefinition(string name, DirectiveOpcode opcode, DirectiveOperandKind operandKind) {
    Name = name;
    Opcode = opcode;
    OperandKind = operandKind;
  }

  internal string Name { get; }
  internal DirectiveOpcode Opcode { get; }
  internal DirectiveOperandKind OperandKind { get; }

  protected virtual int MaximumArguments => 1;

  internal virtual bool Validate(
    IReadOnlyList<string> arguments, string operand, out string error
  ) {
    error = null;
    if (arguments.Count <= MaximumArguments) return true;
    error = Name + " accepts at most " + MaximumArguments +
      (MaximumArguments == 1 ? " argument" : " arguments");
    return false;
  }
}

internal class MarkerDirective : DirectiveDefinition {
  internal MarkerDirective(string name, DirectiveOpcode opcode) : base(name, opcode, DirectiveOperandKind.None) { }
}

internal class NamedDirective : DirectiveDefinition {
  internal NamedDirective(
    string name, DirectiveOpcode opcode, DirectiveOperandKind operandKind = DirectiveOperandKind.None
  ) : base(name, opcode, operandKind) { }

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (!base.Validate(arguments, operand, out error)) return false;
    if (arguments.Count == 1 && !string.IsNullOrEmpty(arguments[0])) return true;
    error = Name + " requires a name";
    return false;
  }
}

internal class ValueDirective : DirectiveDefinition {
  internal ValueDirective(string name, DirectiveOpcode opcode) : base(name, opcode, DirectiveOperandKind.Value) { }
}

internal sealed class BooleanDirective : DirectiveDefinition {
  internal BooleanDirective(string name, DirectiveOpcode opcode) : base(name, opcode, DirectiveOperandKind.Boolean) { }
}

internal sealed class CallDirective : DirectiveDefinition {
  internal CallDirective() : base("CALL", DirectiveOpcode.Call, DirectiveOperandKind.Value) { }
  protected override int MaximumArguments => 2;

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count is 1 or 2 && arguments.All(item => !string.IsNullOrEmpty(item))) {
      error = null;
      return true;
    }
    error = "CALL requires a function label and optionally a return local";
    return false;
  }
}

internal sealed class MixinDirective : ValueDirective {
  internal MixinDirective() : base("MIXIN", DirectiveOpcode.Mixin) { }
  protected override int MaximumArguments => 2;

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (!base.Validate(arguments, operand, out error)) return false;
    if (arguments.Count != 0 && !string.IsNullOrEmpty(arguments[0])) return true;
    error = "MIXIN requires a target";
    return false;
  }
}

internal sealed class DefineTargetDirective : DirectiveDefinition {
  internal DefineTargetDirective() : base("DEFINE_TARGET", DirectiveOpcode.DefineTarget, DirectiveOperandKind.None) { }

  protected override int MaximumArguments => 2;

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count == 2 && arguments.All(item => !string.IsNullOrWhiteSpace(item))) {
      error = null;
      return true;
    }
    error = "DEFINE_TARGET requires a name and value";
    return false;
  }
}

public static class DirectiveLibrary {
  public static readonly DirectiveDefinition Scope = new MarkerDirective("SCOPE", DirectiveOpcode.Scope);
  public static readonly DirectiveDefinition Label = new NamedDirective("LABEL", DirectiveOpcode.Label);
  public static readonly DirectiveDefinition Function = new NamedDirective("FUNC", DirectiveOpcode.Function);
  public static readonly DirectiveDefinition Call = new CallDirective();
  public static readonly DirectiveDefinition Inline = new NamedDirective("INLINE", DirectiveOpcode.Inline);
  public static readonly DirectiveDefinition End = new MarkerDirective("END", DirectiveOpcode.End);
  public static readonly DirectiveDefinition Match = new BooleanDirective("MATCH", DirectiveOpcode.Match);
  public static readonly DirectiveDefinition Assert = new BooleanDirective("ASSERT", DirectiveOpcode.Assert);
  public static readonly DirectiveDefinition Code = new ValueDirective("CODE", DirectiveOpcode.Code);
  public static readonly DirectiveDefinition Mixin = new MixinDirective();
  public static readonly DirectiveDefinition Using = new ValueDirective("USING", DirectiveOpcode.Using);
  public static readonly DirectiveDefinition Local = new NamedDirective(
    "LOCAL", DirectiveOpcode.Local, DirectiveOperandKind.Value
  );
  public static readonly DirectiveDefinition Variable = new NamedDirective(
    "VAR", DirectiveOpcode.Variable, DirectiveOperandKind.Value
  );
  public static readonly DirectiveDefinition Carry = new NamedDirective(
    "CARRY", DirectiveOpcode.Carry, DirectiveOperandKind.Value
  );
  public static readonly DirectiveDefinition Log = new ValueDirective("LOG", DirectiveOpcode.Log);
  public static readonly DirectiveDefinition Annotation = new NamedDirective(
    "ANNOTATION", DirectiveOpcode.Annotation
  );
  public static readonly DirectiveDefinition Prelude = new MarkerDirective(
    "PRELUDE", DirectiveOpcode.Prelude
  );
  public static readonly DirectiveDefinition DefineTarget = new DefineTargetDirective();
  public static readonly DirectiveDefinition Return = new ValueDirective("RETURN", DirectiveOpcode.Return);
  public static readonly DirectiveDefinition Goto = new NamedDirective("GOTO", DirectiveOpcode.Goto);
  public static readonly DirectiveDefinition Skip = new MarkerDirective("SKIP", DirectiveOpcode.Skip);
  public static readonly DirectiveDefinition Fail = new ValueDirective("FAIL", DirectiveOpcode.Fail);

  public static bool TryGet(string name, out DirectiveDefinition definition) {
    definition = name switch {
      "SCOPE" => Scope, "LABEL" => Label, "FUNC" => Function, "CALL" => Call, "INLINE" => Inline, "END" => End,
      "MATCH" => Match, "ASSERT" => Assert, "CODE" => Code, "MIXIN" => Mixin,
      "USING" => Using,
      "LOCAL" => Local, "VAR" => Variable, "CARRY" => Carry, "LOG" => Log,
      "ANNOTATION" => Annotation, "PRELUDE" => Prelude,
      "DEFINE_TARGET" => DefineTarget,
      "RETURN" => Return, "GOTO" => Goto, "SKIP" => Skip, "FAIL" => Fail,
      _ => null
    };
    if (definition is not null) return true;
    if (DirectiveFunctionLibrary.TryGet(name, out var function)) {
      definition = function;
      return true;
    }
    return false;
  }
}
