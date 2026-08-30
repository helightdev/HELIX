using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HelixSourceGenerator.Language;

internal sealed record MixinTargetDescriptor(string Name, bool IsStatic, bool IsPublic, string DelegateType);

internal sealed class RoslynHostExpressionCache {
  private readonly Dictionary<AttributeData, RoslynValueCache> _attributeValues = new();
  private readonly Dictionary<ISymbol, RoslynValueCache> _targetValues =
    new(SymbolEqualityComparer.Default);
  private readonly Dictionary<INamedTypeSymbol, RoslynValueCache> _thisValues =
    new(SymbolEqualityComparer.Default);

  internal RoslynValueCache ForThis(INamedTypeSymbol type) {
    return Get(_thisValues, type);
  }

  internal RoslynValueCache ForTarget(ISymbol target) {
    return Get(_targetValues, target);
  }

  internal RoslynValueCache ForAttribute(AttributeData attribute) {
    return Get(_attributeValues, attribute);
  }

  private static RoslynValueCache Get<T>(IDictionary<T, RoslynValueCache> values, T key) {
    if (key is null) return new RoslynValueCache();
    if (values.TryGetValue(key, out var cached)) return cached;
    cached = new RoslynValueCache();
    values.Add(key, cached);
    return cached;
  }
}

internal sealed class RoslynValueCache {
  private readonly Dictionary<object, Dictionary<string, IMixinValue>> _derived =
    new(ReferenceObjectComparer.Instance);
  private readonly Dictionary<object, object> _fullSnapshots =
    new(ReferenceObjectComparer.Instance);
  private readonly Dictionary<string, IMixinValue> _roots = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<object, object> _shallowSnapshots =
    new(ReferenceObjectComparer.Instance);

  internal IMixinValue Root(string key, Func<IMixinValue> resolve) {
    if (_roots.TryGetValue(key, out var value)) {
      MixinProfiler.Increment("cache.root.hit");
      return value;
    }
    MixinProfiler.Increment("cache.root.miss");
    value = resolve() ?? NullMixinValue.Instance;
    _roots.Add(key, value);
    return value;
  }

  internal IMixinValue Derived(object subject, string member, Func<IMixinValue> resolve) {
    if (!_derived.TryGetValue(subject, out var members)) {
      members = new Dictionary<string, IMixinValue>(StringComparer.OrdinalIgnoreCase);
      _derived.Add(subject, members);
    }
    if (members.TryGetValue(member, out var value)) {
      MixinProfiler.Increment("cache.derived.hit");
      return value;
    }
    MixinProfiler.Increment("cache.derived.miss");
    value = resolve() ?? NullMixinValue.Instance;
    members.Add(member, value);
    return value;
  }

  internal object Snapshot(IMixinValue subject, bool includeMembers, Func<object> create) {
    var snapshots = includeMembers ? _fullSnapshots : _shallowSnapshots;
    if (snapshots.TryGetValue(subject, out var snapshot)) {
      MixinProfiler.Increment("cache.snapshot.hit");
      return snapshot;
    }
    MixinProfiler.Increment("cache.snapshot.miss");
    snapshot = create();
    snapshots.Add(subject, snapshot);
    return snapshot;
  }

  private sealed class ReferenceObjectComparer : IEqualityComparer<object> {
    internal static readonly ReferenceObjectComparer Instance = new();

    public new bool Equals(object x, object y) {
      return ReferenceEquals(x, y);
    }

    public int GetHashCode(object value) {
      return RuntimeHelpers.GetHashCode(value);
    }
  }
}

