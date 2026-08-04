using System;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;
using NativeScrollView = UnityEngine.UIElements.ScrollView;

namespace HELIX.Compose {
  public static class ScrollViewElementExtensions {
    private static readonly ushort _scrollViewElementId = CompositionId.GetTypeId("ScrollViewElement");

    internal static ScopeHandle ScrollViewElement(
      this ref Composition cx,
      HXScrollViewElement element,
      out ScrollViewElementSlots slots,
      Axis axis
    ) {
      var packedId = cx.AUTHORING.PrepareId(_scrollViewElementId);
      if (element.PackedId != packedId) element.PackedId = packedId;
      element.Update(axis);
      slots = new ScrollViewElementSlots(element, cx);
      return cx.AUTHORING.YieldScope(ref cx, element);
    }
  }

  public readonly ref struct ScrollViewElementSlots {
    private readonly HXScrollViewElement _element;
    private readonly Composition _composition;

    public ScrollViewElementSlots(HXScrollViewElement element, Composition composition) {
      _element = element;
      _composition = composition;
    }

    public ScopeHandle Content(bool trimChildren = false) => _element.content.Scope(_composition, trimChildren);
    public ScopeHandle Slider() => _element.slider.Scope(_composition, true);
  }

  public sealed class HXScrollViewElement : VisualElement, ISlotHost {
    public static readonly UniqueStyleString ClassViewport = new("hx-scroll-view-viewport");
    public static readonly UniqueStyleString ClassContent = new("hx-scroll-view-content");
    public static readonly UniqueStyleString ClassSlider = new("hx-scroll-view-slider");

    public readonly NativeScrollView viewport;
    public readonly ComposableSlot content;
    public readonly ComposableSlot slider;

    public HXScrollViewElement() {
      this.FlexContainer(Axis.Horizontal, crossAxisAlign: Align.Stretch);
      pickingMode = PickingMode.Ignore;

      viewport = new NativeScrollView {
        horizontalScrollerVisibility = ScrollerVisibility.Hidden,
        verticalScrollerVisibility = ScrollerVisibility.Hidden
      };
      viewport.WithClasses(ClassViewport);
      viewport.Flexible(1f, 1f, Align.Stretch).AddTo(hierarchy);

      content = new ComposableSlot(this, ClassContent)
        .WithClasses(ClassContent)
        .FlexContainer(Axis.Vertical, crossAxisAlign: Align.Stretch)
        .AddTo(viewport.contentContainer);
      content.style.display = DisplayStyle.Flex;

      slider = new ComposableSlot(this, ClassSlider)
        .WithClasses(ClassSlider)
        .TightStretch()
        .AddTo(hierarchy);
    }

    public VisualElement Element => this;
    public UssFlag Flag { get; set; }
    public ulong PackedId { get; set; }
    public IBoundary Boundary { get; private set; }

    public void Initialize(IBoundary contentBoundary) {
      Boundary = contentBoundary;
    }

    public void Update(Axis axis) {
      style.flexDirection = axis == Axis.Vertical ? FlexDirection.Row : FlexDirection.Column;
      slider.style.flexDirection = axis == Axis.Vertical ? FlexDirection.Column : FlexDirection.Row;
      content.style.flexDirection = axis.ToFlexDirection();
      viewport.mode = axis == Axis.Vertical ? ScrollViewMode.Vertical : ScrollViewMode.Horizontal;
    }

    public void Reset() {
      content.Reset();
      slider.Reset();
      Boundary = null;
    }
  }

  [BoundaryComposable(Extension = false)]
  public partial class ScrollViewBoundary {
    public partial struct Props {
      [PropDefault(null)] public ScrollerSliderController controller;
      [PropDefault(Axis.Vertical)] public Axis axis;
      [PropDefault(false)] public bool reverse;
      [PropDefault(true)] public bool showSlider;
      [PropDefault(null)] public Composable<SliderController> slider;
      [PropDefault(null)] public SliderStyle? sliderStyle;
    }

    public ScrollerSliderController controller { get; private set; }
    public HXScrollViewElement element { get; private set; }

    private bool _automaticController;

