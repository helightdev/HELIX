using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HelixSourceGenerator.Language;

internal sealed record MixinTargetDescriptor(string Name, bool IsStatic, bool IsPublic, string DelegateType);

internal sealed record MixinPropStructValue(IReadOnlyList<PropDefinition> Props,
  PropStructModel Model, bool Augmenting) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) => true;
  public MixinString Render(ExecutionContext context) => context.Intern("<prop struct>");
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(MixinPropStructValue)); builder.Append(Props.Count); builder.Append(Augmenting);
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) => NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => this;
  public bool Equals(IMixinValue other) => ReferenceEquals(this, other);
}

internal sealed record DetachedSemanticData(
  string Rendered, string Unwrapped, string Name, string TypeName, string FullName, string Visibility,
  IReadOnlyList<string> AssignableTypes, IReadOnlyList<string> Traits,
  IReadOnlyDictionary<string, DetachedSemanticData> Members
);

internal sealed record DetachedSemanticMixinValue(
  MixinString Rendered, MixinString Unwrapped, MixinString Name, MixinString TypeName, MixinString FullName,
  MixinString Visibility, IReadOnlyList<MixinString> AssignableTypes,
  IReadOnlyList<MixinString> Traits,
  IReadOnlyList<KeyValuePair<MixinString, IMixinValue>> Members
) : IMixinValue {
  internal static DetachedSemanticMixinValue Materialize(DetachedSemanticData value, MixinStringPool strings) => new(
    strings.Get(value.Rendered), strings.Get(value.Unwrapped), strings.Get(value.Name), strings.Get(value.TypeName),
    strings.Get(value.FullName), strings.Get(value.Visibility),
    value.AssignableTypes.Select(strings.Get).ToArray(), value.Traits.Select(strings.Get).ToArray(),
    value.Members.Select(item => new KeyValuePair<MixinString, IMixinValue>(
      strings.Get(item.Key), Materialize(item.Value, strings))).ToArray()
  );

  public bool IsTruthy(ExecutionContext context) => true;
  public MixinString Render(ExecutionContext context) => Rendered;
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(DetachedSemanticMixinValue)); builder.Append(Rendered.Resolve(context.Strings));
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) =>
    Members.FirstOrDefault(item => item.Key == member).Value ?? NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => new DetachedSemanticData(
    Rendered.Resolve(context.Strings), Unwrapped.Resolve(context.Strings), Name.Resolve(context.Strings), TypeName.Resolve(context.Strings),
    FullName.Resolve(context.Strings), Visibility.Resolve(context.Strings),
    AssignableTypes.Select(item => item.Resolve(context.Strings)).ToArray(),
    Traits.Select(item => item.Resolve(context.Strings)).ToArray(),
    Members.ToDictionary(item => item.Key.Resolve(context.Strings),
      item => (DetachedSemanticData)item.Value.Unlink(context), StringComparer.OrdinalIgnoreCase)
  );
  public bool Equals(IMixinValue other) => other is DetachedSemanticMixinValue value && Equals(value);
}

/// <summary>Roslyn host services for an already lowered mixin program.</summary>
internal sealed class RoslynMixinContext : ExecutionContext {
  private readonly IReadOnlyList<IParameterSymbol> _arguments;
  private readonly AttributeData _attribute;
  private readonly CSharpCompilation _compilation;
  private readonly ISymbol _target;
  private readonly IReadOnlyDictionary<string, string> _targetDefinitions;
  private readonly MixinExpressionPreparedState _preparedExpressions;
  private readonly MixinLibraryCatalog _libraries;
  private readonly Dictionary<string, string> _generatedStructs = new(StringComparer.Ordinal);

  internal RoslynMixinContext(INamedTypeSymbol thisType, ISymbol target, AttributeData attribute,
    IReadOnlyList<IParameterSymbol> arguments, CSharpCompilation compilation,
    INamedTypeSymbol implicitAttributeType = null,
    IReadOnlyDictionary<string, object> implicitValues = null,
    IReadOnlyDictionary<string, string> targetDefinitions = null,
    MixinExpressionPreparedState preparedExpressions = null, MixinLibraryCatalog libraries = null)
    : base(preparedExpressions?.StringPool ?? new MixinStringPoolBuilder().Freeze()) {
    CurrentType = thisType;
    _target = target;
    _attribute = attribute;
    _arguments = arguments ?? [];
    _compilation = compilation;
    _targetDefinitions = targetDefinitions;
    _preparedExpressions = preparedExpressions;
    _libraries = libraries;
  }

