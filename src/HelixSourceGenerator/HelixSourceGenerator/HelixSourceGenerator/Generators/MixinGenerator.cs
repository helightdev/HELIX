using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using HelixSourceGenerator.Language;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static HelixSourceGenerator.Shared.GeneratorAnalysis;
using static HelixSourceGenerator.Shared.GeneratorDiagnostics.Mixins;
using static HelixSourceGenerator.Shared.GeneratorSource;
using static HelixSourceGenerator.Shared.GeneratorStrings;
using ExecutionContext = HelixSourceGenerator.Language.ExecutionContext;
using IMixinValue = HelixSourceGenerator.Language.IMixinValue;

namespace HelixSourceGenerator.Generators;

[Generator(LanguageNames.CSharp)]
public sealed partial class MixinGenerator : IIncrementalGenerator {
  private const string LateClassMarker = "// __HELIX_LATE_CLASS__";
  private const string LateFileMarker = "// __HELIX_LATE_FILE__";
  private static long _generationCounter;

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var mixinCompilation = context.AdditionalTextsProvider
      .Select(MixinLibraryApi.ReadAdditionalFile)
      .Collect()
      .Select(static (files, _) => new MixinLibraryCatalog(files))
      .WithComparer(MixinLibraryCatalogComparer.Instance) // Just to be sure, we'll also try caching before
      .Select(static (catalog, _) => MixinLibraryApi.CompileCached(catalog))
      .WithComparer(MixinCompilationComparer.Instance)
      .WithTrackingName("Mixin.Compilation");

    context.RegisterSourceOutput(
      mixinCompilation,
      static (spc, compilation) => {
        foreach (var diagnostic in compilation.Diagnostics) spc.ReportDiagnostic(diagnostic);
      }
    );

    foreach (var attribute in MixinGeneratorCandidates.AttributeMetadataNames) {
      var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
        attribute,
        static (node, _) => node is TypeDeclarationSyntax {
          RawKind: (int)SyntaxKind.ClassDeclaration or (int)SyntaxKind.RecordDeclaration or
          (int)SyntaxKind.StructDeclaration or (int)SyntaxKind.RecordStructDeclaration
        },
        static (ctx, _) => GetTarget(ctx)
      ).Where(static target => target is not null);

      var primary = targets.Combine(mixinCompilation)
        .Select(static (item, _) => Generate(item.Left, item.Right).Output)
        .WithComparer(MixinOutputModelComparer.Instance)
        .WithTrackingName("Mixin.Prelude");

      context.RegisterSourceOutput(primary, static (spc, model) => EmitDiagnostics(spc, model.Diagnostics));
      var finalized = primary
        .Select(static (model, _) => EvaluateLate(model))
        .WithComparer(MixinOutputModelComparer.Instance)
        .WithTrackingName("Mixin.Evaluation");

