using System;
using System.Linq;
using System.Threading;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Hix.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
namespace Hix.Roslyn.Tests;
public class RoslynBackendTests {
  [Fact] public void RoslynContextRejectsPoolSubmissionButSupportsCurrentThreadQueue() {
    var compilation = CSharpCompilation.Create("queue", [],
      [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
    var backend = new HixRoslynBackend();
    var program = HixCompiler.CompileFunctions(["func main { return(42) }"], backend);
    var context = backend.CreateContext(compilation);
    using var vm = new HixVM([program], workerCount: 1);
    Action submitToPool = () => { _ = vm.InvokeAsync(program, context, useThreadPool: true); };
    Assert.Throws<InvalidOperationException>(submitToPool);
    var completion = vm.InvokeAsync(program, context);
    Assert.False(completion.IsCompleted);
    vm.CurrentThread.RunUntilIdle();
    Assert.True(completion.Result.Success);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(completion.Result.Value).Value);
  }
  [Fact] public void SemanticBackendWorksWithoutGeneratorAssembly() {
    var compilation = CSharpCompilation.Create("test", new[] {CSharpSyntaxTree.ParseText("class MarkerAttribute : System.Attribute {} [Marker] class Example {}")}, new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)}, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    var backend = new HixRoslynBackend();
    var program = HixCompiler.Compile("mixin Test { prelude expression { emit(length(collectAnnotatedTypes(<MarkerAttribute>))) } }", "Test", backend);
    var result = new HixVM(new[] {program}).Run(program, new HixCoroutine(backend.CreateContext(compilation)));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(1, Assert.IsType<NumberHixValue>(result.Outputs.Single().Value).Value);
    Assert.DoesNotContain(typeof(HixVM).Assembly.GetReferencedAssemblies(), assembly => assembly.Name.Contains("CodeAnalysis") || assembly.Name.Contains("Generator"));
    Assert.DoesNotContain(typeof(HixRoslynBackend).Assembly.GetReferencedAssemblies(), assembly => assembly.Name.Contains("Generator"));
  }

  [Fact]
  public void ExternalRoslynBackendCanConstructValuesAndExtendThreads() {
    var compilation = CSharpCompilation.Create("extension", [CSharpSyntaxTree.ParseText("class Example {}")],
      [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
    var type = compilation.GetTypeByMetadataName("Example");
    var backend = new ExtensionBackend(compilation, type);
    var program = HixCompiler.CompileFunctions(["func read { return(customName()) } func main { return(read()) }"], backend);
    var thread = backend.CreateCoroutine();
    var result = new HixVM([program]).Invoke(program, thread);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("Example", result.Value.Unlink(thread));
    Assert.Same(type, Assert.IsType<ExtensionContext>(thread.Context).CurrentType);
  }

  private sealed class ExtensionContext(HixBackend backend, CSharpCompilation compilation, INamedTypeSymbol type)
    : HixRoslynContext(type, type, null, compilation, backend: backend);

  private sealed class ExtensionBackend(CSharpCompilation compilation, INamedTypeSymbol type) : HixRoslynBackend {
    private readonly int callerThread = Environment.CurrentManagedThreadId;
    private readonly SynchronizationContext callerContext = SynchronizationContext.Current;
    public override HixContext CreateContext() => new ExtensionContext(this, compilation, type);
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.Add(new SimpleFunction("customName", [new(HixValueKind.String, [])], (thread, _) => {
        Assert.Equal(callerThread, Environment.CurrentManagedThreadId);
        Assert.Same(callerContext, SynchronizationContext.Current);
        var roslyn = (HixRoslynContext)thread.Context;
        var symbol = new RoslynHixValue(roslyn.CurrentType);
        if (!roslyn.TryGetDerived(symbol, "customName", out var result)) {
          result = new LiteralHixValue(roslyn.NameOfService(thread, symbol));
          roslyn.StoreDerived(symbol, "customName", result);
        }
        return result;
      }));
    }
  }
}
