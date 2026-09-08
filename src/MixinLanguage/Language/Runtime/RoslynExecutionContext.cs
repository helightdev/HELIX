using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mixins.Compiler;
using Mixins.Env;
using Mixins.Roslyn;

namespace Mixins.Runtime;

/// <summary>Roslyn host services for an already lowered mixin program.</summary>
internal sealed class RoslynMixinContext : ExecutionContext {
  private readonly AttributeData _attribute;
  private readonly RoslynValueCache _attributeValues;
  private readonly MixinValueDictionary _committedTargetVariables;
  private readonly CSharpCompilation _compilation;
  private readonly ISymbol _target;
  private readonly Dictionary<string, string> _targetDefinitions;
  private readonly RoslynValueCache _targetValues;
  private readonly RoslynValueCache _thisValues;
  private readonly Dictionary<object, RoslynValueCache> _valueOwners =
    new(RoslynValueOwnerComparer.Instance);

  internal RoslynMixinContext(
    INamedTypeSymbol thisType, ISymbol target, AttributeData attribute,
    CSharpCompilation compilation,
    INamedTypeSymbol implicitAttributeType = null,
    IReadOnlyDictionary<string, object> implicitValues = null,
    IReadOnlyDictionary<string, string> targetDefinitions = null,
    MixinExpressionPreparedState preparedExpressions = null, MixinLibraryCatalog libraries = null,
    RoslynHostExpressionCache hostValues = null, MixinCompilation mixinCompilation = null
  )
    : base(preparedExpressions?.StringPool ?? new MixinStringPoolBuilder().Freeze()) {
    CurrentType = thisType;
    _target = target;
    _attribute = attribute;
    _compilation = compilation;
    _targetDefinitions = (targetDefinitions ?? new Dictionary<string, string>()).ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    hostValues ??= new RoslynHostExpressionCache();
    _thisValues = hostValues.ForThis(thisType);
    _targetValues = hostValues.ForTarget(target);
    _attributeValues = hostValues.ForAttribute(attribute);
    _committedTargetVariables = hostValues.TargetVariables(target);
    foreach (var item in _committedTargetVariables) {
      TargetVariables.StoreIsolated(
        ResolveString(item.Key.DynamicValue ?? ""), AttachTargetValue(item.Value)
      );
    }
  }

  internal INamedTypeSymbol CurrentType { get; }

  internal void CommitTargetVariables() {
    _committedTargetVariables.ReplaceWith(
      TargetVariables.Select(item =>
        new KeyValuePair<MixinString, IMixinValue>(
          MixinString.Dynamic(item.Key.Resolve(Strings)), DetachValue(item.Value)
        )
      )
    );
  }

  private IMixinValue AttachTargetValue(IMixinValue value) {
    return value switch {
      TupleMixinValue tuple => new TupleMixinValue(tuple.Values.Select(AttachTargetValue).ToArray()),
      LiteralMixinValue literal => new LiteralMixinValue(
        ResolveString(literal.Value.DynamicValue ?? literal.Value.Resolve(Strings))
      ),
      MixinTableValue table => new MixinTableValue(
        table.Entries.Select(item =>
          new KeyValuePair<MixinString, IMixinValue>(
            ResolveString(item.Key.DynamicValue ?? item.Key.Resolve(Strings)), AttachTargetValue(item.Value)
          )
        ).ToArray()
      ),
      _ => value
    };
  }

  internal bool TryGetDerived(RoslynMixinValue source, string key, out IMixinValue value) {
    if (!_valueOwners.TryGetValue(source.Value, out var cache)) cache = _targetValues;
    if (!cache.TryGetDerived(source.Value, key, out value)) return false;
    RegisterOwner(value, cache);
    return true;
  }

  internal void StoreDerived(RoslynMixinValue source, string key, IMixinValue value) {
    if (!_valueOwners.TryGetValue(source.Value, out var cache)) cache = _targetValues;
    cache.StoreDerived(source.Value, key, value);
    RegisterOwner(value, cache);
  }

  internal IMixinValue SelectValue(RoslynMixinValue source, MixinString member) {
    var name = member.Resolve(Strings);
    var key = "#" + name;
    if (TryGetDerived(source, key, out var cached)) return cached;
    var selected = SelectMember(source.Value, name);
    IMixinValue result = selected is null or TypedConstant {IsNull: true} ? NullMixinValue.Instance : new RoslynMixinValue(selected);
    StoreDerived(source, key, result);
    return result;
  }

