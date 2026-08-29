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
  private readonly RoslynMixinContext _context;

  internal RoslynMixinValue(RoslynMixinContext context, object value) {
    _context = context;
    Value = value;
  }

  public object Value { get; }
  private ISymbol Symbol => Value as ISymbol;
  private ITypeSymbol Type => RoslynMixinContext.TypeValueOf(Value);
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
  public string Name => RoslynMixinContext.NameOf(Value);
  public string FullName => RoslynMixinContext.FullNameOf(Value);
  public bool Exists => Value is not null;
  public bool IsTruthy => Value switch {
    null => false,
    TypedConstant constant => constant.Kind != TypedConstantKind.Error &&
      !constant.IsNull && constant.Value is not false,
    ImplicitMixinValue value => value.Value is not null && value.Value is not false,
    bool value => value,
    _ => true
  };

  public string Render() {
    return RoslynMixinContext.TryComparableText(Value, out var text) ? text : Convert.ToString(Value);
  }

  public void Fingerprint(MixinFingerprintBuilder builder) {
    builder.Append(nameof(RoslynMixinValue));
    builder.Append(Render());
  }

  public IMixinValue Unwrap() {
    return MixinValue.From(RoslynMixinContext.Unwrap(Value), _context);
  }

  public bool TryGetText(out string text) {
    return RoslynMixinContext.TryComparableText(Value, out text);
  }

  public IMixinValue Select(string path) {
    return MixinValue.From(
      _context.SelectMember(Value, path) ?? RoslynMixinContext.SelectTypeArgument(Value, path), _context
    );
  }

  public bool Is(string type) {
    return Type is not null && _context.IsOrInherits(Type, type);
  }

  public bool Has(IMixinValue member) {
    return Type is not null && RoslynMixinContext.HasConcreteMember(Type, member.Render());
  }

  public bool EqualsTo(IMixinValue expected) {
    return RoslynMixinContext.EqualTo(Value, expected.Render());
  }

  public bool Matches(string pattern, out string error) {
    error = null;
    if (!RoslynMixinContext.TryComparableText(Value, out var text)) return false;
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
    return actual is not null && target is not null && RoslynMixinContext.HaveSameSignature(actual, target);
  }

  public bool IsWireable(string from, string to) {
    var source = _context.ResolveCallable(from);
    var target = _context.ResolveCallable(to);
    return source is not null && target is not null && _context.TryWireParameters(source, target, out _);
  }

  public bool TryWire(string to, out string arguments, out string error) {
    return _context.TryWire(Render(), to, out arguments, out error);
  }

  public bool TryApplyPropStruct(MixinExpressionProperty property, out IMixinValue result, out string error) {
    if (_context.TryApplyPropStructProperty(Value, property, out var raw, out error)) {
      result = MixinValue.From(raw, _context);
      return true;
    }
    result = NullMixinValue.Instance;
    return false;
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

  public IMixinValue FirstAttribute(string type) {
    foreach (var attribute in SourceAttributes()) {
      if (attribute.AttributeClass is not null && _context.IsOrInherits(attribute.AttributeClass, type))
        return new RoslynMixinValue(_context, attribute);
    }
    return NullMixinValue.Instance;
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

  public ITypeSymbol ResolveType(string name) {
    return _context.ResolveType(name);
  }

  public bool IsGeneratedType(string name) {
    return _context.IsGeneratedStructType(name);
  }

  public IMixinValue Unlink() {
    if (Value is ISymbol or TypedConstant) return DetachedSemantic();
    var value = Unwrap();
    return value is NullMixinValue or BooleanMixinValue or StringMixinValue ? value : DetachedSemantic();
  }

  private IMixinValue DetachedSemantic() {
    var type = DetachedType(Type);
    var members = Members(Type);
    var traits = Enum.GetValues(typeof(MixinValueTrait)).Cast<MixinValueTrait>().Where(HasTrait).ToArray();
    var render = Render();
    if (Value is TypedConstant &&
      _context.TryRenderValue(this, MixinExpressionRoot.Attribute, out var constantExpression, out _)
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
      MixinValueTrait.Exposed => Symbol?.DeclaredAccessibility
        is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal,
      MixinValueTrait.Top => Type?.ContainingType is null,
      MixinValueTrait.Concrete => Value switch {
        INamedTypeSymbol named => named.TypeKind != TypeKind.Interface && !named.IsAbstract,
        IMethodSymbol method => !method.IsAbstract && !method.IsVirtual,
        _ => Symbol is not null
      },
      MixinValueTrait.Partial => RoslynMixinContext.IsPartialSymbol(Value),
      MixinValueTrait.Generic => Value is IMethodSymbol method
        ? method.TypeParameters.Length != 0
        : Type is INamedTypeSymbol type && type.TypeParameters.Length != 0,
      MixinValueTrait.Struct => Type?.TypeKind == TypeKind.Struct,
      MixinValueTrait.Class => Type?.IsReferenceType == true,
      _ => false
    };
  }
}