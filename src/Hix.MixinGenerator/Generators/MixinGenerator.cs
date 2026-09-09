using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Hix;
using Hix.Compiler;
using Hix.Diagnostics;
using Hix.Env;
using Hix.Roslyn;
using Hix.Runtime;

using static Hix.Roslyn.GeneratorAnalysis;
using static Hix.Roslyn.GeneratorDiagnostics.Mixins;
using static Hix.Roslyn.GeneratorSource;
using static Hix.Roslyn.GeneratorStrings;
using HixExecutionContext = Hix.Runtime.HixExecutionContext;

namespace HelixSourceGenerator.Generators;

[Generator(LanguageNames.CSharp)]
public sealed partial class MixinGenerator : IIncrementalGenerator {
  private const string LateClassMarker = "// __HELIX_LATE_CLASS__";
  private const string LateFileMarker = "// __HELIX_LATE_FILE__";
  private static long _generationCounter;

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    using var profile = HixProfiler.Measure("generator.initialize");
    var mixinCompilation = context.AdditionalTextsProvider
      .Select(MixinLibraryApi.ReadAdditionalFile)
      .Collect()
      .Select(static (files, _) => new MixinLibraryCatalog(files))
      .WithComparer(HixLibraryCatalogComparer.Instance) // Just to be sure, we'll also try caching before
      .Select(static (catalog, _) => MixinLibraryApi.CompileCached(catalog))
      .WithComparer(HixCompilationComparer.Instance)
      .WithTrackingName("Mixin.Compilation");

    context.RegisterSourceOutput(
      mixinCompilation,
      static (spc, compilation) => {
        foreach (var diagnostic in compilation.Diagnostics) spc.ReportDiagnostic(diagnostic);
        HixProfiler.ScheduleFlush();
      }
    );

    foreach (var attribute in MixinGeneratorCandidates.AttributeMetadataNames) {
      var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
        attribute,
        static (node, _) => IsCandidateSyntax(node),
        static (ctx, _) => GetTarget(ctx)
      ).Where(static target => target is not null);

      var primary = targets.Combine(mixinCompilation)
        .Select(static (item, _) => Generate(item.Left, item.Right))
        .WithComparer(HixOutputModelComparer.Instance)
        .WithTrackingName("Mixin.Prelude");

      var finalized = primary
        .Select(static (model, _) => EvaluateLate(model))
        .WithComparer(HixOutputModelComparer.Instance)
        .WithTrackingName("Mixin.Evaluation");

