using System.Collections.Generic;
using System.Linq;
using HELIX.Compose;
using UnityEditor;
using UnityEngine;

namespace HELIX.Editor {
  public sealed class CompositionIdRegistryInspectorWindow : HelixHierarchyInspectorWindow {
    protected override string EmptyMessage => "The composition ID registry is empty.";
    protected override bool SupportsLiveRefresh => true;

    [MenuItem("Window/HELIX/Inspectors/Composition ID Registry", false, 1011)]
    private static void ShowWindow() {
      var window = GetWindow<CompositionIdRegistryInspectorWindow>();
      window.titleContent = new GUIContent("HELIX Composition IDs");
      window.Show();
    }

    protected override List<InspectorTreeNode> ReadNodes() {
      var compositions = CompositionIdRegistry.CompositionIds.Select((entry, index) => {
        var id = index + 1;
        var name = DisplayName(entry.name);
        return new InspectorTreeNode($"registry:composition:{id}", $"[{id}] {name}", entry.location,
          new[] { Detail("Kind", "Composition ID"), Detail("ID", id), Detail("Name", name), Detail("Location", entry.location) });
      }).ToList();
      var types = CompositionIdRegistry.TypeIds.Select((entry, index) => {
        var id = index + 1;
        var name = DisplayName(entry.name);
        return new InspectorTreeNode($"registry:type:{id}", $"[{id}] {name}", null,
          new[] { Detail("Kind", "Type ID"), Detail("ID", id), Detail("Name", name) });
      }).ToList();
      return new List<InspectorTreeNode> {
        new("registry:compositions", "Composition IDs", $"{compositions.Count} ID(s)", new[] { Detail("Count", compositions.Count) }, compositions),
        new("registry:types", "Type IDs", $"{types.Count} ID(s)", new[] { Detail("Count", types.Count) }, types)
      };
    }

    private static string DisplayName(string value) => string.IsNullOrEmpty(value) ? "<unnamed>" : value;
  }
}