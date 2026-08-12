using System.Collections.Generic;
using System.Linq;

namespace HELIX.Editor {
  public sealed class InspectorHierarchyColumn {
    public readonly string key;
    public readonly string title;
    public readonly int width;
    public readonly bool visibleByDefault;

    public InspectorHierarchyColumn(string key, string title, int width, bool visibleByDefault = false) {
      this.key = key;
      this.title = title;
      this.width = width;
      this.visibleByDefault = visibleByDefault;
    }
  }

  public sealed class InspectorTreeNode {
    public readonly string key;
    public readonly string label;
    public readonly string summary;
    public readonly List<KeyValuePair<string, string>> details;
    public readonly List<InspectorTreeNode> children;
    public readonly Dictionary<string, string> columns;
    public readonly object payload;

    public InspectorTreeNode(
      string key,
      string label,
      string summary = null,
      IEnumerable<KeyValuePair<string, string>> details = null,
      IEnumerable<InspectorTreeNode> children = null,
      IEnumerable<KeyValuePair<string, string>> columns = null,
      object payload = null
    ) {
      this.key = key;
      this.label = label;
      this.summary = summary;
      this.details = details?.ToList() ?? new List<KeyValuePair<string, string>>();
      this.children = children?.ToList() ?? new List<InspectorTreeNode>();
      this.columns = columns?.ToDictionary(entry => entry.Key, entry => entry.Value) ??
                     new Dictionary<string, string>();
      this.payload = payload;
    }

    public string GetColumn(string key) {
      return key == "summary" ? summary : columns.TryGetValue(key, out var value) ? value : null;
    }
  }
}