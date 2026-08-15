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
  public void StarTargetGeneratesAStaticMethodAndInjectsNullForTheClassInstance() {
    var result = Run("""
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Method)] public sealed class MixinMethodAttribute : Attribute {
          public MixinMethodAttribute(string target, int order = 0) { }
        }
        [AttributeUsage(AttributeTargets.Parameter)] public sealed class MixinInjectAttribute : Attribute {
          public MixinInjectAttribute(MixinInject type) { }
        }
        public enum MixinInject { This = 0, Target = 1 }
      }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [HELIX.Context.MixinMethod("*Configure")]
        private static void Contribute(
          [HELIX.Context.MixinInject(HELIX.Context.MixinInject.This)] Demo instance,
          [HELIX.Context.MixinInject(HELIX.Context.MixinInject.Target)] object target
        ) { }
      }
      """);

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("private static void Configure()", result.Generated);
    Assert.Contains("Contribute(null, null);", result.Generated);
    Assert.DoesNotContain("*Configure", result.Generated);
  }

  [Fact]
  public void PublicAndStaticModifiersMayAppearInEitherOrder() {
    var result = Run("""
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Method)] public sealed class MixinMethodAttribute : Attribute {
          public MixinMethodAttribute(string target, int order = 0) { }
        }
      }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [HELIX.Context.MixinMethod("^*Configure")]
        private static void First() { }

        [HELIX.Context.MixinMethod("*^Configure")]
        private static void Second() { }
      }
      """);

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("public static void Configure()", result.Generated);
    Assert.Contains("First();", result.Generated);
    Assert.Contains("Second();", result.Generated);
    Assert.DoesNotContain("^", result.Generated);
  }

  [Fact]
  public void DelegateTargetUsesTheDelegateNameAndSignature() {
    var result = Run("""
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Method)] public sealed class MixinMethodAttribute : Attribute {
          public MixinMethodAttribute(string target, int order = 0) { }
        }
      }
      namespace DemoApi {
        public delegate void RegistrationConfigurator(ref int value, string name);
      }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [HELIX.Context.MixinMethod("*^~DemoApi.RegistrationConfigurator")]
        private static void Contribute(ref int value) { }
      }
      """);

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("public static void RegistrationConfigurator(", result.Generated);
    Assert.Contains("ref global::System.Int32 value", result.Generated);
    Assert.Contains("global::System.String name", result.Generated);
    Assert.Contains("Contribute(ref value);", result.Generated);
  }

  [Fact]
  public void StaticInterfaceExpressionCanResolveTheContainingType() {
    var result = Run("""
      using System;
      namespace HELIX.Context {
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
      [HELIX.Context.MixinExpression(
        "^*~DemoApi.RegistrationConfigurator",
        0,
        "@CODE<^*~DemoApi.RegistrationConfigurator> registration.type = typeof(@this:type);"
      )]
      public interface IComponentMixin : HELIX.Context.IMixin { }
      [HELIX.Context.EnableMixins]
      public partial class Demo : IComponentMixin { }
      """);

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Contains("public static void RegistrationConfigurator(", result.Generated);
    Assert.Contains("registration.type = typeof(global::Demo);", result.Generated);
  }

  [Fact]
  public void MissingDelegateTargetReportsAnInvalidTarget() {
    var result = Run("""
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Method)] public sealed class MixinMethodAttribute : Attribute {
          public MixinMethodAttribute(string target, int order = 0) { }
        }
      }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [HELIX.Context.MixinMethod("~Missing.Configurator")]
        private void Contribute() { }
      }
      """);

    Assert.Contains(result.GeneratorDiagnostics, item => item.Id == "HLXM03" &&
      item.GetMessage().Contains("was not found or is not a delegate"));
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
      diagnostics.AddRange(run.Diagnostics), output.GetDiagnostics()
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
