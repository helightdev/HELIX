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

internal sealed class CollectAnnotatedTypesFunction() : FunctionDefinition("collectAnnotatedTypes", [
  new(MixinValueKind.Tuple, [MixinValueKind.String])
]) {
  public override bool HasEffects => true;
  public override IReadOnlyList<int> CSharpTypeArguments => new[] {0};
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) =>
    execution.IsPrelude && execution.Context is RoslynMixinContext context
      ? context.CollectAnnotatedTypes(arguments[0].Render(context).Resolve(context.Strings))
      : execution.Context.Error("collectAnnotatedTypes requires the Roslyn prelude host");
}

internal sealed class NamespaceFunction() : EvaluatedFunctionDefinition("namespace", 0,
  MixinValueKind.Symbol, MixinValueKind.String) {
  protected override IMixinValue Apply(ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> arguments) =>
    value is RoslynMixinValue symbol && RoslynMixinContext.TypeOf(symbol.Value) is { } type
      ? new LiteralMixinValue(context.ResolveString(type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString()))
      : context.Error("namespace requires a type symbol");
}