internal sealed record MixinPropStructValue(IReadOnlyList<PropDefinition> Props,
  PropStructModel Model, bool Augmenting
) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return true;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic("<prop struct>");
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(MixinPropStructValue));
    builder.Append(Props.Count);
    builder.Append(Augmenting);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return this;
  }

  public bool Equals(IMixinValue other) {
    return ReferenceEquals(this, other);
  }
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
  public bool IsTruthy(ExecutionContext context) {
    return true;
  }

  public MixinString Render(ExecutionContext context) {
    return Rendered;
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(DetachedSemanticMixinValue));
    builder.Append(Rendered.Resolve(context.Strings));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return Members.FirstOrDefault(item => item.Key == member).Value ?? NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return new DetachedSemanticData(
      Rendered.Resolve(context.Strings), Unwrapped.Resolve(context.Strings), Name.Resolve(context.Strings),
      TypeName.Resolve(context.Strings),
      FullName.Resolve(context.Strings), Visibility.Resolve(context.Strings),
      [.. AssignableTypes.Select(item => item.Resolve(context.Strings))],
      [.. Traits.Select(item => item.Resolve(context.Strings))],
      Members.ToDictionary(
        item => item.Key.Resolve(context.Strings),
        item => (DetachedSemanticData)item.Value.Unlink(context), StringComparer.OrdinalIgnoreCase
      )
    );
  }

  public bool Equals(IMixinValue other) {
    return other is DetachedSemanticMixinValue value && Equals(value);
  }

  internal static DetachedSemanticMixinValue Materialize(DetachedSemanticData value, MixinStringPool strings) {
    return new DetachedSemanticMixinValue(
      MixinString.Dynamic(value.Rendered), MixinString.Dynamic(value.Unwrapped),
      MixinString.Dynamic(value.Name), MixinString.Dynamic(value.TypeName),
      MixinString.Dynamic(value.FullName), MixinString.Dynamic(value.Visibility),
      [.. value.AssignableTypes.Select(MixinString.Dynamic)],
      [.. value.Traits.Select(MixinString.Dynamic)],
      [
        .. value.Members.Select(item => new KeyValuePair<MixinString, IMixinValue>(
            strings.Get(item.Key), Materialize(item.Value, strings)
          )
        )
      ]
    );
  }
}

/// <summary>Roslyn host services for an already lowered mixin program.</summary>
internal sealed class RoslynMixinContext : ExecutionContext {
  private readonly IReadOnlyList<IParameterSymbol> _arguments;
  private readonly AttributeData _attribute;
  private readonly RoslynValueCache _attributeValues;
  private readonly CSharpCompilation _compilation;
  private readonly Dictionary<string, string> _generatedStructs = new(StringComparer.Ordinal);
  private readonly MixinLibraryCatalog _libraries;
  private readonly MixinCompilation _mixinCompilation;
  private readonly MixinExpressionPreparedState _preparedExpressions;
  private readonly ISymbol _target;
  private readonly IReadOnlyDictionary<string, string> _targetDefinitions;
  private readonly RoslynValueCache _targetValues;
  private readonly RoslynValueCache _thisValues;
  private readonly Dictionary<object, RoslynValueCache> _valueOwners =
    new(RoslynValueOwnerComparer.Instance);

  internal RoslynMixinContext(
    INamedTypeSymbol thisType, ISymbol target, AttributeData attribute,
    IReadOnlyList<IParameterSymbol> arguments, CSharpCompilation compilation,
    INamedTypeSymbol implicitAttributeType = null,
    IReadOnlyDictionary<string, object> implicitValues = null,
    IReadOnlyDictionary<string, string> targetDefinitions = null,
    MixinExpressionPreparedState preparedExpressions = null, MixinLibraryCatalog libraries = null,
    RoslynHostExpressionCache hostValues = null, MixinCompilation mixinCompilation = null
  )
    : base(preparedExpressions?.StringPool.Fork() ?? new MixinStringPoolBuilder().Freeze()) {
    CurrentType = thisType;
    _target = target;
    _attribute = attribute;
    _arguments = arguments ?? [];
    _compilation = compilation;
    _targetDefinitions = targetDefinitions;
    _preparedExpressions = preparedExpressions;
    _libraries = libraries;
    _mixinCompilation = mixinCompilation;
    hostValues ??= new RoslynHostExpressionCache();
    _thisValues = hostValues.ForThis(thisType);
    _targetValues = hostValues.ForTarget(target);
    _attributeValues = hostValues.ForAttribute(attribute);
  }

  internal INamedTypeSymbol CurrentType { get; }

  internal IMixinValue Derive(RoslynMixinValue source, string key, Func<IMixinValue> resolve) {
    if (!_valueOwners.TryGetValue(source.Value, out var cache)) cache = _targetValues;
    var result = cache.Derived(source.Value, key, resolve);
    RegisterOwner(result, cache);
    return result;
  }

  internal IMixinValue SelectValue(RoslynMixinValue source, MixinString member) {
    var name = member.Resolve(Strings);
    return Derive(
      source, "#" + name, () => {
        var selected = SelectMember(source.Value, name);
        return selected is null ? NullMixinValue.Instance : new RoslynMixinValue(selected);
      }
    );
  }

  private IMixinValue CachedRoot(RoslynValueCache cache, string key, Func<IMixinValue> resolve) {
    var result = cache.Root(key, resolve);
    RegisterOwner(result, cache);
    return result;
  }

