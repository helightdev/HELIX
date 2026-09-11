using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Hix.Standalone;
using Newtonsoft.Json.Linq;
using Xunit;
namespace Hix.Standalone.Tests;
public class StandaloneTests {
  private static HixInvocationResult Run(string source, HixBackend backend = null, string entry = "main", params IHixValue[] args) {
    backend ??= new HixStandaloneBackend(new StringWriter());
    var program = HixCompiler.CompileFunctions(new[] {source}, backend);
    return new HixVM(new[] {program}).Invoke(program, backend.CreateThread(), entry, args);
  }
  [Fact] public void MainReturnsValue() {
    var result = Run("func main { return(42) }");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Value).Value);
  }
  [Fact] public void NamedEntrypointUsesNormalPacking() {
    var first = new LiteralHixValue(HixString.Dynamic("a"));
    var second = new LiteralHixValue(HixString.Dynamic("b"));
    var result = Run("func other { return(param) }", entry: "other", args: new IHixValue[] {first, second});
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(2, Assert.IsType<TupleHixValue>(result.Value).Values.Count);
    Assert.IsType<NullHixValue>(Run("func main { return(param) }").Value);
    Assert.Same(first, Run("func main { return(param) }", args: new IHixValue[] {first}).Value);
  }
  [Fact] public void OutputDoesNotRequirePrelude() {
    var output = new StringWriter();
    var result = Run("func main { print(<hello>); return(1) }", new HixStandaloneBackend(output));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("hello" + Environment.NewLine, output.ToString());
  }
  [Fact] public void BackendDefinitionsAreIsolatedAndCallbacksWork() {
    var result = Run("pure func answer { return(12) } func main { return(custom(answer)) }", new CustomBackend());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(12, Assert.IsType<NumberHixValue>(result.Value).Value);
    Assert.Throws<ArgumentException>(() => Run("func main { custom(main) }"));
    Assert.Throws<ArgumentException>(() => Run("func main { collectAnnotatedTypes(<Test>) }"));
  }
  [Fact] public void PureFunctionsCannotPrint() {
    Assert.Throws<ArgumentException>(() => Run("pure func main { print(<hello>) }"));
  }
  [Fact] public void FailuresAreReported() {
    Assert.False(Run("func other { return } ").Success);
    Assert.False(Run("func main { error(<failed>) }").Success);
  }
  [Fact] public void PackedArgumentRemainsOneArgument() {
    var tuple = new TupleHixValue(new IHixValue[] {new NumberHixValue(1), new NumberHixValue(2)});
    var result = Run("func main { return(length(args)) }", args: new IHixValue[] {tuple});
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(1, Assert.IsType<NumberHixValue>(result.Value).Value);
  }
  [Fact] public void UnknownHostRootsAreCompilationErrors() {
    Assert.Throws<ArgumentException>(() => Run("func main { return(this) }"));
  }
  [Fact] public void CliPassesStringsAndReportsErrors() {
    var file = Path.GetTempFileName();
    try {
      File.WriteAllText(file, "func main { print(param) } func alternate { print(<alternate>) }");
      var output = new StringWriter(); var error = new StringWriter();
      Assert.Equal(0, Program.Run(new[] {file, "--", "007"}, output, error));
      Assert.Equal("007" + Environment.NewLine, output.ToString());
      output.GetStringBuilder().Clear();
      Assert.Equal(0, Program.Run(new[] {file, "--entry", "alternate"}, output, error));
      Assert.Equal("alternate" + Environment.NewLine, output.ToString());
      Assert.Equal(1, Program.Run(new[] {file, "--entry", "missing"}, output, error));
      Assert.Equal(2, Program.Run(new[] {file, "--entry"}, output, error));
      File.WriteAllText(file, "func main { return(error(<failure>)) }");
      Assert.Equal(1, Program.Run(new[] {file}, output, error));
      Assert.Contains("failure", error.ToString());
    } finally { File.Delete(file); }
  }
  [Theory]
  [InlineData("*")]
  [InlineData("**")]
  [InlineData("src/**")]
  public void CliImportsGlobbedHixFiles(string pattern) {
    var directory = Path.Combine(Path.GetTempPath(), "hix-import-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    try {
      var importedDirectory = pattern == "src/**" || pattern == "**" ? Path.Combine(directory, "src", "nested") : directory;
      Directory.CreateDirectory(importedDirectory);
      File.WriteAllText(Path.Combine(importedDirectory, "library.hix"), "func answer { return(42) }");
      var entry = Path.Combine(directory, "main.hix");
      File.WriteAllText(entry, "%import<" + pattern + ">\n---\nfunc main { print(answer()) }");
      var output = new StringWriter(); var error = new StringWriter();
      Assert.Equal(0, Program.Run(new[] {entry}, output, error));
      Assert.Equal("42" + Environment.NewLine, output.ToString());
    } finally { Directory.Delete(directory, true); }
  }
  [Fact] public void BackendCanRegisterRootsWithoutVmChanges() {
    var result = Run("func main { return(environment) }", new CustomBackend());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(7, Assert.IsType<NumberHixValue>(result.Value).Value);
    Assert.Throws<ArgumentException>(() => Run("pure func main { return(environment) }", new CustomBackend()));
  }
  [Fact] public void JsonRoundTripsHixValuesAndSupportsKinds() {
    var json = new LiteralHixValue(HixString.Dynamic("{\"name\":\"Ada\",\"active\":true,\"scores\":[1,2.5],\"missing\":null}"));
    var parsed = Run("func main { return(parseJson(param, table)) }", args: new IHixValue[] {json});
    Assert.True(parsed.Success, parsed.Error.Resolve(parsed.Strings));
    var table = Assert.IsType<HixTableValue>(parsed.Value);
    Assert.Equal("Ada", Assert.IsType<LiteralHixValue>(table.Select(new HixThread(new HixStandaloneBackend().CreateContext()), HixString.Dynamic("name"))).Value.Resolve(null));

    var written = Run("func main { return(writeJson(param, table)) }", args: new[] {parsed.Value});
    Assert.True(written.Success, written.Error.Resolve(written.Strings));
    Assert.True(JToken.DeepEquals(JToken.Parse(json.Value.Resolve(null)),
      JToken.Parse(Assert.IsType<LiteralHixValue>(written.Value).Value.Resolve(written.Strings))));
  }
  [Fact] public void JsonPatternOverloadsEnforceStructureConstantsAndConstraints() {
    const string source = "type Person = @{string name, %min<1> number age, %const<admin> string role, %optional string nickname}\n" +
      "func main { return(parseJson(param, Person)) }";
    var valid = Run(source, args: new IHixValue[] {new LiteralHixValue(HixString.Dynamic("{\"name\":\"Ada\",\"age\":37,\"role\":\"admin\"}"))});
    Assert.True(valid.Success, valid.Error.Resolve(valid.Strings));
    var invalid = Run(source, args: new IHixValue[] {new LiteralHixValue(HixString.Dynamic("{\"name\":\"Ada\",\"age\":0,\"role\":\"admin\"}"))});
    Assert.False(invalid.Success);
    Assert.Contains("constraint failed", invalid.Error.Resolve(invalid.Strings));

    var rejectedWrite = Run("type Positive = %min<1> number\nfunc main { return(writeJson(param, Positive)) }",
      args: new IHixValue[] {new NumberHixValue(0)});
    Assert.False(rejectedWrite.Success);
  }
  [Fact] public void TaggedTypesInsertAndEnforceDiscriminators() {
    const string tagged = "%tagged\ntype Person = @{string name}\nfunc main { return(Person(param)) }";
    var constructed = Run(tagged, args: new IHixValue[] {new HixTableValue([
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic("name"), HixThread.String("Ada"))
    ])});
    Assert.True(constructed.Success, constructed.Error.Resolve(constructed.Strings));
    var table = Assert.IsType<HixTableValue>(constructed.Value);
    Assert.Equal("Person", Assert.IsType<LiteralHixValue>(table.Entries.Single(entry =>
      entry.Key.Resolve(constructed.Strings) == "$type").Value).Value.Resolve(constructed.Strings));

    var matching = Run(tagged, args: new IHixValue[] {new HixTableValue([
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic("name"), HixThread.String("Ada")),
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic("$type"), HixThread.String("Person"))
    ])});
    Assert.True(matching.Success, matching.Error.Resolve(matching.Strings));
    var conflicting = Run(tagged, args: new IHixValue[] {new HixTableValue([
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic("name"), HixThread.String("Ada")),
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic("$type"), HixThread.String("Other"))
    ])});
    Assert.False(conflicting.Success);
    Assert.Contains("constant mismatch", conflicting.Error.Resolve(conflicting.Strings));

    const string custom = "%tagged<person-record>\ntype Person = @{string name}\n";
    var parsed = Run(custom + "func main { return(parseJson(param, Person)) }", args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic("{\"name\":\"Ada\"}"))
    });
    Assert.True(parsed.Success, parsed.Error.Resolve(parsed.Strings));
    var generated = Run(custom + "func main { return(generateJsonSchema(Person)) }");
    Assert.True(generated.Success, generated.Error.Resolve(generated.Strings));
    var definition = JObject.Parse(ReadText(generated))["$defs"]?["Person"];
    Assert.Equal("person-record", (string)definition?["properties"]?["$type"]?["const"]);
    Assert.Contains("$type", definition?["required"]?.Values<string>() ?? []);
    var imported = Run("func main { return(parseJson($1, loadJsonSchema($0))) }", args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic(ReadText(generated))),
      new LiteralHixValue(HixString.Dynamic("{\"name\":\"Ada\"}"))
    });
    Assert.True(imported.Success, imported.Error.Resolve(imported.Strings));
    Assert.Contains(Assert.IsType<HixTableValue>(imported.Value).Entries,
      entry => entry.Key.Resolve(imported.Strings) == "$type" && imported.Value is HixTableValue);
  }
  [Fact] public void TaggedUnionDeserializationSelectsTheDiscriminatedPattern() {
    const string source = """
      %tagged<cat>
      type Cat = @{string name, number lives = [9]}
      %tagged<dog>
      type Dog = @{string name, bool good = [true]}
      type Animal = %union<Cat><Dog>
      func main { return(parseJson(param, Animal)) }
      """;
    var parsed = Run(source, args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic("{\"$type\":\"dog\",\"name\":\"Mochi\"}"))
    });
    Assert.True(parsed.Success, parsed.Error.Resolve(parsed.Strings));
    var dog = Assert.IsType<HixTableValue>(parsed.Value);
    Assert.True(Assert.IsType<BooleanHixValue>(dog.Entries.Single(entry =>
      entry.Key.Resolve(parsed.Strings) == "good").Value).Value);
    Assert.DoesNotContain(dog.Entries, entry => entry.Key.Resolve(parsed.Strings) == "lives");

    var unknown = Run(source, args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic("{\"$type\":\"bird\",\"name\":\"Mochi\"}"))
    });
    Assert.False(unknown.Success);
  }
  [Fact] public void GraphMetadataIsPreservedOnPatternFieldsAndSchemas() {
    const string declaration = "type Node = @{%graph<value> string input, %graph<flow> %many<string> children}\n";
    var unit = AntlrSyntax.Parse(declaration);
    Assert.Empty(unit.Diagnostics);
    var fields = Assert.IsType<TableHixPattern>(Assert.Single(unit.Declarations.OfType<TypeDeclarationIr>()).Pattern).Fields;
    Assert.Equal(HixGraphFieldKind.Value, fields.Single(field => field.Name == "input").Graph);
    Assert.Equal(HixGraphFieldKind.Flow, fields.Single(field => field.Name == "children").Graph);

    var generated = Run(declaration + "func main { return(generateJsonSchema(Node)) }");
    Assert.True(generated.Success, generated.Error.Resolve(generated.Strings));
    var properties = JObject.Parse(ReadText(generated))["$defs"]?["Node"]?["properties"];
    Assert.Equal("value", (string)properties?["input"]?["x-hix-graph"]);
    Assert.Equal("flow", (string)properties?["children"]?["x-hix-graph"]);

    var invalid = AntlrSyntax.Parse("type Node = @{%graph<sideways> string input}");
    Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Message.Contains("'value' or 'flow'"));
  }
  [Fact] public void TitleAndDescriptionDocumentTypesFunctionsAndMixins() {
    const string source = """
      %title<Person record>
      %description<A documented person type.>
      type Person = @{string name}
      %title<Find person>
      %description<Finds one person.>
      func find(Person value) -> Person { return($value) }
      %title<Person processing>
      %description<Processes person records.>
      mixin People { expression { emit(null) } }
      """;
    var analysis = new LanguageAnalysis(source, new HixStandaloneBackend());
    Assert.Empty(analysis.Program.Diagnostics);
    Assert.Contains(analysis.TypeFacts, fact => fact.Documentation.Contains("Person record") &&
      fact.Documentation.Contains("A documented person type."));
    Assert.Contains(analysis.TypeFacts, fact => fact.Documentation.Contains("Find person") &&
      fact.Documentation.Contains("Finds one person."));
    Assert.Contains(analysis.TypeFacts, fact => fact.Documentation.Contains("Person processing") &&
      fact.Documentation.Contains("Processes person records."));

    var generated = Run(source + "\nfunc schema { return(generateJsonSchema(Person)) }", entry: "schema");
    Assert.True(generated.Success, generated.Error.Resolve(generated.Strings));
    var person = JObject.Parse(ReadText(generated))["$defs"]?["Person"];
    Assert.Equal("Person record", (string)person?["title"]);
    Assert.Equal("A documented person type.", (string)person?["description"]);
  }
  [Fact] public void JsonSchemasRoundTripPatternsIncludingNamedReferences() {
    var generated = Run("type Person = @{string name, %matches<^a+$> string code}\n" +
      "func main { return(generateJsonSchema(Person)) }");
    Assert.True(generated.Success, generated.Error.Resolve(generated.Strings));
    var schema = Assert.IsType<LiteralHixValue>(generated.Value).Value.Resolve(generated.Strings);
    var document = JObject.Parse(schema);
    Assert.Equal("https://json-schema.org/draft/2020-12/schema", (string)document["$schema"]);
    Assert.NotNull(document["$defs"]?["Person"]);

    var source = "func main { return(parseJson($1, loadJsonSchema($0))) }";
    var accepted = Run(source, args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic(schema)),
      new LiteralHixValue(HixString.Dynamic("{\"name\":\"Ada\",\"code\":\"aaa\"}"))
    });
    Assert.True(accepted.Success, accepted.Error.Resolve(accepted.Strings));
    var rejected = Run(source, args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic(schema)),
      new LiteralHixValue(HixString.Dynamic("{\"name\":\"Ada\",\"code\":\"abc\"}"))
    });
    Assert.False(rejected.Success);
  }
  [Fact] public void JsonPatternsSupportExclusiveBoundsEnumsMetadataAndDefaults() {
    const string declaration = "type Settings = %title<Settings> %description<App settings> " +
      "@{%enum<draft><live> string mode = [<draft>], %min(1, true) number score, " +
      "%min<1> %max<3> %many<string> tags}\n";
    var parsed = Run(declaration + "func main { return(parseJson(param, Settings)) }", args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic("{\"score\":2,\"tags\":[\"a\",\"b\"]}"))
    });
    Assert.True(parsed.Success, parsed.Error.Resolve(parsed.Strings));
    var table = Assert.IsType<HixTableValue>(parsed.Value);
    Assert.Equal("draft", Assert.IsType<LiteralHixValue>(table.Entries.Single(item =>
      item.Key.Resolve(parsed.Strings) == "mode").Value).Value.Resolve(parsed.Strings));

    foreach (var invalid in new[] {
      "{\"mode\":\"other\",\"score\":2,\"tags\":[\"a\"]}",
      "{\"score\":1,\"tags\":[\"a\"]}",
      "{\"score\":2,\"tags\":[]}"
    }) Assert.False(Run(declaration + "func main { return(parseJson(param, Settings)) }", args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic(invalid))
    }).Success, invalid);

    var generated = Run(declaration + "func main { return(generateJsonSchema(Settings)) }");
    var schemaText = ReadText(generated); var schema = JObject.Parse(schemaText);
    var settings = (JObject)schema["$defs"]?["Settings"];
    Assert.Equal("Settings", (string)settings?["title"]);
    Assert.Equal("App settings", (string)settings?["description"]);
    Assert.Equal(1d, (double)settings?["properties"]?["score"]?["exclusiveMinimum"]);
    Assert.Equal(new[] {"draft", "live"}, settings?["properties"]?["mode"]?["enum"]?.Values<string>());
    Assert.Equal("draft", (string)settings?["properties"]?["mode"]?["default"]);
    Assert.Equal(1, (int)settings?["properties"]?["tags"]?["minItems"]);
    Assert.Equal(3, (int)settings?["properties"]?["tags"]?["maxItems"]);

    var imported = Run("func main { return(parseJson($1, loadJsonSchema($0))) }", args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic(schemaText)),
      new LiteralHixValue(HixString.Dynamic("{\"score\":2,\"tags\":[\"a\"]}"))
    });
    Assert.True(imported.Success, imported.Error.Resolve(imported.Strings));
    Assert.Contains(Assert.IsType<HixTableValue>(imported.Value).Entries,
      item => item.Key.Resolve(imported.Strings) == "mode" &&
        Assert.IsType<LiteralHixValue>(item.Value).Value.Resolve(imported.Strings) == "draft");
    var importedInvalid = Run("func main { return(parseJson($1, loadJsonSchema($0))) }", args: new IHixValue[] {
      new LiteralHixValue(HixString.Dynamic(schemaText)),
      new LiteralHixValue(HixString.Dynamic("{\"score\":2,\"tags\":[]}"))
    });
    Assert.False(importedInvalid.Success);
  }
  [Fact] public void StandalonePathFunctionsUseForwardSlashes() {
    var backend = new HixStandaloneBackend(new StringWriter(), Path.Combine(Path.GetTempPath(), "hix-path-root"));
    var joined = Run("func main { return(joinPath(<path>, <dir>, <..>, <file.txt>)) }", backend);
    Assert.True(joined.Success, joined.Error.Resolve(joined.Strings));
    Assert.Equal("path/file.txt", Assert.IsType<LiteralHixValue>(joined.Value).Value.Resolve(joined.Strings));
    var split = Run("func main { return(splitPath(<path/dir/file.txt>)) }", backend);
    Assert.Equal(new[] {"path", "dir", "file.txt"}, Assert.IsType<TupleHixValue>(split.Value).Values
      .Cast<LiteralHixValue>().Select(value => value.Value.Resolve(split.Strings)));
    Assert.Equal("path/dir", ReadText(Run("func main { return(pathDir(<path/dir/file.txt>)) }", backend)));
    Assert.Equal("path", ReadText(Run("func main { return(pathDir(<path/to/>)) }", backend)));
    Assert.Equal("to", ReadText(Run("func main { return(pathName(<path/to/>)) }", backend)));
    Assert.Equal("path/file", ReadText(Run("func main { return(trimExtension(<path/file.txt>)) }", backend)));
    Assert.Equal("/", ReadText(Run("func main { return(pathSeparator()) }", backend)));
    Assert.DoesNotContain('\\', ReadText(Run("func main { return(currentPath()) }", backend)));
  }
  [Fact] public void StandaloneFileFunctionsOperateRelativeToTheMixinDirectory() {
    var directory = Path.Combine(Path.GetTempPath(), "hix-io-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    try {
      var backend = new HixStandaloneBackend(new StringWriter(), directory);
      Assert.True(Assert.IsType<BooleanHixValue>(Run("func main { return(writeFile(path = <a.txt>, content = <hello>)) }", backend).Value).Value);
      Assert.True(Assert.IsType<BooleanHixValue>(Run("func main { return(writeFile(<a.txt>, <!>, append = true)) }", backend).Value).Value);
      Assert.Equal("hello!", ReadText(Run("func main { return(readFile(path = <a.txt>)) }", backend)));
      Assert.Equal("ell", ReadText(Run("func main { return(readFile(path = <a.txt>, offset = 1, length = 3)) }", backend)));
      Assert.Equal(6, Assert.IsType<NumberHixValue>(Run("func main { return(readFileLength(<a.txt>)) }", backend).Value).Value);
      Assert.True(Assert.IsType<BooleanHixValue>(Run("func main { return(copyFile(<a.txt>, <b.txt>)) }", backend).Value).Value);
      Assert.True(Assert.IsType<BooleanHixValue>(Run("func main { return(renameFile(<b.txt>, <c.txt>)) }", backend).Value).Value);
      Assert.True(Assert.IsType<BooleanHixValue>(Run("func main { return(existsFile(<c.txt>)) }", backend).Value).Value);
      var listed = Assert.IsType<TupleHixValue>(Run("func main { return(listFiles(<.>)) }", backend).Value);
      Assert.Equal(2, listed.Values.Count);
      Assert.True(Assert.IsType<BooleanHixValue>(Run("func main { return(deleteFile(<c.txt>)) }", backend).Value).Value);
    } finally { Directory.Delete(directory, true); }
  }
  private static string ReadText(HixInvocationResult result) {
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    return Assert.IsType<LiteralHixValue>(result.Value).Value.Resolve(result.Strings);
  }
  private sealed class CustomBackend : HixBackend {
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) { base.RegisterFunctions(functions); functions.Add(new CustomFunction()); }
    protected override void RegisterRoots(IDictionary<string,HixBackendRoot> roots) => roots.Add("environment", new("environment", HixValueKind.Number));
    public override IHixValue ResolveRoot(HixThread context, string name) => new NumberHixValue(7);
  }
  private sealed class CustomFunction() : FunctionDefinition("custom", new[] {new Hix.FunctionSignature(HixValueKind.Any, new[] {HixValueKind.Function})}) {
    public override IHixValue Execute(HixThread context, IHixValue[] args, int line) => context.Invoke(args[0], Array.Empty<IHixValue>(), line);
  }
}
