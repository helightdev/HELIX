using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class ComponentDiscoveryGenerator : IIncrementalGenerator {
  private const string ComponentRegistrations = "HELIX.Context.ComponentRegistrations";
  private const string RegistrationConfigurator = "RegistrationConfigurator";

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    RegisterModules(
      context,
      context.SyntaxProvider.ForAttributeWithMetadataName(
        Attributes.HelixModule,
        static (node, _) => node is ClassDeclarationSyntax,
        static (ctx, _) => GetModule(ctx, false)
      )
    );
    RegisterModules(
      context,
      context.SyntaxProvider.ForAttributeWithMetadataName(
        Attributes.HelixApplication,
        static (node, _) => node is ClassDeclarationSyntax,
        static (ctx, _) => GetModule(ctx, true)
      )
    );
  }

  private static void RegisterModules(
    IncrementalGeneratorInitializationContext context,
    IncrementalValuesProvider<ModuleCandidate> modules
  ) {
    context.RegisterSourceOutput(
      modules,
      static (productionContext, module) => Generate(productionContext, module)
    );
  }

  private static ModuleCandidate GetModule(
    GeneratorAttributeSyntaxContext context,
    bool isApplication
  ) {
    var type = (INamedTypeSymbol)context.TargetSymbol;
    var attribute = context.Attributes[0];
    var filter = attribute.ConstructorArguments.Length > 1
      ? attribute.ConstructorArguments[1].Value as string
      : null;
    var importArgument = attribute.ConstructorArguments.Length > 2
      ? attribute.ConstructorArguments[2]
      : default;
    var imports = importArgument.IsNull || importArgument.Values.IsDefaultOrEmpty
      ? Array.Empty<INamedTypeSymbol>()
      : importArgument.Values.Select(item => item.Value)
        .OfType<INamedTypeSymbol>()
        .ToArray();
    return new ModuleCandidate(type, context.SemanticModel.Compilation, filter, imports, isApplication);
  }

  private static void Generate(SourceProductionContext context, ModuleCandidate module) {
    var type = module.Type;
    if (!IsPartial(type) || HasTypeParameters(type)) return;

    var components = TypesIn(type.ContainingAssembly.GlobalNamespace)
      .Where(candidate => candidate.TypeKind == TypeKind.Class &&
        !HasTypeParameters(candidate) &&
        MatchesFilter(candidate, module.Filter) &&
        Attribute(candidate, Attributes.Component) is not null &&
        module.Compilation.IsSymbolAccessibleWithin(candidate, type))
      .Distinct(SymbolEqualityComparer.Default)
      .OrderBy(candidate => candidate.ToDisplayString(TypeDisplayFormat), StringComparer.Ordinal)
      .ToArray();

    var wrapper = WrapType(
      type,
      module.IsApplication ? "helix-application" : "helix-module",
      baseType: "global::HELIX.Context.IHelixModule"
    );
    context.AddSource(
      wrapper.HintName,
      wrapper.Build(builder => {
          var moduleType = type.ToDisplayString(TypeDisplayFormat);
          builder.AppendLine(
            "public static readonly " + moduleType + " Instance = new " + moduleType + "();"
          );
          builder.BlankLine();
          builder.AppendLine(
            "public void Discover(global::HELIX.Context.ComponentRegistrations registrations)"
          );
          using (builder.Block()) {
            foreach (var import in module.Imports) {
              builder.Statement(
                import.ToDisplayString(TypeDisplayFormat) + ".Instance.Discover(registrations)"
              );
            }
            foreach (var component in components) {
              var componentType = component.ToDisplayString(TypeDisplayFormat);
              builder.Statement(
                "registrations.Register(typeof(" + componentType + "), " + componentType + "." +
                RegistrationConfigurator + ")"
              );
            }
          }
          if (!module.IsApplication) return;
          builder.BlankLine();
          builder.AppendLine(
            "public static global::HELIX.Context.ComponentRegistrations Discover()"
          );
          using (builder.Block()) {
            builder.Statement(
              "var registrations = new global::HELIX.Context.ComponentRegistrations()"
            );
            builder.Statement("Instance.Discover(registrations)");
            builder.Return("registrations");
          }
        }
      )
    );
  }

  private static bool MatchesFilter(INamedTypeSymbol type, string filter) {
    if (string.IsNullOrEmpty(filter)) return true;
    var @namespace = type.ContainingNamespace is { IsGlobalNamespace: false } value
      ? value.ToDisplayString()
      : "";
    return @namespace == filter || @namespace.StartsWith(filter + ".", StringComparison.Ordinal);
  }

  private static IEnumerable<INamedTypeSymbol> TypesIn(INamespaceSymbol @namespace) {
    foreach (var type in @namespace.GetTypeMembers()) {
      yield return type;
      foreach (var nested in NestedTypes(type)) yield return nested;
    }
    foreach (var child in @namespace.GetNamespaceMembers())
    foreach (var type in TypesIn(child))
      yield return type;
  }

  private static IEnumerable<INamedTypeSymbol> NestedTypes(INamedTypeSymbol containing) {
    foreach (var type in containing.GetTypeMembers()) {
      yield return type;
      foreach (var nested in NestedTypes(type)) yield return nested;
    }
  }

  private sealed record ModuleCandidate(
    INamedTypeSymbol Type,
    Compilation Compilation,
    string Filter,
    IReadOnlyList<INamedTypeSymbol> Imports,
    bool IsApplication
  );
}
