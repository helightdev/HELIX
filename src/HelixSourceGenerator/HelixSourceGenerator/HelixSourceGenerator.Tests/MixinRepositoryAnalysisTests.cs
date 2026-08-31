using System.IO;
using System.Linq;
using System;
using MixinLanguage;
using MixinLanguage.Compiler;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinRepositorySyntaxTests {
  [Fact]
  public void EditorCatalogUsesRuntimeTypeSignatures() {
    var members = FunctionLibrary.Enumerate().Single(item => item.Name == "members");
    Assert.Equal(MixinLanguageValueKind.Type, members.ReceiverType);
    Assert.Equal(MixinLanguageValueKind.Table, members.ResultType);

    var reduce = FunctionLibrary.Enumerate().Single(item => item.Name == "reduce");
    Assert.Equal(MixinLanguageValueKind.Table, reduce.ReceiverType);
    Assert.Equal([
      MixinLanguageValueKind.Any, MixinLanguageValueKind.Function
    ], reduce.ArgumentTypes);

    var makeGeneric = FunctionLibrary.Enumerate().Single(item => item.Name == "makeGeneric");
    Assert.Equal(MixinLanguageValueKind.Type, makeGeneric.ResultType);
    Assert.Equal([MixinLanguageValueKind.CSharpType], makeGeneric.ArgumentTypes);

    var annotation = DirectiveLibrary.EnumerateLanguageDefinitions()
      .Single(item => item.Name == "ANNOTATION");
    Assert.Equal([MixinLanguageValueKind.CSharpType], annotation.ArgumentTypes);
  }

  [Theory]
  [InlineData("RESOLVE_MIXIN")]
  [InlineData("PUSH")]
  [InlineData("PUT")]
  public void ExecutableDirectivesUseRegisteredFunctionDefinitions(string name) {
    Assert.True(DirectiveLibrary.TryGet(name, out var directive));
    Assert.True(FunctionLibrary.TryGet(name, out var function));

    Assert.Same(function, directive.Function);
    Assert.Equal(function.ArgumentTypes, directive.ArgumentTypes);
    Assert.False(string.IsNullOrWhiteSpace(function.Documentation));
  }

  [Fact]
  public void EveryExpressionRootHasOneDocumentedRegistryDefinition() {
    var definitions = MixinRootLibrary.Enumerate().ToArray();

    Assert.Equal(Enum.GetValues<MixinExpressionRoot>().Length, definitions.Length);
    Assert.Equal(definitions.Length, definitions.Select(item => item.Name).Distinct().Count());
    Assert.Equal(definitions.Length, definitions.Select(item => item.Root).Distinct().Count());
    Assert.All(definitions, definition => {
      Assert.False(string.IsNullOrWhiteSpace(definition.Name));
      Assert.False(string.IsNullOrWhiteSpace(definition.Documentation));
      Assert.True(MixinRootLibrary.TryGet(definition.Name, out var byName));
      Assert.True(MixinRootLibrary.TryGet(definition.Root, out var byRoot));
      Assert.Same(definition, byName);
      Assert.Same(definition, byRoot);
    });
    Assert.Equal(definitions, MixinLanguageCatalog.Roots.Select(item => item.Definition));
  }

  [Fact]
  public void EveryRepositoryMixinFileProducesACompleteCanonicalAst() {
    var root = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (root is not null && !Directory.Exists(Path.Combine(
      root.FullName, "src", "HELIX", "Assets", "Mixins"
    ))) root = root.Parent;
    Assert.NotNull(root);

    var files = Directory.GetFiles(
      Path.Combine(root!.FullName, "src", "HELIX", "Assets", "Mixins"),
      "*.HelixSourceGenerator.additionalfile"
    );
    Assert.NotEmpty(files);

    var programs = files.Select(path => MixinParser.Parse(File.ReadAllText(path))).ToArray();
    Assert.All(programs, program => Assert.Same(program, program.Root));
    Assert.True(programs.Sum(program => program.Children.Count) > 100);
    Assert.True(programs.Sum(program => program.Tokens.Count) > 100);
    Assert.True(programs.SelectMany(program => program.Children)
      .SelectMany(node => node.Children)
      .OfType<LeafAst>()
      .Count(leaf => leaf.ArgumentMetadata is not null) > 100);
  }
}
