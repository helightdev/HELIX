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
    MixinExpressionPreparedState preparedExpressions
  ) {
    var model = new PropStructMixinModel();
    var variables = new Dictionary<string, object>(StringComparer.Ordinal);
    var targetDefinitions = TargetDefinitions(type);
    var sequence = 0;
    EvaluateSymbol(
      production, type, type, compilation, preparedExpressions, variables,
      targetDefinitions, model, ref sequence
    );
    foreach (var property in properties)
      EvaluateSymbol(
        production, type, property, compilation, preparedExpressions, variables,
        targetDefinitions, model, ref sequence
      );
    model.SortConfiguration();
    return model;
  }

  internal static bool TryAnalyzeInlineConfiguration(
    INamedTypeSymbol type,
    IReadOnlyList<ISymbol> properties,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    out IReadOnlyList<string> configuration,
    out string error
  ) {
    if (!MixinLibraryApi.TryPrepare(
      MixinLibraryApi.AttributeOwners(new ISymbol[] { type }.Concat(properties)),
      out preparedExpressions,
      out error
    )) {
      configuration = null;
      return false;
    }
    var model = new PropStructMixinModel();
    var variables = new Dictionary<string, object>(StringComparer.Ordinal);
    var targetDefinitions = TargetDefinitions(type);
    var sequence = 0;
    foreach (var property in properties) {
      foreach (var applied in OrderedAttributes(property)) {
        if (applied.AttributeClass is not { } attributeType) continue;
        foreach (var expressionAttribute in InheritedExpressionAttributes(attributeType)) {
          if (!TryReadConfiguration(
            expressionAttribute, targetDefinitions, out var expression, out var order, out error
          )) {
            configuration = null;
            return false;
          }
          var arguments = property is IParameterSymbol { ContainingSymbol: IMethodSymbol method }
            ? (IReadOnlyList<IParameterSymbol>)method.Parameters
            : Array.Empty<IParameterSymbol>();
          var expressionContext = new RoslynMixinExpressionContext(
            type, property, applied, arguments, compilation,
            targetDefinitions: targetDefinitions,
            preparedExpressions: preparedExpressions
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
    ref int sequence
  ) {
    foreach (var applied in OrderedAttributes(annotated)) {
      if (applied.AttributeClass is not { } attributeType) continue;
      foreach (var configuration in InheritedExpressionAttributes(attributeType)) {
        var location = applied.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated);
        if (!TryReadConfiguration(
          configuration, targetDefinitions, out var expression, out var order, out var failure
        )) {
          ReportFailure(production, location, attributeType.Name, annotated.Name, failure);
          continue;
        }
        var arguments = annotated switch {
          IMethodSymbol method => (IReadOnlyList<IParameterSymbol>)method.Parameters,
          IParameterSymbol { ContainingSymbol: IMethodSymbol method } => method.Parameters,
          _ => []
        };
        var expressionContext = new RoslynMixinExpressionContext(
          type, annotated, applied, arguments, compilation,
          targetDefinitions: targetDefinitions,
          preparedExpressions: preparedExpressions
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

  private static bool TryReadConfiguration(
    AttributeData configuration,
    IReadOnlyDictionary<string, string> targetDefinitions,
    out string expression,
    out int configureOrder,
    out string failure
  ) {
    expression = null;
    configureOrder = 0;
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
    expression = configuration.ConstructorArguments[2].Value as string;
    if (expression is null) {
      failure = "the expression cannot be null";
      return false;
    }
    var targets = Constants(configuration.ConstructorArguments[0]);
    var orders = Constants(configuration.ConstructorArguments[1]);
    if (targets.Count != orders.Count) {
      failure = "target and order arrays must have the same length";
      return false;
    }
    for (var index = 0; index < targets.Count; index++) {
      if (targets[index].Value is not string target || string.IsNullOrWhiteSpace(target)) {
        failure = "target names cannot be empty";
        return false;
      }
      if (!TryConvertToInt32(orders[index].Value, out var order)) {
        failure = "an order value is not a valid integer";
        return false;
      }
      if (RoslynMixinExpressionContext.ParseMixinTarget(target, targetDefinitions).Name == ConfigureTarget)
        configureOrder = order;
    }
    return true;
  }

  private static IReadOnlyList<TypedConstant> Constants(TypedConstant value) =>
    value.Kind == TypedConstantKind.Array ? value.Values : new[] { value };

  private static IReadOnlyDictionary<string, string> TargetDefinitions(INamedTypeSymbol type) {
    var result = new Dictionary<string, string>(StringComparer.Ordinal) {
      ["$Init"] = "*" + ConfigureTarget,
      ["$Configure"] = "*" + ConfigureTarget
    };
    var hierarchy = new Stack<INamedTypeSymbol>();
    for (var current = type; current is not null; current = current.BaseType) hierarchy.Push(current);
    while (hierarchy.Count != 0) {
      foreach (var applied in OrderedAttributes(hierarchy.Pop())) {
        ApplyTargetDefinition(applied, result);
        if (applied.AttributeClass is not { } attributeType) continue;
        var attributeHierarchy = new Stack<INamedTypeSymbol>();
        for (var current = attributeType; current is not null; current = current.BaseType)
          attributeHierarchy.Push(current);
        while (attributeHierarchy.Count != 0)
          foreach (var definition in OrderedAttributes(attributeHierarchy.Pop()))
            ApplyTargetDefinition(definition, result);
      }
    }
    return result;
  }

  private static void ApplyTargetDefinition(
    AttributeData attribute,
    IDictionary<string, string> definitions
  ) {
    if (attribute.AttributeClass?.ToDisplayString() != Attributes.MixinDefineTarget ||
      attribute.ConstructorArguments.Length < 2 ||
      attribute.ConstructorArguments[0].Value is not string key ||
      attribute.ConstructorArguments[1].Value is not string target ||
      string.IsNullOrWhiteSpace(key)) return;
    definitions["$" + key.TrimStart('$')] = target;
  }

  private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) => symbol.GetAttributes()
    .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
    .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
    .ToArray();

  private static IEnumerable<AttributeData> InheritedExpressionAttributes(INamedTypeSymbol type) {
    var hierarchy = new Stack<INamedTypeSymbol>();
    for (var current = type; current is not null; current = current.BaseType) hierarchy.Push(current);
    while (hierarchy.Count != 0)
      foreach (var attribute in OrderedAttributes(hierarchy.Pop()))
        if (attribute.AttributeClass?.ToDisplayString() == Attributes.MixinExpression)
          yield return attribute;
  }

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