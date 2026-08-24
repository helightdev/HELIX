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
    public AxisConstraint cross;
    public StyleLength4 position;
    public int order;

    public DynamicComposable(
      object key,
      Composable composable,
      object userData = null,
      StyleFloat? flex = null,
      int order = 0,
      AxisConstraint? constraint = null,
      AxisConstraint? cross = null,
      StyleLength4? position = null
    ) {
      this.key = key ?? throw new ArgumentNullException(nameof(key));
      Composable = composable;
      UserData = userData;
      this.flex = flex.GetValueOrDefault(StyleKeyword.Null);
      size = constraint ?? AxisConstraint.Null;
      this.cross = cross ?? AxisConstraint.Null;
      this.position = position ?? StyleLength4.Null;
      this.order = order;
    }

    public object UserData { get; set; }

    public Composable Composable {
      get => _composable;
      set => _composable = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsBound => controller != null;
    public void Remove() => controller?.RemoveEntry(this);

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

    public DynamicComposableController() : base(
      nameof(DynamicComposableController),
      typeof(DynamicComposableController)
    ) { }

    public IReadOnlyList<DynamicComposable> Entries => _entries;
    public int Count => _entries.Count;
    internal int Revision => _revision;

    public DynamicComposable AddEntry(
      object key,
      Composable composable,
      object userData = null,
      StyleFloat? flex = null,
      int order = 0,
      AxisConstraint? constraint = null,
      AxisConstraint? cross = null,
      StyleLength4? position = null
    ) => AddEntry(new DynamicComposable(key, composable, userData, flex, order, constraint, cross, position));

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
  public static class BoundaryCollectionHelper {
    private static readonly Comparison<VisualElement> _comparison = CompareElements;

    public static void Synchronize(VisualElement parent, DynamicComposableController controller) {
      if (parent == null) throw new ArgumentNullException(nameof(parent));
      if (controller == null) throw new ArgumentNullException(nameof(controller));

      for (var i = parent.childCount - 1; i >= 0; i--) {
        if (parent.ElementAt(i) is DynamicComposableElement element &&
          !ReferenceEquals(element.Entry?.controller, controller))
          element.RemoveFromHierarchy();
      }

      var entries = controller.Entries;
      for (var i = 0; i < entries.Count; i++) {
        var entry = entries[i];
        DynamicComposableElement element = null;
        for (var j = 0; j < parent.childCount; j++) {
          if (parent.ElementAt(j) is not DynamicComposableElement candidate ||
            !ReferenceEquals(candidate.Entry, entry)) continue;
          element = candidate;
          break;
        }
        if (element == null) {
          element = new DynamicComposableElement { PackedId = CompositionId.Generated(entry.key.GetHashCode()).packed };
          parent.Add(element);
          element.RefreshHierarchy();
        }
        element.Bind(entry, controller.Revision);
      }
      parent.hierarchy.Sort(_comparison);
    }

    public static void Clear(VisualElement parent) {
      if (parent == null) return;
      for (var i = parent.childCount - 1; i >= 0; i--)
        if (parent.ElementAt(i) is DynamicComposableElement)
          parent.RemoveAt(i);
    }

    private static int CompareElements(VisualElement left, VisualElement right) {
      var leftEntry = (left as DynamicComposableElement)?.Entry;
      var rightEntry = (right as DynamicComposableElement)?.Entry;
      var order = (leftEntry?.order ?? 0).CompareTo(rightEntry?.order ?? 0);
      if (order != 0) return order;
      return (leftEntry?.sequence ?? 0).CompareTo(rightEntry?.sequence ?? 0);
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

    protected override void OnDetach() {
      BoundaryCollectionHelper.Clear(Node);
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      if (props.controller == null) return;
      cx.SubscribeTo(props.controller);
      if (props.clear) BoundaryCollectionHelper.Clear(Node);
      FlexGroup.Of(props.axis, props.main, props.cross, reverse: props.reverse).Apply(Node);
      (props.flex ?? Flex.Null).Apply(Node);
      Node.MarkFlag(UssFlag.GroupAlign | UssFlag.Flex);
      BoundaryCollectionHelper.Synchronize(Node, props.controller);
      ApplyGap();
      cx.AUTHORING.cell.cursor = Node.childCount;
    }

    private void ApplyGap() {
      for (var i = 0; i < Node.childCount; i++) {
        var child = Node.ElementAt(i);
        if (child is DynamicComposableElement entry) {
          AxisConstraint.Null.Apply(entry, props.axis == Axis.Horizontal ? Axis.Vertical : Axis.Horizontal);
          entry.Entry.size.Apply(entry, props.axis);
          entry.MarkFlag(UssFlag.Size);
        }
        var hasGap = i > 0;
        child.style.marginTop = props is { axis: Axis.Vertical, reverse: false } && hasGap ? props.gap : 0f;
        child.style.marginBottom = props is { axis: Axis.Vertical, reverse: true } && hasGap ? props.gap : 0f;
        child.style.marginLeft = props is { axis: Axis.Horizontal, reverse: false } && hasGap ? props.gap : 0f;
        child.style.marginRight = props is { axis: Axis.Horizontal, reverse: true } && hasGap ? props.gap : 0f;
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

  [BoundaryComposable(Extension = false)]
  internal partial class DynamicStackBoundary {
    public partial struct Props {
      public DynamicComposableController controller;
      [Prop(false)] public bool clear;
    }

    protected override void OnDetach() {
      BoundaryCollectionHelper.Clear(Node);
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      if (props.controller == null) return;
      cx.SubscribeTo(props.controller);
      if (props.clear) BoundaryCollectionHelper.Clear(Node);
      BoundaryCollectionHelper.Synchronize(Node, props.controller);
      ApplyLayout();
      cx.AUTHORING.cell.cursor = Node.childCount;
    }

    private void ApplyLayout() {
      for (var i = 0; i < Node.childCount; i++) {
        if (Node.ElementAt(i) is not DynamicComposableElement element) continue;
        var entry = element.Entry;
        element.style.flexGrow = StyleKeyword.Null;
        element.style.position = Position.Absolute;
        element.style.left = entry.position.l;
        element.style.top = entry.position.t;
        element.style.right = entry.position.r;
        element.style.bottom = entry.position.b;
        entry.size.Apply(element, Axis.Horizontal);
        entry.cross.Apply(element, Axis.Vertical);
        element.MarkFlag(UssFlag.Flex | UssFlag.Position | UssFlag.Size);
      }
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
      DynamicFlexGroupBoundary.ComposeBoundary(ref cx, controller, axis, main, cross, flex, gap, reverse, clear);
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

    public static ref ElementRef DynamicStack(
      this ref Composition cx,
      DynamicComposableController controller,
      bool clear = false
    ) {
      if (controller == null) throw new ArgumentNullException(nameof(controller));
      return ref DynamicStackBoundary.ComposeBoundary(ref cx, controller, clear);
    }
  }
}
