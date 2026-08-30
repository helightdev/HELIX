using System.Collections.Generic;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class NameFunction() : EvaluatedFunctionDefinition("name", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return new LiteralMixinValue(context.NameOf(value));
  }
}

internal sealed class PathFunction() : EvaluatedFunctionDefinition("path", 1, 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return value.Select(context, arguments[0].Render(context));
  }
}

internal sealed class UnwrapFunction() : EvaluatedFunctionDefinition("unwrap", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return context.Unwrap(value);
  }
}

internal sealed class SwitchFunction() : EvaluatedFunctionDefinition("switch", 2, 2) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return value.IsTruthy(context) ? arguments[0] : arguments[1];
  }
}

internal sealed class SizeFunction() : EvaluatedFunctionDefinition("size", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return new LiteralMixinValue(
      ExecutionContext.Dynamic(
        value is MixinTableValue table
          ? table.Count.ToString()
          : value.Render(context).Resolve(context.Strings).Length.ToString()
      )
    );
  }
}