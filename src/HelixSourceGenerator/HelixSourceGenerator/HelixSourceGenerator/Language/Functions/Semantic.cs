using Microsoft.CodeAnalysis;
using static HelixSourceGenerator.Shared.GeneratorAnalysis;
using static HelixSourceGenerator.Language.Functions.FunctionResults;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class TypeFunction : FunctionDefinition {
  internal TypeFunction() : base("type", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (typed.BackingValue is DetachedSemanticValue detached) value = detached.Type;
    else if (typed.BackingValue is not MixinGeneratedStructReference) value = typed.RoslynType;
    return FunctionResult(Name, value, out error);
  }
}

internal sealed class FullNameFunction : FunctionDefinition {
  internal FullNameFunction() : base("fullName", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    value = typed.BackingValue is MixinGeneratedStructReference generated
      ? generated.TypeName.Replace("global::", "")
      : typed.FullName;
    return FunctionResult(Name, value, out error);
  }
}

internal sealed class MakeGenericFunction : FunctionDefinition {
  internal MakeGenericFunction() : base("makeGeneric", 1, 1) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
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
      value = openType + "<" + property.Argument + ">";
    } else value = genericType.ConstructedFrom.Construct(genericArgument);
    error = null;
    return true;
  }
}

internal sealed class VisibilityFunction : FunctionDefinition {
  internal VisibilityFunction() : base("visibility", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (typed.Visibility is { } detachedVisibility) {
      value = detachedVisibility;
      error = null;
      return true;
    }
    var symbol = typed.RoslynSymbol ?? typed.RoslynType;
    if (symbol is null || symbol.DeclaredAccessibility == Accessibility.NotApplicable) {
      error = "property ':visibility' is not available for this value";
      return false;
    }
    value = AccessibilityText(symbol.DeclaredAccessibility);
    error = null;
    return true;
  }
}

internal static class FunctionResults {
  internal static bool FunctionResult(string name, object value, out string error) {
    error = value is null ? "property ':" + name + "' is not available for this value" : null;
    return error is null;
  }
}

internal enum AttributeFunctionKind { All, Assignable, Exact, First }

internal sealed class AttributeFunction : FunctionDefinition {
  private readonly AttributeFunctionKind _kind;

  internal AttributeFunction(string name, int arguments, AttributeFunctionKind kind) :
    base(name, arguments, arguments) {
    _kind = kind;
  }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    value = _kind switch {
      AttributeFunctionKind.All => typed.Attributes(null, false),
      AttributeFunctionKind.Assignable => typed.Attributes(invocation.Argument, false),
      AttributeFunctionKind.Exact => typed.Attributes(invocation.Argument, true),
      AttributeFunctionKind.First => typed.FirstAttribute(invocation.Argument),
      _ => null
    };
    error = null;
    return true;
  }
}

internal sealed class WireFunction : FunctionDefinition {
  internal WireFunction() : base("wire", 1, 1) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (!typed.TryWire(invocation.Argument, out var wired, out error)) return false;
    value = wired;
    return true;
  }
}

internal sealed class PropStructFunction : FunctionDefinition {
  internal PropStructFunction(string name, int minimum, int maximum) : base(name, minimum, maximum) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    return typed.TryApplyPropStruct((MixinExpressionProperty)invocation, out value, out error);
  }
}