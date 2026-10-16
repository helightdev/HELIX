using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// Renders a compile-time value as the C# literal that reproduces it. Every value that
/// reaches generated text goes through here; getting this wrong by hand is the most common
/// codegen bug there is.
/// </summary>
public static class Lit {
  public static string Of(string value) => value is null ? "null" : Quote(value);
  public static string Of(bool value) => value ? "true" : "false";
  public static string Of(bool? value) => value is null ? "null" : Of(value.Value);
  public static string Of(int value) => value.ToString(CultureInfo.InvariantCulture);
  public static string Of(int? value) => value is null ? "null" : Of(value.Value);
  public static string Of(long value) => value.ToString(CultureInfo.InvariantCulture) + "L";
  public static string Of(double value) =>
    value.ToString("R", CultureInfo.InvariantCulture) + (HasPointOrExponent(value) ? "d" : ".0d");
  public static string Of(float value) =>
    value.ToString("R", CultureInfo.InvariantCulture) + (HasPointOrExponent(value) ? "f" : ".0f");
  public static string Of(char value) => "'" + Escape(value.ToString(), '\'') + "'";
  public static string Of(PureTypeRef value) => value.Exists ? "typeof(" + value.Qualified + ")" : "null";
  public static string Of(TypeRef value) => value.Exists ? "typeof(" + value.Qualified + ")" : "null";

  /// <summary>Fallback for values whose static type isn't known at the call site.</summary>
  public static string Of(object value) => value switch {
    null => "null",
    string text => Quote(text),
    bool flag => Of(flag),
    int number => Of(number),
    long number => Of(number),
    double number => Of(number),
    float number => Of(number),
    char character => Of(character),
    PureTypeRef type => Of(type),
    TypeRef type => Of(type),
    System.Enum => throw new ArgumentException(
      "enum values need their declaring type to render; use Lit.Enum(qualifiedType, value)", nameof(value)),
    IEnumerable<object> items => Array(items),
    _ => Convert.ToString(value, CultureInfo.InvariantCulture),
  };

  /// <summary>A qualified enum member, e.g. <c>global::HELIX.PropInit.Constant</c>.</summary>
  public static string Enum(string qualifiedType, object value) =>
    value is null ? "null" : qualifiedType + "." + value;

  public static string Enum(PureTypeRef type, object value) =>
    !type.Exists || value is null ? "null" : type.Qualified + "." + value;

  public static string Array(IEnumerable<object> values) {
    var items = new List<string>();
    foreach (var value in values) items.Add(Of(value));
    return "new[] { " + string.Join(", ", items) + " }";
  }

  /// <summary>Quotes and escapes a string, including the cases a naive escape misses.</summary>
  public static string Quote(string value) => "\"" + Escape(value, '"') + "\"";

  private static bool HasPointOrExponent(double value) {
    var text = value.ToString("R", CultureInfo.InvariantCulture);
    return text.IndexOf('.') >= 0 || text.IndexOf('E') >= 0 || text.IndexOf('e') >= 0;
  }

  private static string Escape(string value, char quote) {
    var builder = new System.Text.StringBuilder(value.Length + 8);
    foreach (var character in value) {
      switch (character) {
        case '\\': builder.Append(@"\\"); break;
        case '\0': builder.Append(@"\0"); break;
        case '\a': builder.Append(@"\a"); break;
        case '\b': builder.Append(@"\b"); break;
        case '\f': builder.Append(@"\f"); break;
        case '\n': builder.Append(@"\n"); break;
        case '\r': builder.Append(@"\r"); break;
        case '\t': builder.Append(@"\t"); break;
        case '\v': builder.Append(@"\v"); break;
        default:
          if (character == quote) builder.Append('\\').Append(character);
          else if (char.IsControl(character) || char.IsSurrogate(character))
            builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
          else builder.Append(character);
          break;
      }
    }
    return builder.ToString();
  }

  /// <summary>Renders an attribute argument's decoded value, using its symbol type for enums.</summary>
  public static string OfTypedConstant(TypedConstant constant) {
    if (constant.IsNull) return "null";
    switch (constant.Kind) {
      case TypedConstantKind.Enum:
        var name = constant.Type?.ToDisplayString(SymbolFacts.QualifiedFormat);
        var member = constant.Type is INamedTypeSymbol enumType ? EnumMemberName(enumType, constant.Value) : null;
        return member is null
          ? "(" + name + ")" + Convert.ToString(constant.Value, CultureInfo.InvariantCulture)
          : name + "." + member;
      case TypedConstantKind.Type:
        return constant.Value is ITypeSymbol type
          ? "typeof(" + type.ToDisplayString(SymbolFacts.QualifiedFormat) + ")" : "null";
      case TypedConstantKind.Array:
        var items = new List<string>();
        foreach (var item in constant.Values) items.Add(OfTypedConstant(item));
        return "new[] { " + string.Join(", ", items) + " }";
      default:
        return Of(constant.Value);
    }
  }

  private static string EnumMemberName(INamedTypeSymbol enumType, object value) {
    foreach (var member in enumType.GetMembers())
      if (member is IFieldSymbol { HasConstantValue: true } field && Equals(field.ConstantValue, value))
        return field.Name;
    return null;
  }
}
