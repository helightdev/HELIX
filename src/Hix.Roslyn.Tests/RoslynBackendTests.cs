using System;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Hix.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
namespace Hix.Roslyn.Tests;
public class RoslynBackendTests {
  [Fact] public void SemanticBackendWorksWithoutGeneratorAssembly() {
    var compilation = CSharpCompilation.Create("test", new[] {CSharpSyntaxTree.ParseText("class MarkerAttribute : System.Attribute {} [Marker] class Example {}")}, new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)}, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    var backend = new HixRoslynBackend();
    var program = HixCompiler.Compile("mixin Test { prelude expression { emit(length(collectAnnotatedTypes(<MarkerAttribute>))) } }", "Test", backend);
    var result = new HixVM(new[] {program}).Run(program, new HixThread(backend.CreateContext(compilation)));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("1", result.Outputs.Single().Text.Resolve(result.Strings));
    Assert.DoesNotContain(typeof(HixVM).Assembly.GetReferencedAssemblies(), assembly => assembly.Name.Contains("CodeAnalysis") || assembly.Name.Contains("Generator"));
    Assert.DoesNotContain(typeof(HixRoslynBackend).Assembly.GetReferencedAssemblies(), assembly => assembly.Name.Contains("Generator"));
  }

  [Fact]
  public void ExternalRoslynBackendCanConstructValuesAndExtendThreads() {
    var compilation = CSharpCompilation.Create("extension", [CSharpSyntaxTree.ParseText("class Example {}")],
      [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
    var type = compilation.GetTypeByMetadataName("Example");
    var backend = new ExtensionBackend(compilation, type);
    var program = HixCompiler.CompileFunctions(["func main { return(customName()) }"], backend);
    var thread = backend.CreateThread();
    var result = new HixVM([program]).Invoke(program, thread);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("Example", result.Value.Unlink(thread));
    Assert.Same(type, Assert.IsType<ExtensionContext>(thread.Context).CurrentType);
  }

  private sealed class ExtensionContext(HixBackend backend, CSharpCompilation compilation, INamedTypeSymbol type)
    : HixRoslynContext(type, type, null, compilation, backend: backend);

  private sealed class ExtensionBackend(CSharpCompilation compilation, INamedTypeSymbol type) : HixRoslynBackend {
    public override HixContext CreateContext() => new ExtensionContext(this, compilation, type);
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.Add(new SimpleFunction("customName", [new(HixValueKind.String, [])], (thread, _) => {
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
