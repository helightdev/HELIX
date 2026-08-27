using System;
using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Signals;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public abstract class DynamicComposable {
    private Composable _composable;
    internal DynamicComposableController controller;
    internal long sequence;
    public readonly object key;
    public int order;

    protected DynamicComposable(object key, Composable composable, int order, object userData) {
      this.key = key ?? throw new ArgumentNullException(nameof(key));
      Composable = composable;
      UserData = userData;
      this.order = order;
    }

    public object UserData { get; set; }

    public Composable Composable {
      get => _composable;
      set => _composable = value ?? throw new ArgumentNullException(nameof(value));
    }

    public DynamicComposableController Controller => controller;
    public bool IsBound => controller != null;
    public void Remove() => controller?.RemoveEntry(this);

    public static DynamicComposable Lookup(VisualElement element) {
      for (var parent = element; parent != null; parent = parent.hierarchy.parent) {
        if (parent is DynamicComposableElement found) return found.Entry;
      }
      return null;
    }
  }

  public sealed class DynamicComposable<TLayout> : DynamicComposable {
    public readonly TLayout layout;

    public DynamicComposable(
      object key,
      Composable composable,
      TLayout layout,
      object userData = null,
      int order = 0
    ) : base(key, composable, order, userData) {
      this.layout = layout;
    }
  }

  public struct DynamicFlexLayout {
    public StyleFloat flex;
    public AxisConstraint size;

    public DynamicFlexLayout(StyleFloat? flex = null, AxisConstraint? size = null) {
      this.flex = flex.GetValueOrDefault(StyleKeyword.Null);
      this.size = size ?? AxisConstraint.Null;
    }
  }

  public struct DynamicStackLayout {
    public BoxConstraints constraints;
    public StyleLength4 position;

    public DynamicStackLayout(
      StyleLength4? position = null,
      BoxConstraints? constraints = null
    ) {
      this.position = position ?? StyleLength4.Null;
      this.constraints = constraints ?? BoxConstraints.Null;
    }
  }

  /// <summary>
  /// A visual-element-independent collection of keyed composable entries. After changing an entry,
  /// call <see cref="NotifyEntriesChanged"/> to publish the change and reapply its ordering.
  /// </summary>
  public abstract class DynamicComposableController : Signal<int> {
    private int _revision;
    private long _nextSequence;

    public DynamicComposableController() : base(
      nameof(DynamicComposableController),
      typeof(DynamicComposableController)
    ) { }

    public abstract int Count { get; }
    internal int Revision => _revision;
    internal abstract DynamicComposable EntryAt(int index);
    protected abstract void SortEntries();
    protected abstract void ClearEntries();

    protected void Attach(DynamicComposable entry) {
      entry.controller = this;
      entry.sequence = _nextSequence++;
    }

    public abstract bool RemoveEntry(DynamicComposable entry);

    public bool RemoveEntry(object key) {
      var entry = FindEntry(key);
      return entry != null && RemoveEntry(entry);
    }

    public DynamicComposable FindEntry(object key) {
      if (key == null) return null;
      for (var i = 0; i < Count; i++) {
        var entry = EntryAt(i);
        if (Equals(entry.key, key)) return entry;
      }
      return null;
    }

    public void NotifyEntriesChanged() {
      SortEntries();
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
      for (var i = 0; i < Count; i++) EntryAt(i).controller = null;
      ClearEntries();
      base.Dispose();
    }

    protected static int CompareEntries(DynamicComposable left, DynamicComposable right) {
      var order = left.order.CompareTo(right.order);
      return order != 0 ? order : left.sequence.CompareTo(right.sequence);
    }
  }

  public sealed class DynamicComposableController<TLayout> : DynamicComposableController {
    private readonly List<DynamicComposable<TLayout>> _entries = new();

    public IReadOnlyList<DynamicComposable<TLayout>> Entries => _entries;
    public override int Count => _entries.Count;
    internal override DynamicComposable EntryAt(int index) => _entries[index];

    public DynamicComposable<TLayout> AddEntry(
      object key,
      Composable composable,
      TLayout layout,
      object userData = null,
      int order = 0
    ) => AddEntry(new DynamicComposable<TLayout>(key, composable, layout, userData, order));

    public DynamicComposable<TLayout> AddEntry(DynamicComposable<TLayout> entry) {
      if (entry == null) throw new ArgumentNullException(nameof(entry));
      if (entry.controller != null)
        throw new InvalidOperationException("The dynamic composable entry already belongs to a controller.");
      if (FindEntry(entry.key) != null)
        throw new ArgumentException("An entry with the same key already exists.", nameof(entry));
      Attach(entry);
      _entries.Add(entry);
      NotifyEntriesChanged();
      return entry;
    }

    public new DynamicComposable<TLayout> FindEntry(object key) => base.FindEntry(key) as DynamicComposable<TLayout>;

    public override bool RemoveEntry(DynamicComposable entry) {
      if (entry is not DynamicComposable<TLayout> typed ||
        !ReferenceEquals(entry.controller, this) || !_entries.Remove(typed)) return false;
      entry.controller = null;
      NotifyEntriesChanged();
      return true;
    }

    protected override void SortEntries() => _entries.Sort(CompareEntries);
    protected override void ClearEntries() => _entries.Clear();
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
  public static class DynamicCollectionHelper {
    private static readonly Comparison<VisualElement> _comparison = CompareElements;

    public static void Synchronize(VisualElement parent, DynamicComposableController controller) {
      if (parent == null) throw new ArgumentNullException(nameof(parent));
      if (controller == null) throw new ArgumentNullException(nameof(controller));

      for (var i = parent.childCount - 1; i >= 0; i--) {
        if (parent.ElementAt(i) is DynamicComposableElement element &&
          !ReferenceEquals(element.Entry?.controller, controller))
          element.RemoveFromHierarchy();
      }

      for (var i = 0; i < controller.Count; i++) {
        var entry = controller.EntryAt(i);
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

  [EnableMixins]
  [BoundaryComposableMixin(name: "DynamicFlexGroup", extension: true, cacheLookups: true)]
  public partial class DynamicFlexGroupBoundary {
    public partial struct Props {
      public DynamicComposableController<DynamicFlexLayout> controller;
      [Prop(Axis.Vertical)] public Axis axis;
      [Prop(Justify.FlexStart)] public Justify main;
      [Prop(Align.Stretch)] public Align cross;
      [Prop(null)] public Flex? flex;
      [Prop(0f)] public float gap;
      [Prop(false)] public bool reverse;
      [Prop(false)] public bool clear;
    }

    protected override void OnDetach() {
      DynamicCollectionHelper.Clear(Node);
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      if (props.controller == null) return;
      cx.SubscribeTo(props.controller);
      if (props.clear) DynamicCollectionHelper.Clear(Node);
      FlexGroup.Of(props.axis, props.main, props.cross, reverse: props.reverse).Apply(Node);
      (props.flex ?? Flex.Null).Apply(Node);
      Node.MarkFlag(UssFlag.GroupAlign | UssFlag.Flex);
      DynamicCollectionHelper.Synchronize(Node, props.controller);
      ApplyGap();
      cx.AUTHORING.cell.cursor = Node.childCount;
    }

    private void ApplyGap() {
      for (var i = 0; i < Node.childCount; i++) {
        var child = Node.ElementAt(i);
        if (child is DynamicComposableElement { Entry: DynamicComposable<DynamicFlexLayout> entry } element) {
          element.style.flexGrow = entry.layout.flex;
          AxisConstraint.Null.Apply(element, props.axis == Axis.Horizontal ? Axis.Vertical : Axis.Horizontal);
          entry.layout.size.Apply(element, props.axis);
          element.MarkFlag(UssFlag.Flex | UssFlag.Size);
        }
        var hasGap = i > 0;
        child.style.marginTop = props is { axis: Axis.Vertical, reverse: false } && hasGap ? props.gap : 0f;
        child.style.marginBottom = props is { axis: Axis.Vertical, reverse: true } && hasGap ? props.gap : 0f;
        child.style.marginLeft = props is { axis: Axis.Horizontal, reverse: false } && hasGap ? props.gap : 0f;
        child.style.marginRight = props is { axis: Axis.Horizontal, reverse: true } && hasGap ? props.gap : 0f;
      }
    }
  }

  [EnableMixins]
  [BoundaryComposableMixin(name: "DynamicScrollGroup", extension: true, cacheLookups: true)]
  public partial class DynamicScrollGroupBoundary {
    public partial struct Props {
      public DynamicComposableController<DynamicFlexLayout> controller;
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

  [EnableMixins]
  [BoundaryComposableMixin(name: "DynamicStack", extension: true, cacheLookups: true)]
  public partial class DynamicStackBoundary {
    public partial struct Props {
      public DynamicComposableController<DynamicStackLayout> controller;
      [Prop(false)] public bool clear;
    }

    protected override void OnDetach() {
      DynamicCollectionHelper.Clear(Node);
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      if (props.controller == null) return;
      cx.SubscribeTo(props.controller);
      if (props.clear) DynamicCollectionHelper.Clear(Node);
      DynamicCollectionHelper.Synchronize(Node, props.controller);
      ApplyLayout();
      cx.AUTHORING.cell.cursor = Node.childCount;
    }

    private void ApplyLayout() {
      for (var i = 0; i < Node.childCount; i++) {
        if (Node.ElementAt(i) is not DynamicComposableElement element) continue;
        if (element.Entry is not DynamicComposable<DynamicStackLayout> entry) continue;
        element.style.flexGrow = StyleKeyword.Null;
        element.style.position = Position.Absolute;
        element.Position(entry.layout.position);
        entry.layout.constraints.Apply(element);
        element.MarkFlag(UssFlag.Flex | UssFlag.Position | UssFlag.Size);
      }
    }
  }
}