  internal INamedTypeSymbol CurrentType { get; }

  public override IMixinValue InvokeHostDirective(string name, IReadOnlyList<IMixinValue> arguments,
    IMixinValue operand) {
    var values = arguments.Select(Evaluate).ToArray();
    if (values.OfType<ErrorMixinValue>().FirstOrDefault() is { } argumentError) return argumentError;
    string Text(int index) => index < values.Length
      ? values[index].Render(this).Resolve(Strings) : "";
    switch (name) {
      case "RESOLVE_MIXIN": {
        var local = Text(0);
        var target = Evaluate(operand).Render(this).Resolve(Strings);
        var descriptor = ParseMixinTarget(target, _targetDefinitions);
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
        IMixinValue result = callable is null ? NullMixinValue.Instance : new LiteralMixinValue(Intern(callable));
        Locals[Intern(local)] = result;
        return result;
      }
      case "PROP_STRUCT": return CreatePropStruct(Text(0), Text(1), values.Skip(2).Select(item =>
        item.Render(this).Resolve(Strings)).ToArray(), operand);
      case "AUGMENT_STRUCT": return AugmentPropStruct(Text(0), operand);
      default: return base.InvokeHostDirective(name, arguments, operand);
    }
  }

  public override IMixinValue InvokeHostFunction(string name, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments) {
    value = Evaluate(value);
    string Text(IMixinValue item) => Evaluate(item).Render(this).Resolve(Strings);
    if (value is MixinPropStructValue prop) switch (name) {
      case "structHasEquality": return prop.Model.Equality.HasMembers ? BooleanMixinValue.True : BooleanMixinValue.False;
      case "structNoArgs": return prop.Model.ParameterParts.Count == 0 ? BooleanMixinValue.True : BooleanMixinValue.False;
      case "structAugment": return prop.Augmenting ? BooleanMixinValue.True : BooleanMixinValue.False;
      case "structParams": return new LiteralMixinValue(Intern(JoinStructParts(
        arguments.Count == 0 ? null : Text(arguments[0]), prop.Model.ParameterParts)));
      case "structArgs": return new LiteralMixinValue(Intern(JoinStructParts(
        arguments.Count == 0 ? null : Text(arguments[0]), prop.Model.ArgumentParts)));
      case "propStructCall": {
        var target = Text(arguments[0]); var variable = Text(arguments[1]);
        var callArguments = prop.Props.Select(item => (item.RefKind switch {
          RefKind.Ref => "ref ", RefKind.Out => "out ", RefKind.In => "in ", _ => ""
        }) + variable + "." + GeneratorAnalysis.EscapeIdentifier(item.Name));
        return new LiteralMixinValue(Intern(target + "(" + string.Join(", ", callArguments) + ")"));
      }
    }
    IMethodSymbol Callable(IMixinValue item) {
      item = Evaluate(item);
      if (item is RoslynMixinValue { Value: IMethodSymbol method }) return method;
      var text = Text(item);
      if (ResolveType(text) is INamedTypeSymbol { TypeKind: TypeKind.Delegate,
        DelegateInvokeMethod: { } invoke }) return invoke;
      var methodName = text.Substring(text.LastIndexOf('.') + 1);
      var methods = MethodsInHierarchy(CurrentType, methodName).ToArray();
      return methods.Length == 1 ? methods[0] : null;
    }
    if (name == "signature") return SameSignature(Callable(value), Callable(arguments[0]))
      ? BooleanMixinValue.True : BooleanMixinValue.False;
    if (name == "wireable") return TryWireParameters(Callable(arguments[0]), Callable(arguments[1]), out _)
      ? BooleanMixinValue.True : BooleanMixinValue.False;
    if (name == "wire") return TryWireParameters(Callable(value), Callable(arguments[0]), out var wired)
      ? new LiteralMixinValue(Intern(wired)) : Error("methods cannot be wired");
    return base.InvokeHostFunction(name, value, arguments);
  }

