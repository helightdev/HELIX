using System;
using System.IO;
using System.Collections.Generic;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Hix.Standalone;
using Xunit;
namespace Hix.Standalone.Tests;
public class StandaloneTests {
  private static HixInvocationResult Run(string source, HixBackend backend = null, string entry = "main", params IHixValue[] args) {
    backend ??= new HixStandaloneBackend(new StringWriter());
    var program = HixCompiler.CompileFunctions(new[] {source}, backend);
    return new HixVM(new[] {program}).Invoke(program, backend.CreateContext(), entry, args);
  }
  [Fact] public void MainReturnsValue() {
    var result = Run("func main { return(42) }");
    Assert.True(result.Success, result.Error);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Value).Value);
  }
  [Fact] public void NamedEntrypointUsesNormalPacking() {
    var first = new LiteralHixValue(HixString.Dynamic("a"));
    var second = new LiteralHixValue(HixString.Dynamic("b"));
    var result = Run("func other { return(param) }", entry: "other", args: new IHixValue[] {first, second});
    Assert.True(result.Success, result.Error);
    Assert.Equal(2, Assert.IsType<TupleHixValue>(result.Value).Values.Count);
    Assert.IsType<NullHixValue>(Run("func main { return(param) }").Value);
    Assert.Same(first, Run("func main { return(param) }", args: new IHixValue[] {first}).Value);
  }
  [Fact] public void OutputDoesNotRequirePrelude() {
    var output = new StringWriter();
    var result = Run("func main { print(<hello>); return(1) }", new HixStandaloneBackend(output));
    Assert.True(result.Success, result.Error);
    Assert.Equal("hello" + Environment.NewLine, output.ToString());
  }
  [Fact] public void BackendDefinitionsAreIsolatedAndCallbacksWork() {
    var result = Run("pure func answer { return(12) } func main { return(custom(answer)) }", new CustomBackend());
    Assert.True(result.Success, result.Error);
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
    Assert.True(result.Success, result.Error);
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
    Assert.True(result.Success, result.Error);
    Assert.Equal(7, Assert.IsType<NumberHixValue>(result.Value).Value);
    Assert.Throws<ArgumentException>(() => Run("pure func main { return(environment) }", new CustomBackend()));
  }
  private sealed class CustomBackend : HixBackend {
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) { base.RegisterFunctions(functions); functions.Add(new CustomFunction()); }
    protected override void RegisterRoots(IDictionary<string,HixBackendRoot> roots) => roots.Add("environment", new("environment", HixValueKind.Number));
    public override IHixValue ResolveRoot(HixExecutionContext context, string name) => new NumberHixValue(7);
  }
  private sealed class CustomFunction() : FunctionDefinition("custom", new[] {new Hix.FunctionSignature(HixValueKind.Any, new[] {HixValueKind.Function})}) {
    public override IHixValue Execute(HixExecutionContext context, IHixValue[] args, int line) => context.Invoke(args[0], Array.Empty<IHixValue>(), line);
  }
}
