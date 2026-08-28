using System;
using System.Collections.Generic;
using System.Linq;

namespace HELIX.SourceGen.Expressions;

internal enum DirectiveOperandKind { None, Value, Boolean }

/// <summary>Definition and compiler contract for a source-language directive.</summary>
internal abstract class DirectiveDefinition {
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
  internal MarkerDirective(string name, DirectiveOpcode opcode) : base(
    name, opcode, DirectiveOperandKind.None
  ) { }
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
  internal ValueDirective(string name, DirectiveOpcode opcode) : base(
    name, opcode, DirectiveOperandKind.Value
  ) { }
}

internal sealed class BooleanDirective : DirectiveDefinition {
  internal BooleanDirective(string name, DirectiveOpcode opcode) : base(
    name, opcode, DirectiveOperandKind.Boolean
  ) { }
}

internal sealed class DumpDirective : MarkerDirective {
  internal DumpDirective() : base("DUMP", DirectiveOpcode.Dump) { }

  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (!base.Validate(arguments, operand, out error)) return false;
    var kind = arguments.Count == 0 ? null : arguments[0];
    if (string.Equals(kind, "STATE", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(kind, "BUFFER", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(kind, "AST", StringComparison.OrdinalIgnoreCase)) return true;
    error = "DUMP requires STATE, BUFFER or AST";
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

internal static class DirectiveLibrary {
  private static readonly IReadOnlyDictionary<string, DirectiveDefinition> Definitions =
    new DirectiveDefinition[] {
      new MarkerDirective("SCOPE", DirectiveOpcode.Scope),
      new NamedDirective("FUNC", DirectiveOpcode.Function),
      new NamedDirective("CALL", DirectiveOpcode.Call, DirectiveOperandKind.Value),
      new MarkerDirective("END", DirectiveOpcode.End),
      new BooleanDirective("MATCH", DirectiveOpcode.Match),
      new BooleanDirective("ASSERT", DirectiveOpcode.Assert),
      new ValueDirective("CODE", DirectiveOpcode.Code),
      new MixinDirective(),
      new NamedDirective("RESOLVE_MIXIN", DirectiveOpcode.ResolveMixin, DirectiveOperandKind.Value),
      new ValueDirective("USING", DirectiveOpcode.Using),
      new ValueDirective("LOG", DirectiveOpcode.Log),
      new DumpDirective(),
      new NamedDirective("LOCAL", DirectiveOpcode.Local, DirectiveOperandKind.Value),
      new NamedDirective("VAR", DirectiveOpcode.Variable, DirectiveOperandKind.Value),
      new PropStructDirective(),
      new NamedDirective("AUGMENT_STRUCT", DirectiveOpcode.AugmentStruct, DirectiveOperandKind.Value),
      new PutDirective(),
      new NamedDirective("PUSH", DirectiveOpcode.Push, DirectiveOperandKind.Value),
      new MarkerDirective("RETURN", DirectiveOpcode.Return),
      new NamedDirective("GOTO", DirectiveOpcode.Goto),
      new MarkerDirective("SKIP", DirectiveOpcode.Skip),
      new ValueDirective("FAIL", DirectiveOpcode.Fail)
    }.ToDictionary(definition => definition.Name, StringComparer.Ordinal);

  internal static bool TryGet(string name, out DirectiveDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }
}