using System.Linq;
using System;
using Hix;
using Hix.Runtime;
using Hix.Compiler;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixExternalFunctionTests {
  [Fact]
  public void HostDefinitionsExposeOnlyTheirNameAndReceiveErrorsUnchanged() {
    Assert.Equal(new[] {"Name"}, typeof(FunctionDefinition).GetProperties().Select(property => property.Name));
    var backend = new ErrorHandlingBackend();
    var program = HixCompiler.CompileFunctions(["pure func main { return(handle([cast(<kept>, error)?])) }"], backend);
    using var vm = new HixVM([program]);
    var result = vm.Invoke(program, backend.CreateCoroutine());
    Assert.True(result.Success);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Value).Value);
    Assert.IsType<ErrorHixValue>(backend.Received);
  }

  [Fact]
  public void EmbeddedOpaqueDeclarationsRegisterNominalPatternConstructors() {
    var backend = new LibraryBackend("""
      %opaque<tests.box>(1)
      type box = opaque;
      pure func identity(number value) -> number external hostIdentity;
      """);
    var definition = Assert.Single(backend.Functions.OpaqueTypes);
    Assert.Equal("tests.box", definition.Identity);
    Assert.Equal(1, definition.Arity);
    var unit = AntlrSyntax.Parse("type NumberBox = %box<number>;", backend);
    Assert.Empty(unit.Diagnostics);
    var pattern = Assert.IsType<OpaqueHixPattern>(Assert.IsType<TypeDeclarationIr>(Assert.Single(unit.Declarations)).Pattern);
    Assert.Equal("box<number>", pattern.Display);
  }

  [Theory]
  [InlineData("%opaque<tests.box>(-1)")]
  [InlineData("%opaque<tests.box>(0.5)")]
  [InlineData("%opaque<>(1)")]
  public void InvalidEmbeddedOpaqueDeclarationsAreRejected(string metadata) {
    var backend = new LibraryBackend(metadata + " type box = opaque; pure func identity(number value) -> number external hostIdentity;");
    Assert.Throws<ArgumentException>(() => backend.Functions);
  }

  private sealed class ErrorHandlingBackend : HixBackend {
    public IHixValue Received;
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.AddLibrary("pure func handle(any value) -> number external;");
      functions.Add(new SimpleFunction("handle", (_, arguments) => {
        Received = arguments[0];
        return new NumberHixValue(42);
      }));
    }
  }

  [Fact]
  public void MetadataDeclarationsRegisterAnnotationsForExplicitTargetsWithoutRuntimeHandlers() {
    var backend = new LibraryBackend("""
      meta annotation(string text) for function type;
      pure func identity(number value) -> number external hostIdentity;
      """);
    Assert.Single(backend.Functions.ResolveMetadata("annotation", HixMetadataKind.FunctionDefinition, 1));
    Assert.Empty(backend.Functions.Resolve("annotation"));
    Assert.Empty(backend.Functions.ResolveMetadata("annotation", HixMetadataKind.FunctionDefinition, 0));
    Assert.Single(backend.Functions.ResolveMetadata("annotation", HixMetadataKind.TypeDefinition, 1));
    Assert.Empty(backend.Functions.ResolveMetadata("annotation", HixMetadataKind.File, 1));
    var unit = AntlrSyntax.Parse("%annotation<example> func main -> number { return(42) }", backend);
    Assert.Empty(unit.Diagnostics);
    Assert.Throws<ArgumentException>(() => HixCompiler.CompileFunctions(["func main { annotation(\"example\") }"], backend));
  }

  [Fact]
  public void MetadataDeclarationsSurviveParsingRewritingAndSourceRoundtrip() {
    var unit = AntlrSyntax.Parse("meta annotation(string text) for function type;");
    Assert.Empty(unit.Diagnostics);
    var declaration = Assert.IsType<MetadataDeclarationIr>(Assert.Single(unit.Declarations));
    Assert.Equal(HixMetadataKind.FunctionDefinition | HixMetadataKind.TypeDefinition, declaration.Targets);
    Assert.Equal(declaration.Targets, new HixIrRewriter().Rewrite(declaration).Targets);
    var source = HixDecompiler.ToSource(unit);
    Assert.Contains("meta annotation", source);
    Assert.Equal(declaration.Targets, Assert.IsType<MetadataDeclarationIr>(Assert.Single(AntlrSyntax.Parse(source).Declarations)).Targets);
    Assert.NotEmpty(AntlrSyntax.Parse("metadata func annotation() -> pattern external;").Diagnostics);
  }

  [Theory]
  [InlineData("meta annotation() for unknown;", "unknown metadata target")]
  [InlineData("meta annotation() for function function;", "duplicate metadata target")]
  [InlineData("meta annotation(number value, string value) for function;", "duplicate signature field")]
  [InlineData("meta annotation(%variadic any values, number value) for function;", "only the last metadata argument")]
  public void InvalidMetadataDeclarationsAreDiagnosed(string source, string message) {
    Assert.Contains(AntlrSyntax.Parse(source).Diagnostics, diagnostic => diagnostic.Message.Contains(message));
  }

  [Fact]
  public void LocalMetadataDeclarationsAreVisibleBeforeAndAfterTheirDeclaration() {
    const string source = """
      %annotation<before>
      func first -> number { return(42) }
      meta annotation(string text) for function;
      %annotation<after>
      func second -> number { return(42) }
      """;
    Assert.Empty(AntlrSyntax.Parse(source).Diagnostics);
    Assert.NotEmpty(AntlrSyntax.Parse("meta annotation() for function; %annotation type Example = number;").Diagnostics);
  }

  [Fact]
  public void HostEffectsComeFromDeclarationPurityRatherThanTheHandler() {
    var backend = new LibraryBackend("""
      pure func identity(number value) -> number external hostIdentity;
      func effect(number value) -> number external hostIdentity;
      """);
    Assert.False(Assert.Single(backend.Functions.Resolve("identity")).HasEffects);
    Assert.True(Assert.Single(backend.Functions.Resolve("effect")).HasEffects);
    Assert.Empty(AntlrSyntax.Parse("pure func main -> number { return(identity(42)) }", backend).Diagnostics);
    Assert.NotEmpty(AntlrSyntax.Parse("pure func main -> number { return(effect(42)) }", backend).Diagnostics);
  }

  private sealed class LibraryBackend(string source) : HixBackend {
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.Add(new SimpleFunction("hostIdentity", (_, arguments) => arguments[0]));
      functions.AddLibrary(source);
    }
  }

  [Fact]
  public void EmbeddedDeclarationsBindAliasesGenericsAndDefaults() {
    var backend = new LibraryBackend("""
      %description<Host identity>
      %generic<T>
      pure func identity(T value) -> T external hostIdentity;
      pure func defaultIdentity(number value = 42) -> number external hostIdentity;
      """);
    var definition = Assert.Single(backend.Functions.Resolve("identity"));
    Assert.Equal("Host identity", definition.Documentation);
    Assert.Single(definition.Signatures[0].TypeParameters);
    Assert.Empty(backend.Functions.Resolve("hostIdentity"));
    var program = HixCompiler.CompileFunctions(["func main -> number { return(defaultIdentity()) }"], backend);
    using var vm = new HixVM([program]);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(vm.Invoke(program, backend.CreateCoroutine()).Value).Value);
    var analysis = new LanguageAnalysis("func main { local value = identity(42) }", backend);
    Assert.Empty(analysis.Program.Diagnostics);
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local value: number");
  }

  [Theory]
  [InlineData("pure func identity(number value) -> number external missing;")]
  [InlineData("pure func identity(number value) -> number { return($value) }")]
  [InlineData("")]
  public void InvalidLibrariesAndMissingHandlersFailAtInitialization(string source) {
    var backend = new LibraryBackend(source);
    Assert.Throws<InvalidOperationException>(() => backend.Functions);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("hostIdentity")]
  public void ExternalDeclarationsHaveNoBodyAndPreserveSignatureMetadata(string hostName) {
    var source = """
      %description<Deferred identity>
      %generic<T>
      pure func deferred(T value) -> T external;
      func main -> number { return(42) }
      """.Replace("external;", "external" + (hostName == null ? "" : " " + hostName) + ";");
    var analysis = new LanguageAnalysis(source);
    Assert.Empty(analysis.Program.Diagnostics);
    var declaration = analysis.Program.Declarations.OfType<FunctionDeclarationIr>().First();
    Assert.True(declaration.IsExternal);
    Assert.Null(declaration.Body);
    Assert.Equal(hostName, declaration.ExternalName);
    Assert.All(declaration.Children, child => Assert.NotNull(child));
    Assert.Single(declaration.TypeParameters);
    Assert.Single(declaration.Signatures);
    Assert.Equal("T", declaration.Signatures[0].OutputPattern.Display);
    var rewritten = new HixIrRewriter().Rewrite(declaration);
    Assert.True(rewritten.IsExternal);
    Assert.Null(rewritten.Body);
    Assert.Equal(hostName, rewritten.ExternalName);
    var rendered = HixDecompiler.ToSource(analysis.Program);
    Assert.Contains("external" + (hostName == null ? "" : " " + hostName) + ";", rendered);
    Assert.Equal(rendered, HixDecompiler.ToSource(AntlrSyntax.Parse(rendered)));
    var image = HixCompiler.CompileFunctions([source]);
    Assert.DoesNotContain(image.Scope.AllCandidates(), candidate => candidate.Function.Name == "deferred");
    Assert.Contains(image.Scope.AllCandidates(), candidate => candidate.Function.Name == "main");
  }

  [Fact]
  public void AnalyzerInfersExternalGenericCallSignaturesWithoutAnalyzingABody() {
    var analysis = new LanguageAnalysis("""
      %generic<T>
      func deferred(T value) -> T external
      func main { local result = deferred(42) }
      """);
    Assert.Empty(analysis.Program.Diagnostics);
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Documentation == "local result: number");
  }
}