  private void RegisterOwner(IMixinValue value, RoslynValueCache cache) {
    switch (value) {
      case RoslynMixinValue roslyn:
        _valueOwners[roslyn.Value] = cache;
        break;
      case MixinTableValue table:
        foreach (var item in table.Entries) RegisterOwner(item.Value, cache);
        break;
      case DirectiveEffectMixinValue effect:
        RegisterOwner(effect.Value, cache);
        break;
    }
  }

  public override IMixinValue ResolveMixin(MixinString localName, IMixinValue operand) {
    var local = localName.Resolve(Strings);
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
    IMixinValue result = callable is null ? NullMixinValue.Instance : new LiteralMixinValue(Intern(callable));
    Locals.StoreIsolated(Intern(local), result);
    return result;
  }

  public override IMixinValue CreatePropStruct(IReadOnlyList<IMixinValue> arguments, IMixinValue operand) {
    var text = arguments.Select(item => item.Render(this).Resolve(Strings)).ToArray();
    return CreatePropStruct(text[0], text[1], [.. text.Skip(2)], operand);
  }

  public override IMixinValue AugmentPropStruct(MixinString local, IMixinValue operand) {
    return AugmentPropStruct(local.Resolve(Strings), operand);
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

  private IMixinValue CreatePropStruct(
    string structName, string local, IReadOnlyList<string> flags,
    IMixinValue operand
  ) {
    if (!SyntaxFacts.IsValidIdentifier(structName))
      return Error("prop struct name '" + structName + "' is not a valid identifier");
    var subject = Evaluate(operand) is RoslynMixinValue roslyn ? roslyn.Value : null;
    IReadOnlyList<PropDefinition> props = subject switch {
      IMethodSymbol { TypeParameters.Length: > 0 } generic => null,
      IMethodSymbol method => method.Parameters.Select(parameter => new PropDefinition(
          parameter,
          parameter.Type, parameter.Name, GeneratorAnalysis.Attribute(parameter, GeneratorStrings.Attributes.Prop),
          parameter.RefKind
        )
      ).ToArray(),
      INamedTypeSymbol type => GeneratorAnalysis.InstanceFields(type).Select(field => new PropDefinition(
          field,
          field.Type, field.Name, GeneratorAnalysis.Attribute(field, GeneratorStrings.Attributes.Prop)
        )
      ).ToArray(),
      _ => null
    };
    if (subject is IMethodSymbol { TypeParameters.Length: > 0 } methodError)
      return Error("generic method '" + methodError.Name + "' cannot be used as a prop struct target");
    if (props is null) return Error("PROP_STRUCT target must resolve to a method or named type");
    if (!PropStructApi.TryAnalyzeProps(props, out var model, out var diagnostic))
      return Error(diagnostic.GetMessage(CultureInfo.InvariantCulture));
    var handle = new MixinPropStructValue(props, model, false);
    Locals.StoreIsolated(Intern(local), handle);
    var generate = !flags.Any(item => item.Equals("noGenerate", StringComparison.OrdinalIgnoreCase));
    if (!generate) return handle;
    var datatype = flags.Any(item => item.Equals("datatype", StringComparison.OrdinalIgnoreCase));
    var escaped = GeneratorAnalysis.EscapeIdentifier(structName);
    var builder = new SharpStringBuilder();
    using (builder.Type("public struct " + escaped)) {
      foreach (var prop in props) {
        builder.Field(
          "public",
          prop.Type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat), GeneratorAnalysis.EscapeIdentifier(prop.Name)
        );
      }
      if (model.ParameterParts.Count > 0) {
        builder.BlankLine();
        using (builder.Method(
          "public" + (model.RequiresUnsafe ? " unsafe " : " ") + escaped,
          model.ParameterParts
        )) model.AppendAssignments(builder, "this");
      }
      if (datatype) {
        builder.BlankLine();
        if (!TryDatatypeConfiguration(
          [.. props.Select(item => item.Symbol)], false,
          out var configuration, out var error
        ))
          return Error(error);
        PropStructApi.AnalyzeDatatype(escaped, structName, props).AppendMember(builder, configuration);
      }
    }
    return new DirectiveEffectMixinValue(handle, Intern(builder.ToString()));
  }

