using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Compose;
using Unity.Hierarchy;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Editor {
  public abstract class HelixHierarchyInspectorWindow : EditorWindow {
    public sealed class InspectorNodeTypeHandler : HierarchyNodeTypeHandler {
      public InspectorNodeTypeHandler() { }

      public bool Add(in HierarchyNode parent, out HierarchyNode node) => CommandList.Add(in parent, out node);
      public bool Remove(in HierarchyNode node) => CommandList.Remove(in node);

      public bool SetParent(in HierarchyNode node, in HierarchyNode parent, int index) =>
        CommandList.SetParent(in node, in parent, index);

      public bool SetName(in HierarchyNode node, string name) => CommandList.SetName(in node, name);
    }

    private static readonly IReadOnlyList<InspectorHierarchyColumn> _defaultColumns = new[] {
      new InspectorHierarchyColumn("summary", "Summary", 260, true)
    };

    private readonly Dictionary<string, HierarchyNode> _nodesByKey = new();
    private readonly Dictionary<HierarchyNode, InspectorTreeNode> _dataByNode = new();
    private readonly Dictionary<HierarchyViewCell, Label> _labelsByCell = new();
    private Hierarchy _hierarchy;
    private InspectorNodeTypeHandler _nodeHandler;
    private HierarchyView _hierarchyView;
    private VisualElement _detailsView;
    private Label _summaryLabel;
    private ToolbarSearchField _search;
    private ToolbarToggle _liveToggle;
    private ToolbarToggle _composablesToggle;
    private IVisualElementScheduledItem _scheduledRefresh;
    private string _selectedKey;
    private InspectorTreeNode _selectedData;

    protected abstract string EmptyMessage { get; }
    protected virtual bool SupportsLiveRefresh => false;
    protected virtual bool SupportsComposableTree => false;
    protected bool ShowComposables => _composablesToggle?.value ?? false;
    protected virtual IReadOnlyList<InspectorHierarchyColumn> HierarchyColumns => _defaultColumns;
    protected abstract List<InspectorTreeNode> ReadNodes();
    protected virtual void AddToolbarButtons(Toolbar toolbar) { }

    protected virtual void OnDisable() {
      _scheduledRefresh?.Pause();
      SetSelectedData(null);
      _hierarchyView?.Dispose();
      _hierarchyView = null;
      _hierarchy?.Dispose();
      _hierarchy = null;
      _nodeHandler = null;
      _nodesByKey.Clear();
      _dataByNode.Clear();
      _labelsByCell.Clear();
    }

    public void CreateGUI() {
      var toolbar = new Toolbar();
      toolbar.Add(new ToolbarButton(RefreshFromSource) { text = "Refresh" });
      AddToolbarButtons(toolbar);
      if (SupportsLiveRefresh) {
        _liveToggle = new ToolbarToggle { text = "Live", value = false };
        _liveToggle.RegisterValueChangedCallback(OnLiveChanged);
        toolbar.Add(_liveToggle);
      }
      if (SupportsComposableTree) {
        _composablesToggle = new ToolbarToggle { text = "Composables", value = false };
        _composablesToggle.RegisterValueChangedCallback(_ => RefreshFromSource());
        toolbar.Add(_composablesToggle);
      }
      toolbar.Add(new ToolbarSpacer());
      _search = new ToolbarSearchField();
      _search.RegisterValueChangedCallback(evt => ApplySearchFilter(evt.newValue));
      toolbar.Add(_search);
      _summaryLabel = new Label {
        style = {
          marginLeft = 6,
          marginRight = 6,
          alignSelf = Align.Center
        }
      };
      toolbar.Add(_summaryLabel);
      rootVisualElement.Add(toolbar);

      var split = new TwoPaneSplitView(1, 500, TwoPaneSplitViewOrientation.Horizontal).Flexible();
      rootVisualElement.Add(split);

      var hierarchyPane = new VisualElement().WithStyle(style => style.flexGrow = 1);

      _hierarchy = new Hierarchy();
      _nodeHandler = _hierarchy.GetOrCreateNodeTypeHandler<InspectorNodeTypeHandler>();
      _hierarchyView = new HierarchyView().Flexible();
      _hierarchyView.SetSourceHierarchy(_hierarchy);
      _hierarchyView.SetColumnDescriptors(BuildColumnDescriptors(), BuildCellDescriptors(), null);
      _hierarchyView.FlagsChanged += OnHierarchyFlagsChanged;
      _hierarchyView.BindViewItem += OnBindViewItem;
      _hierarchyView.RegisterCallback<KeyDownEvent>(OnHierarchyKeyDown);
      hierarchyPane.Add(_hierarchyView);
      split.Add(hierarchyPane);

      _detailsView = new ScrollView().WithStyle(style => style.flexGrow = 1);
      split.Add(_detailsView);

      RefreshFromSource();
      if (SupportsLiveRefresh) {
        _scheduledRefresh = rootVisualElement.schedule.Execute(RefreshFromSource).Every(500);
        _scheduledRefresh.Pause();
      }
    }

    private IEnumerable<HierarchyViewColumnDescriptor> BuildColumnDescriptors() {
      var priority = 1;
      foreach (var column in HierarchyColumns) {
        yield return new HierarchyViewColumnDescriptor(column.key) {
          Title = column.title,
          Tooltip = column.title,
          DefaultPriority = priority++,
          DefaultWidth = column.width,
          DefaultVisibility = column.visibleByDefault
        };
      }
    }

    private IEnumerable<HierarchyViewCellDescriptor> BuildCellDescriptors() {
      foreach (var column in HierarchyColumns) {
        yield return new HierarchyViewCellDescriptor(column.key, typeof(InspectorNodeTypeHandler)) {
          ClearCellContent = true,
          BindCell = cell => BindCell(cell, column.key),
          UnbindCell = cell => _labelsByCell.Remove(cell)
        };
      }
    }

    private void BindCell(HierarchyViewCell cell, string columnKey) {
      var label = new Label();
      label.style.flexGrow = 1;
      label.style.unityTextAlign = TextAnchor.MiddleLeft;
      label.style.overflow = Overflow.Hidden;
      label.style.color = new Color(0.62f, 0.62f, 0.62f);
      cell.Add(label);
      _labelsByCell[cell] = label;
      UpdateCell(cell, label, columnKey);
    }

    private void UpdateBoundCells() {
      foreach (var entry in _labelsByCell.ToList())
        UpdateCell(entry.Key, entry.Value, entry.Key.Descriptor.ColumnId);
    }

    private void UpdateCell(HierarchyViewCell cell, Label label, string columnKey) {
      var text = _dataByNode.TryGetValue(cell.Node, out var data) ? data.GetColumn(columnKey) : null;
      label.text = text;
      cell.IsDefaultValue = string.IsNullOrEmpty(text);
    }

    private void OnBindViewItem(HierarchyView view, HierarchyViewItem item) {
      var node = item.Node;
      if (!_dataByNode.TryGetValue(node, out var data)) return;
      item.Name.text = data.label;
      item.Icon.Display(false);
      item.tooltip = string.IsNullOrEmpty(data.summary) ? data.label : $"{data.label}\n{data.summary}";
    }

    private void OnHierarchyFlagsChanged(HierarchyView view, HierarchyNodeFlags flags) {
      if ((flags & HierarchyNodeFlags.Selected) == 0) return;
      UpdateSelectionFromView();
    }

    private void OnHierarchyKeyDown(KeyDownEvent evt) {
      if (evt.keyCode is not (KeyCode.UpArrow or KeyCode.DownArrow or KeyCode.Home or KeyCode.End or
        KeyCode.PageUp or KeyCode.PageDown)) return;
      var keyCode = evt.keyCode;
      var selectedKey = _selectedKey;
      // Keyboard navigation is applied by HierarchyView after event callbacks have run.
      // Read its selection on the next UI pass so the details pane follows the outlined row.
      _hierarchyView.schedule.Execute(() => {
          UpdateSelectionFromView();
          if (_selectedKey == selectedKey) MoveSelection(keyCode);
        }
      );
    }

    private void UpdateSelectionFromView() {
      InspectorTreeNode selected = null;
      if (TryGetSelectedNode(out var node)) _dataByNode.TryGetValue(node, out selected);
      _selectedKey = selected?.key;
      SetSelectedData(selected);
    }

    private bool TryGetSelectedNode(out HierarchyNode selected) {
      foreach (var node in _hierarchyView.ViewModel.EnumerateNodesWithFlags(HierarchyNodeFlags.Selected)) {
        selected = node;
        return true;
      }
      selected = HierarchyNode.Null;
      return false;
    }

    private void MoveSelection(KeyCode keyCode) {
      if (!TryGetSelectedNode(out var selected)) return;
      var model = _hierarchyView.ViewModel;
      var index = model.IndexOf(in selected);
      if (index < 0 || model.Count == 0) return;
      var nextIndex = keyCode switch {
        KeyCode.UpArrow => index - 1,
        KeyCode.DownArrow => index + 1,
        KeyCode.Home => 0,
        KeyCode.End => model.Count - 1,
        KeyCode.PageUp => index - 10,
        KeyCode.PageDown => index + 10,
        _ => index
      };
      nextIndex = Mathf.Clamp(nextIndex, 0, model.Count - 1);
      if (nextIndex == index) return;
      var next = model[nextIndex];
      _hierarchyView.SetSelection(in next);
      _hierarchyView.Frame(in next);
      UpdateSelectionFromView();
    }

    private void OnLiveChanged(ChangeEvent<bool> evt) {
      if (evt.newValue) {
        RefreshFromSource();
        _scheduledRefresh?.Resume();
      } else {
        _scheduledRefresh?.Pause();
      }
    }

    private void ApplySearchFilter(string filter) {
      if (_hierarchyView == null) return;
      _hierarchyView.Filter = filter ?? string.Empty;
      _hierarchyView.Update();
    }

    protected void RefreshFromSource() {
      if (_hierarchyView == null) return;
      RefreshNodes(ReadNodes());
    }

    protected void RefreshNodes(List<InspectorTreeNode> nodes) {
      if (_hierarchyView == null) return;
      SynchronizeHierarchy(nodes);
      _summaryLabel.text = nodes.Count == 0 ? EmptyMessage : $"{CountNodes(nodes)} item(s)";
      if (_selectedKey != null && _nodesByKey.TryGetValue(_selectedKey, out var selectedNode) &&
          _dataByNode.TryGetValue(selectedNode, out var selectedData)) {
        SetSelectedData(selectedData);
      } else if (_selectedKey != null) {
        _selectedKey = null;
        SetSelectedData(null);
      }
    }

    private void SynchronizeHierarchy(IEnumerable<InspectorTreeNode> nodes) {
      var desired = new HashSet<string>();
      _dataByNode.Clear();
      var root = _hierarchy.Root;
      var index = 0;
      foreach (var data in nodes) SynchronizeNode(in root, data, index++, desired);

      foreach (var stale in _nodesByKey.Where(entry => !desired.Contains(entry.Key)).ToList()) {
        var node = stale.Value;
        if (_hierarchy.Exists(in node)) _nodeHandler.Remove(in node);
        _nodesByKey.Remove(stale.Key);
      }
      _hierarchy.Update();
      _hierarchyView.Update();
      UpdateBoundCells();
    }

    private void SynchronizeNode(in HierarchyNode parent, InspectorTreeNode data, int index, HashSet<string> desired) {
      desired.Add(data.key);
      if (!_nodesByKey.TryGetValue(data.key, out var node) || !_hierarchy.Exists(in node)) {
        if (!_nodeHandler.Add(in parent, out node))
          throw new InvalidOperationException("Could not add an inspector hierarchy node.");
        _nodesByKey[data.key] = node;
      }
      // Data must be available before the native view binds the node's cell descriptors.
      _dataByNode[node] = data;
      _nodeHandler.SetParent(in node, in parent, index);
      _nodeHandler.SetName(in node, data.label);
      for (var childIndex = 0; childIndex < data.children.Count; childIndex++) {
        SynchronizeNode(in node, data.children[childIndex], childIndex, desired);
      }
    }

    private void SetSelectedData(InspectorTreeNode data) {
      var previous = _selectedData;
      _selectedData = data;
      OnSelectedNodeChanged(previous, data);
      if (_detailsView != null) ShowDetails(data);
    }

    protected virtual void OnSelectedNodeChanged(InspectorTreeNode previous, InspectorTreeNode current) { }
    protected virtual void AddDetailActions(InspectorTreeNode data, VisualElement target) { }

    private void ShowDetails(InspectorTreeNode data) {
      _detailsView.Clear();
      if (data == null) {
        var empty = new Label("Select an item to inspect its details.");
        empty.style.marginLeft = 10;
        empty.style.marginTop = 10;
        _detailsView.Add(empty);
        return;
      }
      var heading = new Label(data.label);
      heading.style.unityFontStyleAndWeight = FontStyle.Bold;
      heading.style.fontSize = 14;
      heading.style.marginLeft = 10;
      heading.style.marginTop = 10;
      heading.style.marginBottom = 8;
      _detailsView.Add(heading);
      AddDetailActions(data, _detailsView);
      foreach (var detail in data.details) AddDetailRow(detail.Key, detail.Value);
      if (data.details.Count == 0 && !string.IsNullOrEmpty(data.summary)) AddDetailRow("Summary", data.summary);
    }

    private void AddDetailRow(string name, string value) {
      var row = new VisualElement();
      row.style.flexDirection = FlexDirection.Row;
      row.style.marginLeft = 10;
      row.style.marginRight = 10;
      row.style.marginBottom = 4;
      var nameLabel = new Label(name);
      nameLabel.style.minWidth = 120;
      nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
      row.Add(nameLabel);
      var valueLabel = new Label(value ?? "<null>");
      valueLabel.style.flexGrow = 1;
      valueLabel.style.whiteSpace = WhiteSpace.Normal;
      row.Add(valueLabel);
      _detailsView.Add(row);
    }

    private static int CountNodes(IEnumerable<InspectorTreeNode> nodes) {
      return nodes.Sum(node => 1 + CountNodes(node.children));
    }

    protected static KeyValuePair<string, string> Detail(string name, object value) {
      return new KeyValuePair<string, string>(name, value?.ToString() ?? "<null>");
    }

    protected static string SafeToString(object value) {
      if (value == null) return "<null>";
      try { return value.ToString(); } catch (Exception exception) {
        return $"<ToString threw {exception.GetType().Name}>";
      }
    }
  }
}
