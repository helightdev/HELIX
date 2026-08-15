using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static HELIX.SourceGen.GeneratorAnalysis;
using static HELIX.SourceGen.GeneratorSource;
using static HELIX.SourceGen.GeneratorStrings;

namespace HELIX.SourceGen {
  [Generator(LanguageNames.CSharp)]
  public sealed class ComponentDiscoveryGenerator : IIncrementalGenerator {
    private const string RootAssembly = "Assembly-CSharp";
    private const string ComponentRegistrations = "HELIX.Context.ComponentRegistrations";
    private const string RegistrationConfigurator = "RegistrationConfigurator";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      context.RegisterSourceOutput(
        context.CompilationProvider,
        static (productionContext, compilation) => Generate(productionContext, compilation)
      );
    }

    private static void Generate(SourceProductionContext context, Compilation compilation) {
      if (!string.Equals(compilation.AssemblyName, RootAssembly, StringComparison.Ordinal) ||
          compilation.GetTypeByMetadataName(ComponentRegistrations) is null) return;

      var stereotypeAssembly = compilation.GetTypeByMetadataName(
        "HELIX.Context.ComponentAttribute"
      )?.ContainingAssembly;
      if (stereotypeAssembly is null) return;

      var components = CandidateAssemblies(compilation, stereotypeAssembly)
        .SelectMany(TypesIn)
        .Where(type => type.TypeKind == TypeKind.Class &&
                       !HasTypeParameters(type) &&
                       HasBuiltinStereotype(type) &&
                       compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        .Distinct(SymbolEqualityComparer.Default)
        .OrderBy(type => type.ToDisplayString(TypeDisplayFormat), StringComparer.Ordinal)
        .ToArray();

      context.AddSource("ComponentDiscovery.g.cs", BuildSource(builder => {
        builder.AppendLine("public static class ComponentDiscovery");
        using (builder.Block()) {
          builder.AppendLine(
            "public static global::HELIX.Context.ComponentRegistrations Discover()"
          );
          using (builder.Block()) {
            builder.Statement(
              "var registrations = new global::HELIX.Context.ComponentRegistrations()"
            );
            foreach (var component in components) {
              var type = component.ToDisplayString(TypeDisplayFormat);
              builder.Statement(
                "registrations.Register(typeof(" + type + "), " + type + "." +
                RegistrationConfigurator + ")"
              );
            }
            builder.Return("registrations");
          }
        }
      }));
    }

    private static IEnumerable<IAssemblySymbol> CandidateAssemblies(
      Compilation compilation,
      IAssemblySymbol stereotypeAssembly
    ) {
      yield return compilation.Assembly;
      foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols) {
        if (SymbolEqualityComparer.Default.Equals(assembly, stereotypeAssembly) ||
            assembly.Modules.Any(module => module.ReferencedAssemblySymbols.Any(reference =>
              SymbolEqualityComparer.Default.Equals(reference, stereotypeAssembly)))) {
          yield return assembly;
        }
      }
    }

    private static IEnumerable<INamedTypeSymbol> TypesIn(IAssemblySymbol assembly) =>
      TypesIn(assembly.GlobalNamespace);

    private static IEnumerable<INamedTypeSymbol> TypesIn(INamespaceSymbol @namespace) {
      foreach (var type in @namespace.GetTypeMembers()) {
        yield return type;
        foreach (var nested in NestedTypes(type)) yield return nested;
      }
      foreach (var child in @namespace.GetNamespaceMembers()) {
        foreach (var type in TypesIn(child)) yield return type;
      }
    }

    private static IEnumerable<INamedTypeSymbol> NestedTypes(INamedTypeSymbol containing) {
      foreach (var type in containing.GetTypeMembers()) {
        yield return type;
        foreach (var nested in NestedTypes(type)) yield return nested;
      }
    }

    private static bool HasBuiltinStereotype(INamedTypeSymbol type) => type.GetAttributes().Any(
      attribute => BuiltinMixinStereotypes.Contains(
        attribute.AttributeClass?.ToDisplayString() ?? "", StringComparer.Ordinal
      )
    );
  }
}
