using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace HELIX.SourceGen.Expressions;

public enum MixinValueTrait {
  Self, Ref, In, Out, InOut, Argument, Static, Async, Public, Exposed,
  Top, Concrete, Partial, Generic, Struct, Class
}

/// <summary>Typed value passed through the interpreter and its functions.</summary>
public interface IMixinValue {
  object BackingValue { get; }
  ISymbol RoslynSymbol { get; }
  ITypeSymbol RoslynType { get; }
  string Visibility { get; }
  string Name { get; }
  string FullName { get; }
  bool Exists { get; }
  bool IsTruthy { get; }
  string Render();
  object Unwrap();
  bool TryGetText(out string text);
  object Select(string path);
  bool Is(string type);
  bool Has(object member);
  bool EqualsTo(object expected);
  bool Matches(string pattern, out string error);
  bool HasSameSignature(string expected);
  bool IsWireable(string from, string to);
  bool TryWire(string to, out string arguments, out string error);
  bool TryApplyPropStruct(MixinExpressionProperty property, out object result, out string error);
  MixinExpressionTable Attributes(string type, bool exact);
  object FirstAttribute(string type);
  bool HasTrait(MixinValueTrait trait);
  ITypeSymbol ResolveType(string name);
  bool IsGeneratedType(string name);
  IMixinValue Unlink();
}

public abstract class MixinValue : IMixinValue {
  public abstract object BackingValue { get; }
  public virtual ISymbol RoslynSymbol => null;
  public virtual ITypeSymbol RoslynType => null;
  public virtual string Visibility => null;
  public virtual string Name => Convert.ToString(BackingValue);
  public virtual string FullName => null;
  public virtual bool Exists => BackingValue is not null;
  public abstract bool IsTruthy { get; }
  public abstract string Render();
  public virtual object Unwrap() => BackingValue;
  public virtual bool TryGetText(out string text) { text = Render(); return true; }
  public virtual object Select(string path) => null;
  public virtual bool Is(string type) => false;
  public virtual bool Has(object member) => false;
  public virtual bool EqualsTo(object expected) => string.Equals(Render(), Convert.ToString(expected), StringComparison.Ordinal);
  public virtual bool Matches(string pattern, out string error) {
    try { error = null; return System.Text.RegularExpressions.Regex.IsMatch(Render(), pattern ?? ""); }
    catch (ArgumentException exception) { error = "invalid regular expression: " + exception.Message; return false; }
  }
  public virtual bool HasSameSignature(string expected) => false;
  public virtual bool IsWireable(string from, string to) => false;
  public virtual bool TryWire(string to, out string arguments, out string error) {
    arguments = null; error = "value does not support method wiring"; return false;
  }
  public virtual bool TryApplyPropStruct(MixinExpressionProperty property, out object result, out string error) {
    result = null; error = ":" + property.Name + " must be called on a prop struct handle"; return false;
  }
  public virtual MixinExpressionTable Attributes(string type, bool exact) => new MixinExpressionTable();
  public virtual object FirstAttribute(string type) => null;
  public virtual bool HasTrait(MixinValueTrait trait) => false;
  public virtual ITypeSymbol ResolveType(string name) => null;
  public virtual bool IsGeneratedType(string name) => false;
  public virtual IMixinValue Unlink() => this;

  internal static IMixinValue From(object value) => value switch {
    IMixinValue typed => typed,
    null => NullMixinValue.Instance,
    bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
    string text => new StringMixinValue(text),
    _ => new ObjectMixinValue(value)
  };

  internal static IMixinValue From(object value, IMixinExpressionContext context) {
    if (value is IMixinValue typed) return typed;
    if (context is RoslynMixinExpressionContext roslyn) return new RoslynMixinValue(roslyn, value);
    return From(value);
  }

  internal static object Unlink(object value, IMixinExpressionContext context) =>
    From(value, context).Unlink().BackingValue;
}

