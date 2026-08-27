using System;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;
using NativeScrollView = UnityEngine.UIElements.ScrollView;

namespace HELIX.Compose {

  [EnableMixins]
  public sealed partial class HXScrollViewElement : ComposableElement, ISlotHost {
    public static readonly UniqueStyleString ClassViewport = new("hx-scroll-view-viewport");
    public static readonly UniqueStyleString ClassContent = new("hx-scroll-view-content");
    public static readonly UniqueStyleString ClassSlider = new("hx-scroll-view-slider");

    public readonly NativeScrollView viewport;
    public readonly ComposableSlot content;
    public readonly ComposableSlot slider;

    public IBoundary Boundary { get; set; }

    public Action<GeometryChangedEvent> OnGeometryChanged { get; set; }


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
    }

    private void GeometryChangedHandler(GeometryChangedEvent evt) {
      OnGeometryChanged?.Invoke(evt);
    }

    [ComposableMethod]
    public void Update(
      [Prop] IBoundary boundary,
      [Prop(null)] Action<GeometryChangedEvent> onChanged,
      [Prop(Axis.Vertical)] Axis axis
    ) {
      Boundary = boundary;
      OnGeometryChanged = onChanged;

      style.flexDirection = axis == Axis.Vertical ? FlexDirection.Row : FlexDirection.Column;
      slider.style.flexDirection = axis == Axis.Vertical ? FlexDirection.Column : FlexDirection.Row;
      content.style.flexDirection = axis.ToFlexDirection();
      viewport.mode = axis == Axis.Vertical ? ScrollViewMode.Vertical : ScrollViewMode.Horizontal;
    }


    public override void Reset() {
      base.Reset();
      content.Reset();
      slider.Reset();
      Boundary = null;
    }
  }

  [EnableMixins]
  [BoundaryComposableMixin]
  public partial class ScrollViewBoundary {
    public partial struct Props {
      [Prop(null)] public ScrollerSliderController controller;
      [Prop(Axis.Vertical)] public Axis axis;
      [Prop(false)] public bool reverse;
      [Prop(true)] public bool showSlider;
      [Prop(null)] public Composable<SliderController> slider;
      [Prop(null)] public SliderStyle? sliderStyle;
    }

    public ScrollerSliderController Controller { get; private set; }
    public HXScrollViewElement ViewElement { get; private set; }
    public ScrollView ScrollView => ViewElement.viewport;

    private bool _automaticController;

    protected override void OnAttach() { }

    protected override void OnDetach() {
      ReleaseController();
      ViewElement = null;
    }

    protected override void OnRecompose(ref Composition cx) {
      Node.style.flexGrow = 1f;
      Node.style.flexShrink = 1f;
      Node.style.alignSelf = Align.Stretch;

      HXScrollViewElement.Compose(ref cx, Node.Parent, OnGeometryChanged, props.axis);
      ViewElement = (HXScrollViewElement)cx.CURSOR.element;
      cx.CURSOR.Flexible().AlignSelf(Align.Stretch).Focusable(false, pickingMode: PickingMode.Ignore);

      EnsureController();

      using (ViewElement.slider.Scope(ref cx)) {
        if (!props.showSlider) return;
        if (props.slider != null) {
          props.slider(ref cx, Controller);
        } else {
          cx.Slider(controller: Controller, style: props.sliderStyle ?? ThemeProperties.Scroller[in cx]).Flexible();
        }
      }
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

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      SynchronizeControllerOptions();
    }

    private void SynchronizeControllerOptions() {
      if (Controller == null) return;
      var scroller = CurrentScroller();
      if (scroller == null) return;

      var min = FiniteOrZero(scroller.lowValue);
      var max = Mathf.Max(min, FiniteOrZero(scroller.highValue));
      var thumbRange = props.axis == Axis.Vertical
        ? FiniteOrZero(ScrollView.contentViewport.layout.height)
        : FiniteOrZero(ScrollView.contentViewport.layout.width);
      var options = new SliderOptions(
        min, max,
        axis: props.axis, reverse: props.reverse, thumbRange: Mathf.Max(0f, thumbRange)
      );
      Controller.SynchronizeScrollValue(scroller.value, in options);
    }

    private Scroller CurrentScroller() {
      if (ViewElement == null) return null;
      return props.axis == Axis.Vertical ? ScrollView.verticalScroller : ScrollView.horizontalScroller;
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
      ref var boundaryRef = ref ScrollViewBoundary.ComposeBoundary(
        ref cx, controller, axis, reverse, showSlider, slider, sliderStyle
      ); // Note: This depends on immediate inlined forward composition.
      var node = boundaryRef.element as CompositionBoundaryNodeBase;
      var boundary = node?.BoundaryComposable as ScrollViewBoundary;
      if (boundary == null) throw new InvalidOperationException("Scroll view boundary was not initialized.");
      return boundary.ViewElement.content.Scope(ref cx);
    }
  }
}
