using System.Collections.Generic;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class WireFunction() : RoslynFunctionDefinition("wire", 1, resultType: HixValueKind.String) {
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return context.Context is HixRoslynContext roslyn &&
      HixRoslynContext.TryWireParameters(roslyn.Callable(context, value), roslyn.Callable(context, arguments[0]), out var wired)
        ? new LiteralHixValue(context.ResolveString(wired))
        : context.Error("methods cannot be wired");
  }
}

public sealed class SignatureFunction() : RoslynFunctionDefinition("signature", 1, resultType: HixValueKind.Bool) {
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(
      context.Context is HixRoslynContext roslyn &&
      HixRoslynContext.SameSignature(roslyn.Callable(context, value), roslyn.Callable(context, arguments[0]))
    );
  }
}

public sealed class WireableFunction() : RoslynFunctionDefinition("wireable", 1, resultType: HixValueKind.Bool) {
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    return BooleanHixValue.From(
      context.Context is HixRoslynContext roslyn &&
      HixRoslynContext.TryWireParameters(roslyn.Callable(context, value), roslyn.Callable(context, arguments[0]), out _)
    );
  }
}

public sealed class CollectAnnotatedTypesFunction() : FunctionDefinition("collectAnnotatedTypes", [
  new(HixValueKind.Tuple, [HixValueKind.String])
]) {
  public override bool HasEffects => true;
  public override bool RequiresPrelude => true;
  public override IReadOnlyDictionary<int, string> ArgumentReferences => new Dictionary<int, string> { {0, "CSharpType"} };
  public override IHixValue Execute(HixThread execution, IHixValue[] arguments, int line) { return execution.IsPrelude && execution.Context is HixRoslynContext context
      ? context.CollectAnnotatedTypes(execution, arguments[0].Render(execution).Resolve(execution.Strings))
      : execution.Error("collectAnnotatedTypes requires the Roslyn prelude host"); }
}

public sealed class NamespaceFunction() : RoslynFunctionDefinition("namespace", 0,
  HixValueKind.Symbol, HixValueKind.String) {
  protected override IHixValue Apply(HixThread context, IHixValue value, IReadOnlyList<IHixValue> arguments) =>
    value is RoslynHixValue symbol && HixRoslynContext.TypeOf(symbol.Value) is { } type
      ? new LiteralHixValue(context.ResolveString(type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString()))
      : context.Error("namespace requires a type symbol");
}
