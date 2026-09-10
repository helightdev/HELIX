using System.Collections.Generic;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class NameFunction() : EvaluatedFunctionDefinition("name", 0, resultType: HixValueKind.String) {
  public override string Documentation => "Returns the backend-defined name of a value.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return new LiteralHixValue(context.NameOf(value));
  }
}

public sealed class UnwrapFunction() : EvaluatedFunctionDefinition("unwrap", 0) {
  public override string Documentation => "Unwraps a host value to its underlying Hix value.";
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return context.Unwrap(value);
  }
}
