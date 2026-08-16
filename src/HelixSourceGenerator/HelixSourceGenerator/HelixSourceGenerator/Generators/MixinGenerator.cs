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

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class MixinGenerator : IIncrementalGenerator {
    private const int Unmarked = -1;
    private const int InjectThis = 0;
    private const int InjectTarget = 1;
    private const int InjectAttribute = 2;
    private const int InjectDelegate = 3;
    private const int InjectReturnValue = 4;
    private const int InjectMember = 5;
    private const int InjectParameter = 6;

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      // An inherited [EnableMixins] cannot be found by an attribute syntax provider. Limit the
      // broader syntax walk to classes which either carry attributes, have a base list, or are
      // partial (the relevant attribute/base list may be on another part). Discard duplicate
      // partial declarations before doing any mixin analysis.
      var targets = context.SyntaxProvider.CreateSyntaxProvider(
          predicate: static (node, _) => node is TypeDeclarationSyntax {
            RawKind: (int)SyntaxKind.ClassDeclaration or (int)SyntaxKind.RecordDeclaration
          } declaration && (declaration.AttributeLists.Count != 0 || declaration.BaseList is not null ||
                            declaration.Modifiers.Any(SyntaxKind.PartialKeyword)),
          transform: static (ctx, _) => GetTarget(ctx)
        )
        .Where(static target => target is not null);

      context.RegisterSourceOutput(targets, static (spc, target) => Generate(spc, target));
    }

    private static MixinTarget GetTarget(GeneratorSyntaxContext context) {
      var declaration = (TypeDeclarationSyntax)context.Node;
      if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol {
            TypeKind: TypeKind.Class
          } type) return null;

      var first = type.DeclaringSyntaxReferences.FirstOrDefault();
      if (first is null || first.SyntaxTree != declaration.SyntaxTree ||
          first.Span.Start != declaration.SpanStart) return null;
      return HasMixinsEnabled(type) && context.SemanticModel.Compilation is CSharpCompilation compilation
        ? new MixinTarget(type, compilation)
        : null;
    }

    private static bool HasMixinsEnabled(INamedTypeSymbol type) {
      for (var current = type; current is not null; current = current.BaseType) {
        if (Attribute(current, Attributes.EnableMixins) is not null ||
            current.GetAttributes().Any(attribute => IsBuiltinMixinStereotype(
              attribute.AttributeClass
            ))) return true;
      }
      return type.AllInterfaces.Any(item => Attribute(item, Attributes.EnableMixins) is not null);
    }

    private static bool IsBuiltinMixinStereotype(INamedTypeSymbol attributeType) {
      for (var current = attributeType; current is not null; current = current.BaseType) {
        if (BuiltinMixinStereotypes.Contains(current.ToDisplayString(), StringComparer.Ordinal)) {
          return true;
        }
      }
      return false;
    }

    private static bool HasComponentStereotype(INamedTypeSymbol type) {
      for (var current = type; current is not null; current = current.BaseType) {
        foreach (var attribute in current.GetAttributes()) {
          for (var attributeType = attribute.AttributeClass;
               attributeType is not null;
               attributeType = attributeType.BaseType) {
            if (attributeType.ToDisplayString() == Attributes.Component) return true;
          }
        }
      }
      return false;
    }

    private static IReadOnlyDictionary<string, string> TargetDefinitions(INamedTypeSymbol type) {
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
                       .Where(item => IsAttribute(item, Attributes.MixinDefineTarget))) {
              ApplyTargetDefinition(definition, definitions);
            }
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

    private static void Generate(SourceProductionContext context, MixinTarget candidate) {
      var target = candidate.Type;
      var location = LocationOf(target);
      if (!IsPartial(target)) {
        context.ReportDiagnostic(Diagnostic.Create(MustBePartial, location, target.Name));
        return;
      }

      var containing = FirstNonPartialContainingType(target);
      if (containing is not null) {
        context.ReportDiagnostic(Diagnostic.Create(
          ContainingTypeMustBePartial, location, target.Name, containing.Name
        ));
        return;
      }

      var interfaces = CollectMixinInterfaces(target).ToList();
      var implicitAttributes = new List<ImplicitMixinAttribute>();
      var implicitInterfaces = new List<INamedTypeSymbol>();
      ResolveMixinRequirements(
        context, target, interfaces, implicitAttributes, implicitInterfaces
      );
      var variables = CollectVariables(context, target, interfaces);
      var properties = CollectProperties(context, target, interfaces);
      var resources = CollectResources(target, variables, properties);
      var contributions = new List<MixinContribution>();
      var attributeExpressionOutputs = new List<MixinExpressionOutput>();
      foreach (var implicitInterface in implicitInterfaces) {
        attributeExpressionOutputs.Add(new MixinExpressionOutput(
          MixinExpressionOutputTarget.Implements,
          implicitInterface.ToDisplayString(TypeDisplayFormat)
        ));
      }
      var expressionVariables = new Dictionary<string, string>(StringComparer.Ordinal);
      CollectLocalContributions(context, target, contributions);
      CollectInterfaceContributions(
        context, target, interfaces, candidate.Compilation, expressionVariables,
        attributeExpressionOutputs, contributions
      );
      SpecializeContributions(
        context, target, resources, candidate.Compilation, expressionVariables, contributions
      );
      CollectAttributeContributions(
        context, target, resources, candidate.Compilation, expressionVariables,
        attributeExpressionOutputs, implicitAttributes, contributions
      );
      var methods = BuildMethods(
        context, target, contributions, resources, candidate.Compilation
      );
      var expressionOutputs = contributions
        .Where(item => item.ExpressionResult is not null)
        .SelectMany(item => item.ExpressionResult.Outputs)
        .Concat(attributeExpressionOutputs)
        .Where(item => !string.IsNullOrEmpty(item.Text))
        .ToArray();
      if (variables.Count == 0 && properties.Count == 0 && methods.Count == 0 &&
          expressionOutputs.Length == 0) return;
      var implemented = expressionOutputs
        .Where(item => item.Target == MixinExpressionOutputTarget.Implements)
        .Select(item => item.Text.Trim())
        .Where(item => item.Length != 0)
        .Distinct(StringComparer.Ordinal)
        .ToArray();
      var wrapper = WrapType(
        target, "mixins", baseType: implemented.Length == 0 ? null : string.Join(", ", implemented),
        typeAttributes: implicitAttributes.Select(item =>
          item.Type.ToDisplayString(TypeDisplayFormat)
        ).ToArray()
      );
      var source = wrapper.Build(builder => {
        if (variables.Count != 0) builder.AppendLine("#pragma warning disable CS0169");
        foreach (var variable in variables) builder.Field(
          "private", variable.Type.ToDisplayString(TypeDisplayFormat), EscapeIdentifier(variable.Name)
        );
        if (variables.Count != 0) builder.AppendLine("#pragma warning restore CS0169");
        if (variables.Count != 0 && (properties.Count != 0 || methods.Count != 0)) builder.BlankLine();

        for (var index = 0; index < properties.Count; index++) {
          AppendProperty(builder, properties[index]);
          if (index != properties.Count - 1 || methods.Count != 0) builder.BlankLine();
        }

        for (var index = 0; index < methods.Count; index++) {
          AppendMethod(builder, methods[index]);
          if (index != methods.Count - 1) builder.BlankLine();
        }
        foreach (var output in expressionOutputs.Where(item =>
                   item.Target == MixinExpressionOutputTarget.Class)) {
          if (variables.Count != 0 || properties.Count != 0 || methods.Count != 0) builder.BlankLine();
          builder.AppendCode(output.Text);
        }
      }, builder => {
        foreach (var output in expressionOutputs.Where(item =>
                   item.Target == MixinExpressionOutputTarget.File)) builder.AppendCode(output.Text);
      }, afterInNamespace: true);
      context.AddSource(wrapper.HintName, source);
    }

    private static IReadOnlyList<INamedTypeSymbol> CollectMixinInterfaces(INamedTypeSymbol target) {
      var result = new List<INamedTypeSymbol>();
      var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

      void Visit(INamedTypeSymbol item) {
        if (!visited.Add(item)) return;
        if (IsMixinInterface(item)) result.Add(item);
        foreach (var inherited in item.Interfaces) Visit(inherited);
      }

      for (var current = target; current is not null; current = current.BaseType) {
        foreach (var item in current.Interfaces) Visit(item);
      }
      return result;
    }

    private static bool IsMixinInterface(INamedTypeSymbol type) =>
      type.ToDisplayString() == Types.Mixin ||
      Attribute(type, Attributes.Mixin) is not null ||
      type.AllInterfaces.Any(item => item.ToDisplayString() == Types.Mixin);

    private static void ResolveMixinRequirements(
      SourceProductionContext context,
      INamedTypeSymbol target,
      IList<INamedTypeSymbol> interfaces,
      ICollection<ImplicitMixinAttribute> implicitAttributes,
      ICollection<INamedTypeSymbol> implicitInterfaces
    ) {
      var interfaceSet = new HashSet<INamedTypeSymbol>(interfaces, SymbolEqualityComparer.Default);
      var explicitClassAttributes = new HashSet<INamedTypeSymbol>(
        target.GetAttributes().Select(item => item.AttributeClass).Where(item => item is not null),
        SymbolEqualityComparer.Default
      );
      var attributeSet = new HashSet<INamedTypeSymbol>(explicitClassAttributes, SymbolEqualityComparer.Default);
      var providers = new Queue<INamedTypeSymbol>();
      foreach (var mixin in interfaces) providers.Enqueue(mixin);

      var providerSymbols = new List<ISymbol> { target };
      providerSymbols.AddRange(OrderedMembers(target).Where(item => !item.IsImplicitlyDeclared));
      foreach (var provider in providerSymbols.SelectMany(item => item.GetAttributes())
                 .Select(item => item.AttributeClass)
                 .Where(IsMixinYieldingAttribute)) providers.Enqueue(provider);

      var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
      while (providers.Count != 0) {
        var provider = providers.Dequeue();
        if (provider is null || !visited.Add(provider)) continue;
        foreach (var requirement in InheritedAttributes(
                   provider, Attributes.RequireMixin, allowMultiple: true)) {
          if (!TryReadRequirement(requirement, out var required, out var declareImplicit) ||
              required is null ||
              !(IsMixinInterface(required) || IsMixinYieldingAttribute(required))) {
            context.ReportDiagnostic(Diagnostic.Create(
              InvalidRequiredMixin, LocationOf(target), provider.Name, target.Name,
              "the target must be an IMixin interface or a mixin-yielding attribute"
            ));
            continue;
          }

          var isInterface = required.TypeKind == TypeKind.Interface;
          var present = isInterface ? interfaceSet.Contains(required) : attributeSet.Contains(required);
          if (present) {
            providers.Enqueue(required);
            continue;
          }
          if (!declareImplicit) {
            context.ReportDiagnostic(Diagnostic.Create(
              MissingRequiredMixin, LocationOf(target), provider.Name, target.Name,
              required.ToDisplayString()
            ));
            continue;
          }

          if (isInterface) {
            if (required.IsUnboundGenericType || ContainsTypeParameter(required)) {
              context.ReportDiagnostic(Diagnostic.Create(
                InvalidRequiredMixin, LocationOf(target), provider.Name, target.Name,
                "implicit interface '" + required.ToDisplayString() + "' must be a closed type"
              ));
              continue;
            }
            interfaceSet.Add(required);
            interfaces.Add(required);
            implicitInterfaces.Add(required);
          } else {
            if (!ImplicitMixinAttribute.TryCreate(required, out var implicitAttribute, out var failure)) {
              context.ReportDiagnostic(Diagnostic.Create(
                InvalidRequiredMixin, LocationOf(target), provider.Name, target.Name, failure
              ));
              continue;
            }
            attributeSet.Add(required);
            implicitAttributes.Add(implicitAttribute);
          }
          providers.Enqueue(required);
        }
      }
    }

    private static bool IsMixinYieldingAttribute(INamedTypeSymbol type) =>
      type is { TypeKind: TypeKind.Class } && InheritsFromSystemAttribute(type) &&
      (InheritedAttributes(type, Attributes.MixinExpression, allowMultiple: false).Count != 0 ||
       InheritedAttributes(type, Attributes.AttributeMixinMethodProxy, allowMultiple: true).Count != 0);

    private static bool InheritsFromSystemAttribute(INamedTypeSymbol type) {
      for (var current = type; current is not null; current = current.BaseType) {
        if (current.ToDisplayString() == "System.Attribute") return true;
      }
      return false;
    }

    private static bool ContainsTypeParameter(ITypeSymbol type) =>
      type.TypeKind == TypeKind.TypeParameter ||
      type is INamedTypeSymbol named && named.TypeArguments.Any(ContainsTypeParameter) ||
      type is IArrayTypeSymbol array && ContainsTypeParameter(array.ElementType);

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
          requirement.ConstructorArguments[1].Value is bool configured) {
        declareImplicit = configured;
      }
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
      IDictionary<string, string> expressionVariables,
      ICollection<MixinExpressionOutput> expressionOutputs,
      ICollection<MixinContribution> result
    ) {
      var sequence = 0;
      foreach (var mixin in interfaces) {
        foreach (var expressionAttribute in OrderedAttributes(mixin)
                   .Where(item => IsAttribute(item, Attributes.MixinExpression))) {
          CollectMixinExpressionContributions(
            context, target, target, null, null, expressionAttribute, compilation,
            expressionVariables, expressionOutputs, result, ref sequence,
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
      IDictionary<string, string> expressionVariables,
      ICollection<MixinExpressionOutput> expressionOutputs,
      IReadOnlyList<ImplicitMixinAttribute> implicitAttributes,
      ICollection<MixinContribution> result
    ) {
      var symbols = new List<ISymbol> { target };
      symbols.AddRange(OrderedMembers(target).Where(item => !item.IsImplicitlyDeclared));
      var sequence = 0;
      foreach (var annotated in symbols) {
        foreach (var applied in OrderedAttributes(annotated)) {
          var attributeType = applied.AttributeClass;
          if (attributeType is null) continue;
          foreach (var expressionAttribute in InheritedAttributes(
                     attributeType, Attributes.MixinExpression, allowMultiple: false)) {
            CollectMixinExpressionContributions(
              context, target, annotated, applied, null, expressionAttribute, compilation,
              expressionVariables, expressionOutputs, result, ref sequence,
              ContributionKind.Attribute, attributeType.Name
            );
          }
          foreach (var proxy in InheritedAttributes(
                     attributeType, Attributes.AttributeMixinMethodProxy, allowMultiple: true)) {
            if (proxy.ConstructorArguments.Length < 2 ||
                proxy.ConstructorArguments[0].Value is not INamedTypeSymbol owner) continue;

            var variants = ProxyVariants(proxy);
            MixinContribution selected = null;
            var failures = new List<string>();
            foreach (var methodName in variants) {
              var methods = OrderedMembers(owner).OfType<IMethodSymbol>()
                .Where(item => item.Name == methodName)
                .ToArray();
              if (methods.Length == 0) {
                failures.Add($"method '{methodName}' was not found");
                continue;
              }

              foreach (var method in methods) {
                var mixin = Attribute(method, Attributes.MixinMethod);
                if (mixin is null) {
                  failures.Add($"method '{methodName}' is not marked [MixinMethod]");
                  continue;
                }
                if (!TryCreateContribution(
                      context, target, method, mixin, ContributionKind.Attribute, annotated, applied,
                      sequence, out var item
                    )) continue;
                if (!TrySpecializeAndCheck(
                      target, resources, compilation, expressionVariables, item, out selected,
                      out var failure
                    )) {
                  failures.Add($"'{methodName}': {failure}");
                  continue;
                }
                break;
              }
              if (selected is not null) break;
            }

            if (selected is not null) {
              result.Add(selected);
              sequence++;
            } else {
              context.ReportDiagnostic(Diagnostic.Create(
                NoMatchingVariant,
                applied.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? LocationOf(annotated),
                attributeType.Name,
                annotated.Name,
                failures.Count == 0 ? "no variants were configured" : string.Join("; ", failures)
              ));
            }
          }
        }
      }
      foreach (var implicitAttribute in implicitAttributes) {
        CollectImplicitAttributeContributions(
          context, target, implicitAttribute, resources, compilation, expressionVariables,
          expressionOutputs, result, ref sequence
        );
      }
    }

    private static void CollectImplicitAttributeContributions(
      SourceProductionContext context,
      INamedTypeSymbol target,
      ImplicitMixinAttribute implicitAttribute,
      IReadOnlyList<MixinResource> resources,
      CSharpCompilation compilation,
      IDictionary<string, string> expressionVariables,
      ICollection<MixinExpressionOutput> expressionOutputs,
      ICollection<MixinContribution> result,
      ref int sequence
    ) {
      foreach (var expressionAttribute in InheritedAttributes(
                 implicitAttribute.Type, Attributes.MixinExpression, allowMultiple: false)) {
        CollectMixinExpressionContributions(
          context, target, target, null, implicitAttribute, expressionAttribute, compilation,
          expressionVariables, expressionOutputs, result, ref sequence,
          ContributionKind.Attribute, implicitAttribute.Type.Name
        );
      }
      foreach (var proxy in InheritedAttributes(
                 implicitAttribute.Type, Attributes.AttributeMixinMethodProxy, allowMultiple: true)) {
        if (proxy.ConstructorArguments.Length < 2 ||
            proxy.ConstructorArguments[0].Value is not INamedTypeSymbol owner) continue;
        MixinContribution selected = null;
        var failures = new List<string>();
        foreach (var methodName in ProxyVariants(proxy)) {
          var methods = OrderedMembers(owner).OfType<IMethodSymbol>()
            .Where(item => item.Name == methodName).ToArray();
          if (methods.Length == 0) {
            failures.Add("method '" + methodName + "' was not found");
            continue;
          }
          foreach (var method in methods) {
            var mixin = Attribute(method, Attributes.MixinMethod);
            if (mixin is null) {
              failures.Add("method '" + methodName + "' is not marked [MixinMethod]");
              continue;
            }
            if (!TryCreateContribution(
                  context, target, method, mixin, ContributionKind.Attribute, target, null,
                  sequence, out var item
                )) continue;
            item.WithImplicitAttribute(implicitAttribute);
            if (!TrySpecializeAndCheck(
                  target, resources, compilation, expressionVariables, item, out selected,
                  out var failure
                )) {
              failures.Add("'" + methodName + "': " + failure);
              continue;
            }
            break;
          }
          if (selected is not null) break;
        }
        if (selected is not null) {
          result.Add(selected);
          sequence++;
        } else {
          context.ReportDiagnostic(Diagnostic.Create(
            NoMatchingVariant, LocationOf(target), implicitAttribute.Type.Name, target.Name,
            failures.Count == 0 ? "no variants were configured" : string.Join("; ", failures)
          ));
        }
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
      IDictionary<string, string> expressionVariables,
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
        declarations.Add(declared, new AttributeExpressionTarget(
          declared, orders[index], targetDefinitions
        ));
      }

      var arguments = annotated is IMethodSymbol method
        ? (IReadOnlyList<IParameterSymbol>)method.Parameters
        : Array.Empty<IParameterSymbol>();
      var expressionContext = new RoslynMixinExpressionContext(
        target, annotated, applied, arguments, compilation, implicitAttribute?.Type,
        implicitAttribute?.Values
      );
      var pendingVariables = new Dictionary<string, string>(expressionVariables, StringComparer.Ordinal);
      var evaluated = new MixinExpressionInterpreter().Execute(
        expression, expressionContext, pendingVariables
      );
      if (!evaluated.Success) {
        ReportInvalidAttributeExpression(
          context, location, attributeName, annotated.Name,
          "line " + evaluated.ErrorLine.ToString(CultureInfo.InvariantCulture) + ": " + evaluated.Error
        );
        return;
      }

      var pendingOutputs = new List<MixinExpressionOutput>();
      var targetOutputs = new Dictionary<string, List<MixinExpressionOutput>>(StringComparer.Ordinal);
      foreach (var output in evaluated.Outputs) {
        if (output.Target is MixinExpressionOutputTarget.Class or
            MixinExpressionOutputTarget.File or MixinExpressionOutputTarget.Implements) {
          if (!string.IsNullOrEmpty(output.Text)) pendingOutputs.Add(output);
          continue;
        }

        string injectionTarget;
        if (output.Target == MixinExpressionOutputTarget.Target) {
          if (targets.Count != 1) {
            ReportInvalidAttributeExpression(
              context, location, attributeName, annotated.Name,
              "@CODE<TARGET> requires exactly one declared target"
            );
            return;
          }
          injectionTarget = targets[0];
        } else {
          injectionTarget = output.InjectionTarget;
        }
        if (string.IsNullOrEmpty(injectionTarget) || !declarations.ContainsKey(injectionTarget)) {
          ReportInvalidAttributeExpression(
            context, location, attributeName, annotated.Name,
            "code target '" + (injectionTarget ?? "") + "' was not declared"
          );
          return;
        }
        if (string.IsNullOrEmpty(output.Text)) continue;
        if (!targetOutputs.TryGetValue(injectionTarget, out var outputs)) {
          outputs = new List<MixinExpressionOutput>();
          targetOutputs.Add(injectionTarget, outputs);
        }
        outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Target, output.Text));
      }

      expressionVariables.Clear();
      foreach (var variable in pendingVariables) expressionVariables[variable.Key] = variable.Value;
      foreach (var output in pendingOutputs) expressionOutputs.Add(output);
      foreach (var item in targetOutputs) {
        var declaration = declarations[item.Key];
        var result = new MixinExpressionResult(true, null, 0, item.Value);
        contributions.Add(new MixinContribution(
          null, declaration.Target, declaration.Order, null,
          contributionKind, sequence++, annotated, applied,
          Array.Empty<MixinParameter>(), targetDefinitions
        ).WithImplicitAttribute(implicitAttribute).WithExpressionResult(result));
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
        : targetArgument.Value is string target ? new[] { target } : Array.Empty<string>();
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
      } else if (TryConvertToInt32(orderArgument.Value, out var order)) {
        orders = new[] { order };
      }
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
    ) => context.ReportDiagnostic(Diagnostic.Create(
      InvalidAttributeExpression, location, attributeName, annotatedName, reason
    ));

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
      IDictionary<string, string> expressionVariables,
      IList<MixinContribution> contributions
    ) {
      for (var index = 0; index < contributions.Count;) {
        if (contributions[index].Method is null) {
          index++;
          continue;
        }
        if (TrySpecializeAndCheck(
              target, resources, compilation, expressionVariables, contributions[index],
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
      INamedTypeSymbol target,
      IReadOnlyList<MixinResource> resources,
      CSharpCompilation compilation,
      IDictionary<string, string> expressionVariables,
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
          if (!TryReadGenericSelector(selector, out var genericIndex, out var source, out var sourceName,
                out var sourceIndex) || genericIndex < 0 || genericIndex >= arguments.Length) {
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
          candidate.ImplicitAttribute?.Type, candidate.ImplicitAttribute?.Values
        );
        var expressionResult = new MixinExpressionInterpreter().Execute(
          candidate.Expression, expressionContext, expressionVariables
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
            item.Injection == InjectReturnValue)?.Symbol.Type;
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
      if (contribution.Kind == ContributionKind.Attribute && contribution.Source is IMethodSymbol method) {
        return SelectParameter(method.Parameters, name, sourceIndex, fallbackIndex)?.Type;
      }
      var positional = contribution.Parameters.Where(item => item.Injection == Unmarked)
        .OrderBy(item => item.Position)
        .Select(item => item.Symbol)
        .ToArray();
      return SelectParameter(positional, name, sourceIndex, fallbackIndex)?.Type;
    }

    private static IParameterSymbol SelectParameter(
      IReadOnlyList<IParameterSymbol> parameters,
      string name,
      int index,
      int fallbackIndex
    ) {
      if (!string.IsNullOrEmpty(name)) {
        return parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal)) ??
               parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
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
      foreach (var parameter in contribution.Parameters.Where(item => item.CheckAssignment)) {
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
    ) => refKind == RefKind.None
      ? compilation.ClassifyConversion(source, destination).IsImplicit
      : SymbolEqualityComparer.Default.Equals(source, destination);

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
        } else if (!compilation.ClassifyConversion(delegateParameter.Type, sourceParameter.Type).IsImplicit) {
          return false;
        }
      }
      if (invoke.ReturnsVoid) return method.ReturnsVoid;
      return !method.ReturnsVoid && compilation.ClassifyConversion(method.ReturnType, invoke.ReturnType).IsImplicit;
    }

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
        context.ReportDiagnostic(Diagnostic.Create(
          InvalidTarget, LocationOf(method), targetName ?? "null",
          source.ContainingType?.Name ?? source.Name, "the target is not a valid method name"
        ));
        return false;
      }

      var order = 0;
      if (attribute.ConstructorArguments.Length > 1 &&
          TryConvertToInt32(attribute.ConstructorArguments[1].Value, out var configuredOrder)) {
        order = configuredOrder;
      }
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
          ReportInvalidMethod(context, method, $"injection on parameter '{parameter.Name}' cannot be passed by reference");
          return false;
        }
        if (injection == InjectAttribute && kind != ContributionKind.Attribute) {
          ReportInvalidMethod(context, method, $"parameter '{parameter.Name}' requests an attribute outside an attribute proxy");
          return false;
        }
        if (injection == InjectDelegate &&
            (kind != ContributionKind.Attribute || source is not IMethodSymbol)) {
          ReportInvalidMethod(context, method, $"parameter '{parameter.Name}' requests a delegate but the annotation target is not a method");
          return false;
        }

        parameters.Add(new MixinParameter(
          parameter, injection, name, injection == Unmarked ? positional++ : -1, checkAssignment
        ));
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
    ) => context.ReportDiagnostic(Diagnostic.Create(
      InvalidMixinMethod, LocationOf(method), method.Name, reason
    ));

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
      if (targetDefinitions is not null && targetDefinitions.TryGetValue(value, out var defined)) {
        value = defined ?? "";
      }
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
    ) => ParseTarget(target, targetDefinitions).Name;

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
            context.ReportDiagnostic(Diagnostic.Create(
              InvalidVariable, LocationOf(mixin), mixin.Name,
              type is null ? "Type must be specified" : $"'{name ?? "null"}' is not a valid name"
            ));
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
            context.ReportDiagnostic(Diagnostic.Create(
              InvalidProperty, LocationOf(property), property.Name,
              property.IsIndexer ? "indexers are not supported" : "static properties are not supported"
            ));
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
                 StringComparer.Ordinal)) {
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
          ReportInvalidMethod(context, contribution.Method,
            "a contribution to a static target must itself be static");
          return false;
        }
        foreach (var parameter in contribution.Parameters) {
          if (isStatic && parameter.Injection == InjectTarget &&
              contribution.Kind == ContributionKind.Attribute &&
              contribution.Source is (IFieldSymbol or IPropertySymbol) &&
              !contribution.Source.IsStatic) {
            ReportInvalidMethod(context, contribution.Method,
              $"target parameter '{parameter.Symbol.Name}' cannot access an instance member from a static target");
            return false;
          }
          if (isStatic && parameter.Injection == InjectDelegate &&
              contribution.Source is IMethodSymbol { IsStatic: false }) {
            ReportInvalidMethod(context, contribution.Method,
              $"delegate parameter '{parameter.Symbol.Name}' cannot access an instance method from a static target");
            return false;
          }
          if (parameter.Injection == InjectMember) {
            if (parameter.Symbol.RefKind is RefKind.Out or RefKind.In) {
              ReportInvalidMethod(context, contribution.Method,
                $"member parameter '{parameter.Symbol.Name}' must be passed by value or ref");
              return false;
            }
            var resourceName = parameter.Name ?? parameter.Symbol.Name;
            var resource = FindResource(resources, resourceName);
            if (resource is null) {
              ReportInvalidMethod(context, contribution.Method,
                $"member parameter '{parameter.Symbol.Name}' does not match a field or property on '{target.Name}'");
              return false;
            }
            if (isStatic && !resource.IsStatic) {
              ReportInvalidMethod(context, contribution.Method,
                $"member parameter '{parameter.Symbol.Name}' cannot access instance member '{resource.Name}' from a static target");
              return false;
            }
            continue;
          }
          if (parameter.Injection != InjectTarget || parameter.Symbol.RefKind == RefKind.None) continue;
          if (parameter.Symbol.RefKind != RefKind.Ref) {
            ReportInvalidMethod(context, contribution.Method,
              $"target parameter '{parameter.Symbol.Name}' must be passed by value or ref");
            return false;
          }
          if (contribution.Kind != ContributionKind.Attribute) {
            ReportInvalidMethod(context, contribution.Method,
              $"the class target parameter '{parameter.Symbol.Name}' cannot be passed by reference");
            return false;
          }
          if (contribution.Source is not (IFieldSymbol or IPropertySymbol)) {
            ReportInvalidMethod(context, contribution.Method,
              $"by-reference target parameter '{parameter.Symbol.Name}' requires a field or property annotation target");
            return false;
          } else if (contribution.Source is IPropertySymbol && parameter.Symbol.RefKind != RefKind.Ref) {
            ReportInvalidMethod(context, contribution.Method,
              $"property target parameter '{parameter.Symbol.Name}' must use ref");
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
            ReportInvalidTarget(context, target, name,
              "the partial declaration does not match the target's static modifier");
            return false;
          }
          if (isPublic && method.DeclaredAccessibility != Accessibility.Public) {
            ReportInvalidTarget(context, target, name,
              "the partial declaration is not public but the target uses the '^' modifier");
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
        ReportInvalidTarget(context, target, name,
          "the inherited declaration is not public but the target uses the '^' modifier");
        return false;
      }

      if (signature is not null && delegateInvoke is not null &&
          !HasSameSignature(signature, delegateInvoke)) {
        ReportInvalidTarget(context, target, name,
          "the partial declaration does not match delegate '" + delegateTargets[0] + "'");
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
            ReportInvalidMethod(context, contribution.Method,
              $"parameter injection '{parameter.Symbol.Name}' does not match a target parameter");
            return false;
          }
          if (parameter.CheckAssignment && !IsAssignable(
                compilation, referenced.TypeSymbol, parameter.Symbol.Type, parameter.Symbol.RefKind
              )) {
            ReportInvalidMethod(context, contribution.Method,
              $"target parameter '{referenced.Name}' is not assignable to '{parameter.Symbol.Name}'");
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
        ReportInvalidMethod(context, checkedReturn.Symbol.ContainingSymbol as IMethodSymbol,
          $"target return value is not assignable to '{checkedReturn.Symbol.Name}'");
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
    ) => context.ReportDiagnostic(Diagnostic.Create(
      InvalidTarget, LocationOf(target), name, target.Name, reason
    ));

    private static bool TryGetUnimplementedPartial(
      IMethodSymbol method,
      out MethodDeclarationSyntax syntax
    ) {
      syntax = method.DeclaringSyntaxReferences
        .Select(item => item.GetSyntax())
        .OfType<MethodDeclarationSyntax>()
        .FirstOrDefault(item => item.Modifiers.Any(SyntaxKind.PartialKeyword) &&
                                item.Body is null && item.ExpressionBody is null);
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
                         !item.IsSealed && (item.IsAbstract || item.IsVirtual || item.IsOverride))
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
    ) => parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal)) ??
         parameters.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));

    private static string BuildMethodDeclaration(
      IMethodSymbol method,
      MethodDeclarationSyntax partialSyntax,
      bool isOverride
    ) {
      var modifiers = new List<string>();
      if (partialSyntax is not null) {
        modifiers.AddRange(partialSyntax.Modifiers
          .Where(item => item.IsKind(SyntaxKind.PublicKeyword) ||
                         item.IsKind(SyntaxKind.PrivateKeyword) ||
                         item.IsKind(SyntaxKind.ProtectedKeyword) ||
                         item.IsKind(SyntaxKind.InternalKeyword) ||
                         item.IsKind(SyntaxKind.StaticKeyword) ||
                         item.IsKind(SyntaxKind.UnsafeKeyword) ||
                         item.IsKind(SyntaxKind.PartialKeyword))
          .Select(item => item.Text));
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
          constraints.Add(parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
            ? "class?"
            : "class");
        }
        if (parameter.HasNotNullConstraint) constraints.Add("notnull");
        constraints.AddRange(parameter.ConstraintTypes.Select(item => item.ToDisplayString(TypeDisplayFormat)));
        if (parameter.HasConstructorConstraint) constraints.Add("new()");
        if (constraints.Count != 0) {
          result.Add("where " + EscapeIdentifier(parameter.Name) + " : " + string.Join(", ", constraints));
        }
      }
      return result;
    }

    private static void AppendProperty(SharpStringBuilder builder, IPropertySymbol property) {
      builder.Append("public ")
        .Append(property.Type.ToDisplayString(TypeDisplayFormat))
        .Append(" ")
        .Append(EscapeIdentifier(property.Name))
        .Append(" { ");
      if (property.GetMethod is not null) builder.Append("get; ");
      if (property.SetMethod is not null) builder.Append(property.SetMethod.IsInitOnly ? "init; " : "set; ");
      builder.AppendLine("}");
    }

    private static void AppendMethod(SharpStringBuilder builder, GeneratedMethod method) {
      var declaration = method.Declaration;
      if (method.TypeParameters.Count != 0 && !declaration.EndsWith(
            ">", StringComparison.Ordinal)) {
        declaration += "<" + string.Join(", ", method.TypeParameters) + ">";
      }
      var parameters = method.Parameters.Select(item => item.Declaration).ToArray();
      builder.Append(declaration).Parameters(parameters, parameters.Length > 1);
      foreach (var constraint in method.Constraints) builder.Append(" ").Append(constraint);
      using (builder.Block()) {
        if (method.ReturnType is not null) {
          builder.Statement(method.ReturnType + " __mixinReturnValue = default");
        }

        var baseEmitted = !method.CallBase;
        var invocationIndex = 0;
        foreach (var contribution in method.Contributions) {
          if (!baseEmitted && contribution.Order >= 0) {
            AppendBaseCall(builder, method);
            baseEmitted = true;
          }
          AppendInvocation(builder, method, contribution, invocationIndex++);
        }
        if (!baseEmitted) AppendBaseCall(builder, method);
        if (method.ReturnType is not null) builder.Return("__mixinReturnValue");
      }
    }

    private static void AppendBaseCall(SharpStringBuilder builder, GeneratedMethod method) {
      var arguments = method.Parameters.Select(item => item.Argument).ToArray();
      var call = "base." + EscapeIdentifier(method.Name) + "(" + string.Join(", ", arguments) + ")";
      builder.Statement(method.ReturnType is null ? call : "__mixinReturnValue = " + call);
    }

    private static void AppendInvocation(
      SharpStringBuilder builder,
      GeneratedMethod target,
      MixinContribution contribution,
      int invocationIndex
    ) {
      if (contribution.ExpressionResult is not null) {
        foreach (var output in contribution.ExpressionResult.Outputs.Where(item =>
                   item.Target == MixinExpressionOutputTarget.Target)) builder.Statement(output.Text);
        return;
      }
      var before = new List<string>();
      var after = new List<string>();
      var arguments = new List<string>(contribution.Parameters.Count);
      foreach (var parameter in contribution.Parameters) {
        var expression = BuildArgument(
          target, contribution, parameter, invocationIndex, before, after
        );
        arguments.Add(RefPrefix(parameter.Symbol.RefKind) + expression);
      }

      foreach (var statement in before) builder.Statement(statement);
      var call = InvocationTarget(contribution) + "(" + string.Join(", ", arguments) + ")";
      if (contribution.Method.ReturnType.SpecialType == SpecialType.System_Boolean) {
        if (after.Count == 0) {
          using (builder.If("!" + call)) {
            AppendEarlyReturn(builder, target);
          }
        } else {
          var continueName = "__mixinContinue" + invocationIndex.ToString(CultureInfo.InvariantCulture);
          builder.Statement("var " + continueName + " = " + call);
          foreach (var statement in after) builder.Statement(statement);
          using (builder.If("!" + continueName)) {
            AppendEarlyReturn(builder, target);
          }
        }
      } else {
        builder.Statement(call);
        foreach (var statement in after) builder.Statement(statement);
      }
    }

    private static void AppendEarlyReturn(SharpStringBuilder builder, GeneratedMethod target) {
      if (target.ReturnType is null) builder.Statement("return");
      else builder.Return("__mixinReturnValue");
    }

    private static string BuildArgument(
      GeneratedMethod target,
      MixinContribution contribution,
      MixinParameter parameter,
      int invocationIndex,
      ICollection<string> before,
      ICollection<string> after
    ) {
      switch (parameter.Injection) {
        case Unmarked:
          return EscapeIdentifier(target.Parameters[parameter.Position].Name);
        case InjectThis:
          return target.IsStatic ? "null" : "this";
        case InjectReturnValue:
          return "__mixinReturnValue";
        case InjectAttribute:
          return AttributeArgument(contribution, parameter);
        case InjectDelegate:
          return MemberReference((IMethodSymbol)contribution.Source);
        case InjectTarget:
          return TargetArgument(target, contribution, parameter, invocationIndex, before, after);
        case InjectMember:
          return LocalMemberArgument(target, parameter, invocationIndex, before, after);
        case InjectParameter:
          return EscapeIdentifier(FindTargetParameter(
            target.Parameters, parameter.Name ?? parameter.Symbol.Name
          ).Name);
        default:
          return "default";
      }
    }

    private static string TargetArgument(
      GeneratedMethod target,
      MixinContribution contribution,
      MixinParameter parameter,
      int invocationIndex,
      ICollection<string> before,
      ICollection<string> after
    ) {
      if (contribution.Kind != ContributionKind.Attribute) return target.IsStatic ? "null" : "this";
      switch (contribution.Source) {
        case IFieldSymbol field:
          return MemberReference(field);
        case IPropertySymbol property when parameter.Symbol.RefKind == RefKind.Ref:
          var temporary = "__mixinTarget" + invocationIndex.ToString(CultureInfo.InvariantCulture) +
                          "_" + parameter.PositionInMethod.ToString(CultureInfo.InvariantCulture);
          before.Add("var " + temporary + " = " + MemberReference(property));
          after.Add(MemberReference(property) + " = " + temporary);
          return temporary;
        case IPropertySymbol property:
          return MemberReference(property);
        case IMethodSymbol method:
          return Literal(method.Name);
        case INamedTypeSymbol type when parameter.Symbol.Type.SpecialType == SpecialType.System_String:
          return Literal(type.Name);
        case INamedTypeSymbol:
          return target.IsStatic ? "null" : "this";
        default:
          return "default";
      }
    }

    private static string LocalMemberArgument(
      GeneratedMethod target,
      MixinParameter parameter,
      int invocationIndex,
      ICollection<string> before,
      ICollection<string> after
    ) {
      var resource = FindResource(target.Resources, parameter.Name ?? parameter.Symbol.Name);
      if (resource is null) return "default";
      if (!resource.IsProperty || parameter.Symbol.RefKind != RefKind.Ref) return resource.Expression;

      var temporary = "__mixinMember" + invocationIndex.ToString(CultureInfo.InvariantCulture) +
                      "_" + parameter.PositionInMethod.ToString(CultureInfo.InvariantCulture);
      before.Add("var " + temporary + " = " + resource.Expression);
      after.Add(resource.Expression + " = " + temporary);
      return temporary;
    }

    private static MixinResource FindResource(
      IReadOnlyList<MixinResource> resources,
      string name
    ) => resources.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal)) ??
         resources.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));

    private static string AttributeArgument(MixinContribution contribution, MixinParameter parameter) {
      var name = parameter.Name ?? parameter.Symbol.Name;
      if (TryFindAttributeConstant(
            contribution.AppliedAttribute, name, -1, out var constant
          )) return TypedConstantExpression(constant);
      return contribution.ImplicitAttribute is not null &&
             contribution.ImplicitAttribute.Values.TryGetValue(name, out var value)
        ? RoslynMixinExpressionContext.ConstantExpression(value.Value, value.Type)
        : "default";
    }

    private static bool TryFindAttributeConstant(
      AttributeData attribute,
      string name,
      int index,
      out TypedConstant constant
    ) {
      constant = default;
      if (attribute is null) return false;
      if (!string.IsNullOrEmpty(name)) {
        foreach (var argument in attribute.NamedArguments) {
          if (!string.Equals(argument.Key, name, StringComparison.OrdinalIgnoreCase)) continue;
          constant = argument.Value;
          return true;
        }
        var constructor = attribute.AttributeConstructor;
        if (constructor is not null) {
          for (var argumentIndex = 0; argumentIndex < constructor.Parameters.Length &&
                                      argumentIndex < attribute.ConstructorArguments.Length; argumentIndex++) {
            if (!string.Equals(
                  constructor.Parameters[argumentIndex].Name, name, StringComparison.OrdinalIgnoreCase
                )) continue;
            constant = attribute.ConstructorArguments[argumentIndex];
            return true;
          }
        }
      }
      if (index < 0 || index >= attribute.ConstructorArguments.Length) return false;
      constant = attribute.ConstructorArguments[index];
      return true;
    }

    private static ITypeSymbol AttributeSourceType(
      MixinContribution contribution,
      string name,
      int index
    ) {
      var attribute = contribution.AppliedAttribute;
      if (TryFindAttributeConstant(attribute, name, index, out var constant)) return constant.Type;
      if (contribution.ImplicitAttribute is { } implicitAttribute) {
        if (!string.IsNullOrEmpty(name) && implicitAttribute.Values.TryGetValue(name, out var value)) {
          return value.Type;
        }
        return implicitAttribute.Type.GetMembers().FirstOrDefault(item =>
          string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)
        ) switch {
          IFieldSymbol field => field.Type,
          IPropertySymbol property => property.Type,
          _ => null
        };
      }
      if (attribute?.AttributeClass is null || string.IsNullOrEmpty(name)) return null;
      foreach (var member in attribute.AttributeClass.GetMembers()) {
        if (!string.Equals(member.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
        if (member is IFieldSymbol field) return field.Type;
        if (member is IPropertySymbol property) return property.Type;
      }
      return null;
    }

    private static string TypedConstantExpression(TypedConstant constant) {
      if (constant.IsNull) return "null";
      if (constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type) {
        return "typeof(" + type.ToDisplayString(TypeDisplayFormat) + ")";
      }
      if (constant.Kind == TypedConstantKind.Array) {
        var element = (constant.Type as IArrayTypeSymbol)?.ElementType.ToDisplayString(TypeDisplayFormat) ?? "object";
        return "new " + element + "[] { " +
               string.Join(", ", constant.Values.Select(TypedConstantExpression)) + " }";
      }
      var literal = CSharpLiteral(constant.Value);
      return constant.Kind == TypedConstantKind.Enum && constant.Type is not null
        ? "(" + constant.Type.ToDisplayString(TypeDisplayFormat) + ")" + literal
        : literal;
    }

    private static string CSharpLiteral(object value) {
      if (value is null) return "null";
      return value switch {
        string text => Literal(text),
        char character => SyntaxFactory.Literal(character).ToFullString(),
        bool boolean => boolean ? "true" : "false",
        float single when float.IsNaN(single) => "global::System.Single.NaN",
        float single when float.IsPositiveInfinity(single) => "global::System.Single.PositiveInfinity",
        float single when float.IsNegativeInfinity(single) => "global::System.Single.NegativeInfinity",
        float single => single.ToString("R", CultureInfo.InvariantCulture) + "F",
        double number when double.IsNaN(number) => "global::System.Double.NaN",
        double number when double.IsPositiveInfinity(number) => "global::System.Double.PositiveInfinity",
        double number when double.IsNegativeInfinity(number) => "global::System.Double.NegativeInfinity",
        double number => number.ToString("R", CultureInfo.InvariantCulture) + "D",
        decimal number => number.ToString(CultureInfo.InvariantCulture) + "M",
        uint number => number.ToString(CultureInfo.InvariantCulture) + "U",
        long number => number.ToString(CultureInfo.InvariantCulture) + "L",
        ulong number => number.ToString(CultureInfo.InvariantCulture) + "UL",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)
      };
    }

    private static string Literal(string value) => SyntaxFactory.Literal(value).ToFullString();

    private static string InvocationTarget(MixinContribution contribution) {
      var genericArguments = contribution.GenericArguments.Count == 0
        ? ""
        : "<" + string.Join(", ", contribution.GenericArguments.Select(item =>
          item.ToDisplayString(TypeDisplayFormat)
        )) + ">";
      if (contribution.Kind == ContributionKind.Local) {
        return contribution.Method.IsStatic
          ? contribution.Method.ContainingType.ToDisplayString(TypeDisplayFormat) + "." +
            EscapeIdentifier(contribution.Method.Name) + genericArguments
          : EscapeIdentifier(contribution.Method.Name) + genericArguments;
      }
      return contribution.Method.ContainingType.ToDisplayString(TypeDisplayFormat) + "." +
             EscapeIdentifier(contribution.Method.Name) + genericArguments;
    }

    private static string MemberReference(ISymbol member) {
      var name = EscapeIdentifier(member.Name);
      return member.IsStatic
        ? member.ContainingType.ToDisplayString(TypeDisplayFormat) + "." + name
        : "this." + name;
    }

    private static string RefPrefix(RefKind kind) => kind switch {
      RefKind.Ref => "ref ",
      RefKind.Out => "out ",
      RefKind.In => "in ",
      _ => ""
    };

    private static IReadOnlyList<ISymbol> OrderedMembers(INamedTypeSymbol type) => type.GetMembers()
      .OrderBy(item => item.Locations.FirstOrDefault(location => location.IsInSource)?.SourceTree?.FilePath,
        StringComparer.Ordinal)
      .ThenBy(SourceOrder)
      .ToArray();

    private static IReadOnlyList<AttributeData> OrderedAttributes(ISymbol symbol) => symbol.GetAttributes()
      .OrderBy(item => item.ApplicationSyntaxReference?.SyntaxTree.FilePath, StringComparer.Ordinal)
      .ThenBy(item => item.ApplicationSyntaxReference?.Span.Start ?? int.MaxValue)
      .ToArray();

    private static IReadOnlyList<AttributeData> InheritedAttributes(
      INamedTypeSymbol type,
      string metadataName,
      bool allowMultiple
    ) {
      var result = new List<AttributeData>();
      for (var current = type; current is not null; current = current.BaseType) {
        var declared = OrderedAttributes(current)
          .Where(item => IsAttribute(item, metadataName))
          .ToArray();
        if (declared.Length == 0) continue;
        result.AddRange(declared);
        if (!allowMultiple) break;
      }
      return result;
    }

    private static bool IsAttribute(AttributeData attribute, string metadataName) =>
      attribute.AttributeClass?.ToDisplayString() == metadataName;

    private static IReadOnlyList<MixinResource> CollectResources(
      INamedTypeSymbol target,
      IReadOnlyList<MixinVariable> variables,
      IReadOnlyList<IPropertySymbol> properties
    ) {
      var result = new List<MixinResource>();
      var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      for (var current = target; current is not null; current = current.BaseType) {
        foreach (var member in OrderedMembers(current)) {
          if (!SymbolEqualityComparer.Default.Equals(current, target) &&
              member.DeclaredAccessibility == Accessibility.Private) continue;
          if (member.IsImplicitlyDeclared || member is not (IFieldSymbol or IPropertySymbol) ||
              !names.Add(member.Name)) continue;
          result.Add(new MixinResource(
            member.Name,
            MemberReference(member),
            member is IPropertySymbol property ? property.Type : ((IFieldSymbol)member).Type,
            member is IPropertySymbol,
            member.IsStatic
          ));
        }
      }
      foreach (var variable in variables) {
        if (names.Add(variable.Name)) result.Add(new MixinResource(
          variable.Name, "this." + EscapeIdentifier(variable.Name), variable.Type, false, false
        ));
      }
      foreach (var property in properties) {
        if (names.Add(property.Name)) result.Add(new MixinResource(
          property.Name, "this." + EscapeIdentifier(property.Name), property.Type, true, false
        ));
      }
      return result;
    }

    private enum ContributionKind { Local, Interface, Attribute }

    private sealed class MixinContribution {
      internal MixinContribution(
        IMethodSymbol method,
        string target,
        int order,
        string expression,
        ContributionKind kind,
        int sequence,
        ISymbol source,
        AttributeData appliedAttribute,
        IReadOnlyList<MixinParameter> parameters,
        IReadOnlyDictionary<string, string> targetDefinitions
      ) {
        var targetSyntax = ParseTarget(target, targetDefinitions);
        Method = method;
        Target = target;
        EmittedTarget = targetSyntax.Name;
        IsStaticTarget = targetSyntax.IsStatic;
        IsPublicTarget = targetSyntax.IsPublic;
        DelegateTarget = targetSyntax.DelegateType;
        Order = order;
        Expression = expression;
        Kind = kind;
        Sequence = sequence;
        Source = source;
        AppliedAttribute = appliedAttribute;
        Parameters = parameters;
        TargetDefinitions = targetDefinitions;
        GenericArguments = Array.Empty<ITypeSymbol>();
        PositionalCount = string.IsNullOrWhiteSpace(expression)
          ? parameters.Count(item => item.Injection == Unmarked)
          : 0;
      }

      internal IMethodSymbol Method { get; }
      internal string Target { get; }
      internal string EmittedTarget { get; }
      internal bool IsStaticTarget { get; }
      internal bool IsPublicTarget { get; }
      internal string DelegateTarget { get; }
      internal int Order { get; }
      internal string Expression { get; }
      internal ContributionKind Kind { get; }
      internal int Sequence { get; }
      internal ISymbol Source { get; }
      internal AttributeData AppliedAttribute { get; }
      internal ImplicitMixinAttribute ImplicitAttribute { get; private set; }
      internal IReadOnlyList<MixinParameter> Parameters { get; }
      internal IReadOnlyList<ITypeSymbol> GenericArguments { get; private set; }
      internal MixinExpressionResult ExpressionResult { get; private set; }
      internal int PositionalCount { get; }
      private IReadOnlyDictionary<string, string> TargetDefinitions { get; }

      internal MixinContribution WithMethod(
        IMethodSymbol method,
        IReadOnlyList<ITypeSymbol> genericArguments
      ) {
        var parameters = new MixinParameter[Parameters.Count];
        for (var index = 0; index < parameters.Length; index++) {
          var source = Parameters[index];
          parameters[index] = new MixinParameter(
            method.Parameters[index], source.Injection, source.Name, source.Position,
            source.CheckAssignment
          );
        }
        var result = new MixinContribution(
          method, Target, Order, Expression, Kind, Sequence, Source,
          AppliedAttribute, parameters, TargetDefinitions
        );
        result.GenericArguments = genericArguments;
        result.ImplicitAttribute = ImplicitAttribute;
        return result;
      }

      internal MixinContribution WithImplicitAttribute(ImplicitMixinAttribute implicitAttribute) {
        ImplicitAttribute = implicitAttribute;
        return this;
      }

      internal MixinContribution WithExpressionResult(MixinExpressionResult expressionResult) {
        ExpressionResult = expressionResult;
        return this;
      }
    }

    private sealed class MixinParameter {
      internal MixinParameter(
        IParameterSymbol symbol,
        int injection,
        string name,
        int position,
        bool checkAssignment
      ) {
        Symbol = symbol;
        Injection = injection;
        Name = name;
        Position = position;
        PositionInMethod = symbol.Ordinal;
        CheckAssignment = checkAssignment;
      }

      internal IParameterSymbol Symbol { get; }
      internal int Injection { get; }
      internal string Name { get; }
      internal int Position { get; }
      internal int PositionInMethod { get; }
      internal bool CheckAssignment { get; }
    }

    private sealed class MixinTargetParameter {
      private MixinTargetParameter(ITypeSymbol type, string name, RefKind refKind, bool isParams) {
        TypeSymbol = type;
        Name = name;
        RefKind = refKind;
        IsParams = isParams;
      }

      internal ITypeSymbol TypeSymbol { get; }
      internal string Type => TypeSymbol.ToDisplayString(TypeDisplayFormat);
      internal string Name { get; }
      internal RefKind RefKind { get; }
      internal bool IsParams { get; }
      internal string Declaration => (IsParams ? "params " : RefPrefix(RefKind)) + Type + " " + EscapeIdentifier(Name);
      internal string Argument => RefPrefix(RefKind) + EscapeIdentifier(Name);

      internal static MixinTargetParameter FromSymbol(IParameterSymbol parameter) => new(
        parameter.Type, parameter.Name, parameter.RefKind, parameter.IsParams
      );
    }

    private sealed class GeneratedMethod {
      internal GeneratedMethod(
        string declaration,
        IReadOnlyList<string> typeParameters,
        IReadOnlyList<string> constraints,
        IReadOnlyList<MixinTargetParameter> parameters,
        string returnType,
        bool callBase,
        bool isStatic,
        string name,
        IReadOnlyList<MixinContribution> contributions,
        IReadOnlyList<MixinResource> resources
      ) {
        Declaration = declaration;
        TypeParameters = typeParameters;
        Constraints = constraints;
        Parameters = parameters;
        ReturnType = returnType;
        CallBase = callBase;
        IsStatic = isStatic;
        Name = name;
        Contributions = contributions;
        Resources = resources;
      }

      internal string Declaration { get; }
      internal IReadOnlyList<string> TypeParameters { get; }
      internal IReadOnlyList<string> Constraints { get; }
      internal IReadOnlyList<MixinTargetParameter> Parameters { get; }
      internal string ReturnType { get; }
      internal bool CallBase { get; }
      internal bool IsStatic { get; }
      internal string Name { get; }
      internal IReadOnlyList<MixinContribution> Contributions { get; }
      internal IReadOnlyList<MixinResource> Resources { get; }
    }

    private sealed class MixinVariable {
      internal MixinVariable(ITypeSymbol type, string name) {
        Type = type;
        Name = name;
      }

      internal ITypeSymbol Type { get; }
      internal string Name { get; }
    }

    private sealed class MixinResource {
      internal MixinResource(
        string name,
        string expression,
        ITypeSymbol type,
        bool isProperty,
        bool isStatic
      ) {
        Name = name;
        Expression = expression;
        Type = type;
        IsProperty = isProperty;
        IsStatic = isStatic;
      }

      internal string Name { get; }
      internal string Expression { get; }
      internal ITypeSymbol Type { get; }
      internal bool IsProperty { get; }
      internal bool IsStatic { get; }
    }

    private sealed class AttributeExpressionTarget {
      internal AttributeExpressionTarget(
        string target,
        int order,
        IReadOnlyDictionary<string, string> targetDefinitions
      ) {
        var targetSyntax = ParseTarget(target, targetDefinitions);
        Target = target;
        EmittedTarget = targetSyntax.Name;
        Order = order;
      }

      internal string Target { get; }
      internal string EmittedTarget { get; }
      internal int Order { get; }
    }

    private sealed class TargetSyntax {
      internal TargetSyntax(string name, bool isStatic, bool isPublic, string delegateType) {
        Name = name;
        IsStatic = isStatic;
        IsPublic = isPublic;
        DelegateType = delegateType;
      }

      internal string Name { get; }
      internal bool IsStatic { get; }
      internal bool IsPublic { get; }
      internal string DelegateType { get; }
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
                         item.Parameters.All(parameter => parameter.HasExplicitDefaultValue))
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

    private sealed class MixinTarget {
      internal MixinTarget(INamedTypeSymbol type, CSharpCompilation compilation) {
        Type = type;
        Compilation = compilation;
      }

      internal INamedTypeSymbol Type { get; }
      internal CSharpCompilation Compilation { get; }
    }
  }
}
