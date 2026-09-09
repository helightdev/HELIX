using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Hix.Compiler;
using Hix.Env;
using Hix.Roslyn;

namespace Hix.Runtime;

/// <summary>Roslyn host services for an already lowered mixin program.</summary>
public partial class HixRoslynContext : HixContext {
  private readonly AttributeData _attribute;
  private readonly RoslynValueCache _attributeValues;
  private readonly HixValueDictionary _committedTargetVariables;
  private readonly CSharpCompilation _compilation;
  private readonly ISymbol _target;

  private readonly RoslynValueCache _targetValues;
  private readonly RoslynValueCache _thisValues;
  private readonly Dictionary<object, RoslynValueCache> _valueOwners =
    new(RoslynValueOwnerComparer.Instance);

  public HixRoslynContext(
    INamedTypeSymbol thisType, ISymbol target, AttributeData attribute,
    CSharpCompilation compilation,
    HixExpressionPreparedState preparedExpressions = null,
    RoslynHostExpressionCache hostValues = null, HixBackend backend = null
  )
    : base(backend, preparedExpressions?.StringPool ?? new HixStringPoolBuilder().Freeze()) {
    CurrentType = thisType;
    _target = target;
    _attribute = attribute;
    _compilation = compilation;
    hostValues ??= new RoslynHostExpressionCache();
    _thisValues = hostValues.ForThis(thisType);
    _targetValues = hostValues.ForTarget(target);
    _attributeValues = hostValues.ForAttribute(attribute);
    _committedTargetVariables = hostValues.TargetVariables(target);
    TargetVariables.ReplaceWith(_committedTargetVariables);
  }

  public INamedTypeSymbol CurrentType { get; }

  public void CommitTargetVariables() => _committedTargetVariables.ReplaceWith(TargetVariables);


  public bool TryGetDerived(RoslynHixValue source, string key, out IHixValue value) {
    if (!_valueOwners.TryGetValue(source.Value, out var cache)) cache = _targetValues;
    if (!cache.TryGetDerived(source.Value, key, out value)) return false;
    RegisterOwner(value, cache);
    return true;
  }

  public void StoreDerived(RoslynHixValue source, string key, IHixValue value) {
    if (!_valueOwners.TryGetValue(source.Value, out var cache)) cache = _targetValues;
    cache.StoreDerived(source.Value, key, value);
    RegisterOwner(value, cache);
  }

  public IHixValue SelectValue(HixThread thread, RoslynHixValue source, HixString member) {
    var name = member.Resolve(thread.Strings);
    var key = "#" + name;
    if (TryGetDerived(source, key, out var cached)) return cached;
    var selected = SelectMember(source.Value, name);
    IHixValue result = selected is null or TypedConstant {IsNull: true} ? NullHixValue.Instance : new RoslynHixValue(selected);
    StoreDerived(source, key, result);
    return result;
  }

  private void RegisterOwner(IHixValue value, RoslynValueCache cache) {
    switch (value) {
      case TupleHixValue tuple:
        foreach (var item in tuple.Values) RegisterOwner(item, cache);
        break;
      case RoslynHixValue roslyn:
        _valueOwners[roslyn.Value] = cache;
        break;
      case HixTableValue table:
        foreach (var item in table.Entries) RegisterOwner(item.Value, cache);
        break;
    }
  }

