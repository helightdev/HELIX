using System;
using System.IO;
using System.Linq;
using Hix.Standalone;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class HixProjectTests {
  [Fact] public void ManifestDefaultsAndMetadataAreParsed() => InProject(directory => {
    Write(directory, "manifest.hal", "--- Manifest\nname = demo\nversion = 2.3.4.5-beta.7\ndescription = Example\napplicationMain = start\n");
    var manifest = HixManifest.Load(directory);
    Assert.Equal("demo", manifest.Name); Assert.Equal("2.3.4.5-beta.7", manifest.Version.ToString());
    Assert.Equal("start", manifest.ApplicationMain); Assert.Equal(["src/**"], manifest.Sources.Main);
    Assert.Equal(["test/**"], manifest.Sources.Test); Assert.Equal(["modules/**"], manifest.Sources.Modules);
  });

  [Fact] public void ManifestRequiresNameAndRejectsUnknownFields() => InProject(directory => {
    Write(directory, "manifest.hal", "--- Manifest\ndescription = unnamed\n");
    Assert.Contains("name", Assert.Throws<ArgumentException>(() => HixManifest.Load(directory)).Message);
    Write(directory, "manifest.hal", "--- Manifest\nname = demo\nbackend = Test\n");
    Assert.Contains("unknown manifest field 'backend'", Assert.Throws<ArgumentException>(() => HixManifest.Load(directory)).Message);
  });

  [Fact] public void EmptyDefaultPathsAreValid() => InProject(directory => {
    Write(directory, "manifest.hal", "--- Manifest\nname = empty\n");
    var project = HixProject.Load(directory); Assert.Empty(project.MainSources()); Assert.Empty(project.TestSources());
  });

  [Fact] public void ModulesAreRecursiveAndOnlyDependenciesCompile() => InProject(directory => {
    Write(directory, "manifest.hal", "--- Manifest\nname = root\ndependencies = { \"root/tools/leaf\" = ModuleDependency {} }\n");
    Write(directory, "src/main.hix", "func main { print(answer()) }");
    Write(directory, "modules/tools/manifest.hal", "--- Manifest\nname = tools\n");
    Write(directory, "modules/tools/modules/leaf/manifest.hal", "--- Manifest\nname = leaf\n");
    Write(directory, "modules/tools/modules/leaf/src/answer.hix", "func answer() -> any { return(42) }");
    Write(directory, "modules/unused/manifest.hal", "--- Manifest\nname = unused\n");
    Write(directory, "modules/unused/src/broken.hix", "func broken { @ }");
    var project = HixProject.Load(directory);
    Assert.Contains("root/tools/leaf", project.Modules.Keys); Assert.DoesNotContain("root/leaf", project.Modules.Keys);
    Assert.DoesNotContain(project.MainSources(), x => x.Path.Contains("unused"));
    var output = new StringWriter(); Assert.Equal(0, Program.Run([directory], output, new StringWriter())); Assert.Equal("42" + Environment.NewLine, output.ToString());
  });

  [Fact] public void PathDependenciesAndRootOnlyTestsWork() => InProject(directory => {
    Write(directory, "manifest.hal", "--- Manifest\nname = app\ndependencies = { lib = PathDependency { uri = \"../library\", version = \"^1.2.0\" } }\n");
    Write(directory, "src/main.hix", "func main { return(answer()) }");
    Write(directory, "test/app.hix", "%backend<Test>\n---\n%test\nfunc appTest { assert(eq(answer(), 7)) }");
    var library = Path.Combine(directory, "..", "library");
    Write(library, "manifest.hal", "--- Manifest\nname = library\nversion = 1.4.0\n");
    Write(library, "src/library.hix", "func answer() -> any { return(7) }");
    Write(library, "test/library.hix", "%backend<Test>\n---\n%test\nfunc dependencyTest { assert(false) }");
    var result = HixTestRunner.Run([directory], new StringWriter()); Assert.True(result.Success); Assert.Single(result.Tests); Assert.Equal("appTest", result.Tests[0].Name);
  });

  [Theory]
  [InlineData("^1.2.3", "1.9.0", true)] [InlineData("^1.2.3", "2.0.0", false)]
  [InlineData("~1.2.3", "1.2.9", true)] [InlineData("1.2.x", "1.2.9", true)]
  [InlineData(">=1.0.0 <2.0.0", "2.0.0", false)] [InlineData("1.0.0 - 2.0.0", "2.0.0", true)]
  [InlineData("1.0.0 || 2.0.0", "2.0.0", true)]
  public void VersionConstraintsMatch(string constraint, string version, bool expected) =>
    Assert.Equal(expected, HixVersionConstraint.Parse(constraint).Matches(HixVersion.Parse(version)));

  private static void InProject(Action<string> action) {
    var root = Path.Combine(Path.GetTempPath(), "hix-project-" + Guid.NewGuid().ToString("N"));
    var directory = Path.Combine(root, "app"); Directory.CreateDirectory(directory);
    try { action(directory); } finally { Directory.Delete(root, true); }
  }
  private static void Write(string directory, string relative, string text) { var path = Path.Combine(directory, relative); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, text); }
}
