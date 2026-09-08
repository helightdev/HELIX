using System;
using System.IO;
using System.Linq;
using Mixins;
using Mixins.Compiler;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinRepositorySyntaxTests {
  [Fact]
  public void FunctionCatalogUsesFlatRuntimeSignatures() {
    var members = FunctionLibrary.Enumerate().Single(item => item.Name == "members");
    Assert.Equal(MixinValueKind.Symbol, members.ReceiverType);
    Assert.Equal(MixinValueKind.Tuple, members.ResultType);
    Assert.Equal([MixinValueKind.Symbol], members.ArgumentTypes);

    var matches = FunctionLibrary.Enumerate().Single(item => item.Name == "matches");
    Assert.Equal(MixinValueKind.Bool, matches.ResultType);
    Assert.Equal([MixinValueKind.String, MixinValueKind.String], matches.ArgumentTypes);
    Assert.DoesNotContain(FunctionLibrary.Enumerate(), item => item.Name == "format");
    Assert.All(FunctionLibrary.Enumerate(), definition =>
      Assert.Equal(definition.ArgumentCount + (definition.IsVariadic ? 1 : 0), definition.ArgumentTypes.Count));
  }

  [Fact]
  public void LanguageDefinitionsResolveFixedAndVariadicSignatures() {
    Assert.True(FunctionLibrary.TryGet("matches", 2, out var matches));
    Assert.False(FunctionLibrary.TryGet("matches", 1, out _));
    Assert.Equal([MixinValueKind.String, MixinValueKind.String], matches.ArgumentTypes);
    Assert.True(FunctionLibrary.TryGet("and", 3, out var and));
    Assert.True(FunctionLibrary.TryGet("and", 5, out _));
    Assert.True(and.IsVariadic);
    var inject = FunctionLibrary.Enumerate().Single(definition => definition.Name == "inject");
    Assert.Equal(2, inject.Signatures.Count);
    Assert.True(inject.MatchesArgumentCount(2));
    Assert.True(inject.MatchesArgumentCount(3));
    Assert.False(inject.MatchesArgumentCount(1));
    Assert.Same(inject, FunctionLibrary.Resolve("inject", 2).Single());
    Assert.Same(inject, FunctionLibrary.Resolve("inject", 3).Single());
    Assert.Equal(3, FunctionLibrary.Enumerate().Single(definition => definition.Name == "length").Signatures.Count);
    Assert.Equal(2, FunctionLibrary.Enumerate().Single(definition => definition.Name == "join").Signatures.Count);
  }

  [Fact]
  public void ValueKindsAreFlatAndComplete() {
    Assert.Equal(new[] {
      MixinValueKind.Any, MixinValueKind.Null, MixinValueKind.String, MixinValueKind.Bool,
      MixinValueKind.Number, MixinValueKind.Tuple, MixinValueKind.Table, MixinValueKind.Symbol,
      MixinValueKind.Function, MixinValueKind.Error, MixinValueKind.Kind
    }, Enum.GetValues<MixinValueKind>());
  }

  [Fact]
  public void EveryExpressionRootHasOneDefinition() {
    var definitions = MixinRootLibrary.Enumerate().ToArray();
    Assert.Equal(Enum.GetValues<MixinExpressionRoot>().Length, definitions.Length);
    Assert.Equal(definitions.Length, definitions.Select(item => item.Name).Distinct().Count());
    Assert.Equal(definitions.Length, definitions.Select(item => item.Root).Distinct().Count());
    Assert.All(definitions, definition => {
      Assert.False(string.IsNullOrWhiteSpace(definition.Documentation));
      Assert.True(MixinRootLibrary.TryGet(definition.Name, out var byName));
      Assert.True(MixinRootLibrary.TryGet(definition.Root, out var byRoot));
      Assert.Same(definition, byName);
      Assert.Same(definition, byRoot);
    });
  }

  [Fact]
  public void EveryRepositoryMixinFileProducesCanonicalSemanticAst() {
    var root = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "src", "HELIX", "Assets", "Mixins")))
      root = root.Parent;
    Assert.NotNull(root);
    var files = Directory.GetFiles(Path.Combine(root!.FullName, "src", "HELIX", "Assets", "Mixins"),
      "*.HelixSourceGenerator.additionalfile");
    Assert.NotEmpty(files);
    var units = files.Select(path => AntlrSyntax.Parse(File.ReadAllText(path))).ToArray();
    Assert.All(units, unit => Assert.Empty(unit.Diagnostics));
    Assert.True(units.Sum(unit => unit.Declarations.Count) > 10);
    Assert.True(units.Sum(unit => unit.Tokens.Count) > 100);
  }
}