internal sealed class DetachedTypeValue : MixinValue {
  private readonly string[] _members;
  private readonly string[] _assignableTypes;
  private readonly DetachedTypeValue[] _typeArguments;
  private readonly string[] _typeParameterNames;
  internal DetachedTypeValue(
    string @namespace,
    IReadOnlyList<string> names,
    IReadOnlyList<string> members,
    string qualifiedName = null,
    IReadOnlyList<string> assignableTypes = null,
    IReadOnlyList<DetachedTypeValue> typeArguments = null,
    IReadOnlyList<string> typeParameterNames = null
  ) {
    Namespace = @namespace ?? "";
    Names = (names ?? Array.Empty<string>()).ToArray();
    _members = (members ?? Array.Empty<string>()).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToArray();
    QualifiedName = qualifiedName;
    _assignableTypes = (assignableTypes ?? Array.Empty<string>())
      .Select(NormalizeTypeName)
      .Distinct(StringComparer.Ordinal)
      .OrderBy(item => item, StringComparer.Ordinal)
      .ToArray();
    _typeArguments = (typeArguments ?? Array.Empty<DetachedTypeValue>()).ToArray();
    _typeParameterNames = (typeParameterNames ?? Array.Empty<string>()).ToArray();
  }
  internal string Namespace { get; }
  internal IReadOnlyList<string> Names { get; }
  private string QualifiedName { get; }
  public override object BackingValue => this;
  public override string Name => Names.Count == 0 ? "" : Names[Names.Count - 1];
  public override string FullName => Render();
  public override bool IsTruthy => Names.Count != 0;
  public override string Render() => QualifiedName ?? (Namespace.Length == 0 ? "" : Namespace + ".") + string.Join(".", Names);
  public override object Unwrap() => Render();
  public override object Select(string path) {
    if (int.TryParse(path, out var index))
      return index >= 0 && index < _typeArguments.Length ? _typeArguments[index] : null;
    for (var parameterIndex = 0; parameterIndex < _typeParameterNames.Length; parameterIndex++)
      if (string.Equals(_typeParameterNames[parameterIndex], path, StringComparison.OrdinalIgnoreCase))
        return parameterIndex < _typeArguments.Length ? _typeArguments[parameterIndex] : null;
    return null;
  }
  public override bool Is(string type) {
    var expected = NormalizeTypeName(type);
    return _assignableTypes.Contains(expected, StringComparer.Ordinal) ||
      string.Equals(NormalizeTypeName(Render()), expected, StringComparison.Ordinal) ||
      string.Equals(Name, expected, StringComparison.Ordinal);
  }
  public override bool Has(object member) => _members.Contains(Convert.ToString(member) ?? "", StringComparer.Ordinal);
  public override bool Equals(object obj) => obj is DetachedTypeValue other && Namespace == other.Namespace && QualifiedName == other.QualifiedName && Names.SequenceEqual(other.Names) && _members.SequenceEqual(other._members) && _assignableTypes.SequenceEqual(other._assignableTypes) && _typeArguments.SequenceEqual(other._typeArguments) && _typeParameterNames.SequenceEqual(other._typeParameterNames);
  public override int GetHashCode() {
    var hash = StringComparer.Ordinal.GetHashCode(Namespace);
    hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(QualifiedName ?? ""));
    foreach (var name in Names) hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(name));
    foreach (var member in _members) hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(member));
    foreach (var type in _assignableTypes) hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(type));
    foreach (var argument in _typeArguments) hash = unchecked(hash * 31 + (argument?.GetHashCode() ?? 0));
    foreach (var parameter in _typeParameterNames) hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(parameter));
    return hash;
  }

  private static string NormalizeTypeName(string value) =>
    (value ?? "").Replace("global::", "").Trim();
}

