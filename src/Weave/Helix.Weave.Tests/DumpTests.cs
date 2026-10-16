using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Helix.Weave.Tests;

/// <summary>Not assertions — a way to eyeball what the pipeline produces while developing.</summary>
public class DumpTests {
  private readonly ITestOutputHelper _output;
  public DumpTests(ITestOutputHelper output) => _output = output;

  [Fact]
  public void DumpLoweredAndGenerated() {
    var lowered = Harness.LowerFixtures();
    var directory = Path.Combine(Path.GetTempPath(), "weave-dump");
    Directory.CreateDirectory(directory);
    foreach (var file in lowered.Files) File.WriteAllText(Path.Combine(directory, file.Key), file.Value);

    var run = Harness.RunFixtures("""
      using Fixtures;
      namespace Game {
        public partial class Player {
          [EventHandler(5)] void OnDamage(Evt evt) { }
          [EventHandler] void OnHeal(ref Evt evt) { }
          [Command("spawn", Description = "spawns \"a\" thing")] static void Spawn() { }
        }

        [Structure]
        public partial struct Settings {
          public int Health;
          [Prop("\"anon\"")] public string Name;
        }
      }
      """);
    foreach (var source in run.Sources) File.WriteAllText(Path.Combine(directory, source.Key), source.Value);
    _output.WriteLine(directory);
  }
}
