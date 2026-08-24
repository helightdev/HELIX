using System;
using System.Collections.Generic;
using HELIX.Signals;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public sealed class DynamicComposable {
    private Composable _composable;
    internal DynamicComposableController controller;
    internal long sequence;
    public readonly object key;
    public StyleFloat flex;
    public AxisConstraint size;
    public int order;

    public DynamicComposable(
      object key,
      Composable composable,
      object userData = null,
      StyleFloat? flex = null,
      int order = 0,
      AxisConstraint? constraint = null
    ) {
      this.key = key ?? throw new ArgumentNullException(nameof(key));
      Composable = composable;
      UserData = userData;
      this.flex = flex.GetValueOrDefault(StyleKeyword.Null);
      size = constraint ?? AxisConstraint.Null;
      this.order = order;
    }

    public object UserData { get; set; }

    public Composable Composable {
      get => _composable;
      set => _composable = value ?? throw new ArgumentNullException(nameof(value));
    }

    public DynamicComposableController Controller => controller;

    public static DynamicComposable Lookup(VisualElement element) {
      for (var parent = element; parent != null; parent = parent.hierarchy.parent) {
        if (parent is DynamicComposableElement found) return found.Entry;
      }
      return null;
    }
  }

  /// <summary>
  /// A visual-element-independent collection of keyed composable entries. After changing an entry,
  /// call <see cref="NotifyEntriesChanged"/> to publish the change and reapply its ordering.
  /// </summary>
  public sealed class DynamicComposableController : Signal<int> {
    private readonly List<DynamicComposable> _entries = new();
    private int _revision;
    private long _nextSequence;

    public DynamicComposableController() : base(nameof(DynamicComposableController), typeof(DynamicComposableController)) { }

    public IReadOnlyList<DynamicComposable> Entries => _entries;
    public int Count => _entries.Count;
    internal int Revision => _revision;

    public DynamicComposable AddEntry(
      object key,
      Composable composable,
      object userData = null,
      StyleFloat? flex = null,
      int order = 0,
      AxisConstraint? constraint = null
    ) => AddEntry(new DynamicComposable(key, composable, userData, flex, order, constraint));

    public DynamicComposable AddEntry(DynamicComposable entry) {
      if (entry == null) throw new ArgumentNullException(nameof(entry));
      if (entry.controller != null)
        throw new InvalidOperationException("The dynamic composable entry already belongs to a controller.");
      if (FindEntry(entry.key) != null)
        throw new ArgumentException("An entry with the same key already exists.", nameof(entry));
      entry.controller = this;
      entry.sequence = _nextSequence++;
      _entries.Add(entry);
      NotifyEntriesChanged();
      return entry;
    }

    public bool RemoveEntry(DynamicComposable entry) {
      if (entry == null || !ReferenceEquals(entry.controller, this) || !_entries.Remove(entry)) return false;
      entry.controller = null;
      NotifyEntriesChanged();
      return true;
    }

    public bool RemoveEntry(object key) {
      var entry = FindEntry(key);
      return entry != null && RemoveEntry(entry);
    }

    public DynamicComposable FindEntry(object key) {
      if (key == null) return null;
      for (var i = 0; i < _entries.Count; i++)
        if (Equals(_entries[i].key, key))
          return _entries[i];
      return null;
    }

    public void NotifyEntriesChanged() {
      _entries.Sort(CompareEntries);
      unchecked { _revision++; }
      NotifyDirty();
      NotifyObservers();
    }

    public override int PeekValue() => _revision;

    public override void SetValue(int newValue) =>
      throw new NotSupportedException("Dynamic composable state is changed through its entries.");

    public override void SetWithoutNotify(int newValue) =>
      throw new NotSupportedException("Dynamic composable state is changed through its entries.");

    public override void Dispose() {
      for (var i = 0; i < _entries.Count; i++) _entries[i].controller = null;
      _entries.Clear();
      base.Dispose();
    }

    private static int CompareEntries(DynamicComposable left, DynamicComposable right) {
      var order = left.order.CompareTo(right.order);
      return order != 0 ? order : left.sequence.CompareTo(right.sequence);
    }
  }

  /// <summary>Owns the independently composed subtree of one dynamic entry.</summary>
  public sealed class DynamicComposableElement : BoundaryVisualElement {
    private DynamicComposable _entry;
    private int _revision = -1;

    public DynamicComposable Entry => _entry;
    public object UserData => _entry?.UserData;

    public DynamicComposableElement() {
      pickingMode = PickingMode.Ignore;
    }

    internal void Bind(DynamicComposable entry, int revision) {
      if (ReferenceEquals(_entry, entry) && _revision == revision) return;
      _entry = entry;
      _revision = revision;
      style.flexGrow = entry.flex;
      MarkFlag(UssFlag.Flex);
      if (panel != null) HXComposer.MarkDirty(this, false);
    }

    public override void PerformCompose(ref Composition cx) => Compose(ref cx);

    public override void Compose(ref Composition cx) {
      if (_entry == null) return;
      cx.AUTHORING.SetId(CompositionId.Generated(_entry.key.GetHashCode()));
      _entry.Composable.Invoke(ref cx);
    }
  }

  /// <summary>
  /// Reconciles keyed boundary elements directly against a visual-element hierarchy. The helper owns
  /// materialization, ordering and removal; entry contents remain independently composed boundaries.
  /// </summary>
  public sealed class BoundaryCollectionHelper<TKey, TEntry, TElement>
  where TElement : BoundaryVisualElement {
    private readonly Dictionary<TKey, TElement> _elements;
    private readonly Dictionary<TElement, int> _orders = new();
    private readonly HashSet<TKey> _retained;
    private readonly List<TKey> _removals = new();
    private readonly Func<TEntry, TKey> _getKey;
    private readonly Func<TKey, TElement> _create;
    private readonly Action<TElement, TEntry, int> _bind;
    private readonly Comparison<VisualElement> _comparison;

    public BoundaryCollectionHelper(
      Func<TEntry, TKey> getKey,
      Func<TKey, TElement> create,
      Action<TElement, TEntry, int> bind,
      IEqualityComparer<TKey> comparer = null
    ) {
      _getKey = getKey ?? throw new ArgumentNullException(nameof(getKey));
      _create = create ?? throw new ArgumentNullException(nameof(create));
      _bind = bind ?? throw new ArgumentNullException(nameof(bind));
      _elements = new Dictionary<TKey, TElement>(comparer);
      _retained = new HashSet<TKey>(comparer);
      _comparison = CompareElements;
    }

    public int Count => _elements.Count;

    public TElement Find(TKey key) => _elements.GetValueOrDefault(key);

    public void Synchronize(VisualElement parent, IReadOnlyList<TEntry> entries, int revision) {
      if (parent == null) throw new ArgumentNullException(nameof(parent));
      if (entries == null) throw new ArgumentNullException(nameof(entries));

      _retained.Clear();
      _orders.Clear();
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        var key = _getKey(entry);
        if (!_retained.Add(key))
          throw new InvalidOperationException("A dynamic boundary collection cannot contain duplicate keys.");
        if (!_elements.TryGetValue(key, out var element)) {
          element = _create(key) ?? throw new InvalidOperationException("The boundary factory returned null.");
          _elements.Add(key, element);
        }

        _bind(element, entry, revision);
        if (!ReferenceEquals(element.parent, parent)) {
          parent.Add(element);
          element.RefreshHierarchy();
        }
        _orders[element] = i;
      }

      _removals.Clear();
      foreach (var pair in _elements) {
        if (!_retained.Contains(pair.Key)) _removals.Add(pair.Key);
      }
      for (var i = 0; i < _removals.Count; i++) {
        var key = _removals[i];
        var element = _elements[key];
        element.RemoveFromHierarchy();
        _elements.Remove(key);
      }
      parent.hierarchy.Sort(_comparison);
    }

    public void Clear() {
      foreach (var pair in _elements) pair.Value.RemoveFromHierarchy();
      _elements.Clear();
      _orders.Clear();
      _retained.Clear();
      _removals.Clear();
    }

    private int CompareElements(VisualElement left, VisualElement right) {
      var leftOrder = left is TElement leftEntry && _orders.TryGetValue(leftEntry, out var resolvedLeft)
        ? resolvedLeft
        : 0;
      var rightOrder = right is TElement rightEntry && _orders.TryGetValue(rightEntry, out var resolvedRight)
        ? resolvedRight
        : 0;
      return leftOrder.CompareTo(rightOrder);
    }
  }

  [BoundaryComposable(Extension = false)]
  internal partial class DynamicFlexGroupBoundary {
    public partial struct Props {
      public DynamicComposableController controller;
      [Prop(Axis.Vertical)] public Axis axis;
      [Prop(Justify.FlexStart)] public Justify main;
      [Prop(Align.Stretch)] public Align cross;
      [Prop(null)] public Flex? flex;
      [Prop(0f)] public float gap;
      [Prop(false)] public bool reverse;
      [Prop(false)] public bool clear;
    }

    private readonly BoundaryCollectionHelper<object, DynamicComposable, DynamicComposableElement>
      _entries = new(
        static entry => entry.key,
        static key => new DynamicComposableElement { PackedId = CompositionId.Generated(key.GetHashCode()).packed },
        static (element, entry, revision) => element.Bind(entry, revision)
      );

    protected override void OnDetach() {
      _entries.Clear();
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      if (props.controller == null) return;
      cx.SubscribeTo(props.controller);
      if (props.clear) _entries.Clear();
      FlexGroup.Of(props.axis, props.main, props.cross, reverse: props.reverse).Apply(Node);
      (props.flex ?? Flex.Null).Apply(Node);
      Node.MarkFlag(UssFlag.GroupAlign | UssFlag.Flex);
      _entries.Synchronize(Node, props.controller.Entries, props.controller.Revision);
      ApplyGap();
      cx.AUTHORING.cell.cursor = Node.childCount;
    }

    private void ApplyGap() {
      for (var i = 0; i < Node.childCount; i++) {
        var child = Node.ElementAt(i);
        if (child is DynamicComposableElement entry) {
          AxisConstraint.Initial.Apply(
            entry,
            props.axis == Axis.Horizontal ? Axis.Vertical : Axis.Horizontal
          );
          entry.Entry.size.Apply(entry, props.axis);
          entry.MarkFlag(UssFlag.Size);
        }
        var hasGap = i > 0;
        child.style.marginTop = props.axis == Axis.Vertical && !props.reverse && hasGap ? props.gap : 0f;
        child.style.marginBottom = props.axis == Axis.Vertical && props.reverse && hasGap ? props.gap : 0f;
        child.style.marginLeft = props.axis == Axis.Horizontal && !props.reverse && hasGap ? props.gap : 0f;
        child.style.marginRight = props.axis == Axis.Horizontal && props.reverse && hasGap ? props.gap : 0f;
      }
    }
  }

  [BoundaryComposable(Extension = false)]
  internal partial class DynamicScrollGroupBoundary {
    public partial struct Props {
      public DynamicComposableController controller;
      [Prop(Axis.Vertical)] public Axis axis;
      [Prop(Justify.FlexStart)] public Justify main;
      [Prop(Align.Stretch)] public Align cross;
      [Prop(0f)] public float gap;
      [Prop(false)] public bool reverse;
      [Prop(null)] public ScrollerSliderController scrollController;
      [Prop(true)] public bool showSlider;
      [Prop(null)] public Composable<SliderController> slider;
      [Prop(null)] public SliderStyle? sliderStyle;
    }

    protected override void OnRecompose(ref Composition cx) {
      if (props.controller == null) return;
      Node.style.flexGrow = 1f;
      Node.style.flexShrink = 1f;
      Node.style.alignSelf = Align.Stretch;
      Node.MarkFlag(UssFlag.Flex);
      using (cx.ScrollView(
        props.scrollController,
        props.axis,
        props.reverse,
        props.showSlider,
        props.slider,
        props.sliderStyle
      ))
        cx.DynamicFlexGroup(
          props.controller,
          props.axis,
          props.main,
          props.cross,
          gap: props.gap,
          reverse: props.reverse
        );
    }
  }

  public static class DynamicGroups {
    public static void DynamicFlexGroup(
      this ref Composition cx,
      DynamicComposableController controller,
      Axis axis = Axis.Vertical,
      Justify main = Justify.FlexStart,
      Align cross = Align.Stretch,
      Flex? flex = null,
      float gap = 0f,
      bool reverse = false,
      bool clear = false
    ) {
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      DynamicFlexGroupBoundary.ComposeBoundary(
        ref cx,
        controller,
        axis,
        main,
        cross,
        flex,
        gap,
        reverse,
        clear
      );
    }

    public static void DynamicScrollGroup(
      this ref Composition cx,
      DynamicComposableController controller,
      Axis axis = Axis.Vertical,
      Justify main = Justify.FlexStart,
      Align cross = Align.Stretch,
      float gap = 0f,
      bool reverse = false,
      ScrollerSliderController scrollController = null,
      bool showSlider = true,
      Composable<SliderController> slider = null,
      SliderStyle? sliderStyle = null
    ) {
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      DynamicScrollGroupBoundary.ComposeBoundary(
        ref cx,
        controller,
        axis,
        main,
        cross,
        gap,
        reverse,
        scrollController,
        showSlider,
        slider,
        sliderStyle
      );
    }
  }
}