    protected override void OnAttach() {
      element = new HXScrollViewElement();
      element.viewport.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      element.viewport.contentContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      element.viewport.contentViewport.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    protected override void OnDetach() {
      element.viewport.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      element.viewport.contentContainer.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      element.viewport.contentViewport.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      ReleaseController();
      element = null;
    }

    protected override void OnRecompose(ref Composition cx) {
      element.Initialize(Node.Parent);
      element.Update(props.axis);
      EnsureController();

      using (cx.ScrollViewElement(element, out var slots, props.axis)) {
        cx.CURSOR.Flexible().AlignSelf(Align.Stretch).Focusable(false, pickingMode: PickingMode.Ignore);

        using (slots.Slider()) {
          if (!props.showSlider) return;
          if (props.slider != null) {
            props.slider(ref cx, controller);
          } else {
            cx.Slider(
              controller: controller,
              style: props.sliderStyle ?? ThemeProperties.Scroller[in cx]
            ).Flexible();
          }
        }
      }
    }

    internal void Prepare(IBoundary boundary) {
      element.Initialize(boundary);
      element.Update(props.axis);
      EnsureController();
    }

    private void EnsureController() {
      if (props.controller == null) {
        if (controller == null || !_automaticController) {
          ReleaseController();
          controller = new ScrollerSliderController();
          _automaticController = true;
        }
      } else if (!ReferenceEquals(controller, props.controller)) {
        ReleaseController();
        controller = props.controller;
        _automaticController = false;
      }

      SynchronizeControllerOptions();
      controller.Bind(CurrentScroller());
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      SynchronizeControllerOptions();
    }

    private void SynchronizeControllerOptions() {
      if (controller == null) return;
      var scroller = CurrentScroller();
      if (scroller == null) return;

      var min = FiniteOrZero(scroller.lowValue);
      var max = Mathf.Max(min, FiniteOrZero(scroller.highValue));
      var thumbRange = props.axis == Axis.Vertical
        ? FiniteOrZero(element.viewport.contentViewport.layout.height)
        : FiniteOrZero(element.viewport.contentViewport.layout.width);
      var options = new SliderOptions(
        min,
        max,
        axis: props.axis,
        reverse: props.reverse,
        thumbRange: Mathf.Max(0f, thumbRange)
      );
      controller.SynchronizeScrollValue(scroller.value, in options);
    }

    private Scroller CurrentScroller() {
      if (element == null) return null;
      return props.axis == Axis.Vertical
        ? element.viewport.verticalScroller
        : element.viewport.horizontalScroller;
    }

    private void ReleaseController() {
      if (controller == null) return;
      controller.Unbind();
      if (_automaticController) controller.Dispose();
      controller = null;
      _automaticController = false;
    }

    private static float FiniteOrZero(float value) {
      return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
    }
  }

  public readonly ref struct ScrollViewSlots {
    private readonly ScrollViewBoundary _boundary;
    private readonly Composition _composition;

    internal ScrollViewSlots(ScrollViewBoundary boundary, Composition composition) {
      _boundary = boundary;
      _composition = composition;
    }

    public ScrollerSliderController Controller => _boundary.controller;
    public ScopeHandle Content() => _boundary.element.content.Scope(_composition, true);
  }

  public static class ScrollViewExtensions {
    public static ScopeHandle ScrollView(
      this ref Composition cx,
      out ScrollViewSlots slots,
      ScrollerSliderController controller = null,
      Axis axis = Axis.Vertical,
      bool reverse = false,
      bool showSlider = true,
      Composable<SliderController> slider = null,
      SliderStyle? sliderStyle = null
    ) {
      ref var boundaryRef = ref ScrollViewBoundary.ComposeBoundary(
        ref cx,
        controller,
        axis,
        reverse,
        showSlider,
        slider,
        sliderStyle
      );
      var node = boundaryRef.element as CompositionBoundaryNodeBase;
      var boundary = node?.BoundaryComposable as ScrollViewBoundary;
      if (boundary == null) throw new InvalidOperationException("Scroll view boundary was not initialized.");

      boundary.Prepare(cx.boundary);
      slots = new ScrollViewSlots(boundary, cx);
      return slots.Content();
    }

    public static ScopeHandle ScrollView(
      this ref Composition cx,
      ScrollerSliderController controller = null,
      Axis axis = Axis.Vertical,
      bool reverse = false,
      bool showSlider = true,
      Composable<SliderController> slider = null,
      SliderStyle? sliderStyle = null
    ) => cx.ScrollView(
      out _, controller, axis, reverse, showSlider, slider, sliderStyle
    );
  }
}