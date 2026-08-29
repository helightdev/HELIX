using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HelixSourceGenerator.Shared;
using Microsoft.CodeAnalysis;

namespace HelixSourceGenerator.Language;

/// <summary>Allocation-light adapter from Roslyn values to interpreter value semantics.</summary>
internal readonly struct RoslynMixinValue : IMixinValue {
  private readonly RoslynMixinExpressionContext _context;

  internal RoslynMixinValue(RoslynMixinExpressionContext context, object value) {
    _context = context;
    Value = value;
  }

  public object Value { get; }
  private ISymbol Symbol => Value as ISymbol;
  private ITypeSymbol Type => RoslynMixinExpressionContext.TypeValueOf(Value);

  public ISymbol RoslynSymbol => Symbol;
  public ITypeSymbol RoslynType => Type;
  public string Visibility {
    get {
      var symbol = Symbol ?? Type;
      return symbol is null || symbol.DeclaredAccessibility == Accessibility.NotApplicable
        ? null
        : GeneratorAnalysis.AccessibilityText(symbol.DeclaredAccessibility);
    }
  }
  public string Name => RoslynMixinExpressionContext.NameOf(Value);
  public string FullName => RoslynMixinExpressionContext.FullNameOf(Value);
  public bool Exists => Value is not null;
  public bool IsTruthy => RoslynMixinExpressionContext.IsTruthy(Value);

  public string Render() => RoslynMixinExpressionContext.TryComparableText(Value, out var text)
    ? text
    : Convert.ToString(Value);

  public void Fingerprint(MixinFingerprintBuilder builder) {
    builder.Append(nameof(RoslynMixinValue));
    builder.Append(Render());
  }

  public object Unwrap() => RoslynMixinExpressionContext.Unwrap(Value);

  public bool TryGetText(out string text) => RoslynMixinExpressionContext.TryComparableText(Value, out text);

  public object Select(string path) => _context.SelectMember(Value, path) ??
    RoslynMixinExpressionContext.SelectTypeArgument(Value, path);

  public bool Is(string type) => Type is not null && _context.IsOrInherits(Type, type);

  public bool Has(object member) {
    return Type is not null && RoslynMixinExpressionContext.HasConcreteMember(Type, Convert.ToString(member));
  }

  public bool EqualsTo(object expected) {
    return RoslynMixinExpressionContext.EqualTo(Value, Convert.ToString(expected));
  }

  public bool Matches(string pattern, out string error) {
    error = null;
    if (!RoslynMixinExpressionContext.TryComparableText(Value, out var text)) return false;
    try {
      return Regex.IsMatch(text, pattern);
    } catch (ArgumentException exception) {
      error = "invalid regular expression: " + exception.Message;
      return false;
    }
  }

  public bool HasSameSignature(string expected) {
    var actual = _context.ResolveCallable(Value);
    var target = _context.ResolveCallable(expected);
    return actual is not null && target is not null &&
      RoslynMixinExpressionContext.HaveSameSignature(actual, target);
  }

  public bool IsWireable(string from, string to) {
    var source = _context.ResolveCallable(from);
    var target = _context.ResolveCallable(to);
    return source is not null && target is not null && _context.TryWireParameters(source, target, out _);
  }

  public bool TryWire(string to, out string arguments, out string error) {
    return _context.TryWire(Render(), to, out arguments, out error);
  }

  public bool TryApplyPropStruct(MixinExpressionProperty property, out object result, out string error) {
    return _context.TryApplyPropStructProperty(Value, property, out result, out error);
  }

  public MixinExpressionTable Attributes(string type, bool exact) {
    var table = new MixinExpressionTable();
    var index = 0;
    foreach (var attribute in SourceAttributes()) {
      if (type is not null && !(exact
        ? MatchesExact(attribute.AttributeClass, type)
        : attribute.AttributeClass is not null && _context.IsOrInherits(attribute.AttributeClass, type))) continue;
      table = table.Put(
        (index++).ToString(CultureInfo.InvariantCulture),
        new RoslynMixinValue(_context, attribute)
      );
    }
    return table;
  }

  public object FirstAttribute(string type) {
    foreach (var attribute in SourceAttributes()) {
      if (attribute.AttributeClass is not null && _context.IsOrInherits(attribute.AttributeClass, type))
        return attribute;
    }
    return null;
  }

  private IEnumerable<AttributeData> SourceAttributes() {
    switch (Value) {
      case ISymbol symbol: return symbol.GetAttributes();
      case AttributeData attribute: {
        var attributeType = attribute.AttributeClass;
        if (attributeType is not null) return attributeType.GetAttributes();
        break;
      }
    }
    var type = Type;
    return type is null ? Array.Empty<AttributeData>() : type.GetAttributes();
  }

  private static bool MatchesExact(ITypeSymbol type, string expected) {
    return type is not null && (
      string.Equals(type.Name, expected, StringComparison.Ordinal) ||
      string.Equals(type.ToDisplayString(), expected, StringComparison.Ordinal) ||
      string.Equals(
        type.ToDisplayString().Replace("global::", ""), expected.Replace("global::", ""), StringComparison.Ordinal
      )
    );
  }

  public ITypeSymbol ResolveType(string name) => _context.ResolveType(name);

  public bool IsGeneratedType(string name) => _context.IsGeneratedStructType(name);

  public IMixinValue Unlink() {
    if (Value is ISymbol || Value is TypedConstant)
      return DetachedSemantic();
    var value = Unwrap();
    if (value is null) return MixinValue.From(null);
    if (value is bool boolean) return MixinValue.From(boolean);
    if (value is string text) return MixinValue.From(text);
    return DetachedSemantic();
  }

  private IMixinValue DetachedSemantic() {
    var type = DetachedType(Type);
    var members = Members(Type);
    var traits = Enum.GetValues(typeof(MixinValueTrait)).Cast<MixinValueTrait>().Where(HasTrait).ToArray();
    var render = Render();
    if (Value is TypedConstant &&
      _context.TryRenderValue(Value, MixinExpressionRoot.Attribute, out var constantExpression, out _)
    ) render = constantExpression;
    return new DetachedSemanticValue(Name, FullName, render, Visibility, type, members, traits);
  }

  private static DetachedTypeValue DetachedType(ITypeSymbol type) {
    if (type is null) return null;
    var names = new Stack<string>();
    for (var current = type; current is not null; current = current.ContainingType) names.Push(current.Name);
    return new DetachedTypeValue(
      type.ContainingNamespace?.IsGlobalNamespace == false ? type.ContainingNamespace.ToDisplayString() : "",
      [.. names],
      Members(type),
      type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", ""),
      AssignableTypes(type),
      type is INamedTypeSymbol named
        ? named.TypeArguments.Select(DetachedType).ToArray()
        : Array.Empty<DetachedTypeValue>(),
      type is INamedTypeSymbol generic
        ? generic.TypeParameters.Select(item => item.Name).ToArray()
        : Array.Empty<string>()
    );
  }

  private static string[] AssignableTypes(ITypeSymbol type) {
    var result = new HashSet<string>(StringComparer.Ordinal);

    Add(type);
    if (type is not INamedTypeSymbol named) return [.. result.OrderBy(item => item, StringComparer.Ordinal)];
    for (var current = named.BaseType; current is not null; current = current.BaseType) Add(current);
    foreach (var implemented in named.AllInterfaces) Add(implemented);
    return [.. result.OrderBy(item => item, StringComparer.Ordinal)];

    void Add(ITypeSymbol item) {
      if (item is null) return;
      result.Add(item.Name);
      result.Add(item.ToDisplayString().Replace("global::", ""));
      result.Add(item.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", ""));
    }
  }

  private static string[] Members(ITypeSymbol type) {
    if (type is not INamedTypeSymbol named) return [];
    return [.. named.GetMembers().Select(item => item.Name).Distinct(StringComparer.Ordinal)];
  }

  public bool HasTrait(MixinValueTrait trait) {
    if (Value is null) return false;
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
      MixinValueTrait.Concrete => Value switch {
        INamedTypeSymbol named => named.TypeKind != TypeKind.Interface && !named.IsAbstract,
        IMethodSymbol method => !method.IsAbstract && !method.IsVirtual,
        _ => Symbol is not null
      },
      MixinValueTrait.Partial => RoslynMixinExpressionContext.IsPartialSymbol(Value),
      MixinValueTrait.Generic => Value is IMethodSymbol method
        ? method.TypeParameters.Length != 0
        : Type is INamedTypeSymbol type && type.TypeParameters.Length != 0,
      MixinValueTrait.Struct => Type?.TypeKind == TypeKind.Struct,
      MixinValueTrait.Class => Type?.IsReferenceType == true,
      _ => false
    };
  }
}