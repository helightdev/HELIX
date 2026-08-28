using System;
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
  bool HasTrait(MixinValueTrait trait);
  ITypeSymbol ResolveType(string name);
  bool IsGeneratedType(string name);
}

public abstract class MixinValue : IMixinValue {
  public abstract object BackingValue { get; }
  public virtual ISymbol RoslynSymbol => null;
  public virtual ITypeSymbol RoslynType => null;
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
  public virtual bool HasTrait(MixinValueTrait trait) => false;
  public virtual ITypeSymbol ResolveType(string name) => null;
  public virtual bool IsGeneratedType(string name) => false;

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
}

internal sealed class NullMixinValue : MixinValue {
  internal static readonly NullMixinValue Instance = new();
  private NullMixinValue() { }
  public override object BackingValue => null;
  public override bool IsTruthy => false;
  public override string Render() => "null";
}

internal sealed class BooleanMixinValue : MixinValue {
  internal static readonly BooleanMixinValue True = new(true);
  internal static readonly BooleanMixinValue False = new(false);
  private readonly bool _value;
  private BooleanMixinValue(bool value) { _value = value; }
  public override object BackingValue => _value;
  public override bool IsTruthy => _value;
  public override string Render() => _value ? "true" : "false";
}

internal sealed class StringMixinValue : MixinValue {
  private readonly string _value;
  internal StringMixinValue(string value) { _value = value ?? ""; }
  public override object BackingValue => _value;
  public override bool IsTruthy => _value.Length != 0 &&
    !string.Equals(_value, "false", StringComparison.OrdinalIgnoreCase) &&
    !string.Equals(_value, "null", StringComparison.OrdinalIgnoreCase);
  public override string Render() => _value;
}

internal sealed class ObjectMixinValue : MixinValue {
  private readonly object _value;
  internal ObjectMixinValue(object value) { _value = value; }
  public override object BackingValue => _value;
  public override bool IsTruthy => _value is not null;
  public override string Render() => Convert.ToString(_value, System.Globalization.CultureInfo.InvariantCulture) ?? "null";
}
