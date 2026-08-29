using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.SourceGen.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorDiagnostics.Mixins;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

/// <summary>Evaluates attribute mixins for generated prop-structure datatype configuration.</summary>
internal static class PropStructMixinApi {
  private const string ConfigureTarget = Members.ConfigureDatatype;

  internal static PropStructMixinModel Analyze(
    SourceProductionContext production,
    INamedTypeSymbol type,
    IReadOnlyList<ISymbol> properties,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    MixinLibraryCatalog libraries
  ) {
    var model = new PropStructMixinModel();
    var variables = new Dictionary<string, object>(StringComparer.Ordinal);
    var targetDefinitions = TargetDefinitions(type, properties, libraries);
    var sequence = 0;
    EvaluateSymbol(
      production, type, type, compilation, preparedExpressions, variables,
      targetDefinitions, model, ref sequence, libraries
    );
    foreach (var property in properties)
      EvaluateSymbol(
        production, type, property, compilation, preparedExpressions, variables,
        targetDefinitions, model, ref sequence, libraries
      );
    model.SortConfiguration();
    return model;
  }

  internal static bool TryAnalyzeInlineConfiguration(
    INamedTypeSymbol type,
    IReadOnlyList<ISymbol> properties,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    MixinLibraryCatalog libraries,
    out IReadOnlyList<string> configuration,
    out string error,
    bool includeTypeConfiguration = false
  ) {
    var model = new PropStructMixinModel();
    var variables = new Dictionary<string, object>(StringComparer.Ordinal);
    var targetDefinitions = TargetDefinitions(type, properties, libraries);
    var sequence = 0;
    var owners = includeTypeConfiguration
      ? new ISymbol[] { type }.Concat(properties)
      : properties;
    foreach (var owner in owners) {
      foreach (var applied in OrderedAttributes(owner)) {
        if (applied.AttributeClass is not { } attributeType) continue;
        if (attributeType.ToDisplayString() == GeneratorStrings.Attributes.Structure) continue;
        foreach (var expressionAttribute in MixinLibraryApi.Annotations(attributeType, libraries)) {
          var expression = expressionAttribute.Prelude + expressionAttribute.Expression;
          const int order = 0;
          var arguments = owner is IParameterSymbol { ContainingSymbol: IMethodSymbol method }
            ? (IReadOnlyList<IParameterSymbol>)method.Parameters
            : Array.Empty<IParameterSymbol>();
          var expressionContext = new RoslynMixinExpressionContext(
            type, owner, applied, arguments, compilation,
            targetDefinitions: targetDefinitions,
            preparedExpressions: preparedExpressions,
            libraries: libraries
          );
          var evaluated = new MixinExpressionInterpreter().Execute(
            expression, expressionContext, variables, preparedExpressions
          );
          if (!evaluated.Success) {
            configuration = null;
            error = "line " + evaluated.ErrorLine.ToString(CultureInfo.InvariantCulture) +
              ": " + evaluated.Error;
            return false;
          }
          foreach (var output in evaluated.Outputs) {
            if (string.IsNullOrEmpty(output.Text)) continue;
            if (output.Target == MixinExpressionOutputTarget.Target) {
              model.AddConfiguration(output.Text, order, sequence++);
              continue;
            }
            if (output.Target is MixinExpressionOutputTarget.Injection or
              MixinExpressionOutputTarget.Mixin) {
              var target = RoslynMixinExpressionContext.ParseMixinTarget(
                output.InjectionTarget, targetDefinitions
              ).Name;
              if (target == ConfigureTarget) {
                model.AddConfiguration(
                  output.Text,
                  output.Target == MixinExpressionOutputTarget.Mixin
                    ? output.InjectionPriority
                    : order,
                  sequence++
                );
                continue;
              }
            }
            configuration = null;
            error = "property mixins used by PROP_STRUCT may only target " + ConfigureTarget;
            return false;
          }
        }
      }
    }
    model.SortConfiguration();
    configuration = model.Configuration;
    error = null;
    return true;
  }

