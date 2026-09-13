using System;
using System.IO;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Hix.Standalone;
using HELIX.SourceGen.Tests;
using Xunit;

namespace Hix.Standalone.Tests;

public class HixDecompilerLibraryTests {
  [Fact]
  public void EntireStandaloneLibraryIsDeterministicAndIdempotentThroughSourceAndBytecode() {
    var root = Path.Combine(AppContext.BaseDirectory, "HixTests");
    var project = HixProject.Load(Path.Combine(root, "manifest.hal"));
    var entries = HixProject.MatchFiles(root, project.Manifest.Sources.Test).ToArray();
    Assert.True(entries.Length >= 15, "The round-trip test must cover the whole example project");
    var working = Path.Combine(Path.GetTempPath(), "hix-decompiler-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(working);
    try {
      var totalTests = 0;
      foreach (var entry in entries) {
        var files = project.MainSources().Concat(HixFileImports.LoadFiles(entry)).DistinctBy(file => file.Path).ToArray();
        var backend = new HixTestBackend(TextWriter.Null, working);
        var sources = files.Select(file => file.Source).ToArray(); backend.SetCompilationSources(sources);
        var units = sources.Select(source => AntlrSyntax.Parse(source, backend)).ToArray();
        Assert.All(units, unit => Assert.Empty(unit.Diagnostics));
        var baseline = HixCompiler.CompileFunctions(sources, backend);
        Assert.Equal(baseline.Identity, HixCompiler.CompileFunctions(sources, backend).Identity);
        var rewritten = units.Select(HixDecompiler.ToSource).ToArray();
        Assert.Equal(rewritten, units.Select(HixDecompiler.ToSource).ToArray());
        Assert.Equal(rewritten, rewritten.Select(source => HixDecompiler.ToSource(AntlrSyntax.Parse(source, backend))).ToArray());
        var sourceRoundTrip = HixCompiler.CompileFunctions(rewritten, backend);
        HixRoundTripAssertions.Equal(baseline, sourceRoundTrip);
        var imageFile = Path.Combine(working, "library.hixbc");
        HixImageFile.Write(imageFile, baseline);
        var loaded = HixImageFile.Read(imageFile, backend, out var storedEntry);
        Assert.Equal("main", storedEntry);
        HixRoundTripAssertions.Equal(baseline, loaded);
        var recovered = HixDecompiler.Decompile(baseline);
        var recoveredGlobals = HixCompiler.PrepareGlobals([recovered], backend);
        HixRoundTripAssertions.Equal(baseline, new HixBytecodeCompiler(recoveredGlobals.StringPool).Compile([], [], recoveredGlobals));
        var decompiled = HixDecompiler.ToSource(recovered);
        Assert.Equal(decompiled, HixDecompiler.ToSource(HixDecompiler.Decompile(baseline)));
        var bytecodeRoundTrip = HixCompiler.CompileFunctions([decompiled], backend);
        HixRoundTripAssertions.Equal(baseline, bytecodeRoundTrip);
        Assert.Equal(decompiled, HixDecompiler.ToSource(HixDecompiler.Decompile(bytecodeRoundTrip)));
        HixRoundTripAssertions.Equal(bytecodeRoundTrip, HixCompiler.CompileFunctions([
          HixDecompiler.ToSource(HixDecompiler.Decompile(bytecodeRoundTrip))], backend));
        var tests = units.SelectMany(unit => unit.Declarations).OfType<FunctionDeclarationIr>()
          .Where(function => function.Metadata.Any(metadata => metadata.Name == "test")).ToArray();
        totalTests += tests.Length;
        foreach (var image in new[] {baseline, sourceRoundTrip, bytecodeRoundTrip, loaded}) {
          var vm = new HixVM([image]);
          foreach (var test in tests) {
            var cases = test.Metadata.Where(metadata => metadata.Name == "testcase").ToArray();
            if (cases.Length == 0) cases = [null];
            foreach (var testCase in cases) {
              var metadata = testCase?.Values.Select(Constant).ToArray() ?? [];
              var args = metadata.Length == 0 ? [] : metadata[0] is TupleHixValue tuple ? tuple.Values.ToArray() : [metadata[0]];
              var thread = backend.CreateCoroutine(); var result = vm.Invoke(image, thread, test.Name, args);
              Assert.True(result.Success, test.Name + ": " + result.Error.Resolve(result.Strings));
              if (metadata.Length == 2) Assert.True(thread.Equal(result.Value, metadata[1]), test.Name);
            }
          }
        }
      }
      Assert.True(totalTests >= 100, "The round-trip runtime check must cover the full test library");
    } finally { Directory.Delete(working, true); }
  }
  private static IHixValue Constant(ExpressionIr expression) => expression switch {
    StringExpressionIr value => HixCoroutine.String(value.Value), NumberExpressionIr value => new NumberHixValue(value.Value),
    BooleanExpressionIr value => BooleanHixValue.From(value.Value), NullExpressionIr => NullHixValue.Instance, MissingExpressionIr => MissingHixValue.Instance,
    TupleExpressionIr value => new TupleHixValue(value.Values.Select(Constant).ToArray()),
    TableExpressionIr value => new HixTableValue(value.Entries.Select(item => new System.Collections.Generic.KeyValuePair<HixString, IHixValue>(HixString.Dynamic(item.Key), Constant(item.Value)))),
    _ => throw new ArgumentException("Test case must be constant")
  };
}