  private IMixinValue CreatePropStruct(string structName, string local, IReadOnlyList<string> flags,
    IMixinValue operand) {
    if (!SyntaxFacts.IsValidIdentifier(structName)) return Error("prop struct name '" + structName + "' is not a valid identifier");
    var subject = Evaluate(operand) is RoslynMixinValue roslyn ? roslyn.Value : null;
    IReadOnlyList<PropDefinition> props = subject switch {
      IMethodSymbol { TypeParameters.Length: > 0 } generic => null,
      IMethodSymbol method => method.Parameters.Select(parameter => new PropDefinition(parameter,
        parameter.Type, parameter.Name, GeneratorAnalysis.Attribute(parameter, GeneratorStrings.Attributes.Prop),
        parameter.RefKind)).ToArray(),
      INamedTypeSymbol type => GeneratorAnalysis.InstanceFields(type).Select(field => new PropDefinition(field,
        field.Type, field.Name, GeneratorAnalysis.Attribute(field, GeneratorStrings.Attributes.Prop))).ToArray(),
      _ => null
    };
    if (subject is IMethodSymbol { TypeParameters.Length: > 0 } methodError)
      return Error("generic method '" + methodError.Name + "' cannot be used as a prop struct target");
    if (props is null) return Error("PROP_STRUCT target must resolve to a method or named type");
    if (!PropStructApi.TryAnalyzeProps(props, out var model, out var diagnostic))
      return Error(diagnostic.GetMessage(CultureInfo.InvariantCulture));
    var handle = new MixinPropStructValue(props, model, false);
    Locals[Intern(local)] = handle;
    var generate = !flags.Any(item => item.Equals("noGenerate", StringComparison.OrdinalIgnoreCase));
    if (!generate) return handle;
    var datatype = flags.Any(item => item.Equals("datatype", StringComparison.OrdinalIgnoreCase));
    var escaped = GeneratorAnalysis.EscapeIdentifier(structName);
    var builder = new SharpStringBuilder();
    using (builder.Type("public struct " + escaped)) {
      foreach (var prop in props) builder.Field("public",
        prop.Type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat), GeneratorAnalysis.EscapeIdentifier(prop.Name));
      if (model.ParameterParts.Count > 0) {
        builder.BlankLine();
        using (builder.Method("public" + (model.RequiresUnsafe ? " unsafe " : " ") + escaped,
          model.ParameterParts)) model.AppendAssignments(builder, "this");
      }
      if (datatype) {
        builder.BlankLine();
        if (!TryDatatypeConfiguration(props.Select(item => item.Symbol).ToArray(), false,
          out var configuration, out var error))
          return Error(error);
        PropStructApi.AnalyzeDatatype(escaped, structName, props).AppendMember(builder, configuration);
      }
    }
    return new DirectiveEffectMixinValue(handle, Intern(builder.ToString()));
  }

  private bool TryDatatypeConfiguration(IReadOnlyList<ISymbol> properties, bool includeType,
    out IReadOnlyList<string> configuration, out string error) {
    var result = new List<string>();
    var variables = new Dictionary<string, object>(StringComparer.Ordinal);
    foreach (var owner in (includeType ? new ISymbol[] { CurrentType }.Concat(properties) : properties))
    foreach (var applied in owner.GetAttributes()
      .OrderBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)) {
      if (applied.AttributeClass?.ToDisplayString() == GeneratorStrings.Attributes.Structure) continue;
      foreach (var annotation in MixinLibraryApi.Annotations(applied.AttributeClass, _libraries)) {
        var arguments = owner is IParameterSymbol { ContainingSymbol: IMethodSymbol method }
          ? (IReadOnlyList<IParameterSymbol>)method.Parameters : [];
        var nested = new RoslynMixinContext(CurrentType, owner, applied, arguments, _compilation,
          targetDefinitions: _targetDefinitions, preparedExpressions: _preparedExpressions, libraries: _libraries);
        var evaluated = MixinExpressionVirtualMachine.Execute(annotation.Prelude + annotation.Expression,
          nested, variables, _preparedExpressions);
        if (!evaluated.Success) {
          configuration = null;
          error = "line " + evaluated.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + evaluated.Error;
          return false;
        }
        foreach (var output in evaluated.Outputs) {
          if (string.IsNullOrEmpty(output.Text)) continue;
          if (output.Target == MixinExpressionOutputTarget.Target) result.Add(output.Text);
          else if (output.Target is MixinExpressionOutputTarget.Mixin or MixinExpressionOutputTarget.Injection &&
            ParseMixinTarget(output.InjectionTarget, _targetDefinitions).Name == "ConfigureDatatype")
            result.Add(output.Text);
          else {
            configuration = null;
            error = "property mixins used by PROP_STRUCT may only target ConfigureDatatype";
            return false;
          }
        }
      }
    }
    configuration = result;
    error = null;
    return true;
  }

  private IMixinValue AugmentPropStruct(string local, IMixinValue operand) {
    object subject = null; string missingName = null;
    if (operand is RootMixinValue { Root: MixinExpressionRoot.This } root) {
      missingName = root.Member.Resolve(Strings);
      subject = string.IsNullOrEmpty(missingName) ? CurrentType : SelectMember(CurrentType, missingName);
    } else if (Evaluate(operand) is RoslynMixinValue roslyn) subject = roslyn.Value;
    if (subject is null && !string.IsNullOrEmpty(missingName)) {
      if (!SyntaxFacts.IsValidIdentifier(missingName)) return Error("struct name '" + missingName + "' is not a valid identifier");
      var empty = new MixinPropStructValue([], PropStructModel.Empty, true);
      Locals[Intern(local)] = empty;
      _generatedStructs[missingName] = CurrentType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." +
        GeneratorAnalysis.EscapeIdentifier(missingName);
      return new DirectiveEffectMixinValue(empty, Intern("public struct " + GeneratorAnalysis.EscapeIdentifier(missingName) + " { }"));
    }
    if (subject is not INamedTypeSymbol { TypeKind: TypeKind.Struct } type)
      return Error("AUGMENT_STRUCT target must resolve to a struct");
    if (!GeneratorAnalysis.IsPartial(type)) return Error("struct '" + type.Name + "' must be partial to be augmented");
    var generateDatatype = SymbolEqualityComparer.Default.Equals(type, CurrentType) &&
      _attribute?.AttributeClass?.ToDisplayString() == GeneratorStrings.Attributes.Structure &&
      _attribute.ConstructorArguments.Length > 0 && _attribute.ConstructorArguments[0].Value is true;
    if (!PropStructApi.TryAnalyze(type, out var model, out var diagnostic, generateDatatype))
      return Error(diagnostic.GetMessage(CultureInfo.InvariantCulture));
    var props = GeneratorAnalysis.InstanceFields(type).Select(field => new PropDefinition(field, field.Type,
      field.Name, GeneratorAnalysis.Attribute(field, GeneratorStrings.Attributes.Prop))).ToArray();
    var handle = new MixinPropStructValue(props, model, true);
    Locals[Intern(local)] = handle;
    var builder = new SharpStringBuilder();
    var augmentingThis = SymbolEqualityComparer.Default.Equals(type, CurrentType);
    if (augmentingThis) {
      if (model.ParameterParts.Count > 0) using (builder.Method(
        GeneratorAnalysis.AccessibilityText(type.DeclaredAccessibility) +
        (model.RequiresUnsafe ? " unsafe " : " ") + GeneratorAnalysis.EscapeIdentifier(type.Name),
        model.ParameterParts)) model.AppendAssignments(builder, "this");
      model.Equality.AppendMembers(builder);
      if (model.Datatype is not null) {
        builder.BlankLine();
        if (!TryDatatypeConfiguration(props.Select(item => item.Symbol).ToArray(), true,
          out var configuration, out var configurationError)) return Error(configurationError);
        model.Datatype.AppendMember(builder, configuration);
      }
    } else {
      if (!SymbolEqualityComparer.Default.Equals(type.ContainingType, CurrentType))
        return Error("AUGMENT_STRUCT target must be a struct nested directly in the current type");
      using (builder.Type("partial struct " + GeneratorAnalysis.EscapeIdentifier(type.Name))) {
        if (model.ParameterParts.Count > 0) using (builder.Method(
          GeneratorAnalysis.AccessibilityText(type.DeclaredAccessibility) +
          (model.RequiresUnsafe ? " unsafe " : " ") + GeneratorAnalysis.EscapeIdentifier(type.Name),
          model.ParameterParts)) model.AppendAssignments(builder, "this");
        model.Equality.AppendMembers(builder);
      }
    }
    return new DirectiveEffectMixinValue(handle, Intern(builder.ToString()));
  }

  public override MixinString NameOf(IMixinValue value) => value is RoslynMixinValue roslyn
    ? Intern((roslyn.Value as ISymbol)?.Name ??
      (roslyn.Value as AttributeData)?.AttributeClass?.Name ?? ComparableText(roslyn.Value))
    : value is DetachedSemanticMixinValue detached ? detached.Name
    : base.NameOf(value);

  public override bool IsType(IMixinValue value, MixinString requested) {
    if (value is DetachedSemanticMixinValue detached) {
      var expected = requested.Resolve(Strings).Replace("global::", "");
      return detached.AssignableTypes.Any(item => {
        var candidate = item.Resolve(Strings).Replace("global::", "");
        return candidate == expected || candidate.Split('.', '+').LastOrDefault() == expected;
      });
    }
    if (value is not RoslynMixinValue roslyn || TypeOf(roslyn.Value) is not INamedTypeSymbol type) return false;
    var name = requested.Resolve(Strings).Replace("global::", "");
    bool Match(ITypeSymbol candidate) => candidate is not null && (
      candidate.Name == name || candidate.ToDisplayString().Replace("global::", "") == name ||
      candidate.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "") == name
    );
    if (Match(type)) return true;
    for (var current = type.BaseType; current is not null; current = current.BaseType)
      if (Match(current)) return true;
    return type.AllInterfaces.Any(Match);
  }

  public override bool HasTrait(IMixinValue value, MixinString requested) {
    var name = requested.Resolve(Strings);
    if (value is DetachedSemanticMixinValue detached)
      return detached.Traits.Any(item => string.Equals(item.Resolve(Strings), name, StringComparison.Ordinal));
    return value is RoslynMixinValue roslyn && SemanticTraits(roslyn.Value).Contains(name, StringComparer.Ordinal);
  }

  internal override object UnlinkSnapshot(IMixinValue value, bool includeMembers) =>
    value is RoslynMixinValue roslyn ? roslyn.Unlink(this, includeMembers) : value.Unlink(this);

  internal static IReadOnlyList<string> SemanticTraits(object value) {
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
    if (symbol is IParameterSymbol parameter) result.Add(parameter.RefKind switch {
      RefKind.Ref => "ref", RefKind.In => "in", RefKind.Out => "out", _ => "argument"
    });
    return result;
  }

  public override IMixinValue Unwrap(IMixinValue value) {
    if (value is RoslynMixinValue { Value: TypedConstant constant }) {
      if (constant.IsNull || constant.Kind == TypedConstantKind.Error) return NullMixinValue.Instance;
      var raw = constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type
        ? type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "")
        : Convert.ToString(constant.Value, CultureInfo.InvariantCulture);
      return new LiteralMixinValue(Intern(raw));
    }
    if (value is DetachedSemanticMixinValue detached)
      return new LiteralMixinValue(detached.Unwrapped);
    return base.Unwrap(value);
  }

  public override IMixinValue Attributes(IMixinValue value, MixinString requested, bool exact, bool first) {
    if (value is not RoslynMixinValue roslyn) return first ? NullMixinValue.Instance : MixinTableValue.Empty;
    var expected = requested.IsInterned || requested.DynamicValue is not null
      ? requested.Resolve(Strings) : null;
    IEnumerable<AttributeData> source = roslyn.Value switch {
      ISymbol symbol => symbol.GetAttributes(),
      AttributeData attribute => attribute.AttributeClass is { } attributeType
        ? attributeType.GetAttributes() : Enumerable.Empty<AttributeData>(),
      _ => TypeOf(roslyn.Value) is { } valueType
        ? valueType.GetAttributes() : Enumerable.Empty<AttributeData>()
    };
    var matches = source.Where(attribute => expected is null || attribute.AttributeClass is { } type &&
      (exact ? TypeMatches(type, expected) : IsOrInherits(type, expected))).ToArray();
    if (first) return matches.Length == 0 ? NullMixinValue.Instance : new RoslynMixinValue(matches[0]);
    return new MixinTableValue(matches.Select((item, index) =>
      new KeyValuePair<MixinString, IMixinValue>(Intern(index.ToString(CultureInfo.InvariantCulture)),
        new RoslynMixinValue(item))).ToArray());
  }

  private bool IsOrInherits(ITypeSymbol type, string expected) {
    if (TypeMatches(type, expected)) return true;
    if (type is not INamedTypeSymbol named) return false;
    for (var current = named.BaseType; current is not null; current = current.BaseType)
      if (TypeMatches(current, expected)) return true;
    return named.AllInterfaces.Any(item => TypeMatches(item, expected));
  }

  private static bool TypeMatches(ITypeSymbol type, string expected) => type is not null && (
    type.Name == expected || type.ToDisplayString().Replace("global::", "") == expected.Replace("global::", "") ||
    type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "") == expected.Replace("global::", "")
  );

  protected override IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member) {
    if (root == MixinExpressionRoot.This && !string.IsNullOrEmpty(member.Resolve(Strings)) &&
      _generatedStructs.TryGetValue(member.Resolve(Strings), out var generatedType)) {
      var typeName = Intern(generatedType);
      return new DetachedSemanticMixinValue(typeName, typeName, Intern(member.Resolve(Strings)), typeName,
        typeName, Intern("public"), [typeName], [Intern("struct")], []);
    }
    object value = root switch {
      MixinExpressionRoot.This => CurrentType,
      MixinExpressionRoot.Target => _target,
      MixinExpressionRoot.Attribute => _attribute,
      MixinExpressionRoot.Argument => SelectArgument(member.Resolve(Strings)),
      _ => null
    };
    if (value is null) return root == MixinExpressionRoot.Argument
      ? NullMixinValue.Instance
      : Error("@" + root.ToString().ToLowerInvariant() + " is not available in this context");
    var name = member.Resolve(Strings);
    if (!string.IsNullOrEmpty(name) && root is not MixinExpressionRoot.Argument)
      value = SelectMember(value, name);
    return value is null ? NullMixinValue.Instance : new RoslynMixinValue(value, root);
  }

  private IParameterSymbol SelectArgument(string name) {
    if (string.IsNullOrEmpty(name)) return null;
    if (int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var index))
      return index >= 0 && index < _arguments.Count ? _arguments[index] : null;
    return _arguments.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
  }

  internal object SelectMember(object subject, string name) {
    if (subject is AttributeData attribute) {
      foreach (var item in attribute.NamedArguments)
        if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase)) return item.Value;
      if (attribute.AttributeConstructor is { } constructor)
        for (var i = 0; i < constructor.Parameters.Length && i < attribute.ConstructorArguments.Length; i++)
          if (string.Equals(constructor.Parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
            return attribute.ConstructorArguments[i];
      if (TryDefaultAttributeMember(attribute.AttributeClass, name, out var defaultValue)) return defaultValue;
      return null;
    }
    if (subject is IMethodSymbol method)
      return method.Parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    var type = TypeOf(subject);
    if (type is INamedTypeSymbol generic) {
      if (int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var argumentIndex) &&
        argumentIndex >= 0 && argumentIndex < generic.TypeArguments.Length)
        return generic.TypeArguments[argumentIndex];
      for (var i = 0; i < generic.TypeParameters.Length; i++)
        if (string.Equals(generic.TypeParameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
          return generic.TypeArguments[i];
    }
    for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType) {
      var result = current.GetMembers().FirstOrDefault(item =>
        string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
      if (result is not null) return result;
    }
    return null;
  }

  private bool TryDefaultAttributeMember(INamedTypeSymbol attributeType, string name, out object value) {
    value = null;
    for (var current = attributeType; current is not null; current = current.BaseType) {
      var member = current.GetMembers().FirstOrDefault(item =>
        string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
      if (member is IFieldSymbol { HasConstantValue: true } field) { value = field.ConstantValue; return true; }
      var initializer = member?.DeclaringSyntaxReferences.Select(item => item.GetSyntax()).Select(item => item switch {
        VariableDeclaratorSyntax variable => variable.Initializer?.Value,
        PropertyDeclarationSyntax property => property.Initializer?.Value,
        _ => null
      }).FirstOrDefault(item => item is not null);
      if (initializer is null) continue;
      var constant = _compilation.GetSemanticModel(initializer.SyntaxTree).GetConstantValue(initializer);
      if (constant.HasValue) { value = constant.Value; return true; }
    }
    return false;
  }

  internal ITypeSymbol ResolveType(string name) {
    var normalized = (name ?? "").Replace("global::", "");
    var direct = _compilation?.GetTypeByMetadataName(normalized);
    if (direct is not null) return direct;
    var simple = normalized.Split('.', '+').LastOrDefault() ?? normalized;
    return _compilation?.GetSymbolsWithName(simple, SymbolFilter.Type).OfType<INamedTypeSymbol>()
      .FirstOrDefault(item => item.ToDisplayString().Replace("global::", "") == normalized);
  }

  private static string JoinStructParts(string prefix, IReadOnlyList<string> parts) {
    if (string.IsNullOrWhiteSpace(prefix)) return string.Join(", ", parts);
    return parts.Count == 0 ? prefix : prefix + ", " + string.Join(", ", parts);
  }

  private static IEnumerable<IMethodSymbol> MethodsInHierarchy(INamedTypeSymbol type, string name) {
    for (var current = type; current is not null; current = current.BaseType)
      foreach (var method in current.GetMembers(name ?? "").OfType<IMethodSymbol>()) yield return method;
  }

  private static string CallableReference(IMethodSymbol method) =>
    method.ContainingType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." + method.Name;

  private static bool SameSignature(IMethodSymbol first, IMethodSymbol second) {
    if (first is null || second is null || first.Parameters.Length != second.Parameters.Length ||
      first.RefKind != second.RefKind) return false;
    for (var i = 0; i < first.Parameters.Length; i++) if (
      first.Parameters[i].RefKind != second.Parameters[i].RefKind ||
      !SymbolEqualityComparer.Default.Equals(first.Parameters[i].Type, second.Parameters[i].Type)) return false;
    return true;
  }

  private static bool TryWireParameters(IMethodSymbol from, IMethodSymbol to, out string arguments) {
    arguments = null;
    if (from is null || to is null || to.Parameters.Length > from.Parameters.Length) return false;
    var result = new string[to.Parameters.Length];
    for (var i = 0; i < to.Parameters.Length; i++) {
      if (!SymbolEqualityComparer.Default.Equals(from.Parameters[i].Type, to.Parameters[i].Type) ||
        from.Parameters[i].RefKind != to.Parameters[i].RefKind) return false;
      result[i] = (from.Parameters[i].RefKind switch {
        RefKind.Ref => "ref ", RefKind.Out => "out ", RefKind.In => "in ", _ => ""
      }) + GeneratorAnalysis.EscapeIdentifier(from.Parameters[i].Name);
    }
    arguments = string.Join(", ", result);
    return true;
  }

  internal static ITypeSymbol TypeOf(object value) => value switch {
    ITypeSymbol type => type, IMethodSymbol method => method.ReturnType,
    IPropertySymbol property => property.Type, IFieldSymbol field => field.Type,
    IParameterSymbol parameter => parameter.Type, IEventSymbol @event => @event.Type,
    AttributeData attribute => attribute.AttributeClass,
    TypedConstant { Kind: TypedConstantKind.Type, Value: ITypeSymbol representedType } => representedType,
    TypedConstant constant => constant.Type, _ => null
  };

  internal static string ComparableText(object value) {
    if (value is TypedConstant constant) value = constant.Value;
    return value switch {
      null => "null", bool boolean => boolean ? "true" : "false",
      ITypeSymbol type => type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat),
      ISymbol symbol => symbol.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat),
      AttributeData attribute => attribute.AttributeClass?.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat),
      _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };
  }

  internal static MixinTargetDescriptor ParseMixinTarget(string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null) {
    var value = target?.Trim() ?? "";
    if (targetDefinitions is not null && targetDefinitions.TryGetValue(value, out var defined))
      return ParseMixinTarget(defined, targetDefinitions);
    var isStatic = false; var isPublic = false;
    while (value.Length != 0 && (value[0] == '*' || value[0] == '^')) {
      if (value[0] == '*') isStatic = true; else isPublic = true;
      value = value.Substring(1);
    }
    string name; string delegateType = null;
    if (value.StartsWith("~", StringComparison.Ordinal)) {
      delegateType = value.Substring(1);
      var normalized = delegateType.Replace("global::", "");
      name = normalized.Split('.', '+').LastOrDefault() ?? normalized;
    } else {
      var separator = value.IndexOf(':');
      name = separator < 0 ? value : value.Substring(0, separator);
      if (separator >= 0) delegateType = value.Substring(separator + 1);
    }
    name = name switch { "$Init" => "Awake", "$Dispose" => "OnDestroy",
      _ when name.StartsWith("$", StringComparison.Ordinal) => name.Substring(1), _ => name };
    return new(name, isStatic, isPublic, delegateType);
  }
}

