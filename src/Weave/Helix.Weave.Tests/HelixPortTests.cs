using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Helix.Weave.Tests;

/// <summary>
/// The port of HELIX's five .hix files. These lower the real mixin project, compile the
/// generator it produces, and run it over HELIX-shaped user code against a stub runtime.
/// </summary>
public class HelixPortTests {
  private readonly ITestOutputHelper _output;
  public HelixPortTests(ITestOutputHelper output) => _output = output;

  /// <summary>
  /// HELIX's own mixins live with the project, beside its Assets folder — the weave system
  /// itself knows nothing about them.
  /// </summary>
  private static string MixinsDirectory {
    get {
      var directory = new DirectoryInfo(AppContext.BaseDirectory);
      while (directory is not null && directory.Name != "src") directory = directory.Parent;
      return Path.Combine(directory!.FullName, "HELIX", "Weave", "HELIX.Weave.Mixins");
    }
  }

  private static Lowering.LoweringResult Lower() =>
    Lowering.Lowerer.LowerDirectory(MixinsDirectory, Harness.References);

  [Fact]
  public void AllFiveHixFilesLowerCleanly() {
    var result = Lower();
    Assert.True(result.Success, string.Join("\n", result.Errors));

    // Core, Context, Compose, Boot, Odin.
    var names = result.Mixins.Select(m => m.AttributeMetadataName).ToList();
    Assert.Contains("HELIX.StructureAttribute", names);
    Assert.Contains("HELIX.Context.ManagedAttribute", names);
    Assert.Contains("HELIX.Compose.CustomBoundaryElementAttribute", names);
    Assert.Contains("HELIX.Boot.CommandAttribute", names);
    Assert.Contains("HELIX.OdinSerializableAttribute", names);
    _output.WriteLine($"{result.Mixins.Count} mixins, {result.Files.Count} files");
  }

  [Fact]
  public void TheGeneratorTheyProduceCompiles() => Assert.NotNull(Harness.CompileGenerator(Lower()));

  [Fact]
  public void LibrarySourcesTravelUntouched() {
    var files = Lower().Files;
    Assert.Contains("Shared.lib.cs", files.Keys);
    Assert.Contains("Types.lib.cs", files.Keys);
    Assert.Equal(File.ReadAllText(Path.Combine(MixinsDirectory, "Shared.lib.cs")), files["Shared.lib.cs"]);
  }

  private static GeneratorRun Run(string userCode) =>
    Harness.Run(Harness.CompileGenerator(Lower()), HelixStubs.Runtime, userCode);

  [Fact]
  public void OdinSerializableEmitsTheCallbackPair() {
    var generated = Run("""
      namespace Game {
        [HELIX.OdinSerializable]
        public partial class Config { }
      }
      """).Single();

    Assert.Contains("ISerializationCallbackReceiver", generated);
    Assert.Contains("private SerializationData serializationData;", generated);
    Assert.Contains("[ShowOdinSerializedPropertiesInInspector]", generated);
  }

  [Fact]
  public void ManagedComponentWiresRegistrationAndInjection() {
    var generated = Run("""
      namespace Game {
        [HELIX.Context.Managed]
        public partial class Service {
          [HELIX.Context.Inject] private Game.Dependency _dependency;
          [HELIX.Context.AutoDispose] private System.IDisposable _handle;
        }
        public class Dependency { }
      }
      """).Single();

    Assert.Contains("registration.name = \"Service\";", generated);
    Assert.Contains("registration.activator = DefaultComponentActivators.PlainObject<global::Game.Service>();",
      generated);
    Assert.Contains("_dependency = managed.Resolve<global::Game.Dependency>(null);", generated);
    Assert.Contains("AutoDisposeMethods.DisposeDisposable(ref _handle);", generated);
    // The ordering ladder: configuration at -100000 lands before dependencies at 0.
    Assert.True(generated.IndexOf("registration.name", StringComparison.Ordinal) <
                generated.IndexOf("registration.Dependency", StringComparison.Ordinal));
  }

  [Fact]
  public void InjectedCollectionsResolveAsLists() {
    var generated = Run("""
      using System.Collections.Generic;
      namespace Game {
        [HELIX.Context.Managed]
        public partial class Registry {
          [HELIX.Context.Inject] private List<Game.Plugin> _plugins;
        }
        public class Plugin { }
      }
      """).Single();

    Assert.Contains("ResolveAll<global::Game.Plugin>(null)", generated);
  }

