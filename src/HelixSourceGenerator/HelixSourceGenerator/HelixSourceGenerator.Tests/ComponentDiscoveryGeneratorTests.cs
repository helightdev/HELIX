using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using HELIX.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class ComponentDiscoveryGeneratorTests {
  [Fact]
  public void AssemblyCSharpEmitsOneDiscoveryForBuiltinStereotypes() {
    var compilation = CreateCompilation("Assembly-CSharp", RuntimeAndComponents);
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      new ISourceGenerator[] {
        new MixinGenerator().AsSourceGenerator(),
        new ComponentDiscoveryGenerator().AsSourceGenerator()
      }
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
    var discoveryResult = driver.GetRunResult().Results[1];
    var generated = Assert.Single(discoveryResult.GeneratedSources).SourceText.ToString();
    Assert.Contains("public static class ComponentDiscovery", generated);
    Assert.Contains("public static global::HELIX.Context.ComponentRegistrations Discover()", generated);
    Assert.Contains(
      "registrations.Register(typeof(global::Component), global::Component.RegistrationConfigurator);",
      generated
    );
    Assert.Contains(
      "registrations.Register(typeof(global::Service), global::Service.RegistrationConfigurator);",
      generated
    );
  }

  [Fact]
  public void NonRootAssembliesDoNotEmitDiscovery() {
    var compilation = CreateCompilation("FeatureAssembly", RuntimeAndComponents);
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      new ComponentDiscoveryGenerator().AsSourceGenerator()
    );
    driver = driver.RunGenerators(compilation);

    Assert.Empty(Assert.Single(driver.GetRunResult().Results).GeneratedSources);
  }

  [Fact]
  public void RootDiscoveryIncludesStereotypesFromReferencedAssemblies() {
    var plugin = CreateCompilation(
      "FeatureAssembly",
      """
      using System;
      using System.Collections.Generic;
      namespace HELIX.Context {
        public sealed class EnableMixinsAttribute : Attribute { }
        public delegate void RegistrationConfigurator(RegistrationEntry registration);
        public delegate ComponentRegistrations RegistrationDiscoveryProvider();
        public sealed class RegistrationEntry { }
        public sealed class ComponentRegistrations {
          public void Register(Type type, RegistrationConfigurator configurator) { }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
        public class ComponentAttribute : Attribute { }
        public class ServiceAttribute : ComponentAttribute { }
      }
      [HELIX.Context.Service]
      public sealed class ExternalService {
        public static void RegistrationConfigurator(HELIX.Context.RegistrationEntry registration) { }
      }
      """
    );
    using var stream = new MemoryStream();
    var emit = plugin.Emit(stream);
    Assert.True(emit.Success, string.Join("\n", emit.Diagnostics));
    var references = PlatformReferences.Add(MetadataReference.CreateFromImage(stream.ToArray()));
    var root = CSharpCompilation.Create(
      "Assembly-CSharp",
      new[] { CSharpSyntaxTree.ParseText("public sealed class RootType { }") },
      references,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      new ComponentDiscoveryGenerator().AsSourceGenerator()
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(root, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains(
      "registrations.Register(typeof(global::ExternalService), global::ExternalService.RegistrationConfigurator);",
      generated
    );
  }

  private static CSharpCompilation CreateCompilation(string assemblyName, string source) {
    return CSharpCompilation.Create(
      assemblyName,
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
  }

  private const string RuntimeAndComponents = """
                                              using System;
                                              using System.Collections.Generic;
                                              namespace HELIX.Context {
                                                [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                                                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                                                  public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
                                                }
                                                public delegate void RegistrationConfigurator(RegistrationEntry registration);
                                                public delegate ComponentRegistrations RegistrationDiscoveryProvider();
                                                public sealed class RegistrationEntry { public string name; }
                                                public sealed class ComponentRegistrations {
                                                  public void Register(Type type, RegistrationConfigurator configurator) { }
                                                }
                                                [MixinExpression(
                                                  new[] { "^*~HELIX.Context.RegistrationConfigurator" },
                                                  new[] { -100000 },
                                                  "@CODE<^*~HELIX.Context.RegistrationConfigurator> registration.name = \"@this:name\";"
                                                )]
                                                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
                                                public class ComponentAttribute : Attribute { }
                                                public class ServiceAttribute : ComponentAttribute { }
                                              }
                                              [HELIX.Context.Component]
                                              public partial class Component { }
                                              [HELIX.Context.Service]
                                              public partial class Service { }
                                              """;

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();
}