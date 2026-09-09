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
        [AttributeUsage(AttributeTargets.Class)] public sealed class MixableAttribute : Attribute { }
      }
      namespace HELIX.Context {
        [AttributeUsage(AttributeTargets.Class)] public sealed class ComponentAttribute : Attribute { }
      }
      [HELIX.Mixable, HELIX.Context.Component]
      public partial class Demo { }
      """
    );

    Assert.Empty(result.GeneratorDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
    Assert.Empty(result.CompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error));
  }

  private static TestResult Run(string source) {
    var compilation = CSharpCompilation.Create(
      "MixinTargetTest",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      PlatformReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    GeneratorDriver driver = MixinTestDriver.Create(compilation);
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
