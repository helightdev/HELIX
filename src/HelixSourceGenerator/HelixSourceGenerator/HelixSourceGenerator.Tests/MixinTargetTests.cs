using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using HELIX.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinTargetTests {
  [Fact]
  public void MultipleCandidateAttributesGenerateTheTargetOnce() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
      }
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class ComponentAttribute : Attribute { }
      }
      [HELIX.EnableMixins, HELIX.Context.Component]
      public partial class Demo { }
      """
    );

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void StarTargetGeneratesAStaticMethod() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      [HELIX.MixinExpression("@MIXIN<*Configure> Contribute(null, null)")]
      [AttributeUsage(AttributeTargets.Class)] public sealed class ConfigureAttribute : Attribute { }
      [HELIX.EnableMixins, Configure]
      public partial class Demo {
        private static void Contribute(Demo instance, object target) { }
      }
      """
    );

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("private static void Configure()", result.Generated);
    Assert.Contains("Contribute(null, null);", result.Generated);
    Assert.DoesNotContain("*Configure", result.Generated);
  }

  [Fact]
  public void PublicAndStaticModifiersMayAppearInEitherOrder() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      [HELIX.MixinExpression("@MIXIN<^*Configure> First()\n@MIXIN<*^Configure> Second()")]
      [AttributeUsage(AttributeTargets.Class)] public sealed class ConfigureAttribute : Attribute { }
      [HELIX.EnableMixins, Configure]
      public partial class Demo {
        private static void First() { }
        private static void Second() { }
      }
      """
    );

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("public static void Configure()", result.Generated);
    Assert.Contains("First();", result.Generated);
    Assert.Contains("Second();", result.Generated);
    Assert.DoesNotContain("^", result.Generated);
  }

  [Fact]
  public void DelegateTargetUsesTheDelegateNameAndSignature() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      namespace DemoApi {
        public delegate void RegistrationConfigurator(ref int value, string name);
      }
      [HELIX.MixinExpression("@MIXIN<*^~DemoApi.RegistrationConfigurator> Contribute(ref value)")]
      [AttributeUsage(AttributeTargets.Class)] public sealed class ConfigureAttribute : Attribute { }
      [HELIX.EnableMixins, Configure]
      public partial class Demo {
        private static void Contribute(ref int value) { }
      }
      """
    );

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("public static void RegistrationConfigurator(", result.Generated);
    Assert.Contains("ref global::System.Int32 value", result.Generated);
    Assert.Contains("global::System.String name", result.Generated);
    Assert.Contains("Contribute(ref value);", result.Generated);
  }

  [Fact]
  public void NamedDelegateTargetUsesTheSpecifiedNameAndDelegateSignature() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      namespace DemoApi {
        public delegate void RegistrationConfigurator(ref int value, string name);
      }
      [HELIX.MixinExpression("@MIXIN<^*Configure:DemoApi.RegistrationConfigurator> Contribute(ref value)")]
      [AttributeUsage(AttributeTargets.Class)] public sealed class ConfigureAttribute : Attribute { }
      [HELIX.EnableMixins, Configure]
      public partial class Demo {
        private static void Contribute(ref int value) { }
      }
      """
    );

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("public static void Configure(", result.Generated);
    Assert.Contains("ref global::System.Int32 value", result.Generated);
    Assert.Contains("global::System.String name", result.Generated);
    Assert.Contains("Contribute(ref value);", result.Generated);
  }

  [Fact]
  public void StaticInterfaceExpressionCanResolveTheContainingType() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Interface)] public sealed class MixinAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string target, int order, string expression) { }
        }
        [Mixin] public interface IMixin { }
      }
      namespace DemoApi {
        public delegate void RegistrationConfigurator(RegistrationEntry registration);
        public sealed class RegistrationEntry { public Type type; }
      }
      [HELIX.MixinExpression(
        "^*~DemoApi.RegistrationConfigurator",
        0,
        "@CODE<^*~DemoApi.RegistrationConfigurator> registration.type = typeof(@this:type);"
      )]
      public interface IComponentMixin : HELIX.IMixin { }
      [HELIX.EnableMixins]
      public partial class Demo : IComponentMixin { }
      """
    );

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("public static void RegistrationConfigurator(", result.Generated);
    Assert.Contains("registration.type = typeof(global::Demo);", result.Generated);
  }

  [Fact]
  public void SuccessfulContributionsEmitCompactHiddenMetadata() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Interface)] public sealed class MixinAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string target, int order, string expression) { }
        }
        [Mixin] public interface IMixin { }
      }
      [HELIX.MixinExpression("Configure", -12, "@CODE<Configure> Apply();")]
      public interface IConfigureMixin : HELIX.IMixin { }
      [HELIX.EnableMixins]
      public partial class Demo : IConfigureMixin {
        private void Apply() { }
      }
      """
    );

    var diagnostic = Assert.Single(
      result.GeneratorDiagnostics
        .Where(item => item.Id == "HLXM14")
        .GroupBy(item => string.Join("|", item.Properties.OrderBy(pair => pair.Key)))
        .Select(group => group.First())
    );
    Assert.Equal(DiagnosticSeverity.Hidden, diagnostic.Severity);
    Assert.Equal(8, diagnostic.Properties.Count);
    Assert.Equal("global::Demo", diagnostic.Properties["Target"]);
    Assert.Equal("Configure", diagnostic.Properties["Method"]);
    Assert.Equal("global::IConfigureMixin", diagnostic.Properties["Mixin"]);
    Assert.Equal("-12", diagnostic.Properties["Priority"]);
    Assert.Equal("global::Demo", diagnostic.Properties["SourceType"]);
    Assert.Equal("", diagnostic.Properties["SourceMember"]);
    Assert.Equal("NamedType", diagnostic.Properties["SourceKind"]);
    Assert.Equal("0", diagnostic.Properties["SourceParameterCount"]);
    Assert.Equal(
      "global::Demo|Configure|global::IConfigureMixin|-12|global::Demo||NamedType|0",
      diagnostic.GetMessage()
    );
  }

  [Fact]
  public void MissingDelegateTargetReportsAnInvalidTarget() {
    var result = Run(
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      [HELIX.MixinExpression("@MIXIN<~Missing.Configurator> Contribute()")]
      [AttributeUsage(AttributeTargets.Class)] public sealed class ConfigureAttribute : Attribute { }
      [HELIX.EnableMixins, Configure]
      public partial class Demo { private void Contribute() { } }
      """
    );

    Assert.Contains(
      result.GeneratorDiagnostics,
      item => item.Id == "HLXM03" &&
        item.GetMessage().Contains("was not found or is not a delegate")
    );
  }

  private static TestResult Run(string source) {
    var compilation = CSharpCompilation.Create(
      "MixinTargetTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
    var run = Assert.Single(driver.GetRunResult().Results);
    return new TestResult(
      run.GeneratedSources.Length == 0 ? "" : Assert.Single(run.GeneratedSources).SourceText.ToString(),
      diagnostics.AddRange(run.Diagnostics),
      output.GetDiagnostics()
    );
  }

  private sealed record TestResult(
    string Generated,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> CompilationDiagnostics
  );

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();
}