      context.RegisterSourceOutput(finalized, static (spc, model) => EmitSource(spc, model));
    }
  }

  private static MixinTarget GetTarget(GeneratorAttributeSyntaxContext context) {
    using var profile = MixinProfiler.Measure("generator.get_target");
    if (context.TargetSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } type)
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

  private static MixinGenerationResult Generate(
    MixinTarget candidate,
    MixinCompilation mixinCompilation
  ) {
    using var profile = MixinProfiler.Measure("generator.generate.total");
    var libraries = mixinCompilation.Catalog;
    var context = new MixinGenerationContext(
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
    using (MixinProfiler.Measure("generator.collect_symbols")) {
      annotatedSymbols = AnnotatedSymbols(target).Select(symbol =>
        new AnnotatedSymbolData(symbol, OrderedAttributes(symbol))
      ).ToArray();
    }
    var targetAttributes = annotatedSymbols[0].Attributes;
    var annotationProviders = annotatedSymbols.SelectMany(item => item.Attributes)
      .Select(attribute => attribute.AttributeClass).Where(type => type is not null)
      .Prepend(target);
    IReadOnlyDictionary<string, string> targetDefinitions;
    using (MixinProfiler.Measure("generator.target_definitions")) {
      targetDefinitions = TargetDefinitions(
        target, annotationProviders, libraries,
        targetAttributes.Any(attribute => IsAttribute(attribute, Attributes.Managed))
      );
    }
    var contributions = new List<MixinContribution>();
    var attributeExpressionOutputs = new List<MixinExpressionOutput>();
    var expressionVariables = new Dictionary<string, object>(StringComparer.Ordinal);
    using (MixinProfiler.Measure("generator.collect_contributions")) {
      CollectAttributeContributions(
        context, target, candidate.Compilation, preparedExpressions, expressionVariables,
        attributeExpressionOutputs, contributions, targetDefinitions, libraries,
        mixinCompilation, annotatedSymbols,
        targetAttributes.Any(attribute => IsAttribute(attribute, Attributes.Structure))
      );
    }
    var placeholderSequence = contributions.Count;
    foreach (var work in context.LateExpressions) {
      contributions.AddRange(
        work.Targets.Select(lateTarget => MixinContribution.Placeholder(lateTarget, placeholderSequence++, work))
      );
    }
    List<GeneratedMethod> methods;
    using (MixinProfiler.Measure("generator.build_methods"))
      methods = BuildMethods(context, target, contributions, candidate.Compilation);
    ReportContributionData(context, target, methods);
    var outputs = new ExpressionOutputs();
    foreach (var output in contributions
      .Where(contribution => contribution.ExpressionResult is not null)
      .SelectMany(contribution => contribution.ExpressionResult.Outputs)) outputs.Add(output);
    foreach (var output in attributeExpressionOutputs) outputs.Add(output);
    if (methods.Count == 0 && !outputs.Any && !context.HasLateExpressions) return context.Complete();
    var finalizedOutputs = outputs.Finish();
    var wrapper = WrapType(target, "mixins");
    var result = context.Complete(
      new MixinRenderModel(
        wrapper.Detach(), methods.ToImmutableArray(),
        finalizedOutputs.Annotations, finalizedOutputs.Class,
        finalizedOutputs.File, finalizedOutputs.Implements,
        finalizedOutputs.Usings, expressionVariables
          .Where(item => !item.Key.StartsWith(
              MixinExpressionVirtualMachine.CarryLocalPrefix, StringComparison.Ordinal
            )
          ),
        preparedExpressions.StringPool.Fork(),
        context.DebugExpressions.ToImmutableArray(), context.Debug, context.DebugStringPool
      )
    );
    return result;
  }

  private static void EmitSource(SourceProductionContext context, MixinOutputModel model) {
    using var profile = MixinProfiler.Measure("generator.emit_source");
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
    if (model.Source is not null) context.AddSource(model.HintName, model.Source);
    MixinProfiler.Flush();
  }

  private static void EmitDiagnostics(
    SourceProductionContext context,
    ImmutableArray<MixinDiagnostic> diagnostics
  ) {
    foreach (var diagnostic in diagnostics) context.ReportDiagnostic(diagnostic.Create());
  }

  private static MixinOutputModel EvaluateLate(MixinOutputModel model) {
    using var profile = MixinProfiler.Measure("generator.evaluate_late.total");
    if (model.Render is null) return model;
    var classCode = model.Render.Class.ToList();
    var fileCode = model.Render.File.ToList();
    var usings = model.Render.Usings.ToList();
    var implements = model.Render.Implements.ToList();
    var annotations = model.Render.Annotations.ToList();
    var errors = new List<string>();
    var logs = new List<MixinReportedLog>();
    var lateContributions = new List<MixinContribution>();
    var lateSequence = 1_000_000;
    var finalDebugStates = model.Render.DebugExpressions
      .GroupBy(DebugStateKey, StringComparer.Ordinal)
      .ToDictionary(
        group => group.Key,
        group => new FinalDebugState(group.Last(), 0, 0),
        StringComparer.Ordinal
      );
    var unlinkedContext = new UnlinkedMixinExpressionContext(model.Render.StringPool);
    var sharedVariables = model.Render.PrimaryVariables.ToDictionary(
      item => item.Key.Resolve(model.Render.StringPool),
      item => item.Value.Unlink(unlinkedContext), StringComparer.Ordinal
    );
    foreach (var work in model.LateExpressions) {
      var variables = work.Variables.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
      foreach (var item in sharedVariables) variables[item.Key] = item.Value;
      var result = MixinExpressionVirtualMachine.Execute(
        work.Program, unlinkedContext, variables
      );
      foreach (var log in result.Logs)
        logs.Add(new MixinReportedLog(log, work.Location));
      if (!result.Success) {
        errors.Add("line " + result.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + result.Error);
        continue;
      }
      var debugStateKey = DebugStateKey(work);
      if (finalDebugStates.TryGetValue(debugStateKey, out var debugState)) {
        finalDebugStates[debugStateKey] = new FinalDebugState(
          debugState.Work, result.ExecutedOperations, result.ExecutionMilliseconds
        );
      }
      foreach (var item in variables
        .Where(static item => !item.Key.StartsWith(
            MixinExpressionVirtualMachine.CarryLocalPrefix, StringComparison.Ordinal
          )
        )) sharedVariables[item.Key] = item.Value;
      foreach (var output in result.Outputs) {
        switch (output.Target) {
          case MixinExpressionOutputTarget.Class: classCode.Add(output); break;
          case MixinExpressionOutputTarget.File: fileCode.Add(output); break;
          case MixinExpressionOutputTarget.Using:
            usings.Add(output); break;
          case MixinExpressionOutputTarget.Extends or MixinExpressionOutputTarget.Implements:
            implements.Add(output); break;
          case MixinExpressionOutputTarget.Annotation: annotations.Add(output); break;
          case MixinExpressionOutputTarget.Target:
            if (work.Targets.Length != 1) {
              errors.Add("@CODE<TARGET> requires exactly one declared target");
              break;
            }
            lateContributions.Add(
              LateContribution(work.Targets[0], work.Targets[0].Order, output, work, lateSequence++)
            );
            break;
          case MixinExpressionOutputTarget.Injection: {
            var target = work.Targets.FirstOrDefault(item =>
              item.DeclaredTarget == output.InjectionTarget || item.EmittedTarget == output.InjectionTarget
            );
            if (target is null) errors.Add("late code target '" + output.InjectionTarget + "' was not declared");
            else lateContributions.Add(LateContribution(target, target.Order, output, work, lateSequence++));
            break;
          }
          case MixinExpressionOutputTarget.Mixin: {
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
    using (MixinProfiler.Measure("generator.evaluate_late.render")) {
      source = model.Render.Wrapper.Build(
        [
          .. usings.Select(item => NormalizeUsing(item.Text)).Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
        ],
        [
          .. implements.OrderBy(item => item.Target == MixinExpressionOutputTarget.Extends ? 0 : 1)
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
    var finalModel = new MixinOutputModel(
      model.Render.Wrapper.HintName, source, null, ImmutableArray<LateExpressionWork>.Empty,
      model.Diagnostics, errors.ToImmutableArray(), logs.ToImmutableArray()
    );
    return finalModel;
  }

  private static string NormalizeUsing(string text) {
    text = (text ?? "").Trim().TrimEnd(';');
    return text.Length == 0 ? "" : "using " + text + ";";
  }

  private static MixinContribution LateContribution(
    LateTarget target, int order, MixinExpressionOutput output, LateExpressionWork work, int sequence
  ) {
    return new MixinContribution(
      target, order, sequence,
      new MixinExpressionResult(
        true, null, 0, [output.Retarget(MixinExpressionOutputTarget.Target)]
      ),
      work
    );
  }

  private static void CollectAttributeContributions(
    MixinGenerationContext context,
    INamedTypeSymbol target,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> result,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries,
    MixinCompilation mixinCompilation,
    IReadOnlyList<AnnotatedSymbolData> annotatedSymbols,
    bool structureTarget
  ) {
    var sequence = 0;
    var hostValues = new RoslynHostExpressionCache();
    structureTarget &= target.TypeKind == TypeKind.Struct;
    foreach (var annotatedData in annotatedSymbols) {
      var annotated = annotatedData.Symbol;
      foreach (var applied in annotatedData.Attributes) {
        var attributeType = applied.AttributeClass;
        if (attributeType is null) continue;
        if (structureTarget && attributeType.ToDisplayString() != Attributes.Structure) continue;
        var providerName = attributeType.ToDisplayString(TypeDisplayFormat);
        if (!mixinCompilation.TryGetAnnotation(providerName, out var expressionAttribute)) continue;
        CollectMixinExpressionContributions(
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

  private static void CollectMixinExpressionContributions(
    MixinGenerationContext context,
    INamedTypeSymbol target,
    ISymbol annotated,
    AttributeData applied,
    CompiledMixinAnnotation configuration,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    ICollection<MixinExpressionOutput> expressionOutputs,
    ICollection<MixinContribution> contributions,
    ref int sequence,
    string providerName,
    IReadOnlyDictionary<string, string> targetDefinitions,
    MixinLibraryCatalog libraries,
    RoslynHostExpressionCache hostValues,
    MixinCompilation mixinCompilation
  ) {
    var location = applied?.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated);
    var attributeName = providerName ?? applied?.AttributeClass?.Name ?? "<unknown>";
    IReadOnlyList<string> targets = [];
    IReadOnlyList<int> orders = [];
    var selectedProgram = annotated is INamedTypeSymbol ? configuration.TypeProgram : configuration.MemberProgram;
    var expression = selectedProgram.Prelude;
    var lateExpression = selectedProgram.Late;

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

    var arguments = annotated is IMethodSymbol method ? (IReadOnlyList<IParameterSymbol>)method.Parameters : [];
    var expressionContext = new RoslynMixinContext(
      target, annotated, applied, arguments, compilation, targetDefinitions: targetDefinitions,
      preparedExpressions: preparedExpressions, libraries: libraries, hostValues: hostValues,
      mixinCompilation: mixinCompilation
    );
    var evaluated = MixinExpressionVirtualMachine.Execute(
      expression, expressionContext, expressionVariables, false
    );
    ReportExpressionLogs(context, location, evaluated.Logs);
    if (!evaluated.Success) {
      ReportInvalidAttributeExpression(
        context, location, attributeName, annotated.Name,
        "line " + evaluated.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + evaluated.Error
      );
      return;
    }
    context.AddDebugExpression(
      selectedProgram.PreludeSource, selectedProgram.LateSource,
      evaluated.Variables,
      providerName ?? attributeName, annotated, evaluated.ExecutedOperations,
      evaluated.ExecutionMilliseconds
    );
    if (lateExpression.Instructions.Count != 0) {
      var sourceType = (annotated as INamedTypeSymbol ?? annotated.ContainingType)
        ?.ToDisplayString(TypeDisplayFormat) ?? "";
      var lateTargets = declarations.Values.Select(item => new LateTarget(
          item.Target, item.EmittedTarget, item.IsStatic, item.IsPublic,
          item.DelegateTarget, item.Order
        )
      ).ToList();
      DiscoverLateMixinTargets(
        selectedProgram.LateSource, evaluated.Variables, targetDefinitions, lateTargets
      );
      context.AddLateExpression(
        lateExpression, selectedProgram.PreludeSource, selectedProgram.LateSource, evaluated.Variables,
        lateTargets.Distinct().ToImmutableArray(), location,
        providerName ?? attributeName, sourceType,
        annotated is INamedTypeSymbol ? "" : annotated.MetadataName,
        annotated.Kind.ToString(),
        annotated is IMethodSymbol sourceMethod ? sourceMethod.Parameters.Length : 0
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
        MixinExpressionOutputTarget.File or MixinExpressionOutputTarget.Extends or
        MixinExpressionOutputTarget.Implements or
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
      if (output.IsEmpty) continue;
      switch (output.Target) {
        case MixinExpressionOutputTarget.Mixin: {
          var result = new MixinExpressionResult(
            true, null, 0, [output.Retarget(MixinExpressionOutputTarget.Target)]
          );
          contributions.Add(
            new MixinContribution(
              output.InjectionTarget, output.InjectionPriority,
              sequence++, targetDefinitions, result, providerName, annotated
            )
          );
          continue;
        }
        case MixinExpressionOutputTarget.Injection when
          !declarations.ContainsKey(output.InjectionTarget):
          contributions.Add(
            new MixinContribution(
              output.InjectionTarget, 0, sequence++, targetDefinitions,
              new MixinExpressionResult(true, null, 0, [output.Retarget(MixinExpressionOutputTarget.Target)]),
              providerName, annotated
            )
          );
          continue;
        case MixinExpressionOutputTarget.Class or
          MixinExpressionOutputTarget.File or MixinExpressionOutputTarget.Extends or
          MixinExpressionOutputTarget.Implements or
          MixinExpressionOutputTarget.Annotation or MixinExpressionOutputTarget.Using:
          expressionOutputs.Add(output);
          continue;
      }
      var injectionTarget = output.Target == MixinExpressionOutputTarget.Target
        ? targets[0]
        : output.InjectionTarget;
      var declaration = declarations[injectionTarget];
      if (declaration.Outputs is null) {
        declaration.Outputs = [];
        (activated ??= []).Add(declaration);
      }
      declaration.Outputs.Add(output.Retarget(MixinExpressionOutputTarget.Target));
    }
    foreach (var declaration in activated ?? Enumerable.Empty<AttributeExpressionTarget>()) {
      var result = new MixinExpressionResult(true, null, 0, declaration.Outputs);
      contributions.Add(
        new MixinContribution(
          declaration.Target, declaration.Order,
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
      var parsed = MixinExpressionParser.ParseDirective(line, 0);
      var instruction = parsed.Node;
      if (parsed.Error is not null || instruction is not MixinDirectiveSyntax mixin ||
        mixin.Target is null) continue;
      string resolved = null;
      if (!mixin.Target.IsDynamic) resolved = mixin.Target.Literal;
      else if (mixin.Target.Expression is { Count: 1 } &&
        mixin.Target.Expression[0].Reference is {
          Root: MixinExpressionRoot.Carry, Member: { } carry, Properties.Count: 0
        } && variables.TryGetValue(MixinExpressionVirtualMachine.CarryLocalPrefix + carry, out var carried))
        resolved = Convert.ToString(carried, CultureInfo.InvariantCulture);
      if (string.IsNullOrWhiteSpace(resolved)) continue;
      var syntax = RoslynMixinContext.ParseMixinTarget(resolved, targetDefinitions);
      targets.Add(
        new LateTarget(
          resolved, syntax.Name, syntax.IsStatic, syntax.IsPublic, syntax.DelegateType, 0
        )
      );
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
    return RoslynMixinContext.ParseMixinTarget(target, targetDefinitions).Name;
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
      var ordered = group.OrderBy(item => item.Order).ThenBy(item => item.Sequence).ToArray();
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
            context, target, name, "the partial declaration does not match the target's static modifier"
          );
          return false;
        }
        if (isPublic && method.DeclaredAccessibility != Accessibility.Public) {
          ReportInvalidTarget(
            context, target, name, "the partial declaration is not public but the target uses the '^' modifier"
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
    var parameters = targetSignature?.Parameters.Select(MixinTargetParameter.FromSymbol).ToArray() ?? [];
    var returnType = targetSignature?.ReturnType;
    var returnsVoid = returnType is null || returnType.SpecialType == SpecialType.System_Void;
    var declaration = signature is null
      ? (isPublic ? "public " : "private ") + (isStatic ? "static " : "") +
      (returnsVoid ? "void" : returnType.ToDisplayString(TypeDisplayFormat)) + " " + EscapeIdentifier(name)
      : BuildMethodDeclaration(signature, partialSyntax, baseMethod is not null);
    generated = new GeneratedMethod(
      declaration,
      signature?.TypeParameters.Select(item => EscapeIdentifier(item.Name)).ToArray() ?? [],
      partialSyntax is null ? [] : TypeParameterConstraints(signature),
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
        ).ToArray();
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
          parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class"
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
      if (output.Target == MixinExpressionOutputTarget.Target)
        builder.Statement(output.Text);
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
    return [
      .. type.GetMembers()
        .OrderBy(
          item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceTree?.FilePath,
          StringComparer.Ordinal
        )
        .ThenBy(SourceOrder)
    ];
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) {
    var attributes = symbol.GetAttributes();
    if (attributes.Length < 2) return attributes;
    return [
      .. attributes
        .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
        .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
    ];
  }

  private static bool IsAttribute(AttributeData attribute, string metadataName) {
    return attribute.AttributeClass?.ToDisplayString() == metadataName;
  }

  private sealed record AnnotatedSymbolData(ISymbol Symbol, IReadOnlyList<AttributeData> Attributes);

  private sealed class MixinGenerationContext {
    private readonly List<DebugExpressionWork> _debugExpressions = [];
    private readonly List<MixinDiagnostic> _diagnostics = [];
    private readonly List<LateExpressionWork> _lateExpressions = [];

    internal MixinGenerationContext(bool debug, bool debugStringPool) {
      Debug = debug;
      DebugStringPool = debugStringPool;
    }

    internal bool Debug { get; }
    internal bool DebugStringPool { get; }
    internal bool HasLateExpressions => _lateExpressions.Count != 0;
    internal IReadOnlyList<LateExpressionWork> LateExpressions => _lateExpressions;
    internal IReadOnlyList<DebugExpressionWork> DebugExpressions => _debugExpressions;

    internal void ReportDiagnostic(Diagnostic diagnostic) {
      _diagnostics.Add(MixinDiagnostic.Detach(diagnostic));
    }

    internal void AddLateExpression(
      MixinExpressionExecutionProgram program,
      string preludeProgram,
      string lateProgram,
      IReadOnlyDictionary<string, object> variables,
      ImmutableArray<LateTarget> targets,
      Location location,
      string provider,
      string sourceType,
      string sourceMember,
      string sourceKind,
      int sourceParameterCount
    ) {
      _lateExpressions.Add(
        new LateExpressionWork(
          program, preludeProgram, lateProgram, variables,
          targets,
          MixinDiagnostic.Detach(Diagnostic.Create(ExpressionLog, location, "")),
          provider ?? "", sourceType ?? "",
          sourceMember ?? "", sourceKind ?? "", sourceParameterCount
        )
      );
    }

    internal void AddDebugExpression(
      string preludeProgram,
      string lateProgram,
      IReadOnlyDictionary<string, object> variables,
      string provider,
      ISymbol source,
      int preludeOperations,
      double preludeMilliseconds
    ) {
      if (!Debug) return;
      _debugExpressions.Add(
        new DebugExpressionWork(
          preludeProgram, lateProgram, variables.ToImmutableDictionary(StringComparer.Ordinal),
          provider ?? "", (source as INamedTypeSymbol ?? source.ContainingType)
          ?.ToDisplayString(TypeDisplayFormat) ?? "",
          source is INamedTypeSymbol ? "" : source.MetadataName,
          preludeOperations, preludeMilliseconds
        )
      );
    }

    internal MixinGenerationResult Complete(MixinRenderModel render = null) {
      return new MixinGenerationResult(
        _diagnostics.ToImmutableArray(), render, _lateExpressions.ToImmutableArray()
      );
    }
  }

  private sealed class MixinGenerationResult {
    internal MixinGenerationResult(
      ImmutableArray<MixinDiagnostic> diagnostics, MixinRenderModel render,
      ImmutableArray<LateExpressionWork> lateExpressions
    ) {
      Diagnostics = diagnostics;
      Render = render;
      LateExpressions = lateExpressions;
    }

    internal ImmutableArray<MixinDiagnostic> Diagnostics { get; }
    internal MixinRenderModel Render { get; }
    internal ImmutableArray<LateExpressionWork> LateExpressions { get; }
    internal MixinOutputModel Output => new(
      Render?.Wrapper.HintName, null, Render, LateExpressions,
      Diagnostics, ImmutableArray<string>.Empty, ImmutableArray<MixinReportedLog>.Empty
    );
  }

  private sealed record LateExpressionWork(
    MixinExpressionExecutionProgram Program,
    string PreludeProgram,
    string LateProgram,
    IReadOnlyDictionary<string, object> Variables,
    ImmutableArray<LateTarget> Targets,
    MixinDiagnostic Location,
    string Provider,
    string SourceType,
    string SourceMember,
    string SourceKind,
    int SourceParameterCount
  );

  private sealed record DebugExpressionWork(
    string PreludeProgram,
    string LateProgram,
    ImmutableDictionary<string, object> Variables,
    string Provider,
    string SourceType,
    string SourceMember,
    int PreludeOperations,
    double PreludeMilliseconds
  );

  private sealed record FinalDebugState(
    DebugExpressionWork Work,
    int LateOperations,
    double LateMilliseconds
  );

  private sealed record LateTarget(
    string DeclaredTarget,
    string EmittedTarget,
    bool IsStatic,
    bool IsPublic,
    string DelegateTarget,
    int Order
  );

  private sealed class MixinRenderModel {
    internal MixinRenderModel(
      DetachedTypeWrapper wrapper,
      ImmutableArray<GeneratedMethod> methods,
      MixinOutputCollection annotations,
      MixinOutputCollection @class,
      MixinOutputCollection file,
      MixinOutputCollection implements,
      MixinOutputCollection usings,
      IEnumerable<KeyValuePair<string, object>> primaryVariables,
      MixinStringPool stringPool,
      ImmutableArray<DebugExpressionWork> debugExpressions,
      bool debug,
      bool debugStringPool
    ) {
      Wrapper = wrapper;
      Methods = methods;
      Annotations = annotations;
      Class = @class;
      File = file;
      Implements = implements;
      Usings = usings;
      PrimaryVariables = new MixinValueDictionary(
        primaryVariables.Select(item =>
          new KeyValuePair<MixinString, IMixinValue>(
            stringPool.Get(item.Key), RuntimeValue(item.Value, stringPool)
          )
        )
      );
      DebugExpressions = debugExpressions;
      Debug = debug;
      DebugStringPool = debugStringPool;
      StringPool = stringPool;
      GenerationVersion = Interlocked.Increment(ref _generationCounter);
      Fingerprint = MixinRenderFingerprint.Create(this);
    }

    internal DetachedTypeWrapper Wrapper { get; }
    internal ImmutableArray<GeneratedMethod> Methods { get; }
    internal MixinOutputCollection Annotations { get; }
    internal MixinOutputCollection Class { get; }
    internal MixinOutputCollection File { get; }
    internal MixinOutputCollection Implements { get; }
    internal MixinOutputCollection Usings { get; }
    internal MixinValueDictionary PrimaryVariables { get; }
    internal ImmutableArray<DebugExpressionWork> DebugExpressions { get; }
    internal bool Debug { get; }
    internal bool DebugStringPool { get; }
    internal MixinStringPool StringPool { get; }

    internal long GenerationVersion { get; }
    internal MixinRenderFingerprint Fingerprint { get; }

    private static IMixinValue RuntimeValue(object value, MixinStringPool strings) {
      return value switch {
        IMixinValue typed => typed,
        null => NullMixinValue.Instance,
        bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
        string text => new LiteralMixinValue(MixinString.Dynamic(text)),
        DetachedSemanticData detached => DetachedSemanticMixinValue.Materialize(detached, strings),
        _ => new ObjectMixinValue(value)
      };
    }
  }

  private sealed record MixinDiagnostic(
    DiagnosticDescriptor Descriptor,
    string Message,
    string Path,
    TextSpan SourceSpan,
    LinePositionSpan LineSpan,
    bool HasLocation,
    ImmutableDictionary<string, string> Properties
  ) {
    internal static MixinDiagnostic Detach(Diagnostic diagnostic) {
      var location = diagnostic.Location;
      if (location == Location.None || !location.IsInSource) {
        return new MixinDiagnostic(
          diagnostic.Descriptor, diagnostic.GetMessage(CultureInfo.InvariantCulture),
          "", default, default, false, diagnostic.Properties
        );
      }
      var lineSpan = location.GetLineSpan();
      return new MixinDiagnostic(
        diagnostic.Descriptor, diagnostic.GetMessage(CultureInfo.InvariantCulture),
        lineSpan.Path ?? "", location.SourceSpan, lineSpan.Span, true, diagnostic.Properties
      );
    }

    internal Diagnostic Create() {
      var descriptor = new DiagnosticDescriptor(
        Descriptor.Id, Descriptor.Title, "{0}", Descriptor.Category,
        Descriptor.DefaultSeverity, Descriptor.IsEnabledByDefault,
        Descriptor.Description, Descriptor.HelpLinkUri, [.. Descriptor.CustomTags]
      );
      var location = HasLocation ? Location.Create(Path, SourceSpan, LineSpan) : Location.None;
      return Diagnostic.Create(descriptor, location, null, Properties, Message);
    }

    internal Diagnostic Create(DiagnosticDescriptor descriptor, string message) {
      var location = HasLocation
        ? Location.Create(Path, SourceSpan, LineSpan)
        : Location.None;
      return Diagnostic.Create(descriptor, location, message);
    }
  }

  private sealed record MixinOutputModel(
    string HintName, string Source, MixinRenderModel Render,
    ImmutableArray<LateExpressionWork> LateExpressions,
    ImmutableArray<MixinDiagnostic> Diagnostics, ImmutableArray<string> Errors,
    ImmutableArray<MixinReportedLog> Logs
  );

  private sealed record MixinReportedLog(MixinExpressionLog Log, MixinDiagnostic Location);

  private readonly struct MixinRenderFingerprint : IEquatable<MixinRenderFingerprint> {
    private MixinRenderFingerprint(
      FingerprintPart outputs,
      FingerprintPart variables,
      FingerprintPart signatures,
      bool debug,
      bool debugStringPool
    ) {
      Outputs = outputs;
      Variables = variables;
      Signatures = signatures;
      Debug = debug;
      DebugStringPool = debugStringPool;
    }

    internal FingerprintPart Outputs { get; }
    internal FingerprintPart Variables { get; }
    internal FingerprintPart Signatures { get; }
    private bool Debug { get; }
    private bool DebugStringPool { get; }

    internal static MixinRenderFingerprint Create(MixinRenderModel render) {
      var outputs = new MixinFingerprintBuilder();
      AppendOutputCollection(outputs, render.Annotations);
      AppendOutputCollection(outputs, render.Class);
      AppendOutputCollection(outputs, render.File);
      AppendOutputCollection(outputs, render.Implements);
      AppendOutputCollection(outputs, render.Usings);

      var variables = new MixinFingerprintBuilder();
      var fingerprintContext = new UnlinkedMixinExpressionContext(render.StringPool);
      variables.Append(render.PrimaryVariables.Count);
      foreach (var variable in render.PrimaryVariables
        .OrderBy(item => item.Key.Id)
        .ThenBy(item => item.Key.DynamicValue, StringComparer.Ordinal)) {
        variables.Append(variable.Key.IsInterned);
        variables.Append(variable.Key.Id);
        if (!variable.Key.IsInterned) variables.Append(variable.Key.DynamicValue);
        variable.Value.Fingerprint(variables, fingerprintContext);
      }

      var signatures = new MixinFingerprintBuilder();
      signatures.Append(render.Wrapper.NamespaceName);
      signatures.Append(render.Wrapper.HintName);
      signatures.Append(render.Wrapper.Declarations.Count);
      foreach (var declaration in render.Wrapper.Declarations) signatures.Append(declaration);
      signatures.Append(render.Methods.Length);
      foreach (var method in render.Methods) {
        signatures.Append(method.Declaration);
        signatures.Append(method.ReturnType);
        signatures.Append(method.CallBase);
        signatures.Append(method.Name);
        signatures.Append(method.TypeParameters.Count);
        foreach (var parameter in method.TypeParameters) signatures.Append(parameter);
        signatures.Append(method.Constraints.Count);
        foreach (var constraint in method.Constraints) signatures.Append(constraint);
        signatures.Append(method.Parameters.Count);
        foreach (var parameter in method.Parameters) {
          signatures.Append(parameter.Declaration);
          signatures.Append(parameter.Argument);
        }
        outputs.Append(method.Contributions.Count);
        foreach (var contribution in method.Contributions) {
          outputs.Append(contribution.EmittedTarget);
          outputs.Append(contribution.Order);
          AppendOutputs(outputs, contribution.ExpressionResult.Outputs);
        }
      }
      signatures.Append(render.DebugStringPool);
      if (render.DebugStringPool) {
        signatures.Append(render.StringPool.Count);
        for (var id = 0; id < render.StringPool.Count; id++)
          signatures.Append(render.StringPool[id]);
      }
      return new MixinRenderFingerprint(
        new FingerprintPart(outputs.Hash, outputs.Length),
        new FingerprintPart(variables.Hash, variables.Length),
        new FingerprintPart(signatures.Hash, signatures.Length),
        render.Debug, render.DebugStringPool
      );
    }

    private static IMixinValue RuntimeValue(object value, MixinStringPool strings) {
      return value switch {
        IMixinValue typed => typed,
        null => NullMixinValue.Instance,
        bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
        string text => new LiteralMixinValue(MixinString.Dynamic(text)),
        DetachedSemanticData detached => DetachedSemanticMixinValue.Materialize(detached, strings),
        _ => new ObjectMixinValue(value)
      };
    }

    private static void AppendOutputCollection(
      MixinFingerprintBuilder builder,
      MixinOutputCollection outputs
    ) {
      builder.Append(outputs.Count);
      builder.Append(unchecked((long)outputs.Hash));
      builder.Append(outputs.Length);
    }

    private static void AppendOutputs(
      MixinFingerprintBuilder builder,
      IReadOnlyCollection<MixinExpressionOutput> outputs
    ) {
      builder.Append(outputs.Count);
      foreach (var output in outputs) {
        builder.Append((int)output.Target);
        builder.Append(output.InjectionTarget);
        builder.Append(output.InjectionPriority);
        builder.Append(output.Segments.Count);
        foreach (var segment in output.Segments) {
          builder.Append(segment.IsInterned);
          builder.Append(output.Resolve(segment));
        }
      }
    }

    public bool Equals(MixinRenderFingerprint other) {
      return Outputs.Equals(other.Outputs) &&
        Variables.Equals(other.Variables) &&
        Signatures.Equals(other.Signatures) && Debug == other.Debug &&
        DebugStringPool == other.DebugStringPool;
    }

    public override bool Equals(object value) {
      return value is MixinRenderFingerprint other && Equals(other);
    }

    public override int GetHashCode() {
      return unchecked(
        (((((((Outputs.GetHashCode() * 397) ^ Variables.GetHashCode()) * 397) ^
          Signatures.GetHashCode()) * 397) ^ Debug.GetHashCode()) * 397) ^
        DebugStringPool.GetHashCode()
      );
    }

    internal readonly struct FingerprintPart : IEquatable<FingerprintPart> {
      internal FingerprintPart(ulong hash, long length) {
        Hash = hash;
        Length = length;
      }

      internal ulong Hash { get; }
      internal long Length { get; }

      public bool Equals(FingerprintPart other) {
        return Hash == other.Hash && Length == other.Length;
      }

      public override bool Equals(object value) {
        return value is FingerprintPart other && Equals(other);
      }

      public override int GetHashCode() {
        return unchecked(((int)(Hash ^ (Hash >> 32)) * 397) ^ Length.GetHashCode());
      }
    }
  }

  private sealed class MixinOutputModelComparer : IEqualityComparer<MixinOutputModel> {
    internal static readonly MixinOutputModelComparer Instance = new();

    public bool Equals(MixinOutputModel x, MixinOutputModel y) {
      return ReferenceEquals(x, y) || (x is not null &&
        y is not null &&
        string.Equals(x.HintName, y.HintName, StringComparison.Ordinal) &&
        Nullable.Equals(x.Render?.Fingerprint, y.Render?.Fingerprint) &&
        DiagnosticKey(x.Diagnostics) == DiagnosticKey(y.Diagnostics) &&
        LateEqual(x.LateExpressions, y.LateExpressions));
    }

    public int GetHashCode(MixinOutputModel value) {
      return unchecked(
        (StringComparer.Ordinal.GetHashCode(value.HintName ?? "") * 397) ^
        (value.Render?.Fingerprint.GetHashCode() ?? 0)
      );
    }

    private static string DiagnosticKey(ImmutableArray<MixinDiagnostic> diagnostics) {
      return string.Join(
        "\u001e", diagnostics.Select(diagnostic => string.Join(
            "\u001f", diagnostic.Descriptor.Id, diagnostic.Descriptor.DefaultSeverity.ToString(), diagnostic.Message,
            diagnostic.Path, diagnostic.SourceSpan.Start.ToString(CultureInfo.InvariantCulture),
            diagnostic.SourceSpan.Length.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.Start.Line.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.Start.Character.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.End.Line.ToString(CultureInfo.InvariantCulture),
            diagnostic.LineSpan.End.Character.ToString(CultureInfo.InvariantCulture), string.Join(
              "\u001d", diagnostic.Properties.OrderBy(item => item.Key).Select(item => item.Key + "=" + item.Value)
            )
          )
        )
      );
    }

    private static bool LateEqual(ImmutableArray<LateExpressionWork> x, ImmutableArray<LateExpressionWork> y) {
      if (x.Length != y.Length) return false;
      for (var index = 0; index < x.Length; index++) {
        var left = x[index];
        var right = y[index];
        if (left.PreludeProgram != right.PreludeProgram || left.LateProgram != right.LateProgram ||
          left.Variables.Count != right.Variables.Count || left.Provider != right.Provider ||
          left.SourceType != right.SourceType || left.SourceMember != right.SourceMember ||
          left.SourceKind != right.SourceKind ||
          left.SourceParameterCount != right.SourceParameterCount ||
          left.Targets.Length != right.Targets.Length) return false;
        for (var targetIndex = 0; targetIndex < left.Targets.Length; targetIndex++) {
          if (left.Targets[targetIndex] != right.Targets[targetIndex])
            return false;
        }
        foreach (var item in left.Variables) {
          if (!right.Variables.TryGetValue(item.Key, out var value) || !Equals(item.Value, value))
            return false;
        }
      }
      return true;
    }
  }

  private sealed class UnlinkedMixinExpressionContext : ExecutionContext {
    internal UnlinkedMixinExpressionContext(MixinStringPool strings) : base(strings) { }

    protected override IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member) {
      return Error("late expressions cannot resolve host values");
    }
  }

  private sealed class ExpressionOutputs {
    private MixinOutputAccumulator _annotations, _class, _file, _implements, _usings;
    internal bool Any { get; private set; }

    internal void AddAnnotation(string text) {
      if (string.IsNullOrEmpty(text)) return;
      Any = true;
      (_annotations ??= new MixinOutputAccumulator()).Add(
        new MixinExpressionOutput(MixinExpressionOutputTarget.Annotation, text)
      );
    }

    internal void Add(MixinExpressionOutput output) {
      if (output.IsEmpty) return;
      if (output.Target == MixinExpressionOutputTarget.Class) {
        Any = true;
        (_class ??= new MixinOutputAccumulator()).Add(output);
        return;
      }
      if (output.Target == MixinExpressionOutputTarget.File) {
        Any = true;
        (_file ??= new MixinOutputAccumulator()).Add(output);
        return;
      }
      Any = true;
      switch (output.Target) {
        case MixinExpressionOutputTarget.Extends or MixinExpressionOutputTarget.Implements:
          (_implements ??= new MixinOutputAccumulator()).Add(output); break;
        case MixinExpressionOutputTarget.Annotation:
          (_annotations ??= new MixinOutputAccumulator()).Add(output); break;
        case MixinExpressionOutputTarget.Using:
          (_usings ??= new MixinOutputAccumulator()).Add(output);
          break;
      }
    }

    internal ExpressionOutputSet Finish() {
      return new ExpressionOutputSet(
        MixinOutputCollection.Finish(_annotations),
        MixinOutputCollection.Finish(_class),
        MixinOutputCollection.Finish(_file),
        MixinOutputCollection.Finish(_implements),
        MixinOutputCollection.Finish(_usings)
      );
    }
  }

  private sealed record ExpressionOutputSet(
    MixinOutputCollection Annotations,
    MixinOutputCollection Class,
    MixinOutputCollection File,
    MixinOutputCollection Implements,
    MixinOutputCollection Usings
  );

  private sealed class MixinOutputAccumulator {
    private readonly List<MixinExpressionOutput> _outputs = [];
    private OutputFingerprintBuilder _fingerprint;

    internal IReadOnlyList<MixinExpressionOutput> Outputs => _outputs;
    internal ulong Hash => _fingerprint.Hash;
    internal long Length => _fingerprint.Length;

    internal void Add(MixinExpressionOutput output) {
      _outputs.Add(output);
      _fingerprint.Append(output);
    }
  }

  private sealed class MixinOutputCollection : IReadOnlyList<MixinExpressionOutput> {
    private static readonly MixinOutputCollection Empty = new(
      [], OutputFingerprintBuilder.EmptyHash, 0
    );
    private readonly MixinExpressionOutput[] _outputs;

    private MixinOutputCollection(MixinExpressionOutput[] outputs, ulong hash, long length) {
      _outputs = outputs;
      Hash = hash;
      Length = length;
    }

    internal ulong Hash { get; }
    internal long Length { get; }
    public int Count => _outputs.Length;
    public MixinExpressionOutput this[int index] => _outputs[index];

    public IEnumerator<MixinExpressionOutput> GetEnumerator() {
      return ((IEnumerable<MixinExpressionOutput>)_outputs).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return _outputs.GetEnumerator();
    }

    internal static MixinOutputCollection Finish(MixinOutputAccumulator accumulator) {
      return accumulator is null || accumulator.Outputs.Count == 0
        ? Empty
        : new MixinOutputCollection(
          [.. accumulator.Outputs], accumulator.Hash, accumulator.Length
        );
    }
  }

  private struct OutputFingerprintBuilder {
    internal const ulong EmptyHash = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;
    private ulong _hash;

    internal ulong Hash => _hash == 0 ? EmptyHash : _hash;
    internal long Length { get; private set; }

    internal void Append(MixinExpressionOutput output) {
      Mix((ulong)output.Target);
      Append(output.InjectionTarget);
      Mix(unchecked((ulong)output.InjectionPriority));
      Mix((ulong)output.Segments.Count);
      foreach (var segment in output.Segments) {
        Mix(segment.IsInterned ? 1UL : 0UL);
        Append(output.Resolve(segment));
      }
    }

    private void Append(string value) {
      Mix((ulong)(value?.Length ?? -1));
      if (value is null) return;
      for (var index = 0; index < value.Length; index++) Mix(value[index]);
      Length += value.Length;
    }

    private void Mix(ulong value) {
      if (_hash == 0) _hash = EmptyHash;
      unchecked {
        _hash ^= value;
        _hash *= Prime;
      }
      Length++;
    }
  }

  private sealed class MixinContribution {
    private MixinContribution(
      LateTarget target,
      int order,
      int sequence,
      MixinExpressionResult expressionResult,
      LateExpressionWork work,
      bool placeholder
    ) : this(target, order, sequence, expressionResult, work) {
      IsPlaceholder = placeholder;
    }

    internal MixinContribution(
      LateTarget target,
      int order,
      int sequence,
      MixinExpressionResult expressionResult,
      LateExpressionWork work
    ) {
      EmittedTarget = target.EmittedTarget;
      IsStaticTarget = target.IsStatic;
      IsPublicTarget = target.IsPublic;
      DelegateTarget = target.DelegateTarget;
      Order = order;
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
      int sequence,
      IReadOnlyDictionary<string, string> targetDefinitions,
      MixinExpressionResult expressionResult,
      string provider,
      ISymbol source
    ) {
      var targetSyntax = RoslynMixinContext.ParseMixinTarget(target, targetDefinitions);
      EmittedTarget = targetSyntax.Name;
      IsStaticTarget = targetSyntax.IsStatic;
      IsPublicTarget = targetSyntax.IsPublic;
      DelegateTarget = targetSyntax.DelegateType;
      Order = order;
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
    internal int Sequence { get; }
    internal MixinExpressionResult ExpressionResult { get; }
    internal string Provider { get; }
    internal string SourceType { get; }
    internal string SourceMember { get; }
    internal string SourceKind { get; }
    internal int SourceParameterCount { get; }
    internal bool IsPlaceholder { get; }

    internal static MixinContribution Placeholder(
      LateTarget target, int sequence, LateExpressionWork work
    ) {
      return new MixinContribution(
        target, target.Order, sequence,
        new MixinExpressionResult(true, null, 0, []),
        work, true
      );
    }
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
      var targetSyntax = RoslynMixinContext.ParseMixinTarget(target, targetDefinitions);
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

  private sealed record MixinTarget(INamedTypeSymbol Type, CSharpCompilation Compilation);
}