  private static void EvaluateSymbol(
    SourceProductionContext production,
    INamedTypeSymbol type,
    ISymbol annotated,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> variables,
    IReadOnlyDictionary<string, string> targetDefinitions,
    PropStructMixinModel model,
    ref int sequence,
    MixinLibraryCatalog libraries
  ) {
    foreach (var applied in OrderedAttributes(annotated)) {
      if (applied.AttributeClass is not { } attributeType) continue;
      foreach (var configuration in MixinLibraryApi.Annotations(attributeType, libraries)) {
        var location = applied.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated);
        var expression = configuration.Prelude + configuration.Expression;
        const int order = 0;
        var arguments = annotated switch {
          IMethodSymbol method => (IReadOnlyList<IParameterSymbol>)method.Parameters,
          IParameterSymbol { ContainingSymbol: IMethodSymbol method } => method.Parameters,
          _ => []
        };
        var expressionContext = new RoslynMixinExpressionContext(
          type, annotated, applied, arguments, compilation,
          targetDefinitions: targetDefinitions,
          preparedExpressions: preparedExpressions,
          libraries: libraries
        );
        var evaluated = new MixinExpressionInterpreter().Execute(
          expression, expressionContext, variables, preparedExpressions
        );
        ReportLogs(production, location, evaluated.Logs);
        if (!evaluated.Success) {
          ReportFailure(
            production, location, attributeType.Name, annotated.Name,
            "line " + evaluated.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + evaluated.Error
          );
          continue;
        }
        foreach (var output in evaluated.Outputs) {
          if (string.IsNullOrEmpty(output.Text)) continue;
          switch (output.Target) {
            case MixinExpressionOutputTarget.Target:
              model.AddConfiguration(output.Text, order, sequence++);
              break;
            case MixinExpressionOutputTarget.Injection:
            case MixinExpressionOutputTarget.Mixin:
              var target = RoslynMixinExpressionContext.ParseMixinTarget(
                output.InjectionTarget, targetDefinitions
              ).Name;
              if (target != ConfigureTarget) {
                ReportFailure(
                  production, location, attributeType.Name, annotated.Name,
                  "code target '" + (output.InjectionTarget ?? "") +
                  "' is unavailable while generating a prop structure"
                );
                break;
              }
              model.AddConfiguration(
                output.Text,
                output.Target == MixinExpressionOutputTarget.Mixin
                  ? output.InjectionPriority
                  : order,
                sequence++
              );
              break;
            default:
              model.AddOutput(output);
              break;
          }
        }
      }
    }
  }

  private static IReadOnlyDictionary<string, string> TargetDefinitions(
    INamedTypeSymbol type,
    IEnumerable<ISymbol> properties,
    MixinLibraryCatalog libraries
  ) {
    var result = new Dictionary<string, string>(StringComparer.Ordinal) {
      ["$Init"] = "*" + ConfigureTarget,
      ["$Configure"] = "*" + ConfigureTarget
    };
    foreach (var owner in new ISymbol[] { type }.Concat(properties))
      foreach (var applied in OrderedAttributes(owner))
        foreach (var annotation in MixinLibraryApi.Annotations(applied.AttributeClass, libraries))
          foreach (var definition in annotation.TargetDefinitions)
            result[definition.Key] = definition.Value;
    return result;
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) => symbol.GetAttributes()
    .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
    .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
    .ToArray();

  private static void ReportFailure(
    SourceProductionContext production,
    Location location,
    string attribute,
    string annotated,
    string failure
  ) => production.ReportDiagnostic(
    Diagnostic.Create(InvalidAttributeExpression, location, attribute, annotated, failure)
  );

  private static void ReportLogs(
    SourceProductionContext production,
    Location location,
    IReadOnlyList<MixinExpressionLog> logs
  ) {
    foreach (var log in logs)
      production.ReportDiagnostic(
        Diagnostic.Create(log.IsHint ? ExpressionHint : ExpressionLog, location, log.Text)
      );
  }
}

internal sealed class PropStructMixinModel {
  private readonly List<ConfigurationOutput> _configuration = new();
  private readonly List<string> _class = new();
  private readonly List<string> _file = new();
  private readonly List<string> _implements = new();
  private readonly List<string> _annotations = new();
  private readonly List<string> _usings = new();

  internal IReadOnlyList<string> Configuration => _configuration.Select(item => item.Text).ToArray();
  internal IReadOnlyList<string> Class => _class;
  internal IReadOnlyList<string> File => _file;
  internal IReadOnlyList<string> Implements => _implements;
  internal IReadOnlyList<string> Annotations => _annotations;
  internal IReadOnlyList<string> Usings => _usings;

  internal void AddConfiguration(string text, int order, int sequence) =>
    _configuration.Add(new ConfigurationOutput(text, order, sequence));

  internal void SortConfiguration() => _configuration.Sort((left, right) => {
      var order = left.Order.CompareTo(right.Order);
      return order != 0 ? order : left.Sequence.CompareTo(right.Sequence);
    }
  );

  internal void AddOutput(MixinExpressionOutput output) {
    var text = output.Text.Trim();
    switch (output.Target) {
      case MixinExpressionOutputTarget.Class: _class.Add(output.Text); break;
      case MixinExpressionOutputTarget.File: _file.Add(output.Text); break;
      case MixinExpressionOutputTarget.Implements: AddUnique(_implements, text); break;
      case MixinExpressionOutputTarget.Annotation: AddUnique(_annotations, text); break;
      case MixinExpressionOutputTarget.Using:
        text = text.TrimEnd(';');
        AddUnique(_usings, text.Length == 0 ? "" : "using " + text + ";");
        break;
    }
  }

  private static void AddUnique(ICollection<string> values, string value) {
    if (value.Length != 0 && !values.Contains(value)) values.Add(value);
  }

  private sealed record ConfigurationOutput(string Text, int Order, int Sequence);
}
