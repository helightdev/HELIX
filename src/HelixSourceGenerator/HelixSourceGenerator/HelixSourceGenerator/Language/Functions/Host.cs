using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Shared;
using Microsoft.CodeAnalysis;

namespace HelixSourceGenerator.Language.Functions;

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

internal abstract class PropStructFunction(string name, int minimumArguments, int maximumArguments,
  bool predicate = false
) : EvaluatedFunctionDefinition(name, minimumArguments, maximumArguments, predicate) {
  protected static MixinPropStructValue Require(
    ExecutionContext context, IMixinValue value,
    out IMixinValue error
  ) {
    error = value is MixinPropStructValue ? null : context.Error("function must be called on a prop struct handle");
    return value as MixinPropStructValue;
  }
}

internal abstract class PropStructPredicate(string name) : PropStructFunction(name, 0, 0, true) {
  protected sealed override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    var prop = Require(context, value, out var error);
    return error ?? (Evaluate(prop) ? BooleanMixinValue.True : BooleanMixinValue.False);
  }

  protected abstract bool Evaluate(MixinPropStructValue value);
}

internal sealed class StructHasEqualityPredicate() : PropStructPredicate("structHasEquality") {
  protected override bool Evaluate(MixinPropStructValue value) {
    return value.Model.Equality.HasMembers;
  }
}

internal sealed class StructNoArgsPredicate() : PropStructPredicate("structNoArgs") {
  protected override bool Evaluate(MixinPropStructValue value) {
    return value.Model.ParameterParts.Count == 0;
  }
}

internal sealed class StructAugmentPredicate() : PropStructPredicate("structAugment") {
  protected override bool Evaluate(MixinPropStructValue value) {
    return value.Augmenting;
  }
}

internal abstract class StructPartsFunction(string name) : PropStructFunction(name, 0, 1) {
  protected sealed override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    var prop = Require(context, value, out var error);
    if (error is not null) return error;
    var parts = Parts(prop);
    var prefix = arguments.Count == 0 ? null : arguments[0].Render(context).Resolve(context.Strings);
    var joined = string.IsNullOrWhiteSpace(prefix) ? string.Join(", ", parts) :
      parts.Count == 0 ? prefix : prefix + ", " + string.Join(", ", parts);
    return new LiteralMixinValue(context.ResolveString(joined));
  }

  protected abstract IReadOnlyList<string> Parts(MixinPropStructValue value);
}

internal sealed class StructParamsFunction() : StructPartsFunction("structParams") {
  protected override IReadOnlyList<string> Parts(MixinPropStructValue value) {
    return value.Model.ParameterParts;
  }
}

internal sealed class StructArgsFunction() : StructPartsFunction("structArgs") {
  protected override IReadOnlyList<string> Parts(MixinPropStructValue value) {
    return value.Model.ArgumentParts;
  }
}

internal sealed class PropStructCallFunction() : PropStructFunction("propStructCall", 2, 2) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    var prop = Require(context, value, out var error);
    if (error is not null) return error;
    var target = arguments[0].Render(context).Resolve(context.Strings);
    var variable = arguments[1].Render(context).Resolve(context.Strings);
    var callArguments = prop.Props.Select(item => item.RefKind switch {
        RefKind.Ref => "ref ", RefKind.Out => "out ", RefKind.In => "in ", _ => ""
      } + variable + "." + GeneratorAnalysis.EscapeIdentifier(item.Name)
    );
    return new LiteralMixinValue(context.ResolveString(target + "(" + string.Join(", ", callArguments) + ")"));
  }
}
