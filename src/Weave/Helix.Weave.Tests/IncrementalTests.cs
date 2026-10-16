using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Helix.Weave.Tests;

/// <summary>
/// The point of the prelude/late split: an edit should re-run the preludes of the symbols it
/// touched and nothing else, and leave untouched types unrendered. These tests read the driver's
/// own step tracking, so they check what Roslyn actually cached rather than what we hoped it did.
/// </summary>
public class IncrementalTests {
  private readonly ITestOutputHelper _output;
  public IncrementalTests(ITestOutputHelper output) => _output = output;

  private const string Untouched = """
    using Fixtures;
    namespace Game {
      public partial class Alpha {
        [EventHandler(1)] void OnOne(Evt e) { }
        [EventHandler(2)] void OnTwo(Evt e) { }
      }
      public partial class Beta {
        [EventHandler(3)] void OnThree(Evt e) { }
      }
    }
    """;

  private static string Edited(string body) => $$"""
    using Fixtures;
    namespace Game {
      public partial class Gamma {
        [EventHandler(4)] void OnFour(Evt e) { {{body}} }
      }
    }
    """;

  private static (GeneratorDriver Driver, Compilation Compilation) First(string edited) {
    var generator = Harness.CompileGenerator(Harness.LowerFixtures());
    var compilation = Harness.Compilation(HelixStubs.Empty, Untouched, edited);
    var driver = Harness.Driver(generator).RunGenerators(compilation);
    return (driver, compilation);
  }

  /// <summary>Re-runs after replacing the last source file, and reports what each step did.</summary>
  private static (int Cached, int Rerun) Rerun(GeneratorDriver driver, Compilation compilation,
                                               string replacement, string stepName) {
    var trees = compilation.SyntaxTrees.ToArray();
    var updated = compilation
      .ReplaceSyntaxTree(trees[^1], Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
        replacement, (Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)trees[^1].Options,
        path: trees[^1].FilePath));

    var result = driver.RunGenerators(updated).GetRunResult().Results.Single();
    var cached = 0;
    var rerun = 0;

    foreach (var step in result.TrackedSteps.Where(pair => pair.Key == stepName).SelectMany(pair => pair.Value))
      foreach (var output in step.Outputs)
        if (output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged) cached++;
        else rerun++;

    return (cached, rerun);
  }

  [Fact]
  public void EditingOneMethodDoesNotRerunOtherSymbolsPreludes() {
    var (driver, compilation) = First(Edited("var x = 1;"));
    var (cached, rerun) = Rerun(driver, compilation, Edited("var x = 2;"),
                                WeaveGeneratorBase.TrackedPrelude);

    _output.WriteLine($"preludes cached: {cached}, re-run: {rerun}");
    Assert.True(cached > 0, "no prelude was cached — the pipeline is not incremental");
    Assert.True(cached > rerun, $"expected most preludes cached, got {cached} cached / {rerun} re-run");
  }

  [Fact]
  public void EditingOneTypeDoesNotRerenderTheOthers() {
    var (driver, compilation) = First(Edited("var x = 1;"));
    var (cached, rerun) = Rerun(driver, compilation, Edited("var x = 2;"),
                                WeaveGeneratorBase.TrackedType);

    _output.WriteLine($"types cached: {cached}, re-run: {rerun}");
    Assert.True(cached > 0, "every type was re-rendered — per-type output is not cached");
  }

  [Fact]
  public void AnUnrelatedEditLeavesEverythingCached() {
    // Changing only whitespace in the edited file: nothing a mixin can observe.
    var (driver, compilation) = First(Edited("var x = 1;"));
    var (cached, rerun) = Rerun(driver, compilation, Edited("var  x  =  1;"),
                                WeaveGeneratorBase.TrackedType);

    _output.WriteLine($"types cached: {cached}, re-run: {rerun}");
    Assert.Equal(0, rerun);
  }

  [Fact]
  public void AddingAnAnnotatedMethodOnlyRunsThatOnesPrelude() {
    // A structural edit, not just a body change: the new handler's prelude must run, and the
    // handlers in the other files must not.
    var (driver, compilation) = First(Edited("var x = 1;"));
    var (cached, rerun) = Rerun(driver, compilation, """
      using Fixtures;
      namespace Game {
        public partial class Gamma {
          [EventHandler(4)] void OnFour(Evt e) { var x = 1; }
          [EventHandler(5)] void OnFive(Evt e) { }
        }
      }
      """, WeaveGeneratorBase.TrackedPrelude);

    _output.WriteLine($"preludes cached: {cached}, re-run: {rerun}");
    Assert.True(cached >= 3, $"the untouched files' preludes should stay cached, got {cached}");
    Assert.True(rerun >= 1, "the new handler's prelude should have run");
  }

  [Fact]
  public void OutputIsUnchangedAcrossAnIncrementalRun() {
    var (driver, compilation) = First(Edited("var x = 1;"));
    var before = driver.GetRunResult().Results.Single().GeneratedSources
      .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

    var trees = compilation.SyntaxTrees.ToArray();
    var updated = compilation.ReplaceSyntaxTree(trees[^1],
      Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(Edited("var x = 2;"),
        (Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)trees[^1].Options, path: trees[^1].FilePath));

    var after = driver.RunGenerators(updated).GetRunResult().Results.Single().GeneratedSources
      .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

    // Caching must not change what is produced, only how much work it took.
    Assert.Equal(before.Keys.OrderBy(key => key), after.Keys.OrderBy(key => key));
    foreach (var key in before.Keys) Assert.Equal(before[key], after[key]);
  }
}
