using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using NativeListView = UnityEngine.UIElements.ListView;
using NativeScrollView = UnityEngine.UIElements.ScrollView;

namespace HELIX.Compose {
  [EnableMixins]
  [BoundaryElementMixin(false)]
  public sealed partial class VirtualizedListItemBoundary {
    private Composable<int> _builder;
    private int _index;
    private bool _separator;

    [Hook]
    private void OnInit() {
      this.TightStretch().Sized(100.Percent());
      pickingMode = PickingMode.Ignore;
    }

    internal void Bind(Composable<int> builder, int index, bool separator) {
      if (ReferenceEquals(_builder, builder) && _index == index && _separator == separator) return;
      _builder = builder;
      _index = index;
      _separator = separator;
      ClearClassList(); // Remove the annoying selection highlights you otherwise can't get properly get rid off
      if (panel != null) HXComposer.MarkDirty(this, false);
    }

    internal void Unbind() {
      if (_builder == null) return;
      _builder = null;
      if (panel != null) HXComposer.MarkDirty(this, false);
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      var identity = _separator ? _index | int.MinValue : _index;
      cx.AUTHORING.SetId(CompositionId.Generated(identity));
      _builder?.Invoke(ref cx, _index);
    }
  }

  internal sealed class VirtualizedIndexList : IList {
    public int Count { get; set; }
    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public bool IsFixedSize => false;
    public bool IsReadOnly => true;

    public object this[int index] {
      get => index >= 0 && index < Count ? index : null;
      set => throw new NotSupportedException();
    }

    public IEnumerator GetEnumerator() {
      for (var i = 0; i < Count; i++) yield return i;
    }

    public int IndexOf(object value) {
      return value is int index && index >= 0 && index < Count ? index : -1;
    }

    public bool Contains(object value) {
      return IndexOf(value) >= 0;
    }

    public void CopyTo(Array array, int index) {
      for (var i = 0; i < Count; i++) array.SetValue(i, index + i);
    }

    public int Add(object value) {
      throw new NotSupportedException();
    }

    public void Clear() {
      throw new NotSupportedException();
    }

    public void Insert(int index, object value) {
      throw new NotSupportedException();
    }

    public void Remove(object value) {
      throw new NotSupportedException();
    }

    public void RemoveAt(int index) {
      throw new NotSupportedException();
    }
  }

  [EnableMixins]
  [CustomBoundaryElement(false, trimChildren: false)]
  public sealed partial class HXListViewElement : VisualElement, ISlotHost {
    public static readonly UniqueStyleString ClassViewport = new("hx-list-view-viewport");
    public static readonly UniqueStyleString ClassSlider = new("hx-list-view-slider");

    private readonly VirtualizedIndexList _indices = new();

    public readonly NativeListView listView;
    public readonly NativeScrollView scrollView;
    public readonly ComposableSlot slider;

    private bool _automaticController;
    private Composable<int> _itemBuilder;
    private Composable<int> _separatorBuilder;

    public HXListViewElement() {
      this.FlexContainer(Axis.Horizontal, crossAxisAlign: Align.Stretch);
      pickingMode = PickingMode.Ignore;

      listView = new NativeListView {
        itemsSource = _indices, makeItem = MakeItem, bindItem = BindItem, unbindItem = UnbindItem,
        destroyItem = DestroyItem, selectionType = SelectionType.None, reorderable = false, allowAdd = false,
        allowRemove = false
      };
      listView.WithClasses(ClassViewport);
      listView.Flexible(1f, 1f, Align.Stretch).AddTo(hierarchy);

      scrollView = listView.Q<NativeScrollView>();
      scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
      scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;

      slider = new ComposableSlot(this, ClassSlider)
        .WithClasses(ClassSlider)
        .TightStretch()
        .AddTo(hierarchy);

      listView.RegisterCallback<GeometryChangedEvent>(GeometryChangedHandler);
      scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(GeometryChangedHandler);
      scrollView.contentViewport.RegisterCallback<GeometryChangedEvent>(GeometryChangedHandler);
      RegisterCallback<AttachToPanelEvent>(_ => AttachBoundary());
      RegisterCallback<DetachFromPanelEvent>(_ => DetachBoundary());
      PostConstruct();
    }

    public ScrollerSliderController Controller { get; private set; }
    public NativeListView ListView => listView;
    public NativeScrollView ScrollView => scrollView;

    public IBoundary Boundary => this;

    [Hook]
    private void OnCompose(ref Composition cx) {
      this.Flexible(selfAlign: Align.Stretch).Focusable(false, pickingMode: PickingMode.Ignore);
      _itemBuilder = props.itemBuilder;
      _separatorBuilder = props.separatorBuilder;

      var visualCount = ResolveVisualCount(props.itemCount, props.separatorBuilder != null);
      if (_indices.Count != visualCount) _indices.Count = visualCount;

      if (props.fixedItemHeight >= 0f) {
        listView.fixedItemHeight = props.fixedItemHeight;
        listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
      } else listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

      listView.RefreshItems();
      EnsureController();
      using (slider.Scope(ref cx)) {
        if (!props.showSlider) return;
        if (props.slider != null) props.slider(ref cx, Controller);
        else cx.Slider(controller: Controller, style: props.sliderStyle ?? ThemeProperties.Scroller[in cx]).Flexible();
      }
    }

