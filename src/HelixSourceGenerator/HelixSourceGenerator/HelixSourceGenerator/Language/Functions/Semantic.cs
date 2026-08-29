using Microsoft.CodeAnalysis;
using static HelixSourceGenerator.Shared.GeneratorAnalysis;
using static HelixSourceGenerator.Language.Functions.FunctionResults;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class TypeFunction : FunctionDefinition {
  internal TypeFunction() : base("type", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (typed.Value is DetachedSemanticValue detached)
      value = detached.Type is null ? NullMixinValue.Instance : detached.Type;
    else if (typed.Value is not MixinGeneratedStructReference) value = MixinValue.From(typed.RoslynType, context);
    return FunctionResult(Name, value, out error);
  }
}

internal sealed class FullNameFunction : FunctionDefinition {
  internal FullNameFunction() : base("fullName", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    value = MixinValue.From(
      typed.Value is MixinGeneratedStructReference generated
        ? generated.TypeName.Replace("global::", "")
        : typed.FullName, context
    );
    return FunctionResult(Name, value, out error);
  }
}

internal sealed class MakeGenericFunction : FunctionDefinition {
  internal MakeGenericFunction() : base("makeGeneric", 1, 1) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    var property = (MixinExpressionProperty)invocation;
    var genericType = typed.RoslynType as INamedTypeSymbol;
    if (genericType is not { IsUnboundGenericType: true, Arity: 1 }) {
      error = ":makeGeneric requires an unbound generic type of arity 1";
      return false;
    }
    var genericArgument = MixinValue.From(property.Values[0], context).RoslynType ??
      typed.ResolveType(property.Argument);
    if (genericArgument is null) {
      if (!typed.IsGeneratedType(property.Argument)) {
        error = ":makeGeneric type argument '" + property.Argument + "' was not found";
        return false;
      }
      var openType = genericType.ConstructedFrom.ToDisplayString(
        SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
      );
      value = MixinValue.From(openType + "<" + property.Argument + ">", context);
    } else value = MixinValue.From(genericType.ConstructedFrom.Construct(genericArgument), context);
    error = null;
    return true;
  }
}

internal sealed class VisibilityFunction : FunctionDefinition {
  internal VisibilityFunction() : base("visibility", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (typed.Visibility is { } detachedVisibility) {
      value = MixinValue.From(detachedVisibility);
      error = null;
      return true;
    }
    var symbol = typed.RoslynSymbol ?? typed.RoslynType;
    if (symbol is null || symbol.DeclaredAccessibility == Accessibility.NotApplicable) {
      error = "property ':visibility' is not available for this value";
      return false;
    }
    value = MixinValue.From(AccessibilityText(symbol.DeclaredAccessibility));
    error = null;
    return true;
  }
}

internal static class FunctionResults {
  internal static bool FunctionResult(string name, IMixinValue value, out string error) {
    error = value is null or NullMixinValue ? "property ':" + name + "' is not available for this value" : null;
    return error is null;
  }
}

internal abstract class AttributeFunction(string name, int arguments)
  : FunctionDefinition(name, arguments, arguments) {
  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    value = Select(typed, invocation);
    error = null;
    return true;
  }

  protected abstract IMixinValue Select(IMixinValue value, FunctionInvocation invocation);
}

internal sealed class AttributesFunction() : AttributeFunction("attributes", 0) {
  protected override IMixinValue Select(IMixinValue value, FunctionInvocation invocation) {
    return value.Attributes(null, false);
  }
}

internal sealed class AttributesOfFunction() : AttributeFunction("attributesOf", 1) {
  protected override IMixinValue Select(IMixinValue value, FunctionInvocation invocation) {
    return value.Attributes(invocation.Argument, false);
  }
}

internal sealed class AttributesOfExactFunction() : AttributeFunction("attributesOfExact", 1) {
  protected override IMixinValue Select(IMixinValue value, FunctionInvocation invocation) {
    return value.Attributes(invocation.Argument, true);
  }
}

internal sealed class AttributeOfFunction() : AttributeFunction("attributeOf", 1) {
  protected override IMixinValue Select(IMixinValue value, FunctionInvocation invocation) {
    return value.FirstAttribute(invocation.Argument);
  }
}

internal sealed class WireFunction : FunctionDefinition {
  internal WireFunction() : base("wire", 1, 1) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (!typed.TryWire(invocation.Argument, out var wired, out error)) return false;
    value = MixinValue.From(wired);
    return true;
  }
}

internal sealed class PropStructFunction : FunctionDefinition {
  internal PropStructFunction(string name, int minimum, int maximum) : base(name, minimum, maximum) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (!typed.TryApplyPropStruct((MixinExpressionProperty)invocation, out var result, out error)) return false;
    value = result;
    return true;
  }
}