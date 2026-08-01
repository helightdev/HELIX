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
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Editor {
  public sealed class ActiveBoundariesInspectorWindow : HelixHierarchyInspectorWindow {
    private static readonly IReadOnlyList<InspectorHierarchyColumn> _columns = new[] {
      new InspectorHierarchyColumn("cid", "ID", 280, true),
      new InspectorHierarchyColumn("runtimeType", "Type", 220),
      new InspectorHierarchyColumn("depth", "Depth", 55),
      new InspectorHierarchyColumn("dirty", "Dirty", 55),
      new InspectorHierarchyColumn("pendingDisposal", "Pending Disposal", 110)
    };
    private sealed class Identity { public readonly int id; public Identity(int id) { this.id = id; } }
    private sealed class BoundaryPayload {
      public readonly WeakReference<VisualElement> element;
      public BoundaryPayload(VisualElement element) { this.element = new WeakReference<VisualElement>(element); }
    }
    private sealed class ComposableSubtreeNode {
      public readonly string key;
      public readonly string label;
      public readonly string composableType;
      public readonly string cid;
      public readonly List<ComposableSubtreeNode> children;

      public ComposableSubtreeNode(string key, string label, string composableType, string cid,
        IEnumerable<ComposableSubtreeNode> children) {
        this.key = key;
        this.label = label;
        this.composableType = composableType;
        this.cid = cid;
        this.children = children?.ToList() ?? new List<ComposableSubtreeNode>();
      }
    }

    private static readonly IReadOnlyList<InspectorHierarchyColumn> _composableColumns = new[] {
      new InspectorHierarchyColumn("composableType", "Composable Type", 260, true),
      new InspectorHierarchyColumn("cid", "Type ID", 180, true)
    };

    private readonly List<IBoundary> _boundaries = new();
    private readonly ReferenceEqualityComparer<IBoundary> _comparer = new();
    private readonly ReferenceEqualityComparer<IPanel> _panelComparer = new();
    private readonly ConditionalWeakTable<IBoundary, Identity> _boundaryIds = new();
    private readonly ConditionalWeakTable<IPanel, Identity> _panelIds = new();
    private readonly ConditionalWeakTable<VisualElement, Identity> _elementIds = new();
    private readonly Dictionary<string, HierarchyNode> _composableNodesByKey = new();
    private readonly Dictionary<HierarchyNode, ComposableSubtreeNode> _composableDataByNode = new();
    private readonly Dictionary<HierarchyViewCell, Label> _composableLabelsByCell = new();
    private int _nextBoundaryId = 1;
    private int _nextPanelId = 1;
    private int _nextElementId = 1;
    private VisualElement _highlightedElement;
    private Hierarchy _composableHierarchy;
    private InspectorNodeTypeHandler _composableNodeHandler;
    private HierarchyView _composableHierarchyView;

    protected override string EmptyMessage => "No active boundaries.";
    protected override bool SupportsLiveRefresh => true;
    protected override IReadOnlyList<InspectorHierarchyColumn> HierarchyColumns => _columns;

    protected override void OnDisable() {
      _composableHierarchyView?.Dispose();
      _composableHierarchyView = null;
      _composableHierarchy?.Dispose();
      _composableHierarchy = null;
      _composableNodeHandler = null;
      _composableNodesByKey.Clear();
      _composableDataByNode.Clear();
      _composableLabelsByCell.Clear();
      base.OnDisable();
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
      var panels = new Dictionary<IPanel, List<IBoundary>>(_panelComparer);
      var detached = new List<IBoundary>();
      foreach (var boundary in _boundaries) {
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
      var disposal = HXComposer.IsBoundaryPendingDisposal(boundary);
      var id = new CompositionId { packed = boundary.PackedId };
      var formatted = FormatCompositionId(id);
      var childNodes = descendants.Where(child => !visited.Contains(child)).Select(child => ToNode(child, children, visited)).ToList();
      return new InspectorTreeNode(key, hierarchyName, formatted, new[] {
        Detail("Name", name), Detail("Runtime Type", boundary.GetType().FullName), Detail("Element Type", element?.GetType().FullName),
        Detail("Boundary Composable", composableType),
        Detail("Tree Depth", boundary.TreeDepth), Detail("Dirty", dirty), Detail("Pending Disposal", disposal),
        Detail("Disposed", boundary.IsDisposed), Detail("ID", formatted), Detail("Packed ID", boundary.PackedId),
        Detail("Composition ID", id.composition), Detail("Type ID", id.type), Detail("Local ID", id.local)
      }, childNodes, new[] {
        Detail("cid", formatted), Detail("runtimeType", hierarchyName), Detail("depth", boundary.TreeDepth),
        Detail("dirty", dirty), Detail("pendingDisposal", disposal)
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
      AddComposableSubtree(data, target);
    }

    private void AddComposableSubtree(InspectorTreeNode data, VisualElement target) {
      if (GetElement(data) is not IBoundary boundary) return;

      var title = new Label("Composable Subtree");
      title.style.unityFontStyleAndWeight = FontStyle.Bold;
      title.style.marginLeft = 10;
      title.style.marginTop = 8;
      title.style.marginBottom = 4;
      target.Add(title);

      EnsureComposableHierarchyView();
      SynchronizeComposableHierarchy(BuildComposableSubtree(boundary));
      target.Add(_composableHierarchyView);
    }

    private void EnsureComposableHierarchyView() {
      if (_composableHierarchyView != null) return;

      _composableHierarchy = new Hierarchy();
      _composableNodeHandler = _composableHierarchy.GetOrCreateNodeTypeHandler<InspectorNodeTypeHandler>();
      _composableHierarchyView = new HierarchyView();
      _composableHierarchyView.style.height = 220;
      _composableHierarchyView.style.marginLeft = 10;
      _composableHierarchyView.style.marginRight = 10;
      _composableHierarchyView.style.marginBottom = 10;
      _composableHierarchyView.SetSourceHierarchy(_composableHierarchy, HierarchyNodeFlags.Expanded);
      _composableHierarchyView.SetColumnDescriptors(BuildComposableColumnDescriptors(), BuildComposableCellDescriptors(), null);
      _composableHierarchyView.BindViewItem += OnBindComposableViewItem;
    }

    private static IEnumerable<HierarchyViewColumnDescriptor> BuildComposableColumnDescriptors() {
      var priority = 1;
      foreach (var column in _composableColumns) {
        yield return new HierarchyViewColumnDescriptor(column.key) {
          Title = column.title,
          Tooltip = column.title,
          DefaultPriority = priority++,
          DefaultWidth = column.width,
          DefaultVisibility = column.visibleByDefault
        };
      }
    }

    private IEnumerable<HierarchyViewCellDescriptor> BuildComposableCellDescriptors() {
      foreach (var column in _composableColumns) {
        yield return new HierarchyViewCellDescriptor(column.key, typeof(InspectorNodeTypeHandler)) {
          ClearCellContent = true,
          BindCell = cell => BindComposableCell(cell, column.key),
          UnbindCell = cell => _composableLabelsByCell.Remove(cell)
        };
      }
    }

    private void BindComposableCell(HierarchyViewCell cell, string columnKey) {
      var label = new Label {
        style = {
          flexGrow = 1,
          unityTextAlign = TextAnchor.MiddleLeft,
          overflow = Overflow.Hidden,
          color = new Color(0.62f, 0.62f, 0.62f)
        }
      };
      cell.Add(label);
      _composableLabelsByCell[cell] = label;
      UpdateComposableCell(cell, label, columnKey);
    }

    private void OnBindComposableViewItem(HierarchyView view, HierarchyViewItem item) {
      if (_composableDataByNode.TryGetValue(item.Node, out var data)) item.Name.text = data.label;
    }

    private void SynchronizeComposableHierarchy(ComposableSubtreeNode rootData) {
      var desired = new HashSet<string>();
      _composableDataByNode.Clear();
      if (rootData != null) {
        var root = _composableHierarchy.Root;
        SynchronizeComposableNode(in root, rootData, 0, desired);
      }

      foreach (var stale in _composableNodesByKey.Where(entry => !desired.Contains(entry.Key)).ToList()) {
        var node = stale.Value;
        if (_composableHierarchy.Exists(in node)) _composableNodeHandler.Remove(in node);
        _composableNodesByKey.Remove(stale.Key);
      }
      _composableHierarchy.Update();
      _composableHierarchyView.Update();
      UpdateComposableCells();
    }

    private void SynchronizeComposableNode(in HierarchyNode parent, ComposableSubtreeNode data, int index,
      HashSet<string> desired) {
      desired.Add(data.key);
      if (!_composableNodesByKey.TryGetValue(data.key, out var node) || !_composableHierarchy.Exists(in node)) {
        if (!_composableNodeHandler.Add(in parent, out node))
          throw new InvalidOperationException("Could not add a composable subtree node.");
        _composableNodesByKey[data.key] = node;
      }
      _composableDataByNode[node] = data;
      _composableNodeHandler.SetParent(in node, in parent, index);
      _composableNodeHandler.SetName(in node, data.label);
      for (var childIndex = 0; childIndex < data.children.Count; childIndex++)
        SynchronizeComposableNode(in node, data.children[childIndex], childIndex, desired);
    }

    private void UpdateComposableCells() {
      foreach (var entry in _composableLabelsByCell.ToList())
        UpdateComposableCell(entry.Key, entry.Value, entry.Key.Descriptor.ColumnId);
    }

    private void UpdateComposableCell(HierarchyViewCell cell, Label label, string columnKey) {
      var text = _composableDataByNode.TryGetValue(cell.Node, out var data)
        ? columnKey == "composableType" ? data.composableType : data.cid
        : null;
      label.text = text;
      cell.IsDefaultValue = string.IsNullOrEmpty(text);
    }

    private ComposableSubtreeNode BuildComposableSubtree(IBoundary boundary) {
      var visited = new HashSet<VisualElement>(new ReferenceEqualityComparer<VisualElement>());
      return BuildComposableSubtree(boundary.Element, true, visited);
    }

    private ComposableSubtreeNode BuildComposableSubtree(VisualElement element, bool isRoot,
      HashSet<VisualElement> visited) {
      if (element == null || !visited.Add(element)) return null;
      var composable = GetComposable(element);
      var boundaryComposable = element is CompositionBoundaryNodeBase boundary
        ? boundary.BoundaryComposable
        : null;
      var packedId = composable?.PackedId ?? 0;
      var composableType = GetDisplayTypeName(packedId, boundaryComposable, element, composable);
      var compositionId = composable == null ? null : FormatCompositionId(new CompositionId { packed = packedId });
      var label = composableType;
      var children = new List<ComposableSubtreeNode>();
      if (isRoot || element is not IBoundary) {
        for (var index = 0; index < element.childCount; index++) {
          var child = BuildComposableSubtree(element.ElementAt(index), false, visited);
          if (child != null) children.Add(child);
        }
      }
      return new ComposableSubtreeNode(ElementKey(element), label, composableType, compositionId, children);
    }

    private static IComposable GetComposable(VisualElement element) =>
      element as IComposable ?? element.userData as IComposable;

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
      node?.payload is BoundaryPayload payload && payload.element.TryGetTarget(out var element) ? element : null;

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