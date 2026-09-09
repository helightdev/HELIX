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
    var result = new HixVM(new[] {program}).Run(program, backend.CreateContext(compilation));
    Assert.True(result.Success, result.Error);
    Assert.Equal("1", result.Outputs.Single().Text);
    Assert.DoesNotContain(typeof(HixVM).Assembly.GetReferencedAssemblies(), assembly => assembly.Name.Contains("CodeAnalysis") || assembly.Name.Contains("Generator"));
    Assert.DoesNotContain(typeof(HixRoslynBackend).Assembly.GetReferencedAssemblies(), assembly => assembly.Name.Contains("Generator"));
  }
}
