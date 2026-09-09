using System;
using System.IO;
using System.Linq;
using Hix;
using Hix.Compiler;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinRepositorySyntaxTests {
  [Fact]
  public void FunctionCatalogUsesFlatRuntimeSignatures() {
    var members = Hix.HixMixinBackend.Instance.Functions.Enumerate().Single(item => item.Name == "members");
    Assert.Equal(HixValueKind.Symbol, members.ReceiverType);
    Assert.Equal(HixValueKind.Tuple, members.ResultType);
    Assert.Equal([HixValueKind.Symbol], members.ArgumentTypes);

    var matches = Hix.HixMixinBackend.Instance.Functions.Enumerate().Single(item => item.Name == "matches");
    Assert.Equal(HixValueKind.Bool, matches.ResultType);
    Assert.Equal([HixValueKind.String, HixValueKind.String], matches.ArgumentTypes);
    Assert.DoesNotContain(Hix.HixMixinBackend.Instance.Functions.Enumerate(), item => item.Name == "format");
    Assert.All(Hix.HixMixinBackend.Instance.Functions.Enumerate(), definition =>
      Assert.Equal(definition.ArgumentCount + (definition.IsVariadic ? 1 : 0), definition.ArgumentTypes.Count));
  }

  [Fact]
  public void LanguageDefinitionsResolveFixedAndVariadicSignatures() {
    Assert.True(Hix.HixMixinBackend.Instance.Functions.TryGet("matches", 2, out var matches));
    Assert.False(Hix.HixMixinBackend.Instance.Functions.TryGet("matches", 1, out _));
    Assert.Equal([HixValueKind.String, HixValueKind.String], matches.ArgumentTypes);
    Assert.True(Hix.HixMixinBackend.Instance.Functions.TryGet("and", 3, out var and));
    Assert.True(Hix.HixMixinBackend.Instance.Functions.TryGet("and", 5, out _));
    Assert.True(and.IsVariadic);
    var inject = Hix.HixMixinBackend.Instance.Functions.Enumerate().Single(definition => definition.Name == "inject");
    Assert.Equal(2, inject.Signatures.Count);
    Assert.True(inject.MatchesArgumentCount(2));
    Assert.True(inject.MatchesArgumentCount(3));
    Assert.False(inject.MatchesArgumentCount(1));
    Assert.Same(inject, Hix.HixMixinBackend.Instance.Functions.Resolve("inject", 2).Single());
    Assert.Same(inject, Hix.HixMixinBackend.Instance.Functions.Resolve("inject", 3).Single());
    Assert.Equal(3, Hix.HixMixinBackend.Instance.Functions.Enumerate().Single(definition => definition.Name == "length").Signatures.Count);
    Assert.Equal(2, Hix.HixMixinBackend.Instance.Functions.Enumerate().Single(definition => definition.Name == "join").Signatures.Count);
  }

  [Fact]
  public void ValueKindsAreFlatAndComplete() {
    Assert.Equal(new[] {
      HixValueKind.Any, HixValueKind.Null, HixValueKind.String, HixValueKind.Bool,
      HixValueKind.Number, HixValueKind.Tuple, HixValueKind.Table, HixValueKind.Symbol,
      HixValueKind.Function, HixValueKind.Error, HixValueKind.Kind, HixValueKind.Pattern
    }, Enum.GetValues<HixValueKind>());
  }

  [Fact]
  public void RootDefinitionsHaveUniqueNamesAndResolvableMetadata() {
    var definitions = HixRootLibrary.Enumerate().ToArray();
    Assert.Equal(definitions.Length, definitions.Select(item => item.Name).Distinct().Count());
    Assert.Equal(definitions.Length, definitions.Select(item => item.Root).Distinct().Count());
    Assert.All(definitions, definition => {
      Assert.False(string.IsNullOrWhiteSpace(definition.Documentation));
      Assert.True(HixRootLibrary.TryGet(definition.Name, out var byName));
      Assert.True(HixRootLibrary.TryGet(definition.Root, out var byRoot));
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
