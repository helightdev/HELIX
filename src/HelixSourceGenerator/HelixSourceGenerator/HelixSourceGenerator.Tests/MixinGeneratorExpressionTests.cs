using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using HELIX.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinGeneratorExpressionTests {
  [Fact]
  public void ComponentLifecycleExpressionsEmitPublicComponentTargets() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
        }
        [MixinExpression(
          new[] { "$Init", "$Dispose" },
          new[] { 0, 0 },
          "@CODE<$Init> Initialize();\n@CODE<$Dispose> Release();"
        )]
        [AttributeUsage(AttributeTargets.Class)] public class ComponentAttribute : Attribute { }
        public sealed class ServiceAttribute : ComponentAttribute { }
      }
      [HELIX.Context.Service]
      public partial class Demo {
        private void Initialize() { }
        private void Release() { }
      }
      """;

    var compilation = CSharpCompilation.Create(
      "ComponentLifecycleExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public void LoadComponent()", text);
    Assert.Contains("public void UnloadComponent()", text);
    Assert.Contains("Initialize();", text);
    Assert.Contains("Release();", text);
    Assert.DoesNotContain("void Awake()", text);
    Assert.DoesNotContain("void OnDestroy()", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void TargetDefinitionsOnTheClassAndItsAttributesOverridePseudonymsInOrder() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
        public sealed class MixinDefineTargetAttribute : Attribute {
          public MixinDefineTargetAttribute(string key, string target) { }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
        }
      }
      [HELIX.Context.MixinDefineTarget("Init", "FirstStart")]
      [HELIX.Context.MixinDefineTarget("$Dispose", "^Cleanup")]
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class LifecycleAttribute : Attribute { }

      [HELIX.Context.MixinExpression(
        new[] { "$Init", "$Dispose" },
        new[] { 0, 0 },
        "@CODE<$Init> Initialize();\n@CODE<$Dispose> Release();"
      )]
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class ContributionsAttribute : Attribute { }

      [HELIX.Context.EnableMixins]
      [Lifecycle]
      [HELIX.Context.MixinDefineTarget("$Init", "^StartUp")]
      [Contributions]
      public partial class Demo {
        private void Initialize() { }
        private void Release() { }
      }
      """;

    var compilation = CSharpCompilation.Create(
      "DefinedMixinTargetsTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public void StartUp()", text);
    Assert.Contains("public void Cleanup()", text);
    Assert.DoesNotContain("FirstStart", text);
    Assert.DoesNotContain("void Awake()", text);
    Assert.DoesNotContain("void OnDestroy()", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionSelectsAndEmitsAttributeMixinCode() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Method)] public sealed class MixinMethodAttribute : Attribute {
          public MixinMethodAttribute(string target, int order, string expression) { }
        }
        [AttributeUsage(AttributeTargets.Class)] public sealed class AttributeMixinMethodProxyAttribute : Attribute {
          public AttributeMixinMethodProxyAttribute(Type owner, string method) { }
        }
      }
      public interface IEvt { }
      public struct Evt : IEvt { }
      [HELIX.Context.AttributeMixinMethodProxy(typeof(Handlers), "Register")]
      [AttributeUsage(AttributeTargets.Method)]
      public sealed class ReactAttribute : Attribute {
        public ReactAttribute(int priority) { Priority = priority; }
        public int Priority { get; }
      }
      public static class Handlers {
        [HELIX.Context.MixinMethod("$Init", 0, "@MATCH @arg#0:!?inout\n@ASSERT @arg#0:type:?is<IEvt>\n@CODE this.Register<@arg#0:type>(@target, @attr#Priority)")]
        public static void Register() { }
      }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [React(7)] private void React(ref Evt evt) { }
        private void Register<T>(ActionRef<T> handler, int priority) { }
        private delegate void ActionRef<T>(ref T value);
      }
      """;

    var compilation = CSharpCompilation.Create(
      "ExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(driver.GetRunResult().Results).GeneratedSources;
    var text = Assert.Single(generated).SourceText.ToString();
    Assert.Contains("private void Awake()", text);
    Assert.Contains("this.Register<global::Evt>(this.React, 7);", text);
    Assert.DoesNotContain("Handlers.Register(", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AttributeExpressionCanEmitMultiplePredeclaredInjectionPoints() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Method)] public sealed class MixinMethodAttribute : Attribute {
          public MixinMethodAttribute(string target, int order = 0) { }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
        }
      }
      [HELIX.Context.MixinExpression(
        new[] { "$Init", "$Dispose", "Unused" },
        new[] { -10, 5, 0 },
        "@CODE<$Init> Before(@attr#value)\n@CODE<$Dispose> After()\n@CODE<Unused>"
      )]
      [AttributeUsage(AttributeTargets.Method)]
      public sealed class MarkAttribute : Attribute {
        public MarkAttribute(int value) { }
      }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [Mark(7)] private void Work() { }
        [HELIX.Context.MixinMethod("$Init", 0)] private void Normal() { }
        private void Before(int value) { }
        private void After() { }
      }
      """;

    var compilation = CSharpCompilation.Create(
      "AttributeExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("Before(7);", text);
    Assert.Contains("After();", text);
    Assert.DoesNotContain("void Unused", text);
    Assert.True(text.IndexOf("Before(7);", StringComparison.Ordinal) <
                text.IndexOf("Normal();", StringComparison.Ordinal));
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AttributeExpressionRejectsUndeclaredInjectionPoints() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string target, int order, string expression) { }
        }
      }
      [HELIX.Context.MixinExpression("$Init", 0, "@CODE<Other> Wrong()")]
      [AttributeUsage(AttributeTargets.Method)]
      public sealed class MarkAttribute : Attribute { }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [Mark] private void Work() { }
      }
      """;

    var compilation = CSharpCompilation.Create(
      "InvalidAttributeExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGenerators(compilation);
    var result = Assert.Single(driver.GetRunResult().Results);

    Assert.Contains(result.Diagnostics, item => item.Id == "HLXM08" &&
                                                item.GetMessage().Contains("was not declared"));
    Assert.Empty(result.GeneratedSources);
  }

  [Fact]
  public void AttributeExpressionCanReadAPropertyInitializerDefault() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string target, int order, string expression) { }
        }
      }
      [HELIX.Context.MixinExpression("$Init", 1, "@CODE<$Init> Capture(@attr#Priority);")]
      [AttributeUsage(AttributeTargets.Method)]
      public sealed class EventHandlerAttribute : Attribute {
        public int Priority { get; set; } = 0;
      }
      [HELIX.Context.EnableMixins]
      public partial class Demo {
        [EventHandler] private void Handle() { }
        private void Capture(int priority) { }
      }
      """;

    var compilation = CSharpCompilation.Create(
      "AttributeDefaultTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("Capture(0);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionWithoutTargetsCanEmitClassFileAndInterfaceDeclarations() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      public interface IMarker { }
      [HELIX.Context.MixinExpression(
        "@CODE<CLASS> public int GeneratedValue => 3;\n" +
        "@CODE<FILE> internal sealed class GeneratedFileType { }\n" +
        "@CODE<IMPLEMENTS> global::IMarker"
      )]
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class MarkerAttribute : Attribute { }
      [HELIX.Context.EnableMixins, Marker]
      public partial class Demo { }
      """;

    var compilation = CSharpCompilation.Create(
      "TargetlessExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("partial class Demo : global::IMarker", generated);
    Assert.Contains("public int GeneratedValue => 3;", generated);
    Assert.Contains("internal sealed class GeneratedFileType", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionCanEmitInterpolatedTypeAnnotationsAndUsings() {
    const string source = """
      using System;
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      namespace DemoAnnotations {
        [AttributeUsage(AttributeTargets.Class)]
        public sealed class GeneratedMarkerAttribute : Attribute { }
      }
      [HELIX.Context.MixinExpression(
        "@LOCAL<annotation> GeneratedMarker\n" +
        "@LOCAL<namespace> DemoAnnotations\n" +
        "@CODE<ANNOTATION> @local#annotation\n" +
        "@USING @local#namespace"
      )]
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class MarkerAttribute : Attribute { }
      [HELIX.Context.EnableMixins, Marker]
      public partial class Demo { }
      """;

    var compilation = CSharpCompilation.Create(
      "AnnotationAndUsingExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("using DemoAnnotations;", generated);
    Assert.Contains("[GeneratedMarker]", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();
}