  private bool TryDatatypeConfiguration(
    IReadOnlyList<ISymbol> properties, bool includeType,
    out IReadOnlyList<string> configuration, out string error
  ) {
    var result = new List<string>();
    var variables = new Dictionary<string, object>(StringComparer.Ordinal);
    foreach (var owner in includeType ? new ISymbol[] { CurrentType }.Concat(properties) : properties)
    foreach (var applied in owner.GetAttributes()
      .OrderBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)) {
      if (applied.AttributeClass?.ToDisplayString() == GeneratorStrings.Attributes.Structure) continue;
      foreach (var annotation in MixinLibraryApi.Annotations(applied.AttributeClass, _libraries)) {
        var arguments = owner is IParameterSymbol { ContainingSymbol: IMethodSymbol method }
          ? (IReadOnlyList<IParameterSymbol>)method.Parameters
          : [];
        var nested = new RoslynMixinContext(
          CurrentType, owner, applied, arguments, _compilation,
          targetDefinitions: _targetDefinitions, preparedExpressions: _preparedExpressions, libraries: _libraries,
          mixinCompilation: _mixinCompilation
        );
        MixinExpressionResult evaluated;
        if (_mixinCompilation is not null && applied.AttributeClass is { } attributeType &&
          _mixinCompilation.TryGetAnnotation(
            attributeType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat), out var compiled
          )) {
          var program = owner is INamedTypeSymbol ? compiled.TypeProgram : compiled.MemberProgram;
          var preludeResult = MixinExpressionVirtualMachine.Execute(
            program.Prelude, nested, variables, false
          );
          if (!preludeResult.Success) evaluated = preludeResult;
          else {
            var lateResult = MixinExpressionVirtualMachine.Execute(
              program.Late, nested, variables, true
            );
            evaluated = !lateResult.Success
              ? lateResult
              : new MixinExpressionResult(
                true, null, 0, [.. preludeResult.Outputs, .. lateResult.Outputs],
                [.. preludeResult.Logs, .. lateResult.Logs], lateResult.Variables,
                preludeResult.ExecutedOperations + lateResult.ExecutedOperations,
                preludeResult.ExecutionMilliseconds + lateResult.ExecutionMilliseconds
              );
          }
        } else {
          evaluated = MixinExpressionVirtualMachine.Execute(
            annotation.Prelude + annotation.Expression,
            nested, variables, _preparedExpressions
          );
        }
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
    object subject = null;
    string missingName = null;
    if (operand is RootMixinValue { Root: MixinExpressionRoot.This } root) {
      missingName = root.Member.Resolve(Strings);
      subject = string.IsNullOrEmpty(missingName) ? CurrentType : SelectMember(CurrentType, missingName);
    } else if (Evaluate(operand) is RoslynMixinValue roslyn) subject = roslyn.Value;
    if (subject is null && !string.IsNullOrEmpty(missingName)) {
      if (!SyntaxFacts.IsValidIdentifier(missingName))
        return Error("struct name '" + missingName + "' is not a valid identifier");
      var empty = new MixinPropStructValue([], PropStructModel.Empty, true);
      Locals.StoreIsolated(Intern(local), empty);
      _generatedStructs[missingName] = CurrentType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." +
        GeneratorAnalysis.EscapeIdentifier(missingName);
      return new DirectiveEffectMixinValue(
        empty, Intern("public struct " + GeneratorAnalysis.EscapeIdentifier(missingName) + " { }")
      );
    }
    if (subject is not INamedTypeSymbol { TypeKind: TypeKind.Struct } type)
      return Error("AUGMENT_STRUCT target must resolve to a struct");
    if (!GeneratorAnalysis.IsPartial(type)) return Error("struct '" + type.Name + "' must be partial to be augmented");
    var generateDatatype = SymbolEqualityComparer.Default.Equals(type, CurrentType) &&
      _attribute?.AttributeClass?.ToDisplayString() == GeneratorStrings.Attributes.Structure &&
      _attribute.ConstructorArguments.Length > 0 && _attribute.ConstructorArguments[0].Value is true;
    if (!PropStructApi.TryAnalyze(type, out var model, out var diagnostic, generateDatatype))
      return Error(diagnostic.GetMessage(CultureInfo.InvariantCulture));
    var props = GeneratorAnalysis.InstanceFields(type).Select(field => new PropDefinition(
        field, field.Type,
        field.Name, GeneratorAnalysis.Attribute(field, GeneratorStrings.Attributes.Prop)
      )
    ).ToArray();
    var handle = new MixinPropStructValue(props, model, true);
    Locals.StoreIsolated(Intern(local), handle);
    var builder = new SharpStringBuilder();
    var augmentingThis = SymbolEqualityComparer.Default.Equals(type, CurrentType);
    if (augmentingThis) {
      if (model.ParameterParts.Count > 0) {
        using (builder.Method(
          GeneratorAnalysis.AccessibilityText(type.DeclaredAccessibility) +
          (model.RequiresUnsafe ? " unsafe " : " ") + GeneratorAnalysis.EscapeIdentifier(type.Name),
          model.ParameterParts
        )) model.AppendAssignments(builder, "this");
      }
      model.Equality.AppendMembers(builder);
      if (model.Datatype is not null) {
        builder.BlankLine();
        if (!TryDatatypeConfiguration(
          [.. props.Select(item => item.Symbol)], true,
          out var configuration, out var configurationError
        )) return Error(configurationError);
        model.Datatype.AppendMember(builder, configuration);
      }
    } else {
      if (!SymbolEqualityComparer.Default.Equals(type.ContainingType, CurrentType))
        return Error("AUGMENT_STRUCT target must be a struct nested directly in the current type");
      using (builder.Type("partial struct " + GeneratorAnalysis.EscapeIdentifier(type.Name))) {
        if (model.ParameterParts.Count > 0) {
          using (builder.Method(
            GeneratorAnalysis.AccessibilityText(type.DeclaredAccessibility) +
            (model.RequiresUnsafe ? " unsafe " : " ") + GeneratorAnalysis.EscapeIdentifier(type.Name),
            model.ParameterParts
          )) model.AppendAssignments(builder, "this");
        }
        model.Equality.AppendMembers(builder);
      }
    }
    return new DirectiveEffectMixinValue(handle, Intern(builder.ToString()));
  }

