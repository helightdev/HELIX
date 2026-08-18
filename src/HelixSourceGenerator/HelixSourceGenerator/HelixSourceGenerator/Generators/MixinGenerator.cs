using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Mixins;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class MixinGenerator : IIncrementalGenerator {
  private static readonly ConditionalWeakTable<INamedTypeSymbol, Dictionary<string, string>>
    TargetDefinitionCache = new();

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var preparedExpressions =
      context.CompilationProvider.Select(static (compilation, _) => CollectPreparedExpressions(compilation)
      );

    context.RegisterSourceOutput(
      preparedExpressions,
      static (spc, prepared) => ReportPreparedExpressionDiagnostics(spc, prepared)
    );

    foreach (var attribute in MixinGeneratorCandidates.AttributeMetadataNames) {
      var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
        attribute,
        static (node, _) => node is TypeDeclarationSyntax {
          RawKind: (int)SyntaxKind.ClassDeclaration or (int)SyntaxKind.RecordDeclaration
        },
        static (ctx, _) => GetTarget(ctx)
      ).Where(static target => target is not null);
      context.RegisterSourceOutput(
        targets.Combine(preparedExpressions),
        static (spc, input) => Generate(
          spc,
          input.Left,
          input.Right.State
        )
      );
    }
  }

  private static PreparedMixinExpressions CollectPreparedExpressions(
    Compilation compilation
  ) {
    var result = new List<PreparedMixinExpression>();
    var assemblies = compilation.SourceModule.ReferencedAssemblySymbols
      .OrderBy(item => item.Identity.Name, StringComparer.Ordinal)
      .Concat(new[] { compilation.Assembly });
    var interpreter = new MixinExpressionInterpreter();
    foreach (var assembly in assemblies) {
      foreach (var attribute in assembly.GetAttributes().Where(item =>
        IsAttribute(item, Attributes.MixinPrepareGlobal)
      )) {
        var expression = attribute.ConstructorArguments.Length == 1
          ? attribute.ConstructorArguments[0].Value as string
          : null;
        var validation = interpreter.ValidateSyntax(expression);
        result.Add(
          new PreparedMixinExpression(
            assembly.Identity.Name,
            expression ?? "",
            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
            validation
          )
        );
      }
    }
    var state = interpreter.PrepareGlobals(
      result.Where(item => item.Validation.Success).Select(item => item.Expression)
    );
    return new PreparedMixinExpressions(result, state);
  }

  private static void ReportPreparedExpressionDiagnostics(
    SourceProductionContext context,
    PreparedMixinExpressions preparedExpressions
  ) {
    foreach (var prepared in preparedExpressions.Items.Where(item => !item.Validation.Success)) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          InvalidPreparedExpression,
          prepared.Location,
          prepared.Provider,
          prepared.Validation.ErrorLine.ToString(CultureInfo.InvariantCulture),
          prepared.Validation.Error
        )
      );
    }
    var valid = preparedExpressions.Items.Where(item => item.Validation.Success).ToArray();
    foreach (var log in preparedExpressions.State.Logs) {
      if (log.ProgramIndex < 0 || log.ProgramIndex >= valid.Length) continue;
      context.ReportDiagnostic(
        Diagnostic.Create(
          ExpressionLog,
          valid[log.ProgramIndex].Location,
          log.Text
        )
      );
    }
  }

  private static MixinTarget GetTarget(GeneratorAttributeSyntaxContext context) {
    if (context.TargetSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } type)
      return null;
    var matchedCandidate = context.Attributes.FirstOrDefault()?.AttributeClass?.ToDisplayString();
    var canonicalCandidate = MixinGeneratorCandidates.AttributeMetadataNames.FirstOrDefault(candidate =>
      type.GetAttributes().Any(attribute => IsAttribute(attribute, candidate))
    );
    if (matchedCandidate != canonicalCandidate) return null;
    return context.SemanticModel.Compilation is CSharpCompilation compilation
      ? new MixinTarget(type, compilation)
      : null;
  }

  private static bool HasComponentStereotype(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.BaseType) {
      foreach (var attribute in current.GetAttributes()) {
        for (var attributeType = attribute.AttributeClass;
          attributeType is not null;
          attributeType = attributeType.BaseType)
          if (attributeType.ToDisplayString() == Attributes.Component)
            return true;
      }
    }
    return false;
  }

  private static IReadOnlyDictionary<string, string> TargetDefinitions(INamedTypeSymbol type) {
    return TargetDefinitionCache.GetValue(type, CreateTargetDefinitions);
  }

  private static Dictionary<string, string> CreateTargetDefinitions(INamedTypeSymbol type) {
    var definitions = new Dictionary<string, string>(StringComparer.Ordinal) {
      ["$Init"] = HasComponentStereotype(type) ? "^LoadComponent" : "Awake",
      ["$Dispose"] = HasComponentStereotype(type) ? "^UnloadComponent" : "OnDestroy"
    };
    var hierarchy = new Stack<INamedTypeSymbol>();
    for (var current = type; current is not null; current = current.BaseType) hierarchy.Push(current);
    while (hierarchy.Count != 0) {
      foreach (var applied in OrderedAttributes(hierarchy.Pop())) {
        if (IsAttribute(applied, Attributes.MixinDefineTarget)) ApplyTargetDefinition(applied, definitions);
        if (applied.AttributeClass is null) continue;
        var attributeHierarchy = new Stack<INamedTypeSymbol>();
        for (var current = applied.AttributeClass;
          current is not null;
          current = current.BaseType) attributeHierarchy.Push(current);
        while (attributeHierarchy.Count != 0) {
          foreach (var definition in OrderedAttributes(attributeHierarchy.Pop())
            .Where(item => IsAttribute(item, Attributes.MixinDefineTarget)))
            ApplyTargetDefinition(definition, definitions);
        }
      }
    }
    return definitions;
  }

  private static void ApplyTargetDefinition(
    AttributeData attribute,
    IDictionary<string, string> definitions
  ) {
    if (attribute.ConstructorArguments.Length < 2 ||
      attribute.ConstructorArguments[0].Value is not string key ||
      attribute.ConstructorArguments[1].Value is not string target ||
      string.IsNullOrEmpty(key)) return;
    definitions["$" + key.TrimStart('$')] = target;
  }

  private static void Generate(
    SourceProductionContext context,
    MixinTarget candidate,
    MixinExpressionPreparedState preparedExpressions
  ) {
    var target = candidate.Type;
    var location = LocationOf(target);
    if (!IsPartial(target)) {
      context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, target.Name));
      return;
    }

    var containing = FirstNonPartialContainingType(target);
    if (containing is not null) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          ContainingTypeMustBePartial, location, target.Name, containing.Name
        )
      );
      return;
    }

    var interfaces = CollectMixinInterfaces(target);
    var implicitAttributes = new List<ImplicitMixinAttribute>();
    var implicitInterfaces = new List<INamedTypeSymbol>();
    ResolveMixinRequirements(
      context, target, interfaces, implicitAttributes, implicitInterfaces
    );
    var contributions = new List<MixinContribution>();
    var attributeExpressionOutputs = new List<MixinExpressionOutput>();
    foreach (var implicitInterface in implicitInterfaces) {
      attributeExpressionOutputs.Add(
        new MixinExpressionOutput(
          MixinExpressionOutputTarget.Implements,
          implicitInterface.ToDisplayString(TypeDisplayFormat)
        )
      );
    }
    var expressionVariables = new Dictionary<string, object>(StringComparer.Ordinal);
    CollectInterfaceContributions(
      context, target, interfaces, candidate.Compilation, preparedExpressions, expressionVariables,
      attributeExpressionOutputs, contributions
    );
    CollectAttributeContributions(
      context, target, candidate.Compilation, preparedExpressions, expressionVariables,
      attributeExpressionOutputs, implicitAttributes, contributions
    );
    var methods = BuildMethods(
      context, target, contributions, candidate.Compilation
    );
    var outputs = new ExpressionOutputs();
    foreach (var attribute in implicitAttributes) {
      outputs.AddAnnotation(attribute.Type.ToDisplayString(TypeDisplayFormat));
    }
    foreach (var contribution in contributions) {
      if (contribution.ExpressionResult is null) continue;
      foreach (var output in contribution.ExpressionResult.Outputs) outputs.Add(output);
    }
    foreach (var output in attributeExpressionOutputs) outputs.Add(output);

    if (methods.Count == 0 && !outputs.Any) return;
    var wrapper = WrapType(
      target, "mixins", outputs.Usings,
      baseType: outputs.Implements.Count == 0 ? null : string.Join(", ", outputs.Implements),
      typeAttributes: outputs.Annotations
    );
    var source = wrapper.Build(
      builder => {
        for (var index = 0; index < methods.Count; index++) {
          AppendMethod(builder, methods[index]);
          if (index != methods.Count - 1) builder.BlankLine();
        }
        foreach (var output in outputs.Class) {
          if (methods.Count != 0) builder.BlankLine();
          builder.AppendCode(output);
        }
      }, builder => {
        foreach (var output in outputs.File) builder.AppendCode(output);
      }, true
    );
    context.AddSource(wrapper.HintName, source);
  }

  private enum ContributionKind { Local, Interface, Attribute }

  private sealed class ExpressionOutputs {
    private List<string> _annotations, _class, _file, _implements, _usings;
    private HashSet<string> _annotationSet, _implementSet, _usingSet;
    internal IReadOnlyList<string> Annotations => _annotations ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> Class => _class ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> File => _file ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> Implements => _implements ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal IReadOnlyList<string> Usings => _usings ?? (IReadOnlyList<string>)Array.Empty<string>();
    internal bool Any { get; private set; }

    internal void AddAnnotation(string text) => AddUnique(
      ref _annotations, ref _annotationSet, text
    );

    internal void Add(MixinExpressionOutput output) {
      if (string.IsNullOrEmpty(output.Text)) return;
      Any = true;
      var text = output.Text.Trim();
      switch (output.Target) {
        case MixinExpressionOutputTarget.Implements:
          AddUnique(ref _implements, ref _implementSet, text); break;
        case MixinExpressionOutputTarget.Annotation:
          AddUnique(ref _annotations, ref _annotationSet, text); break;
        case MixinExpressionOutputTarget.Using:
          text = text.TrimEnd(';');
          AddUnique(ref _usings, ref _usingSet, text.Length == 0 ? "" : "using " + text + ";");
          break;
        case MixinExpressionOutputTarget.Class: (_class ??= new List<string>()).Add(output.Text); break;
        case MixinExpressionOutputTarget.File: (_file ??= new List<string>()).Add(output.Text); break;
      }
    }

    private static void AddUnique(ref List<string> values, ref HashSet<string> seen, string value) {
      if (value.Length == 0 || !(seen ??= new HashSet<string>(StringComparer.Ordinal)).Add(value)) return;
      (values ??= new List<string>()).Add(value);
    }
  }

  private sealed class MixinContribution {
    internal MixinContribution(
      string target,
      int order,
      ContributionKind kind,
      int sequence,
      IReadOnlyDictionary<string, string> targetDefinitions,
      MixinExpressionResult expressionResult
    ) {
      var targetSyntax = RoslynMixinExpressionContext.ParseMixinTarget(target, targetDefinitions);
      EmittedTarget = targetSyntax.Name;
      IsStaticTarget = targetSyntax.IsStatic;
      IsPublicTarget = targetSyntax.IsPublic;
      DelegateTarget = targetSyntax.DelegateType;
      Order = order;
      Kind = kind;
      Sequence = sequence;
      ExpressionResult = expressionResult;
    }

    internal string EmittedTarget { get; }
    internal bool IsStaticTarget { get; }
    internal bool IsPublicTarget { get; }
    internal string DelegateTarget { get; }
    internal int Order { get; }
    internal ContributionKind Kind { get; }
    internal int Sequence { get; }
    internal MixinExpressionResult ExpressionResult { get; }
  }

  private sealed record MixinTargetParameter(
    ITypeSymbol TypeSymbol, string Name, RefKind RefKind, bool IsParams
  ) {
    internal string Type => TypeSymbol.ToDisplayString(TypeDisplayFormat);
    internal string Declaration => (IsParams ? "params " : RefPrefix(RefKind)) + Type + " " + EscapeIdentifier(Name);
    internal string Argument => RefPrefix(RefKind) + EscapeIdentifier(Name);

    internal static MixinTargetParameter FromSymbol(IParameterSymbol parameter) {
      return new MixinTargetParameter(parameter.Type, parameter.Name, parameter.RefKind, parameter.IsParams);
    }
  }

  private sealed record GeneratedMethod(
    string Declaration,
    IReadOnlyList<string> TypeParameters,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<MixinTargetParameter> Parameters,
    string ReturnType,
    bool CallBase,
    string Name,
    IReadOnlyList<MixinContribution> Contributions
  );

  private sealed class AttributeExpressionTarget {
    internal AttributeExpressionTarget(
      string target,
      int order,
      IReadOnlyDictionary<string, string> targetDefinitions
    ) {
      var targetSyntax = RoslynMixinExpressionContext.ParseMixinTarget(target, targetDefinitions);
      Target = target;
      EmittedTarget = targetSyntax.Name;
      Order = order;
    }

    internal string Target { get; }
    internal string EmittedTarget { get; }
    internal int Order { get; }
    internal List<MixinExpressionOutput> Outputs { get; set; }
  }

  private sealed class ImplicitMixinAttribute {
    private ImplicitMixinAttribute(
      INamedTypeSymbol type,
      IReadOnlyDictionary<string, ImplicitMixinValue> values
    ) {
      Type = type;
      Values = values;
    }

    internal INamedTypeSymbol Type { get; }
    internal IReadOnlyDictionary<string, ImplicitMixinValue> Values { get; }

    internal static bool TryCreate(
      INamedTypeSymbol type,
      out ImplicitMixinAttribute attribute,
      out string failure
    ) {
      attribute = null;
      failure = null;
      if (type.IsAbstract || type.IsGenericType) {
        failure = "implicit attribute '" + type.ToDisplayString() +
          "' must be a non-abstract, non-generic attribute type";
        return false;
      }
      var usage = Attribute(type, "System.AttributeUsageAttribute");
      if (usage is not null &&
        (usage.ConstructorArguments.Length == 0 ||
          !TryConvertToInt32(usage.ConstructorArguments[0].Value, out var targets) ||
          (targets & 4) == 0)) {
        failure = "implicit attribute '" + type.ToDisplayString() +
          "' does not allow class-level application";
        return false;
      }
      var constructor = type.InstanceConstructors
        .Where(item => item.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal &&
          item.Parameters.All(parameter => parameter.HasExplicitDefaultValue)
        )
        .OrderBy(item => item.Parameters.Length)
        .FirstOrDefault();
      if (constructor is null) {
        failure = "implicit attribute '" + type.ToDisplayString() +
          "' has no accessible constructor whose parameters all have defaults";
        return false;
      }
      var values = new Dictionary<string, ImplicitMixinValue>(StringComparer.OrdinalIgnoreCase);
      foreach (var parameter in constructor.Parameters) {
        values[parameter.Name] = new ImplicitMixinValue(
          parameter.Type, parameter.ExplicitDefaultValue
        );
      }
      attribute = new ImplicitMixinAttribute(type, values);
      return true;
    }
  }

  private sealed record MixinTarget(INamedTypeSymbol Type, CSharpCompilation Compilation);

  private sealed record PreparedMixinExpression(
    string Provider,
    string Expression,
    Location Location,
    MixinExpressionValidationResult Validation
  );

  private sealed record PreparedMixinExpressions(
    IReadOnlyList<PreparedMixinExpression> Items, MixinExpressionPreparedState State
  );

  private static List<INamedTypeSymbol> CollectMixinInterfaces(INamedTypeSymbol target) {
    var result = new List<INamedTypeSymbol>();
    var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

    void Visit(INamedTypeSymbol item) {
      if (!visited.Add(item)) return;
      if (IsMixinInterface(item)) result.Add(item);
      foreach (var inherited in item.Interfaces) Visit(inherited);
    }

    for (var current = target; current is not null; current = current.BaseType)
      foreach (var item in current.Interfaces)
        Visit(item);
    return result;
  }

  private static bool IsMixinInterface(INamedTypeSymbol type) {
    return type.ToDisplayString() == Types.Mixin ||
      Attribute(type, Attributes.Mixin) is not null ||
      type.AllInterfaces.Any(item => item.ToDisplayString() == Types.Mixin);
  }

  private static void ResolveMixinRequirements(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IList<INamedTypeSymbol> interfaces,
    ICollection<ImplicitMixinAttribute> implicitAttributes,
    ICollection<INamedTypeSymbol> implicitInterfaces
  ) {
    var interfaceSet = new HashSet<INamedTypeSymbol>(interfaces, SymbolEqualityComparer.Default);
    var attributeSet = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    foreach (var attribute in target.GetAttributes()) {
      if (attribute.AttributeClass is not null) attributeSet.Add(attribute.AttributeClass);
    }
    var providers = new Queue<INamedTypeSymbol>();
    foreach (var mixin in interfaces) providers.Enqueue(mixin);

    foreach (var attribute in target.GetAttributes()) {
      if (IsMixinYieldingAttribute(attribute.AttributeClass)) providers.Enqueue(attribute.AttributeClass);
    }
    foreach (var member in OrderedMembers(target)) {
      if (member.IsImplicitlyDeclared) continue;
      foreach (var attribute in member.GetAttributes()) {
        if (IsMixinYieldingAttribute(attribute.AttributeClass)) providers.Enqueue(attribute.AttributeClass);
      }
    }

    var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    while (providers.Count != 0) {
      var provider = providers.Dequeue();
      if (provider is null || !visited.Add(provider)) continue;
      foreach (var requirement in InheritedAttributes(
        provider, Attributes.RequireMixin, true
      )) {
        if (!TryReadRequirement(requirement, out var required, out var declareImplicit) ||
          required is null ||
          !(IsMixinInterface(required) || IsMixinYieldingAttribute(required))) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidRequiredMixin, LocationOf(target), provider.Name, target.Name,
              "the target must be an IMixin interface or a mixin-yielding attribute"
            )
          );
          continue;
        }

        var isInterface = required.TypeKind == TypeKind.Interface;
        var present = isInterface ? interfaceSet.Contains(required) : attributeSet.Contains(required);
        if (present) {
          providers.Enqueue(required);
          continue;
        }
        if (!declareImplicit) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              MissingRequiredMixin, LocationOf(target), provider.Name, target.Name,
              required.ToDisplayString()
            )
          );
          continue;
        }

        if (isInterface) {
          if (required.IsUnboundGenericType || ContainsTypeParameter(required)) {
            context.ReportDiagnostic(
              Diagnostic.Create(
                InvalidRequiredMixin, LocationOf(target), provider.Name, target.Name,
                "implicit interface '" + required.ToDisplayString() + "' must be a closed type"
              )
            );
            continue;
          }
          interfaceSet.Add(required);
          interfaces.Add(required);
          implicitInterfaces.Add(required);
        } else {
          if (!ImplicitMixinAttribute.TryCreate(required, out var implicitAttribute, out var failure)) {
            context.ReportDiagnostic(
              Diagnostic.Create(
                InvalidRequiredMixin, LocationOf(target), provider.Name, target.Name, failure
              )
            );
            continue;
          }
          attributeSet.Add(required);
          implicitAttributes.Add(implicitAttribute);
        }
        providers.Enqueue(required);
      }
    }
  }

  private static bool IsMixinYieldingAttribute(INamedTypeSymbol type) {
    return type is { TypeKind: TypeKind.Class } &&
      InheritsFromSystemAttribute(type) &&
      InheritedAttributes(type, Attributes.MixinExpression, false).Count != 0;
  }

  private static bool InheritsFromSystemAttribute(INamedTypeSymbol type) {
    for (var current = type; current is not null; current = current.BaseType)
      if (current.ToDisplayString() == "System.Attribute")
        return true;
    return false;
  }

  private static bool ContainsTypeParameter(ITypeSymbol type) {
    return type.TypeKind == TypeKind.TypeParameter ||
      (type is INamedTypeSymbol named && named.TypeArguments.Any(ContainsTypeParameter)) ||
      (type is IArrayTypeSymbol array && ContainsTypeParameter(array.ElementType));
  }

  private static bool TryReadRequirement(
    AttributeData requirement,
    out INamedTypeSymbol target,
    out bool declareImplicit
  ) {
    target = null;
    declareImplicit = false;
    if (requirement.ConstructorArguments.Length == 0) return false;
    target = requirement.ConstructorArguments[0].Value as INamedTypeSymbol;
    if (requirement.ConstructorArguments.Length > 1 &&
      requirement.ConstructorArguments[1].Value is bool configured) declareImplicit = configured;
    return target is not null;
  }

  private static void CollectInterfaceContributions(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<INamedTypeSymbol> interfaces,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> result
  ) {
    var sequence = 0;
    foreach (var mixin in interfaces) {
      foreach (var expressionAttribute in OrderedAttributes(mixin)
        .Where(item => IsAttribute(item, Attributes.MixinExpression))) {
        CollectMixinExpressionContributions(
          context, target, target, null, null, expressionAttribute, compilation,
          preparedExpressions, expressionVariables, expressionOutputs, result, ref sequence,
          ContributionKind.Interface, mixin.Name
        );
      }
    }
  }

  private static void CollectAttributeContributions(
    SourceProductionContext context,
    INamedTypeSymbol target,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    IReadOnlyList<ImplicitMixinAttribute> implicitAttributes,
    ICollection<MixinContribution> result
  ) {
    var sequence = 0;
    foreach (var annotated in AnnotatedSymbols(target)) {
      foreach (var applied in OrderedAttributes(annotated)) {
        var attributeType = applied.AttributeClass;
        if (attributeType is null) continue;
        foreach (var expressionAttribute in InheritedAttributes(
          attributeType, Attributes.MixinExpression,
          true, true
        )) {
          CollectMixinExpressionContributions(
            context, target, annotated, applied, null, expressionAttribute, compilation,
            preparedExpressions, expressionVariables, expressionOutputs, result, ref sequence,
            ContributionKind.Attribute, attributeType.Name
          );
        }
      }
    }
    foreach (var implicitAttribute in implicitAttributes) {
      CollectImplicitAttributeContributions(
        context, target, implicitAttribute, compilation, preparedExpressions,
        expressionVariables, expressionOutputs, result, ref sequence
      );
    }
  }

  private static IEnumerable<ISymbol> AnnotatedSymbols(INamedTypeSymbol target) {
    yield return target;
    foreach (var member in OrderedMembers(target))
      if (!member.IsImplicitlyDeclared)
        yield return member;
  }

  private static void CollectImplicitAttributeContributions(
    SourceProductionContext context,
    INamedTypeSymbol target,
    ImplicitMixinAttribute implicitAttribute,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> result,
    ref int sequence
  ) {
    foreach (var expressionAttribute in InheritedAttributes(
      implicitAttribute.Type, Attributes.MixinExpression,
      true, true
    )) {
      CollectMixinExpressionContributions(
        context, target, target, null, implicitAttribute, expressionAttribute, compilation,
        preparedExpressions, expressionVariables, expressionOutputs, result, ref sequence,
        ContributionKind.Attribute, implicitAttribute.Type.Name
      );
    }
  }

  private static void CollectMixinExpressionContributions(
    SourceProductionContext context,
    INamedTypeSymbol target,
    ISymbol annotated,
    AttributeData applied,
    ImplicitMixinAttribute implicitAttribute,
    AttributeData configuration,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> contributions,
    ref int sequence,
    ContributionKind contributionKind,
    string providerName
  ) {
    var location = applied?.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated);
    var attributeName = providerName ?? applied?.AttributeClass?.Name ??
      implicitAttribute?.Type.Name ?? "<unknown>";
    if (!TryReadAttributeExpression(
      configuration, out var targets, out var orders, out var expression, out var failure
    )) {
      ReportInvalidAttributeExpression(
        context, location, attributeName, annotated.Name, failure
      );
      return;
    }

    var declarations = new Dictionary<string, AttributeExpressionTarget>(StringComparer.Ordinal);
    var targetDefinitions = TargetDefinitions(target);
    for (var index = 0; index < targets.Count; index++) {
      var declared = targets[index];
      var emitted = EmittedTarget(declared, targetDefinitions);
      if (!IsValidIdentifier(emitted)) {
        ReportInvalidAttributeExpression(
          context, location, attributeName, annotated.Name,
          "target '" + (declared ?? "null") + "' is not a valid mixin target"
        );
        return;
      }
      if (declarations.ContainsKey(declared)) {
        ReportInvalidAttributeExpression(
          context, location, attributeName, annotated.Name,
          "target '" + declared + "' is declared more than once"
        );
        return;
      }
      declarations.Add(
        declared, new AttributeExpressionTarget(
          declared, orders[index], targetDefinitions
        )
      );
    }

    var arguments = annotated is IMethodSymbol method
      ? (IReadOnlyList<IParameterSymbol>)method.Parameters
      : Array.Empty<IParameterSymbol>();
    var expressionContext = new RoslynMixinExpressionContext(
      target, annotated, applied, arguments, compilation, implicitAttribute?.Type,
      implicitAttribute?.Values, targetDefinitions
    );
    var evaluated = new MixinExpressionInterpreter().Execute(
      expression, expressionContext, expressionVariables, preparedExpressions
    );
    ReportExpressionLogs(context, location, evaluated.Logs);
    if (!evaluated.Success) {
      ReportInvalidAttributeExpression(
        context, location, attributeName, annotated.Name,
        "line " + evaluated.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + evaluated.Error
      );
      return;
    }

    // Validate every destination before publishing any output from this expression.
    foreach (var output in evaluated.Outputs) {
      if (output.Target == MixinExpressionOutputTarget.Mixin) {
        var emitted = EmittedTarget(output.InjectionTarget, targetDefinitions);
        if (!IsValidIdentifier(emitted)) {
          ReportInvalidAttributeExpression(
            context, location, attributeName, annotated.Name,
            "mixin target '" + (output.InjectionTarget ?? "") + "' is not a valid mixin target"
          );
          return;
        }
        continue;
      }
      if (output.Target is MixinExpressionOutputTarget.Class or
        MixinExpressionOutputTarget.File or MixinExpressionOutputTarget.Implements or
        MixinExpressionOutputTarget.Annotation or MixinExpressionOutputTarget.Using) continue;

      if (output.Target == MixinExpressionOutputTarget.Target && targets.Count != 1) {
        ReportInvalidAttributeExpression(
          context, location, attributeName, annotated.Name,
          "@CODE<TARGET> requires exactly one declared target"
        );
        return;
      }
      var injectionTarget = output.Target == MixinExpressionOutputTarget.Target
        ? targets[0]
        : output.InjectionTarget;
      if (string.IsNullOrEmpty(injectionTarget) || !declarations.ContainsKey(injectionTarget)) {
        ReportInvalidAttributeExpression(
          context, location, attributeName, annotated.Name,
          "code target '" + (injectionTarget ?? "") + "' was not declared"
        );
        return;
      }
    }

    List<AttributeExpressionTarget> activated = null;
    foreach (var output in evaluated.Outputs) {
      if (string.IsNullOrEmpty(output.Text)) continue;
      if (output.Target == MixinExpressionOutputTarget.Mixin) {
        var result = new MixinExpressionResult(
          true, null, 0, new[] {
            new MixinExpressionOutput(MixinExpressionOutputTarget.Target, output.Text)
          }
        );
        contributions.Add(
          new MixinContribution(
            output.InjectionTarget, output.InjectionPriority, contributionKind,
            sequence++, targetDefinitions, result
          )
        );
        continue;
      }
      if (output.Target is MixinExpressionOutputTarget.Class or
        MixinExpressionOutputTarget.File or MixinExpressionOutputTarget.Implements or
        MixinExpressionOutputTarget.Annotation or MixinExpressionOutputTarget.Using) {
        expressionOutputs.Add(output);
        continue;
      }
      var injectionTarget = output.Target == MixinExpressionOutputTarget.Target
        ? targets[0]
        : output.InjectionTarget;
      var declaration = declarations[injectionTarget];
      if (declaration.Outputs is null) {
        declaration.Outputs = new List<MixinExpressionOutput>();
        (activated ??= new List<AttributeExpressionTarget>()).Add(declaration);
      }
      declaration.Outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Target, output.Text));
    }
    foreach (var declaration in activated ?? Enumerable.Empty<AttributeExpressionTarget>()) {
      var result = new MixinExpressionResult(true, null, 0, declaration.Outputs);
      contributions.Add(
        new MixinContribution(
          declaration.Target, declaration.Order, contributionKind,
          sequence++, targetDefinitions, result
        )
      );
    }
  }

  private static bool TryReadAttributeExpression(
    AttributeData configuration,
    out IReadOnlyList<string> targets,
    out IReadOnlyList<int> orders,
    out string expression,
    out string failure
  ) {
    targets = Array.Empty<string>();
    orders = Array.Empty<int>();
    expression = null;
    failure = null;
    if (configuration.ConstructorArguments.Length == 1) {
      expression = configuration.ConstructorArguments[0].Value as string;
      if (expression is null) failure = "the expression cannot be null";
      return failure is null;
    }
    if (configuration.ConstructorArguments.Length != 3) {
      failure = "the configuration constructor must declare an expression, optionally with targets and orders";
      return false;
    }

    var targetArgument = configuration.ConstructorArguments[0];
    var orderArgument = configuration.ConstructorArguments[1];
    targets = targetArgument.Kind == TypedConstantKind.Array
      ? targetArgument.Values.Select(item => item.Value as string).ToArray()
      : targetArgument.Value is string target
        ? new[] { target }
        : Array.Empty<string>();
    if (orderArgument.Kind == TypedConstantKind.Array) {
      var values = new List<int>(orderArgument.Values.Length);
      foreach (var item in orderArgument.Values) {
        if (!TryConvertToInt32(item.Value, out var order)) {
          failure = "an order value is not a valid integer";
          return false;
        }
        values.Add(order);
      }
      orders = values;
    } else if (TryConvertToInt32(orderArgument.Value, out var order)) orders = new[] { order };
    expression = configuration.ConstructorArguments[2].Value as string;
    if (targets.Any(string.IsNullOrWhiteSpace)) failure = "target names cannot be empty";
    else if (targets.Count != orders.Count) failure = "target and order arrays must have the same length";
    else if (expression is null) failure = "the expression cannot be null";
    return failure is null;
  }

  private static void ReportInvalidAttributeExpression(
    SourceProductionContext context,
    Location location,
    string attributeName,
    string annotatedName,
    string reason
  ) {
    context.ReportDiagnostic(
      Diagnostic.Create(
        InvalidAttributeExpression, location, attributeName, annotatedName, reason
      )
    );
  }

  private static void ReportExpressionLogs(
    SourceProductionContext context,
    Location location,
    IReadOnlyList<MixinExpressionLog> logs
  ) {
    foreach (var log in logs)
      context.ReportDiagnostic(
        Diagnostic.Create(log.IsHint ? ExpressionHint : ExpressionLog, location, log.Text)
      );
  }


  private static string EmittedTarget(
    string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    return RoslynMixinExpressionContext.ParseMixinTarget(target, targetDefinitions).Name;
  }

  private static List<GeneratedMethod> BuildMethods(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<MixinContribution> contributions,
    CSharpCompilation compilation
  ) {
    var result = new List<GeneratedMethod>();
    foreach (var group in contributions.GroupBy(
      item => (item.IsStaticTarget ? "*" : "") + item.EmittedTarget,
      StringComparer.Ordinal
    )) {
      var ordered = group.OrderBy(item => item.Order)
        .ThenBy(item => item.Kind)
        .ThenBy(item => item.Sequence)
        .ToArray();
      if (TryBuildMethod(
        context, target, ordered[0].EmittedTarget, ordered, compilation, out var method
      )) result.Add(method);
    }
    return result;
  }

  private static bool TryBuildMethod(
    SourceProductionContext context,
    INamedTypeSymbol target,
    string name,
    IReadOnlyList<MixinContribution> contributions,
    CSharpCompilation compilation,
    out GeneratedMethod generated
  ) {
    generated = null;
    var isStatic = contributions[0].IsStaticTarget;
    var isPublic = contributions.Any(item => item.IsPublicTarget);
    var delegateTargets = contributions
      .Select(item => item.DelegateTarget)
      .Where(item => !string.IsNullOrEmpty(item))
      .Distinct(StringComparer.Ordinal)
      .ToArray();
    IMethodSymbol delegateInvoke = null;
    INamedTypeSymbol resolvedDelegate = null;
    foreach (var delegateTarget in delegateTargets) {
      var delegateType = ResolveDelegateTarget(compilation, delegateTarget);
      if (delegateType is not { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke }) {
        ReportInvalidTarget(
          context, target, name,
          "delegate type '" + delegateTarget + "' was not found or is not a delegate"
        );
        return false;
      }
      if (delegateType.IsGenericType) {
        ReportInvalidTarget(context, target, name, "generic delegate target types are not supported");
        return false;
      }
      if (resolvedDelegate is not null &&
        !SymbolEqualityComparer.Default.Equals(resolvedDelegate, delegateType)) {
        ReportInvalidTarget(context, target, name, "contributions specify conflicting delegate signatures");
        return false;
      }
      resolvedDelegate = delegateType;
      delegateInvoke = invoke;
    }

    var declared = target.GetMembers(name).ToArray();
    IMethodSymbol signature = null;
    MethodDeclarationSyntax partialSyntax = null;
    foreach (var member in declared) {
      if (member is IMethodSymbol method && TryGetUnimplementedPartial(method, out var syntax)) {
        if (method.IsStatic != isStatic) {
          ReportInvalidTarget(
            context, target, name,
            "the partial declaration does not match the target's static modifier"
          );
          return false;
        }
        if (isPublic && method.DeclaredAccessibility != Accessibility.Public) {
          ReportInvalidTarget(
            context, target, name,
            "the partial declaration is not public but the target uses the '^' modifier"
          );
          return false;
        }
        if (signature is not null) {
          ReportInvalidTarget(context, target, name, "more than one partial overload matches the target");
          return false;
        }
        signature = method;
        partialSyntax = syntax;
        continue;
      }
      context.ReportDiagnostic(Diagnostic.Create(ExistingTarget, LocationOf(member), name, target.Name));
      return false;
    }

    IMethodSymbol baseMethod = null;
    if (signature is null && !isStatic && delegateInvoke is null) {
      baseMethod = FindBaseMethod(target, name, 0);
      signature = baseMethod;
    }
    if (isPublic && baseMethod is not null &&
      baseMethod.DeclaredAccessibility != Accessibility.Public) {
      ReportInvalidTarget(
        context, target, name,
        "the inherited declaration is not public but the target uses the '^' modifier"
      );
      return false;
    }
    if (signature is not null && delegateInvoke is not null &&
      !HasSameSignature(signature, delegateInvoke)) {
      ReportInvalidTarget(
        context, target, name,
        "the partial declaration does not match delegate '" + delegateTargets[0] + "'"
      );
      return false;
    }

    var targetSignature = delegateInvoke ?? signature;
    if (targetSignature is { ReturnsByRef: true } or { ReturnsByRefReadonly: true }) {
      ReportInvalidTarget(context, target, name, "ref returns are not supported");
      return false;
    }
    var parameters = targetSignature?.Parameters.Select(MixinTargetParameter.FromSymbol).ToArray() ??
      Array.Empty<MixinTargetParameter>();
    var returnType = targetSignature?.ReturnType;
    var returnsVoid = returnType is null || returnType.SpecialType == SpecialType.System_Void;
    var declaration = signature is null
      ? (isPublic ? "public " : "private ") + (isStatic ? "static " : "") +
      (returnsVoid ? "void" : returnType.ToDisplayString(TypeDisplayFormat)) + " " + EscapeIdentifier(name)
      : BuildMethodDeclaration(signature, partialSyntax, baseMethod is not null);
    generated = new GeneratedMethod(
      declaration,
      signature?.TypeParameters.Select(item => EscapeIdentifier(item.Name)).ToArray() ?? Array.Empty<string>(),
      partialSyntax is null ? Array.Empty<string>() : TypeParameterConstraints(signature),
      parameters,
      returnsVoid ? null : returnType.ToDisplayString(TypeDisplayFormat),
      baseMethod is { IsAbstract: false },
      name,
      contributions
    );
    return true;
  }

  private static void ReportInvalidTarget(
    SourceProductionContext context,
    INamedTypeSymbol target,
    string name,
    string reason
  ) {
    context.ReportDiagnostic(
      Diagnostic.Create(
        InvalidTarget, LocationOf(target), name, target.Name, reason
      )
    );
  }

  private static bool TryGetUnimplementedPartial(
    IMethodSymbol method,
    out MethodDeclarationSyntax syntax
  ) {
    syntax = method.DeclaringSyntaxReferences
      .Select(item => item.GetSyntax())
      .OfType<MethodDeclarationSyntax>()
      .FirstOrDefault(item => item.Modifiers.Any(SyntaxKind.PartialKeyword) &&
        item.Body is null && item.ExpressionBody is null
      );
    return syntax is not null && method.PartialImplementationPart is null;
  }

  private static IMethodSymbol FindBaseMethod(
    INamedTypeSymbol target,
    string name,
    int parameterCount
  ) {
    for (var current = target.BaseType; current is not null; current = current.BaseType) {
      var methods = current.GetMembers(name).OfType<IMethodSymbol>()
        .Where(item => !item.IsStatic && item.DeclaredAccessibility != Accessibility.Private &&
          !item.IsSealed && (item.IsAbstract || item.IsVirtual || item.IsOverride)
        )
        .ToArray();
      var exact = methods.FirstOrDefault(item => item.Parameters.Length == parameterCount);
      if (exact is not null) return exact;
      if (methods.Length == 1) return methods[0];
    }
    return null;
  }

  private static INamedTypeSymbol ResolveDelegateTarget(
    CSharpCompilation compilation,
    string typeName
  ) {
    var normalized = typeName.StartsWith("global::", StringComparison.Ordinal)
      ? typeName.Substring("global::".Length)
      : typeName;
    var direct = compilation.GetTypeByMetadataName(normalized);
    if (direct is not null) return direct;
    var separator = Math.Max(normalized.LastIndexOf('.'), normalized.LastIndexOf('+'));
    var simpleName = separator < 0 ? normalized : normalized.Substring(separator + 1);
    return compilation.GetSymbolsWithName(simpleName, SymbolFilter.Type)
      .OfType<INamedTypeSymbol>()
      .FirstOrDefault(item => item.ToDisplayString() == normalized);
  }

  private static bool HasSameSignature(IMethodSymbol method, IMethodSymbol expected) {
    if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, expected.ReturnType) ||
      method.RefKind != expected.RefKind || method.Parameters.Length != expected.Parameters.Length ||
      method.TypeParameters.Length != 0) return false;
    for (var index = 0; index < method.Parameters.Length; index++) {
      var actual = method.Parameters[index];
      var wanted = expected.Parameters[index];
      if (actual.RefKind != wanted.RefKind ||
        !SymbolEqualityComparer.Default.Equals(actual.Type, wanted.Type)) return false;
    }
    return true;
  }

  private static string BuildMethodDeclaration(
    IMethodSymbol method,
    MethodDeclarationSyntax partialSyntax,
    bool isOverride
  ) {
    var modifiers = new List<string>();
    if (partialSyntax is not null) {
      modifiers.AddRange(
        partialSyntax.Modifiers
          .Where(item => item.IsKind(SyntaxKind.PublicKeyword) ||
            item.IsKind(SyntaxKind.PrivateKeyword) ||
            item.IsKind(SyntaxKind.ProtectedKeyword) ||
            item.IsKind(SyntaxKind.InternalKeyword) ||
            item.IsKind(SyntaxKind.StaticKeyword) ||
            item.IsKind(SyntaxKind.UnsafeKeyword) ||
            item.IsKind(SyntaxKind.PartialKeyword)
          )
          .Select(item => item.Text)
      );
    } else if (isOverride) {
      modifiers.Add(AccessibilityText(method.DeclaredAccessibility));
      modifiers.Add("override");
    }

    var typeParameters = method.TypeParameters.Length == 0
      ? ""
      : "<" + string.Join(", ", method.TypeParameters.Select(item => EscapeIdentifier(item.Name))) + ">";
    return string.Join(" ", modifiers) +
      (modifiers.Count == 0 ? "" : " ") +
      (method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString(TypeDisplayFormat)) + " " +
      EscapeIdentifier(method.Name) + typeParameters;
  }

  private static IReadOnlyList<string> TypeParameterConstraints(IMethodSymbol method) {
    var result = new List<string>();
    foreach (var parameter in method.TypeParameters) {
      var constraints = new List<string>();
      if (parameter.HasUnmanagedTypeConstraint) constraints.Add("unmanaged");
      else if (parameter.HasValueTypeConstraint) constraints.Add("struct");
      else if (parameter.HasReferenceTypeConstraint) {
        constraints.Add(
          parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
            ? "class?"
            : "class"
        );
      }
      if (parameter.HasNotNullConstraint) constraints.Add("notnull");
      constraints.AddRange(parameter.ConstraintTypes.Select(item => item.ToDisplayString(TypeDisplayFormat)));
      if (parameter.HasConstructorConstraint) constraints.Add("new()");
      if (constraints.Count != 0)
        result.Add("where " + EscapeIdentifier(parameter.Name) + " : " + string.Join(", ", constraints));
    }
    return result;
  }

  private static void AppendMethod(SharpStringBuilder builder, GeneratedMethod method) {
    var declaration = method.Declaration;
    if (method.TypeParameters.Count != 0 && !declaration.EndsWith(
      ">", StringComparison.Ordinal
    )) declaration += "<" + string.Join(", ", method.TypeParameters) + ">";
    var parameters = method.Parameters.Select(item => item.Declaration).ToArray();
    builder.Append(declaration).Parameters(parameters, parameters.Length > 1);
    foreach (var constraint in method.Constraints) builder.Append(" ").Append(constraint);
    using (builder.Block()) {
      if (method.ReturnType is not null) builder.Statement(method.ReturnType + " __mixinReturnValue = default");

      var baseEmitted = !method.CallBase;
      foreach (var contribution in method.Contributions) {
        if (!baseEmitted && contribution.Order >= 0) {
          AppendBaseCall(builder, method);
          baseEmitted = true;
        }
        AppendInvocation(builder, contribution);
      }
      if (!baseEmitted) AppendBaseCall(builder, method);
      if (method.ReturnType is not null) builder.Return("__mixinReturnValue");
    }
  }

  private static void AppendBaseCall(SharpStringBuilder builder, GeneratedMethod method) {
    var call = "base." + EscapeIdentifier(method.Name) + "(" +
      string.Join(", ", method.Parameters.Select(item => item.Argument)) + ")";
    builder.Statement(method.ReturnType is null ? call : "__mixinReturnValue = " + call);
  }

  private static void AppendInvocation(
    SharpStringBuilder builder,
    MixinContribution contribution
  ) {
    foreach (var output in contribution.ExpressionResult.Outputs) {
      if (output.Target == MixinExpressionOutputTarget.Target) builder.Statement(output.Text);
    }
  }

  private static string RefPrefix(RefKind kind) {
    return kind switch {
      RefKind.Ref => "ref ",
      RefKind.Out => "out ",
      RefKind.In => "in ",
      _ => ""
    };
  }

  private static IReadOnlyList<ISymbol> OrderedMembers(INamedTypeSymbol type) {
    return type.GetMembers()
      .OrderBy(
        item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceTree?.FilePath,
        StringComparer.Ordinal
      )
      .ThenBy(SourceOrder)
      .ToArray();
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) {
    return symbol.GetAttributes()
      .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
      .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
      .ToArray();
  }

  private static IReadOnlyList<AttributeData> InheritedAttributes(
    INamedTypeSymbol type,
    string metadataName,
    bool allowMultiple,
    bool baseFirst = false
  ) {
    var result = new List<AttributeData>();
    var hierarchy = new List<INamedTypeSymbol>();
    for (var current = type; current is not null; current = current.BaseType) hierarchy.Add(current);
    if (baseFirst) hierarchy.Reverse();
    foreach (var current in hierarchy) {
      var declared = OrderedAttributes(current)
        .Where(item => IsAttribute(item, metadataName))
        .ToArray();
      if (declared.Length == 0) continue;
      result.AddRange(declared);
      if (!allowMultiple) break;
    }
    return result;
  }

  private static bool IsAttribute(AttributeData attribute, string metadataName) {
    return attribute.AttributeClass?.ToDisplayString() == metadataName;
  }
}