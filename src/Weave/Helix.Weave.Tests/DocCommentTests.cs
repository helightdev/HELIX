using System.Linq;
using Helix.Weave;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Helix.Weave.Tests;

/// <summary>
/// The labels and descriptions generated UI needs come from the documentation the source already
/// carries, so these pin down what <see cref="DocComments"/> reads out of it.
/// </summary>
public class DocCommentTests {
  private const string Source = """
    namespace Sample;

    public class Thing {
      /// <summary>How much health the thing has.</summary>
      public int health;

      /// <summary>Spawns a thing.</summary>
      /// <param name="count">How many to spawn.</param>
      /// <param name="loud">Report every <see cref="Thing"/> spawned.</param>
      public void Spawn(int count, bool loud) { }

      public int undocumented;
    }
    """;

  private static (SymbolFacts facts, INamedTypeSymbol type) Load(
    DocumentationMode mode = DocumentationMode.Parse
  ) {
    var tree = CSharpSyntaxTree.ParseText(
      Source,
      new CSharpParseOptions(LanguageVersion.Preview, mode),
      path: "sample.cs"
    );
    var compilation = CSharpCompilation.Create(
      "UserCode", new[] { tree }, Harness.References,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    var type = compilation.GetTypeByMetadataName("Sample.Thing");
    Assert.NotNull(type);
    return (new SymbolFacts(), type);
  }

  [Fact]
  public void ReadsAFieldSummary() {
    var (facts, type) = Load();
    var field = type.GetMembers("health").Single();
    Assert.Equal("How much health the thing has.", facts.Docs.Describe(field));
  }

  [Fact]
  public void ReadsAParameterFromItsMethod() {
    var (facts, type) = Load();
    var method = (IMethodSymbol)type.GetMembers("Spawn").Single();
    Assert.Equal("How many to spawn.", facts.Docs.Describe(method.Parameters[0]));
  }

  [Fact]
  public void FlattensInlineReferencesToTheirName() {
    var (facts, type) = Load();
    var method = (IMethodSymbol)type.GetMembers("Spawn").Single();
    Assert.Equal("Report every Thing spawned.", facts.Docs.Describe(method.Parameters[1]));
  }

  [Fact]
  public void AnUndocumentedMemberDescribesAsEmpty() {
    var (facts, type) = Load();
    var field = type.GetMembers("undocumented").Single();
    Assert.Equal("", facts.Docs.Describe(field));
  }

  /// <summary>
  /// Unity compiles without /doc, so Roslyn hands back no comment XML and the trivia is lexed as
  /// ordinary comments. The descriptions still have to come through.
  /// </summary>
  [Theory]
  [InlineData(DocumentationMode.None)]
  [InlineData(DocumentationMode.Parse)]
  [InlineData(DocumentationMode.Diagnose)]
  public void ReadsDocumentationInEveryParseMode(DocumentationMode mode) {
    var (facts, type) = Load(mode);
    var method = (IMethodSymbol)type.GetMembers("Spawn").Single();
    Assert.Equal("How much health the thing has.", facts.Docs.Describe(type.GetMembers("health").Single()));
    Assert.Equal("How many to spawn.", facts.Docs.Describe(method.Parameters[0]));
  }

  [Fact]
  public void SeedsTheDescriptionOntoTheDerivedProperty() {
    var (facts, type) = Load();
    var field = type.GetMembers("health").Single();
    Assert.Equal("How much health the thing has.", PropertySeed.Seed(field, facts).Description);
  }
}
