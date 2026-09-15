using System;
using System.IO;
using System.Linq;
using Hix.Standalone.Analysis;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class HalGraphTests : IDisposable {
  private readonly string root = Path.Combine(Path.GetTempPath(), "hal-graph-" + Guid.NewGuid().ToString("N"));
  private readonly HalGraphService service = new();
  public HalGraphTests() {
    Directory.CreateDirectory(Path.Combine(root, "assets"));
    File.WriteAllText(Path.Combine(root, "manifest.hal"), "--- Manifest\nname = graph\nsources = {assets=[\"**\"], main = [\"*.hix\"], test = [], modules = [] }\n");
    File.WriteAllText(Path.Combine(root, "types.hix"), "type Item = @{string name};\ntype Asset = @{Item item, %many<Item> tuple items, %optional string note};\n");
  }
  private HalGraphSnapshot Open(string text = "--- Asset\n// keep this comment\nitem = Item {name = \"first\"}\nitems = []\n\n--- Item &second\nname = \"second\"\n") {
    var path = Path.Combine(root, "assets", "asset.hal"); File.WriteAllText(path, text); return service.Open(path);
  }
  private static string Supplier(HalGraphSnapshot doc, string port) => doc.Edges.Single(e => e.Target == "section/0" && e.TargetPort == port).Source;
  [Fact] public void DiscoversWorkspaceAndProjectsNestedObjectsListsAndMissingFields() {
    var doc = Open();
    Assert.Equal(root, doc.Workspace); Assert.Empty(doc.Diagnostics);
    Assert.Contains(doc.Types, t => t.Name == "Asset");
    Assert.Contains(doc.Nodes, n => n.Kind == "list");
    Assert.Contains(doc.Nodes.Single(n => n.Id == "section/0").Ports, p => p.Name == "note" && p.Optional && p.Text == null);
  }
  [Fact] public void EditsPreserveTriviaAndSupportRevisionsUndoRedoAndSave() {
    var doc = Open(); var before = doc.Text;
    doc = service.Edit(doc.Session, doc.Revision, new("set", Supplier(doc, "item"), "name", "\"edited\""));
    Assert.Contains("// keep this comment", doc.Text); Assert.True(doc.Dirty);
    Assert.Throws<InvalidOperationException>(() => service.Change(doc.Session, 1, ""));
    doc = service.Undo(doc.Session, doc.Revision); Assert.Equal(before, doc.Text);
    doc = service.Undo(doc.Session, doc.Revision, true); Assert.Contains("edited", doc.Text);
    doc = service.Save(doc.Session, doc.Revision); Assert.False(doc.Dirty); Assert.Equal(doc.Text, File.ReadAllText(doc.Path));
    File.AppendAllText(doc.Path, "\n// external edit");
    Assert.Throws<InvalidOperationException>(() => service.Save(doc.Session, doc.Revision));
  }
  [Fact] public void ConnectsReferencesAndListItemsAndRejectsInvalidTypes() {
    var doc = Open();
    doc = service.Edit(doc.Session, doc.Revision, new("connect", "section/0", "item", Source: "section/second"));
    Assert.Contains("ref(\"second\")", doc.Text); Assert.Empty(doc.Diagnostics);
    doc = service.Edit(doc.Session, doc.Revision, new("connect", Supplier(doc, "items"), "+", Source: "section/second"));
    Assert.Empty(doc.Diagnostics); Assert.Contains(doc.Edges, e => e.Source == "section/second" && e.TargetPort == "0");
    Assert.Throws<ArgumentException>(() => service.Edit(doc.Session, doc.Revision, new("connect", "section/0", "note", Source: "section/second")));
    Assert.Throws<ArgumentException>(() => service.Edit(doc.Session, doc.Revision, new("remove", "section/second")));
  }
  [Fact] public void MissingFieldsAndTrailingCommaListsAreEditable() {
    var doc = Open();
    doc = service.Edit(doc.Session, doc.Revision, new("set", "section/0", "note", "\"hi\""));
    Assert.Empty(doc.Diagnostics); Assert.Contains("\"note\" = \"hi\"", doc.Text);
    doc = service.Edit(doc.Session, doc.Revision, new("set", "section/0", "items", "[Item {name = a},]"));
    doc = service.Edit(doc.Session, doc.Revision, new("connect", Supplier(doc, "items"), "+", Source: "section/second"));
    Assert.Empty(doc.Diagnostics);
  }
  [Fact] public void InvalidSourceCanBeRecoveredWithoutLosingHistory() {
    var doc = Open(); doc = service.Change(doc.Session, doc.Revision, "---");
    Assert.NotEmpty(doc.Diagnostics); Assert.Empty(doc.Nodes);
    doc = service.Undo(doc.Session, doc.Revision); Assert.Empty(doc.Diagnostics);
    service.Close(doc.Session, doc.Revision); Assert.Throws<ArgumentException>(() => service.Get(doc.Session));
  }
  [Fact] public void InspectorRejectsAdditionalFieldsAndSections() {
    var doc = Open();
    Assert.Throws<ArgumentException>(() => service.Edit(doc.Session, doc.Revision, new("set", "section/0", "note", "hello\nextra = 1")));
    Assert.Equal(doc.Text, service.Get(doc.Session).Text);
  }
  [Fact] public void RemovesListItemsWithCommasOrNewlines() {
    foreach (var value in new[] { "[Item {name = a}, Item {name = b},]", "[Item {name = a}\nItem {name = b}]" }) {
      var doc = Open();
      doc = service.Edit(doc.Session, doc.Revision, new("set", "section/0", "items", value));
      doc = service.Edit(doc.Session, doc.Revision, new("removeItem", Supplier(doc, "items"), "0"));
      Assert.Empty(doc.Diagnostics);
      doc = service.Edit(doc.Session, doc.Revision, new("removeItem", Supplier(doc, "items"), "0"));
      Assert.Empty(doc.Diagnostics);
      Assert.Single(doc.Nodes.Single(n => n.Id == Supplier(doc, "items")).Ports);
    }
  }
  [Fact] public void RefreshesTypesAndPrefersNearestManifest() {
    var doc = Open();
    File.AppendAllText(Path.Combine(root, "types.hix"), "type Extra = @{number count};");
    Assert.Contains(service.Get(doc.Session).Types, t => t.Name == "Extra");
    var nested = Path.Combine(root, "assets");
    File.WriteAllText(Path.Combine(nested, "manifest.hal"), "--- Manifest\nname = inner\nsources = {assets=[\"**\"],main = [\"*.hix\"], test = [], modules = []}\n");
    File.WriteAllText(Path.Combine(nested, "local.hix"), "type Local = @{number count};");
    var next = service.Get(doc.Session);
    Assert.Equal(nested, next.Workspace);
    Assert.Contains(next.Types, t => t.Name == "Local"); Assert.DoesNotContain(next.Types, t => t.Name == "Extra");
  }
  [Fact] public void ExtractsEveryContainerIntoASectionAndKeepsSharedReferences() {
    var doc = Open();
    Assert.True(doc.Dirty); Assert.True(doc.CanUndo);
    Assert.All(doc.Nodes, node => Assert.NotNull(node.SectionId));
    Assert.All(doc.Nodes, node => Assert.StartsWith("section/", node.Id));
    Assert.Equal(doc.Nodes.Count, Hix.Hal.HalDocumentSyntax.Parse(doc.Text).Sections.Count);
    var item = Supplier(doc, "item");
    doc = service.Edit(doc.Session, doc.Revision, new("connect", Supplier(doc, "items"), "+", Source: item));
    Assert.Equal(2, doc.Edges.Count(e => e.Source == item));
    doc = service.Edit(doc.Session, doc.Revision, new("set", item, "name", "changed"));
    Assert.Equal(2, doc.Edges.Count(e => e.Source == item));
    Assert.Empty(doc.Diagnostics);
  }
  [Fact] public void CreatesPrimitiveCollectionsAndEditsTheirContents() {
    var doc = Open();
    doc = service.Edit(doc.Session, doc.Revision, new("add", Value: "table"));
    var table = doc.Nodes.Last();
    doc = service.Edit(doc.Session, doc.Revision, new("addField", table.Id, "a b", "12"));
    Assert.Contains("\"a b\" = 12", doc.Text);
    doc = service.Edit(doc.Session, doc.Revision, new("add", Value: "tuple"));
    var list = doc.Nodes.Last();
    doc = service.Edit(doc.Session, doc.Revision, new("connect", list.Id, "+", Source: table.Id));
    Assert.Empty(doc.Diagnostics); Assert.Contains(doc.Edges, e => e.Source == table.Id && e.Target == list.Id);
    doc = service.Save(doc.Session, doc.Revision);
    var reopened = service.Open(doc.Path); Assert.Equal(doc.Text, reopened.Text); Assert.False(reopened.Dirty);
  }
  [Fact] public void OffersSuppliersAndReceiversAndCreatesConnectedSectionsAtomically() {
    var doc = Open();
    var candidates = service.Candidates(doc.Session, doc.Revision, "section/0", "item", "supplier");
    Assert.Contains(candidates, c => c.Type == "Item"); Assert.DoesNotContain(candidates, c => c.Type == "tuple");
    var before = doc.Text;
    doc = service.Edit(doc.Session, doc.Revision, new("createConnected", "section/0", "item", "Item", Direction: "supplier"));
    Assert.Contains(doc.Nodes, n => n.Id == Supplier(doc, "item"));
    doc = service.Undo(doc.Session, doc.Revision); Assert.Equal(before, doc.Text);
    candidates = service.Candidates(doc.Session, doc.Revision, "section/second", null, "receiver");
    Assert.Contains(candidates, c => c.Type == "Asset" && c.Port == "item");
    Assert.DoesNotContain(candidates, c => c.Type == "Asset" && c.Port == "note");
    doc = service.Edit(doc.Session, doc.Revision, new("createConnected", "section/second", Value: "Asset", Direction: "receiver", ReceiverPort: "item"));
    Assert.Contains(doc.Edges, e => e.Source == "section/second" && e.Target == doc.Nodes.Last().Id);
    Assert.Throws<InvalidOperationException>(() => service.Candidates(doc.Session, 1, "section/0", "item", "supplier"));
  }
  [Fact] public void RejectsCyclesAndInvalidCreationWithoutChangingTheSession() {
    var doc = Open("--- table &first\nx = null\n--- table &second\nx = null\n");
    doc = service.Edit(doc.Session, doc.Revision, new("connect", "section/first", "x", Source: "section/second"));
    Assert.Throws<ArgumentException>(() => service.Edit(doc.Session, doc.Revision, new("connect", "section/second", "x", Source: "section/first")));
    Assert.Throws<ArgumentException>(() => service.Edit(doc.Session, doc.Revision, new("createConnected", "section/first", "x", "invalid", Direction: "supplier")));
    Assert.Equal(doc.Text, service.Get(doc.Session).Text);
  }
  [Fact] public void PersistsPositionsAsMetadataAndSupportsUndoAndReopen() {
    var doc = Open(); var before = doc.Text;
    doc = service.Edit(doc.Session, doc.Revision, new("move", "section/0", X: -125.5, Y: 42.25));
    Assert.Contains("%graphPosition(x=-125.5, y=42.25)", doc.Text);
    Assert.Equal(new HalGraphPosition(-125.5, 42.25), doc.Nodes.Single(n => n.Id == "section/0").Position);
    doc = service.Edit(doc.Session, doc.Revision, new("move", "section/0", X: 200, Y: 300));
    Assert.Single(Hix.Hal.HalDocumentSyntax.Parse(doc.Text).Sections.First().Metadata.Where(m => m.Name == "graphPosition"));
    doc = service.Save(doc.Session, doc.Revision);
    var reopened = service.Open(doc.Path); Assert.Equal(new HalGraphPosition(200, 300), reopened.Nodes.First().Position);
    doc = service.Undo(doc.Session, doc.Revision); doc = service.Undo(doc.Session, doc.Revision);
    Assert.Equal(before, doc.Text);
    Assert.Throws<ArgumentException>(() => service.Edit(doc.Session, doc.Revision, new("move", "section/0", X: double.NaN, Y: 0)));
  }
  [Fact] public void AppliesMultiNodeLayoutAsOneUndoStepWithoutRemovingOtherMetadata() {
    var doc = Open("%custom(1)\n--- table &first\nx = 1\n%custom(2)\n--- tuple &second\n[]\n"); var before = doc.Text;
    doc = service.Edit(doc.Session, doc.Revision, new("layout", Positions: new[] { new HalGraphPlacement("section/first", 1, 2), new HalGraphPlacement("section/second", 3, 4) }));
    Assert.Contains("%custom(1)", doc.Text); Assert.Contains("%custom(2)", doc.Text);
    Assert.All(doc.Nodes, n => Assert.NotNull(n.Position));
    doc = service.Undo(doc.Session, doc.Revision); Assert.Equal(before, doc.Text);
  }
  public void Dispose() => Directory.Delete(root, true);
}
