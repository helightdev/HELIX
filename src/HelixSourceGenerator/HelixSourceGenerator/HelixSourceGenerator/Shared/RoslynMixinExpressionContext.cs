using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;

namespace HELIX.SourceGen;

internal sealed class ImplicitMixinValue {
  internal ImplicitMixinValue(ITypeSymbol type, object value) {
    Type = type;
    Value = value;
  }

  internal ITypeSymbol Type { get; }
  internal object Value { get; }
}

internal sealed class RoslynMixinExpressionContext :
  IMixinExpressionContext, IMixinExpressionSignatureContext {
  private static readonly SymbolDisplayFormat FullNameDisplayFormat =
    SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(
      SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

  private readonly INamedTypeSymbol _thisType;
  private readonly ISymbol _target;
  private readonly AttributeData _attribute;
  private readonly INamedTypeSymbol _implicitAttributeType;
  private readonly IReadOnlyDictionary<string, ImplicitMixinValue> _implicitValues;
  private readonly IReadOnlyList<IParameterSymbol> _arguments;
  private readonly CSharpCompilation _compilation;
  private readonly IReadOnlyDictionary<string, string> _targetDefinitions;

  internal RoslynMixinExpressionContext(
    INamedTypeSymbol thisType,
    ISymbol target,
    AttributeData attribute,
    IReadOnlyList<IParameterSymbol> arguments,
    CSharpCompilation compilation,
    INamedTypeSymbol implicitAttributeType = null,
    IReadOnlyDictionary<string, ImplicitMixinValue> implicitValues = null,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    _thisType = thisType;
    _target = target;
    _attribute = attribute;
    _arguments = arguments ?? Array.Empty<IParameterSymbol>();
    _compilation = compilation;
    _implicitAttributeType = implicitAttributeType;
    _implicitValues = implicitValues;
    _targetDefinitions = targetDefinitions;
  }

  public bool TryResolve(
    MixinExpressionReference reference,
    out string value,
    out string error
  ) {
    if (!TrySubject(reference, out var subject, out error) ||
      !ApplyValueProperties(reference, ref subject, out error)) {
      value = null;
      return false;
    }
    if (reference.Properties.Any(IsPredicate)) {
      value = null;
      error = "boolean pseudo-properties cannot be interpolated as strings";
      return false;
    }
    var renderRoot = reference.Properties.Any(item => !IsPredicate(item) && item.Name == "type")
      ? null
      : reference.Root;
    return TryRender(subject, renderRoot, out value, out error);
  }

  public bool TryEvaluate(
    MixinExpressionReference reference,
    out bool value,
    out string error
  ) {
    value = false;
    var predicates = reference.Properties.Where(IsPredicate).ToArray();
    if (!TrySubject(reference, out var subject, out error) ||
      !ApplyValueProperties(reference, ref subject, out error)) {
      if (IsDeveloperExpressionError(error)) return false;
      error = null;
      if (predicates.Length == 0) {
        value = false;
        return true;
      }
      value = true;
      foreach (var predicate in predicates) {
        var item = predicate.Name == "eq" && EqualTo(null, predicate.Argument);
        value &= predicate.Negated ? !item : item;
      }
      return true;
    }
    if (predicates.Length == 0) {
      value = IsTruthy(subject);
      return true;
    }
    value = true;
    foreach (var predicate in predicates) {
      if (!TryPredicate(subject, predicate, out var item, out error)) return false;
      value &= predicate.Negated ? !item : item;
    }
    return true;
  }

  public bool TryResolveMixin(string target, out string callable, out string error) {
    error = null;
    callable = null;
    var raw = target?.Trim();
    while (!string.IsNullOrEmpty(raw) && raw[0] is '*' or '^') raw = raw.Substring(1);
    if (!string.IsNullOrEmpty(raw) && raw[0] == '~') {
      var delegateType = ResolveType(raw.Substring(1));
      if (delegateType is { TypeKind: TypeKind.Delegate })
        callable = delegateType.ToDisplayString(TypeDisplayFormat);
      return true;
    }
    var name = NormalizeMixinTarget(target);
    if (string.IsNullOrEmpty(name)) return true;
    var resolvedDelegate = ResolveType(name);
    if (resolvedDelegate is { TypeKind: TypeKind.Delegate }) {
      callable = resolvedDelegate.ToDisplayString(TypeDisplayFormat);
      return true;
    }
    var methods = MethodsInHierarchy(_thisType, name).ToArray();
    if (methods.Length == 1) callable = CallableReference(methods[0]);
    return true;
  }

  public bool TryWire(
    string from,
    string to,
    out string arguments,
    out string error
  ) {
    arguments = null;
    error = null;
    var fromMethod = ResolveCallable(from);
    var toMethod = ResolveCallable(to);
    if (fromMethod is null || toMethod is null) {
      error = "wire requires two resolvable methods or delegates";
      return false;
    }
    if (!TryWireParameters(fromMethod, toMethod, out arguments)) {
      error = "method '" + toMethod.Name + "' cannot receive the parameters of '" +
        fromMethod.Name + "'";
      return false;
    }
    return true;
  }

  public bool TryHaveSameSignature(
    string first, string second, out bool value, out string error
  ) {
    error = null;
    var firstMethod = ResolveCallable(first);
    var secondMethod = ResolveCallable(second);
    value = firstMethod is not null && secondMethod is not null &&
      HaveSameSignature(firstMethod, secondMethod);
    return true;
  }

  public bool TryWireable(
    string from, string to, out bool value, out string error
  ) {
    error = null;
    var fromMethod = ResolveCallable(from);
    var toMethod = ResolveCallable(to);
    value = fromMethod is not null && toMethod is not null &&
      TryWireParameters(fromMethod, toMethod, out _);
    return true;
  }

  private bool TrySubject(
    MixinExpressionReference reference,
    out object subject,
    out string error
  ) {
    subject = null;
    error = null;
    switch (reference.Root) {
      case "this": subject = _thisType; break;
      case "target": subject = _target; break;
      case "attr": subject = (object)_attribute ?? _implicitAttributeType; break;
      case "arg":
        subject = SelectArgument(reference.Member);
        if (subject is null) error = "unknown argument '" + (reference.Member ?? "") + "'";
        return subject is not null;
      default:
        error = "unknown expression root '@" + reference.Root + "'";
        return false;
    }
    if (subject is null) {
      error = "@" + reference.Root + " is not available in this context";
      return false;
    }
    if (!string.IsNullOrEmpty(reference.Member)) {
      subject = SelectMember(subject, reference.Member);
      if (subject is null) {
        error = "member '" + reference.Member + "' was not found on @" + reference.Root;
        return false;
      }
    }
    return true;
  }

  private IParameterSymbol SelectArgument(string member) {
    if (string.IsNullOrEmpty(member)) return null;
    if (int.TryParse(member, NumberStyles.None, CultureInfo.InvariantCulture, out var index))
      return index >= 0 && index < _arguments.Count ? _arguments[index] : null;
    return _arguments.FirstOrDefault(item => item.Name == member) ??
      _arguments.FirstOrDefault(item => string.Equals(item.Name, member, StringComparison.OrdinalIgnoreCase));
  }

  private object SelectMember(object subject, string name) {
    if (_implicitAttributeType is not null &&
      SymbolEqualityComparer.Default.Equals(subject as ISymbol, _implicitAttributeType)) {
      if (_implicitValues is not null && _implicitValues.TryGetValue(name, out var implicitValue)) return implicitValue;
      return null;
    }
    if (subject is AttributeData attribute) {
      foreach (var named in attribute.NamedArguments)
        if (string.Equals(named.Key, name, StringComparison.OrdinalIgnoreCase))
          return named.Value;
      if (attribute.AttributeConstructor is { } constructor) {
        for (var index = 0; index < constructor.Parameters.Length && index < attribute.ConstructorArguments.Length;
          index++)
          if (string.Equals(constructor.Parameters[index].Name, name, StringComparison.OrdinalIgnoreCase))
            return attribute.ConstructorArguments[index];
      }
      if (TryDefaultAttributeMember(attribute.AttributeClass, name, out var defaultValue)) return defaultValue;
      return null;
    }
    if (subject is IMethodSymbol method) {
      return method.Parameters.FirstOrDefault(item => item.Name == name) ??
        method.Parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    }
    var type = AsType(subject);
    if (type is null) return null;
    for (var current = type; current is not null; current = current.BaseType) {
      var member = current.GetMembers().FirstOrDefault(item => item.Name == name) ??
        current.GetMembers().FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
      if (member is not null) return member;
    }
    return null;
  }

  private bool TryDefaultAttributeMember(
    INamedTypeSymbol attributeType,
    string name,
    out ImplicitMixinValue value
  ) {
    value = null;
    if (attributeType is null || _compilation is null) return false;
    for (var current = attributeType; current is not null; current = current.BaseType) {
      var member = current.GetMembers().FirstOrDefault(item => item.Name == name) ??
        current.GetMembers().FirstOrDefault(item =>
          string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)
        );
      if (member is IFieldSymbol { HasConstantValue: true } constantField) {
        value = new ImplicitMixinValue(constantField.Type, constantField.ConstantValue);
        return true;
      }
      if (member is not (IFieldSymbol or IPropertySymbol)) continue;
      var initializer = member.DeclaringSyntaxReferences
        .Select(item => item.GetSyntax())
        .Select(item => item switch {
            VariableDeclaratorSyntax field => field.Initializer?.Value,
            PropertyDeclarationSyntax property => property.Initializer?.Value,
            _ => null
          }
        )
        .FirstOrDefault(item => item is not null);
      if (initializer is null) return false;
      var constant = _compilation.GetSemanticModel(initializer.SyntaxTree).GetConstantValue(initializer);
      if (!constant.HasValue) return false;
      value = new ImplicitMixinValue(
        member is IFieldSymbol fieldSymbol ? fieldSymbol.Type : ((IPropertySymbol)member).Type,
        constant.Value
      );
      return true;
    }
    return false;
  }

  private static bool ApplyValueProperties(
    MixinExpressionReference reference,
    ref object subject,
    out string error
  ) {
    error = null;
    foreach (var property in reference.Properties.Where(item => !IsPredicate(item))) {
      switch (property.Name) {
        case "name":
          subject = NameOf(subject);
          break;
        case "type":
          subject = AsType(subject);
          break;
        case "fullName":
          subject = FullNameOf(subject);
          break;
        case "unwrap":
          subject = Unwrap(subject);
          break;
        case "replace":
        case "replaceFirst":
          if (property.Arguments.Count != 2) {
            error = ":" + property.Name + " requires a regex and replacement";
            return false;
          }
          if (!TryComparableText(subject, out var replaced)) {
            error = "property ':" + property.Name + "' is not available for this value";
            return false;
          }
          try {
            subject = property.Name == "replace"
              ? Regex.Replace(replaced, property.Arguments[0], property.Arguments[1])
              : new Regex(property.Arguments[0]).Replace(replaced, property.Arguments[1], 1);
          } catch (ArgumentException exception) {
            error = "invalid regular expression: " + exception.Message;
            return false;
          }
          break;
        case "switch":
          if (property.Arguments.Count != 2) {
            error = ":switch requires truthy and falsy values";
            return false;
          }
          subject = IsTruthy(subject) ? property.Arguments[0] : property.Arguments[1];
          break;
        case "size":
          subject = TryComparableText(subject, out var sized)
            ? sized.Length.ToString(CultureInfo.InvariantCulture)
            : "0";
          break;
        case "path":
          subject = SelectTypeArgument(subject, property.Argument);
          break;
        default:
          error = "unknown value property ':" + property.Name + "'";
          return false;
      }
      if (subject is null) {
        error = "property ':" + property.Name + "' is not available for this value";
        return false;
      }
    }
    return true;
  }

  private static string FullNameOf(object subject) {
    return TypeValueOf(subject)
      ?.ToDisplayString(FullNameDisplayFormat);
  }

  private static object Unwrap(object subject) {
    switch (subject) {
      case TypedConstant { Kind: TypedConstantKind.Type, Value: ITypeSymbol type }:
        return UnqualifiedGlobalName(type);
      case TypedConstant { Value: string text }:
        return text;
      case ImplicitMixinValue { Value: ITypeSymbol type }:
        return UnqualifiedGlobalName(type);
      case ImplicitMixinValue { Value: string text }:
        return text;
      case ITypeSymbol type:
        return UnqualifiedGlobalName(type);
      case string text:
        return text;
      default:
        return subject;
    }
  }

  private static ITypeSymbol TypeValueOf(object subject) {
    return subject switch {
      TypedConstant { Kind: TypedConstantKind.Type, Value: ITypeSymbol type } => type,
      ImplicitMixinValue { Value: ITypeSymbol type } => type,
      _ => AsType(subject)
    };
  }

  private static string UnqualifiedGlobalName(ITypeSymbol type) {
    return type.ToDisplayString(TypeDisplayFormat).Replace("global::", "");
  }

  private static ITypeSymbol SelectTypeArgument(object subject, string path) {
    if (AsType(subject) is not INamedTypeSymbol type) return null;
    if (int.TryParse(path, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedIndex)) {
      return parsedIndex >= 0 && parsedIndex < type.TypeArguments.Length
        ? type.TypeArguments[parsedIndex]
        : null;
    }
    for (var parameterIndex = 0; parameterIndex < type.TypeParameters.Length; parameterIndex++) {
      if (string.Equals(
        type.TypeParameters[parameterIndex].Name, path, StringComparison.OrdinalIgnoreCase
      )) return type.TypeArguments[parameterIndex];
    }
    return null;
  }

  private static bool IsTruthy(object subject) {
    return subject switch {
      null => false,
      TypedConstant constant => constant.Kind != TypedConstantKind.Error &&
        !constant.IsNull && constant.Value is not false,
      ImplicitMixinValue value => value.Value is not null && value.Value is not false,
      bool value => value,
      _ => true
    };
  }

  private bool TryPredicate(
    object subject,
    MixinExpressionProperty property,
    out bool value,
    out string error
  ) {
    value = false;
    error = null;
    var symbol = subject as ISymbol;
    var type = AsType(subject);
    switch (property.Name) {
      case "exists":
        value = true;
        return true;
      case "is":
        if (string.IsNullOrWhiteSpace(property.Argument)) {
          error = ":?is requires a type";
          return false;
        }
        value = type is not null && IsOrInherits(type, property.Argument);
        return true;
      case "has":
        if (string.IsNullOrWhiteSpace(property.Argument)) {
          error = ":?has requires a member";
          return false;
        }
        value = type is not null && HasConcreteMember(type, property.Argument);
        return true;
      case "eq":
        value = EqualTo(subject, property.Argument);
        return true;
      case "matches":
        if (property.Arguments.Count != 1) {
          error = ":?matches requires a regex";
          return false;
        }
        if (!TryComparableText(subject, out var matchText)) {
          value = false;
          return true;
        }
        try {
          value = Regex.IsMatch(matchText, property.Argument);
          return true;
        } catch (ArgumentException exception) {
          error = "invalid regular expression: " + exception.Message;
          return false;
        }
      case "signature":
        var expected = ResolveCallable(property.Argument);
        var actual = ResolveCallable(subject);
        value = actual is not null && expected is not null && HaveSameSignature(actual, expected);
        return true;
      case "wireable":
        var from = ResolveCallable(property.Arguments[0]);
        var to = ResolveCallable(property.Arguments[1]);
        value = from is not null && to is not null && TryWireParameters(from, to, out _);
        return true;
      case "isSelf":
        value = type is not null && SymbolEqualityComparer.Default.Equals(type, _thisType);
        return true;
      case "ref":
        value = symbol is IParameterSymbol { RefKind: RefKind.Ref };
        return true;
      case "in":
        value = symbol is IParameterSymbol { RefKind: RefKind.In };
        return true;
      case "out":
        value = symbol is IParameterSymbol { RefKind: RefKind.Out };
        return true;
      case "inout":
        value = symbol is IParameterSymbol { RefKind: RefKind.In or RefKind.Out };
        return true;
      case "argument":
        value = symbol is IParameterSymbol { RefKind: RefKind.None };
        return true;
      case "static":
        value = symbol?.IsStatic == true;
        return true;
      case "public":
        value = symbol?.DeclaredAccessibility == Accessibility.Public;
        return true;
      case "exposed":
        value = symbol?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal
          or Accessibility.ProtectedOrInternal;
        return true;
      case "top":
        value = type?.ContainingType is null;
        return true;
      case "concrete":
        value = subject switch {
          INamedTypeSymbol named => named.TypeKind != TypeKind.Interface && !named.IsAbstract,
          IMethodSymbol method => !method.IsAbstract && !method.IsVirtual,
          _ => symbol is not null
        };
        return true;
      case "partial":
        value = IsPartialSymbol(subject);
        return true;
      case "generic":
        value = subject is IMethodSymbol genericMethod
          ? genericMethod.TypeParameters.Length != 0
          : type is INamedTypeSymbol genericType && genericType.TypeParameters.Length != 0;
        return true;
      case "struct":
        value = type?.TypeKind == TypeKind.Struct;
        return true;
      case "class":
        value = type?.IsReferenceType == true;
        return true;
      default:
        error = "unknown boolean pseudo-property ':?" + property.Name + "'";
        return false;
    }
  }

  private static bool IsDeveloperExpressionError(string error) {
    return error?.StartsWith("unknown expression root", StringComparison.Ordinal) == true ||
      error?.StartsWith("unknown value property", StringComparison.Ordinal) == true ||
      error?.StartsWith("unknown boolean pseudo-property", StringComparison.Ordinal) == true ||
      error?.Contains(" requires a ") == true;
  }

  private IMethodSymbol ResolveCallable(object value) {
    if (value is IMethodSymbol method) return method;
    if (value is INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke })
      return invoke;
    if (value is not string text || string.IsNullOrWhiteSpace(text) || text == "null") return null;
    text = text.Trim();
    if (text.StartsWith("typeof(", StringComparison.Ordinal) && text.EndsWith(")", StringComparison.Ordinal))
      text = text.Substring(7, text.Length - 8);
    if (text.StartsWith("this.", StringComparison.Ordinal)) text = text.Substring(5);
    var type = ResolveType(text);
    if (type is { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } delegateInvoke })
      return delegateInvoke;
    var separator = text.LastIndexOf('.');
    var name = separator < 0 ? text : text.Substring(separator + 1);
    var methods = MethodsInHierarchy(_thisType, name).ToArray();
    return methods.Length == 1 ? methods[0] : null;
  }

  private INamedTypeSymbol ResolveType(string name) {
    var normalized = name.StartsWith("global::", StringComparison.Ordinal)
      ? name.Substring(8)
      : name;
    var direct = _compilation?.GetTypeByMetadataName(normalized);
    if (direct is not null) return direct;
    var separator = Math.Max(normalized.LastIndexOf('.'), normalized.LastIndexOf('+'));
    var simple = separator < 0 ? normalized : normalized.Substring(separator + 1);
    return _compilation?.GetSymbolsWithName(simple, SymbolFilter.Type)
      .OfType<INamedTypeSymbol>()
      .FirstOrDefault(item => item.ToDisplayString() == normalized);
  }

  private static IEnumerable<IMethodSymbol> MethodsInHierarchy(INamedTypeSymbol type, string name) {
    for (var current = type; current is not null; current = current.BaseType)
      foreach (var method in current.GetMembers(name).OfType<IMethodSymbol>())
        yield return method;
  }

  private string NormalizeMixinTarget(string target) {
    if (string.IsNullOrWhiteSpace(target)) return null;
    var value = target.Trim();
    while (value.Length != 0 && value[0] is '*' or '^') value = value.Substring(1);
    if (value.Length == 0) return null;
    if (_targetDefinitions is not null && _targetDefinitions.TryGetValue(value, out var defined))
      return NormalizeMixinTarget(defined);
    if (value[0] == '~') return value.Substring(1);
    return value switch { "$Init" => "Awake", "$Dispose" => "OnDestroy", _ => value };
  }

  private static string CallableReference(IMethodSymbol method) {
    return method.ContainingType.ToDisplayString(TypeDisplayFormat) + "." + method.Name;
  }

  private static bool HaveSameSignature(IMethodSymbol first, IMethodSymbol second) {
    if (first.RefKind != second.RefKind || first.Parameters.Length != second.Parameters.Length ||
      first.TypeParameters.Length != second.TypeParameters.Length ||
      !SymbolEqualityComparer.Default.Equals(first.ReturnType, second.ReturnType)) return false;
    for (var index = 0; index < first.Parameters.Length; index++) {
      if (first.Parameters[index].RefKind != second.Parameters[index].RefKind ||
        !SymbolEqualityComparer.Default.Equals(
          first.Parameters[index].Type, second.Parameters[index].Type
        )) return false;
    }
    return true;
  }

  private bool TryWireParameters(
    IMethodSymbol from,
    IMethodSymbol to,
    out string arguments
  ) {
    arguments = null;
    if (to.Parameters.Length > from.Parameters.Length) return false;
    var result = new string[to.Parameters.Length];
    for (var index = 0; index < to.Parameters.Length; index++) {
      var source = from.Parameters[index];
      var destination = to.Parameters[index];
      if (source.RefKind != destination.RefKind) return false;
      if (source.RefKind == RefKind.None) {
        if (_compilation?.ClassifyConversion(source.Type, destination.Type).IsImplicit != true)
          return false;
      } else if (!SymbolEqualityComparer.Default.Equals(source.Type, destination.Type)) return false;
      var prefix = source.RefKind switch {
        RefKind.Ref => "ref ", RefKind.In => "in ", RefKind.Out => "out ", _ => ""
      };
      result[index] = prefix + EscapeIdentifier(source.Name);
    }
    arguments = string.Join(", ", result);
    return true;
  }

  private static bool EqualTo(object subject, string expected) {
    if (IsNullLike(subject)) return string.Equals(expected, "null", StringComparison.OrdinalIgnoreCase);
    if (TryComparableText(subject, out var rendered) &&
      string.Equals(rendered, expected ?? "", StringComparison.Ordinal)) return true;
    var unwrapped = Unwrap(subject);
    return TryComparableText(unwrapped, out rendered) && string.Equals(
      UnwrapComparable(rendered), UnwrapComparable(expected ?? ""), StringComparison.OrdinalIgnoreCase
    );
  }

  private static bool IsNullLike(object subject) {
    return subject switch {
      null => true,
      TypedConstant constant => constant.IsNull || constant.Kind == TypedConstantKind.Error,
      ImplicitMixinValue value => value.Value is null,
      _ => false
    };
  }

  private static bool TryComparableText(object subject, out string value) {
    subject = subject switch {
      TypedConstant constant => constant.Value,
      ImplicitMixinValue implicitValue => implicitValue.Value,
      _ => subject
    };
    if (subject is ITypeSymbol type) {
      value = type.ToDisplayString(TypeDisplayFormat);
      return true;
    }
    if (subject is AttributeData attribute) {
      value = attribute.AttributeClass?.ToDisplayString(TypeDisplayFormat);
      return value is not null;
    }
    if (subject is ISymbol symbol) {
      value = symbol.ToDisplayString(TypeDisplayFormat);
      return true;
    }
    value = Convert.ToString(subject, CultureInfo.InvariantCulture);
    return value is not null;
  }

  private static string UnwrapComparable(string value) {
    if (value.StartsWith("global::", StringComparison.Ordinal)) value = value.Substring(8);
    if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
      value = value.Substring(1, value.Length - 2);
    return value;
  }

  private bool IsOrInherits(ITypeSymbol type, string requested) {
    bool Matches(ITypeSymbol candidate) {
      var display = candidate.ToDisplayString(TypeDisplayFormat);
      return candidate.Name == requested || display == requested || display == "global::" + requested ||
        candidate.ToDisplayString() == requested;
    }

    if (Matches(type)) return true;
    if (type is not INamedTypeSymbol named) return false;
    for (var current = named.BaseType; current is not null; current = current.BaseType)
      if (Matches(current))
        return true;
    return named.AllInterfaces.Any(Matches);
  }

  private static bool HasConcreteMember(ITypeSymbol type, string requested) {
    if (type is not INamedTypeSymbol named) return false;
    for (var current = named; current is not null; current = current.BaseType)
      if (current.GetMembers(requested).Any(item => item is not IMethodSymbol { IsAbstract: true }))
        return true;
    return false;
  }

  private static bool IsPartialSymbol(object subject) {
    if (subject is INamedTypeSymbol named) return IsPartial(named);
    return subject is IMethodSymbol method && method.DeclaringSyntaxReferences.Any(item =>
      item.GetSyntax() is MethodDeclarationSyntax syntax && syntax.Modifiers.Any(SyntaxKind.PartialKeyword)
    );
  }

  private static bool IsPredicate(MixinExpressionProperty property) {
    return property.Name is
      "exists" or "is" or "has" or "eq" or "isSelf" or "ref" or "in" or "out" or "inout" or
      "argument" or "static" or "public" or "exposed" or "top" or "concrete" or
      "partial" or "generic" or "struct" or "class" or "matches" or "signature" or "wireable";
  }

  private static string NameOf(object subject) {
    return subject switch {
      ISymbol symbol => symbol.Name,
      AttributeData attribute => attribute.AttributeClass?.Name,
      TypedConstant constant => Convert.ToString(constant.Value, CultureInfo.InvariantCulture),
      ImplicitMixinValue value => Convert.ToString(value.Value, CultureInfo.InvariantCulture),
      _ => Convert.ToString(subject, CultureInfo.InvariantCulture)
    };
  }

  private static ITypeSymbol AsType(object subject) {
    return subject switch {
      ITypeSymbol type => type,
      IMethodSymbol method => method.ReturnType,
      IPropertySymbol property => property.Type,
      IFieldSymbol field => field.Type,
      IParameterSymbol parameter => parameter.Type,
      IEventSymbol @event => @event.Type,
      AttributeData attribute => attribute.AttributeClass,
      TypedConstant constant => constant.Type,
      ImplicitMixinValue value => value.Type,
      _ => null
    };
  }

  private bool TryRender(
    object subject,
    string root,
    out string value,
    out string error
  ) {
    error = null;
    switch (subject) {
      case string text:
        value = text;
        return true;
      case INamedTypeSymbol when root is "this" or "target":
        value = "this";
        return true;
      case ITypeSymbol type:
        value = type.ToDisplayString(TypeDisplayFormat);
        return true;
      case IParameterSymbol parameter:
        value = EscapeIdentifier(parameter.Name);
        return true;
      case IFieldSymbol field:
        value = SymbolReference(field);
        return true;
      case IPropertySymbol property:
        value = SymbolReference(property);
        return true;
      case IMethodSymbol method:
        value = SymbolReference(method);
        return true;
      case ISymbol symbol:
        value = EscapeIdentifier(symbol.Name);
        return true;
      case TypedConstant constant:
        value = ConstantExpression(constant);
        return true;
      case ImplicitMixinValue implicitValue:
        value = ConstantExpression(implicitValue.Value, implicitValue.Type);
        return true;
      case AttributeData attribute:
        value = attribute.AttributeClass?.ToDisplayString(TypeDisplayFormat);
        return true;
      default:
        value = null;
        error = "the expression value cannot be rendered";
        return false;
    }
  }

  private static string SymbolReference(ISymbol symbol) {
    var name = EscapeIdentifier(symbol.Name);
    return symbol.IsStatic
      ? symbol.ContainingType.ToDisplayString(TypeDisplayFormat) + "." + name
      : "this." + name;
  }

  private static string ConstantExpression(TypedConstant constant) {
    if (constant.IsNull) return "null";
    if (constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type)
      return "typeof(" + type.ToDisplayString(TypeDisplayFormat) + ")";
    if (constant.Kind == TypedConstantKind.Array)
      return "new[] { " + string.Join(", ", constant.Values.Select(ConstantExpression)) + " }";
    return constant.Value switch {
      string text => SyntaxFactory.Literal(text).ToFullString(),
      char character => SyntaxFactory.Literal(character).ToFullString(),
      bool boolean => boolean ? "true" : "false",
      float single => single.ToString("R", CultureInfo.InvariantCulture) + "F",
      double number => number.ToString("R", CultureInfo.InvariantCulture) + "D",
      decimal number => number.ToString(CultureInfo.InvariantCulture) + "M",
      uint number => number.ToString(CultureInfo.InvariantCulture) + "U",
      long number => number.ToString(CultureInfo.InvariantCulture) + "L",
      ulong number => number.ToString(CultureInfo.InvariantCulture) + "UL",
      _ => Convert.ToString(constant.Value, CultureInfo.InvariantCulture)
    };
  }

  internal static string ConstantExpression(object value, ITypeSymbol type = null) {
    if (value is null) return "null";
    if (value is ITypeSymbol typeValue) return "typeof(" + typeValue.ToDisplayString(TypeDisplayFormat) + ")";
    if (type?.TypeKind == TypeKind.Enum) {
      return "(" + type.ToDisplayString(TypeDisplayFormat) + ")" +
        Convert.ToString(value, CultureInfo.InvariantCulture);
    }
    return value switch {
      string text => SyntaxFactory.Literal(text).ToFullString(),
      char character => SyntaxFactory.Literal(character).ToFullString(),
      bool boolean => boolean ? "true" : "false",
      float single => single.ToString("R", CultureInfo.InvariantCulture) + "F",
      double number => number.ToString("R", CultureInfo.InvariantCulture) + "D",
      decimal number => number.ToString(CultureInfo.InvariantCulture) + "M",
      uint number => number.ToString(CultureInfo.InvariantCulture) + "U",
      long number => number.ToString(CultureInfo.InvariantCulture) + "L",
      ulong number => number.ToString(CultureInfo.InvariantCulture) + "UL",
      _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };
  }
}
