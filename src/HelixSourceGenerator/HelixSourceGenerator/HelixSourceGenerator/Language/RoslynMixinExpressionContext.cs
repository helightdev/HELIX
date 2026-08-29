using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Functions;
using HelixSourceGenerator.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HelixSourceGenerator.Shared.GeneratorAnalysis;

namespace HelixSourceGenerator.Language;

internal sealed class ImplicitMixinValue {
  internal ImplicitMixinValue(ITypeSymbol type, object value) {
    Type = type;
    Value = value;
  }

  internal ITypeSymbol Type { get; }
  internal object Value { get; }
}

internal sealed record MixinTargetSyntax(
  string Name, bool IsStatic, bool IsPublic, string DelegateType
);

internal sealed class MixinPropStructHandle {
  internal MixinPropStructHandle(
    IReadOnlyList<PropDefinition> props,
    PropStructModel model,
    bool augmenting
  ) {
    Props = props;
    Model = model;
    Augmenting = augmenting;
  }

  internal IReadOnlyList<PropDefinition> Props { get; }
  internal PropStructModel Model { get; }
  internal bool Augmenting { get; }
}

internal sealed record MixinGeneratedStructReference(string Name, string TypeName);

internal sealed class RoslynMixinExpressionContext :
  IMixinExpressionValueContext,
  IMixinExpressionSignatureContext,
  IMixinExpressionPropStructContext,
  IMixinExpressionConfigurablePropStructContext,
  IMixinExpressionStructAugmentationContext {
  private static readonly SymbolDisplayFormat FullNameDisplayFormat =
    SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(
      SymbolDisplayGenericsOptions.IncludeTypeParameters
    );
  private readonly IReadOnlyList<IParameterSymbol> _arguments;
  private readonly AttributeData _attribute;
  private readonly CSharpCompilation _compilation;
  private readonly Dictionary<string, MixinGeneratedStructReference> _generatedStructs =
    new(StringComparer.Ordinal);
  private readonly INamedTypeSymbol _implicitAttributeType;
  private readonly IReadOnlyDictionary<string, ImplicitMixinValue> _implicitValues;
  private readonly MixinLibraryCatalog _libraries;
  private readonly MixinExpressionPreparedState _preparedExpressions;
  private readonly ISymbol _target;
  private readonly IReadOnlyDictionary<string, string> _targetDefinitions;

  internal RoslynMixinExpressionContext(
    INamedTypeSymbol thisType,
    ISymbol target,
    AttributeData attribute,
    IReadOnlyList<IParameterSymbol> arguments,
    CSharpCompilation compilation,
    INamedTypeSymbol implicitAttributeType = null,
    IReadOnlyDictionary<string, ImplicitMixinValue> implicitValues = null,
    IReadOnlyDictionary<string, string> targetDefinitions = null,
    MixinExpressionPreparedState preparedExpressions = null,
    MixinLibraryCatalog libraries = null
  ) {
    CurrentType = thisType;
    _target = target;
    _attribute = attribute;
    _arguments = arguments ?? Array.Empty<IParameterSymbol>();
    _compilation = compilation;
    _implicitAttributeType = implicitAttributeType;
    _implicitValues = implicitValues;
    _targetDefinitions = targetDefinitions;
    _preparedExpressions = preparedExpressions;
    _libraries = libraries;
  }

  internal INamedTypeSymbol CurrentType { get; }

  public bool TryCreatePropStruct(
    string structName,
    MixinExpressionReference syntaxTarget,
    bool generateDatatype,
    bool generateDeclaration,
    out object handle,
    out string declaration,
    out string error
  ) {
    handle = null;
    declaration = null;
    error = null;
    if (!IsValidIdentifier(structName)) {
      error = "prop struct name '" + (structName ?? "") + "' is not a valid identifier";
      return false;
    }
    if (syntaxTarget.Properties.Count != 0 ||
      !TrySubject(syntaxTarget, out var subject, out error)) return false;

    IReadOnlyList<PropDefinition> props;
    switch (subject) {
      case IMethodSymbol { TypeParameters.Length: > 0 } method:
        error = "generic method '" + method.Name + "' cannot be used as a prop struct syntax target";
        return false;
      case IMethodSymbol method:
        props = method.Parameters.Select(parameter => new PropDefinition(
            parameter,
            parameter.Type,
            parameter.Name,
            Attribute(parameter, GeneratorStrings.Attributes.Prop),
            parameter.RefKind
          )
        ).ToArray();
        break;
      case INamedTypeSymbol type:
        props = InstanceFields(type).Select(field => new PropDefinition(
            field,
            field.Type,
            field.Name,
            Attribute(field, GeneratorStrings.Attributes.Prop)
          )
        ).ToArray();
        break;
      default:
        error = "PROP_STRUCT syntax target must resolve to a method or named type";
        return false;
    }

    if (!PropStructApi.TryAnalyzeProps(props, out var model, out var diagnostic)) {
      error = diagnostic.GetMessage(CultureInfo.InvariantCulture);
      return false;
    }
    handle = new MixinPropStructHandle(props, model, false);
    if (!generateDeclaration) return true;

    IReadOnlyList<string> configuration = Array.Empty<string>();
    if (generateDatatype && !PropStructMixinApi.TryAnalyzeInlineConfiguration(
      CurrentType,
      props.Select(prop => prop.Symbol).ToArray(),
      _compilation,
      _preparedExpressions,
      _libraries,
      out configuration,
      out error
    )) return false;

    var escapedName = EscapeIdentifier(structName);
    var builder = new SharpStringBuilder();
    using (builder.Type("public struct " + escapedName)) {
      foreach (var prop in props) {
        builder.Field(
          "public",
          prop.Type.ToDisplayString(TypeDisplayFormat),
          EscapeIdentifier(prop.Name)
        );
      }
      if (model.ParameterParts.Count > 0) {
        builder.BlankLine();
        using (builder.Method(
          "public" + (model.RequiresUnsafe ? " unsafe " : " ") + escapedName,
          model.ParameterParts
        )) model.AppendAssignments(builder, "this");
      }
      if (generateDatatype) {
        builder.BlankLine();
        PropStructApi.AnalyzeDatatype(escapedName, structName, props)
          .AppendMember(builder, configuration);
      }
    }
    declaration = builder.ToString();
    return true;
  }

  public bool TryCreatePropStruct(
    string structName,
    MixinExpressionReference syntaxTarget,
    out object handle,
    out string declaration,
    out string error
  ) {
    return TryCreatePropStruct(
      structName, syntaxTarget, false, true,
      out handle, out declaration, out error
    );
  }

  public bool TryApplyPropStructProperty(
    object handle,
    MixinExpressionProperty property,
    out object value,
    out string error
  ) {
    value = null;
    error = null;
    if (handle is not MixinPropStructHandle propStruct) {
      error = ":" + property.Name + " must be called on a prop struct handle";
      return false;
    }
    switch (property.Name) {
      case "structHasEquality":
        value = propStruct.Model.Equality.HasMembers;
        return true;
      case "structNoArgs":
        value = propStruct.Model.ParameterParts.Count == 0;
        return true;
      case "structAugment":
        value = propStruct.Augmenting;
        return true;
      case "structParams":
        value = JoinStructParts(property.Argument, propStruct.Model.ParameterParts);
        return true;
      case "structArgs":
        value = JoinStructParts(property.Argument, propStruct.Model.ArgumentParts);
        return true;
      case "propStructCall":
        break;
      default:
        error = "unknown prop struct property ':" + property.Name + "'";
        return false;
    }
    if (property.Arguments.Count != 2) {
      error = ":propStructCall requires a target and prop struct variable";
      return false;
    }
    var target = property.Arguments[0];
    var variable = property.Arguments[1];
    if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(variable)) {
      error = ":propStructCall target and variable cannot be empty";
      return false;
    }
    var arguments = propStruct.Props.Select(prop => {
        var modifier = prop.RefKind switch {
          RefKind.Ref => "ref ",
          RefKind.Out => "out ",
          RefKind.In => "in ",
          _ => ""
        };
        return modifier + variable + "." + EscapeIdentifier(prop.Name);
      }
    );
    value = target + "(" + string.Join(", ", arguments) + ")";
    return true;
  }

  public bool TryResolveMixin(string target, out string callable, out string error) {
    error = null;
    callable = null;
    var syntax = ParseMixinTarget(target, _targetDefinitions);
    if (!string.IsNullOrEmpty(syntax.DelegateType)) {
      var delegateType = ResolveType(syntax.DelegateType);
      if (delegateType is { TypeKind: TypeKind.Delegate })
        callable = delegateType.ToDisplayString(TypeDisplayFormat);
      return true;
    }
    var name = syntax.Name;
    if (string.IsNullOrEmpty(name)) return true;
    var resolvedDelegate = ResolveType(name);
    if (resolvedDelegate is { TypeKind: TypeKind.Delegate }) {
      callable = resolvedDelegate.ToDisplayString(TypeDisplayFormat);
      return true;
    }
    var methods = MethodsInHierarchy(CurrentType, name).ToArray();
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

  public bool TryAugmentPropStruct(
    MixinExpressionReference syntaxTarget,
    out object handle,
    out string declaration,
    out string error
  ) {
    handle = null;
    declaration = null;
    error = null;
    if (syntaxTarget.Properties.Count != 0) {
      error = "AUGMENT_STRUCT syntax target cannot have properties";
      return false;
    }
    if (syntaxTarget.Root == MixinExpressionRoot.This && !string.IsNullOrEmpty(syntaxTarget.Member) &&
      CurrentType.GetMembers(syntaxTarget.Member).Length == 0) {
      if (!IsValidIdentifier(syntaxTarget.Member)) {
        error = "struct name '" + syntaxTarget.Member + "' is not a valid identifier";
        return false;
      }
      var name = EscapeIdentifier(syntaxTarget.Member);
      var generated = new MixinGeneratedStructReference(
        syntaxTarget.Member,
        CurrentType.ToDisplayString(TypeDisplayFormat) + "." + name
      );
      _generatedStructs[syntaxTarget.Member] = generated;
      handle = new MixinPropStructHandle(
        Array.Empty<PropDefinition>(), PropStructModel.Empty, true
      );
      declaration = "public struct " + name + " { }";
      return true;
    }
    if (!TrySubject(syntaxTarget, out var subject, out error)) return false;
    if (subject is not INamedTypeSymbol { TypeKind: TypeKind.Struct } type) {
      error = "AUGMENT_STRUCT syntax target must resolve to a struct";
      return false;
    }
    var augmentingThis = SymbolEqualityComparer.Default.Equals(type, CurrentType);
    if (augmentingThis) {
      if (!IsPartial(type)) {
        error = "struct '" + type.Name + "' must be partial to be augmented";
        return false;
      }
      var generateDatatype = _attribute?.AttributeClass?.ToDisplayString() ==
        GeneratorStrings.Attributes.Structure &&
        _attribute.ConstructorArguments.Length > 0 &&
        _attribute.ConstructorArguments[0].Value is true;
      if (!PropStructApi.TryAnalyze(type, out var selfModel, out var selfDiagnostic, generateDatatype)) {
        error = selfDiagnostic.GetMessage(CultureInfo.InvariantCulture);
        return false;
      }
      var selfProps = InstanceFields(type).Select(field => new PropDefinition(
          field,
          field.Type,
          field.Name,
          Attribute(field, GeneratorStrings.Attributes.Prop)
        )
      ).ToArray();
      IReadOnlyList<string> configuration = Array.Empty<string>();
      if (generateDatatype && !PropStructMixinApi.TryAnalyzeInlineConfiguration(
        type, selfProps.Select(prop => prop.Symbol).ToArray(), _compilation,
        _preparedExpressions, _libraries, out configuration, out error,
        true
      )) return false;
      var selfBuilder = new SharpStringBuilder();
      if (selfModel.ParameterParts.Count > 0) {
        using (selfBuilder.Method(
          AccessibilityText(type.DeclaredAccessibility) +
          (selfModel.RequiresUnsafe ? " unsafe " : " ") + EscapeIdentifier(type.Name),
          selfModel.ParameterParts
        )) selfModel.AppendAssignments(selfBuilder, "this");
      }
      selfModel.Equality.AppendMembers(selfBuilder);
      if (selfModel.Datatype is not null) {
        selfBuilder.BlankLine();
        selfModel.Datatype.AppendMember(selfBuilder, configuration);
      }
      handle = new MixinPropStructHandle(selfProps, selfModel, true);
      declaration = selfBuilder.ToString();
      return true;
    }
    if (!SymbolEqualityComparer.Default.Equals(type.ContainingType, CurrentType)) {
      error = "AUGMENT_STRUCT syntax target must be a struct nested directly in the current type";
      return false;
    }
    if (!IsPartial(type)) {
      error = "struct '" + type.Name + "' must be partial to be augmented";
      return false;
    }
    if (!PropStructApi.TryAnalyze(type, out var model, out var diagnostic)) {
      error = diagnostic.GetMessage(CultureInfo.InvariantCulture);
      return false;
    }

    var props = InstanceFields(type).Select(field => new PropDefinition(
        field,
        field.Type,
        field.Name,
        Attribute(field, GeneratorStrings.Attributes.Prop)
      )
    ).ToArray();
    var builder = new SharpStringBuilder();
    using (builder.Type("partial struct " + EscapeIdentifier(type.Name))) {
      if (model.ParameterParts.Count > 0) {
        using (builder.Method(
          AccessibilityText(type.DeclaredAccessibility) +
          (model.RequiresUnsafe ? " unsafe " : " ") +
          EscapeIdentifier(type.Name),
          model.ParameterParts
        )) model.AppendAssignments(builder, "this");
      }
      model.Equality.AppendMembers(builder);
    }
    handle = new MixinPropStructHandle(props, model, true);
    declaration = builder.ToString();
    return true;
  }

  public bool TryResolve(
    MixinExpressionReference reference,
    out string value,
    out string error
  ) {
    if (!TryResolveValue(reference, out var subject, out error)) {
      value = null;
      return false;
    }
    MixinExpressionRoot? renderRoot = reference.Properties.Any(item => !IsPredicate(item) && item.Name == "type"
    )
      ? null
      : reference.Root;
    return TryRender(subject, renderRoot, out value, out error);
  }

  public bool TryResolveValue(
    MixinExpressionReference reference,
    out object value,
    out string error
  ) {
    if (!TrySubject(reference, out value, out error) ||
      !ApplyValueProperties(reference, ref value, out error)) return false;
    if (!reference.Properties.Any(IsPredicate)) return true;
    value = null;
    error = "boolean pseudo-properties cannot be used as values";
    return false;
  }

  public bool TryRenderValue(
    object value, MixinExpressionRoot? root, out string text, out string error
  ) {
    return TryRender(value, root, out text, out error);
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
        if (!TryEvaluatePredicate(null, predicate, out var item, out error)) return false;
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
      if (!TryEvaluatePredicate(subject, predicate, out var item, out error)) return false;
      value &= predicate.Negated ? !item : item;
    }
    return true;
  }

  private static string JoinStructParts(string prefix, IReadOnlyList<string> parts) {
    var hasPrefix = !string.IsNullOrWhiteSpace(prefix);
    if (!hasPrefix) return string.Join(", ", parts);
    return parts.Count == 0
      ? prefix
      : prefix + ", " + string.Join(", ", parts);
  }

  private bool TrySubject(
    MixinExpressionReference reference,
    out object subject,
    out string error
  ) {
    subject = null;
    error = null;
    switch (reference.Root) {
      case MixinExpressionRoot.This: subject = CurrentType; break;
      case MixinExpressionRoot.Target: subject = _target; break;
      case MixinExpressionRoot.Attribute: subject = (object)_attribute ?? _implicitAttributeType; break;
      case MixinExpressionRoot.Argument:
        subject = string.IsNullOrEmpty(reference.Member) ? ArgumentsTable() : SelectArgument(reference.Member);
        if (subject is null) error = "unknown argument '" + (reference.Member ?? "") + "'";
        return subject is not null;
      default:
        error = "unknown expression root '@" + reference.Root.Keyword() + "'";
        return false;
    }
    if (subject is null) {
      error = "@" + reference.Root.Keyword() + " is not available in this context";
      return false;
    }
    if (!string.IsNullOrEmpty(reference.Member)) {
      subject = SelectMember(subject, reference.Member);
      if (subject is null) {
        error = "member '" + reference.Member + "' was not found on @" + reference.Root.Keyword();
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

  private MixinExpressionTable ArgumentsTable() {
    var table = new MixinExpressionTable();
    for (var index = 0; index < _arguments.Count; index++)
      table = table.Put(index.ToString(CultureInfo.InvariantCulture), _arguments[index]);
    return table;
  }

  private object SelectMember(object subject, string name) {
    if (_implicitAttributeType is not null &&
      SymbolEqualityComparer.Default.Equals(subject as ISymbol, _implicitAttributeType)) {
      if (_implicitValues is not null && _implicitValues.TryGetValue(name, out var implicitValue)) return implicitValue;
      return null;
    }
    if (subject is AttributeData attribute) {
      foreach (var named in attribute.NamedArguments) {
        if (string.Equals(named.Key, name, StringComparison.OrdinalIgnoreCase))
          return named.Value;
      }
      if (attribute.AttributeConstructor is { } constructor) {
        for (var index = 0; index < constructor.Parameters.Length && index < attribute.ConstructorArguments.Length;
          index++) {
          if (string.Equals(constructor.Parameters[index].Name, name, StringComparison.OrdinalIgnoreCase))
            return attribute.ConstructorArguments[index];
        }
      }
      if (TryDefaultAttributeMember(attribute.AttributeClass, name, out var defaultValue)) return defaultValue;
      return null;
    }
    if (subject is IMethodSymbol method) {
      return method.Parameters.FirstOrDefault(item => item.Name == name) ??
        method.Parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    }
    if (SymbolEqualityComparer.Default.Equals(subject as ISymbol, CurrentType) &&
      _generatedStructs.TryGetValue(name, out var generatedStruct)) return generatedStruct;
    var type = AsType(subject);
    if (type is null) return null;
    for (var current = type; current is not null; current = current.BaseType) {
      var member = current.GetMembers().FirstOrDefault(item => item.Name == name) ??
        current.GetMembers().FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
      if (member is not null) return member;
    }
    return null;
  }

  internal object SelectValueMember(object subject, string name) {
    return SelectMember(subject, name);
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

  private bool ApplyValueProperties(
    MixinExpressionReference reference,
    ref object subject,
    out string error
  ) {
    foreach (var property in reference.Properties.Where(item => !IsPredicate(item))) {
      if (!FunctionLibrary.TryInvoke(
        property, this, reference.Root.Keyword(), reference.Member, ref subject, out error
      )) return false;
    }
    error = null;
    return true;
  }

  internal bool IsGeneratedStructType(string name) {
    return _generatedStructs.Values.Any(item =>
      string.Equals(item.TypeName, name, StringComparison.Ordinal) ||
      string.Equals(item.TypeName.Replace("global::", ""), name.Replace("global::", ""), StringComparison.Ordinal)
    );
  }

  internal static string FullNameOf(object subject) {
    return TypeValueOf(subject)
      ?.ToDisplayString(FullNameDisplayFormat);
  }

  internal static object Unwrap(object subject) {
    switch (subject) {
      case MixinGeneratedStructReference generated:
        return generated.TypeName.Replace("global::", "");
      case TypedConstant { Kind: TypedConstantKind.Type, Value: ITypeSymbol type }:
        return UnqualifiedGlobalName(type);
      case TypedConstant { Value: string text }:
        return text;
      case TypedConstant constant:
        return constant.Value;
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

  internal static ITypeSymbol TypeValueOf(object subject) {
    return subject switch {
      TypedConstant { Kind: TypedConstantKind.Type, Value: ITypeSymbol type } => type,
      ImplicitMixinValue { Value: ITypeSymbol type } => type,
      _ => AsType(subject)
    };
  }

  private static string UnqualifiedGlobalName(ITypeSymbol type) {
    return type.ToDisplayString(TypeDisplayFormat).Replace("global::", "");
  }

  internal static ITypeSymbol SelectTypeArgument(object subject, string path) {
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

  internal static bool IsTruthy(object subject) {
    return subject switch {
      null => false,
      TypedConstant constant => constant.Kind != TypedConstantKind.Error &&
        !constant.IsNull && constant.Value is not false,
      ImplicitMixinValue value => value.Value is not null && value.Value is not false,
      bool value => value,
      _ => true
    };
  }

  private bool TryEvaluatePredicate(
    object subject,
    MixinExpressionProperty property,
    out bool value,
    out string error
  ) {
    if (FunctionLibrary.TryGet(property.Name, out var function) &&
      function is PredicateFunctionDefinition predicate)
      return predicate.Evaluate(new RoslynMixinValue(this, subject), property, out value, out error);
    value = false;
    error = "unknown boolean pseudo-property ':?" + property.Name + "'";
    return false;
  }

  private static bool IsDeveloperExpressionError(string error) {
    return error?.StartsWith("unknown expression root", StringComparison.Ordinal) == true ||
      error?.StartsWith("unknown value property", StringComparison.Ordinal) == true ||
      error?.StartsWith("unknown boolean pseudo-property", StringComparison.Ordinal) == true ||
      error?.Contains(" requires a ") == true;
  }

  internal IMethodSymbol ResolveCallable(object value) {
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
    var methods = MethodsInHierarchy(CurrentType, name).ToArray();
    return methods.Length == 1 ? methods[0] : null;
  }

  internal INamedTypeSymbol ResolveType(string name) {
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
    for (var current = type; current is not null; current = current.BaseType) {
      foreach (var method in current.GetMembers(name).OfType<IMethodSymbol>())
        yield return method;
    }
  }

  internal static MixinTargetSyntax ParseMixinTarget(
    string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    var value = target?.Trim() ?? "";
    if (targetDefinitions is not null && targetDefinitions.TryGetValue(value, out var defined))
      return ParseMixinTarget(defined, targetDefinitions);

    var isStatic = false;
    var isPublic = false;
    while (value.Length != 0) {
      if (value[0] == '*' && !isStatic) {
        isStatic = true;
        value = value.Substring(1);
        continue;
      }
      if (value[0] == '^' && !isPublic) {
        isPublic = true;
        value = value.Substring(1);
        continue;
      }
      break;
    }

    string name;
    string delegateType = null;
    if (value.StartsWith("~", StringComparison.Ordinal)) {
      delegateType = value.Substring(1);
      var normalized = delegateType.StartsWith("global::", StringComparison.Ordinal)
        ? delegateType.Substring("global::".Length)
        : delegateType;
      var separator = Math.Max(normalized.LastIndexOf('.'), normalized.LastIndexOf('+'));
      name = separator < 0 ? normalized : normalized.Substring(separator + 1);
    } else {
      var separator = value.IndexOf(':');
      if (separator >= 0) {
        name = value.Substring(0, separator);
        delegateType = value.Substring(separator + 1);
      } else name = value;
    }

    name = name switch {
      "$Init" => "Awake",
      "$Dispose" => "OnDestroy",
      _ when name.StartsWith("$", StringComparison.Ordinal) => name.Substring(1),
      _ => name
    };
    return new MixinTargetSyntax(name, isStatic, isPublic, delegateType);
  }

  private static string CallableReference(IMethodSymbol method) {
    return method.ContainingType.ToDisplayString(TypeDisplayFormat) + "." + method.Name;
  }

  internal static bool HaveSameSignature(IMethodSymbol first, IMethodSymbol second) {
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

  internal bool TryWireParameters(
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

  internal static bool EqualTo(object subject, string expected) {
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

  internal static bool TryComparableText(object subject, out string value) {
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

  internal bool IsOrInherits(ITypeSymbol type, string requested) {
    bool Matches(ITypeSymbol candidate) {
      var display = candidate.ToDisplayString(TypeDisplayFormat);
      return candidate.Name == requested || display == requested || display == "global::" + requested ||
        candidate.ToDisplayString() == requested;
    }

    if (Matches(type)) return true;
    if (type is not INamedTypeSymbol named) return false;
    for (var current = named.BaseType; current is not null; current = current.BaseType) {
      if (Matches(current))
        return true;
    }
    return named.AllInterfaces.Any(Matches);
  }

  internal static bool HasConcreteMember(ITypeSymbol type, string requested) {
    if (type is not INamedTypeSymbol named) return false;
    for (var current = named; current is not null; current = current.BaseType) {
      if (current.GetMembers(requested).Any(item => item is not IMethodSymbol { IsAbstract: true }))
        return true;
    }
    return false;
  }

  internal static bool IsPartialSymbol(object subject) {
    if (subject is INamedTypeSymbol named) return IsPartial(named);
    return subject is IMethodSymbol method && method.DeclaringSyntaxReferences.Any(item =>
      item.GetSyntax() is MethodDeclarationSyntax syntax && syntax.Modifiers.Any(SyntaxKind.PartialKeyword)
    );
  }

  private static bool IsPredicate(MixinExpressionProperty property) {
    return FunctionLibrary.IsPredicate(property.Name);
  }

  internal static string NameOf(object subject) {
    return subject switch {
      MixinGeneratedStructReference generated => generated.Name,
      ISymbol symbol => symbol.Name,
      AttributeData attribute => attribute.AttributeClass?.Name,
      TypedConstant constant => Convert.ToString(constant.Value, CultureInfo.InvariantCulture),
      ImplicitMixinValue value => Convert.ToString(value.Value, CultureInfo.InvariantCulture),
      _ => Convert.ToString(subject, CultureInfo.InvariantCulture)
    };
  }

  internal static ITypeSymbol AsType(object subject) {
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
    MixinExpressionRoot? root,
    out string value,
    out string error
  ) {
    error = null;
    switch (subject) {
      case null:
        value = "null";
        return true;
      case bool boolean:
        value = boolean ? "true" : "false";
        return true;
      case char or sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal:
        value = ConstantExpression(subject);
        return true;
      case string text:
        value = text;
        return true;
      case MixinGeneratedStructReference generated:
        value = generated.TypeName;
        return true;
      case INamedTypeSymbol when root is MixinExpressionRoot.This or MixinExpressionRoot.Target:
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
      case MixinExpressionTable table:
        value = null;
        error = "a table cannot be rendered directly; select an entry or use :joinKeys, :joinValues or :join";
        return false;
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