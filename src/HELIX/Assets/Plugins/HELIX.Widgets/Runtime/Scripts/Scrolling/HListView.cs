using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling {
  /// <summary>
  /// A widget that efficiently renders a large number of children in a vertical list.
  /// </summary>
  /// <remarks>Wraps Unity's <see cref="ListView"/>.</remarks>
  public class HListView : Widget {
    public readonly BuildFunction<int> builder;
    public readonly int count;
    public readonly float fixedItemHeight;
    public readonly ScrollController scrollController;

    /// <summary>
    /// Creates a widget that efficiently renders a large number of children in a vertical list.
    /// </summary>
    /// <param name="builder">The function that builds a child widget for a given index.</param>
    /// <param name="count">The number of items in the list.</param>
    /// <param name="fixedItemHeight">
    /// If set to a non-negative value, the height of each item will be fixed to this value, improving performance.
    /// If set to a negative value, the height will be dynamically determined for each item.
    /// </param>
    /// <param name="scrollController">
    /// The <see cref="ScrollController"/> to use for this list.
    /// If not specified, a controller will be created and managed by the widget's state.
    /// </param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    public HListView(
      BuildFunction<int> builder,
      int count,
      float fixedItemHeight = -1,
      ScrollController scrollController = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants) {
      this.fixedItemHeight = fixedItemHeight;
      this.builder = builder;
      this.count = count;
      this.scrollController = scrollController;

      DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new HListViewElement());
    }
  }

  public class HListViewElement : WidgetBaseElement<HListView>, IHierarchyDisposable {
    private readonly DummyCounterList _dummyList = new();
    private readonly ListView _listView;
    private readonly ScrollPosition _scrollPosition;
    private readonly ScrollView _scrollView;
    private ScrollController _scrollController;

    public HListViewElement() {
      _listView = new ListView().AddTo(this);
      _scrollView = _listView.Q<ScrollView>();
      _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
      _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
      _scrollView.contentContainer.userData = new ElementTreeAncestorTraversalHint(this);
      _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
      _listView.makeItem = () => new WidgetHostElement().TightStretch().Sized(100.Percent());
      _listView.destroyItem = element => element.Clear();
      _listView.bindItem = BindItem;
      _listView.unbindItem = UnbindItem;
      _listView.allowAdd = false;
      _listView.allowRemove = false;
      _listView.selectionType = SelectionType.None;
      _scrollPosition = new ScrollerScrollPosition(_scrollView.verticalScroller, _scrollView);
    }

    public void Dispose() {
      _scrollPosition.Dispose();
    }

    protected override void OnAttached(AttachToPanelEvent evt) {
      base.OnAttached(evt);
      _scrollController?.Attach(_scrollPosition);
    }

    protected override void OnDetached(DetachFromPanelEvent evt) {
      base.OnDetached(evt);
      _scrollController?.Detach(_scrollPosition);
    }

    private void UnbindItem(VisualElement elem, int index) {
      var host = elem as WidgetHostElement;
      if (host == null) return;
      host.Buildable = null;
      ModificationBarrier.Rebuild(host);
    }

    private void BindItem(VisualElement elem, int index) {
      var host = elem as WidgetHostElement;
      var typed = TypedDescriptor;
      if (host == null || typed == null) return;
      host.style.backgroundColor = Color.clear;
      if (index < 0 || index >= typed.count) {
        host.Buildable = null;
        ModificationBarrier.Rebuild(host);
        return;
      }

      var buildable = new ParameterizedFunctionBuildable<int>(typed.builder, index);
      host.Buildable = buildable;
      ModificationBarrier.Rebuild(host);
    }

    public override void Apply(HListView previous, HListView widget) {
      base.Apply(previous, widget);

      if (_dummyList.Count != widget.count) {
        _dummyList.Count = widget.count;
        _listView.itemsSource = _dummyList;
      }

      if (widget.fixedItemHeight >= 0) {
        _listView.fixedItemHeight = widget.fixedItemHeight;
        _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
      } else
        _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

      _listView.RefreshItems();

      if (widget.scrollController != _scrollController) {
        _scrollController?.Detach(_scrollPosition);
        widget.scrollController?.Attach(_scrollPosition);
        _scrollController = widget.scrollController;
      }
    }
  }
}