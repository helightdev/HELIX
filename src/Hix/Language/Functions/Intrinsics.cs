using System.Collections.Generic;
using Hix.Runtime;

namespace Hix.Functions;

internal sealed class NameFunction() : EvaluatedFunctionDefinition("name", 0, resultType: HixValueKind.String) {
  protected override IHixValue Apply(
    HixExecutionContext context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return new LiteralHixValue(context.NameOf(value));
  }
}

internal sealed class UnwrapFunction() : EvaluatedFunctionDefinition("unwrap", 0) {
  protected override IHixValue Apply(
    HixExecutionContext context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return context.Unwrap(value);
  }
}
