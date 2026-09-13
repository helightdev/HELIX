using System.Collections.Generic;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class WireFunction() : RoslynFunctionDefinition("wire") {
  protected override IHixValue Apply(
    HixCoroutine context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return context.Context is HixRoslynContext roslyn &&
      HixRoslynContext.TryWireParameters(roslyn.Callable(context, value), roslyn.Callable(context, arguments[0]), out var wired)
        ? new LiteralHixValue(context.ResolveString(wired))
        : context.Error("methods cannot be wired");
  }
}

public sealed class SignatureFunction() : RoslynFunctionDefinition("signature") {
  protected override IHixValue Apply(
    HixCoroutine context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(
      context.Context is HixRoslynContext roslyn &&
      HixRoslynContext.SameSignature(roslyn.Callable(context, value), roslyn.Callable(context, arguments[0]))
    );
  }
}

public sealed class WireableFunction() : RoslynFunctionDefinition("wireable") {
  protected override IHixValue Apply(
    HixCoroutine context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(
      context.Context is HixRoslynContext roslyn &&
      HixRoslynContext.TryWireParameters(roslyn.Callable(context, value), roslyn.Callable(context, arguments[0]), out _)
    );
  }
}

public sealed class CollectAnnotatedTypesFunction() : FunctionDefinition("collectAnnotatedTypes") {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) { return HixExpressionContext.IsPrelude(execution) && execution.Context is HixRoslynContext context
      ? context.CollectAnnotatedTypes(execution, arguments[0].Render(execution).Resolve(execution.Strings))
      : execution.Error("collectAnnotatedTypes requires the Roslyn prelude host"); }
}

public sealed class NamespaceFunction() : RoslynFunctionDefinition("namespace") {
  protected override IHixValue Apply(HixCoroutine context, IHixValue value, IReadOnlyList<IHixValue> arguments) =>
    value is RoslynHixValue symbol && HixRoslynContext.TypeOf(symbol.Value) is { } type
      ? new LiteralHixValue(context.ResolveString(type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString()))
      : context.Error("namespace requires a type symbol");
}
