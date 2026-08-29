using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using HELIX.SourceGen;
using HelixSourceGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class ComponentDiscoveryGeneratorTests {
  [Fact]
  public void ApplicationModuleDiscoversOnlyExactComponentsAndExposesFactory() {
    var result = Run(
      Runtime +
      """
      namespace Feature {
        [HELIX.Mixable, HELIX.Context.Managed]
        public partial class Component { }

        [HELIX.Mixable, HELIX.Context.Service]
        public partial class Service { }

        [HELIX.Mixable, HELIX.Context.HelixApplication(filter: "Feature")]
        public partial class Application { }
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(result.Generated).SourceText.ToString();
    Assert.Contains("partial class Application : global::HELIX.Context.IHelixModule", generated);
    Assert.Contains(
      "public static readonly global::Feature.Application Instance = new global::Feature.Application();",
      generated
    );
    Assert.Contains(
      "public void Discover(global::HELIX.Context.ManagedRegistrations registrations)",
      generated
    );
    Assert.Contains("public static global::HELIX.Context.ManagedRegistrations Discover()", generated);
    Assert.Contains(
      "registrations.Register(typeof(global::Feature.Component), global::Feature.Component.RegistrationConfigurator);",
      generated
    );
    Assert.DoesNotContain("global::Feature.Service.RegistrationConfigurator", generated);
  }

  [Fact]
  public void ModuleImportsInvokeTheImportedInstanceWithoutFactoryGeneration() {
    var result = Run(
      Runtime +
      """
      namespace Feature {
        [HELIX.Mixable, HELIX.Context.Managed]
        public partial class Component { }

        [HELIX.Mixable, HELIX.Context.HelixModule(filter: "Feature")]
        public partial class FeatureModule { }
      }
      namespace Root {
        [HELIX.Context.HelixApplication(
          filter: "Root",
          import: new[] { typeof(Feature.FeatureModule) }
        )]
        public partial class Application { }
      }
      """
    );

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.OutputDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Equal(2, result.Generated.Length);
    var module = Assert.Single(result.Generated.Where(item =>
      item.HintName.Contains("FeatureModule"))).SourceText.ToString();
    var application = Assert.Single(result.Generated.Where(item =>
      item.HintName.Contains("Application"))).SourceText.ToString();
    Assert.Contains("global::Feature.Component.RegistrationConfigurator", module);
    Assert.DoesNotContain("ManagedRegistrations Discover()", module);
    Assert.Contains("global::Feature.FeatureModule.Instance.Discover(registrations);", application);
    Assert.Contains("ManagedRegistrations Discover()", application);
  }

  [Fact]
  public void CompilationWithoutAModuleDoesNotEmitDiscovery() {
    var result = Run(Runtime + "public sealed class OrdinaryType { }");

    Assert.Empty(result.Generated);
  }

  private static TestResult Run(string source) {
    var compilation = CSharpCompilation.Create(
      "FeatureAssembly",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(
      compilation,
      new ISourceGenerator[] {
        new MixinGenerator().AsSourceGenerator(),
        new ComponentDiscoveryGenerator().AsSourceGenerator()
      }
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
    var run = driver.GetRunResult();
    var generated = run.Results.SelectMany(item => item.GeneratedSources)
      .Where(item => item.HintName.Contains("helix-"))
      .ToImmutableArray();
    return new TestResult(
      generated,
      diagnostics.AddRange(run.Diagnostics),
      output.GetDiagnostics()
    );
  }

  private sealed record TestResult(
    ImmutableArray<GeneratedSourceResult> Generated,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<Diagnostic> OutputDiagnostics
  );

  private const string Runtime = """
                                 using System;
                                 namespace HELIX {
                                   [AttributeUsage(AttributeTargets.Class)]
                                   public sealed class MixableAttribute : Attribute { }
                                   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
                                   public sealed class MixinExpressionAttribute : Attribute {
                                     public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
                                   }
                                 }
                                 namespace HELIX.Context {
                                   public delegate void RegistrationConfigurator(RegistrationEntry registration);
                                   public sealed class RegistrationEntry { }
                                   public sealed class ManagedRegistrations {
                                     public void Register(Type type, RegistrationConfigurator configurator) { }
                                   }
                                   public interface IHelixModule {
                                     void Discover(ManagedRegistrations registrations) { }
                                   }
                                   [AttributeUsage(AttributeTargets.Class)]
                                   public sealed class HelixModuleAttribute : Attribute {
                                     public HelixModuleAttribute(string name = null, string filter = null, Type[] import = null) { }
                                   }
                                   [AttributeUsage(AttributeTargets.Class)]
                                   public sealed class HelixApplicationAttribute : Attribute {
                                     public HelixApplicationAttribute(string name = null, string filter = null, Type[] import = null) { }
                                   }
                                   [HELIX.MixinExpression(
                                     new[] { "^*~HELIX.Context.RegistrationConfigurator" },
                                     new[] { -100000 },
                                     "@CODE<^*~HELIX.Context.RegistrationConfigurator> registration.ToString();"
                                   )]
                                   [AttributeUsage(AttributeTargets.Class)]
                                   public sealed class ManagedAttribute : Attribute { }
                                   [AttributeUsage(AttributeTargets.Class)]
                                   public sealed class ServiceAttribute : Attribute { }
                                 }
                                 """;

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();
}
