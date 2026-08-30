using System.Collections.Generic;

namespace MixinLanguage.Functions;

internal sealed class WireFunction() : EvaluatedFunctionDefinition("wire", 1, 1) {
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

internal sealed class SignaturePredicate() : PredicateFunctionDefinition("signature", 1, 1) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      context is RoslynMixinContext roslyn &&
      RoslynMixinContext.SameSignature(roslyn.Callable(value), roslyn.Callable(arguments[0]))
    );
  }
}

internal sealed class WireablePredicate() : PredicateFunctionDefinition("wireable", 2, 2) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Result(
      context is RoslynMixinContext roslyn &&
      RoslynMixinContext.TryWireParameters(roslyn.Callable(arguments[0]), roslyn.Callable(arguments[1]), out _)
    );
  }
}