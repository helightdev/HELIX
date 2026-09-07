using System.Collections.Generic;
using Mixins.Runtime;

namespace Mixins.Functions;

internal sealed class NameFunction() : EvaluatedFunctionDefinition("name", 0, resultType: MixinValueKind.String) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return new LiteralMixinValue(context.NameOf(value));
  }
}

internal sealed class UnwrapFunction() : EvaluatedFunctionDefinition("unwrap", 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return context.Unwrap(value);
  }
}
