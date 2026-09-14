using System.Linq;
using Hix.Standalone.Analysis;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class HixAnalyzerServiceTests {
  [Fact]
  public void CatalogCarriesParameterNamesDefaultsAndGenericBounds() {
    var definitions = new HixAnalyzerService(new HixStandaloneBackend()).Definitions;
    var cast = Assert.Single(definitions.Where(item => item.Kind == "Function" && item.Name == "cast"));
    Assert.Equal(new[] {"value", "target"}, cast.ArgumentNames);
    Assert.Contains("TValue", cast.TypeParameters);
    var spawn = Assert.Single(definitions.Where(item => item.Kind == "Function" && item.Name == "spawn"));
    Assert.Equal(new[] {"function", "arguments", "useThreadPool"}, spawn.ArgumentNames);
    Assert.Equal(new[] {false, true, true}, spawn.OptionalArguments);
    Assert.Equal(new[] {"", "@[]", "false"}, spawn.ArgumentDefaults);
  }

  [Fact]
  public void CallFactsExposeResolvedGenericsAndExtractedTraitFields() {
    const string source = "trait type Named = @{string name}; type User = %trait<Named> @{number id}; " +
      "func inspect() -> string { local user = User(id = 1, Named = Named(name = <Ada>)) " +
      "local component = as($user, Named) local string explicit = <Ada> return($component#name) }";
    var snapshot = Assert.Single(new HixAnalyzerService().Synchronize([new HixDocument("/tests/inspect.hix", source, 1)]));
    Assert.Empty(snapshot.Diagnostics);
    var extract = Assert.Single(snapshot.TypeFacts.Where(fact => fact.Kind == "Call" &&
      source.Substring(fact.Range.Start, fact.Range.Length) == "as"));
    Assert.Equal("{}?", extract.CallSignature.ResultType);
    Assert.Equal(new[] {"value", "target"}, extract.CallSignature.ArgumentNames);
    var constructor = Assert.Single(snapshot.TypeFacts.Where(fact => fact.Kind == "Call" &&
      source.Substring(fact.Range.Start, fact.Range.Length) == "Named"));
    Assert.Equal("{}", constructor.Type);
    Assert.Equal(new[] {"name"}, constructor.CallSignature.ArgumentNames);
    Assert.Contains(snapshot.TypeFacts, fact => fact.Kind == "Expression" &&
      source.Substring(fact.Range.Start, fact.Range.Length) == "$component" &&
      fact.MemberNames.SequenceEqual(new[] {"name"}) && fact.MemberTypes.SequenceEqual(new[] {"string"}));
    Assert.DoesNotContain(snapshot.TypeFacts, fact => fact.Inlay && fact.Kind == "Type" &&
      source.Substring(fact.Range.Start, fact.Range.Length) == "explicit");
  }

  [Fact]
  public void PrimitivePatternsReplaceKindDefinitionsInTheCatalog() {
    var definitions = new HixAnalyzerService().Definitions;
    Assert.DoesNotContain(definitions, item => item.Name == "kind" || item.Kind == "Kind");
    Assert.Contains(definitions, item => item is {Name: "string", Kind: "PrimitivePattern", ResultType: "pattern"});
    Assert.Contains(definitions, item => item is {Name: "pattern", Kind: "PrimitivePattern", ResultType: "pattern"});
    Assert.Contains(definitions, item => item is {Name: "ComparisonOperator", Kind: "Type"});
  }

  [Fact]
  public void CatalogIncludesBackendFileMetadata() {
    var backend = Assert.Single(new HixAnalyzerService().Definitions.Where(item => item.Name == "backend"));

    Assert.Equal("FileMetadata", backend.Kind);
    Assert.Equal(new[] {"string"}, backend.ArgumentTypes);
    var definition = Assert.Single(HixCoreBackend.Instance.Functions.EnumerateAll()
      .Where(item => item.Name == "backend"));
    Assert.Equal(HixMetadataKind.File, definition.Metadata);
    Assert.DoesNotContain(HixCoreBackend.Instance.Functions.Enumerate(), item => item.Name == "backend");
  }

  [Fact]
  public void TestBackendCatalogIncludesTestMetadata() {
    var definitions = new HixAnalyzerService(new HixTestBackend()).Definitions;
    Assert.Contains(definitions, item => item is {Name: "test", Kind: "DeclarationMetadata"});
    Assert.Equal(2, definitions.Count(item => item is {Name: "testcase", Kind: "DeclarationMetadata"}));
    Assert.DoesNotContain(definitions, item => item.Name == "syntaxError");
  }

  [Fact]
  public void DefinitionQueriesAreContextualAndDoNotDependOnDocumentRanges() {
    var service = new HixAnalyzerService();

    Assert.All(service.QueryDefinitions("FileMetadata", "", "", ""),
      definition => Assert.Equal("FileMetadata", definition.Kind));
    Assert.All(service.QueryDefinitions("Function", "String", "", ""), definition =>
      Assert.True(definition.ReceiverType is "string" or "any"));
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

  [Fact]
  public void SemanticDiagnosticsPreferTheOffendingSymbolOverLeadingWhitespace() {
    const string source = "func main {\n  local value = eq()\n}";
    var snapshot = Assert.Single(new HixAnalyzerService().Synchronize([
      new HixDocument("/workspace/A.hix", source, 1)
    ]));

    var diagnostic = Assert.Single(snapshot.Diagnostics.Where(item => item.Message.Contains("'eq'")));
    Assert.Equal("eq", source.Substring(diagnostic.Range.Start, diagnostic.Range.Length));
  }
}
