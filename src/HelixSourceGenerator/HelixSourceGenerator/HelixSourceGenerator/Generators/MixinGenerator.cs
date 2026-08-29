using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
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
  private const string LateClassMarker = "// __HELIX_LATE_CLASS__";
  private const string LateFileMarker = "// __HELIX_LATE_FILE__";
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var libraryCatalog = context.AdditionalTextsProvider
      .Select(static (file, cancellationToken) =>
        MixinLibraryApi.ReadAdditionalFile(file, cancellationToken)
      )
      .Collect()
      .Select(static (files, _) => new MixinLibraryCatalog(files))
      .WithComparer(MixinLibraryCatalogComparer.Instance)
      .WithTrackingName("Mixin.LibraryCatalog");

    StructureSupport.Register(context, libraryCatalog);

    foreach (var attribute in MixinGeneratorCandidates.AttributeMetadataNames) {
      var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
        attribute,
        static (node, _) => node is TypeDeclarationSyntax {
          RawKind: (int)SyntaxKind.ClassDeclaration or (int)SyntaxKind.RecordDeclaration
        },
        static (ctx, _) => GetTarget(ctx)
      ).Where(static target => target is not null);
      var collected = targets.Combine(libraryCatalog)
        .Select(static (item, _) => new CollectedMixinProgram(item.Left, item.Right))
        .WithTrackingName("Mixin.Collect");
      var primary = collected
        .Select(static (program, _) => Generate(program.Target, program.Libraries))
        .WithTrackingName("Mixin.PrimaryEvaluation");
      var outputModels = primary
        .Select(static (result, _) => result.Output)
        .WithComparer(MixinOutputModelComparer.Instance)
        .WithTrackingName("Mixin.UnlinkedModel");
      var finalized = outputModels
        .Select(static (model, _) => EvaluateLate(model))
        .WithTrackingName("Mixin.LateEvaluation");
      context.RegisterSourceOutput(
        primary,
        static (spc, result) => EmitDiagnostics(spc, result)
      );
      context.RegisterSourceOutput(
        finalized,
        static (spc, model) => EmitSource(spc, model)
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
          if (attributeType.ToDisplayString() == Attributes.Managed)
            return true;
      }
    }
    return false;
  }

  private static IReadOnlyDictionary<string, string> TargetDefinitions(
    INamedTypeSymbol type,
    IEnumerable<INamedTypeSymbol> providers,
    MixinLibraryCatalog libraries
  ) {
    var definitions = new Dictionary<string, string>(StringComparer.Ordinal) {
      ["$Init"] = HasComponentStereotype(type) ? "^LoadComponent" : "Awake",
      ["$Dispose"] = HasComponentStereotype(type) ? "^UnloadComponent" : "OnDestroy"
    };
    var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    foreach (var provider in providers.Where(item => item is not null)) {
      if (SymbolEqualityComparer.Default.Equals(provider, type)) continue;
      if (!visited.Add(provider)) continue;
      foreach (var annotation in AnnotationDefinitions(provider, libraries))
        foreach (var definition in annotation.TargetDefinitions)
          definitions[definition.Key] = definition.Value;
    }
    foreach (var annotation in AnnotationDefinitions(type, libraries))
      foreach (var definition in annotation.TargetDefinitions)
        definitions[definition.Key] = definition.Value;
    return definitions;
  }

  private static IEnumerable<MixinAnnotationDefinition> AnnotationDefinitions(
    INamedTypeSymbol type,
    MixinLibraryCatalog libraries
  ) => MixinLibraryApi.Annotations(type, libraries);

  private static MixinGenerationResult Generate(
    MixinTarget candidate,
    MixinLibraryCatalog libraries
  ) {
    var context = new MixinGenerationContext();
    var target = candidate.Type;
    var location = LocationOf(target);
    if (!IsPartial(target)) {
      context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, target.Name));
      return context.Complete();
    }

    var containing = FirstNonPartialContainingType(target);
    if (containing is not null) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          ContainingTypeMustBePartial, location, target.Name, containing.Name
        )
      );
      return context.Complete();
    }

    var interfaces = CollectMixinInterfaces(target);
    var implicitAttributes = new List<ImplicitMixinAttribute>();
    var implicitInterfaces = new List<INamedTypeSymbol>();
    ResolveMixinRequirements(
      context, target, interfaces, implicitAttributes, implicitInterfaces, libraries
    );
    var libraryOwners = new List<INamedTypeSymbol> { target };
    libraryOwners.AddRange(interfaces);
    libraryOwners.AddRange(implicitInterfaces);
    libraryOwners.AddRange(implicitAttributes.Select(item => item.Type));
    libraryOwners.AddRange(MixinLibraryApi.AttributeOwners(AnnotatedSymbols(target)));
    var preparedExpressions = MixinLibraryApi.Prepare(
      context.ReportDiagnostic, libraryOwners, libraries
    );
    var annotationProviders = interfaces
      .Concat(implicitInterfaces)
      .Concat(implicitAttributes.Select(item => item.Type))
      .Concat(MixinLibraryApi.AttributeOwners(AnnotatedSymbols(target)))
      .Concat(new[] { target });
    var targetDefinitions = TargetDefinitions(target, annotationProviders, libraries);
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
      attributeExpressionOutputs, contributions, targetDefinitions, libraries
    );
    CollectAttributeContributions(
      context, target, candidate.Compilation, preparedExpressions, expressionVariables,
      attributeExpressionOutputs, implicitAttributes, contributions, targetDefinitions, libraries
    );
    var placeholderSequence = contributions.Count;
    foreach (var work in context.LateExpressions)
      foreach (var lateTarget in work.Targets)
        contributions.Add(MixinContribution.Placeholder(lateTarget, placeholderSequence++, work));
    var methods = BuildMethods(
      context, target, contributions, candidate.Compilation
    );
    ReportContributionData(context, target, methods);
    var outputs = new ExpressionOutputs();
    foreach (var attribute in implicitAttributes) {
      outputs.AddAnnotation(attribute.Type.ToDisplayString(TypeDisplayFormat));
    }
    foreach (var contribution in contributions) {
      if (contribution.ExpressionResult is null) continue;
      foreach (var output in contribution.ExpressionResult.Outputs) outputs.Add(output);
    }
    foreach (var output in attributeExpressionOutputs) outputs.Add(output);
    if (methods.Count == 0 && !outputs.Any && !context.HasLateExpressions) return context.Complete();
    var wrapper = WrapType(
      target, "mixins"
    );
    return context.Complete(
      new MixinRenderModel(
        wrapper.Detach(), methods.ToImmutableArray(),
        outputs.Annotations.ToImmutableArray(), outputs.Class.ToImmutableArray(),
        outputs.File.ToImmutableArray(), outputs.Implements.ToImmutableArray(),
        outputs.Usings.ToImmutableArray()
      )
    );
  }

  private static void EmitDiagnostics(SourceProductionContext context, MixinGenerationResult result) {
    foreach (var diagnostic in result.Diagnostics) context.ReportDiagnostic(diagnostic);
  }

  private static void EmitSource(SourceProductionContext context, MixinOutputModel model) {
    foreach (var log in model.Logs)
      context.ReportDiagnostic(Diagnostic.Create(log.IsHint ? ExpressionHint : ExpressionLog, Location.None, log.Text));
    foreach (var error in model.Errors)
      context.ReportDiagnostic(Diagnostic.Create(InvalidAttributeExpression, Location.None, "late expression", "late evaluation", error));
    if (model.Source is not null) context.AddSource(model.HintName, model.Source);
  }

  private static MixinOutputModel EvaluateLate(MixinOutputModel model) {
    if (model.Render is null) return model;
    var classCode = model.Render.Class.ToList();
    var fileCode = model.Render.File.ToList();
    var usings = new HashSet<string>(model.Render.Usings, StringComparer.Ordinal);
    var implements = model.Render.Implements.ToList();
    var annotations = model.Render.Annotations.ToList();
    var errors = new List<string>();
    var logs = new List<MixinExpressionLog>();
    var lateContributions = new List<MixinContribution>();
    var lateSequence = 1_000_000;
    var sharedVariables = new Dictionary<string, object>(StringComparer.Ordinal);
    var interpreter = new MixinExpressionInterpreter();
    foreach (var work in model.LateExpressions) {
      var variables = work.Variables.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
      foreach (var item in sharedVariables) variables[item.Key] = item.Value;
      var result = interpreter.Execute(
        work.Expression, UnlinkedMixinExpressionContext.Instance, variables, work.PreparedState,
        new MixinExpressionPreludeSnapshot(work.Variables, work.PreludeOutputs)
      );
      logs.AddRange(result.Logs);
      if (!result.Success) {
        errors.Add("line " + result.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + result.Error);
        continue;
      }
      foreach (var item in variables)
        if (!item.Key.StartsWith(MixinExpressionInterpreter.CarryLocalPrefix, StringComparison.Ordinal))
          sharedVariables[item.Key] = item.Value;
      foreach (var output in result.Outputs) switch (output.Target) {
        case MixinExpressionOutputTarget.Class: classCode.Add(output.Text); break;
        case MixinExpressionOutputTarget.File: fileCode.Add(output.Text); break;
        case MixinExpressionOutputTarget.Using:
          usings.Add("using " + output.Text.Trim().TrimEnd(';') + ";"); break;
        case MixinExpressionOutputTarget.Implements: implements.Add(output.Text.Trim()); break;
        case MixinExpressionOutputTarget.Annotation: annotations.Add(output.Text.Trim()); break;
        case MixinExpressionOutputTarget.Target:
          if (work.Targets.Length != 1) {
            errors.Add("@CODE<TARGET> requires exactly one declared target");
            break;
          }
          lateContributions.Add(LateContribution(
            work.Targets[0], work.Targets[0].Order, output.Text, work, lateSequence++
          ));
          break;
        case MixinExpressionOutputTarget.Injection: {
          var target = work.Targets.FirstOrDefault(item =>
            item.DeclaredTarget == output.InjectionTarget || item.EmittedTarget == output.InjectionTarget
          );
          if (target is null) errors.Add("late code target '" + output.InjectionTarget + "' was not declared");
          else lateContributions.Add(LateContribution(target, target.Order, output.Text, work, lateSequence++));
          break;
        }
        case MixinExpressionOutputTarget.Mixin: {
          var target = work.Targets.FirstOrDefault(item =>
            item.DeclaredTarget == output.InjectionTarget || item.EmittedTarget == output.InjectionTarget
          );
          if (target is null) errors.Add("late mixin target '" + output.InjectionTarget + "' was not prepared by the Prelude");
          else lateContributions.Add(LateContribution(
            target, output.InjectionPriority, output.Text, work, lateSequence++
          ));
          break;
        }
        default:
          errors.Add("late expression output '" + output.Target + "' requires a detached method contribution"); break;
      }
    }
    var methods = model.Render.Methods.Select(method => {
      var retained = method.Contributions.Where(item => !item.IsPlaceholder).ToList();
      retained.AddRange(lateContributions.Where(item =>
        item.EmittedTarget == method.Name &&
        item.IsStaticTarget == method.Contributions.First().IsStaticTarget
      ));
      return retained.Count == 0 ? null : method with {
        Contributions = retained.OrderBy(item => item.Order)
          .ThenBy(item => item.Kind).ThenBy(item => item.Sequence).ToArray()
      };
    }).Where(item => item is not null).ToImmutableArray();
    var source = model.Render.Wrapper.Build(
      usings.OrderBy(item => item, StringComparer.Ordinal).ToArray(),
      implements.Distinct(StringComparer.Ordinal).ToArray(),
      annotations.Distinct(StringComparer.Ordinal).ToArray(),
      builder => {
        for (var index = 0; index < methods.Length; index++) {
          AppendMethod(builder, methods[index]);
          if (index != methods.Length - 1) builder.BlankLine();
        }
        foreach (var output in classCode) {
          if (methods.Length != 0) builder.BlankLine();
          builder.AppendCode(output);
        }
      }, builder => {
        foreach (var output in fileCode) builder.AppendCode(output);
      }
    );
    return new MixinOutputModel(
      model.Render.Wrapper.HintName, source, null, ImmutableArray<LateExpressionWork>.Empty,
      errors.ToImmutableArray(), logs.ToImmutableArray()
    );
  }

  private static MixinContribution LateContribution(
    LateTarget target, int order, string text, LateExpressionWork work, int sequence
  ) => new(
    target, order, work.Kind, sequence,
    new MixinExpressionResult(true, null, 0, new[] {
      new MixinExpressionOutput(MixinExpressionOutputTarget.Target, text)
    }),
    work
  );

  private sealed class MixinGenerationContext {
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly List<LateExpressionWork> _lateExpressions = new();
    internal bool HasLateExpressions => _lateExpressions.Count != 0;
    internal IReadOnlyList<LateExpressionWork> LateExpressions => _lateExpressions;

    internal void ReportDiagnostic(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);

    internal void AddLateExpression(
      string expression,
      IReadOnlyDictionary<string, object> variables,
      IReadOnlyList<MixinExpressionOutput> preludeOutputs,
      MixinExpressionPreparedState preparedState,
      ImmutableArray<LateTarget> targets,
      ContributionKind kind,
      string provider,
      string sourceType,
      string sourceMember,
      string sourceKind,
      int sourceParameterCount
    ) =>
      _lateExpressions.Add(new LateExpressionWork(
        expression, variables.ToImmutableDictionary(StringComparer.Ordinal),
        preludeOutputs.ToImmutableArray(), preparedState,
        PreparedStateKey(preparedState), targets, kind, provider ?? "",
        sourceType ?? "", sourceMember ?? "", sourceKind ?? "", sourceParameterCount
      ));

    private static string PreparedStateKey(MixinExpressionPreparedState state) => string.Join(
      "\u001e", state.Instructions.Select(instruction =>
        instruction.Command + "\u001f" + string.Join("\u001f", instruction.Arguments) + "\u001f" + instruction.Operand
      )
    );

    internal MixinGenerationResult Complete(MixinRenderModel render = null) =>
      new(_diagnostics.ToImmutableArray(), render, _lateExpressions.ToImmutableArray());
  }

  private sealed class MixinGenerationResult {
    internal MixinGenerationResult(
      ImmutableArray<Diagnostic> diagnostics, MixinRenderModel render,
      ImmutableArray<LateExpressionWork> lateExpressions
    ) {
      Diagnostics = diagnostics;
      Render = render;
      LateExpressions = lateExpressions;
    }

    internal ImmutableArray<Diagnostic> Diagnostics { get; }
    internal MixinRenderModel Render { get; }
    internal ImmutableArray<LateExpressionWork> LateExpressions { get; }
    internal MixinOutputModel Output => new(
      Render?.Wrapper.HintName, null, Render, LateExpressions,
      ImmutableArray<string>.Empty, ImmutableArray<MixinExpressionLog>.Empty
    );
  }

  private sealed record CollectedMixinProgram(
    MixinTarget Target,
    MixinLibraryCatalog Libraries
  );

  private sealed record LateExpressionWork(
    string Expression,
    ImmutableDictionary<string, object> Variables,
    ImmutableArray<MixinExpressionOutput> PreludeOutputs,
    MixinExpressionPreparedState PreparedState,
    string PreparedStateKey,
    ImmutableArray<LateTarget> Targets,
    ContributionKind Kind,
    string Provider,
    string SourceType,
    string SourceMember,
    string SourceKind,
    int SourceParameterCount
  );

  private sealed record LateTarget(
    string DeclaredTarget,
    string EmittedTarget,
    bool IsStatic,
    bool IsPublic,
    string DelegateTarget,
    int Order
  );

  private sealed record MixinRenderModel(
    DetachedTypeWrapper Wrapper,
    ImmutableArray<GeneratedMethod> Methods,
    ImmutableArray<string> Annotations,
    ImmutableArray<string> Class,
    ImmutableArray<string> File,
    ImmutableArray<string> Implements,
    ImmutableArray<string> Usings
  );

  private sealed record MixinOutputModel(
    string HintName, string Source, MixinRenderModel Render,
    ImmutableArray<LateExpressionWork> LateExpressions, ImmutableArray<string> Errors,
    ImmutableArray<MixinExpressionLog> Logs
  );

  private sealed class MixinOutputModelComparer : IEqualityComparer<MixinOutputModel> {
    internal static readonly MixinOutputModelComparer Instance = new();
    public bool Equals(MixinOutputModel x, MixinOutputModel y) =>
      ReferenceEquals(x, y) || x is not null && y is not null &&
      string.Equals(x.HintName, y.HintName, StringComparison.Ordinal) &&
      string.Equals(x.Source, y.Source, StringComparison.Ordinal) &&
      string.Equals(RenderKey(x.Render), RenderKey(y.Render), StringComparison.Ordinal) &&
      LateEqual(x.LateExpressions, y.LateExpressions);

    private static string RenderKey(MixinRenderModel render) {
      if (render is null) return "";
      var values = new List<string> {
        render.Wrapper.NamespaceName ?? "", render.Wrapper.HintName ?? ""
      };
      values.AddRange(render.Wrapper.Declarations);
      values.AddRange(render.Annotations);
      values.AddRange(render.Class);
      values.AddRange(render.File);
      values.AddRange(render.Implements);
      values.AddRange(render.Usings);
      foreach (var method in render.Methods) {
        values.Add(method.Declaration);
        values.Add(method.ReturnType ?? "");
        values.Add(method.CallBase.ToString());
        values.Add(method.Name);
        values.AddRange(method.TypeParameters);
        values.AddRange(method.Constraints);
        foreach (var parameter in method.Parameters) {
          values.Add(parameter.Declaration);
          values.Add(parameter.Argument);
        }
        foreach (var contribution in method.Contributions) {
          values.Add(contribution.EmittedTarget);
          values.Add(contribution.Order.ToString(CultureInfo.InvariantCulture));
          foreach (var output in contribution.ExpressionResult.Outputs) {
            values.Add(output.Target.ToString());
            values.Add(output.Text);
          }
        }
      }
      return string.Join("\u001f", values);
    }

    private static bool LateEqual(ImmutableArray<LateExpressionWork> x, ImmutableArray<LateExpressionWork> y) {
      if (x.Length != y.Length) return false;
      for (var index = 0; index < x.Length; index++) {
        var left = x[index];
        var right = y[index];
        if (left.Expression != right.Expression || left.PreparedStateKey != right.PreparedStateKey ||
          left.Variables.Count != right.Variables.Count || left.Kind != right.Kind ||
          left.Provider != right.Provider || left.SourceType != right.SourceType ||
          left.SourceMember != right.SourceMember || left.SourceKind != right.SourceKind ||
          left.SourceParameterCount != right.SourceParameterCount ||
          left.PreludeOutputs.Length != right.PreludeOutputs.Length ||
          left.Targets.Length != right.Targets.Length) return false;
        for (var outputIndex = 0; outputIndex < left.PreludeOutputs.Length; outputIndex++) {
          var leftOutput = left.PreludeOutputs[outputIndex];
          var rightOutput = right.PreludeOutputs[outputIndex];
          if (leftOutput.Target != rightOutput.Target || leftOutput.Text != rightOutput.Text ||
            leftOutput.InjectionTarget != rightOutput.InjectionTarget ||
            leftOutput.InjectionPriority != rightOutput.InjectionPriority) return false;
        }
        for (var targetIndex = 0; targetIndex < left.Targets.Length; targetIndex++)
          if (left.Targets[targetIndex] != right.Targets[targetIndex]) return false;
        foreach (var item in left.Variables)
          if (!right.Variables.TryGetValue(item.Key, out var value) || !Equals(item.Value, value)) return false;
      }
      return true;
    }
    public int GetHashCode(MixinOutputModel value) => value is null
      ? 0
      : unchecked(
        StringComparer.Ordinal.GetHashCode(value.HintName ?? "") * 397 ^
        StringComparer.Ordinal.GetHashCode(value.Source ?? "") * 31 ^
        StringComparer.Ordinal.GetHashCode(RenderKey(value.Render))
      );
  }

  private sealed class UnlinkedMixinExpressionContext : IMixinExpressionContext {
    internal static readonly UnlinkedMixinExpressionContext Instance = new();

    public bool TryResolve(
      MixinExpressionReference reference, out string value, out string error
    ) {
      value = null;
      error = "late expressions cannot resolve Roslyn value '@" + reference.Root + "'";
      return false;
    }

    public bool TryEvaluate(
      MixinExpressionReference reference, out bool value, out string error
    ) {
      value = false;
      error = "late expressions cannot evaluate Roslyn value '@" + reference.Root + "'";
      return false;
    }
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
    private MixinContribution(
      LateTarget target,
      int order,
      ContributionKind kind,
      int sequence,
      MixinExpressionResult expressionResult,
      LateExpressionWork work,
      bool placeholder
    ) : this(target, order, kind, sequence, expressionResult, work) {
      IsPlaceholder = placeholder;
    }

    internal static MixinContribution Placeholder(
      LateTarget target, int sequence, LateExpressionWork work
    ) => new(
      target, target.Order, work.Kind, sequence,
      new MixinExpressionResult(true, null, 0, Array.Empty<MixinExpressionOutput>()),
      work, true
    );

    internal MixinContribution(
      LateTarget target,
      int order,
      ContributionKind kind,
      int sequence,
      MixinExpressionResult expressionResult,
      LateExpressionWork work
    ) {
      EmittedTarget = target.EmittedTarget;
      IsStaticTarget = target.IsStatic;
      IsPublicTarget = target.IsPublic;
      DelegateTarget = target.DelegateTarget;
      Order = order;
      Kind = kind;
      Sequence = sequence;
      ExpressionResult = expressionResult;
      Provider = work.Provider;
      SourceType = work.SourceType;
      SourceMember = work.SourceMember;
      SourceKind = work.SourceKind;
      SourceParameterCount = work.SourceParameterCount;
    }

    internal MixinContribution(
      string target,
      int order,
      ContributionKind kind,
      int sequence,
      IReadOnlyDictionary<string, string> targetDefinitions,
      MixinExpressionResult expressionResult,
      string provider,
      ISymbol source
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
      Provider = provider;
      SourceType = (source as INamedTypeSymbol ?? source.ContainingType)
        ?.ToDisplayString(TypeDisplayFormat) ?? "";
      SourceMember = source is INamedTypeSymbol ? "" : source.MetadataName;
      SourceKind = source.Kind.ToString();
      SourceParameterCount = source is IMethodSymbol method ? method.Parameters.Length : 0;
    }

    internal string EmittedTarget { get; }
    internal bool IsStaticTarget { get; }
    internal bool IsPublicTarget { get; }
    internal string DelegateTarget { get; }
    internal int Order { get; }
    internal ContributionKind Kind { get; }
    internal int Sequence { get; }
    internal MixinExpressionResult ExpressionResult { get; }
    internal string Provider { get; }
    internal string SourceType { get; }
    internal string SourceMember { get; }
    internal string SourceKind { get; }
    internal int SourceParameterCount { get; }
    internal bool IsPlaceholder { get; }
  }

  private sealed record MixinTargetParameter(string Declaration, string Argument) {
    internal static MixinTargetParameter FromSymbol(IParameterSymbol parameter) {
      var type = parameter.Type.ToDisplayString(TypeDisplayFormat);
      return new MixinTargetParameter(
        (parameter.IsParams ? "params " : RefPrefix(parameter.RefKind)) + type + " " + EscapeIdentifier(parameter.Name),
        RefPrefix(parameter.RefKind) + EscapeIdentifier(parameter.Name)
      );
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
      IsStatic = targetSyntax.IsStatic;
      IsPublic = targetSyntax.IsPublic;
      DelegateTarget = targetSyntax.DelegateType;
      Order = order;
    }

    internal string Target { get; }
    internal string EmittedTarget { get; }
    internal bool IsStatic { get; }
    internal bool IsPublic { get; }
    internal string DelegateTarget { get; }
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
    MixinGenerationContext context,
    INamedTypeSymbol target,
    IList<INamedTypeSymbol> interfaces,
    ICollection<ImplicitMixinAttribute> implicitAttributes,
    ICollection<INamedTypeSymbol> implicitInterfaces,
    MixinLibraryCatalog libraries
  ) {
    var interfaceSet = new HashSet<INamedTypeSymbol>(interfaces, SymbolEqualityComparer.Default);
    var attributeSet = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    foreach (var attribute in target.GetAttributes()) {
      if (attribute.AttributeClass is not null) attributeSet.Add(attribute.AttributeClass);
    }
    var providers = new Queue<INamedTypeSymbol>();
    foreach (var mixin in interfaces) providers.Enqueue(mixin);

    foreach (var attribute in target.GetAttributes()) {
      if (IsMixinYieldingAttribute(attribute.AttributeClass, libraries)) providers.Enqueue(attribute.AttributeClass);
    }
    foreach (var member in OrderedMembers(target)) {
      if (member.IsImplicitlyDeclared) continue;
      foreach (var attribute in member.GetAttributes()) {
        if (IsMixinYieldingAttribute(attribute.AttributeClass, libraries)) providers.Enqueue(attribute.AttributeClass);
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
          !(IsMixinInterface(required) || IsMixinYieldingAttribute(required, libraries))) {
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

  private static bool IsMixinYieldingAttribute(INamedTypeSymbol type, MixinLibraryCatalog libraries) {
    return type is { TypeKind: TypeKind.Class } &&
      InheritsFromSystemAttribute(type) &&
      AnnotationDefinitions(type, libraries).Any();
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
    MixinGenerationContext context,
    INamedTypeSymbol target,
    IReadOnlyList<INamedTypeSymbol> interfaces,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> result,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries
  ) {
    var sequence = 0;
    foreach (var mixin in interfaces) {
      foreach (var expressionAttribute in AnnotationDefinitions(mixin, libraries)) {
        CollectMixinExpressionContributions(
          context, target, target, null, null, expressionAttribute, compilation,
          preparedExpressions, expressionVariables, expressionOutputs, result, ref sequence,
          ContributionKind.Interface, mixin.ToDisplayString(TypeDisplayFormat), targetDefinitions,
          libraries
        );
      }
    }
  }

  private static void CollectAttributeContributions(
    MixinGenerationContext context,
    INamedTypeSymbol target,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    IReadOnlyList<ImplicitMixinAttribute> implicitAttributes,
    ICollection<MixinContribution> result,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries
  ) {
    var sequence = 0;
    foreach (var annotated in AnnotatedSymbols(target)) {
      foreach (var applied in OrderedAttributes(annotated)) {
        var attributeType = applied.AttributeClass;
        if (attributeType is null) continue;
        foreach (var expressionAttribute in AnnotationDefinitions(attributeType, libraries)) {
          CollectMixinExpressionContributions(
            context, target, annotated, applied, null, expressionAttribute, compilation,
            preparedExpressions, expressionVariables, expressionOutputs, result, ref sequence,
            ContributionKind.Attribute, attributeType.ToDisplayString(TypeDisplayFormat), targetDefinitions,
            libraries
          );
        }
      }
    }
    foreach (var implicitAttribute in implicitAttributes) {
      CollectImplicitAttributeContributions(
        context, target, implicitAttribute, compilation, preparedExpressions,
        expressionVariables, expressionOutputs, result, ref sequence, targetDefinitions, libraries
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
    MixinGenerationContext context,
    INamedTypeSymbol target,
    ImplicitMixinAttribute implicitAttribute,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> result,
    ref int sequence,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries
  ) {
    foreach (var expressionAttribute in AnnotationDefinitions(implicitAttribute.Type, libraries)) {
      CollectMixinExpressionContributions(
        context, target, target, null, implicitAttribute, expressionAttribute, compilation,
        preparedExpressions, expressionVariables, expressionOutputs, result, ref sequence,
        ContributionKind.Attribute, implicitAttribute.Type.ToDisplayString(TypeDisplayFormat), targetDefinitions,
        libraries
      );
    }
  }

  private static void CollectMixinExpressionContributions(
    MixinGenerationContext context,
    INamedTypeSymbol target,
    ISymbol annotated,
    AttributeData applied,
    ImplicitMixinAttribute implicitAttribute,
    MixinAnnotationDefinition configuration,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> contributions,
    ref int sequence,
    ContributionKind contributionKind,
    string providerName,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries
  ) {
    var location = applied?.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated);
    var attributeName = providerName ?? applied?.AttributeClass?.Name ??
      implicitAttribute?.Type.Name ?? "<unknown>";
    IReadOnlyList<string> targets = Array.Empty<string>();
    IReadOnlyList<int> orders = Array.Empty<int>();
    var expression = configuration.Expression;
    var prelude = configuration.Prelude;
    var preludeModel = true;
    string lateExpression = null;

    var declarations = new Dictionary<string, AttributeExpressionTarget>(StringComparer.Ordinal);
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
      implicitAttribute?.Values, targetDefinitions, preparedExpressions, libraries
    );
    if (preludeModel) {
      if (!string.IsNullOrEmpty(lateExpression)) {
        ReportInvalidAttributeExpression(
          context, location, attributeName, annotated.Name,
          "Prelude and LateExpression cannot be used together"
        );
        return;
      }
      if (!MixinExpressionCompiler.TryHoistPrelude(
        prelude, expression, out expression, out lateExpression,
        out var hoistError, out var hoistLine
      )) {
        ReportInvalidAttributeExpression(
          context, location, attributeName, annotated.Name,
          "line " + hoistLine.ToString(CultureInfo.InvariantCulture) + ": " + hoistError
        );
        return;
      }
    } else MixinExpressionCompiler.HoistLateCarries(
      expression, lateExpression, out expression, out lateExpression
    );
    var interpreter = new MixinExpressionInterpreter();
    var evaluated = interpreter.Execute(
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
    if (!string.IsNullOrEmpty(lateExpression)) {
      var sourceType = (annotated as INamedTypeSymbol ?? annotated.ContainingType)
        ?.ToDisplayString(TypeDisplayFormat) ?? "";
      var lateTargets = declarations.Values.Select(item => new LateTarget(
        item.Target, item.EmittedTarget, item.IsStatic, item.IsPublic,
        item.DelegateTarget, item.Order
      )).ToList();
      DiscoverLateMixinTargets(
        lateExpression, evaluated.Variables, targetDefinitions, lateTargets
      );
      context.AddLateExpression(
        lateExpression, evaluated.Variables, evaluated.Outputs, preparedExpressions,
        lateTargets.Distinct().ToImmutableArray(),
        contributionKind, providerName ?? attributeName, sourceType,
        annotated is INamedTypeSymbol ? "" : annotated.MetadataName,
        annotated.Kind.ToString(), annotated is IMethodSymbol sourceMethod ? sourceMethod.Parameters.Length : 0
      );
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
      if (output.Target == MixinExpressionOutputTarget.Injection &&
        !declarations.ContainsKey(output.InjectionTarget)) continue;
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
            sequence++, targetDefinitions, result, providerName, annotated
          )
        );
        continue;
      }
      if (output.Target == MixinExpressionOutputTarget.Injection &&
        !declarations.ContainsKey(output.InjectionTarget)) {
        contributions.Add(
          new MixinContribution(
            output.InjectionTarget, 0, contributionKind,
            sequence++, targetDefinitions,
            new MixinExpressionResult(true, null, 0, new[] {
              new MixinExpressionOutput(MixinExpressionOutputTarget.Target, output.Text)
            }), providerName, annotated
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
          sequence++, targetDefinitions, result, providerName, annotated
        )
      );
    }
  }

  private static void DiscoverLateMixinTargets(
    string expression,
    IReadOnlyDictionary<string, object> variables,
    IReadOnlyDictionary<string, string> targetDefinitions,
    ICollection<LateTarget> targets
  ) {
    foreach (var line in MixinExpressionParser.SplitLines(expression ?? "")) {
      var instruction = MixinExpressionParser.ParseDirective(line, 0);
      if (instruction.Error is not null || instruction.Opcode != DirectiveOpcode.Mixin ||
        instruction.Arguments.Count == 0) continue;
      var argument = instruction.Arguments[0];
      string resolved = null;
      if (!MixinExpressionParser.IsDynamicArgument(argument)) resolved = argument;
      else {
        var inner = argument.Substring(1, argument.Length - 2).Trim();
        const string carryPrefix = "@carry#";
        if (inner.StartsWith(carryPrefix, StringComparison.Ordinal) &&
          variables.TryGetValue(
            MixinExpressionInterpreter.CarryLocalPrefix + inner.Substring(carryPrefix.Length),
            out var carried
          )) resolved = MixinValue.From(carried).Render();
      }
      if (string.IsNullOrWhiteSpace(resolved)) continue;
      var syntax = RoslynMixinExpressionContext.ParseMixinTarget(resolved, targetDefinitions);
      targets.Add(new LateTarget(
        resolved, syntax.Name, syntax.IsStatic, syntax.IsPublic, syntax.DelegateType, 0
      ));
    }
  }

  private static void ReportInvalidAttributeExpression(
    MixinGenerationContext context,
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
    MixinGenerationContext context,
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
    MixinGenerationContext context,
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

  private static void ReportContributionData(
    MixinGenerationContext context,
    INamedTypeSymbol target,
    IReadOnlyList<GeneratedMethod> methods
  ) {
    var targetName = target.ToDisplayString(TypeDisplayFormat);
    var location = LocationOf(target);
    foreach (var method in methods) {
      foreach (var contribution in method.Contributions) {
        if (contribution.IsPlaceholder) continue;
        var properties = ImmutableDictionary<string, string>.Empty
          .Add("Target", targetName)
          .Add("Method", method.Name)
          .Add("Mixin", contribution.Provider)
          .Add("Priority", contribution.Order.ToString(CultureInfo.InvariantCulture))
          .Add("SourceType", contribution.SourceType)
          .Add("SourceMember", contribution.SourceMember)
          .Add("SourceKind", contribution.SourceKind)
          .Add("SourceParameterCount", contribution.SourceParameterCount.ToString(CultureInfo.InvariantCulture));
        context.ReportDiagnostic(
          Diagnostic.Create(
            ContributionData, location, properties,
            targetName, method.Name, contribution.Provider,
            contribution.Order.ToString(CultureInfo.InvariantCulture),
            contribution.SourceType, contribution.SourceMember, contribution.SourceKind,
            contribution.SourceParameterCount.ToString(CultureInfo.InvariantCulture)
          )
        );
      }
    }
  }

  private static bool TryBuildMethod(
    MixinGenerationContext context,
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
    MixinGenerationContext context,
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
