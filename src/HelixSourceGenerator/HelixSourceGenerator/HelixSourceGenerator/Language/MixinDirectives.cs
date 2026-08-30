using System;
using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Shared;

namespace HelixSourceGenerator.Language;

public enum DirectiveOperandKind { None, Value, Boolean }

public class DirectiveDefinition(string name, DirectiveOperandKind operandKind, int maximumArguments = 1) {
  public string Name { get; } = name;
  public DirectiveOperandKind OperandKind { get; } = operandKind;
  public int MaximumArguments { get; } = maximumArguments;

  internal virtual bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count <= MaximumArguments) {
      error = null;
      return true;
    }
    error = Name + " accepts at most " + MaximumArguments + " arguments";
    return false;
  }
}

public abstract class DirectiveFunctionDefinition(string name, DirectiveOperandKind operandKind,
  int maximumArguments = 1, int hoistedLocalArgumentIndex = -1
) : DirectiveDefinition(name, operandKind, maximumArguments) {
  public int HoistedLocalArgumentIndex { get; } = hoistedLocalArgumentIndex;

  public abstract IMixinValue Invoke(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  );
}

public static class DirectiveLibrary {
  private static readonly IReadOnlyDictionary<string, DirectiveDefinition> Definitions =
    CreateDefinitions();

  private static IReadOnlyDictionary<string, DirectiveDefinition> CreateDefinitions() {
    using var profile = MixinProfiler.Measure("static.directive_library");
    return new Dictionary<string, DirectiveDefinition>(StringComparer.Ordinal) {
      ["RESOLVE_MIXIN"] = new ResolveMixinDirective(), ["PROP_STRUCT"] = new PropStructDirective(),
      ["AUGMENT_STRUCT"] = new AugmentStructDirective(), ["PUSH"] = new PushDirective(), ["PUT"] = new PutDirective()
    };
  }

  public static bool TryGet(string name, out DirectiveDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }
}

internal abstract class EvaluatedDirective(string name, int arguments, int hoistedLocal)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value, arguments, hoistedLocal) {
  protected virtual bool EvaluateOperand => true;

  public sealed override IMixinValue Invoke(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    if (EvaluateOperand) operand = context.Evaluate(operand);
    return operand is ErrorMixinValue ? operand : Apply(context, values, operand);
  }

  protected abstract IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  );
}

internal sealed class ResolveMixinDirective() : EvaluatedDirective("RESOLVE_MIXIN", 1, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    return context.ResolveMixin(arguments[0].Render(context), operand);
  }
}

internal sealed class PropStructDirective() : EvaluatedDirective("PROP_STRUCT", 4, 1) {
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

  protected override IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    return context.CreatePropStruct(arguments, operand);
  }
}

internal sealed class AugmentStructDirective() : EvaluatedDirective("AUGMENT_STRUCT", 2, 0) {
  protected override bool EvaluateOperand => false;

  protected override IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    return context.AugmentPropStruct(arguments[0].Render(context), operand);
  }
}

internal abstract class TableDirective(string name, int arguments)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value, arguments) {
  public override IMixinValue Invoke(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    operand = context.Evaluate(operand);
    if (operand is ErrorMixinValue) return operand;
    var local = values[0].Render(context);
    var table = context.Locals.TryGetValue(local, out var existing) && existing is MixinTableValue typed
      ? typed
      : MixinTableValue.Empty;
    table = Mutate(context, table, values, operand);
    context.Locals.StoreIsolated(local, table);
    return table;
  }

  protected abstract MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  );
}

internal sealed class PushDirective() : TableDirective("PUSH", 1) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Push(context, operand);
  }
}

internal sealed class PutDirective() : TableDirective("PUT", 2) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Put(arguments[1].Render(context), operand);
  }
}