  public IMethodSymbol Callable(HixThread thread, IHixValue value) {
    value = thread.Evaluate(value);
    if (value is RoslynHixValue { Value: IMethodSymbol method }) return method;
    var text = value.Render(thread).Resolve(thread.Strings);
    if (ResolveType(text) is INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke })
      return invoke;
    var methodName = text.Substring(text.LastIndexOf('.') + 1);
    var methods = MethodsInHierarchy(CurrentType, methodName).ToArray();
    return methods.Length == 1 ? methods[0] : null;
  }

  public HixString NameOfService(HixThread thread, IHixValue value) {
    return value is RoslynHixValue roslyn
      ? thread.ResolveString(
        (roslyn.Value as ISymbol)?.Name ??
        (roslyn.Value as AttributeData)?.AttributeClass?.Name ?? ComparableText(roslyn.Value)
      )
      : value is DetachedSemanticHixValue detached
        ? detached.TypeName
        : value.Render(thread);
  }

  public bool IsTypeService(HixThread thread, IHixValue value, HixString requested) {
    using var profile = HixProfiler.Measure("roslyn.is_type");
    if (value is DetachedSemanticHixValue detached) {
      var expected = requested.Resolve(thread.Strings).Replace("global::", "");
      var candidate = detached.Render(thread).Resolve(thread.Strings);
      return candidate == expected || detached.TypeName.Resolve(thread.Strings) == expected;
    }
    if (value is not RoslynHixValue roslyn || TypeOf(roslyn.Value) is not INamedTypeSymbol type) return false;
    var name = requested.Resolve(thread.Strings).Replace("global::", "");

    if (MatchesTypeName(type, name)) return true;
    for (var current = type.BaseType; current is not null; current = current.BaseType) {
      if (MatchesTypeName(current, name))
        return true;
    }
    foreach (var item in type.AllInterfaces) if (MatchesTypeName(item, name)) return true;
    return false;
  }

  private static bool MatchesTypeName(ITypeSymbol candidate, string name) => candidate is not null && (
    candidate.Name == name || candidate.ToDisplayString() == name ||
    candidate.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal) == name);

  public bool HasTraitService(HixThread thread, IHixValue value, HixString requested) {
    using var profile = HixProfiler.Measure("roslyn.has_trait");
    var name = requested.Resolve(thread.Strings);
    if (value is DetachedSemanticHixValue) return false;
    return value is RoslynHixValue roslyn && SemanticTraits(roslyn.Value).Contains(name, StringComparer.Ordinal);
  }

  public object UnlinkSemanticSnapshot(HixThread thread, IHixValue value) {
    if (value is not RoslynHixValue roslyn) return value.Unlink(thread);
    if (!_valueOwners.TryGetValue(roslyn.Value, out var cache)) cache = _targetValues;
    return cache.Snapshot(roslyn, () => roslyn.Unlink(thread));
  }

  public IHixValue DetachSemanticValue(HixThread thread, IHixValue value) {
    value = thread.Evaluate(value);
    if (value is RoslynHixValue roslyn) {
      var detached = thread.UnlinkSnapshot(roslyn);
      return detached is DetachedSemanticData semantic
        ? DetachedSemanticHixValue.Materialize(semantic)
        : thread.DetachCore(
          detached switch {
            IHixValue typed => typed,
            null => NullHixValue.Instance,
            bool boolean => boolean ? BooleanHixValue.True : BooleanHixValue.False,
            string text => new LiteralHixValue(HixString.Dynamic(text)),
            _ => new ObjectHixValue(detached)
          }
        );
    }
    return thread.DetachCore(value);
  }

  public static IReadOnlyList<string> SemanticTraits(object value) {
    using var profile = HixProfiler.Measure("roslyn.semantic_traits");
    var result = new List<string>();
    var type = TypeOf(value);
    var symbol = value as ISymbol;
    if (type?.IsReferenceType == true) result.Add("class");
    if (type?.IsValueType == true) result.Add("struct");
    if (type is INamedTypeSymbol { IsGenericType: true }) result.Add("generic");
    if (type is INamedTypeSymbol named && GeneratorAnalysis.IsPartial(named)) result.Add("partial");
    if (symbol?.IsStatic == true) result.Add("static");
    if (symbol?.DeclaredAccessibility == Accessibility.Public) result.Add("public");
    if (symbol is IMethodSymbol { IsAsync: true }) result.Add("async");
    if (symbol is IParameterSymbol parameter) {
      result.Add(
        parameter.RefKind switch {
          RefKind.Ref => "ref",
          RefKind.In => "in",
          RefKind.Out => "out",
          _ => "argument"
        }
      );
    }
    return result;
  }

  public IHixValue UnwrapService(HixThread thread, IHixValue value) {
    using var profile = HixProfiler.Measure("roslyn.unwrap");
    switch (value) {
      // Defaults recovered from a referenced attribute declaration are CLR constants rather
      // than TypedConstant instances. Normalize both representations to the same language
      // values so execution does not depend on whether Roslyn supplied source or metadata.
      case RoslynHixValue { Value: null }:
        return NullHixValue.Instance;
      case RoslynHixValue { Value: bool boolean }:
        return boolean ? BooleanHixValue.True : BooleanHixValue.False;
      case RoslynHixValue { Value: string text }:
        return new LiteralHixValue(thread.ResolveString(text));
      case RoslynHixValue { Value: char character }:
        return new LiteralHixValue(thread.ResolveString(character.ToString()));
      case RoslynHixValue { Value: ITypeSymbol type }:
        return new LiteralHixValue(thread.ResolveString(type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal)));
      case RoslynHixValue { Value: byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal } scalar:
        return new NumberHixValue(Convert.ToDouble(scalar.Value, CultureInfo.InvariantCulture));
      case RoslynHixValue { Value: TypedConstant constant }
        when constant.IsNull || constant.Kind == TypedConstantKind.Error:
        return NullHixValue.Instance;
      case RoslynHixValue { Value: TypedConstant constant }:
        if (constant.Kind == TypedConstantKind.Array)
          return new TupleHixValue(constant.Values.Select(item => thread.Unwrap(new RoslynHixValue(item))).ToArray());
        return constant.Value switch {
          bool boolean => boolean ? BooleanHixValue.True : BooleanHixValue.False,
          string text => new LiteralHixValue(thread.ResolveString(text)),
          char character => new LiteralHixValue(thread.ResolveString(character.ToString())),
          ITypeSymbol type => new LiteralHixValue(thread.ResolveString(type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal))),
          byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal =>
            new NumberHixValue(Convert.ToDouble(constant.Value, CultureInfo.InvariantCulture)),
          _ => thread.Error("unsupported host constant")
        };
      case DetachedSemanticHixValue detached: return new LiteralHixValue(detached.Render(thread));
      default: return value;
    }
  }

  public IHixValue AttributesService(HixThread thread, IHixValue value, HixString requested, bool exact, bool first) {
    using var profile = HixProfiler.Measure("roslyn.attributes");
    if (value is not RoslynHixValue roslyn) return first ? NullHixValue.Instance : TupleHixValue.Empty;
    var expected = requested.IsInterned || requested.DynamicValue is not null
      ? requested.Resolve(thread.Strings)
      : null;
    var source = roslyn.Value switch {
      ISymbol symbol => symbol.GetAttributes(),
      AttributeData attribute => attribute.AttributeClass is { } attributeType
        ? attributeType.GetAttributes()
        : Enumerable.Empty<AttributeData>(),
      _ => TypeOf(roslyn.Value) is { } valueType
        ? valueType.GetAttributes()
        : Enumerable.Empty<AttributeData>()
    };
    var matches = source.Where(attribute => expected is null || (attribute.AttributeClass is { } type &&
      (exact ? TypeMatches(type, expected) : IsOrInherits(type, expected)))
    ).ToArray();
    RoslynValueCache owner = null;
    if (roslyn.Value is not null) _valueOwners.TryGetValue(roslyn.Value, out owner);
    owner ??= _targetValues;
    if (first) {
      IHixValue result = matches.Length == 0 ? NullHixValue.Instance : new RoslynHixValue(matches[0]);
      RegisterOwner(result, owner);
      return result;
    }
    var entries = matches.Select((item, index) => {
      IHixValue result = new RoslynHixValue(item);
      RegisterOwner(result, owner);
      return result;
    }
    ).ToArray();
    return new TupleHixValue(entries);
  }

  private bool IsOrInherits(ITypeSymbol type, string expected) {
    if (TypeMatches(type, expected)) return true;
    if (type is not INamedTypeSymbol named) return false;
    for (var current = named.BaseType; current is not null; current = current.BaseType) {
      if (TypeMatches(current, expected))
        return true;
    }
    return named.AllInterfaces.Any(item => TypeMatches(item, expected));
  }

  private static bool TypeMatches(ITypeSymbol type, string expected) {
    using var profile = HixProfiler.Measure("roslyn.type_matches");
    return type is not null && (
      type.Name == expected || type.ToDisplayString() == expected.Replace("global::", "") ||
      type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal) ==
      expected.Replace("global::", "")
    );
  }

  public IHixValue ResolveHostService(HixThread thread, HixExpressionRoot root, HixString member) {
    using var profile = HixProfiler.Measure("roslyn.resolve_host");
    var memberName = member.Resolve(thread.Strings);
    var name = memberName;
    var cache = root switch {
      HixExpressionRoot.This => _thisValues,
      HixExpressionRoot.Attribute => _attributeValues,
      _ => _targetValues
    };
    var key = root + "#" + name;
    if (cache.TryGetRoot(key, out var cached)) {
      RegisterOwner(cached, cache);
      return cached;
    }
    object value = root switch {
      HixExpressionRoot.This => CurrentType,
      HixExpressionRoot.Target => _target,
      HixExpressionRoot.Attribute => _attribute,
      _ => null
    };
    IHixValue result;
    if (value is null) {
      result = thread.Error("@" + root.ToString().ToLowerInvariant() + " is not available in this context");
    } else {
      if (!string.IsNullOrEmpty(name)) value = SelectMember(value, name);
      result = value is null or TypedConstant {IsNull: true} ? NullHixValue.Instance : new RoslynHixValue(value, root);
    }
    cache.StoreRoot(key, result);
    RegisterOwner(result, cache);
    return result;
  }

  public object SelectMember(object subject, string name) {
    using var profile = HixProfiler.Measure("roslyn.select_member");
    switch (subject) {
      case AttributeData attribute: {
          foreach (var item in attribute.NamedArguments) {
            if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
              return item.Value;
          }
          if (attribute.AttributeConstructor is { } constructor) {
            for (var i = 0; i < constructor.Parameters.Length && i < attribute.ConstructorArguments.Length; i++) {
              if (string.Equals(constructor.Parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return attribute.ConstructorArguments[i];
            }
          }
          if (TryDefaultAttributeMember(attribute.AttributeClass, name, out var defaultValue)) return defaultValue;
          return null;
      }
      case IMethodSymbol method:
        if (int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var parameterIndex))
          return parameterIndex >= 0 && parameterIndex < method.Parameters.Length
            ? method.Parameters[parameterIndex]
            : null;
        return method.Parameters.FirstOrDefault(item => string.Equals(
            item.Name, name, StringComparison.OrdinalIgnoreCase
          )
        );
    }
    var type = TypeOf(subject);
    if (type is INamedTypeSymbol generic) {
      if (int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var argumentIndex) &&
        argumentIndex >= 0 && argumentIndex < generic.TypeArguments.Length)
        return generic.TypeArguments[argumentIndex];
      for (var i = 0; i < generic.TypeParameters.Length; i++) {
        if (string.Equals(generic.TypeParameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
          return generic.TypeArguments[i];
      }
    }
    for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType) {
      var result = current.GetMembers().FirstOrDefault(item =>
        string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)
      );
      if (result is not null) return result;
    }
    return null;
  }

  private bool TryDefaultAttributeMember(INamedTypeSymbol attributeType, string name, out object value) {
    using var profile = HixProfiler.Measure("roslyn.attribute_default");
    value = null;
    for (var current = attributeType; current is not null; current = current.BaseType) {
      var member = current.GetMembers().FirstOrDefault(item =>
        string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)
      );
      if (member is IFieldSymbol { HasConstantValue: true } field) {
        value = field.ConstantValue;
        return true;
      }
      var initializer = member?.DeclaringSyntaxReferences.Select(item => item.GetSyntax()).Select(item => item switch {
        VariableDeclaratorSyntax variable => variable.Initializer?.Value,
        PropertyDeclarationSyntax property => property.Initializer?.Value,
        _ => null
      }
      ).FirstOrDefault(item => item is not null);
      if (initializer is null) continue;
      if (_compilation is not null && _compilation.SyntaxTrees.Contains(initializer.SyntaxTree)) {
        var constant = _compilation.GetSemanticModel(initializer.SyntaxTree).GetConstantValue(initializer);
        if (constant.HasValue) {
          value = constant.Value;
          return true;
        }
      }
      // CompilationReference symbols retain source syntax from their owning compilation, but that
      // tree cannot be passed to this compilation's GetSemanticModel. Literal defaults do not need
      // semantic binding and cover attribute property defaults exposed from such references.
      if (TryReadLiteral(initializer, out value)) return true;
    }
    return false;
  }

  private static bool TryReadLiteral(ExpressionSyntax expression, out object value) {
    while (expression is ParenthesizedExpressionSyntax parenthesized)
      expression = parenthesized.Expression;
    if (expression is LiteralExpressionSyntax literal) {
      value = literal.Token.Value;
      return true;
    }
    if (expression is PrefixUnaryExpressionSyntax unary &&
      unary.Operand is LiteralExpressionSyntax operand && operand.Token.Value is { } number) {
      try {
        value = unary.Kind() switch {
          SyntaxKind.UnaryMinusExpression => number switch {
            int item => -item,
            long item => -item,
            float item => -item,
            double item => -item,
            decimal item => -item,
            _ => null
          },
          SyntaxKind.UnaryPlusExpression => number,
          _ => null
        };
        return value is not null;
      } catch (OverflowException) {
        // Let a missing default fall back to the derivation's seeded value.
      }
    }
    value = null;
    return false;
  }

  public ITypeSymbol ResolveType(string name) {
    using var profile = HixProfiler.Measure("roslyn.resolve_type");
    var normalized = (name ?? "").Replace("global::", "");
    var direct = _compilation?.GetTypeByMetadataName(normalized);
    if (direct is not null) return direct;
    var simple = normalized.Split('.', '+').LastOrDefault() ?? normalized;
    return _compilation?.GetSymbolsWithName(simple, SymbolFilter.Type).OfType<INamedTypeSymbol>()
      .FirstOrDefault(item => item.ToDisplayString() == normalized);
  }

  public static IEnumerable<IMethodSymbol> MethodsInHierarchy(INamedTypeSymbol type, string name) {
    for (var current = type; current is not null; current = current.BaseType) {
      foreach (var method in current.GetMembers(name ?? "").OfType<IMethodSymbol>())
        yield return method;
    }
  }

  public static string CallableReference(IMethodSymbol method) {
    return method.ContainingType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." + method.Name;
  }

  public static bool SameSignature(IMethodSymbol first, IMethodSymbol second) {
    if (first is null || second is null || first.Parameters.Length != second.Parameters.Length ||
      first.RefKind != second.RefKind) return false;
    for (var i = 0; i < first.Parameters.Length; i++) {
      if (
        first.Parameters[i].RefKind != second.Parameters[i].RefKind ||
        !SymbolEqualityComparer.Default.Equals(first.Parameters[i].Type, second.Parameters[i].Type)) return false;
    }
    return true;
  }

  public static bool TryWireParameters(IMethodSymbol from, IMethodSymbol to, out string arguments) {
    arguments = null;
    if (from is null || to is null || to.Parameters.Length > from.Parameters.Length) return false;
    var result = new string[to.Parameters.Length];
    for (var i = 0; i < to.Parameters.Length; i++) {
      if (!SymbolEqualityComparer.Default.Equals(from.Parameters[i].Type, to.Parameters[i].Type) ||
        from.Parameters[i].RefKind != to.Parameters[i].RefKind) return false;
      result[i] = from.Parameters[i].RefKind switch {
        RefKind.Ref => "ref ",
        RefKind.Out => "out ",
        RefKind.In => "in ",
        _ => ""
      } + GeneratorAnalysis.EscapeIdentifier(from.Parameters[i].Name);
    }
    arguments = string.Join(", ", result);
    return true;
  }

  public static ITypeSymbol TypeOf(object value) {
    return value switch {
      ITypeSymbol type => type,
      IMethodSymbol method => method.ReturnType,
      IPropertySymbol property => property.Type,
      IFieldSymbol field => field.Type,
      IParameterSymbol parameter => parameter.Type,
      IEventSymbol @event => @event.Type,
      AttributeData attribute => attribute.AttributeClass,
      TypedConstant { Kind: TypedConstantKind.Type, Value: ITypeSymbol representedType } => representedType,
      TypedConstant constant => constant.Type,
      _ => null
    };
  }

  public static string ComparableText(object value) {
    using var profile = HixProfiler.Measure("roslyn.comparable_text");
    if (value is TypedConstant constant) value = constant.Value;
    return value switch {
      null => "null",
      bool boolean => boolean ? "true" : "false",
      ITypeSymbol type => type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat),
      ISymbol symbol => symbol.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat),
      AttributeData attribute => attribute.AttributeClass?.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat),
      _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };
  }

  private sealed class RoslynValueOwnerComparer : IEqualityComparer<object> {
    public static readonly RoslynValueOwnerComparer Instance = new();

    public new bool Equals(object x, object y) {
      return ReferenceEquals(x, y);
    }

    public int GetHashCode(object value) {
      return RuntimeHelpers.GetHashCode(value);
    }
  }
}
