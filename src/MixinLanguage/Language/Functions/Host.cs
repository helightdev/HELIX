using System.Collections.Generic;
using Mixins.Runtime;

namespace Mixins.Functions;

internal sealed class WireFunction() : EvaluatedFunctionDefinition("wire", 1, resultType: MixinValueKind.String) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return context is RoslynMixinContext roslyn &&
      RoslynMixinContext.TryWireParameters(roslyn.Callable(value), roslyn.Callable(arguments[0]), out var wired)
        ? new LiteralMixinValue(context.ResolveString(wired))
        : context.Error("methods cannot be wired");
  }
}

internal sealed class SignatureFunction() : EvaluatedFunctionDefinition("signature", 1, resultType: MixinValueKind.Bool) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return BooleanMixinValue.From(
      context is RoslynMixinContext roslyn &&
      RoslynMixinContext.SameSignature(roslyn.Callable(value), roslyn.Callable(arguments[0]))
    );
  }
}

internal sealed class WireableFunction() : EvaluatedFunctionDefinition("wireable", 1, resultType: MixinValueKind.Bool) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return BooleanMixinValue.From(
      context is RoslynMixinContext roslyn &&
      RoslynMixinContext.TryWireParameters(roslyn.Callable(value), roslyn.Callable(arguments[0]), out _)
    );
  }
}