    [Hook]
    private void OnReset() {
      slider.Reset();
      _itemBuilder = null;
      _separatorBuilder = null;
      _indices.Count = 0;
    }

    [Hook]
    private void OnDispose() {
      ReleaseController();
    }

    private VisualElement MakeItem() {
      return new VirtualizedListItemBoundary();
    }

    private void BindItem(VisualElement element, int visualIndex) {
      if (element is not VirtualizedListItemBoundary host) return;
      if (_separatorBuilder == null) {
        host.Bind(_itemBuilder, visualIndex, false);
        return;
      }

      var separator = (visualIndex & 1) != 0;
      host.Bind(separator ? _separatorBuilder : _itemBuilder, visualIndex / 2, separator);
    }

    private static void UnbindItem(VisualElement element, int _) {
      if (element is VirtualizedListItemBoundary host) host.Unbind();
    }

    private static void DestroyItem(VisualElement element) {
      using (HXComposer.BeginBatch()) {
        element.Clear();
        if (element is VirtualizedListItemBoundary host) host.Dispose();
      }
    }

    private void GeometryChangedHandler(GeometryChangedEvent evt) {
      SynchronizeControllerOptions();
    }

    private static int ResolveVisualCount(int itemCount, bool separated) {
      if (!separated || itemCount == 0) return itemCount;
      var maximumItemCount = int.MaxValue / 2 + 1;
      return itemCount >= maximumItemCount ? int.MaxValue : itemCount * 2 - 1;
    }

    private void EnsureController() {
      if (props.controller == null) {
        if (Controller == null || !_automaticController) {
          ReleaseController();
          Controller = new ScrollerSliderController();
          _automaticController = true;
        }
      } else if (!ReferenceEquals(Controller, props.controller)) {
        ReleaseController();
        Controller = props.controller;
        _automaticController = false;
      }

      SynchronizeControllerOptions();
      Controller.Bind(ScrollView.verticalScroller);
    }

    private void SynchronizeControllerOptions() {
      if (Controller == null) return;
      var scroller = ScrollView.verticalScroller;
      var min = FiniteOrZero(scroller.lowValue);
      var max = Mathf.Max(min, FiniteOrZero(scroller.highValue));
      var thumbRange = Mathf.Max(0f, FiniteOrZero(ScrollView.contentViewport.layout.height));
      var options = new SliderOptions(
        min,
        max,
        axis: Axis.Vertical,
        reverse: props.reverse,
        thumbRange: thumbRange
      );
      Controller.SynchronizeScrollValue(scroller.value, in options);
    }

    private void ReleaseController() {
      if (Controller == null) return;
      Controller.Unbind();
      if (_automaticController) Controller.Dispose();
      Controller = null;
      _automaticController = false;
    }

    private static float FiniteOrZero(float value) {
      return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
    }

    public partial struct Props {
      public int itemCount;
      public Composable<int> itemBuilder;
      [Prop(null)] public Composable<int> separatorBuilder;
      [Prop(-1f)] public float fixedItemHeight;
      [Prop(null)] public ScrollerSliderController controller;
      [Prop(false)] public bool reverse;
      [Prop(true)] public bool showSlider;
      [Prop(null)] public Composable<SliderController> slider;
      [Prop(null)] public SliderStyle? sliderStyle;
    }
  }

  public static class ListViewExtensions {
    public static ref ElementRef ListView(
      this ref Composition cx,
      int itemCount,
      Composable<int> itemBuilder,
      float fixedItemHeight = -1f,
      ScrollerSliderController controller = null,
      bool reverse = false,
      bool showSlider = true,
      Composable<SliderController> slider = null,
      SliderStyle? sliderStyle = null
    ) {
      Validate(itemCount, itemBuilder, nameof(itemBuilder));
      return ref HXListViewElement.ComposeBoundary(
        ref cx,
        itemCount,
        itemBuilder,
        null,
        fixedItemHeight,
        controller,
        reverse,
        showSlider,
        slider,
        sliderStyle
      );
    }

    public static ref ElementRef ListView(
      this ref Composition cx,
      int itemCount,
      Composable<int> itemBuilder,
      Composable<int> separatorBuilder,
      float fixedItemHeight = -1f,
      ScrollerSliderController controller = null,
      bool reverse = false,
      bool showSlider = true,
      Composable<SliderController> slider = null,
      SliderStyle? sliderStyle = null
    ) {
      Validate(itemCount, itemBuilder, nameof(itemBuilder));
      if (separatorBuilder == null) throw new ArgumentNullException(nameof(separatorBuilder));
      return ref HXListViewElement.ComposeBoundary(
        ref cx,
        itemCount,
        itemBuilder,
        separatorBuilder,
        fixedItemHeight,
        controller,
        reverse,
        showSlider,
        slider,
        sliderStyle
      );
    }

    private static void Validate(int itemCount, Composable<int> builder, string parameterName) {
      if (itemCount < 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
      if (builder == null) throw new ArgumentNullException(parameterName);
    }
  }
}