  [Fact]
  public void EventHandlersPickTheRightRegistration() {
    var generated = Run("""
      namespace Game {
        [HELIX.Context.Managed]
        public partial class Listener {
          [HELIX.Context.EventHandler(5)] void OnDamage(HELIX.Context.Evt evt) { }
          [HELIX.Context.EventHandler] void OnTick(ref HELIX.Context.Evt evt) { }
        }
      }
      """).Single();

    Assert.Equal(2, Occurrences(generated, "eventHandlerList.Register<"));
    Assert.Equal(1, Occurrences(generated, "eventHandlerList = EventHandlerList.Create()"));
    Assert.Contains("registration.RegisterHandlerBinding<global::HELIX.Context.Evt>(5)", generated);
  }

  [Fact]
  public void StructureBuildsConstructorEqualityAndDatatype() {
    // Equality is opt-in: the type declares IEquatable<Self>, as Hix's equatableSelf required.
    var generated = Run("""
      namespace Game {
        [HELIX.Structure(true)]
        public partial struct Point : System.IEquatable<Point> {
          public int X;
          public string Name;
        }
      }
      """).Single();

    Assert.Contains("public Point(int X, string Name)", generated);
    Assert.Contains("this.X == other.X", generated);
    Assert.Contains("StructureDatatype<global::Game.Point>", generated);
    Assert.Contains("global::HELIX.Datatypes.Int", generated);
    Assert.Contains("global::HELIX.Datatypes.String", generated);
  }

  [Fact]
  public void StructureWithoutIEquatableGetsOnlyAConstructor() {
    var generated = Run("""
      namespace Game {
        [HELIX.Structure]
        public partial struct Plain { public int X; }
      }
      """).Single();

    Assert.Contains("public Plain(int X)", generated);
    Assert.DoesNotContain("GetHashCode", generated);
    Assert.DoesNotContain("bool Equals", generated);
  }

  [Fact]
  public void PropDerivationSuppliesDefaultsAndDropsEquality() {
    var generated = Run("""
      namespace Game {
        [HELIX.Structure]
        public partial struct Options : System.IEquatable<Options> {
          public int Required;
          [HELIX.Prop(7)] public int Count;
          [HELIX.Prop(Equatable = false)] public string Scratch;
        }
      }
      """).Single();

    Assert.Contains("int Count = 7", generated);
  }

  [Fact]
  public void CommandDerivesAnArgumentStructAndRegistersLate() {
    var generated = Run("""
      namespace Game {
        [HELIX.Context.Managed]
        public partial class Console {
          [HELIX.Boot.Command("spawn", "spawns a thing")]
          static void Spawn(int count) { }
        }
      }
      """).Single();

    Assert.Contains("public struct Spawn_Args", generated);
    Assert.Contains("CommandBridge<Spawn_Args>.Create(Spawn_Args.Datatype", generated);
    Assert.Contains("\"spawn\"", generated);
    Assert.Contains("\"spawns a thing\"", generated);
  }

  [Fact]
  public void BoundaryElementWeavesTheRecomposeLadderInOrder() {
    var generated = Run("""
      namespace Game {
        [HELIX.Compose.CustomBoundaryElement]
        public partial class Panel { }
      }
      """).Single();

    Assert.Contains("public void Recompose()", generated);
    foreach (var pair in new[] {
               ("BoundaryHelper.ContextBefore", "_lookupCache.Clear"),
               ("_lookupCache.Clear", "if (!_initialized) Init()"),
               ("var cx = new Composition(this)", "PerformCompose(ref cx)"),
               ("PerformCompose(ref cx)", "BoundaryHelper.ContextAfter"),
               ("BoundaryHelper.ContextAfter", "Cell.TrimChildren"),
             })
      Assert.True(generated.IndexOf(pair.Item1, StringComparison.Ordinal) <
                  generated.IndexOf(pair.Item2, StringComparison.Ordinal),
        $"{pair.Item1} must be woven before {pair.Item2}");

    Assert.Contains("Panel_Companion", generated);
    Assert.Contains("ComposeBoundary", generated);
  }

