using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using HELIX.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinGeneratorExpressionTests {
  [Theory]
  [InlineData("Core")]
  [InlineData("Context")]
  [InlineData("Compose")]
  [InlineData("Boot")]
  [InlineData("Odin")]
  public void UnityAdditionalMixinFilesParseAsAnnotationCatalogs(string name) {
    var path = Path.GetFullPath(Path.Combine(
      AppContext.BaseDirectory,
      "../../../../../../HELIX/Assets/Mixins/" + name + ".HelixSourceGenerator.additionalfile"
    ));
    var api = typeof(MixinGenerator).Assembly.GetType("HELIX.SourceGen.MixinLibraryApi");
    var method = api!.GetMethod("ReadAdditionalFile", BindingFlags.Static | BindingFlags.NonPublic);
    var parsed = method!.Invoke(null, new object[] {
      new TestAdditionalText(path, File.ReadAllText(path)), default(System.Threading.CancellationToken)
    });
    var type = parsed!.GetType();
    var success = (bool)type.GetProperty("Success")!.GetValue(parsed)!;
    var error = type.GetProperty("Error")!.GetValue(parsed);
    var errorLine = type.GetProperty("ErrorLine")!.GetValue(parsed);
    var annotations = (System.Collections.IDictionary)type.GetProperty("Annotations")!.GetValue(parsed)!;

    Assert.True(success, name + ": line " + errorLine + ": " + error);
    Assert.NotEmpty(annotations);
  }

  [Fact]
  public void MixinLibraryCanResolvePreparedAdditionalFileReference() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinLibraryAttribute : Attribute { public MixinLibraryAttribute(string reference) { } }
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinImportAttribute : Attribute { public MixinImportAttribute(Type library) { } }
                          }
                          [HELIX.MixinLibrary("External")]
                          public static class ExternalLibrary { }
                          [HELIX.MixinImport(typeof(ExternalLibrary))]
                          [AttributeUsage(AttributeTargets.Class)] public sealed class ExternalAttribute : Attribute { }
                          [HELIX.Mixable, External] public partial class Demo { private void Run() { } }
                          """;
    var compilation = CSharpCompilation.Create(
      "AdditionalMixinLibraryTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      generators: new[] { new MixinGenerator().AsSourceGenerator() },
      additionalTexts: new AdditionalText[] {
        new TestAdditionalText(
          "/project/External.HelixSourceGenerator.additionalfile",
          "@FUNC<Imported>\n@CODE<CLASS> public int FromAdditionalFile;\n@END\n" +
          "@ANNOTATION<ExternalAttribute>\n" +
          "@DEFINE_TARGET<Init><Initialize>\n" +
          "@PRELUDE\n@CALL<Imported>\n@END\n" +
          "@MIXIN<$Init><7> Run();\n@END"
        )
      },
      parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public int FromAdditionalFile;", generated);
    Assert.Contains("private void Initialize()", generated);
    Assert.Contains("Run();", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AdditionalMixinDebugConfigurationEmitsProgramsAndCarriedState() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                          }
                          [AttributeUsage(AttributeTargets.Class)] public sealed class PreludeOnlyAttribute : Attribute { }
                          [AttributeUsage(AttributeTargets.Field)] public sealed class ExternalAttribute : Attribute { }
                          [HELIX.Mixable, PreludeOnly] public partial class Demo {
                            [External] private int Value;
                            private void PreludeOnlyLogic() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "AdditionalMixinDebugTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      generators: new[] { new MixinGenerator().AsSourceGenerator() },
      additionalTexts: new AdditionalText[] {
        new TestAdditionalText(
          "/project/External.HelixSourceGenerator.additionalfile",
          "@CONFIG<DEBUG> | ignored\n" +
          "@FUNC<Prepare>\n@VAR<Prepared> true\n@END\n" +
          "@ANNOTATION<PreludeOnlyAttribute>\n" +
          "@PRELUDE\n@MIXIN<$Init> PreludeOnlyLogic();\n" +
          "@CODE<CLASS> public const string RewrittenName = \"@target:name\";\n@END\n@END\n" +
          "@ANNOTATION<ExternalAttribute>\n" +
          "@PRELUDE\n@CALL<Prepare>\n@CARRY<Name> @target:name\n@CARRY<Unused> ignored\n@END\n" +
          "@CODE<CLASS> public const string CarriedName = \"@carry#Name\";\n@END"
        )
      },
      parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.StartsWith("// ============================================================================\n// HELIX MIXIN PROGRAM DUMP", generated);
    Assert.DoesNotContain("// GLOBAL STATE", generated);
    Assert.Contains("// HELIX MIXIN FINAL STATE", generated);
    Assert.Contains("//   preludeOperations = ", generated);
    Assert.Contains("//   preludeDurationMs = ", generated);
    Assert.Contains("//   lateOperations = ", generated);
    Assert.Contains("//   lateDurationMs = ", generated);
    Assert.Contains("// TOTAL durationMs = ", generated);
    Assert.True(
      generated.IndexOf("// HELIX MIXIN FINAL STATE", StringComparison.Ordinal) >
      generated.IndexOf("partial class Demo", StringComparison.Ordinal)
    );
    Assert.Contains("global::PreludeOnlyAttribute on global::Demo", generated);
    Assert.Contains("// PRELUDE PROGRAM (PREPARED)\n//   @MIXIN<$Init> PreludeOnlyLogic();", generated);
    Assert.Contains("//   @CODE<CLASS> public const string RewrittenName = \"@this:name\";", generated);
    Assert.Contains("public const string RewrittenName = \"Demo\";", generated);
    Assert.Contains("global::ExternalAttribute on global::Demo.Value", generated);
    Assert.Contains("// PRELUDE PROGRAM (PREPARED)\n//   @CALL<Prepare>\n//   @CARRY<Name> @target:name", generated);
    Assert.DoesNotContain("//   @FUNC<Prepare>", generated);
    Assert.Contains("// LATE PROGRAM (PREPARED)\n//   @CODE<CLASS> public const string CarriedName", generated);
    Assert.Contains("// CARRIED VALUES\n//   @carry#Name = Value", generated);
    Assert.DoesNotContain("//   @carry#Unused =", generated);
    Assert.Contains("// SHARED VARIABLES\n//   @var#Prepared = true", generated);
    Assert.Equal(
      1,
      generated.Split(new[] { "//   @var#Prepared = true" }, StringSplitOptions.None).Length - 1
    );
    Assert.Contains("PreludeOnlyLogic();", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void DebugStringPoolOptionPrintsPoolAndInternedProgramIds() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                          }
                          [AttributeUsage(AttributeTargets.Class)] public sealed class PooledAttribute : Attribute { }
                          [HELIX.Mixable, Pooled] public partial class Demo { }
                          """;
    var compilation = CSharpCompilation.Create(
      "DebugStringPoolTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      generators: new[] { new MixinGenerator().AsSourceGenerator() },
      additionalTexts: new AdditionalText[] {
        new TestAdditionalText(
          "/project/Pool.HelixSourceGenerator.additionalfile",
          "@CONFIG<DEBUG> | Other StringPool\n" +
          "@ANNOTATION<PooledAttribute>\n" +
          "@CODE<CLASS> public int PooledValue;\n@END"
        )
      },
      parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("// INTERNED STRING POOL\n//   §", generated);
    Assert.Contains(" = CODE", generated);
    Assert.Contains("// LATE PROGRAM (PREPARED)\n//   @§", generated);
    Assert.Contains("public int PooledValue;", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void InlineExpandsPreparedFunctionsBeforePreludeAndLateHoisting() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                          }
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class InlineAttribute : Attribute {
                            public InlineAttribute(bool skip) { }
                          }
                          [HELIX.Mixable] public partial class Demo {
                            [Inline(false)] private void Run() { }
                            private void Accept(int value) { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "InlineMixinTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      generators: new[] { new MixinGenerator().AsSourceGenerator() },
      additionalTexts: new AdditionalText[] {
        new TestAdditionalText(
          "/project/Inline.HelixSourceGenerator.additionalfile",
          "@CONFIG<DEBUG>\n" +
          "@FUNC<PreludePart>\n" +
          "@CODE<CLASS> private int FromPrelude;\n" +
          "@CODE<CLASS> private void MultiLine() {\n@\\  FromPrelude = 1;\n@\\}\n@END\n" +
          "@ANNOTATION<InlineAttribute>\n" +
          "@PRELUDE\n@INLINE<PreludePart>\n@END\n" +
          "@FUNC<LatePart>\n" +
          "@MIXIN<$Init> @target:name();\n" +
          "@MIXIN<$Init> Accept(\n@\\  1\n@\\);\n@RETURN\n" +
          "@MIXIN<$Init> MustNotBeGenerated();\n@END\n" +
          "@INLINE<LatePart>\n@END"
        )
      },
      parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("private int FromPrelude;", generated);
    Assert.Contains("private void MultiLine()", generated);
    Assert.Contains("FromPrelude = 1;", generated);
    Assert.Contains("Run();", generated);
    Assert.Contains("Accept(", generated);
    Assert.DoesNotContain("@INLINE", generated);
    Assert.Contains("//   @GOTO<__inline_", generated);
    Assert.Contains("//   @CARRY<__0> @target:name", generated);
    Assert.DoesNotContain("MustNotBeGenerated();", generated.Substring(generated.IndexOf("// <auto-generated/>", StringComparison.Ordinal)));
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void LateInlineSeesVariablesCommittedByAllPrimaryPreludes() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                          }
                          [AttributeUsage(AttributeTargets.Class)] public sealed class ListenerAttribute : Attribute { }
                          [AttributeUsage(AttributeTargets.Method)] public sealed class ClickAttribute : Attribute { }
                          [HELIX.Mixable, Listener] public partial class Demo {
                            [Click] private void OnClick() { }
                            private void Listen() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "InlineVariableCarryTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      generators: new[] { new MixinGenerator().AsSourceGenerator() },
      additionalTexts: new AdditionalText[] {
        new TestAdditionalText(
          "/project/InlineVariables.HelixSourceGenerator.additionalfile",
          "@FUNC<RequireState>\n" +
          "@SCOPE\n@MATCH @var#HasState:!?eq<true>\n" +
          "@VAR<HasState> true\n@CODE<CLASS> private int state;\n@END\n@END\n" +
          "@ANNOTATION<ListenerAttribute>\n" +
          "@INLINE<RequireState>\n@MIXIN<$Init> Listen();\n@END\n" +
          "@ANNOTATION<ClickAttribute>\n" +
          "@PRELUDE\n@INLINE<RequireState>\n@END\n" +
          "@MIXIN<$Init> OnClick();\n@END"
        )
      },
      parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Equal(1, CountOccurrences(generated, "private int state;"));
    Assert.Contains("Listen();", generated);
    Assert.Contains("OnClick();", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PreludeModelSharesPersistentVariablesButKeepsCarriesLocal() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinExpression("@SCOPE\n@MATCH @var#Declared:!?eq<true>\n@CODE<CLASS> private int shared;\n@VAR<Declared> true\n@END", Prelude = "")]
                          [AttributeUsage(AttributeTargets.Method)] public sealed class LateAttribute : Attribute { }
                          [HELIX.Mixable] public partial class Demo {
                            [Late] private void First() { }
                            [Late] private void Second() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "PreludeSharedVariablesTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Equal(1, text.Split(new[] { "private int shared;" }, StringSplitOptions.None).Length - 1);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PreludeModelDistinguishesRawAndUnwrappedTypeAttributeValues() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinExpression("@CODE<CLASS> public static global::System.Type Raw = @attr#scope;\n@CODE<CLASS> public const string Unwrapped = \"@attr#scope:unwrap\";\n@CODE<CLASS> public const string RawText = @attr#text;", Prelude = "")]
                          [AttributeUsage(AttributeTargets.Field)] public sealed class LateAttribute : Attribute {
                            public LateAttribute(Type scope, string text = "quoted") { }
                          }
                          [HELIX.Mixable] public partial class Demo {
                            [Late(typeof(string))] private int Value;
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "PreludeTypeAttributeTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("Raw = typeof(global::System.String);", text);
    Assert.Contains("Unwrapped = \"System.String\";", text);
    Assert.Contains("RawText = \"quoted\";", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PreludeModelPreparesDynamicMixinTargetFromCarriedAttributeValue() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinExpression("@MIXIN<(@attr#target:unwrap)><(@attr#order)> Consume();", Prelude = "")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class LateAttribute : Attribute {
                            public LateAttribute(string target = "Tick", int order = 3) { }
                          }
                          [HELIX.Mixable] public partial class Demo {
                            [Late] public void Source() { }
                            private void Consume() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "PreludeDynamicMixinTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("void Tick()", text);
    Assert.Contains("Consume();", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PreludeModelCarriesNamedDefaultAfterEarlierOptionalParameter() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinExpression("@MIXIN<(@attr#target:unwrap)><(@attr#order)> Source();", Prelude = "")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class LateAttribute : Attribute {
                            public LateAttribute(string time = "tick", string target = "Update", string condition = null, int order = 10) { }
                          }
                          [HELIX.Mixable] public partial class Demo {
                            [Late] private void Source() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "PreludeOptionalTargetTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("void Update()", text);
    Assert.DoesNotContain("void _(", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PreludeModelRendersDeclaredTargetContributionsLate() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string target, int order, string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinExpression("Generated", 7, "@CODE<TARGET> Consume(\"@target:name\");", Prelude = "")]
                          [AttributeUsage(AttributeTargets.Method)] public sealed class LateAttribute : Attribute { }
                          [HELIX.Mixable] public partial class Demo {
                            [Late] public void Source() { }
                            private void Consume(string value) { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "PreludeTargetTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("void Generated()", text);
    Assert.Contains("Consume(\"Source\");", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PreludeModelRejectsImportedCallsInLateExpression() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinLibraryAttribute : Attribute { public MixinLibraryAttribute(string value) { } }
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinImportAttribute : Attribute { public MixinImportAttribute(Type value) { } }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinLibrary("@FUNC<Imported>\n@CODE<CLASS> public int Value;\n@END")]
                          public static class Library { }
                          [HELIX.MixinImport(typeof(Library))]
                          [HELIX.MixinExpression("@CALL<Imported>", Prelude = "")]
                          [AttributeUsage(AttributeTargets.Class)] public sealed class LateAttribute : Attribute { }
                          [HELIX.Mixable, Late] public partial class Demo { }
                          """;
    var compilation = CSharpCompilation.Create(
      "PreludeImportedCallTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGenerators(compilation);

    var diagnostic = Assert.Single(driver.GetRunResult().Diagnostics.Where(item => item.Id == "HLXM11"));
    Assert.Contains("may only call functions declared in the same expression", diagnostic.GetMessage());
  }

  [Fact]
  public void PreludeModelAutomaticallyHoistsRoslynAccessesAndRunsExpressionLate() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@LOCAL<Name> @target:name\n@LOCAL<NameCopy> @target:name\n@CODE<CLASS> public @target:type @(local#NameCopy)Generated;",
                            Prelude = ""
                          )]
                          [AttributeUsage(AttributeTargets.Field)] public sealed class LateAttribute : Attribute { }
                          [HELIX.Mixable] public partial class Demo {
                            [Late] public string Text;
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "PreludeExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("public global::System.String TextGenerated;", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void LatePipelineIsCachedWhenDetachedPrimaryModelIsUnchanged() {
    const string prefix = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string LateExpression { get; set; }
                            }
                          }
                          [HELIX.MixinExpression("@CARRY<Name> @target:name", LateExpression = "@CODE<CLASS> public const string Value = \"@carry#Name\";")]
                          [AttributeUsage(AttributeTargets.Method)] public sealed class CachedAttribute : Attribute { }
                          [HELIX.Mixable] public partial class Demo {
                            [Cached] public void Work() { int value =
                          """;

    CSharpCompilation Compilation(string value) => CSharpCompilation.Create(
      "LateCacheTest",
      new[] { CSharpSyntaxTree.ParseText(prefix + value + "; } }", new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );

    GeneratorDriver driver = CSharpGeneratorDriver.Create(
      new[] { new MixinGenerator().AsSourceGenerator() },
      driverOptions: new GeneratorDriverOptions(
        IncrementalGeneratorOutputKind.None,
        trackIncrementalGeneratorSteps: true
      )
    );
    driver = driver.RunGenerators(Compilation("1"));
    driver = driver.RunGenerators(Compilation("2"));

    var steps = Assert.Single(driver.GetRunResult().Results).TrackedSteps["Mixin.Evaluation"];
    Assert.All(
      steps.SelectMany(step => step.Outputs),
      output => Assert.Equal(IncrementalStepRunReason.Cached, output.Reason)
    );
  }

  [Fact]
  public void CarryRetainsDetachedTargetSemanticsForLateExpression() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string LateExpression { get; set; }
                            }
                          }
                          [HELIX.MixinExpression(
                            "",
                            LateExpression =
                              "@CARRY<Target> @target\n" +
                              "@CARRY<AttributeTable> @attr:table\n" +
                              "@ASSERT @carry#Target:?has<Length>\n" +
                              "@ASSERT @carry#Target:type:?is<System.String>\n" +
                              "@ASSERT @carry#Target:type:?is<System.IComparable>\n" +
                              "@CODE<CLASS> public const string Carried = \"@carry#Target:name|@carry#Target:type|@carry#Target:visibility|@carry#AttributeTable#0:name\";"
                          )]
                          [AttributeUsage(AttributeTargets.Field)] public sealed class CarryAttribute : Attribute { }
                          [HELIX.Mixable]
                          public partial class Demo {
                            [Carry] public string Text;
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "CarryExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("Carried = \"Text|System.String|public|CarryAttribute\"", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void CarryRetainsDetachedGenericTypeArgumentsForLateExpression() {
    const string source = """
                          using System;
                          using System.Collections.Generic;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string Prelude { get; set; }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@ASSERT @carry#Type#0:?is<System.String>\n" +
                            "@ASSERT @carry#Type#T:?is<System.String>\n" +
                            "@CODE<CLASS> public const string ItemType = \"@carry#Type#0\";",
                            Prelude = "@CARRY<Type> @target:type"
                          )]
                          [AttributeUsage(AttributeTargets.Field)] public sealed class GenericAttribute : Attribute { }
                          [HELIX.Mixable] public partial class Demo {
                            [Generic] private List<string> Values;
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "DetachedGenericCarryTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("ItemType = \"System.String\";", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void LateExpressionUsesUnlinkedPrimaryVariables() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
                            public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                              public string LateExpression { get; set; }
                            }
                          }
                          [HELIX.MixinExpression(
                            "@VAR<Name> @target:name",
                            LateExpression = "@CODE<CLASS> public const string LateName = \"@var#Name\";"
                          )]
                          [AttributeUsage(AttributeTargets.Method)] public sealed class LateAttribute : Attribute { }
                          [HELIX.Mixable]
                          public partial class Demo {
                            [Late] public void Work() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "LateExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("LateName = \"Work\"", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void MapValuesCanSelectConstructorArgumentFromAttributeParameter() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                          }
                          [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                          public sealed class MarkerAttribute : Attribute {
                            public MarkerAttribute(string name) { }
                          }
                          [HELIX.MixinExpression(
                            "@FUNC<ReadName>\n@RETURN @param#name:unwrap\n@END\n" +
                            "@LOCAL<items> @target:attributesOf<MarkerAttribute>\n" +
                            "@LOCAL<names> @local#items:mapValues<ReadName>\n" +
                            "@CODE<CLASS> public const string Names = \"@local#names:joinValues<,>\";"
                          )]
                          [AttributeUsage(AttributeTargets.Method)] public sealed class ReadMarkersAttribute : Attribute { }
                          [HELIX.Mixable]
                          public partial class Demo {
                            [ReadMarkers, Marker("Hello"), Marker("World"), Marker("Test")]
                            public void Work() { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "AttributeMapValueTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("Names = \"Hello,World,Test\"", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  private void ImportedHookFunctionKeepsRoslynConstantsRawUntilRendering() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinLibraryAttribute : Attribute {
                              public MixinLibraryAttribute(string content) { }
                            }
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinImportAttribute : Attribute {
                              public MixinImportAttribute(Type type) { }
                            }
                            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)] public sealed class MixinDefineTargetAttribute : Attribute {
                              public MixinDefineTargetAttribute(string key, string target) { }
                            }
                            [AttributeUsage(AttributeTargets.Method)]
                            [MixinImport(typeof(CoreLibrary))]
                            [MixinExpression("@CALL<HookImpl>")]
                            public sealed class HookAttribute : Attribute {
                              public HookAttribute(string target = null, int order = 0) { }
                            }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string expression) { }
                            }
                            [MixinLibrary("@FUNC<HookImpl>\n@LOCAL<Name> @attr#target:unwrap\n@SCOPE\n@MATCH @local#Name:eq<null>\n@LOCAL<Name> $@target:name:replaceFirst<^On><>\n@END\n@RESOLVE_MIXIN<Delegate> @local#Name\n@ASSERT @local#Delegate:!?eq<null>\n@MIXIN<(@local#Name)><(@attr#order)> @target:name(@local#Delegate:wire<(@target)>);\n@END")]
                            public static class CoreLibrary { }
                          }
                          [HELIX.Mixable, HELIX.MixinDefineTarget("$Compose", "Compose")]
                          public partial class Demo {
                            private delegate void ComposeDelegate(int value);
                            private partial void Compose(int value);
                            [HELIX.Hook(order: -2)] private void OnCompose(int value) { }
                          }
                          """;
    var compilation = CSharpCompilation.Create(
      "HookRawValueTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("OnCompose", text);
  }

  [Fact]
  public void AttributeFunctionsReturnOrderedAndFilteredAttributeValues() {
    const string source = "using System;\n" + """
                                              namespace HELIX {
                                                [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                                                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                                                  public MixinExpressionAttribute(string expression) { }
                                                }
                                              }
                                              public class BaseMarkerAttribute : Attribute { }
                                              public sealed class DerivedMarkerAttribute : BaseMarkerAttribute { }
                                              public sealed class OtherMarkerAttribute : Attribute { }
                                              [HELIX.MixinExpression(
                                                "@LOCAL<markers> @target:attributesOf<BaseMarkerAttribute>\n" +
                                                "@CODE<CLASS> public const string Counts = \"@target:attributes:size|@local#markers:size|@target:attributesOfExact<DerivedMarkerAttribute>:size\";\n" +
                                                "@CODE<CLASS> public const string First = \"@target:attributeOf<BaseMarkerAttribute>:name\";"
                                              )]
                                              [AttributeUsage(AttributeTargets.Method)] public sealed class InspectAttribute : Attribute { }
                                              [HELIX.Mixable]
                                              public partial class Demo {
                                                [Inspect, DerivedMarker, OtherMarker] private void Work() { }
                                              }
                                              """;
    var compilation = CSharpCompilation.Create(
      "AttributeFunctionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources).SourceText.ToString();
    Assert.Contains("Counts = \"3|1|1\"", text);
    Assert.Contains("First = \"DerivedMarkerAttribute\"", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PropStructDirectiveGeneratesMethodParameterStructAndUnwrappedCall() {
    const string source = "using System;\n" + PropStructDatatypeRuntime + """
                                                                          namespace HELIX {
                                                                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                                                                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                                                                              public MixinExpressionAttribute(string expression) { }
                                                                            }
                                                                          }
                                                                          [AttributeUsage(AttributeTargets.Method)]
                                                                          [HELIX.MixinExpression(
                                                                            "@PROP_STRUCT<WorkProps><workProps><datatype> @target\n" +
                                                                            "@ASSERT @local#workProps:!?structAugment\n" +
                                                                            "@CODE<CLASS> private void Forward(@local#workProps:structParams) { Work(@local#workProps:structArgs); }\n" +
                                                                            "@CODE<CLASS> private void Dispatch(WorkProps value) { @local#workProps:propStructCall<this.Work><value>; }"
                                                                          )]
                                                                          public sealed class GenerateWorkPropsAttribute : Attribute { }
                                                                          [AttributeUsage(AttributeTargets.Parameter)]
                                                                          [HELIX.MixinExpression(
                                                                            "@CODE global::HELIX.Boot.CommandBridge.Named(datatype, \"@target:name\");"
                                                                          )]
                                                                          public sealed class NamedArgAttribute : Attribute { }

                                                                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
    Assert.Contains("Forward(global::System.Int32 count, in global::System.String label)", text);
    Assert.Contains("Work(count, in label);", text);
    Assert.Contains("this.Work(value.count, in value.label);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PropStructDirectiveGeneratesClassFieldStructAndUnwrappedCall() {
    const string source = "using System;\n" + PropStructDatatypeRuntime + """
                                                                          namespace HELIX {
                                                                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                                                                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixinExpressionAttribute : Attribute {
                                                                              public MixinExpressionAttribute(string expression) { }
                                                                            }
                                                                          }
                                                                          [AttributeUsage(AttributeTargets.Class)]
                                                                          [HELIX.MixinExpression(
                                                                            "@PROP_STRUCT<Snapshot><snapshot><datatype> @target\n" +
                                                                            "@CODE<CLASS> private void Dispatch(Snapshot value) { @local#snapshot:propStructCall<Consume><value>; }"
                                                                          )]
                                                                          public sealed class GenerateSnapshotAttribute : Attribute { }

                                                                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
                                                                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                                                                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                                                                              public MixinExpressionAttribute(string expression) { }
                                                                            }
                                                                          }
                                                                          [AttributeUsage(AttributeTargets.Method)]
                                                                          [HELIX.MixinExpression(
                                                                            "@PROP_STRUCT<WorkProps><workProps> @target\n" +
                                                                            "@ASSERT @local#workProps:?structNoArgs\n" +
                                                                            "@ASSERT @local#workProps:!?structHasEquality\n" +
                                                                            "@ASSERT @local#workProps:!?structAugment"
                                                                          )]
                                                                          public sealed class GenerateWorkPropsAttribute : Attribute { }

                                                                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public struct WorkProps", text);
    Assert.DoesNotContain("public WorkProps(", text);
    Assert.DoesNotContain("StructureDatatype<WorkProps>", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void PropStructNoGenerateFlagOnlyReturnsTheHandle() {
    const string source = "using System;\n" + """
                                              namespace HELIX {
                                                [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                                                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] public sealed class MixinExpressionAttribute : Attribute {
                                                  public MixinExpressionAttribute(string expression) { }
                                                }
                                              }
                                              [AttributeUsage(AttributeTargets.Method)]
                                              [HELIX.MixinExpression(
                                                "@PROP_STRUCT<WorkProps><workProps><noGenerate><datatype> @target\n" +
                                                "@ASSERT @local#workProps:!?structNoArgs\n" +
                                                "@CODE<CLASS> private void Forward(@local#workProps:structParams) { Work(@local#workProps:structArgs); }"
                                              )]
                                              public sealed class GenerateWorkPropsAttribute : Attribute { }

                                              [HELIX.Mixable]
                                              public partial class Demo {
                                                [GenerateWorkProps] private void Work(int count) { }
                                              }
                                              """;

    var compilation = CSharpCompilation.Create(
      "NoGenerateMethodPropStructExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.DoesNotContain("public struct WorkProps", text);
    Assert.DoesNotContain("StructureDatatype<WorkProps>", text);
    Assert.Contains("Forward(global::System.Int32 count)", text);
    Assert.Contains("Work(count);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AugmentStructGeneratesPropMembersAndExposesStructModelPredicates() {
    const string source = "using System;\n" + """
                                              namespace HELIX {
                                                [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                                                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                                                  public MixinExpressionAttribute(string expression) { }
                                                }
                                              }
                                              public abstract class GenericBase<T> { }

                                              [AttributeUsage(AttributeTargets.Class)]
                                              [HELIX.MixinExpression(
                                                "@AUGMENT_STRUCT<props> @this#Props\n" +
                                                "@ASSERT @local#props:?structHasEquality\n" +
                                                "@ASSERT @local#props:!?structNoArgs\n" +
                                                "@ASSERT @local#props:?structAugment\n" +
                                                "@CODE<IMPLEMENTS> @attr#Base:makeGeneric<(@this#Props:type)>\n" +
                                                "@CODE<CLASS> private void Expanded(@local#props:structParams) { Consume(@local#props:structArgs); }\n" +
                                                "@CODE<CLASS> private void ExpandedWithPrefix(@local#props:structParams<int extra>) { ConsumeWithPrefix(@local#props:structArgs<extra>); }\n" +
                                                "@CODE<CLASS> public const string TargetVisibility = \"@this:visibility\";\n" +
                                                "@CODE<CLASS> public const string FieldVisibility = \"@this#hidden:visibility\";\n" +
                                                "@CODE<CLASS> public const string ClosedFromLiteral = \"@attr#Base:makeGeneric<System.Int32>\";"
                                              )]
                                              public sealed class GenerateAttribute : Attribute {
                                                public GenerateAttribute(Type Base) { }
                                              }

                                              [HELIX.Mixable]
                                              [Generate(typeof(GenericBase<>))]
                                              public partial class Demo {
                                                private int hidden;
                                                private void Consume(int count) { }
                                                private void ConsumeWithPrefix(int extra, int count) { }

                                                public partial struct Props : IEquatable<Props> {
                                                  public int count;
                                                }
                                              }
                                              """;

    var compilation = CSharpCompilation.Create(
      "AugmentStructExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("partial class Demo : global::GenericBase<global::Demo.Props>", text);
    Assert.Contains("partial struct Props", text);
    Assert.Contains("public Props(", text);
    Assert.Contains("public bool Equals", text);
    Assert.Contains("public override int GetHashCode", text);
    Assert.Contains("Expanded(global::System.Int32 count)", text);
    Assert.Contains("Consume(count);", text);
    Assert.Contains("ExpandedWithPrefix(int extra, global::System.Int32 count)", text);
    Assert.Contains("ConsumeWithPrefix(extra, count);", text);
    Assert.Contains("TargetVisibility = \"public\";", text);
    Assert.Contains("FieldVisibility = \"private\";", text);
    Assert.Contains("ClosedFromLiteral = \"global::GenericBase<global::System.Int32>\";", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AugmentStructDoesNotGenerateConstructorForEmptyStruct() {
    const string source = "using System;\n" + """
                                              namespace HELIX {
                                                [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                                                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                                                  public MixinExpressionAttribute(string expression) { }
                                                }
                                              }
                                              public abstract class GenericBase<T> { }
                                              [AttributeUsage(AttributeTargets.Class)]
                                              [HELIX.MixinExpression(
                                                "@AUGMENT_STRUCT<props> @this#Props\n" +
                                                "@ASSERT @local#props:!?structHasEquality\n" +
                                                "@ASSERT @local#props:?structNoArgs\n" +
                                                "@ASSERT @local#props:?structAugment\n" +
                                                "@CODE<IMPLEMENTS> @attr#Base:makeGeneric<(@this#Props:type)>\n" +
                                                "@CODE<CLASS> public const string GeneratedPropsType = \"@this#Props:type\";\n" +
                                                "@CODE<CLASS> public const bool NoConstructor = @local#props:?structNoArgs;\n" +
                                                "@CODE<CLASS> private void Expanded(@local#props:structParams) { }\n" +
                                                "@CODE<CLASS> private void ExpandedWithPrefix(@local#props:structParams<int extra>) { Consume(@local#props:structArgs<extra>); }"
                                              )]
                                              public sealed class GenerateAttribute : Attribute {
                                                public GenerateAttribute(Type Base) { }
                                              }

                                              [HELIX.Mixable]
                                              [Generate(typeof(GenericBase<>))]
                                              public partial class Demo {
                                                private void Consume(int extra) { }
                                              }
                                              """;

    var compilation = CSharpCompilation.Create(
      "EmptyAugmentStructExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("partial class Demo : global::GenericBase<global::Demo.Props>", text);
    Assert.Contains("public struct Props { }", text);
    Assert.DoesNotContain(" Props(", text);
    Assert.Contains("GeneratedPropsType = \"global::Demo.Props\";", text);
    Assert.Contains("NoConstructor = true;", text);
    Assert.Contains("Expanded()", text);
    Assert.Contains("ExpandedWithPrefix(int extra)", text);
    Assert.Contains("Consume(extra);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void TargetDefinitionsOnTheClassAndItsAttributesOverridePseudonymsInOrder() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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

                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
  public void AdditionalFileAnnotationCanInjectDirectlyIntoAnyTarget() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string target, int order, string expression) { }
                            }
                          }
                          [HELIX.MixinExpression("$Init", 0, "@CODE<Other> Wrong()")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class MarkAttribute : Attribute { }
                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGenerators(compilation);
    var result = Assert.Single(driver.GetRunResult().Results);

    Assert.Empty(result.Diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(result.GeneratedSources).SourceText.ToString();
    Assert.Contains("private void Other()", generated);
    Assert.Contains("Wrong();", generated);
  }

  [Fact]
  public void AttributeExpressionCanReadAPropertyInitializerDefault() {
    const string source = """
                          using System;
                          namespace HELIX {
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
                            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
                              public MixinExpressionAttribute(string target, int order, string expression) { }
                            }
                          }
                          [HELIX.MixinExpression("$Init", 1, "@CODE<$Init> Capture(@attr#Priority);")]
                          [AttributeUsage(AttributeTargets.Method)]
                          public sealed class EventHandlerAttribute : Attribute {
                            public int Priority { get; set; } = 0;
                          }
                          [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
                          [HELIX.Mixable, Marker]
                          public partial class Demo { }
                          """;

    var compilation = CSharpCompilation.Create(
      "TargetlessExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
                            [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
                          [HELIX.Mixable, Marker]
                          public partial class Demo { }
                          """;

    var compilation = CSharpCompilation.Create(
      "AnnotationAndUsingExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("using DemoAnnotations;", generated);
    Assert.Contains("[GeneratedMarker]", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  private void AttributeExecutesAllInheritedExpressionsBaseFirst() {
    const string source =
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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

      [HELIX.Mixable, DerivedMarker]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
    const string source =
      """
      using System;
      using System.Collections.Generic;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
      [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public global::System.Int32 GeneratedValue;", generated);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionCanRenderFullNamesAndUnwrapStringsAndTypes() {
    const string source =
      """
      using System;
      using System.Collections.Generic;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
      [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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

  private void ImportedFunctionLibraryIsAvailableToMixinAttributes() {
    const string source =
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public sealed class MixinImportAttribute : Attribute {
          public MixinImportAttribute(Type library) { }
        }
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixinLibraryAttribute : Attribute {
          public MixinLibraryAttribute(string content) { }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
        public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string target, int order, string expression) { }
        }
      }
      [HELIX.MixinLibrary(
        "@FUNC<emit>\n" +
        "@CODE<$Init> Prepared()\n" +
        "@RETURN\n" +
        "@END"
      )]
      public static class CommonFunctions { }
      [HELIX.MixinImport(typeof(CommonFunctions))]
      [HELIX.MixinExpression("$Init", 0, "@CALL<emit>\n@CODE<$Init> Local()")]
      [AttributeUsage(AttributeTargets.Method)]
      public sealed class MarkAttribute : Attribute { }
      [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var generated = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    var preparedCall = generated.IndexOf("Prepared();", StringComparison.Ordinal);
    var localCall = generated.IndexOf("Local();", StringComparison.Ordinal);
    Assert.True(preparedCall >= 0 && preparedCall < localCall);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  private void InvalidImportedLibraryReportsDedicatedDiagnostic() {
    const string source =
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public sealed class MixinImportAttribute : Attribute {
          public MixinImportAttribute(Type library) { }
        }
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixinLibraryAttribute : Attribute {
          public MixinLibraryAttribute(string content) { }
        }
      }
      [HELIX.MixinLibrary("@FUNC<broken>\n@UNKNOWN\n@END")]
      public static class BrokenFunctions { }
      [HELIX.Mixable]
      [HELIX.MixinImport(typeof(BrokenFunctions))]
      public partial class Demo { }
      """;
    var compilation = CSharpCompilation.Create(
      "InvalidPreparedMixinExpressionTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGenerators(compilation);

    Assert.Contains(
      Assert.Single(driver.GetRunResult().Results).Diagnostics,
      item =>
        item.Id == "HLXM11" && item.GetMessage().Contains("line 2") &&
        item.GetMessage().Contains("unknown directive")
    );
  }

  [Fact]
  public void ExpressionLogsAreReportedAsWarningDiagnostics() {
    const string source =
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
        public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      [HELIX.MixinExpression(
        "@LOG generating @this:name\n" +
        "@CODE<CLASS> public int Generated;"
      )]
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class MarkAttribute : Attribute { }
      [HELIX.Mixable, Mark]
      public partial class Demo { }
      """;
    var compilation = CSharpCompilation.Create(
      "MixinExpressionLogTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGenerators(compilation);
    var diagnostics = Assert.Single(driver.GetRunResult().Results).Diagnostics
      .Where(item => item.Id == "HLXM12")
      .ToArray();

    Assert.Single(diagnostics);
    Assert.All(diagnostics, item => Assert.Equal(DiagnosticSeverity.Warning, item.Severity));
    Assert.Equal("generating Demo", diagnostics[0].GetMessage());
  }

  [Fact]
  public void MixinExpressionCanDeclareItsOwnDynamicInjectionTargetAndPriority() {
    const string source =
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
      [HELIX.Mixable, Mark]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.True(
      text.IndexOf("Before();", StringComparison.Ordinal) < text.IndexOf("After();", StringComparison.Ordinal)
    );
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void ExpressionCanResolveCheckAndWireAMixinTarget() {
    const string source =
      """
      using System;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
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
      [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("Handle(value);", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  [Fact]
  public void AsyncPredicateRecognizesAsyncMethods() {
    const string source =
      """
      using System;
      using System.Threading.Tasks;
      namespace HELIX {
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)] public sealed class MixinExpressionAttribute : Attribute {
          public MixinExpressionAttribute(string expression) { }
        }
      }
      [HELIX.MixinExpression(
        "@ASSERT @target:?async\n@CODE<CLASS> public int AsyncMatched;"
      )]
      [AttributeUsage(AttributeTargets.Method)] public sealed class AsyncMarkerAttribute : Attribute { }
      [HELIX.Mixable]
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
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

    Assert.Empty(diagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    var text = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources)
      .SourceText.ToString();
    Assert.Contains("public int AsyncMatched;", text);
    Assert.Empty(output.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  private static int CountOccurrences(string text, string value) {
    var count = 0;
    for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
      count++;
    return count;
  }

  private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path))
    .ToImmutableArray<MetadataReference>();

  private const string PropStructDatatypeRuntime =
    """
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

internal sealed class TestAdditionalText : AdditionalText {
  private readonly SourceText _text;
  internal TestAdditionalText(string path, string text) {
    Path = path;
    _text = SourceText.From(text);
  }
  public override string Path { get; }
  public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default) => _text;
}
