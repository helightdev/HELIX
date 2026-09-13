using System;
using System.IO;
using Hix.Compiler;
using Hix.Runtime;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Hix.Standalone.Tests;

public class HixImageCommandTests : IDisposable {
  private readonly string directory = Path.Combine(Path.GetTempPath(), "hix-image-" + Guid.NewGuid().ToString("N"));
  public HixImageCommandTests() => Directory.CreateDirectory(directory);
  public void Dispose() => Directory.Delete(directory, true);
  private string FilePath(string name) => Path.Combine(directory, name);
  private string Run(params string[] args) {
    var output = new StringWriter(); var error = new StringWriter();
    Assert.True(Program.Run(args, output, error) == 0, error.ToString()); return output.ToString();
  }
  [Fact]
  public void CommandsCompileRunDisassembleAndDecompileWithoutOriginalSources() {
    var source = FilePath("entry.hix"); var library = FilePath("library.hix"); var image = FilePath("entry.hixbc");
    File.WriteAllText(library, "pure func twice(number value) -> number => plus($value, $value)");
    File.WriteAllText(source, "%import<library.hix>\n---\nfunc main { print(twice(21)); return(missing) } func alternate { print(param) }");
    Run("compile", source, "-o", image);
    var original = File.ReadAllText(image);
    Run("compile", source, "--output", image);
    Assert.Equal(original, File.ReadAllText(image));
    File.Delete(source); File.Delete(library);
    Assert.Equal("42" + Environment.NewLine, Run("run", image));
    Assert.Equal("007" + Environment.NewLine, Run("run", image, "--entry", "alternate", "--", "007"));
    Assert.Contains("LoadMissing", Run("disassemble", image), StringComparison.OrdinalIgnoreCase);
    var assembly = FilePath("image.txt"); Run("disassemble", image, "-o", assembly);
    Assert.Equal(Run("disassemble", image), File.ReadAllText(assembly));
    var decompiled = FilePath("recovered.hix"); Run("decompile", image, "--output", decompiled);
    Assert.Equal(Run("decompile", image), File.ReadAllText(decompiled));
    var recovered = FilePath("recovered.hixbc"); Run("compile", decompiled, "-o", recovered);
    Assert.Equal("42" + Environment.NewLine, Run("run", recovered));
  }
  [Fact]
  public void ProjectMainIsStoredAndCanBeOverridden() {
    var manifest = FilePath("manifest.hal");
    File.WriteAllText(manifest, "--- Manifest\nname = image-test\napplicationMain = alternate\nsources = { main = [\"entry.hix\"] }\n");
    File.WriteAllText(FilePath("entry.hix"), "func main { print(<main>) } func alternate { print(<alternate>) }");
    var image = FilePath("project.hixbc"); Run("compile", directory, "-o", image);
    Assert.Equal("alternate" + Environment.NewLine, Run("run", image));
    Run("compile", directory, "-o", image, "--entry", "main");
    Assert.Equal("main" + Environment.NewLine, Run("run", image));
  }
  [Fact]
  public void ImageStoragePreservesLocalScopesDerivationsParametersAndMarkers() {
    var backend = new HixStandaloneBackend(new StringWriter(), directory);
    const string source = """
      derivation mixin Named { expression { local value = 1 } }
      pure func helper(number value) -> number { return($value) }
      mixin Example(string value = [<default>]) {
        pure func localHelper(number value) -> number { return(helper($value)) }
        prelude expression { carry local saved = <prelude> }
        expression { %marker(<entry>) local result = localHelper(42); local other = [$value] }
      }
      """;
    var original = HixCompiler.Compile(source, "Example", backend);
    var image = FilePath("scopes.hixbc"); HixImageFile.Write(image, original);
    var loaded = HixImageFile.Read(image, backend, out _);
    HELIX.SourceGen.Tests.HixRoundTripAssertions.Equal(original, loaded);
    Assert.Equal(original.Bytecode, loaded.Bytecode);
    Assert.Equal(original.SourceLines, loaded.SourceLines);
    Assert.Equal(original.Markers, loaded.Markers);
    Assert.Single(loaded.Derivations);
    Assert.Single(loaded.Parameters);
  }
  [Fact]
  public void CompilationDoesNotExecuteOrOverwriteAnImageOnFailure() {
    var source = FilePath("source.hix"); var image = FilePath("output.hixbc");
    File.WriteAllText(source, "func main { print(<must not execute>) }");
    Assert.Equal("", Run("compile", source, "-o", image));
    var original = File.ReadAllText(image);
    File.WriteAllText(source, "func main { @ }");
    Assert.Equal(1, Program.Run(["compile", source, "-o", image], new StringWriter(), new StringWriter()));
    Assert.Equal(original, File.ReadAllText(image));
  }
  [Theory]
  [InlineData("compile")]
  [InlineData("run")]
  [InlineData("disassemble")]
  [InlineData("decompile")]
  public void CommandsRejectMissingInputsAndOptions(string command) {
    Assert.Equal(2, Program.Run([command], new StringWriter(), new StringWriter()));
    Assert.Equal(2, Program.Run([command, "file", "--bogus"], new StringWriter(), new StringWriter()));
    Assert.Equal(2, Program.Run([command, "file", "--output"], new StringWriter(), new StringWriter()));
  }
  [Fact]
  public void MalformedImagesAndRuntimeFailuresReturnErrors() {
    var image = FilePath("bad.hixbc"); File.WriteAllText(image, "{}");
    var error = new StringWriter();
    Assert.Equal(1, Program.Run(["run", image], new StringWriter(), error)); Assert.Contains("format", error.ToString());
    File.WriteAllText(FilePath("error.hix"), "func main { return(cast(<failure>, error)) }");
    Run("compile", FilePath("error.hix"), "-o", image);
    error = new StringWriter(); Assert.Equal(1, Program.Run(["run", image], new StringWriter(), error));
    Assert.Contains("failure", error.ToString());
    var json = JObject.Parse(File.ReadAllText(image)); json["Bytecode"] = Convert.ToBase64String([255]);
    File.WriteAllText(image, json.ToString());
    foreach (var command in new[] {"run", "disassemble", "decompile"}) {
      error = new StringWriter(); Assert.Equal(1, Program.Run([command, image], new StringWriter(), error));
      Assert.Contains("offset", error.ToString(), StringComparison.OrdinalIgnoreCase);
    }
  }
}
