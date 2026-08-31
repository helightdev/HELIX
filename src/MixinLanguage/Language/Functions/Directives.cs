using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;

namespace Mixins.Functions;

internal sealed class ResolveMixinDirectiveFunction() : EvaluatedDirectiveFunction(
  "RESOLVE_MIXIN", 1, [MixinLanguageValueKind.Text]
) {
  protected override IMixinValue Apply(
    ExecutionContext context, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand
  ) {
    return context.ResolveMixin(arguments[0].Render(context), operand);
  }
}

internal abstract class TableDirectiveFunction(string name, int arguments)
  : FunctionDefinition(name, arguments, argumentTypes: arguments == 1
    ? [MixinLanguageValueKind.Identifier]
    : [MixinLanguageValueKind.Identifier, MixinLanguageValueKind.Any]) {
  public override IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  ) {
    var values = arguments.Select(context.Evaluate).ToArray();
    var error = values.OfType<ErrorMixinValue>().FirstOrDefault();
    if (error is not null) return error;
    instance = context.Evaluate(instance);
    if (instance is ErrorMixinValue) return instance;
    var local = values[0].Render(context);
    var table = context.Locals.TryGetValue(local, out var existing) && existing is MixinTableValue typed
      ? typed
      : MixinTableValue.Empty;
    table = Mutate(context, table, values, instance);
    context.Locals.StoreIsolated(local, table);
    return table;
  }

  protected abstract MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  );
}

internal sealed class PushDirectiveFunction() : TableDirectiveFunction("PUSH", 1) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Push(context, operand);
  }
}

internal sealed class PutDirectiveFunction() : TableDirectiveFunction("PUT", 2) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments, IMixinValue operand
  ) {
    return table.Put(context, arguments[1].Render(context), operand);
  }
}
