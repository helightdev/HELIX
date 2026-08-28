using System;
using System.Text.RegularExpressions;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;

namespace HELIX.SourceGen;

/// <summary>Allocation-light adapter from Roslyn values to interpreter value semantics.</summary>
internal readonly struct RoslynMixinValue : IMixinValue {
  private readonly RoslynMixinExpressionContext _context;
  private readonly object _value;

  internal RoslynMixinValue(RoslynMixinExpressionContext context, object value) {
    _context = context;
    _value = value;
  }

  private ISymbol Symbol => _value as ISymbol;
  private ITypeSymbol Type => RoslynMixinExpressionContext.TypeValueOf(_value);

  public object BackingValue => _value;
  public ISymbol RoslynSymbol => Symbol;
  public ITypeSymbol RoslynType => Type;
  public string Name => RoslynMixinExpressionContext.NameOf(_value);
  public string FullName => RoslynMixinExpressionContext.FullNameOf(_value);
  public bool Exists => _value is not null;
  public bool IsTruthy => RoslynMixinExpressionContext.IsTruthy(_value);
  public string Render() => RoslynMixinExpressionContext.TryComparableText(_value, out var text)
    ? text
    : Convert.ToString(_value);
  public object Unwrap() => RoslynMixinExpressionContext.Unwrap(_value);
  public bool TryGetText(out string text) => RoslynMixinExpressionContext.TryComparableText(_value, out text);
  public object Select(string path) => RoslynMixinExpressionContext.SelectTypeArgument(_value, path);
  public bool Is(string type) => Type is not null && _context.IsOrInherits(Type, type);
  public bool Has(object member) => Type is not null &&
    RoslynMixinExpressionContext.HasConcreteMember(Type, Convert.ToString(member));
  public bool EqualsTo(object expected) =>
    RoslynMixinExpressionContext.EqualTo(_value, Convert.ToString(expected));

  public bool Matches(string pattern, out string error) {
    error = null;
    if (!RoslynMixinExpressionContext.TryComparableText(_value, out var text)) return false;
    try {
      return Regex.IsMatch(text, pattern);
    } catch (ArgumentException exception) {
      error = "invalid regular expression: " + exception.Message;
      return false;
    }
  }

  public bool HasSameSignature(string expected) {
    var actual = _context.ResolveCallable(_value);
    var target = _context.ResolveCallable(expected);
    return actual is not null && target is not null &&
      RoslynMixinExpressionContext.HaveSameSignature(actual, target);
  }

  public bool IsWireable(string from, string to) {
    var source = _context.ResolveCallable(from);
    var target = _context.ResolveCallable(to);
    return source is not null && target is not null && _context.TryWireParameters(source, target, out _);
  }

  public bool TryWire(string to, out string arguments, out string error) =>
    _context.TryWire(Render(), to, out arguments, out error);

  public bool TryApplyPropStruct(MixinExpressionProperty property, out object result, out string error) =>
    _context.TryApplyPropStructProperty(_value, property, out result, out error);

  public ITypeSymbol ResolveType(string name) => _context.ResolveType(name);
  public bool IsGeneratedType(string name) => _context.IsGeneratedStructType(name);

  public bool HasTrait(MixinValueTrait trait) {
    if (_value is null) return false;
    return trait switch {
    MixinValueTrait.Self => Type is not null && SymbolEqualityComparer.Default.Equals(Type, _context.CurrentType),
    MixinValueTrait.Ref => Symbol is IParameterSymbol { RefKind: RefKind.Ref },
    MixinValueTrait.In => Symbol is IParameterSymbol { RefKind: RefKind.In },
    MixinValueTrait.Out => Symbol is IParameterSymbol { RefKind: RefKind.Out },
    MixinValueTrait.InOut => Symbol is IParameterSymbol { RefKind: RefKind.In or RefKind.Out },
    MixinValueTrait.Argument => Symbol is IParameterSymbol { RefKind: RefKind.None },
    MixinValueTrait.Static => Symbol?.IsStatic == true,
    MixinValueTrait.Async => Symbol is IMethodSymbol { IsAsync: true },
    MixinValueTrait.Public => Symbol?.DeclaredAccessibility == Accessibility.Public,
    MixinValueTrait.Exposed => Symbol?.DeclaredAccessibility is Accessibility.Public or
      Accessibility.Internal or Accessibility.ProtectedOrInternal,
    MixinValueTrait.Top => Type?.ContainingType is null,
    MixinValueTrait.Concrete => _value switch {
      INamedTypeSymbol named => named.TypeKind != TypeKind.Interface && !named.IsAbstract,
      IMethodSymbol method => !method.IsAbstract && !method.IsVirtual,
      _ => Symbol is not null
    },
    MixinValueTrait.Partial => RoslynMixinExpressionContext.IsPartialSymbol(_value),
    MixinValueTrait.Generic => _value is IMethodSymbol method
      ? method.TypeParameters.Length != 0
      : Type is INamedTypeSymbol type && type.TypeParameters.Length != 0,
    MixinValueTrait.Struct => Type?.TypeKind == TypeKind.Struct,
    MixinValueTrait.Class => Type?.IsReferenceType == true,
      _ => false
    };
  }
}
