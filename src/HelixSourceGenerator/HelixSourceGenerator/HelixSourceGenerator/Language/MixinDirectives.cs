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

internal sealed class PutDirective : ValueDirective {
  internal PutDirective() : base("PUT", DirectiveOpcode.Put) { }
  protected override int MaximumArguments => 2;

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (!base.Validate(arguments, operand, out error)) return false;
    if (arguments.Count == 2) return true;
    error = "PUT requires a local name and key";
    return false;
  }
}

internal sealed class PropStructDirective : ValueDirective {
  internal PropStructDirective() : base("PROP_STRUCT", DirectiveOpcode.PropStruct) { }
  protected override int MaximumArguments => 4;

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (!base.Validate(arguments, operand, out error)) return false;
    if (arguments.Count < 2 || string.IsNullOrEmpty(arguments[0]) || string.IsNullOrEmpty(arguments[1])) {
      error = "PROP_STRUCT requires a struct name and local name";
      return false;
    }
    var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 2; index < arguments.Count; index++) {
      var flag = arguments[index];
      if (MixinExpressionParser.IsDynamicArgument(flag)) continue;
      if (!string.Equals(flag, "datatype", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(flag, "noGenerate", StringComparison.OrdinalIgnoreCase)) {
        error = "unknown PROP_STRUCT flag '" + flag + "'";
        return false;
      }
      if (!flags.Add(flag)) {
        error = "PROP_STRUCT flag '" + flag + "' was specified more than once";
        return false;
      }
    }
    error = null;
    return true;
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
  public static readonly DirectiveDefinition ResolveMixin = new NamedDirective(
    "RESOLVE_MIXIN", DirectiveOpcode.ResolveMixin, DirectiveOperandKind.Value
  );
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
  public static readonly DirectiveDefinition Return = new ValueDirective("RETURN", DirectiveOpcode.Return);
  public static readonly DirectiveDefinition Goto = new NamedDirective("GOTO", DirectiveOpcode.Goto);
  public static readonly DirectiveDefinition Skip = new MarkerDirective("SKIP", DirectiveOpcode.Skip);
  public static readonly DirectiveDefinition Fail = new ValueDirective("FAIL", DirectiveOpcode.Fail);

  // Expanded directives live in a registry so adding one does not grow intrinsic dispatch.
  public static readonly IReadOnlyDictionary<string, DirectiveDefinition> Expanded =
    new Dictionary<string, DirectiveDefinition>(StringComparer.Ordinal) {
      ["LOG"] = new ValueDirective("LOG", DirectiveOpcode.Log), ["PROP_STRUCT"] = new PropStructDirective(),
      ["AUGMENT_STRUCT"] = new NamedDirective(
        "AUGMENT_STRUCT", DirectiveOpcode.AugmentStruct, DirectiveOperandKind.Value
      ),
      ["PUSH"] = new NamedDirective("PUSH", DirectiveOpcode.Push, DirectiveOperandKind.Value),
      ["PUT"] = new PutDirective(), ["ANNOTATION"] = new NamedDirective("ANNOTATION", DirectiveOpcode.Annotation),
      ["PRELUDE"] = new MarkerDirective("PRELUDE", DirectiveOpcode.Prelude),
      ["DEFINE_TARGET"] = new DefineTargetDirective()
    };

  public static bool TryGet(string name, out DirectiveDefinition definition) {
    definition = name switch {
      "SCOPE" => Scope, "LABEL" => Label, "FUNC" => Function, "CALL" => Call, "INLINE" => Inline, "END" => End,
      "MATCH" => Match, "ASSERT" => Assert, "CODE" => Code, "MIXIN" => Mixin,
      "RESOLVE_MIXIN" => ResolveMixin, "USING" => Using,
      "LOCAL" => Local, "VAR" => Variable, "CARRY" => Carry,
      "RETURN" => Return, "GOTO" => Goto, "SKIP" => Skip, "FAIL" => Fail,
      _ => null
    };
    return definition is not null || Expanded.TryGetValue(name ?? "", out definition);
  }
}
