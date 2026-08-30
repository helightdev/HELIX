using System;
using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;

namespace HelixSourceGenerator.Language;

public enum DirectiveOperandKind { None, Value, Boolean }

public class DirectiveDefinition(string name, DirectiveOperandKind operandKind, int maximumArguments = 1) {
  public string Name { get; } = name;
  public DirectiveOperandKind OperandKind { get; } = operandKind;
  public int MaximumArguments { get; } = maximumArguments;
  internal virtual bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count <= MaximumArguments) { error = null; return true; }
    error = Name + " accepts at most " + MaximumArguments + " arguments";
    return false;
  }
}

public abstract class DirectiveFunctionDefinition(string name, DirectiveOperandKind operandKind,
  int maximumArguments = 1, int hoistedLocalArgumentIndex = -1) : DirectiveDefinition(name, operandKind, maximumArguments) {
  public int HoistedLocalArgumentIndex { get; } = hoistedLocalArgumentIndex;
  public abstract IMixinValue Invoke(ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand);
}

public static class DirectiveLibrary {
  private static readonly IReadOnlyDictionary<string, DirectiveDefinition> Definitions =
    new Dictionary<string, DirectiveDefinition>(StringComparer.Ordinal) {
      ["RESOLVE_MIXIN"] = new HostDirective("RESOLVE_MIXIN", 1, 0),
      ["PROP_STRUCT"] = new PropStructDirective(),
      ["AUGMENT_STRUCT"] = new HostDirective("AUGMENT_STRUCT", 2, 0),
      ["PUSH"] = new TableDirective("PUSH", 1),
      ["PUT"] = new TableDirective("PUT", 2)
    };
  public static bool TryGet(string name, out DirectiveDefinition definition) =>
    Definitions.TryGetValue(name ?? "", out definition);
}

internal class HostDirective(string name, int arguments, int hoistedLocal)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value, arguments, hoistedLocal) {
  public override IMixinValue Invoke(ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand) => context.InvokeHostDirective(Name, arguments, operand);
}

internal sealed class PropStructDirective() : HostDirective("PROP_STRUCT", 4, 1) {
  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count > 4) {
      error = "PROP_STRUCT accepts at most 4 arguments";
      return false;
    }
    var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var flag in arguments.Skip(2)) {
      if (flag is null || !(flag.Equals("datatype", StringComparison.OrdinalIgnoreCase) ||
        flag.Equals("noGenerate", StringComparison.OrdinalIgnoreCase))) {
        error = "unknown PROP_STRUCT flag '" + (flag ?? "") + "'";
        return false;
      }
      if (!flags.Add(flag)) {
        error = "PROP_STRUCT flag '" + flag + "' is specified more than once";
        return false;
      }
    }
    error = null;
    return true;
  }
}

internal sealed class TableDirective(string name, int arguments)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value, arguments) {
  public override IMixinValue Invoke(ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    operand = context.Evaluate(operand);
    if (operand is ErrorMixinValue) return operand;
    var local = values[0].Render(context);
    var table = context.Locals.TryGetValue(local, out var existing) && existing is MixinTableValue typed
      ? typed : MixinTableValue.Empty;
    table = Name == "PUSH" ? table.Push(context, operand) : table.Put(values[1].Render(context), operand);
    context.Locals[local] = table;
    return table;
  }
}

internal sealed class MixinValueDictionary : Dictionary<MixinString, IMixinValue> {
  internal MixinValueDictionary() { }
  internal MixinValueDictionary(IEnumerable<KeyValuePair<MixinString, IMixinValue>> values) {
    foreach (var item in values ?? []) Add(item.Key, item.Value);
  }
  internal IEnumerable<KeyValuePair<MixinString, IMixinValue>> TypedValues => this;
}
