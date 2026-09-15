using System;
using System.IO;
using System.Linq;
using Hix.Runtime;
using Hix.Standalone.Analysis;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class AssetDatabaseTests : IDisposable {
  private readonly string root = Path.Combine(Path.GetTempPath(), "hix-database-" + Guid.NewGuid().ToString("N"));
  public AssetDatabaseTests() {
    Directory.CreateDirectory(root);
    Write("manifest.hal", "--- Manifest\nname = database\nsources = {assets=[\"**\"],main=[\"*.hix\"],test=[],modules=[\"modules/*\"]}\n");
    Write("types.hix", "type Item = @{string name, number count = 2};\ntype Container = @{Item item};\n");
    Write("item.hal", "--- Item &item\nname = first\n");
    Write("container.hal", "--- Container\nitem = asset(path(\"item.hal\"), \"item\")\n");
  }
  private string Write(string relative, string text) { var path = Path.Combine(root, relative); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path,text); return path; }
  [Fact] public void AssetSetsControlIndexingDirectAccessAndNestedWorkspaceOwnership() {
    Write("manifest.hal", "--- Manifest\nname = database\nsources = {main = [\"*.hix\"], assets = [\"content/**\"], modules = [\"modules/*\"]}\n");
    var included = Write("content/item.hal", "--- Item\nname = included\n");
    Write("modules/child/manifest.hal", "--- Manifest\nname = child\nsources = {assets = [\"data/**\"]}\n");
    var child = Write("modules/child/data/item.hal", "--- table\nname = child\n");
    var excluded = Write("modules/child/item.hal", "--- table\nname = excluded\n");
    Write("content/private/manifest.hal", "--- Manifest\nname = unrelated\n");
    Write("content/private/hidden.hal", "--- table\n");
    using (var db = AssetDatabase.Open(root)) {
      Assert.Equal(2, db.Refresh());
      Assert.Equal(new[] { included, child }.OrderBy(x => x), db.Query().Select(x => x.Path).OrderBy(x => x));
      Assert.Throws<ArgumentException>(() => db.Index(excluded));
      Assert.Throws<ArgumentException>(() => db.Build(Path.Combine(root, "item.hal")));
    }
    Assert.Throws<ArgumentException>(() => new HalGraphService().Open(excluded));
    Write("manifest.hal", "--- Manifest\nname = database\nsources = {assets = [], modules = []}\n");
    using var empty = AssetDatabase.Open(root);
    Assert.Equal(0, empty.Refresh()); Assert.Empty(empty.Query());
  }

  [Fact] public void LooseAssetFoldersDoNotIndexNestedManifestWorkspaces() {
    File.Delete(Path.Combine(root, "manifest.hal"));
    Write("nested/manifest.hal", "--- Manifest\nname = nested\n");
    Write("nested/assets/item.hal", "--- table\n");
    using var database = AssetDatabase.Open(root);
    Assert.Equal(2, database.Refresh());
    Assert.DoesNotContain(database.Query(), item => item.Path.Contains("nested"));
  }

  [Fact] public void EditorSynchronizationCreatesDiskCacheWithoutPersistingUnsavedBuffer() {
    var path = Path.Combine(root, "types.hix");
    var server = new HixAnalysisServer();
    var result = (System.Collections.Generic.IReadOnlyList<HixDocumentSnapshot>)server.Dispatch("hix/synchronize",
      Newtonsoft.Json.Linq.JObject.FromObject(new { documents = new[] { new HixDocument(path, "type Unsaved = string;", 9) } }));
    Assert.Equal(9, Assert.Single(result).Revision);
    Assert.True(File.Exists(Path.Combine(root, ".hix", "assets.duckdb")));
    using var database = AssetDatabase.Open(path);
    Assert.Single(database.Patterns("Item"));
    Assert.Empty(database.Patterns("Unsaved"));
    Assert.Contains(Assert.Single(result).Declarations, declaration => declaration.Name == "Unsaved");
  }
  [Fact] public void IndexPersistsIdentitiesAndSupportsTypesDeletionAndInvalidSource() {
    string id;
    using (var db = AssetDatabase.Open(root)) {
      Assert.Equal(2, db.Refresh());
      id = Assert.Single(db.Query("Item")).AssetId;
      Assert.True(File.Exists(Path.Combine(root,".hix","assets.duckdb")));
      Assert.Single(db.Query(limit:1));
    }
    using (var db = AssetDatabase.Open(root)) {
      Assert.Equal(id, Assert.Single(db.Query("Item")).AssetId);
      Write("item.hal", "---"); db.Refresh();
      Assert.NotEmpty(db.Find(Path.Combine(root,"item.hal")).Diagnostic);
      Assert.Equal(id,Assert.Single(db.Query("Item")).AssetId);
      File.Delete(Path.Combine(root,"item.hal")); db.Refresh(); Assert.Empty(db.Query("Item"));
      Assert.True(db.Find(id).Deleted);
      Write("item.hal", "--- Item &item\nname = back\n"); db.Refresh();
      Assert.Equal(id,Assert.Single(db.Query("Item")).AssetId);
    }
  }
  [Fact] public void BuildsFullValuesCachesAcrossConnectionsAndInvalidatesTransitiveDependencies() {
    string fingerprint;
    using (var db = AssetDatabase.Open(root)) {
      var build = db.Build("container.hal"); Assert.False(build.CacheHit); fingerprint = build.Fingerprint;
      using var thread = new HixCoroutine();
      var item = build.Values["0"].Select(thread,HixString.Dynamic("item"));
      Assert.Equal(2, ((NumberHixValue)item.Select(thread,HixString.Dynamic("count"))).Value);
      Assert.Single(db.Dependencies("container.hal")); Assert.Single(db.Dependencies("item.hal",true));
    }
    using (var db = AssetDatabase.Open(root)) {
      Assert.True(db.Build("container.hal").CacheHit);
      Write("item.hal", "--- Item &item\nname = changed\n");
      var changed = db.Build("container.hal"); Assert.False(changed.CacheHit); Assert.NotEqual(fingerprint,changed.Fingerprint);
      Assert.True(db.Build("container.hal").CacheHit);
      Assert.False(db.Build("container.hal", "processor-v2").CacheHit);
      Write("types.hix", "type Item = @{string name, number count = 3};\ntype Container = @{Item item};\n");
      Assert.False(db.Build("container.hal", "processor-v2").CacheHit);
    }
  }
  [Fact] public void InvalidDeletedAndCyclicDependenciesNeverReturnOldSuccess() {
    using var db = AssetDatabase.Open(root); db.Build("container.hal");
    Write("item.hal", "--- Item &item\nname = 12\n");
    Assert.ThrowsAny<Exception>(() => db.Build("container.hal"));
    Write("item.hal", "--- Item &item\nname = first\n"); Assert.True(db.Build("container.hal").CacheHit);
    File.Delete(Path.Combine(root,"item.hal")); Assert.Throws<FileNotFoundException>(() => db.Build("container.hal"));
    Write("item.hal", "--- table &item\nchild = asset(path(\"container.hal\"))\n");
    Assert.Contains("Cyclic",Assert.Throws<ArgumentException>(() => db.Build("container.hal")).Message);
    Write("item.hal", "--- Item &item\nname = first\n");
    Write("container.hal", "--- Container\nitem = asset(path(\"item.hal\"),\"missing\")\n");
    Assert.Contains("Unknown section",Assert.Throws<ArgumentException>(() => db.Build("container.hal")).Message);
  }
  [Fact] public void AnalysisPersistsCanonicalPatternsFactsAndSymbolsAndRefreshesChangedSources() {
    using (var db = AssetDatabase.Open(root)) {
      Assert.False(db.Analyze().CacheHit); Assert.Single(db.Symbols("Item"));
      Assert.Contains(db.Document(Path.Combine(root,"types.hix")).Declarations,d => d.Name == "Item");
    }
    using (var db = AssetDatabase.Open(root)) {
      Assert.True(db.Analyze().CacheHit);
      Write("types.hix", "type Changed = @{string name};\n");
      Assert.False(db.Analyze().CacheHit); Assert.Empty(db.Symbols("Item")); Assert.Single(db.Symbols("Changed"));
      File.Delete(Path.Combine(root,"types.hix")); Assert.Empty(db.Analyze().Diagnostics); Assert.Empty(db.Symbols("Changed"));
    }
  }
  [Fact] public void AssociatedModulesShareUppermostStorageButUseNearestTypes() {
    Write("modules/child/manifest.hal", "--- Manifest\nname=child\nsources={assets=[\"**\"],main=[\"*.hix\"],test=[],modules=[]}\n");
    Write("modules/child/child.hix", "type Child = @{string name};\n");
    var asset = Write("modules/child/child.hal", "--- Child\nname=child\n");
    using (var db = AssetDatabase.Open(asset)) {
      Assert.Equal(root,db.Workspace.Root); Assert.Equal(Path.GetDirectoryName(asset),db.Workspace.Directory);
      Assert.True(db.Analyze().Patterns.ContainsKey("Child")); Assert.False(db.Analyze().Patterns.ContainsKey("Item"));
      Assert.False(db.Build(asset).CacheHit);
      Assert.False(Directory.Exists(Path.Combine(root,"modules/child/.hix")));
    }
    Write("unrelated/manifest.hal", "--- Manifest\nname=unrelated\nsources={assets=[\"**\"],main=[],test=[],modules=[]}\n");
    using var unrelated = AssetDatabase.Open(Path.Combine(root,"unrelated"));
    Assert.Equal(Path.Combine(root,"unrelated"),unrelated.Workspace.Root);
  }
  [Fact] public void GraphUsesPersistentTypesAndLeavesUnsavedBuffersOutOfIndex() {
    var service = new HalGraphService(); var path = Path.Combine(root,"item.hal"); var graph = service.Open(path);
    Assert.Empty(graph.Diagnostics);
    var changed = service.Edit(graph.Session,graph.Revision,new("set","section/item","name","\"unsaved\""));
    using (var db = AssetDatabase.Open(root)) {
      db.Refresh(); Assert.Equal(AssetDatabaseHash(File.ReadAllText(path)),db.Find(path).SourceHash);
      Assert.True(db.Analyze().CacheHit);
    }
    service.Save(changed.Session,changed.Revision);
    using (var db = AssetDatabase.Open(root)) Assert.Equal(AssetDatabaseHash(changed.Text),db.Find(path).SourceHash);
    Assert.Empty(service.Open(Path.Combine(root,"container.hal")).Diagnostics);
  }
  [Fact] public void PrimaryUidIsIndexedAndReferencesSurviveMovingTheFile() {
    Write("item.hal", "%uid(\"inventory-item\")\n--- Item &item\nname=first\n");
    Write("container.hal", "--- Container\nitem=asset(uid(\"inventory-item\"))\n");
    using var db = AssetDatabase.Open(root);
    db.Refresh(); Assert.Equal("inventory-item", Assert.Single(db.Query("Item")).AssetId);
    Assert.False(db.Build("container.hal").CacheHit);
    File.Move(Path.Combine(root,"item.hal"),Path.Combine(root,"moved.hal"));
    var moved = db.Build("container.hal"); Assert.False(moved.CacheHit);
    Assert.Equal(Path.Combine(root,"moved.hal"),db.Find("inventory-item").Path);
    Assert.Equal("inventory-item",Assert.Single(db.Query("Item")).AssetId);
    Assert.True(db.Build("container.hal").CacheHit);
    Write("copy.hal",File.ReadAllText(Path.Combine(root,"moved.hal")));
    Assert.Contains("Duplicate asset uid",Assert.Throws<ArgumentException>(()=>db.Refresh()).Message);
  }
  [Theory]
  [InlineData("%uid(\"\")\n--- table\n")]
  [InlineData("%uid(12)\n--- table\n")]
  [InlineData("%uid(\"one\")\n%uid(\"two\")\n--- table\n")]
  [InlineData("--- table\n%uid(\"one\")\n--- table &second\n")]
  [InlineData("--- table\n%uid(\"one\")\nfield = value\n")]
  public void RejectsInvalidOrMisplacedUidMetadata(string source) {
    Assert.NotEmpty(Hix.Hal.HalDocumentSyntax.Parse(source).Diagnostics);
  }
  [Theory]
  [InlineData("%uid(\"one\")\n--- table\n")]
  [InlineData("%uid<one>\n--- table\n")]
  public void CanonicalHalParserExposesUid(string source) {
    var syntax=Hix.Hal.HalDocumentSyntax.Parse(source); Assert.Empty(syntax.Diagnostics); Assert.Equal("one",syntax.Uid);
  }
  [Fact] public void ResolvesTypesAcrossSourceFilesAndRejectsLegacyForeignRef() {
    Write("types.hix", "type Item = @{string name};\n");
    Write("container.hix", "type Container = @{Item item};\n");
    using var db = AssetDatabase.Open(root);
    Assert.Empty(db.Analyze().Diagnostics); Assert.False(db.Build("container.hal").CacheHit);
    Write("container.hal", "--- Container\nitem=ref(\"item.hal\",\"item\")\n");
    Assert.ThrowsAny<Exception>(()=>db.Build("container.hal"));
  }
  [Theory]
  [InlineData("asset(\"item.hal\")")]
  [InlineData("asset(path(12))")]
  [InlineData("asset(uid(\"\"))")]
  [InlineData("asset(ref(\"item\"))")]
  [InlineData("asset(path(\"item.hal\"), null)")]
  public void RejectsAmbiguousForeignAssetSelectors(string expression) {
    Write("container.hal", "--- Container\nitem=" + expression + "\n");
    using var db = AssetDatabase.Open(root); Assert.Throws<ArgumentException>(()=>db.Build("container.hal"));
  }
  [Fact] public void RemovingUidStopsUidResolutionAndUnrelatedWorkspacesAreNotIndexed() {
    Write("item.hal", "%uid(\"inventory-item\")\n--- Item &item\nname=first\n");
    Write("container.hal", "--- Container\nitem=asset(uid(\"inventory-item\"))\n");
    Write("unrelated/manifest.hal", "--- Manifest\nname=other\nsources={assets=[\"**\"],main=[],test=[],modules=[]}\n");
    Write("unrelated/other.hal", "--- table\nname=other\n");
    using var db = AssetDatabase.Open(root); Assert.Equal(2,db.Refresh()); db.Build("container.hal");
    Write("item.hal", "--- Item &item\nname=first\n");
    Assert.Contains("Unknown asset",Assert.Throws<ArgumentException>(()=>db.Build("container.hal")).Message);
    Assert.NotEqual("inventory-item",Assert.Single(db.Query("Item")).AssetId);
  }
  [Fact] public void TagsAreRepeatableDeduplicatedQueryableAndReplacedOnEdit() {
    Write("item.hal", "%tag(\"loot\")\n%tag<rare>\n%tag(\"loot\")\n--- Item &item\nname=first\n");
    using (var db=AssetDatabase.Open(root)) {
      db.Refresh(); var item=Assert.Single(db.Query(tag:"loot"));
      Assert.Equal(new[]{"loot","rare"},db.Tags(item.AssetId));
      Assert.Single(db.Query(type:"Item",tag:"rare")); Assert.Empty(db.Query(type:"Container",tag:"rare"));
    }
    using (var db=AssetDatabase.Open(root)) {
      Assert.Single(db.Query(tag:"loot"));
      Write("item.hal", "%tag(\"common\")\n--- Item &item\nname=first\n"); db.Refresh();
      Assert.Empty(db.Query(tag:"loot")); Assert.Single(db.Query(tag:"common"));
      File.Delete(Path.Combine(root,"item.hal")); db.Refresh(); Assert.Empty(db.Query(tag:"common"));
    }
  }
  [Theory]
  [InlineData("%tag()\n--- table\n")]
  [InlineData("%tag(12)\n--- table\n")]
  [InlineData("%tag(\"\")\n--- table\n")]
  [InlineData("--- table\n%tag(\"bad\")\nfield=value\n")]
  [InlineData("--- table\n%tag(\"bad\")\n--- table &second\n")]
  public void InvalidTagsHaveCanonicalDiagnostics(string source) => Assert.NotEmpty(Hix.Hal.HalDocumentSyntax.Parse(source).Diagnostics);
  [Fact] public void PatternsAndDocumentFactsHaveRelationalRowsAndSharedFileKeys() {
    using(var db=AssetDatabase.Open(root)) {
      db.Analyze(); Assert.Equal("Item",Assert.Single(db.Patterns(name:"Item")).Name);
      Assert.Single(db.Patterns(limit:1)); Assert.Empty(db.Patterns(name:"Missing"));
      Assert.All(db.Patterns(kind:"TableHixPattern"),p=>Assert.Equal("TableHixPattern",p.Kind));
    }
    using(var connection=SqlConnection()) {
      Assert.Equal(1L,Sql(connection,"SELECT count(*) FROM patterns WHERE name='Item' AND kind='TableHixPattern'"));
      Assert.Equal(0L,Sql(connection,"SELECT count(*) FROM information_schema.columns WHERE table_name IN ('symbols','diagnostics','symbol_references','type_facts','patterns','dependencies') AND column_name IN ('path','workspace')"));
      Assert.Equal(1L,Sql(connection,"SELECT count(*) FROM files"));
      Assert.True((long)Sql(connection,"SELECT count(*) FROM symbols s JOIN documents d ON d.id=s.document_id JOIN files f ON f.id=d.file_id")>0);
      Assert.Equal(0L,Sql(connection,"SELECT count(*) FROM information_schema.tables WHERE table_name='analysis'"));
    }
    using(var db=AssetDatabase.Open(root)) {
      Assert.True(db.Analyze().CacheHit);
      Assert.NotEmpty(db.Document(Path.Combine(root,"types.hix")).TypeFacts);
      Write("types.hix","type Other = @{string value};\n"); db.Analyze(); Assert.Empty(db.Patterns(name:"Item"));
    }
  }
  [Theory]
  [InlineData(1)]
  [InlineData(999)]
  public void AnyDifferentCacheVersionDiscardsAllCachedTables(int version) {
    using(var db=AssetDatabase.Open(root)) {db.Refresh(); db.Analyze(); db.Build("container.hal");}
    using(var connection=SqlConnection()) {
      Sql(connection,"UPDATE hix_schema SET version="+version);
      Sql(connection,"CREATE TABLE obsolete_cache(value INTEGER)");
    }
    using(var db=AssetDatabase.Open(root)) {
      Assert.Empty(db.Query()); Assert.Empty(db.Patterns());
      Assert.False(db.Analyze().CacheHit); Assert.False(db.Build("container.hal").CacheHit);
    }
    using(var connection=SqlConnection()) Assert.Equal(0L,Sql(connection,"SELECT count(*) FROM information_schema.tables WHERE table_name='obsolete_cache'"));
  }
  private DuckDB.NET.Data.DuckDBConnection SqlConnection() {
    var connection=new DuckDB.NET.Data.DuckDBConnection("Data Source="+Path.Combine(root,".hix/assets.duckdb"));connection.Open();return connection;
  }
  private static object Sql(DuckDB.NET.Data.DuckDBConnection connection,string sql) {using var command=connection.CreateCommand();command.CommandText=sql;return command.ExecuteScalar();}
  private static string AssetDatabaseHash(string text) { using var sha=System.Security.Cryptography.SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant(); }
  public void Dispose() { if (Directory.Exists(root)) Directory.Delete(root,true); }
}
