using System;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;
using NativeScrollView = UnityEngine.UIElements.ScrollView;

namespace HELIX.Compose {

  [EnableMixins]
  [CustomBoundaryElement(constructor: false, trimChildren: false)]
  public sealed partial class HXScrollViewElement : ComposableElement, ISlotHost {
    public static readonly UniqueStyleString ClassViewport = new("hx-scroll-view-viewport");
    public static readonly UniqueStyleString ClassContent = new("hx-scroll-view-content");
    public static readonly UniqueStyleString ClassSlider = new("hx-scroll-view-slider");

    public readonly NativeScrollView viewport;
    public readonly ComposableSlot content;
    public readonly ComposableSlot slider;

    public partial struct Props {
      [Prop(null)] public ScrollerSliderController controller;
      [Prop(Axis.Vertical)] public Axis axis;
      [Prop(false)] public bool reverse;
      [Prop(true)] public bool showSlider;
      [Prop(null)] public Composable<SliderController> slider;
      [Prop(null)] public SliderStyle? sliderStyle;
    }

    public IBoundary Boundary => this;
    public ScrollerSliderController Controller { get; private set; }
    public NativeScrollView ScrollView => viewport;

    private bool _automaticController;


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

      viewport.RegisterCallback<GeometryChangedEvent>(GeometryChangedHandler);
      viewport.contentContainer.RegisterCallback<GeometryChangedEvent>(GeometryChangedHandler);
      viewport.contentViewport.RegisterCallback<GeometryChangedEvent>(GeometryChangedHandler);
      RegisterCallback<AttachToPanelEvent>(_ => AttachBoundary());
      RegisterCallback<DetachFromPanelEvent>(_ => DetachBoundary());
      PostConstruct();
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      this.Flexible(selfAlign: Align.Stretch).Focusable(false, pickingMode: PickingMode.Ignore);
      style.flexDirection = props.axis == Axis.Vertical ? FlexDirection.Row : FlexDirection.Column;
      slider.style.flexDirection = props.axis == Axis.Vertical ? FlexDirection.Column : FlexDirection.Row;
      content.style.flexDirection = props.axis.ToFlexDirection();
      viewport.mode = props.axis == Axis.Vertical ? ScrollViewMode.Vertical : ScrollViewMode.Horizontal;

      EnsureController();
      using (slider.Scope(ref cx)) {
        if (!props.showSlider) return;
        if (props.slider != null) props.slider(ref cx, Controller);
        else cx.Slider(controller: Controller, style: props.sliderStyle ?? ThemeProperties.Scroller[in cx]).Flexible();
      }
    }

    [Hook]
    private void OnReset() {
      base.Reset();
      content.Reset();
      slider.Reset();
    }

    [Hook]
    private void OnDispose() {
      ReleaseController();
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
      Controller.Bind(CurrentScroller());
    }

    private void GeometryChangedHandler(GeometryChangedEvent evt) => SynchronizeControllerOptions();

    private void SynchronizeControllerOptions() {
      if (Controller == null) return;
      var scroller = CurrentScroller();
      if (scroller == null) return;

      var min = FiniteOrZero(scroller.lowValue);
      var max = Mathf.Max(min, FiniteOrZero(scroller.highValue));
      var thumbRange = props.axis == Axis.Vertical
        ? FiniteOrZero(viewport.contentViewport.layout.height)
        : FiniteOrZero(viewport.contentViewport.layout.width);
      var options = new SliderOptions(
        min, max,
        axis: props.axis, reverse: props.reverse, thumbRange: Mathf.Max(0f, thumbRange)
      );
      Controller.SynchronizeScrollValue(scroller.value, in options);
    }

    private Scroller CurrentScroller() {
      return props.axis == Axis.Vertical ? viewport.verticalScroller : viewport.horizontalScroller;
    }

    private void ReleaseController() {
      if (Controller == null) return;
      Controller.Unbind();
      if (_automaticController) Controller.Dispose();
      Controller = null;
      _automaticController = false;
    }

    private static float FiniteOrZero(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
  }

  public static class ScrollViewExtensions {
    public static ScopeHandle ScrollView(
      this ref Composition cx,
      ScrollerSliderController controller = null,
      Axis axis = Axis.Vertical,
      bool reverse = false,
      bool showSlider = true,
      Composable<SliderController> slider = null,
      SliderStyle? sliderStyle = null
    ) {
      ref var boundaryRef = ref HXScrollViewElement.ComposeBoundary(
        ref cx, controller, axis, reverse, showSlider, slider, sliderStyle
      ); // Note: This depends on immediate inlined forward composition.
      var boundary = boundaryRef.element as HXScrollViewElement;
      if (boundary == null) throw new InvalidOperationException("Scroll view boundary was not initialized.");
      return boundary.content.Scope(ref cx);
    }
  }
}
