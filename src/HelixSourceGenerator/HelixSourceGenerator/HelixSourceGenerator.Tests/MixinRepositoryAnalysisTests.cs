using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using Mixins;
using Mixins.Compiler;
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

    var annotation = DirectiveLibrary.Enumerate()
      .Single(item => item.Name == "ANNOTATION");
    Assert.Equal([MixinLanguageValueKind.CSharpType], annotation.ArgumentTypes);
  }

  [Theory]
  [InlineData("RESOLVE_MIXIN")]
  [InlineData("PUSH")]
  [InlineData("PUT")]
  public void ExecutableDirectivesUseRegisteredFunctionDefinitions(string name) {
    var directive = Assert.Single(DirectiveLibrary.Enumerate().Where(item => item.Name == name));
    Assert.True(FunctionLibrary.TryGet(name, directive.ArgumentCount, out var function));

    Assert.Same(function, directive.Function);
    Assert.Equal(function.ArgumentTypes, directive.ArgumentTypes);
    Assert.False(string.IsNullOrWhiteSpace(function.Documentation));
  }

  [Fact]
  public void LanguageDefinitionsResolveFixedAndVariadicSignatures() {
    Assert.True(FunctionLibrary.TryGet("format", 1, out var formatOne));
    Assert.True(FunctionLibrary.TryGet("format", 2, out var formatTwo));
    Assert.False(FunctionLibrary.TryGet("format", 3, out _));
    Assert.NotSame(formatOne, formatTwo);

    Assert.True(FunctionLibrary.TryGet("and", 1, out var and));
    Assert.True(FunctionLibrary.TryGet("and", 3, out _));
    Assert.True(and.IsVariadic);
    Assert.Equal([MixinLanguageValueKind.Boolean, MixinLanguageValueKind.Boolean], and.ArgumentTypes);

    Assert.True(DirectiveLibrary.TryGet("CALL", 1, out _));
    Assert.True(DirectiveLibrary.TryGet("CALL", 2, out _));
    Assert.False(DirectiveLibrary.TryGet("CALL", 0, out _));
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
    Assert.Equal(definitions, MixinRootLibrary.Enumerate());
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
    Assert.True(programs.SelectMany(Descendants)
      .OfType<DirectiveArgumentAst>()
      .Count(argument => argument.ArgumentMetadata is not null) > 100);
  }

  private static IEnumerable<MixinAst> Descendants(MixinAst node) {
    foreach (var child in node.Children) {
      yield return child;
      foreach (var descendant in Descendants(child)) yield return descendant;
    }
  }
}