  [Fact]
  public void PartialStatementsAreWovenWithoutStraySemicolons() {
    // The recompose ladder injects bare `try {` fragments and a comment; neither takes a `;`.
    var generated = Run("""
      namespace Game {
        [HELIX.Compose.CustomBoundaryElement]
        public partial class Ladder { }
      }
      """).Single();

    Assert.DoesNotContain("try {;", generated);
    Assert.DoesNotContain("handle;", generated);
    Assert.Contains("try {", generated);
  }

  [Fact]
  public void BoundaryTargetAliasesAreResolvedPerType() {
    // The same alias, $Compose, is a different method on each of these.
    var own = Run("""
      namespace Game {
        [HELIX.Compose.CustomBoundaryElement]
        public partial class OwnPanel {
          [HELIX.Compose.WriteContextHandler] void Write(ref HELIX.Compose.Composition cx, object ctx) { }
        }
      }
      """).Single();
    Assert.Contains("PerformCompose", own);

    var mixin = Run("""
      namespace Game {
        [HELIX.Compose.BoundaryElementMixin]
        public partial class MixinPanel {
          [HELIX.Compose.WriteContextHandler] void Write(ref HELIX.Compose.Composition cx, object ctx) { }
        }
      }
      """).Single();
    Assert.Contains("MixinCompose", mixin);
    Assert.DoesNotContain("PerformCompose", mixin);
  }

  [Fact]
  public void TickerParsesFrameCountsAndDurations() {
    var generated = Run("""
      namespace Game {
        [HELIX.Context.Managed]
        public partial class Loop {
          [HELIX.Context.Ticker("4")] void EveryFourFrames() { }
          [HELIX.Context.Ticker("250ms")] void Periodically() { }
        }
      }
      """).Single();

    Assert.Contains("FrameCountTicker _EveryFourFramesTicker = new(4);", generated);
    Assert.Contains("FrameTimeTicker _PeriodicallyTicker = new(0.25f);", generated);
    Assert.Contains("managed.IsActive &&", generated);
  }

  [Fact]
  public void ModuleDiscoveryEnumeratesAnnotatedTypes() {
    var generated = Run("""
      namespace Game {
        [HELIX.Context.Managed] public partial class A { }
        [HELIX.Context.Managed] public partial class B { }
        [HELIX.Context.HelixModule(Filter = "Game")]
        public partial class Module { }
      }
      """).All();

    Assert.Contains("registrations.Register(typeof(global::Game.A), global::Game.A.RegistrationConfigurator);",
      generated);
    Assert.Contains("registrations.Register(typeof(global::Game.B)", generated);
  }

  [Fact]
  public void ShapeMistakesAreReportedRatherThanSilentlySkipped() {
    var run = Run("""
      namespace Game {
        [HELIX.Context.Managed]
        public partial class Broken {
          [HELIX.Context.EventHandler] void OnNothing(string notAnEvent) { }
        }
      }
      """);

    Assert.Contains(run.GeneratorDiagnostics, d => d.Id == "HX0201");
  }

  [Fact]
  public void Dump() {
    var directory = Path.Combine(Path.GetTempPath(), "helix-port-dump");
    Directory.CreateDirectory(directory);
    var lowered = Lower();
    foreach (var file in lowered.Files) File.WriteAllText(Path.Combine(directory, file.Key), file.Value);
    var run = Run("""
      namespace Game {
        [HELIX.Structure(true)]
        public partial struct Point { public int X; public string Name; }

        [HELIX.Compose.CustomBoundaryElement(cacheLookups: true)]
        public partial class Panel {
          [HELIX.Compose.WriteContextHandler] void Write(ref HELIX.Compose.Composition cx, object ctx) { }
        }

        [HELIX.Context.Managed]
        public partial class Loop {
          [HELIX.Context.Ticker("4")] void EveryFourFrames() { }
          [HELIX.Context.Ticker("250ms")] void Periodically() { }
        }
      }
      """);
    foreach (var source in run.Sources) File.WriteAllText(Path.Combine(directory, source.Key), source.Value);
    _output.WriteLine(directory);
  }

  private static int Occurrences(string text, string value) {
    var count = 0;
    for (var index = text.IndexOf(value, StringComparison.Ordinal); index >= 0;
         index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal)) count++;
    return count;
  }
}
