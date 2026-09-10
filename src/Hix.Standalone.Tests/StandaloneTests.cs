using System;
using System.IO;
using System.Collections.Generic;
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
  private sealed class CustomBackend : HixBackend {
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) { base.RegisterFunctions(functions); functions.Add(new CustomFunction()); }
    protected override void RegisterRoots(IDictionary<string,HixBackendRoot> roots) => roots.Add("environment", new("environment", HixValueKind.Number));
    public override IHixValue ResolveRoot(HixThread context, string name) => new NumberHixValue(7);
  }
  private sealed class CustomFunction() : FunctionDefinition("custom", new[] {new Hix.FunctionSignature(HixValueKind.Any, new[] {HixValueKind.Function})}) {
    public override IHixValue Execute(HixThread context, IHixValue[] args, int line) => context.Invoke(args[0], Array.Empty<IHixValue>(), line);
  }
}
