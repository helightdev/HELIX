using System.Collections.Generic;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class WireFunction() : RoslynFunctionDefinition("wire", 1, resultType: HixValueKind.String) {
  public override string Documentation => "Renders a semantic value for wiring into generated source. Methods cannot be wired directly.";
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
  public override string Documentation => "Renders the backend signature of a semantic value.";
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
  public override string Documentation => "Tests whether a semantic value can be wired into generated code.";
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
  public override string Documentation => "Returns types in the current compilation carrying the named attribute, including nested types. Results are deduplicated and sorted.";
  public override bool HasEffects => true;
  public override bool RequiresPrelude => true;
  public override IReadOnlyDictionary<int, string> ArgumentReferences => new Dictionary<int, string> { {0, "CSharpType"} };
  public override IHixValue Execute(HixThread execution, IHixValue[] arguments, int line) { return execution.IsPrelude && execution.Context is HixRoslynContext context
      ? context.CollectAnnotatedTypes(execution, arguments[0].Render(execution).Resolve(execution.Strings))
      : execution.Error("collectAnnotatedTypes requires the Roslyn prelude host"); }
}

public sealed class NamespaceFunction() : RoslynFunctionDefinition("namespace", 0,
  HixValueKind.Symbol, HixValueKind.String) {
  public override string Documentation => "Returns the namespace of a type symbol.";
  protected override IHixValue Apply(HixThread context, IHixValue value, IReadOnlyList<IHixValue> arguments) =>
    value is RoslynHixValue symbol && HixRoslynContext.TypeOf(symbol.Value) is { } type
      ? new LiteralHixValue(context.ResolveString(type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString()))
      : context.Error("namespace requires a type symbol");
}
