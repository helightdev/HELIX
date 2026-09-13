using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Compiler;
using Hix.Standalone.Analysis;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class OpaqueConcurrencyTypeTests {
  [Fact]
  public void MetadataGenericsAndNestedConstructorsAgreeWithAnalyzer() {
    var backend = new HixStandaloneBackend();
    var source = """
      %generic<T>
      func result(%task<T> task) -> T { return(awaitTask($task)) }
      type Nested = %many(many<string>);
      func main {
        local number = result(spawn(func => 42))
        local Nested nested = @[@[<hello>]]
      }
      """;
    var analysis = new LanguageAnalysis(source, backend);
    Assert.Empty(analysis.Program.Diagnostics);
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local number: number");
    var declaration = analysis.Program.Declarations.OfType<FunctionDeclarationIr>().First();
    Assert.Single(declaration.TypeParameters);
    Assert.IsType<TypeParameterHixPattern>(declaration.Signatures[0].OutputPattern);
    Assert.Contains(analysis.Declarations, symbol => symbol.Kind == "Pattern" && symbol.Name == "T");
    Assert.Contains(analysis.TypeFacts, fact => fact.Documentation == "generic T (unbounded)");
    Assert.DoesNotContain(analysis.References, reference => reference.Kind == "Function" && reference.Name == "many");
    HixCompiler.CompileFunctions([source], backend);
  }

  [Fact]
  public void AnalyzerAndCompilerAgreeOnInstantiatedConcurrencyTypes() {
    var backend = new HixStandaloneBackend();
    var source = """
      func main {
        local task = spawn(func => plus($0, 1), @[41])
        local result = awaitTask(local#task)
        local other = spawn(func => uppercase($0), @[<hello>])
        local joined = awaitAll(@[local#task, local#other])
        local first = awaitAny(@[local#task, local#other])
        local channel = channel()
        local semaphore = semaphore()
      }
      """;
    var analysis = new LanguageAnalysis(source, backend);
    Assert.Empty(analysis.Program.Diagnostics);
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local task: task(number)");
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local result: number");
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Type == "[]");
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Type == "{}");
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local channel: channel(any)");
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local semaphore: semaphore");
    HixCompiler.CompileFunctions([source], backend);
  }

  [Fact]
  public void ExplicitIrChannelArgumentsConstrainSendAndReceive() {
    var backend = new HixStandaloneBackend();
    var patterns = new Dictionary<string, HixPattern>();
    var functions = new Dictionary<string, FunctionDeclarationIr[]>();
    var number = new KindHixPattern(HixValueKind.Number);
    var created = new CallExpressionIr("channel", []) { TypeArguments = [number] };
    var channel = HixCallResolver.Resolve(created, functions, backend, patterns, _ => HixPattern.Any, out _);
    Assert.Equal("channel(number)", channel.Signature.Result.Display);
    var handle = new RootExpressionIr("handle") { InferredPattern = channel.Signature.Result };
    var received = HixCallResolver.Resolve(new("receive", [handle]), functions, backend, patterns,
      expression => expression.InferredPattern, out _);
    Assert.Equal("number", received.Signature.Result.Display);
    var sent = new CallExpressionIr("send", [handle, new StringExpressionIr("text")]);
    Assert.Null(HixCallResolver.Resolve(sent, functions, backend, patterns, expression =>
      expression == handle ? handle.InferredPattern : new KindHixPattern(HixValueKind.String), out _));
  }

  [Fact]
  public void CatalogExposesNominalTypesAndGenericSignatures() {
    var service = new HixAnalyzerService(new HixStandaloneBackend());
    var definitions = service.QueryDefinitions("", "", "", "");
    Assert.Contains(definitions, definition => definition.Name == "task" && definition.Kind == "Type");
    Assert.Contains(definitions, definition => definition.Name == "task" && definition.Kind == "PatternMetadata" &&
      definition.ArgumentTypes.SequenceEqual(["pattern"]));
    Assert.Contains(definitions, definition => definition.Name == "generic" && definition.Kind == "DeclarationMetadata");
    Assert.Contains(definitions, definition => definition.Name == "awaitTask" && definition.ResultType == "T" &&
      definition.ArgumentTypes.SequenceEqual(["task(T)"]));
    Assert.DoesNotContain(definitions, definition => definition.Name == "symbol" && definition.Kind == "Kind");
  }
}