/// <summary>Immutable handle to a Roslyn semantic value; all services come from ExecutionContext.</summary>
internal sealed record RoslynMixinValue(object Value, MixinExpressionRoot Root = MixinExpressionRoot.Null) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) => Value switch {
    null => false, bool boolean => boolean,
    TypedConstant constant => constant.Kind != TypedConstantKind.Error && !constant.IsNull && constant.Value is not false,
    _ => true
  };
  public MixinString Render(ExecutionContext context) {
    if (Root is MixinExpressionRoot.This or MixinExpressionRoot.Target && Value is INamedTypeSymbol)
      return context.Intern("this");
    if (Value is IParameterSymbol parameter) return context.Intern(GeneratorAnalysis.EscapeIdentifier(parameter.Name));
    if (Value is IMethodSymbol method) return context.Intern(method.IsStatic
      ? method.ContainingType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." + method.Name
      : "this." + GeneratorAnalysis.EscapeIdentifier(method.Name));
    if (Value is TypedConstant constant) return context.Intern(RenderConstant(constant));
    return context.Intern(RoslynMixinContext.ComparableText(Value));
  }
  private static string RenderConstant(TypedConstant constant) {
    if (constant.IsNull || constant.Kind == TypedConstantKind.Error) return "null";
    if (constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type)
      return "typeof(" + type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + ")";
    return constant.Value switch {
      string text => SymbolDisplay.FormatLiteral(text, true),
      char character => SymbolDisplay.FormatLiteral(character, true),
      bool boolean => boolean ? "true" : "false",
      _ => Convert.ToString(constant.Value, CultureInfo.InvariantCulture)
    };
  }
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(RoslynMixinValue)); builder.Append(Render(context).Resolve(context.Strings));
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) =>
    context is RoslynMixinContext roslyn && roslyn.SelectMember(Value, member.Resolve(context.Strings)) is { } selected
      ? new RoslynMixinValue(selected) : NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => Unlink(context, true);
  internal object Unlink(ExecutionContext context, bool includeMembers) => Value switch {
    TypedConstant or ISymbol or AttributeData => Detach(context, includeMembers), _ => Value
  };

  private DetachedSemanticData Detach(ExecutionContext context, bool includeMembers) {
    var type = RoslynMixinContext.TypeOf(Value);
    var symbol = Value as ISymbol ?? type;
    var members = new Dictionary<string, DetachedSemanticData>(StringComparer.OrdinalIgnoreCase);
    if (includeMembers && type is INamedTypeSymbol named) {
      for (var i = 0; i < named.TypeArguments.Length; i++) {
        var item = DetachType(named.TypeArguments[i]);
        members[i.ToString(CultureInfo.InvariantCulture)] = item;
        if (i < named.TypeParameters.Length) members[named.TypeParameters[i].Name] = item;
      }
      foreach (var member in named.GetMembers()) if (!members.ContainsKey(member.Name))
        members[member.Name] = DetachSymbol(member);
    }
    var unwrapped = Value is TypedConstant constant
      ? constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol constantType
        ? constantType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "")
        : Convert.ToString(constant.Value, CultureInfo.InvariantCulture)
      : Render(context).Resolve(context.Strings);
    return new DetachedSemanticData(Render(context).Resolve(context.Strings), unwrapped,
      (Value as ISymbol)?.Name ?? (Value as AttributeData)?.AttributeClass?.Name ??
        RoslynMixinContext.ComparableText(Value),
      type is null ? "" : type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", ""),
      type is null ? "" : type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
        .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)),
      symbol?.DeclaredAccessibility.ToString().ToLowerInvariant() ?? "",
      Assignable(type).Distinct(StringComparer.Ordinal).ToArray(),
      RoslynMixinContext.SemanticTraits(Value), members);

    DetachedSemanticData DetachSymbol(ISymbol item) => new(
      item.Name, item.Name, item.Name,
      RoslynMixinContext.TypeOf(item)?.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "") ?? "",
      RoslynMixinContext.TypeOf(item)?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "",
      item.DeclaredAccessibility.ToString().ToLowerInvariant(),
      Assignable(RoslynMixinContext.TypeOf(item)).Distinct(StringComparer.Ordinal).ToArray(),
      RoslynMixinContext.SemanticTraits(item),
      new Dictionary<string, DetachedSemanticData>(StringComparer.OrdinalIgnoreCase));
    DetachedSemanticData DetachType(ITypeSymbol item) => new(
      item.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", ""),
      item.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", ""), item.Name,
      item.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", ""),
      item.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
      item.DeclaredAccessibility.ToString().ToLowerInvariant(),
      Assignable(item).Distinct(StringComparer.Ordinal).ToArray(),
      RoslynMixinContext.SemanticTraits(item),
      new Dictionary<string, DetachedSemanticData>(StringComparer.OrdinalIgnoreCase));
    IEnumerable<string> Assignable(ITypeSymbol item) {
      if (item is null) yield break;
      yield return item.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "");
      if (item is not INamedTypeSymbol namedItem) yield break;
      for (var current = namedItem.BaseType; current is not null; current = current.BaseType)
        yield return current.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "");
      foreach (var contract in namedItem.AllInterfaces)
        yield return contract.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat).Replace("global::", "");
    }
  }
  public bool Equals(IMixinValue other) => other is RoslynMixinValue value && (
    SymbolEqualityComparer.Default.Equals(Value as ISymbol, value.Value as ISymbol) ||
    Value is not ISymbol && Equals(Value, value.Value)
  );
}