  private void RegisterOwner(IMixinValue value, RoslynValueCache cache) {
    switch (value) {
      case TupleMixinValue tuple:
        foreach (var item in tuple.Values) RegisterOwner(item, cache);
        break;
      case RoslynMixinValue roslyn:
        _valueOwners[roslyn.Value] = cache;
        break;
      case MixinTableValue table:
        foreach (var item in table.Entries) RegisterOwner(item.Value, cache);
        break;
    }
  }

  public override IMixinValue DefineTarget(string name, string descriptor) {
    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(descriptor)) return Error("target alias and descriptor cannot be empty");
    _targetDefinitions["$" + name.TrimStart('$')] = descriptor;
    return NullMixinValue.Instance;
  }

  public override string ResolveInjectionTarget(string target) {
    var descriptor = ParseMixinTarget(target, _targetDefinitions);
    return (descriptor.IsPublic ? "^" : "") + (descriptor.IsStatic ? "*" : "") + descriptor.Name +
      (string.IsNullOrEmpty(descriptor.DelegateType) ? "" : ":" + descriptor.DelegateType);
  }

  public override IMixinValue Configure(string name, IMixinValue value) => name.ToUpperInvariant() is "DEBUG" or "PROFILE"
    ? NullMixinValue.Instance : Error("unknown host configuration '" + name + "'");

  public override IMixinValue ResolveMixin(MixinString localName, IMixinValue operand) {
    var descriptor = ParseMixinTarget(operand.Render(this).Resolve(Strings), _targetDefinitions);
    string callable = null;
    if (!string.IsNullOrEmpty(descriptor.DelegateType)) {
      if (ResolveType(descriptor.DelegateType) is { TypeKind: TypeKind.Delegate } delegateType)
        callable = delegateType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat);
      else callable = descriptor.DelegateType;
    } else if (ResolveType(descriptor.Name) is { TypeKind: TypeKind.Delegate } namedDelegate)
      callable = namedDelegate.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat);
    else {
      var methods = MethodsInHierarchy(CurrentType, descriptor.Name).ToArray();
      if (methods.Length == 1) callable = CallableReference(methods[0]);
    }
    IMixinValue result = callable is null ? NullMixinValue.Instance : new LiteralMixinValue(ResolveString(callable));
    return result;
  }

  internal IMethodSymbol Callable(IMixinValue value) {
    value = Evaluate(value);
    if (value is RoslynMixinValue { Value: IMethodSymbol method }) return method;
    var text = value.Render(this).Resolve(Strings);
    if (ResolveType(text) is INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke })
      return invoke;
    var methodName = text.Substring(text.LastIndexOf('.') + 1);
    var methods = MethodsInHierarchy(CurrentType, methodName).ToArray();
    return methods.Length == 1 ? methods[0] : null;
  }

  public override MixinString NameOf(IMixinValue value) {
    return value is RoslynMixinValue roslyn
      ? ResolveString(
        (roslyn.Value as ISymbol)?.Name ??
        (roslyn.Value as AttributeData)?.AttributeClass?.Name ?? ComparableText(roslyn.Value)
      )
      : value is DetachedSemanticMixinValue detached
        ? detached.TypeName
        : base.NameOf(value);
  }

  public override bool IsType(IMixinValue value, MixinString requested) {
    using var profile = MixinProfiler.Measure("roslyn.is_type");
    if (value is DetachedSemanticMixinValue detached) {
      var expected = requested.Resolve(Strings).Replace("global::", "");
      var candidate = detached.Render(this).Resolve(Strings);
      return candidate == expected || detached.TypeName.Resolve(Strings) == expected;
    }
    if (value is not RoslynMixinValue roslyn || TypeOf(roslyn.Value) is not INamedTypeSymbol type) return false;
    var name = requested.Resolve(Strings).Replace("global::", "");

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

  public override bool HasTrait(IMixinValue value, MixinString requested) {
    using var profile = MixinProfiler.Measure("roslyn.has_trait");
    var name = requested.Resolve(Strings);
    if (value is DetachedSemanticMixinValue) return false;
    return value is RoslynMixinValue roslyn && SemanticTraits(roslyn.Value).Contains(name, StringComparer.Ordinal);
  }

  internal override object UnlinkSnapshot(IMixinValue value) {
    if (value is not RoslynMixinValue roslyn) return value.Unlink(this);
    if (!_valueOwners.TryGetValue(roslyn.Value, out var cache)) cache = _targetValues;
    return cache.Snapshot(roslyn, () => roslyn.Unlink(this));
  }

  internal override IMixinValue DetachValue(IMixinValue value) {
    value = Evaluate(value);
    if (value is RoslynMixinValue roslyn) {
      var detached = UnlinkSnapshot(roslyn);
      return detached is DetachedSemanticData semantic
        ? DetachedSemanticMixinValue.Materialize(semantic)
        : base.DetachValue(
          detached switch {
            IMixinValue typed => typed,
            null => NullMixinValue.Instance,
            bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
            string text => new LiteralMixinValue(MixinString.Dynamic(text)),
            _ => new ObjectMixinValue(detached)
          }
        );
    }
    return base.DetachValue(value);
  }

  internal static IReadOnlyList<string> SemanticTraits(object value) {
    using var profile = MixinProfiler.Measure("roslyn.semantic_traits");
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

  public override IMixinValue Unwrap(IMixinValue value) {
    using var profile = MixinProfiler.Measure("roslyn.unwrap");
    switch (value) {
      // Defaults recovered from a referenced attribute declaration are CLR constants rather
      // than TypedConstant instances. Normalize both representations to the same language
      // values so execution does not depend on whether Roslyn supplied source or metadata.
      case RoslynMixinValue { Value: null }:
        return NullMixinValue.Instance;
      case RoslynMixinValue { Value: bool boolean }:
        return boolean ? BooleanMixinValue.True : BooleanMixinValue.False;
      case RoslynMixinValue { Value: string text }:
        return new LiteralMixinValue(ResolveString(text));
      case RoslynMixinValue { Value: char character }:
        return new LiteralMixinValue(ResolveString(character.ToString()));
      case RoslynMixinValue { Value: ITypeSymbol type }:
        return new LiteralMixinValue(ResolveString(type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal)));
      case RoslynMixinValue { Value: byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal } scalar:
        return new NumberMixinValue(Convert.ToDouble(scalar.Value, CultureInfo.InvariantCulture));
      case RoslynMixinValue { Value: TypedConstant constant }
        when constant.IsNull || constant.Kind == TypedConstantKind.Error:
        return NullMixinValue.Instance;
      case RoslynMixinValue { Value: TypedConstant constant }:
        if (constant.Kind == TypedConstantKind.Array)
          return new TupleMixinValue(constant.Values.Select(item => Unwrap(new RoslynMixinValue(item))).ToArray());
        return constant.Value switch {
          bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
          string text => new LiteralMixinValue(ResolveString(text)),
          char character => new LiteralMixinValue(ResolveString(character.ToString())),
          ITypeSymbol type => new LiteralMixinValue(ResolveString(type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal))),
          byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal =>
            new NumberMixinValue(Convert.ToDouble(constant.Value, CultureInfo.InvariantCulture)),
          _ => Error("unsupported host constant")
        };
      case DetachedSemanticMixinValue detached: return new LiteralMixinValue(detached.Render(this));
      default: return base.Unwrap(value);
    }
  }

  public override IMixinValue Attributes(IMixinValue value, MixinString requested, bool exact, bool first) {
    using var profile = MixinProfiler.Measure("roslyn.attributes");
    if (value is not RoslynMixinValue roslyn) return first ? NullMixinValue.Instance : TupleMixinValue.Empty;
    var expected = requested.IsInterned || requested.DynamicValue is not null
      ? requested.Resolve(Strings)
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
      IMixinValue result = matches.Length == 0 ? NullMixinValue.Instance : new RoslynMixinValue(matches[0]);
      RegisterOwner(result, owner);
      return result;
    }
    var entries = matches.Select((item, index) => {
      IMixinValue result = new RoslynMixinValue(item);
      RegisterOwner(result, owner);
      return result;
    }
    ).ToArray();
    return new TupleMixinValue(entries);
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
    using var profile = MixinProfiler.Measure("roslyn.type_matches");
    return type is not null && (
      type.Name == expected || type.ToDisplayString() == expected.Replace("global::", "") ||
      type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal) ==
      expected.Replace("global::", "")
    );
  }

  protected override IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member) {
    using var profile = MixinProfiler.Measure("roslyn.resolve_host");
    var memberName = member.Resolve(Strings);
    var name = memberName;
    var cache = root switch {
      MixinExpressionRoot.This => _thisValues,
      MixinExpressionRoot.Attribute => _attributeValues,
      _ => _targetValues
    };
    var key = root + "#" + name;
    if (cache.TryGetRoot(key, out var cached)) {
      RegisterOwner(cached, cache);
      return cached;
    }
    object value = root switch {
      MixinExpressionRoot.This => CurrentType,
      MixinExpressionRoot.Target => _target,
      MixinExpressionRoot.Attribute => _attribute,
      _ => null
    };
    IMixinValue result;
    if (value is null) {
      result = Error("@" + root.ToString().ToLowerInvariant() + " is not available in this context");
    } else {
      if (!string.IsNullOrEmpty(name)) value = SelectMember(value, name);
      result = value is null or TypedConstant {IsNull: true} ? NullMixinValue.Instance : new RoslynMixinValue(value, root);
    }
    cache.StoreRoot(key, result);
    RegisterOwner(result, cache);
    return result;
  }

  internal object SelectMember(object subject, string name) {
    using var profile = MixinProfiler.Measure("roslyn.select_member");
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
    using var profile = MixinProfiler.Measure("roslyn.attribute_default");
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

  internal ITypeSymbol ResolveType(string name) {
    using var profile = MixinProfiler.Measure("roslyn.resolve_type");
    var normalized = (name ?? "").Replace("global::", "");
    var direct = _compilation?.GetTypeByMetadataName(normalized);
    if (direct is not null) return direct;
    var simple = normalized.Split('.', '+').LastOrDefault() ?? normalized;
    return _compilation?.GetSymbolsWithName(simple, SymbolFilter.Type).OfType<INamedTypeSymbol>()
      .FirstOrDefault(item => item.ToDisplayString() == normalized);
  }

  private static IEnumerable<IMethodSymbol> MethodsInHierarchy(INamedTypeSymbol type, string name) {
    for (var current = type; current is not null; current = current.BaseType) {
      foreach (var method in current.GetMembers(name ?? "").OfType<IMethodSymbol>())
        yield return method;
    }
  }

  private static string CallableReference(IMethodSymbol method) {
    return method.ContainingType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." + method.Name;
  }

  internal static bool SameSignature(IMethodSymbol first, IMethodSymbol second) {
    if (first is null || second is null || first.Parameters.Length != second.Parameters.Length ||
      first.RefKind != second.RefKind) return false;
    for (var i = 0; i < first.Parameters.Length; i++) {
      if (
        first.Parameters[i].RefKind != second.Parameters[i].RefKind ||
        !SymbolEqualityComparer.Default.Equals(first.Parameters[i].Type, second.Parameters[i].Type)) return false;
    }
    return true;
  }

  internal static bool TryWireParameters(IMethodSymbol from, IMethodSymbol to, out string arguments) {
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

  internal static ITypeSymbol TypeOf(object value) {
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

  internal static string ComparableText(object value) {
    using var profile = MixinProfiler.Measure("roslyn.comparable_text");
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

  internal static MixinTargetDescriptor ParseMixinTarget(
    string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    var value = target?.Trim() ?? "";
    if (targetDefinitions is not null && targetDefinitions.TryGetValue(value, out var defined))
      return ParseMixinTarget(defined, targetDefinitions);
    var isStatic = false;
    var isPublic = false;
    while (value.Length != 0 && (value[0] == '*' || value[0] == '^')) {
      if (value[0] == '*') isStatic = true;
      else isPublic = true;
      value = value.Substring(1);
    }
    string name;
    string delegateType = null;
    if (value.StartsWith("~", StringComparison.Ordinal)) {
      delegateType = value.Substring(1);
      var normalized = delegateType.Replace("global::", "");
      name = normalized.Split('.', '+').LastOrDefault() ?? normalized;
    } else {
      var separator = value.IndexOf(':');
      name = separator < 0 ? value : value.Substring(0, separator);
      if (separator >= 0) delegateType = value.Substring(separator + 1);
    }
    name = name switch {
      "$Init" => "Awake",
      "$Dispose" => "OnDestroy",
      _ when name.StartsWith("$", StringComparison.Ordinal) => name.Substring(1),
      _ => name
    };
    return new MixinTargetDescriptor(name, isStatic, isPublic, delegateType);
  }

  private sealed class RoslynValueOwnerComparer : IEqualityComparer<object> {
    internal static readonly RoslynValueOwnerComparer Instance = new();

    public new bool Equals(object x, object y) {
      return ReferenceEquals(x, y);
    }

    public int GetHashCode(object value) {
      return RuntimeHelpers.GetHashCode(value);
    }
  }
}
