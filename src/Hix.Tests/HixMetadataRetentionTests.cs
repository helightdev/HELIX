using System;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixMetadataRetentionTests {
  private const string Source = """
    %erased<file-compiler>
    %transient<file-bytecode>
    %retained<file-runtime>
    ---
    compiler meta erased(any data) for file function field type mixin;
    bytecode meta transient(any data) for file function field type mixin;
    runtime meta retained(any data) for file function field type mixin;

    %erased<type-compiler>
    %transient<type-bytecode>
    %retained<type-runtime>
    type Record = @{
      %erased<member-compiler> %transient<member-bytecode> %retained<member-runtime> number value
    };

    %erased<function-compiler>
    %transient(@{key=<function-bytecode>})
    %retained(@[<function-runtime>, 2])
    pure noinline func decorated(
      %erased<field-compiler> %transient<field-bytecode> %retained<field-runtime> number value
    ) -> number => $value;

    pure func main -> number => decorated(7);
    """;

  [Fact]
  public void RetentionControlsImageAndLoadedDescriptorsWithoutChangingExecution() {
    var image = HixCompiler.CompileFunctions([Source]);
    var function = Assert.Single(image.Functions.Where(entry => entry.Function.Name == "decorated")).Function;
    Assert.Equal(new[] {"transient", "retained"}, function.Metadata.Select(item => item.Name));
    Assert.IsType<HixTableValue>(function.Metadata[0].Values[0]);
    Assert.IsType<TupleHixValue>(function.Metadata[1].Values[0]);
    Assert.Equal(new[] {"transient", "retained"}, function.Signatures[0].Inputs[0].Metadata.Select(item => item.Name));
    Assert.Equal(new[] {"transient", "retained"}, image.Metadata.Select(item => item.Name));
    Assert.Equal(new[] {"transient", "retained"}, image.TypeMetadata["Record"].Select(item => item.Name));
    Assert.Equal(new[] {"transient", "retained"}, image.MetadataDefinitions.Select(item => item.Name));
    Assert.Equal(2, Assert.IsType<TableHixPattern>(image.Patterns["Record"]).Fields[0].Metadata.Count);

    using var vm = new HixVM([image]);
    var loaded = Assert.Single(vm.Functions.Where(entry => entry.Function.Name == "decorated")).Function;
    Assert.Equal("retained", Assert.Single(loaded.Metadata).Name);
    Assert.Equal("retained", Assert.Single(loaded.Signatures[0].Inputs[0].Metadata).Name);
    Assert.Equal("retained", Assert.Single(vm.Metadata).Name);
    Assert.Equal("retained", Assert.Single(vm.TypeMetadata["Record"]).Name);
    Assert.Equal("retained", Assert.Single(vm.MetadataDefinitions).Name);
    Assert.Equal("retained", Assert.Single(Assert.IsType<TableHixPattern>(vm.Patterns["Record"]).Fields[0].Metadata).Name);
    Assert.DoesNotContain(loaded.Metadata, item => item.Retention != HixMetadataRetention.Runtime);
    // Loading creates descriptors without mutating the reusable compiled artifact.
    Assert.Equal(2, function.Metadata.Count);
    var result = vm.Invoke(image, new HixCoroutine());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(7, Assert.IsType<NumberHixValue>(result.Value).Value);
  }

  [Fact]
  public void MixinParametersAndDerivationsFollowTheSameLoadingBoundary() {
    var unit = AntlrSyntax.Parse(Source + """

      %erased<mixin-compiler> %transient<mixin-bytecode> %retained<mixin-runtime>
      mixin Test(%transient<parameter-bytecode> %retained<parameter-runtime> number value) {
        expression { emit(<ok>) }
      }
      %transient<provider-bytecode> %retained<provider-runtime>
      derivation mixin Provider { expression { return(param#value) } }
      """);
    Assert.Empty(unit.Diagnostics);
    var image = HixCompiler.Compile(unit, "Test");
    Assert.Equal(2, image.MixinMetadata.Count);
    Assert.Equal(2, image.Parameters[0].Metadata.Count);
    Assert.Equal(2, Assert.Single(image.Derivations).Metadata.Count);
    Assert.Equal(2, image.ForPass(true).MixinMetadata.Count);
    using var vm = new HixVM([image]);
    Assert.Equal("retained", Assert.Single(vm.MixinMetadata).Name);
    Assert.Equal("retained", Assert.Single(vm.Parameters[0].Metadata).Name);
    var provider = Assert.Single(vm.Derivations);
    Assert.Equal("retained", Assert.Single(provider.Metadata).Name);
    Assert.All(provider.Expressions, expression => Assert.Contains(expression.Function.Metadata, item => item.Name == "synthetic"));
    Assert.All(vm.Functions.SelectMany(entry => entry.Scope.Functions()).SelectMany(entry => entry.Function.Metadata),
      item => Assert.Equal(HixMetadataRetention.Runtime, item.Retention));
    var decompiled = AntlrSyntax.Parse(HixDecompiler.ToSource(HixDecompiler.Decompile(image)));
    Assert.Empty(decompiled.Diagnostics);
    var rebuilt = HixCompiler.Compile(decompiled, "Decompiled");
    Assert.Equal(2, rebuilt.MixinMetadata.Count);
    Assert.Equal(2, rebuilt.Parameters[0].Metadata.Count);
  }

  [Fact]
  public void GeneratedConstructorsLambdasAndExpressionFunctionsAreSynthetic() {
    var unit = AntlrSyntax.Parse("""
      type Record = @{number value};
      type Pair = @[number value];
      pure func declared -> function => func => $0;
      mixin Test { expression { emit(<ok>) } }
      """);
    var catalog = HixCompiler.PrepareGlobals([unit]);
    foreach (var function in catalog.Functions.Where(function => function.Name is "Record" or "Pair" || function.Name.StartsWith("__lambda_"))) {
      var metadata = Assert.Single(function.Metadata.Where(item => item.Name == "synthetic"));
      Assert.Equal(HixMetadataRetention.Runtime, metadata.Retention);
    }
    Assert.DoesNotContain(Assert.Single(catalog.Functions.Where(function => function.Name == "declared")).Metadata,
      item => item.Name == "synthetic");
    var image = HixCompiler.Compile(unit, "Test");
    Assert.Equal("synthetic", Assert.Single(Assert.Single(image.Expressions).Function.Metadata).Name);
    using var vm = new HixVM([image]);
    Assert.All(vm.Functions.Where(entry => entry.Function.Name == "" || entry.Function.Name is "Record" or "Pair" || entry.Function.Name.StartsWith("__lambda_")),
      entry => Assert.Contains(entry.Function.Metadata, item => item.Name == "synthetic"));
  }

  [Fact]
  public void RetainedMetadataSurvivesIrRewritingAndBytecodeSourceRoundtrip() {
    var unit = AntlrSyntax.Parse(Source);
    var rewriter = new HixIrRewriter();
    Assert.All(unit.Declarations.OfType<MetadataDeclarationIr>(), declaration =>
      Assert.Equal(declaration.Retention, rewriter.Rewrite(declaration).Retention));
    var function = Assert.Single(unit.Declarations.OfType<FunctionDeclarationIr>().Where(function => function.Name == "decorated"));
    Assert.Equal(function.Metadata.Select(item => item.Retention), rewriter.Rewrite(function).Metadata.Select(item => item.Retention));
    var image = HixCompiler.CompileFunctions([unit]);
    var source = HixDecompiler.ToSource(HixDecompiler.Decompile(image));
    var reparsed = AntlrSyntax.Parse(source);
    Assert.Empty(reparsed.Diagnostics);
    var rebuilt = HixCompiler.CompileFunctions([reparsed]);
    var metadata = Assert.Single(rebuilt.Functions.Where(entry => entry.Function.Name == "decorated")).Function.Metadata;
    Assert.Equal(new[] {HixMetadataRetention.Bytecode, HixMetadataRetention.Runtime}, metadata.Select(item => item.Retention));
    Assert.Equal(2, rebuilt.TypeMetadata["Record"].Count);
  }

  [Fact]
  public void MetadataChangesAffectImageIdentity() {
    var first = HixCompiler.CompileFunctions(["runtime meta label(string text) for function; %label<a> pure func main -> number => 1;"]);
    var second = HixCompiler.CompileFunctions(["runtime meta label(string text) for function; %label<b> pure func main -> number => 1;"]);
    Assert.NotEqual(first.Identity, second.Identity);
    var composite = HixCompiler.CompileFunctions([Source]);
    var changed = HixCompiler.CompileFunctions([Source.Replace("function-runtime", "different-runtime")]);
    Assert.NotEqual(composite.Identity, changed.Identity);
    var changedField = HixCompiler.CompileFunctions([Source.Replace("member-runtime", "different-member")]);
    Assert.NotEqual(composite.Identity, changedField.Identity);
  }

  [Theory]
  [InlineData("compiler runtime meta invalid() for function;")]
  [InlineData("bytecode bytecode meta invalid() for function;")]
  [InlineData("runtime meta invalid() for function; compiler meta invalid(string text) for function;")]
  public void ConflictingRetentionModifiersAreRejected(string source) =>
    Assert.NotEmpty(AntlrSyntax.Parse(source).Diagnostics);

  [Fact]
  public void RetainedMetadataRejectsNonconstantArguments() => Assert.Throws<ArgumentException>(() =>
    HixCompiler.CompileFunctions(["runtime meta label(string text) for function; %label(uppercase(<a>)) pure func main -> number => 1;"]));
}
