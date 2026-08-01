using System.Collections.Generic;
using System.Linq;
using HELIX.Compose;
using UnityEditor;
using UnityEngine;

namespace HELIX.Editor {
  public sealed class ContextKeysInspectorWindow : HelixHierarchyInspectorWindow {
    private readonly List<KeyValuePair<int, ContextKeyData>> _keys = new();
    protected override string EmptyMessage => "No active context keys.";

    [MenuItem("Window/HELIX/Inspectors/Context Keys", false, 1010)]
    private static void ShowWindow() {
      var window = GetWindow<ContextKeysInspectorWindow>();
      window.titleContent = new GUIContent("HELIX Context Keys");
      window.Show();
    }

    protected override List<InspectorTreeNode> ReadNodes() {
      _keys.Clear();
      ContextKeyData.CollectRegistry(_keys);
      var named = _keys.Where(entry => entry.Key > 0).OrderBy(entry => entry.Key).Select(ToNode).ToList();
      var anonymous = _keys.Where(entry => entry.Key < 0).OrderByDescending(entry => entry.Key).Select(ToNode).ToList();
      return new List<InspectorTreeNode> { Group("context:named", "Named", named), Group("context:anonymous", "Anonymous", anonymous) };
    }

    private static InspectorTreeNode Group(string key, string name, List<InspectorTreeNode> children) {
      return new InspectorTreeNode(key, name, $"{children.Count} key(s)", new[] { Detail("Count", children.Count) }, children);
    }

    private static InspectorTreeNode ToNode(KeyValuePair<int, ContextKeyData> entry) {
      var name = string.IsNullOrEmpty(entry.Value.name) ? "<unnamed>" : entry.Value.name;
      var type = entry.Value.type?.FullName ?? "<unknown type>";
      var details = new List<KeyValuePair<string, string>> {
        Detail("ID", entry.Key), Detail("Kind", entry.Key < 0 ? "Anonymous" : "Named"),
        Detail("Name", name), Detail("Registered Type", type)
      };
      var summary = type;
      if (entry.Key < 0) {
        if (entry.Value.TryGetInstance(out var target)) {
          details.Add(Detail("Reference", "Alive"));
          details.Add(Detail("Instance Type", target.GetType().FullName));
          details.Add(Detail("Instance", SafeToString(target)));
          summary += $" · {target.GetType().Name}";
        } else {
          var state = entry.Value.reference == null ? "Not tracked" : "Collected";
          details.Add(Detail("Reference", state));
          summary += $" · {state.ToLowerInvariant()}";
        }
      }
      return new InspectorTreeNode($"context:key:{entry.Key}", $"[{entry.Key}] {name}", summary, details);
    }
  }
}