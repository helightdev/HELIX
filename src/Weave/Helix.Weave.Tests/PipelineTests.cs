using System.Linq;
using Xunit;

namespace Helix.Weave.Tests;

/// <summary>
/// The pipeline as the build runs it: lower the fixture mixins, compile the lowered output into
/// a real generator, run it over user code. If the generated code is wrong, it fails to compile
/// here — which is why this needs no golden files.
/// </summary>
public class PipelineTests {
  private const string Handler = """
    using Fixtures;

    namespace Game {
      public partial class Player {
        [EventHandler(5)]
        void OnDamage(Evt evt) { }
      }
    }
    """;

  [Fact]
  public void FixturesLowerCleanly() {
    var lowered = Harness.LowerFixtures();
    Assert.True(lowered.Success, string.Join("\n", lowered.Errors));
    Assert.Equal(4, lowered.Mixins.Count);
    Assert.Contains("MixinRegistry.g.cs", lowered.Files.Keys);
  }

  [Fact]
  public void LibrarySourcesAreCopiedVerbatim() {
    var lowered = Harness.LowerFixtures();
    var original = System.IO.File.ReadAllText(
      System.IO.Path.Combine(Harness.FixturesDirectory, "Targets.lib.cs"));
    Assert.Equal(original, lowered.Files["Targets.lib.cs"]);
  }

  [Fact]
  public void LoweredOutputCompiles() {
    var lowered = Harness.LowerFixtures();
    Assert.NotNull(Harness.CompileGenerator(lowered));
  }

  [Fact]
  public void GeneratedCodeCompiles() {
    var run = Harness.RunFixtures(Handler);
    Assert.Empty(run.CompilationErrors);
    Assert.Empty(run.GeneratorDiagnostics);
  }

  [Fact]
  public void WeavesInjectionIntoNamedMethod() {
    var generated = Harness.RunFixtures(Handler).Single();
    Assert.Contains("partial class Player", generated);
    Assert.Contains("private void Awake()", generated);
    Assert.Contains("eventHandlerList.Register<global::Fixtures.Evt>(OnDamage, 5);", generated);
  }

  [Fact]
  public void RequireOnceRunsOnlyOnceAcrossMixins() {
    var generated = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        public partial class Player {
          [EventHandler] void OnA(Evt e) { }
          [EventHandler] void OnB(Evt e) { }
        }
      }
      """).Single();

    // The handler list comes from Shared.RequireHandler, guarded by a type-scoped slot.
    Assert.Equal(1, Occurrences(generated, "private readonly global::Fixtures.EventHandlerList eventHandlerList"));
    Assert.Equal(2, Occurrences(generated, "eventHandlerList.Register<"));
    Assert.Equal(1, Occurrences(generated, "eventHandlerList.UnregisterAll();"));
  }

  [Fact]
  public void InjectionOrdersByPriorityThenInsertion() {
    var generated = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        public partial class Player {
          [EventHandler(1)] void OnFirst(Evt e) { }
          [EventHandler(2)] void OnSecond(Evt e) { }
        }
      }
      """).Single();

    var first = generated.IndexOf("OnFirst, 1", System.StringComparison.Ordinal);
    var second = generated.IndexOf("OnSecond, 2", System.StringComparison.Ordinal);
    Assert.True(first >= 0 && second >= 0);
    Assert.True(first < second, "equal priorities must keep insertion order");
  }

  [Fact]
  public void ReportsShapeMismatchInsteadOfSilentlyEmittingNothing() {
    var run = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        public partial class Player {
          [EventHandler] void OnNothing(string notAnEvent) { }
        }
      }
      """);

    Assert.Contains(run.GeneratorDiagnostics, d => d.Id == "FX002");
  }

  [Fact]
  public void TwoPassMixinCarriesDataToTheLatePass() {
    var run = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        public partial class Console {
          [Command("spawn", Description = "spawns a thing")]
          static void Spawn() { }
        }
      }
      """);

    Assert.Empty(run.CompilationErrors);
    var generated = run.Single();
    Assert.Contains("\"spawn\"", generated);
    Assert.Contains("\"spawns a thing\"", generated);
    Assert.Contains("public static void Configure(", generated);
  }

  [Fact]
  public void AttributeDefaultsSurviveDecoding() {
    var generated = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        public partial class Console {
          [Command] static void Help() { }
        }
      }
      """).Single();

    // No name given: the decoder leaves it null and the mixin falls back to the method name.
    Assert.Contains("\"Help\"", generated);
    Assert.Contains("null", generated);
  }

  [Fact]
  public void DerivesFieldsIntoAConstructorAndEquality() {
    var run = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        [Structure]
        public partial struct Point {
          public int X;
          public string Label;
        }
      }
      """);

    Assert.Empty(run.CompilationErrors);
    var generated = run.Single();
    Assert.Contains("public Point(int X, string Label)", generated);
    Assert.Contains("this.X = X;", generated);
    Assert.Contains("this.X == other.X", generated);
    // Reference types go through the comparer; primitives compare directly.
    Assert.Contains("EqualityComparer<string>.Default.Equals(this.Label, other.Label)", generated);
    Assert.Contains("hashCode.Add(this.X);", generated);
  }

  [Fact]
  public void DerivationStepRewritesTheProperty() {
    var run = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        [Structure]
        public partial struct Settings {
          public int Required;
          [Prop("42")] public int Optional;
        }
      }
      """);

    Assert.Empty(run.CompilationErrors);
    Assert.Contains("public Settings(int Required, int Optional = 42)", run.Single());
  }

  [Fact]
  public void DerivationStepCanDropAPropertyFromEquality() {
    var generated = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        [Structure]
        public partial struct Tagged {
          public int Id;
          [Prop(Equatable = false)] public int Scratch;
        }
      }
      """).Single();

    Assert.Contains("this.Id == other.Id", generated);
    Assert.DoesNotContain("other.Scratch", generated);
    Assert.DoesNotContain("hashCode.Add(this.Scratch);", generated);
  }

  [Fact]
  public void StructureMixinReportsRequiredAfterOptional() {
    var run = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        [Structure]
        public partial struct Bad {
          [Prop("1")] public int Optional;
          public int Required;
        }
      }
      """);

    Assert.Contains(run.GeneratorDiagnostics, d => d.Id == "FX010");
  }

  [Fact]
  public void StructureWithoutEqualityEmitsOnlyTheConstructor() {
    var generated = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        [Structure(Equality = false)]
        public partial struct Plain { public int X; }
      }
      """).Single();

    Assert.Contains("public Plain(", generated);
    Assert.DoesNotContain("GetHashCode", generated);
  }

  private static int Occurrences(string text, string value) {
    var count = 0;
    for (var index = text.IndexOf(value, System.StringComparison.Ordinal); index >= 0;
         index = text.IndexOf(value, index + value.Length, System.StringComparison.Ordinal)) count++;
    return count;
  }
}
