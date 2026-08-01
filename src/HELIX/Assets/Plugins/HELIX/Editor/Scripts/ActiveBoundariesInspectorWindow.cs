using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using HELIX.Compose;
using HELIX.Compose.Collections;
using Unity.Hierarchy;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Editor {
  public sealed class ActiveBoundariesInspectorWindow : HelixHierarchyInspectorWindow {
    private static readonly IReadOnlyList<InspectorHierarchyColumn> _columns = new[] {
      new InspectorHierarchyColumn("cid", "ID", 280, true),
      new InspectorHierarchyColumn("runtimeType", "Type", 220),
      new InspectorHierarchyColumn("depth", "Depth", 55),
      new InspectorHierarchyColumn("dirty", "Dirty", 55),
      new InspectorHierarchyColumn("disposal", "Disposal", 75),
      new InspectorHierarchyColumn("ussFlags", "USS Flags", 160)
    };
    private sealed class Identity { public readonly int id; public Identity(int id) { this.id = id; } }
    private class ElementPayload {
      public readonly WeakReference<VisualElement> element;
      public ElementPayload(VisualElement element) { this.element = new WeakReference<VisualElement>(element); }
    }
    private sealed class BoundaryPayload : ElementPayload {
      public BoundaryPayload(VisualElement element) : base(element) { }
    }

    private readonly List<IBoundary> _boundaries = new();
    private readonly ReferenceEqualityComparer<IBoundary> _comparer = new();
    private readonly ReferenceEqualityComparer<IPanel> _panelComparer = new();
    private readonly ConditionalWeakTable<IBoundary, Identity> _boundaryIds = new();
    private readonly ConditionalWeakTable<IPanel, Identity> _panelIds = new();
    private readonly ConditionalWeakTable<VisualElement, Identity> _elementIds = new();
    private int _nextBoundaryId = 1;
    private int _nextPanelId = 1;
    private int _nextElementId = 1;
    private VisualElement _highlightedElement;

    protected override string EmptyMessage => "No active boundaries.";
    protected override bool SupportsLiveRefresh => true;
    protected override bool SupportsComposableTree => true;
    protected override IReadOnlyList<InspectorHierarchyColumn> HierarchyColumns => _columns;

    protected override void AddToolbarButtons(Toolbar toolbar) {
      toolbar.Add(new ToolbarButton(RefreshCurrentBoundaries) {
        text = "Refresh Current",
        tooltip = "Refresh values for the boundaries already listed without re-enumerating active boundaries."
      });
    }

    [MenuItem("Window/HELIX/Inspectors/Active Boundaries", false, 1012)]
    private static void ShowWindow() {
      var window = GetWindow<ActiveBoundariesInspectorWindow>();
      window.titleContent = new GUIContent("HELIX Boundaries");
      window.Show();
    }

    protected override List<InspectorTreeNode> ReadNodes() {
      _boundaries.Clear();
      _boundaries.AddRange(HX.Boundaries);
      return BuildNodes(_boundaries);
    }

    private void RefreshCurrentBoundaries() {
      RefreshNodes(BuildNodes(_boundaries));
    }

    private List<InspectorTreeNode> BuildNodes(IEnumerable<IBoundary> source) {
      var boundaries = source.ToList();
      var panels = new Dictionary<IPanel, List<IBoundary>>(_panelComparer);
      var detached = new List<IBoundary>();
      foreach (var boundary in boundaries) {
        var panel = boundary.Element?.panel;
        if (panel == null) detached.Add(boundary);
        else {
          if (!panels.TryGetValue(panel, out var list)) panels.Add(panel, list = new List<IBoundary>());
          list.Add(boundary);
        }
      }
      var visited = new HashSet<IBoundary>(_comparer);
      var nodes = panels.Select(entry => PanelNode(entry.Key, entry.Value, visited)).OrderBy(node => node.label).ToList();
      if (detached.Count > 0) nodes.Add(BoundaryGroup("panel:detached", "Detached", detached, visited, null));
      return nodes;
    }

    private InspectorTreeNode PanelNode(IPanel panel, List<IBoundary> boundaries, HashSet<IBoundary> visited) {
      var name = GetPanelName(panel);
      return BoundaryGroup(PanelKey(panel), name, boundaries, visited,
        new[] { Detail("Panel", name), Detail("Panel Type", panel.GetType().FullName), Detail("Boundaries", boundaries.Count) });
    }

    private InspectorTreeNode BoundaryGroup(string key, string label, List<IBoundary> boundaries,
      HashSet<IBoundary> visited, IEnumerable<KeyValuePair<string, string>> details) {
      var active = new HashSet<IBoundary>(boundaries, _comparer);
      var children = boundaries.ToDictionary(boundary => boundary, _ => new List<IBoundary>(), _comparer);
      foreach (var boundary in boundaries)
        if (boundary.Parent != null && active.Contains(boundary.Parent)) children[boundary.Parent].Add(boundary);
      var roots = boundaries.Where(boundary => boundary.Parent == null || !active.Contains(boundary.Parent)).ToList();
      roots.Sort(CompareBoundaries);
      var childNodes = roots.Select(root => ToNode(root, children, visited)).ToList();
      foreach (var boundary in boundaries) if (!visited.Contains(boundary)) childNodes.Add(ToNode(boundary, children, visited));
      return new InspectorTreeNode(key, label, $"{boundaries.Count} boundary/boundaries",
        details ?? new[] { Detail("Boundaries", boundaries.Count) }, childNodes);
    }

    private InspectorTreeNode ToNode(IBoundary boundary, Dictionary<IBoundary, List<IBoundary>> children,
      HashSet<IBoundary> visited) {
      var key = BoundaryKey(boundary);
      if (!visited.Add(boundary)) return new InspectorTreeNode(key + ":cycle", "<parent cycle>");
      var descendants = children[boundary];
      descendants.Sort(CompareBoundaries);
      var element = boundary.Element;
      var name = string.IsNullOrEmpty(element?.name) ? boundary.GetType().Name : element.name;
      var boundaryComposable = boundary is CompositionBoundaryNodeBase { BoundaryComposable: { } composable }
        ? composable
        : null;
      var hierarchyName = GetDisplayTypeName(boundary.PackedId, boundaryComposable, element, boundary);
      var composableType = boundaryComposable == null ? null : SimpleTypeName(boundaryComposable.GetType());
      var dirty = HXComposer.IsBoundaryDirty(boundary);
      var pendingDisposal = HXComposer.IsBoundaryPendingDisposal(boundary);
      var disposal = boundary.IsDisposed ? "Disposed" : pendingDisposal ? "Pending" : "Active";
      var ussFlags = FormatUssFlags(boundary.Flag);
      var id = new CompositionId { packed = boundary.PackedId };
      var formatted = FormatCompositionId(id);
      var childNodes = ShowComposables
        ? BuildBoundaryChildren(boundary, children, descendants, visited)
        : descendants.Where(child => !visited.Contains(child)).Select(child => ToNode(child, children, visited)).ToList();
      return new InspectorTreeNode(key, hierarchyName, formatted, new[] {
        Detail("Name", name), Detail("Runtime Type", boundary.GetType().FullName), Detail("Element Type", element?.GetType().FullName),
        Detail("Boundary Composable", composableType),
        Detail("Tree Depth", boundary.TreeDepth), Detail("Dirty", dirty), Detail("Disposal", disposal),
        Detail("USS Dirty Flags", ussFlags),
        Detail("ID", formatted), Detail("Packed ID", boundary.PackedId)
      }, childNodes, new[] {
        Detail("cid", formatted), Detail("runtimeType", hierarchyName), Detail("depth", boundary.TreeDepth),
        Detail("dirty", dirty), Detail("disposal", disposal), Detail("ussFlags", ussFlags)
      }, element == null ? null : new BoundaryPayload(element));
    }

    protected override void OnSelectedNodeChanged(InspectorTreeNode previous, InspectorTreeNode current) {
      var next = GetElement(current);
      if (ReferenceEquals(_highlightedElement, next)) return;
      if (_highlightedElement != null) {
        _highlightedElement.generateVisualContent -= DrawHighlight;
        _highlightedElement.MarkDirtyRepaint();
      }
      _highlightedElement = next;
      if (next != null) {
        next.generateVisualContent += DrawHighlight;
        next.MarkDirtyRepaint();
      }
    }

    protected override void AddDetailActions(InspectorTreeNode data, VisualElement target) {
      var element = GetElement(data);
      if (element == null) return;
      var actions = new VisualElement();
      actions.style.flexDirection = FlexDirection.Row;
      actions.style.marginLeft = 10;
      actions.style.marginBottom = 10;
      actions.Add(new Button(() => UIElementsDebuggerBridge.Open(element)) { text = "Open in UI Toolkit Debugger" });
      target.Add(actions);
    }

    private List<InspectorTreeNode> BuildBoundaryChildren(IBoundary boundary,
      Dictionary<IBoundary, List<IBoundary>> boundaryChildren, List<IBoundary> descendants,
      HashSet<IBoundary> visitedBoundaries) {
      var nodes = new List<InspectorTreeNode>();
      var visitedElements = new HashSet<VisualElement>(new ReferenceEqualityComparer<VisualElement>());
      var element = boundary.Element;
      if (element != null) {
        for (var index = 0; index < element.childCount; index++) {
          var node = BuildComposableNode(element.ElementAt(index), boundaryChildren, visitedBoundaries, visitedElements);
          if (node != null) nodes.Add(node);
        }
      }

      // A boundary can be logically parented without being a visual child. Keep those branches visible.
      foreach (var descendant in descendants)
        if (!visitedBoundaries.Contains(descendant)) nodes.Add(ToNode(descendant, boundaryChildren, visitedBoundaries));
      return nodes;
    }

    private InspectorTreeNode BuildComposableNode(VisualElement element,
      Dictionary<IBoundary, List<IBoundary>> boundaryChildren, HashSet<IBoundary> visitedBoundaries,
      HashSet<VisualElement> visitedElements) {
      if (element == null || !visitedElements.Add(element)) return null;

      if (element is IBoundary boundary && boundaryChildren.ContainsKey(boundary))
        return visitedBoundaries.Contains(boundary) ? null : ToNode(boundary, boundaryChildren, visitedBoundaries);

      var composable = GetComposable(element);
      var boundaryComposable = element is CompositionBoundaryNodeBase boundaryElement
        ? boundaryElement.BoundaryComposable
        : null;
      var packedId = composable?.PackedId ?? 0;
      var displayName = GetDisplayTypeName(packedId, boundaryComposable, element, composable);
      var formattedId = composable == null ? null : FormatCompositionId(new CompositionId { packed = packedId });
      var ussFlags = composable == null ? null : FormatUssFlags(composable.Flag);
      var children = new List<InspectorTreeNode>();
      if (element is not IBoundary) {
        for (var index = 0; index < element.childCount; index++) {
          var child = BuildComposableNode(element.ElementAt(index), boundaryChildren, visitedBoundaries, visitedElements);
          if (child != null) children.Add(child);
        }
      }
      return new InspectorTreeNode(ElementKey(element), displayName, formattedId, new[] {
        Detail("Element Type", element.GetType().FullName),
        Detail("Composable Type", composable == null ? null : SimpleTypeName(composable.GetType())),
        Detail("USS Dirty Flags", ussFlags),
        Detail("ID", formattedId)
      }, children, new[] {
        Detail("cid", formattedId), Detail("runtimeType", displayName), Detail("ussFlags", ussFlags)
      }, new ElementPayload(element));
    }

    private static IComposable GetComposable(VisualElement element) =>
      element as IComposable ?? element.userData as IComposable;

    private static string FormatUssFlags(UssFlag flags) => flags.ToString();

    private static string GetDisplayTypeName(ulong cid, object contextComposable, VisualElement element,
      IComposable composable) {
      var id = new CompositionId { packed = cid };
      var registeredName = CompositionIdRegistry.GetTypeName(id.type);
      if (!string.IsNullOrEmpty(registeredName)) return registeredName;
      if (contextComposable != null) return SimpleTypeName(contextComposable.GetType());
      if (composable != null && !ReferenceEquals(composable, element)) return SimpleTypeName(element?.GetType());
      return SimpleTypeName(composable?.GetType()) ?? SimpleTypeName(element?.GetType()) ?? "<unknown>";
    }

    private static string SimpleTypeName(Type type) {
      if (type == null) return null;
      var name = type.Name;
      var genericArity = name.IndexOf('`');
      return genericArity < 0 ? name : name.Substring(0, genericArity);
    }

    private static VisualElement GetElement(InspectorTreeNode node) =>
      node?.payload is ElementPayload payload && payload.element.TryGetTarget(out var element) ? element : null;

    private void DrawHighlight(MeshGenerationContext context) {
      if (_highlightedElement == null) return;
      var rect = _highlightedElement.contentRect;
      if (rect.width <= 0 || rect.height <= 0) return;
      rect.xMin++; rect.yMin++; rect.xMax--; rect.yMax--;
      var painter = context.painter2D;
      painter.strokeColor = new Color(0.1f, 0.85f, 1f, 1f);
      painter.lineWidth = 2;
      painter.BeginPath();
      painter.MoveTo(new Vector2(rect.xMin, rect.yMin)); painter.LineTo(new Vector2(rect.xMax, rect.yMin));
      painter.LineTo(new Vector2(rect.xMax, rect.yMax)); painter.LineTo(new Vector2(rect.xMin, rect.yMax));
      painter.ClosePath(); painter.Stroke();
    }

    private static string FormatCompositionId(CompositionId id) {
      var builder = new StringBuilder();
      var type = CompositionIdRegistry.GetTypeName(id.type);
      var composition = CompositionIdRegistry.GetCompositionName(id.composition);

      builder.Append(id.local.depth);
      builder.Append(":");
      builder.Append(id.local.index);
      builder.Append(" ");

      if (!string.IsNullOrEmpty(type)) {
        builder.Append(type);
        builder.Append("(");
        builder.Append(id.type);
        builder.Append(")");
      } else {
        builder.Append("T");
        builder.Append(id.type);
      }
      builder.Append(" ");

      if (!string.IsNullOrEmpty(composition)) {
        builder.Append(composition);
        builder.Append("(");
        builder.Append(id.composition);
        builder.Append(")");
      } else {
        builder.Append("C");
        builder.Append(id.composition);
      }

      return builder.ToString();
    }

    private string BoundaryKey(IBoundary boundary) =>
      $"boundary:{_boundaryIds.GetValue(boundary, _ => new Identity(_nextBoundaryId++)).id}";
    private string PanelKey(IPanel panel) => $"panel:{_panelIds.GetValue(panel, _ => new Identity(_nextPanelId++)).id}";
    private string ElementKey(VisualElement element) =>
      $"element:{_elementIds.GetValue(element, _ => new Identity(_nextElementId++)).id}";

    private static string GetPanelName(IPanel panel) {
      try {
        var property = panel.GetType().GetProperty("ownerObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property?.GetValue(panel) is UnityEngine.Object owner && owner != null) return $"{owner.name} ({owner.GetType().Name})";
      } catch { }
      return string.IsNullOrEmpty(panel.visualTree?.name) ? panel.GetType().Name : panel.visualTree.name;
    }

    private static int CompareBoundaries(IBoundary left, IBoundary right) {
      var depth = left.TreeDepth.CompareTo(right.TreeDepth);
      return depth != 0 ? depth : string.Compare(left.GetType().FullName, right.GetType().FullName, StringComparison.Ordinal);
    }
  }
}
