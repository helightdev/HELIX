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
private static bool TryCreateContribution(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IMethodSymbol method,
    AttributeData attribute,
    ContributionKind kind,
    ISymbol source,
    AttributeData appliedAttribute,
    int sequence,
    out MixinContribution contribution
  ) {
    contribution = null;
    if (method.MethodKind != MethodKind.Ordinary) {
      ReportInvalidMethod(context, method, "only ordinary methods are supported");
      return false;
    }
    if (kind != ContributionKind.Local && !method.IsStatic) {
      ReportInvalidMethod(context, method, "interface and attribute proxy methods must be static");
      return false;
    }
    if (method.IsAbstract) {
      ReportInvalidMethod(context, method, "it has no implementation");
      return false;
    }
    if (!method.ReturnsVoid && method.ReturnType.SpecialType != SpecialType.System_Boolean) {
      ReportInvalidMethod(context, method, "it must return void or bool");
      return false;
    }
    if (kind == ContributionKind.Attribute &&
      source is not (INamedTypeSymbol or IMethodSymbol or IFieldSymbol or IPropertySymbol)) {
      ReportInvalidMethod(context, method, "its proxy is applied to an unsupported symbol");
      return false;
    }

    var targetName = attribute.ConstructorArguments.Length > 0
      ? attribute.ConstructorArguments[0].Value as string
      : null;
    if (string.IsNullOrEmpty(targetName)) targetName = ImplicitTarget(method.Name);
    var targetDefinitions = TargetDefinitions(target);
    var emittedName = EmittedTarget(targetName, targetDefinitions);
    if (!IsValidIdentifier(emittedName)) {
      context.ReportDiagnostic(
        Diagnostic.Create(
          InvalidTarget, LocationOf(method), targetName ?? "null",
          source.ContainingType?.Name ?? source.Name, "the target is not a valid method name"
        )
      );
      return false;
    }

    var order = 0;
    if (attribute.ConstructorArguments.Length > 1 &&
      TryConvertToInt32(attribute.ConstructorArguments[1].Value, out var configuredOrder)) order = configuredOrder;
    string expression = null;
    if (attribute.AttributeConstructor is { } attributeConstructor) {
      for (var index = 0; index < attributeConstructor.Parameters.Length &&
        index < attribute.ConstructorArguments.Length; index++) {
        if (attributeConstructor.Parameters[index].Name == "expression") {
          expression = attribute.ConstructorArguments[index].Value as string;
          break;
        }
      }
    }

    var parameters = new List<MixinParameter>(method.Parameters.Length);
    var positional = 0;
    foreach (var parameter in method.Parameters) {
      var inject = Attribute(parameter, Attributes.MixinInject);
      var injection = Unmarked;
      string name = null;
      var checkAssignment = false;
      if (inject is not null) {
        if (inject.ConstructorArguments.Length == 0) injection = InjectTarget;
        else if (!TryConvertToInt32(inject.ConstructorArguments[0].Value, out injection)) {
          ReportInvalidMethod(context, method, $"parameter '{parameter.Name}' uses an invalid injection kind");
          return false;
        }
        if (inject.ConstructorArguments.Length > 1) name = inject.ConstructorArguments[1].Value as string;
        checkAssignment = inject.NamedArguments.Any(item =>
          item.Key == "CheckAssignment" && item.Value.Value is true
        );
      }

      if (injection is < Unmarked or > InjectParameter) {
        ReportInvalidMethod(context, method, $"parameter '{parameter.Name}' uses an unknown injection kind");
        return false;
      }
      if (injection == InjectReturnValue && parameter.RefKind != RefKind.Ref) {
        ReportInvalidMethod(context, method, $"return-value parameter '{parameter.Name}' must be ref");
        return false;
      }
      if (injection is InjectThis or InjectAttribute or InjectDelegate &&
        parameter.RefKind != RefKind.None) {
        ReportInvalidMethod(
          context, method, $"injection on parameter '{parameter.Name}' cannot be passed by reference"
        );
        return false;
      }
      if (injection == InjectAttribute && kind != ContributionKind.Attribute) {
        ReportInvalidMethod(
          context, method, $"parameter '{parameter.Name}' requests an attribute outside an attribute proxy"
        );
        return false;
      }
      if (injection == InjectDelegate &&
        (kind != ContributionKind.Attribute || source is not IMethodSymbol)) {
        ReportInvalidMethod(
          context, method,
          $"parameter '{parameter.Name}' requests a delegate but the annotation target is not a method"
        );
        return false;
      }

      parameters.Add(
        new MixinParameter(
          parameter, injection, name, injection == Unmarked ? positional++ : -1, checkAssignment
        )
      );
    }

    contribution = new MixinContribution(
      method, targetName, order, expression, kind, sequence, source,
      appliedAttribute, parameters, targetDefinitions
    );
    return true;
  }

  private static void ReportInvalidMethod(
    SourceProductionContext context,
    IMethodSymbol method,
    string reason
  ) {
    context.ReportDiagnostic(
      Diagnostic.Create(
        InvalidMixinMethod, LocationOf(method), method.Name, reason
      )
    );
  }

  private static string ImplicitTarget(string methodName) {
    if (methodName.StartsWith("On", StringComparison.Ordinal) && methodName.Length > 2) {
      var target = methodName.Substring(2);
      return target is "Init" or "Dispose" ? "$" + target : target;
    }
    return methodName;
  }

  private static TargetSyntax ParseTarget(
    string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    var value = target ?? "";
    if (targetDefinitions is not null && targetDefinitions.TryGetValue(value, out var defined)) value = defined ?? "";
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
    string delegateType = null;
    if (value.StartsWith("~", StringComparison.Ordinal)) {
      delegateType = value.Substring(1);
      var normalized = delegateType.StartsWith("global::", StringComparison.Ordinal)
        ? delegateType.Substring("global::".Length)
        : delegateType;
      var separator = Math.Max(normalized.LastIndexOf('.'), normalized.LastIndexOf('+'));
      value = separator < 0 ? normalized : normalized.Substring(separator + 1);
    }
    var emitted = value switch { "$Init" => "Awake", "$Dispose" => "OnDestroy", _ => value };
    return new TargetSyntax(emitted, isStatic, isPublic, delegateType);
  }

  private static string EmittedTarget(
    string target,
    IReadOnlyDictionary<string, string> targetDefinitions = null
  ) {
    return ParseTarget(target, targetDefinitions).Name;
  }

  private static List<MixinVariable> CollectVariables(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<INamedTypeSymbol> interfaces
  ) {
    var result = new List<MixinVariable>();
    var names = new HashSet<string>(target.GetMembers().Select(item => item.Name), StringComparer.Ordinal);
    foreach (var mixin in interfaces) {
      foreach (var attribute in OrderedAttributes(mixin)
        .Where(item => IsAttribute(item, Attributes.MixinDeclareVariable))) {
        var type = TypeArgument(attribute, "Type");
        var name = StringArgument(attribute, "Name");
        if (type is null || !IsValidIdentifier(name)) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidVariable, LocationOf(mixin), mixin.Name,
              type is null ? "Type must be specified" : $"'{name ?? "null"}' is not a valid name"
            )
          );
          continue;
        }
        if (!names.Add(name)) continue;
        result.Add(new MixinVariable(type, name));
      }
    }
    return result;
  }

  private static List<IPropertySymbol> CollectProperties(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<INamedTypeSymbol> interfaces
  ) {
    var result = new List<IPropertySymbol>();
    var names = new HashSet<string>(StringComparer.Ordinal);
    foreach (var mixin in interfaces) {
      foreach (var property in OrderedMembers(mixin).OfType<IPropertySymbol>()) {
        if (Attribute(property, Attributes.MixinProperty) is null || !names.Add(property.Name)) continue;
        if (property.IsStatic || property.IsIndexer) {
          context.ReportDiagnostic(
            Diagnostic.Create(
              InvalidProperty, LocationOf(property), property.Name,
              property.IsIndexer ? "indexers are not supported" : "static properties are not supported"
            )
          );
          continue;
        }
        if (target.FindImplementationForInterfaceMember(property) is not null ||
          FindPropertyInHierarchy(target, property.Name) is not null) continue;
        result.Add(property);
      }
    }
    return result;
  }

  private static IPropertySymbol FindPropertyInHierarchy(INamedTypeSymbol target, string name) {
    for (var current = target; current is not null; current = current.BaseType) {
      var property = current.GetMembers(name).OfType<IPropertySymbol>().FirstOrDefault();
      if (property is not null) return property;
    }
    return null;
  }

  private static List<GeneratedMethod> BuildMethods(
    SourceProductionContext context,
    INamedTypeSymbol target,
    IReadOnlyList<MixinContribution> contributions,
    IReadOnlyList<MixinResource> resources,
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
        context, target, ordered[0].EmittedTarget, ordered, resources, compilation, out var method
      )) result.Add(method);
    }
    return result;
  }

  private static bool TryBuildMethod(
    SourceProductionContext context,
    INamedTypeSymbol target,
    string name,
    IReadOnlyList<MixinContribution> contributions,
    IReadOnlyList<MixinResource> resources,
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
    foreach (var contribution in contributions) {
      if (isStatic && contribution.Kind == ContributionKind.Local && !contribution.Method.IsStatic) {
        ReportInvalidMethod(
          context, contribution.Method,
          "a contribution to a static target must itself be static"
        );
        return false;
      }
      foreach (var parameter in contribution.Parameters) {
        if (isStatic && parameter.Injection == InjectTarget &&
          contribution.Kind == ContributionKind.Attribute &&
          contribution.Source is IFieldSymbol or IPropertySymbol &&
          !contribution.Source.IsStatic) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"target parameter '{parameter.Symbol.Name}' cannot access an instance member from a static target"
          );
          return false;
        }
        if (isStatic && parameter.Injection == InjectDelegate &&
          contribution.Source is IMethodSymbol { IsStatic: false }) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"delegate parameter '{parameter.Symbol.Name}' cannot access an instance method from a static target"
          );
          return false;
        }
        if (parameter.Injection == InjectMember) {
          if (parameter.Symbol.RefKind is RefKind.Out or RefKind.In) {
            ReportInvalidMethod(
              context, contribution.Method,
              $"member parameter '{parameter.Symbol.Name}' must be passed by value or ref"
            );
            return false;
          }
          var resourceName = parameter.Name ?? parameter.Symbol.Name;
          var resource = FindResource(resources, resourceName);
          if (resource is null) {
            ReportInvalidMethod(
              context, contribution.Method,
              $"member parameter '{parameter.Symbol.Name}' does not match a field or property on '{target.Name}'"
            );
            return false;
          }
          if (isStatic && !resource.IsStatic) {
            ReportInvalidMethod(
              context, contribution.Method,
              $"member parameter '{parameter.Symbol.Name}' cannot access instance member '{resource.Name}' from a static target"
            );
            return false;
          }
          continue;
        }
        if (parameter.Injection != InjectTarget || parameter.Symbol.RefKind == RefKind.None) continue;
        if (parameter.Symbol.RefKind != RefKind.Ref) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"target parameter '{parameter.Symbol.Name}' must be passed by value or ref"
          );
          return false;
        }
        if (contribution.Kind != ContributionKind.Attribute) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"the class target parameter '{parameter.Symbol.Name}' cannot be passed by reference"
          );
          return false;
        }
        if (contribution.Source is not (IFieldSymbol or IPropertySymbol)) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"by-reference target parameter '{parameter.Symbol.Name}' requires a field or property annotation target"
          );
          return false;
        } else if (contribution.Source is IPropertySymbol && parameter.Symbol.RefKind != RefKind.Ref) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"property target parameter '{parameter.Symbol.Name}' must use ref"
          );
          return false;
        }
      }
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
      baseMethod = FindBaseMethod(target, name, contributions.Max(item => item.PositionalCount));
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

    var parameters = targetSignature is null
      ? InferParameters(contributions)
      : targetSignature.Parameters.Select(MixinTargetParameter.FromSymbol).ToArray();
    if (contributions.Any(item => item.PositionalCount > parameters.Count)) {
      ReportInvalidTarget(context, target, name, "a mixin method has more positional parameters than the target");
      return false;
    }

    foreach (var contribution in contributions) {
      foreach (var parameter in contribution.Parameters.Where(item => item.Injection == InjectParameter)) {
        var referenced = FindTargetParameter(parameters, parameter.Name ?? parameter.Symbol.Name);
        if (referenced is null) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"parameter injection '{parameter.Symbol.Name}' does not match a target parameter"
          );
          return false;
        }
        if (parameter.CheckAssignment && !IsAssignable(
          compilation, referenced.TypeSymbol, parameter.Symbol.Type, parameter.Symbol.RefKind
        )) {
          ReportInvalidMethod(
            context, contribution.Method,
            $"target parameter '{referenced.Name}' is not assignable to '{parameter.Symbol.Name}'"
          );
          return false;
        }
      }
    }

    var returnType = targetSignature?.ReturnType;
    var returnParameter = contributions.SelectMany(item => item.Parameters)
      .FirstOrDefault(item => item.Injection == InjectReturnValue)?.Symbol;
    if (returnType is null) returnType = returnParameter?.Type;
    var returnsVoid = returnType is null || returnType.SpecialType == SpecialType.System_Void;
    if (targetSignature is { ReturnsByRef: true } or { ReturnsByRefReadonly: true }) {
      ReportInvalidTarget(context, target, name, "ref returns are not supported");
      return false;
    }
    if (returnParameter is not null && returnsVoid) {
      ReportInvalidTarget(context, target, name, "return-value injection requires a non-void target");
      return false;
    }
    foreach (var checkedReturn in contributions.SelectMany(item => item.Parameters)
      .Where(item => item.Injection == InjectReturnValue && item.CheckAssignment)) {
      if (IsAssignable(compilation, returnType, checkedReturn.Symbol.Type, RefKind.Ref)) continue;
      ReportInvalidMethod(
        context, checkedReturn.Symbol.ContainingSymbol as IMethodSymbol,
        $"target return value is not assignable to '{checkedReturn.Symbol.Name}'"
      );
      return false;
    }

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
      isStatic,
      name,
      contributions,
      resources
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

  private static IReadOnlyList<MixinTargetParameter> InferParameters(
    IReadOnlyList<MixinContribution> contributions
  ) {
    var source = contributions.OrderByDescending(item => item.PositionalCount).First();
    return source.Parameters.Where(item => item.Injection == Unmarked)
      .OrderBy(item => item.Position)
      .Select(item => MixinTargetParameter.FromSymbol(item.Symbol))
      .ToArray();
  }

  private static MixinTargetParameter FindTargetParameter(
    IReadOnlyList<MixinTargetParameter> parameters,
    string name
  ) {
    return parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal)) ??
      parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
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
}