  public override MixinString NameOf(IMixinValue value) {
    return value is RoslynMixinValue roslyn
      ? Intern(
        (roslyn.Value as ISymbol)?.Name ??
        (roslyn.Value as AttributeData)?.AttributeClass?.Name ?? ComparableText(roslyn.Value)
      )
      : value is DetachedSemanticMixinValue detached
        ? detached.Name
        : base.NameOf(value);
  }

  public override bool IsType(IMixinValue value, MixinString requested) {
    using var profile = MixinProfiler.Measure("roslyn.is_type");
    if (value is DetachedSemanticMixinValue detached) {
      var expected = requested.Resolve(Strings).Replace("global::", "");
      return detached.AssignableTypes.Any(item => {
          var candidate = item.Resolve(Strings).Replace("global::", "");
          return candidate == expected || candidate.Split('.', '+').LastOrDefault() == expected;
        }
      );
    }
    if (value is not RoslynMixinValue roslyn || TypeOf(roslyn.Value) is not INamedTypeSymbol type) return false;
    var name = requested.Resolve(Strings).Replace("global::", "");

    bool Match(ITypeSymbol candidate) {
      return candidate is not null && (
        candidate.Name == name || candidate.ToDisplayString() == name ||
        candidate.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal) == name
      );
    }

    if (Match(type)) return true;
    for (var current = type.BaseType; current is not null; current = current.BaseType)
      if (Match(current))
        return true;
    return type.AllInterfaces.Any(Match);
  }

  public override bool HasTrait(IMixinValue value, MixinString requested) {
    using var profile = MixinProfiler.Measure("roslyn.has_trait");
    var name = requested.Resolve(Strings);
    if (value is DetachedSemanticMixinValue detached)
      return detached.Traits.Any(item => string.Equals(item.Resolve(Strings), name, StringComparison.Ordinal));
    return value is RoslynMixinValue roslyn && SemanticTraits(roslyn.Value).Contains(name, StringComparer.Ordinal);
  }

  internal override object UnlinkSnapshot(IMixinValue value, bool includeMembers) {
    if (value is not RoslynMixinValue roslyn) return value.Unlink(this);
    if (!includeMembers) return roslyn.UnlinkShallow(this);
    if (!_valueOwners.TryGetValue(roslyn.Value, out var cache)) cache = _targetValues;
    return cache.Snapshot(roslyn, includeMembers, () => roslyn.Unlink(this, includeMembers));
  }

  internal override IMixinValue DetachValue(IMixinValue value, bool includeMembers = true) {
    value = Evaluate(value);
    if (value is RoslynMixinValue roslyn) {
      var detached = UnlinkSnapshot(roslyn, includeMembers);
      return detached is DetachedSemanticData semantic
        ? DetachedSemanticMixinValue.Materialize(semantic, Strings)
        : base.DetachValue(
          detached switch {
            IMixinValue typed => typed,
            null => NullMixinValue.Instance,
            bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
            string text => new LiteralMixinValue(MixinString.Dynamic(text)),
            _ => new ObjectMixinValue(detached)
          }, includeMembers
        );
    }
    return base.DetachValue(value, includeMembers);
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
          RefKind.Ref => "ref", RefKind.In => "in", RefKind.Out => "out", _ => "argument"
        }
      );
    }
    return result;
  }

  public override IMixinValue Unwrap(IMixinValue value) {
    using var profile = MixinProfiler.Measure("roslyn.unwrap");
    if (value is RoslynMixinValue { Value: TypedConstant constant }) {
      if (constant.IsNull || constant.Kind == TypedConstantKind.Error) return NullMixinValue.Instance;
      var raw = constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type
        ? type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal)
        : Convert.ToString(constant.Value, CultureInfo.InvariantCulture);
      return new LiteralMixinValue(Intern(raw));
    }
    if (value is DetachedSemanticMixinValue detached)
      return new LiteralMixinValue(detached.Unwrapped);
    return base.Unwrap(value);
  }

  public override IMixinValue Attributes(IMixinValue value, MixinString requested, bool exact, bool first) {
    using var profile = MixinProfiler.Measure("roslyn.attributes");
    if (value is not RoslynMixinValue roslyn) return first ? NullMixinValue.Instance : MixinTableValue.Empty;
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
        return new KeyValuePair<MixinString, IMixinValue>(
          Intern(index.ToString(CultureInfo.InvariantCulture)), result
        );
      }
    ).ToArray();
    return new MixinTableValue(entries);
  }

  private bool IsOrInherits(ITypeSymbol type, string expected) {
    if (TypeMatches(type, expected)) return true;
    if (type is not INamedTypeSymbol named) return false;
    for (var current = named.BaseType; current is not null; current = current.BaseType)
      if (TypeMatches(current, expected))
        return true;
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
    if (root == MixinExpressionRoot.This && !string.IsNullOrEmpty(member.Resolve(Strings)) &&
      _generatedStructs.TryGetValue(member.Resolve(Strings), out var generatedType)) {
      var typeName = Intern(generatedType);
      return new DetachedSemanticMixinValue(
        typeName, typeName, Intern(member.Resolve(Strings)), typeName,
        typeName, Intern("public"), [typeName], [Intern("struct")], []
      );
    }
    var name = member.Resolve(Strings);
    var cache = root switch {
      MixinExpressionRoot.This => _thisValues,
      MixinExpressionRoot.Attribute => _attributeValues,
      _ => _targetValues
    };
    var result = CachedRoot(
      cache, root + "#" + name, () => {
        object value = root switch {
          MixinExpressionRoot.This => CurrentType,
          MixinExpressionRoot.Target => _target,
          MixinExpressionRoot.Attribute => _attribute,
          MixinExpressionRoot.Argument => SelectArgument(name),
          _ => null
        };
        if (value is null) {
          return root == MixinExpressionRoot.Argument
            ? NullMixinValue.Instance
            : Error("@" + root.ToString().ToLowerInvariant() + " is not available in this context");
        }
        if (!string.IsNullOrEmpty(name) && root is not MixinExpressionRoot.Argument)
          value = SelectMember(value, name);
        return value is null ? NullMixinValue.Instance : new RoslynMixinValue(value, root);
      }
    );
    return result;
  }

  private IParameterSymbol SelectArgument(string name) {
    if (string.IsNullOrEmpty(name)) return null;
    if (int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var index))
      return index >= 0 && index < _arguments.Count ? _arguments[index] : null;
    return _arguments.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
  }

  internal object SelectMember(object subject, string name) {
    using var profile = MixinProfiler.Measure("roslyn.select_member");
    if (subject is AttributeData attribute) {
      foreach (var item in attribute.NamedArguments)
        if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
          return item.Value;
      if (attribute.AttributeConstructor is { } constructor) {
        for (var i = 0; i < constructor.Parameters.Length && i < attribute.ConstructorArguments.Length; i++) {
          if (string.Equals(constructor.Parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
            return attribute.ConstructorArguments[i];
        }
      }
      if (TryDefaultAttributeMember(attribute.AttributeClass, name, out var defaultValue)) return defaultValue;
      return null;
    }
    if (subject is IMethodSymbol method)
      return method.Parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)
      );
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
      var constant = _compilation.GetSemanticModel(initializer.SyntaxTree).GetConstantValue(initializer);
      if (constant.HasValue) {
        value = constant.Value;
        return true;
      }
    }
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
    for (var current = type; current is not null; current = current.BaseType)
      foreach (var method in current.GetMembers(name ?? "").OfType<IMethodSymbol>())
        yield return method;
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
        RefKind.Ref => "ref ", RefKind.Out => "out ", RefKind.In => "in ", _ => ""
      } + GeneratorAnalysis.EscapeIdentifier(from.Parameters[i].Name);
    }
    arguments = string.Join(", ", result);
    return true;
  }

  internal static ITypeSymbol TypeOf(object value) {
    return value switch {
      ITypeSymbol type => type, IMethodSymbol method => method.ReturnType,
      IPropertySymbol property => property.Type, IFieldSymbol field => field.Type,
      IParameterSymbol parameter => parameter.Type, IEventSymbol @event => @event.Type,
      AttributeData attribute => attribute.AttributeClass,
      TypedConstant { Kind: TypedConstantKind.Type, Value: ITypeSymbol representedType } => representedType,
      TypedConstant constant => constant.Type, _ => null
    };
  }

  internal static string ComparableText(object value) {
    using var profile = MixinProfiler.Measure("roslyn.comparable_text");
    if (value is TypedConstant constant) value = constant.Value;
    return value switch {
      null => "null", bool boolean => boolean ? "true" : "false",
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
      "$Init" => "Awake", "$Dispose" => "OnDestroy",
      _ when name.StartsWith("$", StringComparison.Ordinal) => name.Substring(1), _ => name
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

/// <summary>Immutable handle to a Roslyn semantic value; all services come from ExecutionContext.</summary>
internal sealed record RoslynMixinValue(object Value, MixinExpressionRoot Root = MixinExpressionRoot.Null)
  : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Value switch {
      null => false, bool boolean => boolean,
      TypedConstant constant => constant.Kind != TypedConstantKind.Error && !constant.IsNull &&
        constant.Value is not false,
      _ => true
    };
  }

  public MixinString Render(ExecutionContext context) {
    using var profile = MixinProfiler.Measure("roslyn.value.render");
    if (Root is MixinExpressionRoot.This or MixinExpressionRoot.Target && Value is INamedTypeSymbol)
      return ExecutionContext.Dynamic("this");
    if (Value is IParameterSymbol parameter)
      return ExecutionContext.Dynamic(GeneratorAnalysis.EscapeIdentifier(parameter.Name));
    if (Value is IMethodSymbol method) {
      return ExecutionContext.Dynamic(
        method.IsStatic
          ? method.ContainingType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat) + "." + method.Name
          : "this." + GeneratorAnalysis.EscapeIdentifier(method.Name)
      );
    }
    if (Value is TypedConstant constant) return ExecutionContext.Dynamic(RenderConstant(constant));
    return ExecutionContext.Dynamic(RoslynMixinContext.ComparableText(Value));
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(RoslynMixinValue));
    builder.Append(Render(context).Resolve(context.Strings));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return context is RoslynMixinContext roslyn ? roslyn.SelectValue(this, member) : NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Unlink(context, true);
  }

  public bool Equals(IMixinValue other) {
    return other is RoslynMixinValue value && (
      SymbolEqualityComparer.Default.Equals(Value as ISymbol, value.Value as ISymbol) ||
      Value is not ISymbol && Equals(Value, value.Value)
    );
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

  internal object Unlink(ExecutionContext context, bool includeMembers) {
    return Value switch {
      TypedConstant or ISymbol or AttributeData => Detach(context, includeMembers), _ => Value
    };
  }

  internal object UnlinkShallow(ExecutionContext context) {
    return Value switch {
      TypedConstant { Kind: TypedConstantKind.Type } => Render(context).Resolve(context.Strings),
      TypedConstant { Value: string or char } => Render(context).Resolve(context.Strings),
      TypedConstant constant => constant.Value,
      ITypeSymbol type => type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat),
      ISymbol or AttributeData => Render(context).Resolve(context.Strings),
      _ => Value
    };
  }

  private DetachedSemanticData Detach(ExecutionContext context, bool includeMembers) {
    using var profile = MixinProfiler.Measure(includeMembers
      ? "semantic.detach.full" : "semantic.detach.shallow");
    var type = RoslynMixinContext.TypeOf(Value);
    var symbol = Value as ISymbol ?? type;
    var members = new Dictionary<string, DetachedSemanticData>(StringComparer.OrdinalIgnoreCase);
    if (includeMembers && Value is AttributeData attribute) {
      foreach (var item in attribute.NamedArguments)
        members[item.Key] = DetachConstant(item.Value, item.Key);
      if (attribute.AttributeConstructor is { } constructor) {
        for (var index = 0; index < constructor.Parameters.Length && index < attribute.ConstructorArguments.Length;
          index++) {
          members[constructor.Parameters[index].Name] = DetachConstant(
            attribute.ConstructorArguments[index], constructor.Parameters[index].Name
          );
        }
      }
    }
    if (includeMembers && type is INamedTypeSymbol named) {
      for (var i = 0; i < named.TypeArguments.Length; i++) {
        var item = DetachType(named.TypeArguments[i]);
        members[i.ToString(CultureInfo.InvariantCulture)] = item;
        if (i < named.TypeParameters.Length) members[named.TypeParameters[i].Name] = item;
      }
      foreach (var member in named.GetMembers()) {
        if (!members.ContainsKey(member.Name))
          members[member.Name] = DetachSymbol(member);
      }
    }
    var rendered = Render(context).Resolve(context.Strings);
    var unwrapped = Value is TypedConstant constant
      ? constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol constantType
        ? constantType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal)
        : Convert.ToString(constant.Value, CultureInfo.InvariantCulture)
      : rendered;
    return new DetachedSemanticData(
      rendered, unwrapped,
      (Value as ISymbol)?.Name ?? (Value as AttributeData)?.AttributeClass?.Name ??
      RoslynMixinContext.ComparableText(Value),
      type is null ? "" : type.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal),
      type is null
        ? ""
        : type.ToDisplayString(
          SymbolDisplayFormat.MinimallyQualifiedFormat
            .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
        ),
      symbol?.DeclaredAccessibility.ToString().ToLowerInvariant() ?? "",
      [.. Assignable(type).Distinct(StringComparer.Ordinal)],
      RoslynMixinContext.SemanticTraits(Value), members
    );

    DetachedSemanticData DetachConstant(TypedConstant item, string name) {
      using var nestedProfile = MixinProfiler.Measure("semantic.detach.constant");
      var constantType = item.Type;
      var rendered = RenderConstant(item);
      return new DetachedSemanticData(
        rendered,
        item.Kind == TypedConstantKind.Type && item.Value is ITypeSymbol valueType
          ? valueType.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal)
          : Convert.ToString(item.Value, CultureInfo.InvariantCulture),
        name, constantType?.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal) ?? "",
        constantType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "",
        "", [.. Assignable(constantType).Distinct(StringComparer.Ordinal)],
        RoslynMixinContext.SemanticTraits(item),
        new Dictionary<string, DetachedSemanticData>(StringComparer.OrdinalIgnoreCase)
      );
    }

    DetachedSemanticData DetachSymbol(ISymbol item) {
      using var nestedProfile = MixinProfiler.Measure("semantic.detach.symbol");
      var itemType = RoslynMixinContext.TypeOf(item);
      return new DetachedSemanticData(
        item.Name, item.Name, item.Name,
        itemType?.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal) ?? "",
        itemType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "",
        item.DeclaredAccessibility.ToString().ToLowerInvariant(),
        [.. Assignable(itemType).Distinct(StringComparer.Ordinal)],
        RoslynMixinContext.SemanticTraits(item),
        new Dictionary<string, DetachedSemanticData>(StringComparer.OrdinalIgnoreCase)
      );
    }

    DetachedSemanticData DetachType(ITypeSymbol item) {
      using var nestedProfile = MixinProfiler.Measure("semantic.detach.type");
      var typeName = item.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal);
      return new DetachedSemanticData(
        typeName, typeName, item.Name, typeName,
        item.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
        item.DeclaredAccessibility.ToString().ToLowerInvariant(),
        [.. Assignable(item).Distinct(StringComparer.Ordinal)],
        RoslynMixinContext.SemanticTraits(item),
        new Dictionary<string, DetachedSemanticData>(StringComparer.OrdinalIgnoreCase)
      );
    }

    IEnumerable<string> Assignable(ITypeSymbol item) {
      using var nestedProfile = MixinProfiler.Measure("semantic.detach.assignable");
      if (item is null) yield break;
      yield return item.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal);
      if (item is not INamedTypeSymbol namedItem) yield break;
      for (var current = namedItem.BaseType; current is not null; current = current.BaseType)
        yield return current.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal);
      foreach (var contract in namedItem.AllInterfaces)
        yield return contract.ToDisplayString(GeneratorAnalysis.TypeDisplayFormatWithoutGlobal);
    }
  }
}
