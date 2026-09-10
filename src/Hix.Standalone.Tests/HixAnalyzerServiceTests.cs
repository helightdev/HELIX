using System.Linq;
using Hix.Standalone.Analysis;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class HixAnalyzerServiceTests {
  [Fact]
  public void CatalogIncludesBackendFileMetadata() {
    var backend = Assert.Single(new HixAnalyzerService().Definitions.Where(item => item.Name == "backend"));

    Assert.Equal("FileMetadata", backend.Kind);
    Assert.Equal(new[] {"String"}, backend.ArgumentTypes);
    var definition = Assert.Single(HixCoreBackend.Instance.Functions.EnumerateAll()
      .Where(item => item.Name == "backend"));
    Assert.Equal(HixMetadataKind.File, definition.Metadata);
    Assert.DoesNotContain(HixCoreBackend.Instance.Functions.Enumerate(), item => item.Name == "backend");
  }

  [Fact]
  public void DefinitionQueriesAreContextualAndDoNotDependOnDocumentRanges() {
    var service = new HixAnalyzerService();

    Assert.All(service.QueryDefinitions("FileMetadata", "", "", ""),
      definition => Assert.Equal("FileMetadata", definition.Kind));
    Assert.All(service.QueryDefinitions("Function", "String", "", ""), definition =>
      Assert.True(definition.ReceiverType is "String" or "Any"));
    Assert.Empty(service.QueryDefinitions("Root", "", "", "definitelyMissing"));
  }

  [Fact]
  public void ResolvesSiblingPatternsAndKeepsCompletionsLazy() {
    var service = new HixAnalyzerService();
    var snapshots = service.Synchronize([
      new HixDocument("/workspace/Patterns.hix", "type Person = @{string name}", 1),
      new HixDocument("/workspace/Use.hix",
        "func name(Person value) -> string { return(param#value#name) }", 1)
    ]);

    var use = snapshots.Single(item => item.Path.EndsWith("Use.hix"));
    Assert.Contains(use.References, item => item is {Name: "Person", Kind: "Pattern"} &&
      item.Target?.Path.EndsWith("Patterns.hix") == true);
    Assert.Empty(service.Complete("CSharpType", "Str"));
  }

  [Fact]
  public void SynchronizeOwnsRevisionsCacheAndDirectoryMembership() {
    var service = new HixAnalyzerService();
    service.Synchronize([
      new HixDocument("/workspace/A.hix", "type A = string", 1),
      new HixDocument("/workspace/B.hix", "type B = string", 1)
    ]);

    var snapshots = service.Synchronize([
      new HixDocument("/workspace/A.hix", "type A = string", 2)
    ]);

    Assert.Equal(2, Assert.Single(snapshots).Revision);
    Assert.Null(service.Get("/workspace/B.hix"));
  }

  [Fact]
  public void IncompleteHeaderDoesNotTaintFollowingDeclarations() {
    var service = new HixAnalyzerService();
    var snapshot = Assert.Single(service.Synchronize([
      new HixDocument("/workspace/A.hix", "%\n---\ntype A = string", 1)
    ]));

    Assert.Contains(snapshot.Declarations, item => item is {Name: "A", Kind: "Pattern"});
  }
}