internal sealed class DetachedSemanticValue : MixinValue {
  private readonly string[] _members;
  private readonly HashSet<MixinValueTrait> _traits;
  internal DetachedSemanticValue(string name, string fullName, string render, string visibility, DetachedTypeValue type, IEnumerable<string> members, IEnumerable<MixinValueTrait> traits) {
    NameValue = name;
    FullNameValue = fullName;
    RenderValue = render ?? name ?? "null";
    VisibilityValue = visibility;
    Type = type;
    _members = (members ?? Array.Empty<string>()).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToArray();
    _traits = new HashSet<MixinValueTrait>(traits ?? Array.Empty<MixinValueTrait>());
  }
  private string NameValue { get; }
  private string FullNameValue { get; }
  private string RenderValue { get; }
  private string VisibilityValue { get; }
  internal DetachedTypeValue Type { get; }
  public override object BackingValue => this;
  public override string Name => NameValue;
  public override string FullName => FullNameValue;
  public override string Visibility => VisibilityValue;
  public override bool IsTruthy => true;
  public override string Render() => RenderValue;
  public override object Unwrap() => RenderValue;
  public override object Select(string path) => string.Equals(path, "type", StringComparison.Ordinal)
    ? Type
    : Type?.Select(path);
  public override bool Has(object member) => _members.Contains(Convert.ToString(member) ?? "", StringComparer.Ordinal) || Type?.Has(member) == true;
  public override bool Is(string type) => Type?.Is(type) == true;
  public override bool HasTrait(MixinValueTrait trait) => _traits.Contains(trait);
  public override bool Equals(object obj) => obj is DetachedSemanticValue other && NameValue == other.NameValue && FullNameValue == other.FullNameValue && RenderValue == other.RenderValue && VisibilityValue == other.VisibilityValue && Equals(Type, other.Type) && _members.SequenceEqual(other._members) && _traits.SetEquals(other._traits);
  public override int GetHashCode() {
    var hash = StringComparer.Ordinal.GetHashCode(NameValue ?? "");
    hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(FullNameValue ?? ""));
    hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(RenderValue ?? ""));
    hash = unchecked(hash * 31 + StringComparer.Ordinal.GetHashCode(VisibilityValue ?? ""));
    return unchecked(hash * 31 + (Type?.GetHashCode() ?? 0));
  }
}

internal sealed class NullMixinValue : MixinValue {
  internal static readonly NullMixinValue Instance = new();
  private NullMixinValue() { }
  public override object BackingValue => null;
  public override bool IsTruthy => false;
  public override string Render() => "null";
  public override bool Equals(object obj) => obj is NullMixinValue;
  public override int GetHashCode() => 0;
}

internal sealed class BooleanMixinValue : MixinValue {
  internal static readonly BooleanMixinValue True = new(true);
  internal static readonly BooleanMixinValue False = new(false);
  private readonly bool _value;
  private BooleanMixinValue(bool value) { _value = value; }
  public override object BackingValue => _value;
  public override bool IsTruthy => _value;
  public override string Render() => _value ? "true" : "false";
  public override bool Equals(object obj) => obj is BooleanMixinValue other && _value == other._value;
  public override int GetHashCode() => _value.GetHashCode();
}

internal sealed class StringMixinValue : MixinValue {
  private readonly string _value;
  internal StringMixinValue(string value) { _value = value ?? ""; }
  public override object BackingValue => _value;
  public override bool IsTruthy => _value.Length != 0 &&
    !string.Equals(_value, "false", StringComparison.OrdinalIgnoreCase) &&
    !string.Equals(_value, "null", StringComparison.OrdinalIgnoreCase);
  public override string Render() => _value;
  public override bool Equals(object obj) => obj is StringMixinValue other && _value == other._value;
  public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_value);
}

internal sealed class ObjectMixinValue : MixinValue {
  private readonly object _value;
  internal ObjectMixinValue(object value) { _value = value; }
  public override object BackingValue => _value;
  public override bool IsTruthy => _value is not null;
  public override string Render() => Convert.ToString(_value, System.Globalization.CultureInfo.InvariantCulture) ?? "null";
  public override IMixinValue Unlink() => _value switch {
    null => NullMixinValue.Instance,
    bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
    string text => new StringMixinValue(text),
    _ => new StringMixinValue(Render())
  };
  public override bool Equals(object obj) => obj is ObjectMixinValue other && Equals(_value, other._value);
  public override int GetHashCode() => _value?.GetHashCode() ?? 0;
}

internal sealed class FailedMixinValue : MixinValue {
  internal FailedMixinValue(string error) { Error = error ?? "expression failed"; }
  internal string Error { get; }
  public override object BackingValue => this;
  public override bool Exists => false;
  public override bool IsTruthy => false;
  public override string Render() => "null";
  public override object Unwrap() => null;
  public override bool Equals(object obj) => obj is FailedMixinValue other && Error == other.Error;
  public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Error);
}
