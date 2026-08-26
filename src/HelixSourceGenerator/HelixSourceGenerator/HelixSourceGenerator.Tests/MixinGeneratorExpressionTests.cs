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
  public void PropStructDirectiveGeneratesMethodParameterStructAndUnwrappedCall() {
    const string source = "using System;\n" + PropStructDatatypeRuntime + """
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [AttributeUsage(AttributeTargets.Method)]
                          [HELIX.MixinExpression(
                            "@PROP_STRUCT<WorkProps><workProps> @target\n" +
                            "@CODE<CLASS> private void Dispatch(WorkProps value) { @local#workProps:propStructCall<this.Work><value>; }"
                          )]
                          public sealed class GenerateWorkPropsAttribute : Attribute { }
                          [AttributeUsage(AttributeTargets.Parameter)]
                          [HELIX.MixinExpression(
                            "@CODE global::HELIX.Boot.CommandBridge.Named(datatype, \"@target:name\");"
                          )]
                          public sealed class NamedArgAttribute : Attribute { }

                          [HELIX.EnableMixins]
                          public partial class Demo {
                            [GenerateWorkProps] private void Work(int count, [NamedArg] in string label) { }
                          }
                          """;

    var compilation = CSharpCompilation.Create(
      "MethodPropStructExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public struct WorkProps", text);
    Assert.Contains("public global::System.Int32 count;", text);
    Assert.Contains("public global::System.String label;", text);
    Assert.Contains("public WorkProps(", text);
    Assert.Contains("public static readonly global::HELIX.StructureDatatype<WorkProps> Datatype =", text);
    Assert.Contains("new global::HELIX.ConfigurableStructureDatatype<WorkProps>(", text);
    Assert.Contains("global::HELIX.Boot.CommandBridge.Named(datatype, \"label\");", text);
    Assert.Contains("this.Work(value.count, in value.label);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PropStructDirectiveGeneratesClassFieldStructAndUnwrappedCall() {
    const string source = "using System;\n" + PropStructDatatypeRuntime + """
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [AttributeUsage(AttributeTargets.Class)]
                          [HELIX.MixinExpression(
                            "@PROP_STRUCT<Snapshot><snapshot> @target\n" +
                            "@CODE<CLASS> private void Dispatch(Snapshot value) { @local#snapshot:propStructCall<Consume><value>; }"
                          )]
                          public sealed class GenerateSnapshotAttribute : Attribute { }

                          [HELIX.EnableMixins]
                          [GenerateSnapshot]
                          public partial class Demo {
                            private int count;
                            private string label;
                            private void Consume(int count, string label) { }
                          }
                          """;

    var compilation = CSharpCompilation.Create(
      "ClassPropStructExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public struct Snapshot", text);
    Assert.Contains("public global::System.Int32 count;", text);
    Assert.Contains("public global::System.String label;", text);
    Assert.Contains("public static readonly global::HELIX.StructureDatatype<Snapshot> Datatype =", text);
    Assert.Contains("new global::HELIX.ConfigurableStructureDatatype<Snapshot>(", text);
    Assert.Contains("Consume(value.count, value.label);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PropStructDirectiveDoesNotGenerateConstructorWithoutParameters() {
    const string source = "using System;\n" + PropStructDatatypeRuntime + """
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [AttributeUsage(AttributeTargets.Method)]
                          [HELIX.MixinExpression("@PROP_STRUCT<WorkProps><workProps> @target")]
                          public sealed class GenerateWorkPropsAttribute : Attribute { }

                          [HELIX.EnableMixins]
                          public partial class Demo {
                            [GenerateWorkProps] private void Work() { }
                          }
                          """;

    var compilation = CSharpCompilation.Create(
      "EmptyMethodPropStructExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public struct WorkProps", text);
    Assert.DoesNotContain("public WorkProps(", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ComponentLifecycleExpressionsEmitPublicComponentTargets() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
                            }
                          }
                          namespace HELIX.Context {
                            [HELIX.MixinExpression(
                              new[] { "$Init", "$Dispose" },
                              new[] { 0, 0 },
                              "@CODE<$Init> Initialize();\n@CODE<$Dispose> Release();"
                            )]
                            [AttributeUsage(AttributeTargets.Class)] public class ComponentAttribute : Attribute { }
                            public sealed class ServiceAttribute : ComponentAttribute { }
                          }
                          [HELIX.Context.Component]
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
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                            public sealed class MixinDefineTargetAttribute : Attribute {
                              public MixinDefineTargetAttribute(string key, string target) { }
                            }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
                            }
                          }
                          [HELIX.MixinDefineTarget("Init", "FirstStart")]
                          [HELIX.MixinDefineTarget("$Dispose", "^Cleanup")]
                          [AttributeUsage(AttributeTargets.Class)]
                          public sealed class LifecycleAttribute : Attribute { }

                          [HELIX.MixinExpression(
                            new[] { "$Init", "$Dispose" },
                            new[] { 0, 0 },
                            "@CODE<$Init> Initialize();\n@CODE<$Dispose> Release();"
                          )]
                          [AttributeUsage(AttributeTargets.Class)]
                          public sealed class ContributionsAttribute : Attribute { }

                          [HELIX.EnableMixins]
                          [Lifecycle]
                          [HELIX.MixinDefineTarget("$Init", "^StartUp")]
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
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          public interface IEvt { }
                          public struct Evt : IEvt { }
                          [HELIX.MixinExpression("@MATCH @arg#0:!?inout\n@ASSERT @arg#0:type:?is<IEvt>\n@MIXIN<$Init> this.Register<@arg#0:type>(@target, @attr#Priority)")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class ReactAttribute : Attribute {
                            public ReactAttribute(int priority) { Priority = priority; }
                            public int Priority { get; }
                          }
                          [HELIX.EnableMixins]
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
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AttributeExpressionCanEmitMultiplePredeclaredInjectionPoints() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string[] target, int[] order, string expression) { }
                            }
                          }
                          [HELIX.MixinExpression(
                            new[] { "$Init", "$Dispose", "Unused" },
                            new[] { -10, 5, 0 },
                            "@CODE<$Init> Before(@attr#value)\n@CODE<$Dispose> After()\n@CODE<Unused>\n@MIXIN<$Init> Normal()"
                          )]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class MarkAttribute : Attribute {
                            public MarkAttribute(int value) { }
                          }
                          [HELIX.EnableMixins]
                          public partial class Demo {
                            [Mark(7)] private void Work() { }
                            private void Normal() { }
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
    Assert.True(
      text.IndexOf("Before(7);", StringComparison.Ordinal) <
      text.IndexOf("Normal();", StringComparison.Ordinal)
    );
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AttributeExpressionRejectsUndeclaredInjectionPoints() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string target, int order, string expression) { }
                            }
                          }
                          [HELIX.MixinExpression("$Init", 0, "@CODE<Other> Wrong()")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class MarkAttribute : Attribute { }
                          [HELIX.EnableMixins]
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

    Assert.Contains(
      result.Diagnostics,
      item => item.Id == "HLXM08" &&
        item.GetMessage().Contains("was not declared")
    );
    Assert.Empty(result.GeneratedSources);
  }

  [Fact]
  public void AttributeExpressionCanReadAPropertyInitializerDefault() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string target, int order, string expression) { }
                            }
                          }
                          [HELIX.MixinExpression("$Init", 1, "@CODE<$Init> Capture(@attr#Priority);")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class EventHandlerAttribute : Attribute {
                            public int Priority { get; set; } = 0;
                          }
                          [HELIX.EnableMixins]
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
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          public interface IMarker { }
                          [HELIX.MixinExpression(
                            "@CODE<CLASS> public int GeneratedValue => 3;\n" +
                            "@CODE<FILE> internal sealed class GeneratedFileType { }\n" +
                            "@CODE<IMPLEMENTS> global::IMarker"
                          )]
                          [AttributeUsage(AttributeTargets.Class)]
                          public sealed class MarkerAttribute : Attribute { }
                          [HELIX.EnableMixins, Marker]
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
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          namespace DemoAnnotations {
                            [AttributeUsage(AttributeTargets.Class)]
                            public sealed class GeneratedMarkerAttribute : Attribute { }
                          }
                          [HELIX.MixinExpression(
                            "@LOCAL<annotation> GeneratedMarker\n" +
                            "@LOCAL<namespace> DemoAnnotations\n" +
                            "@CODE<ANNOTATION> @local#annotation\n" +
                            "@USING @local#namespace"
                          )]
                          [AttributeUsage(AttributeTargets.Class)]
                          public sealed class MarkerAttribute : Attribute { }
                          [HELIX.EnableMixins, Marker]
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

  [Fact]
  public void AttributeExecutesAllInheritedExpressionsBaseFirst() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string target, int order, string expression) { }
                            }
                          }
                          [HELIX.MixinExpression("$Init", 0, "@CODE BaseFirst()")]
                          [HELIX.MixinExpression("$Init", 0, "@CODE BaseSecond()")]
                          [AttributeUsage(AttributeTargets.Class, Inherited = true)]
                          public class BaseMarkerAttribute : Attribute { }

                          [HELIX.MixinExpression("$Init", 0, "@CODE DerivedFirst()")]
                          [HELIX.MixinExpression("$Init", 0, "@CODE DerivedSecond()")]
                          public sealed class DerivedMarkerAttribute : BaseMarkerAttribute { }

                          [HELIX.EnableMixins, DerivedMarker]
                          public partial class Demo {
                            private void BaseFirst() { }
                            private void BaseSecond() { }
                            private void DerivedFirst() { }
                            private void DerivedSecond() { }
                          }
                          """;

    var compilation = CSharpCompilation.Create(
      "InheritedAttributeExpressionsTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    var baseFirst = generated.IndexOf("BaseFirst();", StringComparison.Ordinal);
    var baseSecond = generated.IndexOf("BaseSecond();", StringComparison.Ordinal);
    var derivedFirst = generated.IndexOf("DerivedFirst();", StringComparison.Ordinal);
    var derivedSecond = generated.IndexOf("DerivedSecond();", StringComparison.Ordinal);
    Assert.True(baseFirst >= 0 && baseFirst < baseSecond);
    Assert.True(baseSecond < derivedFirst);
    Assert.True(derivedFirst < derivedSecond);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionSupportsTypeArgumentPathsExistenceAndValueTruthiness() {
    const string source = """
                          using System;
                          using System.Collections.Generic;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@SCOPE<disabled>\n" +
                            "@MATCH @attr#disabled\n" +
                            "@FAIL\n" +
                            "@SCOPE<null>\n" +
                            "@MATCH @attr#optional\n" +
                            "@FAIL\n" +
                            "@SCOPE<missing-value>\n" +
                            "@MATCH @attr#missing\n" +
                            "@FAIL\n" +
                            "@SCOPE<missing-exists>\n" +
                            "@MATCH @attr#missing:?exists\n" +
                            "@FAIL\n" +
                            "@SCOPE<generate>\n" +
                            "@MATCH @attr#enabled\n" +
                            "@MATCH @attr#missing:!?exists\n" +
                            "@MATCH @target:type#0:?is<global::System.Int32>\n" +
                            "@CODE<CLASS> public @target:type#T GeneratedValue;\n" +
                            "@RETURN"
                          )]
                          [AttributeUsage(AttributeTargets.Field)]
                          public sealed class GenerateAttribute : Attribute {
                            public GenerateAttribute(bool enabled, bool disabled = false, string optional = null) { }
                          }
                          [HELIX.EnableMixins]
                          public partial class Demo {
                            [Generate(true)] private List<int> values;
                          }
                          """;

    var compilation = CSharpCompilation.Create(
      "TypeArgumentPathExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public global::System.Int32 GeneratedValue;", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionCanRenderFullNamesAndUnwrapStringsAndTypes() {
    const string source = """
                          using System;
                          using System.Collections.Generic;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@CODE<CLASS> public const string GeneratedText = \"@attr#text:unwrap\";\n" +
                            "@CODE<CLASS> public const string GeneratedFullName = \"@target:type:fullName\";\n" +
                            "@CODE<CLASS> public @attr#selectedType:unwrap GeneratedValue;\n" +
                            "@CODE<CLASS> public const int GeneratedNumber = @attr#number:unwrap;"
                          )]
                          [AttributeUsage(AttributeTargets.Field)]
                          public sealed class RenderAttribute : Attribute {
                            public RenderAttribute(string text, Type selectedType, int number) { }
                          }
                          [HELIX.EnableMixins]
                          public partial class Demo {
                            [Render("plain text", typeof(Dictionary<string, int>), 42)]
                            private List<int> values;
                          }
                          """;

    var compilation = CSharpCompilation.Create(
      "FullNameAndUnwrapExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public const string GeneratedText = \"plain text\";", generated);
    Assert.Contains("public const string GeneratedFullName = \"List<int>\";", generated);
    Assert.Contains(
      "public System.Collections.Generic.Dictionary<System.String, System.Int32> GeneratedValue;",
      generated
    );
    Assert.Contains("public const int GeneratedNumber = 42;", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PreparedExpressionFunctionsAndVariablesAreAvailableToAttributes() {
    const string source = """
                          using System;
                          [assembly: HELIX.MixinPrepareGlobal(
                            "@VAR<call> Prepared()\n" +
                            "@FUNC<emit>\n" +
                            "@CODE<$Init> @var#call\n" +
                            "@RETURN\n" +
                            "@END"
                          )]
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Assembly)] public sealed class MixinPrepareGlobalAttribute : Attribute {
                              public MixinPrepareGlobalAttribute(string content) { }
                            }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string target, int order, string expression) { }
                            }
                          }
                          [HELIX.MixinExpression("$Init", 0, "@CALL<emit>\n@CODE<$Init> Local()")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class MarkAttribute : Attribute { }
                          [HELIX.EnableMixins]
                          public partial class Demo {
                            [Mark] private void Work() { }
                            private void Prepared() { }
                            private void Local() { }
                          }
                          """;
    var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
    var compilation = CSharpCompilation.Create(
      "PreparedMixinExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, parseOptions) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      new[] { new MixinGenerator().AsSourceGenerator() },
      parseOptions: parseOptions
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    var preparedCall = generated.IndexOf("Prepared();", StringComparison.Ordinal);
    var localCall = generated.IndexOf("Local();", StringComparison.Ordinal);
    Assert.True(preparedCall >= 0 && preparedCall < localCall);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void InvalidPreparedExpressionReportsDedicatedDiagnostic() {
    const string source = """
                          using System;
                          [assembly: HELIX.MixinPrepareGlobal("@FUNC<broken>\n@UNKNOWN\n@END")]
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Assembly)] public sealed class MixinPrepareGlobalAttribute : Attribute {
                              public MixinPrepareGlobalAttribute(string content) { }
                            }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "InvalidPreparedMixinExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGenerators(compilation);

    Assert.Contains(
      Assert.Single(driver.GetRunResult().Results).Diagnostics,
      item =>
        item.Id == "HLXM11" && item.GetMessage().Contains("line 2") &&
        item.GetMessage().Contains("unknown directive")
    );
  }

  [Fact]
  public void ExpressionLogsAndDumpsAreReportedAsWarningDiagnostics() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@LOG generating @this:name\n" +
                            "@CODE<CLASS> public int Generated;\n" +
                            "@DUMP<STATE>\n" +
                            "@DUMP<BUFFER>"
                          )]
                          [AttributeUsage(AttributeTargets.Class)]
                          public sealed class MarkAttribute : Attribute { }
                          [HELIX.EnableMixins, Mark]
                          public partial class Demo { }
                          """;
    var compilation = CSharpCompilation.Create(
      "MixinExpressionLogTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGenerators(compilation);
    var diagnostics = Assert.Single(driver.GetRunResult().Results).Diagnostics
      .Where(item => item.Id == "HLXM12")
      .ToArray();

    Assert.Equal(3, diagnostics.Length);
    Assert.All(diagnostics, item => Assert.Equal(DiagnosticSeverity.Warning, item.Severity));
    Assert.Contains(diagnostics, item => item.GetMessage() == "generating Demo");
    Assert.Contains(diagnostics, item => item.GetMessage().StartsWith("STATE "));
    Assert.Contains(diagnostics, item => item.GetMessage().Contains("Class: public int Generated;"));
  }

  [Fact]
  public void MixinExpressionCanDeclareItsOwnDynamicInjectionTargetAndPriority() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@VAR<target> $Init\n" +
                            "@VAR<priority> -10\n" +
                            "@MIXIN<(@var#target)><(@var#priority)> Before()\n" +
                            "@MIXIN<$Init><10> After()"
                          )]
                          [AttributeUsage(AttributeTargets.Class)] public sealed class MarkAttribute : Attribute { }
                          [HELIX.EnableMixins, Mark]
                          public partial class Demo {
                            private void Before() { }
                            private void After() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "SelfDeclaredMixinExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.True(text.IndexOf("Before();", StringComparison.Ordinal) < text.IndexOf("After();", StringComparison.Ordinal));
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionCanResolveCheckAndWireAMixinTarget() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@VAR<target> $Init\n" +
                            "@RESOLVE_MIXIN<Method> @var#target\n" +
                            "@ASSERT @local#Method:?signature<(@target)>\n" +
                            "@ASSERT @true:?wireable<(@local#Method)><(@target)>\n" +
                            "@MIXIN<$Init> @target:name(@local#Method:wire<(@target)>);"
                          )]
                          [AttributeUsage(AttributeTargets.Method)] public sealed class HandleAttribute : Attribute { }
                          [HELIX.EnableMixins]
                          public partial class Demo {
                            private partial void Awake(int value);
                            [Handle] private void Handle(int value) { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "ResolvedMixinWiringTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("Handle(value);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AsyncPredicateRecognizesAsyncMethods() {
    const string source = """
                          using System;
                          using System.Threading.Tasks;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class EnableMixinsAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@ASSERT @target:?async\n@CODE<CLASS> public int AsyncMatched;"
                          )]
                          [AttributeUsage(AttributeTargets.Method)] public sealed class AsyncMarkerAttribute : Attribute { }
                          [HELIX.EnableMixins]
                          public partial class Demo {
                            [AsyncMarker] private async Task Work() { await Task.Yield(); }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "AsyncMixinPredicateTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(new MixinGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public int AsyncMatched;", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();

  private const string PropStructDatatypeRuntime = """
                                                     namespace HELIX {
                                                       public interface IDatatype<T> { }
                                                       public sealed class PrimitiveDatatype<T> : IDatatype<T> { }
                                                       public static class Datatypes {
                                                         public static readonly IDatatype<string> String = new PrimitiveDatatype<string>();
                                                         public static readonly IDatatype<int> Int = new PrimitiveDatatype<int>();
                                                         public static IDatatype<T> Object<T>() => new PrimitiveDatatype<T>();
                                                       }
                                                       public abstract class StructurePropertyDatatype<T> { }
                                                       public sealed class StructurePropertyDatatype<T, TValue> : StructurePropertyDatatype<T> {
                                                         public delegate TValue Getter(ref T value);
                                                         public delegate void Setter(ref T value, TValue propertyValue);
                                                         public StructurePropertyDatatype(
                                                           string name, IDatatype<TValue> datatype, Getter getter, Setter setter,
                                                           System.Collections.Generic.IList<object> modifiers = null,
                                                           bool required = true, object defaultValue = null
                                                         ) { }
                                                       }
                                                       public sealed class StructureDatatype<T> {
                                                         public StructureDatatype(
                                                           string name,
                                                           System.Collections.Generic.IList<StructurePropertyDatatype<T>> properties
                                                         ) { }
                                                       }
                                                       public sealed class ConfigurableStructureDatatype<T> {
                                                         private readonly StructureDatatype<T> datatype;
                                                         public ConfigurableStructureDatatype(
                                                           StructureDatatype<T> datatype,
                                                           System.Action<StructureDatatype<T>> configure
                                                         ) {
                                                           this.datatype = datatype;
                                                           configure(datatype);
                                                         }
                                                         public StructureDatatype<T> Datatype => datatype;
                                                       }
                                                     }
                                                     namespace HELIX.Boot {
                                                       public static class CommandBridge {
                                                         public static void Named<T>(
                                                           HELIX.StructureDatatype<T> datatype, string fieldName
                                                         ) { }
                                                       }
                                                     }
                                                     """;
}
