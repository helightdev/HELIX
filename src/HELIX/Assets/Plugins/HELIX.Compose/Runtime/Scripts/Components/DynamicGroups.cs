using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public abstract class DynamicComposable {
    public readonly object key;
    private Composable _composable;
    internal DynamicComposableController controller;
    public int order;
    internal long sequence;

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
    public bool IsAttached => controller?.IsAttached(this) ?? false;

    public bool Detach() {
      return controller?.DetachEntry(this) ?? false;
    }

    public bool Reattach() {
      return controller?.ReattachEntry(this) ?? false;
    }

    public void Remove() {
      controller?.RemoveEntry(this);
    }

    public static DynamicComposable Lookup(VisualElement element) {
      for (var parent = element; parent != null; parent = parent.hierarchy.parent)
        if (parent is DynamicComposableElement found)
          return found.Entry;
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
  ///   A visual-element-independent collection of keyed composable entries. After changing an entry,
  ///   call <see cref="NotifyEntriesChanged" /> to publish the change and reapply its ordering.
  /// </summary>
  public abstract class DynamicComposableController : Signal<int> {
    private long _nextSequence;

    public DynamicComposableController() : base(
      nameof(DynamicComposableController),
      typeof(DynamicComposableController)
    ) { }

    public abstract int Count { get; }
    public abstract int DetachedCount { get; }
    internal int Revision { get; private set; }
    internal abstract DynamicComposable EntryAt(int index);
    internal abstract DynamicComposable DetachedEntryAt(int index);
    internal abstract void PoolDetachedElement(DynamicComposableElement element);
    internal abstract DynamicComposableElement TakeDetachedElement(DynamicComposable entry);
    protected abstract void SortEntries();
    protected abstract void ClearEntries();

    protected void Attach(DynamicComposable entry) {
      entry.controller = this;
      entry.sequence = _nextSequence++;
    }

    public abstract bool RemoveEntry(DynamicComposable entry);
    public abstract bool DetachEntry(DynamicComposable entry);
    public abstract bool ReattachEntry(DynamicComposable entry);
    public abstract bool IsAttached(DynamicComposable entry);

    public bool DetachEntry(object key) {
      var entry = FindEntry(key);
      return entry != null && DetachEntry(entry);
    }

    public bool ReattachEntry(object key) {
      var entry = FindEntry(key);
      return entry != null && ReattachEntry(entry);
    }

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
      for (var i = 0; i < DetachedCount; i++) {
        var entry = DetachedEntryAt(i);
        if (Equals(entry.key, key)) return entry;
      }
      return null;
    }

    public void NotifyEntriesChanged() {
      SortEntries();
      unchecked { Revision++; }
      NotifyDirty();
      NotifyObservers();
    }

    public override int PeekValue() {
      return Revision;
    }

    public override void SetValue(int newValue) {
      throw new NotSupportedException("Dynamic composable state is changed through its entries.");
    }

    public override void SetWithoutNotify(int newValue) {
      throw new NotSupportedException("Dynamic composable state is changed through its entries.");
    }

    public override void Dispose() {
      for (var i = 0; i < Count; i++) EntryAt(i).controller = null;
      for (var i = 0; i < DetachedCount; i++) DetachedEntryAt(i).controller = null;
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
    private readonly List<DynamicComposable<TLayout>> _detachedEntries = new();
    private readonly List<DynamicComposableElement> _detachedElements = new();
    private int _detachedElementCapacity;

    public IReadOnlyList<DynamicComposable<TLayout>> Entries => _entries;
    public IReadOnlyList<DynamicComposable<TLayout>> DetachedEntries => _detachedEntries;
    public override int Count => _entries.Count;
    public override int DetachedCount => _detachedEntries.Count;

    /// <summary>Maximum number of detached visual subtrees retained for later reattachment.</summary>
    public int DetachedElementCapacity {
      get => _detachedElementCapacity;
      set {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        _detachedElementCapacity = value;
        while (_detachedElements.Count > value) _detachedElements.RemoveAt(0);
      }
    }

    public DynamicComposableController(int detachedElementCapacity = 0) {
      DetachedElementCapacity = detachedElementCapacity;
    }

    internal override DynamicComposable EntryAt(int index) {
      return _entries[index];
    }

    internal override DynamicComposable DetachedEntryAt(int index) {
      return _detachedEntries[index];
    }

    public DynamicComposable<TLayout> AddEntry(
      object key,
      Composable composable,
      TLayout layout,
      object userData = null,
      int order = 0
    ) {
      return AddEntry(new DynamicComposable<TLayout>(key, composable, layout, userData, order));
    }

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

    public new DynamicComposable<TLayout> FindEntry(object key) {
      return base.FindEntry(key) as DynamicComposable<TLayout>;
    }

    public override bool RemoveEntry(DynamicComposable entry) {
      if (entry is not DynamicComposable<TLayout> typed || !ReferenceEquals(entry.controller, this)) return false;
      if (!_entries.Remove(typed) && !_detachedEntries.Remove(typed)) return false;
      DiscardDetachedElement(entry);
      entry.controller = null;
      NotifyEntriesChanged();
      return true;
    }

    public override bool DetachEntry(DynamicComposable entry) {
      if (entry is not DynamicComposable<TLayout> typed ||
        !ReferenceEquals(entry.controller, this) || !_entries.Remove(typed)) return false;
      _detachedEntries.Add(typed);
      NotifyEntriesChanged();
      return true;
    }

    public override bool ReattachEntry(DynamicComposable entry) {
      if (entry is not DynamicComposable<TLayout> typed ||
        !ReferenceEquals(entry.controller, this) || !_detachedEntries.Remove(typed)) return false;
      _entries.Add(typed);
      NotifyEntriesChanged();
      return true;
    }

    public override bool IsAttached(DynamicComposable entry) {
      return entry is DynamicComposable<TLayout> typed &&
        ReferenceEquals(entry.controller, this) && _entries.Contains(typed);
    }

    internal override void PoolDetachedElement(DynamicComposableElement element) {
      if (_detachedElementCapacity == 0 || element?.Entry == null) return;
      DiscardDetachedElement(element.Entry);
      if (_detachedElements.Count == _detachedElementCapacity) _detachedElements.RemoveAt(0);
      _detachedElements.Add(element);
    }

    internal override DynamicComposableElement TakeDetachedElement(DynamicComposable entry) {
      for (var i = 0; i < _detachedElements.Count; i++) {
        var element = _detachedElements[i];
        if (!ReferenceEquals(element.Entry, entry)) continue;
        _detachedElements.RemoveAt(i);
        return element;
      }
      return null;
    }

    private void DiscardDetachedElement(DynamicComposable entry) {
      for (var i = _detachedElements.Count - 1; i >= 0; i--)
        if (ReferenceEquals(_detachedElements[i].Entry, entry)) _detachedElements.RemoveAt(i);
    }

    protected override void SortEntries() {
      _entries.Sort(CompareEntries);
    }

    protected override void ClearEntries() {
      _entries.Clear();
      _detachedEntries.Clear();
      _detachedElements.Clear();
    }
  }

  /// <summary>Owns the independently composed subtree of one dynamic entry.</summary>
  [Mixable]
  [BoundaryElementMixin(false)]
  public sealed partial class DynamicComposableElement {
    private int _revision = -1;

    public DynamicComposable Entry { get; private set; }
    public object UserData => Entry?.UserData;

    [Hook]
    private void OnInit() {
      pickingMode = PickingMode.Ignore;
    }

    internal void Bind(DynamicComposable entry, int revision) {
      if (ReferenceEquals(Entry, entry) && _revision == revision) return;
      Entry = entry;
      _revision = revision;
      if (panel != null) HXComposer.MarkDirty(this, false);
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      if (Entry == null) return;
      cx.AUTHORING.SetId(CompositionId.Generated(Entry.key.GetHashCode()));
      Entry.Composable.Invoke(ref cx);
    }
  }

  /// <summary>
  ///   Reconciles keyed boundary elements directly against a visual-element hierarchy. The helper owns
  ///   materialization, ordering and removal; entry contents remain independently composed boundaries.
  /// </summary>
  public static class DynamicCollectionHelper {
    private static readonly Comparison<VisualElement> _comparison = CompareElements;

    public static void Synchronize(VisualElement parent, DynamicComposableController controller) {
      if (parent == null) throw new ArgumentNullException(nameof(parent));
      if (controller == null) throw new ArgumentNullException(nameof(controller));

      for (var i = parent.childCount - 1; i >= 0; i--) {
        if (parent.ElementAt(i) is not DynamicComposableElement element) continue;
        if (ReferenceEquals(element.Entry?.controller, controller) && !controller.IsAttached(element.Entry))
          controller.PoolDetachedElement(element);
        if (!ReferenceEquals(element.Entry?.controller, controller) || !controller.IsAttached(element.Entry))
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
          element = controller.TakeDetachedElement(entry) ??
            new DynamicComposableElement { PackedId = CompositionId.Generated(entry.key.GetHashCode()).packed };
          parent.Add(element);
          if (element.Entry == null) element.RefreshHierarchy();
        }
        element.Bind(entry, controller.Revision);
      }
      parent.hierarchy.Sort(_comparison);
    }

    public static void Clear(VisualElement parent) {
      if (parent == null) return;
      for (var i = parent.childCount - 1; i >= 0; i--) {
        if (parent.ElementAt(i) is DynamicComposableElement)
          parent.RemoveAt(i);
      }
    }

    private static int CompareElements(VisualElement left, VisualElement right) {
      var leftEntry = (left as DynamicComposableElement)?.Entry;
      var rightEntry = (right as DynamicComposableElement)?.Entry;
      var order = (leftEntry?.order ?? 0).CompareTo(rightEntry?.order ?? 0);
      if (order != 0) return order;
      return (leftEntry?.sequence ?? 0).CompareTo(rightEntry?.sequence ?? 0);
    }
  }

  [Mixable]
  [BoundaryElementMixin(name: "DynamicFlexGroup", extension: true, cacheLookups: true)]
  public partial class DynamicFlexGroupBoundary {
    [Hook]
    private void OnDispose() {
      DynamicCollectionHelper.Clear(this);
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      if (props.controller == null) return;
      cx.SubscribeTo(props.controller);
      if (props.clear) DynamicCollectionHelper.Clear(this);
      FlexGroup.Of(props.axis, props.main, props.cross, reverse: props.reverse).Apply(this);
      (props.flex ?? Flex.Null).Apply(this);
      this.MarkFlag(UssFlag.GroupAlign | UssFlag.Flex);
      DynamicCollectionHelper.Synchronize(this, props.controller);
      ApplyGap();
      cx.AUTHORING.cell.cursor = childCount;
    }

    private void ApplyGap() {
      for (var i = 0; i < childCount; i++) {
        var child = ElementAt(i);
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
  }

  [Mixable]
  [BoundaryElementMixin(name: "DynamicScrollGroup", extension: true, cacheLookups: true)]
  public partial class DynamicScrollGroupBoundary {
    [Hook]
    private void OnCompose(ref Composition cx) {
      if (props.controller == null) return;
      using (cx.ScrollView(
        props.scrollController,
        props.axis,
        props.reverse,
        props.showSlider,
        props.slider,
        props.sliderStyle
      )) {
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
  }

  [Mixable]
  [BoundaryElementMixin(name: "DynamicStack", extension: true, cacheLookups: true)]
  public partial class DynamicStackBoundary {
    [Hook]
    private void OnDispose() {
      DynamicCollectionHelper.Clear(this);
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      if (props.controller == null) return;
      cx.SubscribeTo(props.controller);
      if (props.clear) DynamicCollectionHelper.Clear(this);
      DynamicCollectionHelper.Synchronize(this, props.controller);
      ApplyLayout();
      cx.AUTHORING.cell.cursor = childCount;
    }

    private void ApplyLayout() {
      for (var i = 0; i < childCount; i++) {
        if (ElementAt(i) is not DynamicComposableElement element) continue;
        if (element.Entry is not DynamicComposable<DynamicStackLayout> entry) continue;
        element.style.flexGrow = StyleKeyword.Null;
        element.style.position = Position.Absolute;
        element.Position(entry.layout.position);
        entry.layout.constraints.Apply(element);
        element.MarkFlag(UssFlag.Flex | UssFlag.Position | UssFlag.Size);
      }
    }

    public partial struct Props {
      public DynamicComposableController<DynamicStackLayout> controller;
      [Prop(false)] public bool clear;
    }
  }
}
