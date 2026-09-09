using System.Collections.Generic;
using Hix.Runtime;

namespace Hix.Functions;

internal sealed class WireFunction() : RoslynFunctionDefinition("wire", 1, resultType: HixValueKind.String) {
  protected override IHixValue Apply(
    HixExecutionContext context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return context is HixRoslynContext roslyn &&
      HixRoslynContext.TryWireParameters(roslyn.Callable(value), roslyn.Callable(arguments[0]), out var wired)
        ? new LiteralHixValue(context.ResolveString(wired))
        : context.Error("methods cannot be wired");
  }
}

internal sealed class SignatureFunction() : RoslynFunctionDefinition("signature", 1, resultType: HixValueKind.Bool) {
  protected override IHixValue Apply(
    HixExecutionContext context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(
      context is HixRoslynContext roslyn &&
      HixRoslynContext.SameSignature(roslyn.Callable(value), roslyn.Callable(arguments[0]))
    );
  }
}

internal sealed class WireableFunction() : RoslynFunctionDefinition("wireable", 1, resultType: HixValueKind.Bool) {
  protected override IHixValue Apply(
    HixExecutionContext context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(
      context is HixRoslynContext roslyn &&
      HixRoslynContext.TryWireParameters(roslyn.Callable(value), roslyn.Callable(arguments[0]), out _)
    );
  }
}

internal sealed class CollectAnnotatedTypesFunction() : FunctionDefinition("collectAnnotatedTypes", [
  new(HixValueKind.Tuple, [HixValueKind.String])
]) {
  public override bool HasEffects => true;
  public override bool RequiresPrelude => true;
  public override IReadOnlyDictionary<int, string> ArgumentReferences => new Dictionary<int, string> { {0, "CSharpType"} };
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) { var execution = invocation; return execution.IsPrelude && execution is HixRoslynContext context
      ? context.CollectAnnotatedTypes(arguments[0].Render(context).Resolve(context.Strings))
      : execution.Error("collectAnnotatedTypes requires the Roslyn prelude host"); }
}

internal sealed class NamespaceFunction() : RoslynFunctionDefinition("namespace", 0,
  HixValueKind.Symbol, HixValueKind.String) {
  protected override IHixValue Apply(HixExecutionContext context, IHixValue value, IReadOnlyList<IHixValue> arguments) =>
    value is RoslynHixValue symbol && HixRoslynContext.TypeOf(symbol.Value) is { } type
      ? new LiteralHixValue(context.ResolveString(type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString()))
      : context.Error("namespace requires a type symbol");
}
