using System.Collections.Generic;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class NameFunction() : EvaluatedFunctionDefinition("name") {
  protected override IHixValue Apply(
    HixCoroutine context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return new LiteralHixValue(context.NameOf(value));
  }
}

public sealed class UnwrapFunction() : EvaluatedFunctionDefinition("unwrap") {
  protected override IHixValue Apply(
    HixCoroutine context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return context.Unwrap(value);
  }
}
