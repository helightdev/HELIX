using System;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mixins.Compiler;
using Mixins.Env;
using Mixins.Roslyn;

namespace Mixins.Runtime;

/// <summary>Immutable handle to a Roslyn semantic value; all services come from ExecutionContext.</summary>
internal sealed record RoslynMixinValue(object Value, MixinExpressionRoot Root = MixinExpressionRoot.Null)
  : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Value switch {
      null => false, bool boolean => boolean,
      TypedConstant constant => constant.Kind != TypedConstantKind.Error && !constant.IsNull &&
        constant.Value is not false,
      _ => true
    };
  }

  public MixinString Render(ExecutionContext context) {
    using var profile = MixinProfiler.Measure("roslyn.value.render");
    if (Root is MixinExpressionRoot.This or MixinExpressionRoot.Target && Value is INamedTypeSymbol)
      return ExecutionContext.Dynamic("this");
    if (Value is IParameterSymbol parameter)
      return ExecutionContext.Dynamic(GeneratorAnalysis.EscapeIdentifier(parameter.Name));
    if (Value is IMethodSymbol method) {
      return ExecutionContext.Dynamic(
        method.IsStatic
          ? method.ContainingType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." + method.Name
          : "this." + GeneratorAnalysis.EscapeIdentifier(method.Name)
      );
    }
    if (Value is TypedConstant constant) return ExecutionContext.Dynamic(RenderConstant(constant));
    return ExecutionContext.Dynamic(RoslynMixinContext.ComparableText(Value));
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(RoslynMixinValue));
    builder.Append(Render(context).Resolve(context.Strings));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return context is RoslynMixinContext roslyn ? roslyn.SelectValue(this, member) : NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Value switch {
      TypedConstant { Kind: TypedConstantKind.Type } => Render(context).Resolve(context.Strings),
      TypedConstant { Value: string or char } => Render(context).Resolve(context.Strings),
      TypedConstant constant => constant.Value,
      ISymbol or AttributeData => Detach(),
      _ => Value
    };
  }

  public bool Equals(IMixinValue other) {
    return other is RoslynMixinValue value && (
      SymbolEqualityComparer.Default.Equals(Value as ISymbol, value.Value as ISymbol) ||
      (Value is not ISymbol && Equals(Value, value.Value))
    );
  }

  private static string RenderConstant(TypedConstant constant) {
    if (constant.IsNull || constant.Kind == TypedConstantKind.Error) return "null";
    if (constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type)
      return "typeof(" + type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + ")";
    return constant.Value switch {
      string text => SymbolDisplay.FormatLiteral(text, true),
      char character => SymbolDisplay.FormatLiteral(character, true),
      bool boolean => boolean ? "true" : "false",
      uint number => number.ToString(CultureInfo.InvariantCulture) + "U",
      long number => number.ToString(CultureInfo.InvariantCulture) + "L",
      ulong number => number.ToString(CultureInfo.InvariantCulture) + "UL",
      float number when float.IsNaN(number) => "global::System.Single.NaN",
      float number when float.IsPositiveInfinity(number) => "global::System.Single.PositiveInfinity",
      float number when float.IsNegativeInfinity(number) => "global::System.Single.NegativeInfinity",
      float number => number.ToString("R", CultureInfo.InvariantCulture) + "F",
      double number when double.IsNaN(number) => "global::System.Double.NaN",
      double number when double.IsPositiveInfinity(number) => "global::System.Double.PositiveInfinity",
      double number when double.IsNegativeInfinity(number) => "global::System.Double.NegativeInfinity",
      double number => number.ToString("R", CultureInfo.InvariantCulture) + "D",
      _ => Convert.ToString(constant.Value, CultureInfo.InvariantCulture)
    };
  }

  internal static string RenderCSharpConstant(TypedConstant constant) {
    if (constant.Kind != TypedConstantKind.Enum || constant.Type is not INamedTypeSymbol enumType)
      return RenderConstant(constant);
    var member = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(item =>
      item.HasConstantValue && Equals(item.ConstantValue, constant.Value)
    );
    var enumName = enumType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat);
    return member is not null
      ? enumName + "." + GeneratorAnalysis.EscapeIdentifier(member.Name)
      : "(" + enumName + ")" + Convert.ToString(constant.Value, CultureInfo.InvariantCulture);
  }

  private DetachedSemanticData Detach() {
    using var profile = MixinProfiler.Measure("semantic.detach");
    var type = RoslynMixinContext.TypeOf(Value);
    if (type is null) return new DetachedSemanticData("", "");
    var typeNamespace = type.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace
      ? containingNamespace.ToDisplayString()
      : "";
    var qualified = type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal);
    var prefix = typeNamespace.Length == 0 ? "" : typeNamespace + ".";
    var typeName = qualified.StartsWith(prefix, StringComparison.Ordinal)
      ? qualified.Substring(prefix.Length)
      : qualified;
    return new DetachedSemanticData(typeNamespace, typeName);
  }
}



internal sealed record DetachedSemanticData(string Namespace, string TypeName);

internal sealed record DetachedSemanticMixinValue(
  MixinString Namespace, MixinString TypeName
) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return true;
  }

  public MixinString Render(ExecutionContext context) {
    var name = TypeName.Resolve(context.Strings);
    var typeNamespace = Namespace.Resolve(context.Strings);
    if (string.IsNullOrEmpty(name)) return ExecutionContext.Dynamic("");
    return ExecutionContext.Dynamic(
      "global::" + (string.IsNullOrEmpty(typeNamespace) ? name : typeNamespace + "." + name)
    );
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(DetachedSemanticMixinValue));
    builder.Append(Namespace.Resolve(context.Strings));
    builder.Append(TypeName.Resolve(context.Strings));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return new DetachedSemanticData(
      Namespace.Resolve(context.Strings), TypeName.Resolve(context.Strings)
    );
  }

  public bool Equals(IMixinValue other) {
    return other is DetachedSemanticMixinValue value && Equals(value);
  }

  internal static DetachedSemanticMixinValue Materialize(DetachedSemanticData value) {
    return new DetachedSemanticMixinValue(
      MixinString.Dynamic(value.Namespace), MixinString.Dynamic(value.TypeName)
    );
  }
}