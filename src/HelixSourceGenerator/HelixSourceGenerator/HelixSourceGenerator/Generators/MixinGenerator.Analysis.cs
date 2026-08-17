using System;
using System.Collections.Generic;
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

public sealed partial class MixinGenerator {
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
      (InheritedAttributes(type, Attributes.MixinExpression, false).Count != 0 ||
        InheritedAttributes(type, Attributes.AttributeMixinMethodProxy, true).Count != 0);
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

  private static void CollectLocalContributions(
    SourceProductionContext context,
    INamedTypeSymbol target,
    ICollection<MixinContribution> result
  ) {
    var sequence = 0;
    foreach (var method in OrderedMembers(target).OfType<IMethodSymbol>()) {
      var attribute = Attribute(method, Attributes.MixinMethod);
      if (attribute is null) continue;
      if (TryCreateContribution(
        context, target, method, attribute, ContributionKind.Local, target, null, sequence++, out var item
      )) result.Add(item);
    }
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
      foreach (var method in OrderedMembers(mixin).OfType<IMethodSymbol>()) {
        var attribute = Attribute(method, Attributes.MixinMethod);
        if (attribute is null) continue;
        if (TryCreateContribution(
          context, target, method, attribute, ContributionKind.Interface, mixin, null, sequence++, out var item
        )) result.Add(item);
      }
    }
  }

  private static void CollectAttributeContributions(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<MixinResource> resources,
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
        foreach (var proxy in InheritedAttributes(
          attributeType, Attributes.AttributeMixinMethodProxy, true
        )) {
          if (proxy.ConstructorArguments.Length < 2 ||
            proxy.ConstructorArguments[0].Value is not INamedTypeSymbol owner) continue;

          if (TrySelectProxy(
            context, target, annotated, applied, null, owner, proxy, resources, compilation,
            preparedExpressions, expressionVariables, sequence, out var selected, out var failures
          )) {
            result.Add(selected);
            sequence++;
          } else {
            context.ReportDiagnostic(
              Diagnostic.Create(
                NoMatchingVariant,
                applied.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated),
                attributeType.Name,
                annotated.Name,
                failures
              )
            );
          }
        }
      }
    }
    foreach (var implicitAttribute in implicitAttributes) {
      CollectImplicitAttributeContributions(
        context, target, implicitAttribute, resources, compilation, preparedExpressions,
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
    IReadOnlyList<MixinResource> resources,
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
    foreach (var proxy in InheritedAttributes(
      implicitAttribute.Type, Attributes.AttributeMixinMethodProxy, true
    )) {
      if (proxy.ConstructorArguments.Length < 2 ||
        proxy.ConstructorArguments[0].Value is not INamedTypeSymbol owner) continue;
      if (TrySelectProxy(
        context, target, target, null, implicitAttribute, owner, proxy, resources, compilation,
        preparedExpressions, expressionVariables, sequence, out var selected, out var failures
      )) {
        result.Add(selected);
        sequence++;
      } else {
        context.ReportDiagnostic(
          Diagnostic.Create(
            NoMatchingVariant, LocationOf(target), implicitAttribute.Type.Name, target.Name,
            failures
          )
        );
      }
    }
  }

  private static bool TrySelectProxy(
    SourceProductionContext context,
    INamedTypeSymbol target,
    ISymbol source,
    AttributeData applied,
    ImplicitMixinAttribute implicitAttribute,
    INamedTypeSymbol owner,
    AttributeData proxy,
    IReadOnlyList<MixinResource> resources,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    int sequence,
    out MixinContribution selected,
    out string failures
  ) {
    selected = null;
    List<string> errors = null;
    var members = OrderedMembers(owner);
    foreach (var methodName in ProxyVariants(proxy)) {
      var found = false;
      foreach (var member in members) {
        if (member is not IMethodSymbol method || method.Name != methodName) continue;
        found = true;
        var mixin = Attribute(method, Attributes.MixinMethod);
        if (mixin is null) {
          (errors ??= new List<string>()).Add($"method '{methodName}' is not marked [MixinMethod]");
          continue;
        }
        if (!TryCreateContribution(
          context, target, method, mixin, ContributionKind.Attribute, source, applied,
          sequence, out var item
        )) continue;
        if (implicitAttribute is not null) item.WithImplicitAttribute(implicitAttribute);
        if (TrySpecializeAndCheck(
          context, target, resources, compilation, preparedExpressions,
          expressionVariables, item, out selected, out var failure
        )) break;
        (errors ??= new List<string>()).Add($"'{methodName}': {failure}");
      }
      if (selected is not null) break;
      if (!found) (errors ??= new List<string>()).Add($"method '{methodName}' was not found");
    }
    failures = errors is null ? "no variants were configured" : string.Join("; ", errors);
    return selected is not null;
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
        ? targets[0] : output.InjectionTarget;
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
            null, output.InjectionTarget, output.InjectionPriority, null,
            contributionKind, sequence++, annotated, applied,
            Array.Empty<MixinParameter>(), targetDefinitions
          ).WithImplicitAttribute(implicitAttribute).WithExpressionResult(result)
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
        ? targets[0] : output.InjectionTarget;
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
          null, declaration.Target, declaration.Order, null,
          contributionKind, sequence++, annotated, applied,
          Array.Empty<MixinParameter>(), targetDefinitions
        ).WithImplicitAttribute(implicitAttribute).WithExpressionResult(result)
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

  private static IReadOnlyList<string> ProxyVariants(AttributeData proxy) {
    if (proxy.ConstructorArguments.Length < 2) return Array.Empty<string>();
    var value = proxy.ConstructorArguments[1];
    if (value.Kind == TypedConstantKind.Array) {
      return value.Values.Select(item => item.Value as string)
        .Where(item => !string.IsNullOrWhiteSpace(item))
        .ToArray();
    }
    return value.Value is string method ? new[] { method } : Array.Empty<string>();
  }

  private static void SpecializeContributions(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<MixinResource> resources,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    IList<MixinContribution> contributions
  ) {
    for (var index = 0; index < contributions.Count;) {
      if (contributions[index].Method is null) {
        index++;
        continue;
      }
      if (TrySpecializeAndCheck(
        context, target, resources, compilation, preparedExpressions,
        expressionVariables, contributions[index],
        out var resolved, out var failure
      )) {
        contributions[index] = resolved;
        index++;
        continue;
      }
      ReportInvalidMethod(context, contributions[index].Method, failure);
      contributions.RemoveAt(index);
    }
  }

  private static bool TrySpecializeAndCheck(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<MixinResource> resources,
    CSharpCompilation compilation,
    MixinExpressionPreparedState preparedExpressions,
    IDictionary<string, object> expressionVariables,
    MixinContribution contribution,
    out MixinContribution resolved,
    out string failure
  ) {
    resolved = null;
    failure = null;
    var candidate = contribution;
    var selectors = OrderedAttributes(contribution.Method)
      .Where(item => IsAttribute(item, Attributes.MixinMethodGenericSource))
      .ToArray();
    if (selectors.Length != 0) {
      if (!contribution.Method.IsGenericMethod) {
        failure = "generic sources are declared on a non-generic method";
        return false;
      }

      var arguments = new ITypeSymbol[contribution.Method.TypeParameters.Length];
      foreach (var selector in selectors) {
        if (!TryReadGenericSelector(
          selector, out var genericIndex, out var source, out var sourceName,
          out var sourceIndex
        ) || genericIndex < 0 || genericIndex >= arguments.Length) {
          failure = "a generic source has an invalid generic parameter index or source";
          return false;
        }
        if (!TryResolveSelectorType(
          target, resources, contribution, compilation, genericIndex, source, sourceName,
          sourceIndex, out var sourceType, out failure
        )) return false;
        if (arguments[genericIndex] is not null &&
          !SymbolEqualityComparer.Default.Equals(arguments[genericIndex], sourceType)) {
          failure = $"generic parameter {genericIndex} has conflicting sources";
          return false;
        }
        arguments[genericIndex] = sourceType;
      }

      var missing = Array.FindIndex(arguments, item => item is null);
      if (missing >= 0) {
        failure = $"generic parameter {missing} has no [MixinMethodGenericSource]";
        return false;
      }

      var constructed = contribution.Method.Construct(arguments);
      candidate = contribution.WithMethod(constructed, arguments);
    }

    if (!TryCheckKnownAssignments(target, resources, compilation, candidate, out failure)) return false;
    if (!string.IsNullOrWhiteSpace(candidate.Expression)) {
      var arguments = candidate.Source is IMethodSymbol sourceMethod
        ? (IReadOnlyList<IParameterSymbol>)sourceMethod.Parameters
        : candidate.Method.Parameters;
      var expressionTarget = candidate.Kind == ContributionKind.Attribute
        ? candidate.Source
        : target;
      var expressionContext = new RoslynMixinExpressionContext(
        target, expressionTarget, candidate.AppliedAttribute, arguments, compilation,
        candidate.ImplicitAttribute?.Type, candidate.ImplicitAttribute?.Values,
        candidate.TargetDefinitions
      );
      var expressionResult = new MixinExpressionInterpreter().Execute(
        candidate.Expression, expressionContext, expressionVariables, preparedExpressions
      );
      ReportExpressionLogs(
        context,
        candidate.AppliedAttribute?.ApplicationSyntaxReference?.GetSyntax().GetLocation() ??
        LocationOf(candidate.Source ?? target),
        expressionResult.Logs
      );
      if (!expressionResult.Success) {
        failure = "expression line " + expressionResult.ErrorLine.ToString(CultureInfo.InvariantCulture) +
          ": " + expressionResult.Error;
        return false;
      }
      candidate = candidate.WithExpressionResult(expressionResult);
    }
    resolved = candidate;
    return true;
  }

  private static void ReportExpressionLogs(
    SourceProductionContext context,
    Location location,
    IReadOnlyList<MixinExpressionLog> logs
  ) {
    foreach (var log in logs) context.ReportDiagnostic(
      Diagnostic.Create(log.IsHint ? ExpressionHint : ExpressionLog, location, log.Text)
    );
  }

  private static bool TryReadGenericSelector(
    AttributeData selector,
    out int genericIndex,
    out int source,
    out string sourceName,
    out int sourceIndex
  ) {
    genericIndex = -1;
    source = -1;
    sourceName = null;
    sourceIndex = -1;
    if (selector.ConstructorArguments.Length < 2 ||
      !TryConvertToInt32(selector.ConstructorArguments[0].Value, out genericIndex) ||
      !TryConvertToInt32(selector.ConstructorArguments[1].Value, out source) ||
      source is < InjectThis or > InjectParameter) return false;
    if (selector.ConstructorArguments.Length < 3) return true;
    var selected = selector.ConstructorArguments[2];
    if (selected.Type?.SpecialType == SpecialType.System_String) sourceName = selected.Value as string;
    else if (!TryConvertToInt32(selected.Value, out sourceIndex)) return false;
    return true;
  }

  private static bool TryResolveSelectorType(
    INamedTypeSymbol target,
    IReadOnlyList<MixinResource> resources,
    MixinContribution contribution,
    CSharpCompilation compilation,
    int genericIndex,
    int source,
    string sourceName,
    int sourceIndex,
    out ITypeSymbol type,
    out string failure
  ) {
    type = null;
    failure = null;
    var defaultName = contribution.Method.TypeParameters[genericIndex].Name;
    switch (source) {
      case InjectThis:
        type = target;
        break;
      case InjectTarget:
        type = ReferencedTargetType(target, contribution, compilation, null);
        break;
      case InjectAttribute:
        type = AttributeSourceType(
          contribution, sourceName ?? defaultName, sourceIndex
        );
        break;
      case InjectMember:
        type = FindResource(resources, sourceName ?? defaultName)?.Type;
        break;
      case InjectParameter:
        type = ReferencedParameterType(contribution, sourceName, sourceIndex, genericIndex);
        break;
      case InjectReturnValue:
        type = contribution.Parameters.FirstOrDefault(item =>
          item.Injection == InjectReturnValue
        )?.Symbol.Type;
        break;
      case InjectDelegate:
        failure = "a delegate method group has no standalone source type";
        return false;
    }
    if (type is not null) return true;
    failure = $"generic parameter {genericIndex} could not resolve its selected source";
    return false;
  }

  private static ITypeSymbol ReferencedParameterType(
    MixinContribution contribution,
    string name,
    int sourceIndex,
    int fallbackIndex
  ) {
    if (contribution.Kind == ContributionKind.Attribute && contribution.Source is IMethodSymbol method)
      return SelectParameter(method.Parameters, name, sourceIndex, fallbackIndex)?.Type;
    var selected = sourceIndex >= 0 ? sourceIndex : fallbackIndex;
    IParameterSymbol insensitive = null;
    foreach (var parameter in contribution.Parameters) {
      if (parameter.Injection != Unmarked) continue;
      if (!string.IsNullOrEmpty(name)) {
        if (string.Equals(parameter.Symbol.Name, name, StringComparison.Ordinal)) return parameter.Symbol.Type;
        if (insensitive is null && string.Equals(
          parameter.Symbol.Name, name, StringComparison.OrdinalIgnoreCase
        )) insensitive = parameter.Symbol;
      } else if (parameter.Position == selected) return parameter.Symbol.Type;
    }
    return insensitive?.Type;
  }

  private static IParameterSymbol SelectParameter(
    IReadOnlyList<IParameterSymbol> parameters,
    string name,
    int index,
    int fallbackIndex
  ) {
    if (!string.IsNullOrEmpty(name)) {
      IParameterSymbol insensitive = null;
      foreach (var parameter in parameters) {
        if (string.Equals(parameter.Name, name, StringComparison.Ordinal)) return parameter;
        if (insensitive is null && string.Equals(
          parameter.Name, name, StringComparison.OrdinalIgnoreCase
        )) insensitive = parameter;
      }
      return insensitive;
    }
    var selected = index >= 0 ? index : fallbackIndex;
    return selected >= 0 && selected < parameters.Count ? parameters[selected] : null;
  }

  private static bool TryCheckKnownAssignments(
    INamedTypeSymbol target,
    IReadOnlyList<MixinResource> resources,
    CSharpCompilation compilation,
    MixinContribution contribution,
    out string failure
  ) {
    failure = null;
    foreach (var parameter in contribution.Parameters) {
      if (!parameter.CheckAssignment) continue;
      if (parameter.Injection is InjectParameter or InjectReturnValue) continue;
      if (parameter.Injection == InjectDelegate) {
        if (contribution.Source is IMethodSymbol method &&
          IsMethodGroupAssignable(compilation, method, parameter.Symbol.Type)) continue;
        failure = $"delegate target is not assignable to parameter '{parameter.Symbol.Name}'";
        return false;
      }

      ITypeSymbol sourceType = null;
      switch (parameter.Injection) {
        case InjectThis:
          sourceType = target;
          break;
        case InjectTarget:
          sourceType = ReferencedTargetType(target, contribution, compilation, parameter.Symbol.Type);
          break;
        case InjectMember:
          sourceType = FindResource(resources, parameter.Name ?? parameter.Symbol.Name)?.Type;
          break;
        case InjectAttribute:
          sourceType = AttributeSourceType(
            contribution, parameter.Name ?? parameter.Symbol.Name, -1
          );
          break;
      }
      if (sourceType is not null && IsAssignable(
        compilation, sourceType, parameter.Symbol.Type, parameter.Symbol.RefKind
      )) continue;
      failure = $"source is not assignable to parameter '{parameter.Symbol.Name}'";
      return false;
    }
    return true;
  }

  private static ITypeSymbol ReferencedTargetType(
    INamedTypeSymbol target,
    MixinContribution contribution,
    CSharpCompilation compilation,
    ITypeSymbol destination
  ) {
    if (contribution.Kind != ContributionKind.Attribute) return target;
    switch (contribution.Source) {
      case IFieldSymbol field:
        return field.Type;
      case IPropertySymbol property:
        return property.Type;
      case IMethodSymbol:
        return compilation.GetSpecialType(SpecialType.System_String);
      case INamedTypeSymbol when destination?.SpecialType == SpecialType.System_String:
        return compilation.GetSpecialType(SpecialType.System_String);
      case INamedTypeSymbol:
        return target;
      default:
        return null;
    }
  }

  private static bool IsAssignable(
    CSharpCompilation compilation,
    ITypeSymbol source,
    ITypeSymbol destination,
    RefKind refKind
  ) {
    return refKind == RefKind.None
      ? compilation.ClassifyConversion(source, destination).IsImplicit
      : SymbolEqualityComparer.Default.Equals(source, destination);
  }

  private static bool IsMethodGroupAssignable(
    CSharpCompilation compilation,
    IMethodSymbol method,
    ITypeSymbol destination
  ) {
    if (destination is not INamedTypeSymbol { TypeKind: TypeKind.Delegate } delegateType ||
      delegateType.DelegateInvokeMethod is not { } invoke ||
      method.Parameters.Length != invoke.Parameters.Length) return false;
    for (var index = 0; index < method.Parameters.Length; index++) {
      var sourceParameter = method.Parameters[index];
      var delegateParameter = invoke.Parameters[index];
      if (sourceParameter.RefKind != delegateParameter.RefKind) return false;
      if (sourceParameter.RefKind != RefKind.None) {
        if (!SymbolEqualityComparer.Default.Equals(sourceParameter.Type, delegateParameter.Type)) return false;
      } else if (!compilation.ClassifyConversion(delegateParameter.Type, sourceParameter.Type).IsImplicit) return false;
    }
    if (invoke.ReturnsVoid) return method.ReturnsVoid;
    return !method.ReturnsVoid && compilation.ClassifyConversion(method.ReturnType, invoke.ReturnType).IsImplicit;
  }
}