      context.RegisterSourceOutput(finalized, static (spc, model) => EmitSource(spc, model));
      context.RegisterSourceOutput(finalized.Collect(), static (_, _) => HixProfiler.Flush());
    }
  }

  private static bool IsCandidateSyntax(SyntaxNode node) {
    using var profile = HixProfiler.Measure("generator.syntax_predicate");
    return node is TypeDeclarationSyntax {
      RawKind: (int)SyntaxKind.ClassDeclaration or (int)SyntaxKind.RecordDeclaration or
      (int)SyntaxKind.StructDeclaration or (int)SyntaxKind.RecordStructDeclaration
    };
  }

  private static HixTarget GetTarget(GeneratorAttributeSyntaxContext context) {
    using var profile = HixProfiler.Measure("generator.get_target");
    if (context.TargetSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } type)
      return null;
    var matchedCandidate = context.Attributes.FirstOrDefault()?.AttributeClass?.ToDisplayString();
    var canonicalCandidate = MixinGeneratorCandidates.AttributeMetadataNames.FirstOrDefault(candidate =>
      type.GetAttributes().Any(attribute => IsAttribute(attribute, candidate))
    );

    if (matchedCandidate != canonicalCandidate) return null;
    return context.SemanticModel.Compilation is CSharpCompilation compilation
      ? new HixTarget(type, compilation)
      : null;
  }

  private static IReadOnlyDictionary<string, string> TargetDefinitions(
    INamedTypeSymbol type,
    IEnumerable<INamedTypeSymbol> providers,
    MixinLibraryCatalog libraries,
    bool component
  ) {
    var definitions = new Dictionary<string, string>(StringComparer.Ordinal) {
      ["$Init"] = component ? "^LoadComponent" : "Awake", ["$Dispose"] = component ? "^UnloadComponent" : "OnDestroy"
    };
    var seenProviders = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    foreach (var provider in providers.Where(item => item is not null)) {
      if (!seenProviders.Add(provider)) continue;
      if (SymbolEqualityComparer.Default.Equals(provider, type)) continue;
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
  ) {
    return MixinLibraryApi.Annotations(type, libraries);
  }

  private static HixOutputModel Generate(
    HixTarget candidate,
    MixinCompilation mixinCompilation
  ) {
    using var profile = HixProfiler.Measure("generator.generate.total");
    var libraries = mixinCompilation.Catalog;
    var context = new HixGenerationContext(
      libraries.HasConfiguration("DEBUG"),
      libraries.HasConfigurationOption("DEBUG", "StringPool")
    );
    var target = candidate.Type;
    var location = LocationOf(target);
    if (!IsPartial(target)) {
      context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, target.Name));
      return context.Complete();
    }

    var containing = FirstNonPartialContainingType(target);
    if (containing is not null) {
      context.ReportDiagnostic(Diagnostic.Create(ContainingTypeMustBePartial, location, target.Name, containing.Name));
      return context.Complete();
    }

    var preparedExpressions = mixinCompilation.PreparedState;
    AnnotatedSymbolData[] annotatedSymbols;
    using (HixProfiler.Measure("generator.collect_symbols")) {
      annotatedSymbols = [
        .. AnnotatedSymbols(target).Select(symbol =>
          new AnnotatedSymbolData(symbol, OrderedAttributes(symbol))
        )
      ];
    }
    var targetAttributes = annotatedSymbols[0].Attributes;
    var annotationProviders = annotatedSymbols.SelectMany(item => item.Attributes)
      .Select(attribute => attribute.AttributeClass).Where(type => type is not null)
      .Prepend(target);
    IReadOnlyDictionary<string, string> targetDefinitions;
    using (HixProfiler.Measure("generator.target_definitions")) {
      targetDefinitions = TargetDefinitions(
        target, annotationProviders, libraries,
        targetAttributes.Any(attribute => IsAttribute(attribute, GeneratorStrings.Attributes.Managed))
      );
    }
    var contributions = new List<HixContribution>();
    var attributeExpressionOutputs = new List<HixExpressionOutput>();
    var expressionVariables = new Dictionary<string, object>(StringComparer.Ordinal);
    var hostValues = new RoslynHostExpressionCache();
    using (HixProfiler.Measure("generator.collect_contributions")) {
      CollectAttributeContributions(
        context, target, candidate.Compilation, preparedExpressions, expressionVariables,
        attributeExpressionOutputs, contributions, targetDefinitions, libraries,
        mixinCompilation, annotatedSymbols, hostValues,
        targetAttributes.Any(attribute => IsAttribute(attribute, GeneratorStrings.Attributes.Structure))
      );
    }
    var placeholderSequence = contributions.Count;
    foreach (var work in context.LateExpressions) {
      contributions.AddRange(
        work.Targets.Select(lateTarget => HixContribution.Placeholder(lateTarget, placeholderSequence++, work))
      );
    }
    List<GeneratedMethod> methods;
    using (HixProfiler.Measure("generator.build_methods"))
      methods = BuildMethods(context, target, contributions, candidate.Compilation);
    ReportContributionData(context, target, methods);
    var outputs = new ExpressionOutputs();
    foreach (var output in contributions
      .Where(contribution => contribution.ExpressionResult is not null)
      .SelectMany(contribution => contribution.ExpressionResult.Outputs)) outputs.Add(output);
    foreach (var output in attributeExpressionOutputs) outputs.Add(output);
    if (methods.Count == 0 && !outputs.Any && !context.HasLateExpressions && context.DebugExpressions.Count == 0)
      return context.Complete();
    var finalizedOutputs = outputs.Finish();
    var wrapper = WrapType(target, "mixins");
    var result = context.Complete(
      new HixRenderModel(
        wrapper.Detach(), methods.ToImmutableArray(),
        finalizedOutputs.Annotations, finalizedOutputs.Class,
        finalizedOutputs.File, finalizedOutputs.Implements,
        finalizedOutputs.Usings, expressionVariables,
        preparedExpressions.StringPool,
        hostValues.TargetVariableFingerprintValues(preparedExpressions.StringPool),
        context.DebugExpressions.ToImmutableArray(), context.Debug, context.DebugStringPool
      )
    );
    return result;
  }

  private static void EmitSource(SourceProductionContext context, HixOutputModel model) {
    using var profile = HixProfiler.Measure("generator.emit_source");
    EmitModelDiagnostics(context, model);
    if (model.Source is not null) context.AddSource(model.HintName, model.Source);
    HixProfiler.ScheduleFlush();
  }

  private static void EmitModelDiagnostics(SourceProductionContext context, HixOutputModel model) {
    foreach (var diagnostic in model.Diagnostics) context.ReportDiagnostic(diagnostic.Create());
    foreach (var log in model.Logs) {
      context.ReportDiagnostic(
        log.Location.Create(log.Log.IsHint ? ExpressionHint : ExpressionLog, log.Log.Text)
      );
    }
    foreach (var error in model.Errors) {
      context.ReportDiagnostic(
        Diagnostic.Create(InvalidAttributeExpression, Location.None, "late expression", "late evaluation", error)
      );
    }
  }

  private static HixOutputModel EvaluateLate(HixOutputModel model) {
    using var profile = HixProfiler.Measure("generator.evaluate_late.total");
    if (model.Render is null) return model;
    var classCode = model.Render.Class.ToList();
    var fileCode = model.Render.File.ToList();
    var usings = model.Render.Usings.ToList();
    var implements = model.Render.Implements.ToList();
    var annotations = model.Render.Annotations.ToList();
    var errors = new List<string>();
    var logs = new List<HixReportedLog>();
    var lateContributions = new List<HixContribution>();
    var lateSequence = 1_000_000;
    var finalDebugStates = model.Render.DebugExpressions
      .GroupBy(HixDebugRenderer.StateKey, StringComparer.Ordinal)
      .ToDictionary(
        group => group.Key,
        group => new HixDebugFinalState(group.Last(), 0, 0),
        StringComparer.Ordinal
      );
    var unlinkedContext = new UnlinkedHixExpressionContext(model.Render.StringPool);
    var sharedVariables = model.Render.PrimaryVariables.ToDictionary(
      item => item.Key.Resolve(model.Render.StringPool),
      item => item.Value.Unlink(unlinkedContext), StringComparer.Ordinal
    );
    foreach (var work in model.LateExpressions) {
      var variables = work.Variables.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
      foreach (var item in sharedVariables) variables[item.Key] = item.Value;
      var result = HixVM.Execute(work.Program, unlinkedContext, variables, work.Carries);
      foreach (var log in result.Logs)
        logs.Add(new HixReportedLog(log, work.Location));
      if (!result.Success) {
        errors.Add(
          work.Provider + " on " + work.SourceType + "." + work.SourceMember + ", line " +
          result.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + result.Error
        );
        continue;
      }
      var debugStateKey = DebugStateKey(work);
      if (finalDebugStates.TryGetValue(debugStateKey, out var debugState)) {
        finalDebugStates[debugStateKey] = new HixDebugFinalState(
          debugState.Work, result.ExecutedOperations, result.ExecutionMilliseconds
        );
      }
      foreach (var item in variables) sharedVariables[item.Key] = item.Value;
      foreach (var output in result.Outputs) {
        switch (output.Target) {
          case HixEmissionTarget.Class: classCode.Add(output); break;
          case HixEmissionTarget.File: fileCode.Add(output); break;
          case HixEmissionTarget.Using:
            usings.Add(output); break;
          case HixEmissionTarget.Extends or HixEmissionTarget.Implements:
            implements.Add(output); break;
          case HixEmissionTarget.Annotation: annotations.Add(output); break;
          case HixEmissionTarget.Target:
            if (work.Targets.Length != 1) {
              errors.Add("@CODE<TARGET> requires exactly one declared target");
              break;
            }
            lateContributions.Add(
              LateContribution(work.Targets[0], work.Targets[0].Order, output, work, lateSequence++)
            );
            break;
          case HixEmissionTarget.Injection: {
            var target = work.Targets.FirstOrDefault(item =>
              item.DeclaredTarget == output.InjectionTarget || item.EmittedTarget == output.InjectionTarget
            );
            if (target is null) errors.Add("late code target '" + output.InjectionTarget + "' was not declared");
            else lateContributions.Add(LateContribution(target, target.Order, output, work, lateSequence++));
            break;
          }
          case HixEmissionTarget.Mixin: {
            var target = work.Targets.FirstOrDefault(item =>
              item.DeclaredTarget == output.InjectionTarget || item.EmittedTarget == output.InjectionTarget
            );
            if (target is null)
              errors.Add("late mixin target '" + output.InjectionTarget + "' was not prepared by the Prelude");
            else {
              lateContributions.Add(
                LateContribution(
                  target, output.InjectionPriority, output, work, lateSequence++
                )
              );
            }
            break;
          }
          default:
            errors.Add("late expression output '" + output.Target + "' requires a detached method contribution"); break;
        }
      }
    }
    var methods = model.Render.Methods.Select(method => {
        var retained = method.Contributions.Where(item => !item.IsPlaceholder).ToList();
        retained.AddRange(
          lateContributions.Where(item =>
            item.EmittedTarget == method.Name &&
            item.IsStaticTarget == method.Contributions.First().IsStaticTarget
          )
        );
        return retained.Count == 0
          ? null
          : method with {
            Contributions = [
              .. retained.OrderBy(item => item.Order)
                .ThenBy(item => item.Sequence)
            ]
          };
      }
    ).Where(item => item is not null).ToImmutableArray();
    string source;
    using (HixProfiler.Measure("generator.evaluate_late.render")) {
      source = model.Render.Wrapper.Build(
        [
          .. usings.Select(item => NormalizeUsing(item.Text)).Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
        ],
        [
          .. implements.OrderBy(item => item.Target == HixEmissionTarget.Extends ? 0 : 1)
            .Select(item => item.Text.Trim()).Distinct(StringComparer.Ordinal)
        ],
        [.. annotations.Select(item => item.Text.Trim()).Distinct(StringComparer.Ordinal)],
        builder => {
          for (var index = 0; index < methods.Length; index++) {
            AppendMethod(builder, methods[index]);
            if (index != methods.Length - 1) builder.BlankLine();
          }
          foreach (var output in classCode) {
            if (methods.Length != 0) builder.BlankLine();
            builder.AppendCode(output.Text);
          }
        }, builder => {
          foreach (var output in fileCode) builder.AppendCode(output.Text);
        }
      );
    }
    if (model.Render.Debug) {
      source = BuildDebugTrace(model.Render) + source +
        BuildFinalDebugState(finalDebugStates.Values, sharedVariables);
    }
    var finalModel = new HixOutputModel(
      model.Render.Wrapper.HintName, source, null, ImmutableArray<LateExpressionWork>.Empty,
      model.Diagnostics, errors.ToImmutableArray(), logs.ToImmutableArray()
    );
    return finalModel;
  }

  private static string NormalizeUsing(string text) {
    text = (text ?? "").Trim().TrimEnd(';');
    return text.Length == 0 ? "" : "using " + text + ";";
  }

  private static HixContribution LateContribution(
    LateTarget target, int order, HixExpressionOutput output, LateExpressionWork work, int sequence
  ) {
    return new HixContribution(
      target, order, sequence,
      new HixExpressionResult(
        true, null, 0, [output.Retarget(HixEmissionTarget.Target)]
      ),
      work
    );
  }

  private static void CollectAttributeContributions(
    HixGenerationContext context,
    INamedTypeSymbol target,
    CSharpCompilation compilation,
    HixExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<HixExpressionOutput> expressionOutputs,
    ICollection<HixContribution> result,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries,
    MixinCompilation mixinCompilation,
    IReadOnlyList<AnnotatedSymbolData> annotatedSymbols,
    RoslynHostExpressionCache hostValues,
    bool structureTarget
  ) {
    using var profile = HixProfiler.Measure("generator.roslyn.attribute_contributions");
    var sequence = 0;
    structureTarget &= target.TypeKind == TypeKind.Struct;
    foreach (var annotatedData in annotatedSymbols) {
      var annotated = annotatedData.Symbol;
      foreach (var applied in annotatedData.Attributes) {
        var attributeType = applied.AttributeClass;
        if (attributeType is null) continue;
        if (structureTarget && attributeType.ToDisplayString() != GeneratorStrings.Attributes.Structure) continue;
        var providerName = attributeType.ToDisplayString(TypeDisplayFormat);
        if (!mixinCompilation.TryGetAnnotation(providerName, out var expressionAttribute)) continue;
        CollectHixExpressionContributions(
          context, target, annotated, applied, expressionAttribute, compilation,
          preparedExpressions, expressionVariables, expressionOutputs, result, ref sequence,
          providerName, targetDefinitions, libraries, hostValues, mixinCompilation
        );
      }
    }
  }

  private static IEnumerable<ISymbol> AnnotatedSymbols(INamedTypeSymbol target) {
    yield return target;
    foreach (var member in OrderedMembers(target)) {
      if (!member.IsImplicitlyDeclared)
        yield return member;
    }
  }

  private static void CollectHixExpressionContributions(
    HixGenerationContext context,
    INamedTypeSymbol target,
    ISymbol annotated,
    AttributeData applied,
    CompiledHixAnnotation configuration,
    CSharpCompilation compilation,
    HixExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<HixExpressionOutput> expressionOutputs,
    ICollection<HixContribution> contributions,
    ref int sequence,
    string providerName,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries,
    RoslynHostExpressionCache hostValues,
    MixinCompilation mixinCompilation
  ) {
    var location = applied?.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated);
    var attributeName = providerName ?? applied?.AttributeClass?.Name ?? "<unknown>";
    var selectedProgram = configuration.Program;
    var expression = selectedProgram.Prelude;
    var lateExpression = selectedProgram.Late;

    var expressionContext = HixMixinBackend.Instance.CreateContext(target, annotated, applied, compilation, targetDefinitions, preparedExpressions, hostValues);
    var evaluated = HixVM.Execute(expression, expressionContext, expressionVariables);
    ReportExpressionLogs(context, location, evaluated.Logs);
    if (!evaluated.Success) {
      ReportInvalidAttributeExpression(
        context, location, attributeName, annotated.Name,
        "line " + evaluated.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + evaluated.Error
      );
      return;
    }
    expressionContext.CommitTargetVariables();
    context.AddDebugExpression(
      selectedProgram.Prelude, selectedProgram.Late,
      evaluated.Variables, evaluated.Carries,
      providerName ?? attributeName, annotated, evaluated.ExecutedOperations,
      evaluated.ExecutionMilliseconds
    );
    if (lateExpression.Expressions.Count != 0) {
      var sourceType = (annotated as INamedTypeSymbol ?? annotated.ContainingType)
        ?.ToDisplayString(TypeDisplayFormat) ?? "";
      var lateTargets = new List<LateTarget>();
      DiscoverLateHixTargets(
        lateExpression, evaluated.Carries, targetDefinitions, lateTargets
      );
      context.AddLateExpression(
        lateExpression, selectedProgram.Prelude.Identity, selectedProgram.Late.Identity,
        evaluated.Variables, evaluated.Carries,
        lateTargets.Distinct().ToImmutableArray(), location,
        providerName ?? attributeName, sourceType,
        annotated is INamedTypeSymbol ? "" : annotated.MetadataName,
        annotated.Kind.ToString(),
        annotated is IMethodSymbol sourceMethod ? sourceMethod.Parameters.Length : 0
      );
    }

    // Validate every destination before publishing any output from this expression.
    foreach (var output in evaluated.Outputs) {
      if (output.Target == HixEmissionTarget.Mixin) {
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
      if (output.Target is HixEmissionTarget.Injection or HixEmissionTarget.Class or
        HixEmissionTarget.File or HixEmissionTarget.Extends or HixEmissionTarget.Implements or
        HixEmissionTarget.Annotation or HixEmissionTarget.Using) continue;
      ReportInvalidAttributeExpression(context, location, attributeName, annotated.Name,
        "output requires an explicit injection target");
      return;
    }

    foreach (var output in evaluated.Outputs) {
      if (output.IsEmpty) continue;
      switch (output.Target) {
        case HixEmissionTarget.Mixin: {
          var result = new HixExpressionResult(
            true, null, 0, [output.Retarget(HixEmissionTarget.Target)]
          );
          contributions.Add(
            new HixContribution(
              output.InjectionTarget, output.InjectionPriority,
              sequence++, targetDefinitions, result, providerName, annotated
            )
          );
          continue;
        }
        case HixEmissionTarget.Injection:
          contributions.Add(
            new HixContribution(
              output.InjectionTarget, 0, sequence++, targetDefinitions,
              new HixExpressionResult(true, null, 0, [output.Retarget(HixEmissionTarget.Target)]),
              providerName, annotated
            )
          );
          continue;
        case HixEmissionTarget.Class or
          HixEmissionTarget.File or HixEmissionTarget.Extends or
          HixEmissionTarget.Implements or
          HixEmissionTarget.Annotation or HixEmissionTarget.Using:
          expressionOutputs.Add(output);
          continue;
      }
    }
  }

  private static void DiscoverLateHixTargets(
    HixExpressionExecutionProgram program,
    IReadOnlyDictionary<string, object> carries,
    IReadOnlyDictionary<string, string> targetDefinitions,
    ICollection<LateTarget> targets
  ) {
    foreach (var target in program.LateTargets) {
      string resolved = target.IsCarry
        ? carries.TryGetValue(target.Value, out var carried) ? Convert.ToString(carried, CultureInfo.InvariantCulture) : null
        : target.Value;
      if (string.IsNullOrWhiteSpace(resolved)) continue;
      var syntax = HixMixinContext.ParseMixinTarget(resolved, targetDefinitions);
      targets.Add(new LateTarget(resolved, syntax.Name, syntax.IsStatic, syntax.IsPublic, syntax.DelegateType, 0));
    }
  }

  private static void ReportInvalidAttributeExpression(
    HixGenerationContext context,
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
    HixGenerationContext context,
    Location location,
    IReadOnlyList<HixExpressionLog> logs
  ) {
    foreach (var log in logs) {
      context.ReportDiagnostic(
        Diagnostic.Create(log.IsHint ? ExpressionHint : ExpressionLog, location, log.Text)
      );
    }
  }


  private static string EmittedTarget(
    string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    return HixMixinContext.ParseMixinTarget(target, targetDefinitions).Name;
  }

  private static IReadOnlyList<ISymbol> OrderedMembers(INamedTypeSymbol type) {
    using var profile = HixProfiler.Measure("generator.roslyn.ordered_members");
    ImmutableArray<ISymbol> members;
    using (HixProfiler.Measure("generator.roslyn.ordered_members.get"))
      members = type.GetMembers();
    using (HixProfiler.Measure("generator.roslyn.ordered_members.sort"))
      return [
        .. members
          .OrderBy(
            item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceTree?.FilePath,
            StringComparer.Ordinal
          )
          .ThenBy(SourceOrder)
      ];
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) {
    using var profile = HixProfiler.Measure("generator.roslyn.ordered_attributes");
    var attributes = AttributeList(symbol);

    using (HixProfiler.Measure("generator.roslyn.ordered_attributes.sort"))
      return [
        .. attributes
          .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
          .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
      ];
  }

  private static bool IsAttribute(AttributeData attribute, string metadataName) {
    using var profile = HixProfiler.Measure("generator.roslyn.is_attribute");
    return attribute.AttributeClass?.ToDisplayString() == metadataName;
  }

}
