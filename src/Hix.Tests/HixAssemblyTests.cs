using System;
using System.Linq;
using System.IO;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixAssemblyTests {
  private static HixProgramImage RoundTrip(HixProgramImage assembly) {
    using var stream = new MemoryStream();
    HixAssemblySerializer.Write(stream, assembly);
    stream.Position = 0;
    return HixAssemblySerializer.Read(stream);
  }

  [Fact]
  public void SerializedAssembliesSupportReflectionAndCrossAssemblyCallbacks() {
    var library = RoundTrip(HixCompiler.CompileAssembly("library", [
      "pure func origin -> string { return(assemblyName()) }"
    ]));
    var application = RoundTrip(HixCompiler.CompileAssembly("application", ["""
      mixin Main { expression {
        emit(assemblyName())
        emit(call(assemblyFunction(<library>, <origin>)))
        emit(assemblyName())
        emit(length(assemblyNames()))
        emit(length(assemblyFunctions(<library>)))
      } }
      """], [library], "Main"));
    var vm = new HixVM([application, library]);
    var result = vm.Run(application, new HixCoroutine());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"application", "library", "application", "2", "1"}, result.Outputs.Select(output => output.ReadText()));
    Assert.Null(vm.BindAssemblyFunction("application", "origin"));
  }

  [Fact]
  public void AssemblyFileRejectsUnknownVersionsAndTruncation() {
    var image = HixCompiler.CompileAssembly("empty", []);
    using var stream = new MemoryStream();
    HixAssemblySerializer.Write(stream, image);
    var bytes = stream.ToArray();
    bytes[4] = 99;
    Assert.Throws<InvalidDataException>(() => HixAssemblySerializer.Read(new MemoryStream(bytes)));
    Assert.Throws<EndOfStreamException>(() => HixAssemblySerializer.Read(new MemoryStream(bytes.Take(3).ToArray())));
  }

  [Fact]
  public void SerializedDependencyDefaultsRetainTablesAndExplicitNulls() {
    var library = RoundTrip(HixCompiler.CompileAssembly("library", ["""
      pure func describe(any record = @{name=<default>, note=null}) -> string { return(get($record, <name>)) }
      """]));
    var application = RoundTrip(HixCompiler.CompileAssembly("application", ["""
      mixin Main { expression { emit(describe()); emit(describe(record = @{name=<named>, note=null})) } }
      """], [library], "Main"));
    var result = new HixVM([application, library]).Run(application, new HixCoroutine());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"default", "named"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void AnalyzerUsesDependencySignaturesForCallTypes() {
    var library = HixCompiler.CompileAssembly("library", ["pure func describe(number value) -> string { return(<result>) }"]);
    var analysis = new LanguageAnalysis("mixin Main { expression { emit(describe(42)) } }", assemblies: [library]);
    Assert.Contains(analysis.TypeFacts, fact => fact.Kind == "Call" && fact.Type == "string" && fact.Documentation.Contains("describe"));
    Assert.DoesNotContain(analysis.Declarations, declaration => declaration.Name == "describe");
  }

  [Fact]
  public void DependenciesExecuteWithMergedStringsAndLocalCodeAndConstants() {
    var library = HixCompiler.CompileAssembly("library", [
      "pure func describe(number value) -> string { return(<library[$value]>) }"
    ]);
    var application = HixCompiler.CompileAssembly("application", ["""
      pure func localValue -> number { return(37) }
      mixin Main { expression { emit(describe(localValue())); emit(<application>) } }
      """], [library], "Main");
    Assert.DoesNotContain(application.Scope.AllCandidates(), candidate => candidate.Function.Name == "describe");
    var vm = new HixVM([application, library]);
    var result = vm.Run(application, new HixCoroutine());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"library37", "application"}, result.Outputs.Select(output => output.ReadText()));
    Assert.Equal(2, vm.Assemblies.Count);
    // Loading another VM must not mutate the portable scope or string identities.
    var other = new HixVM([library, application]);
    var repeated = other.Run(application, new HixCoroutine());
    Assert.Equal(result.Outputs.Select(output => output.ReadText()), repeated.Outputs.Select(output => output.ReadText()));
    Assert.DoesNotContain(application.Scope.AllCandidates(), candidate => candidate.Function.Name == "describe");
  }

  [Fact]
  public void MissingAndDuplicateAssemblyNamesAreRejected() {
    var library = HixCompiler.CompileAssembly("library", ["pure func value -> number { return(1) }"]);
    var application = HixCompiler.CompileAssembly("application", [], [library]);
    Assert.Throws<ArgumentException>(() => new HixVM([application]));
    var duplicate = HixCompiler.CompileAssembly("library", []);
    Assert.Throws<ArgumentException>(() => new HixVM([library, duplicate]));
  }
}
