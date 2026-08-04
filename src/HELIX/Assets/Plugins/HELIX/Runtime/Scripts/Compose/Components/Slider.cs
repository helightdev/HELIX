using System;
using HELIX.Extensions;
using HELIX.Signals;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [PropStruct] public readonly partial struct SliderOptions : IEquatable<SliderOptions> {
    public static readonly SliderOptions Default = new(0f, 1f);

    public readonly float min;
    public readonly float max;
    [Prop(0f)] public readonly float step;
    [Prop(Axis.Horizontal)] public readonly Axis axis;
    [Prop(false)] public readonly bool reverse;
    [Prop(0f)] public readonly float thumbRange;
  }

  [PropStruct] public readonly partial struct SliderStyle : IEquatable<SliderStyle> {
    [Prop(Equatable = false)] public readonly HXControlBoxStyle box;
    public readonly Composable<State> track;
    public readonly Composable<State> progress;
    public readonly Composable<State> thumb;
    [Prop(4f)] public readonly float trackSize;
    [Prop(16f)] public readonly float thumbSize;
  }

  [ComposableProxy(Extension = false)]
  public sealed partial class HXSliderElement : ComposableElement, ISlotHost {
    public static readonly UniqueStyleString ClassTrack = new("hx-slider-track");
    public static readonly UniqueStyleString ClassThumb = new("hx-slider-thumb");

    public readonly ComposableSlot track;
    public readonly ComposableSlot thumb;

    private SliderOptions _options;
    public float Value { get; set; }
    public float TrackSize { get; set; }
    public float ThumbSize { get; set; }

    public HXSliderElement() {
      pickingMode = PickingMode.Ignore;
      this.MakeRelative();

      track = new ComposableSlot(this, ClassTrack)
        .WithClasses(ClassTrack)
        .MakeAbsolute()
        .AddTo(this);
      thumb = new ComposableSlot(this, ClassThumb)
        .WithClasses(ClassThumb)
        .MakeAbsolute()
        .AddTo(this);

      RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    public IBoundary Boundary { get; set; }
    public SliderOptions Options { get => _options; set => _options = value; }

    public void Update(
      [Prop] IBoundary boundary,
      [Prop] float value,
      [Prop] in SliderOptions options,
      [Prop] float trackSize,
      [Prop] float thumbSize
    ) {
      Boundary = boundary;
      Value = value;
      Options = options;
      TrackSize = trackSize;
      ThumbSize = thumbSize;
      ApplyLayout();
    }


    public override void Reset() {
      base.Reset();
      track.Reset();
      thumb.Reset();
      Boundary = null;
      Options = SliderOptions.Default;
      Value = 0f;
      TrackSize = 0f;
      ThumbSize = 0f;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) => ApplyLayout();

    public void ApplyLayout() {
      var normalized = NormalizeValue(Value, in _options);
      if (Options.axis == Axis.Horizontal) {
        var thumbMainSize = ResolveThumbSize(contentRect.width, in _options, ThumbSize);
        var offset = normalized * Mathf.Max(0f, contentRect.width - thumbMainSize);
        track.style.left = contentRect.xMin;
        track.style.top = contentRect.yMin + (contentRect.height - TrackSize) * 0.5f;
        track.style.width = contentRect.width;
        track.style.height = TrackSize;
        thumb.style.left = contentRect.xMin + offset;
        thumb.style.top = contentRect.yMin + (contentRect.height - ThumbSize) * 0.5f;
        thumb.style.width = thumbMainSize;
        thumb.style.height = ThumbSize;
      } else {
        var thumbMainSize = ResolveThumbSize(contentRect.height, in _options, ThumbSize);
        var offset = normalized * Mathf.Max(0f, contentRect.height - thumbMainSize);
        track.style.left = contentRect.xMin + (contentRect.width - TrackSize) * 0.5f;
        track.style.top = contentRect.yMin;
        track.style.width = TrackSize;
        track.style.height = contentRect.height;
        thumb.style.left = contentRect.xMin + (contentRect.width - ThumbSize) * 0.5f;
        thumb.style.top = contentRect.yMin + offset;
        thumb.style.width = ThumbSize;
        thumb.style.height = thumbMainSize;
      }
    }

    internal static SliderOptions NormalizeOptions(in SliderOptions options) {
      if (options.max >= options.min) return options;
      return new SliderOptions(
        options.max, options.min, options.step, options.axis, options.reverse, options.thumbRange
      );
    }

    internal static float ClampAndSnap(float value, in SliderOptions options) {
      var result = Mathf.Clamp(value, options.min, options.max);
      if (options.step <= 0f || result <= options.min || result >= options.max) return result;

      var snapped = options.min + Mathf.Round((result - options.min) / options.step) * options.step;
      snapped = Mathf.Clamp(snapped, options.min, options.max);
      return options.max - result <= Mathf.Abs(snapped - result) ? options.max : snapped;
    }

    internal static float NormalizeValue(float value, in SliderOptions options) {
      var range = options.max - options.min;
      if (Mathf.Approximately(range, 0f)) return 0f;
      var normalized = Mathf.Clamp01((value - options.min) / range);
      return options.reverse ? 1f - normalized : normalized;
    }

    internal static float ResolveThumbSize(float length, in SliderOptions options, float minimum) {
      if (options.thumbRange <= 0f) return Mathf.Min(length, minimum);

      var valueRange = Mathf.Max(0f, options.max - options.min);
      var totalRange = valueRange + options.thumbRange;
      if (totalRange <= 0f) return length;
      return Mathf.Clamp(length * options.thumbRange / totalRange, minimum, length);
    }
  }

  public class SliderController : Signal<float> {
    private float _value;

    public CompositionAction<float> onChanged;
    public CompositionAction<float> onCommitted;
    public SliderOptions options = SliderOptions.Default;
    public bool enabled = true;
    public bool error;

    public SliderController(float initialValue = 0f)
      : this(initialValue, "SliderController", typeof(SliderController)) { }

    protected SliderController(float initialValue, string name, Type registeredType)
      : base(name, registeredType) {
      _value = initialValue;
    }


    public State State {
      get {
        var state = State.None;
        state |= enabled ? State.None : State.Disabled;
        state |= error ? State.Error : State.None;
        return state;
      }
    }

    public override float PeekValue() => _value;

    public override void SetValue(float newValue) {
      newValue = NormalizeValue(newValue);
      if (Mathf.Approximately(_value, newValue)) return;
      _value = newValue;
      NotifyListeners();
    }

    public override void SetWithoutNotify(float newValue) {
      _value = NormalizeValue(newValue);
      NotifyDirty();
    }

    internal void SynchronizeValue(float newValue) {
      _value = NormalizeValue(newValue);
    }

    internal void SynchronizeScrollValue(
      float newValue,
      in SliderOptions newOptions,
      bool notifyObservers = true
    ) {
      var normalizedOptions = HXSliderElement.NormalizeOptions(in newOptions);
      newValue = HXSliderElement.ClampAndSnap(newValue, in normalizedOptions);
      var changed = !options.Equals(normalizedOptions) || !Mathf.Approximately(_value, newValue);
      if (!changed) return;

      options = normalizedOptions;
      _value = newValue;
      NotifyDirty();
      if (notifyObservers) NotifyObservers();
    }

    internal void SetUserValue(IBoundary boundary, float newValue, bool commit) {
      newValue = NormalizeValue(newValue);
      var changed = !Mathf.Approximately(_value, newValue);
      if (changed) {
        _value = newValue;
        onChanged?.Call(boundary, newValue);
      }
      if (commit) onCommitted?.Call(boundary, newValue);
      if (changed) NotifyListeners();
    }

    private float NormalizeValue(float newValue) {
      var normalizedOptions = HXSliderElement.NormalizeOptions(in options);
      return HXSliderElement.ClampAndSnap(newValue, in normalizedOptions);
    }

    protected virtual void NotifyListeners() {
      NotifyDirty();
      NotifyObservers();
    }
  }

  public sealed class ScrollerSliderController : SliderController {
    private bool _synchronizing;

    public ScrollerSliderController(float initialValue = 0f)
      : base(initialValue, "ScrollerSliderController", typeof(ScrollerSliderController)) { }

    public Scroller Scroller { get; private set; }

    public void Bind(Scroller scroller, bool syncValueFromScroller = true) {
      if (ReferenceEquals(Scroller, scroller)) {
        if (syncValueFromScroller) RefreshFromScroller();
        else SynchronizeScroller();
        return;
      }

      Unbind();
      Scroller = scroller;
      if (Scroller == null) return;

      Scroller.valueChanged += OnScrollerValueChanged;
      if (syncValueFromScroller) RefreshFromScroller();
      else SynchronizeScroller();
    }

    public void Unbind() {
      if (Scroller == null) return;
      Scroller.valueChanged -= OnScrollerValueChanged;
      Scroller = null;
    }

    public void RefreshFromScroller() {
      if (Scroller == null) return;
      SetValue(Scroller.value);
    }

    public override void SetWithoutNotify(float newValue) {
      base.SetWithoutNotify(newValue);
      SynchronizeScroller();
    }

    public override void Dispose() {
      Unbind();
      base.Dispose();
    }

    protected override void NotifyListeners() {
      SynchronizeScroller();
      base.NotifyListeners();
    }

    private void OnScrollerValueChanged(float value) {
      if (_synchronizing) return;
      SetValue(value);
    }

    private void SynchronizeScroller() {
      if (Scroller == null || Mathf.Approximately(Scroller.value, PeekValue())) return;
      try {
        _synchronizing = true;
        Scroller.value = PeekValue();
      } finally {
        _synchronizing = false;
      }
    }
  }

  [BoundaryComposable(Base = typeof(InputClickableComposable<>), Extension = true)]
  public partial class Slider {
    public partial struct Props {
      // Keep value first to preserve the cx.Slider(value, ...) call shape.
      [Prop(null)] public float? value;
      [Prop(null)] public SliderController controller;
      [Prop(null)] public float? initialValue;
      [Prop(null)] public CompositionAction<float> onChanged;
      [Prop(null)] public CompositionAction<float> onCommitted;
      [Prop("SliderOptions.Default", PropInit.Deferred)] public SliderOptions options;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool error;
      [Prop(null)] public SliderStyle? style;
    }

    public SliderController controller;
    public bool isAutomaticController = true;

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<KeyDownEvent>(OnKeyDown);
      DisposeAutomaticController();
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      EnsureController(props.controller);
      cx.SubscribeTo(controller);

      var options = HXSliderElement.NormalizeOptions(in controller.options);
      var value = HXSliderElement.ClampAndSnap(controller.PeekValue(), in options);
      var style = props.style ?? ThemeProperties.Slider[in cx];

      this.Toggle(State.Disabled, !controller.enabled);
      this.Toggle(State.Error, controller.error);
      var passedState = controller.State | InputState;
      cx.CURSOR.Focusable(controller.enabled);
      style.box.RenderBoundary(ref cx, passedState);

      var normalized = HXSliderElement.NormalizeValue(value, in options);
      var elementHandle = HXSliderElement.Compose(ref cx, Node, value, in options, style.trackSize, style.thumbSize);
      elementHandle.Flexible().AlignSelf(Align.Stretch).Focusable(false, pickingMode: PickingMode.Ignore);

      var element = (HXSliderElement)elementHandle.composable;
      using (element.track.Scope(cx)) {
        style.track?.Invoke(ref cx, passedState);
        ComposeProgress(ref cx, style.progress, passedState, normalized, options.axis);
      }

      using (element.thumb.Scope(cx)) {
        style.thumb?.Invoke(ref cx, passedState);
      }
    }

    public void EnsureController(SliderController given) {
      if (ReferenceEquals(given, controller) && controller != null) return;
      if (given == null) {
        if (isAutomaticController && controller != null) {
          ConfigureAutomaticController();
          controller.SynchronizeValue(props.value ?? controller.PeekValue());
        } else {
          controller = new SliderController();
          isAutomaticController = true;
          ConfigureAutomaticController();
          controller.SynchronizeValue(props.value ?? props.initialValue ?? 0f);
        }
      } else {
        DisposeAutomaticController();
        controller = given;
      }
    }

    private void ConfigureAutomaticController() {
      controller.onChanged = props.onChanged;
      controller.onCommitted = props.onCommitted;
      controller.options = props.options;
      controller.enabled = props.enabled;
      controller.error = props.error;
    }

    private void DisposeAutomaticController() {
      if (!isAutomaticController) return;
      controller?.Dispose();
      isAutomaticController = false;
    }

    protected override void OnPointerDown(PointerDownEvent evt) {
      if (controller == null || !controller.enabled) return;
      base.OnPointerDown(evt);
      if (!Active) return;
      this.Enable(State.Active);
      SetFromLocalPosition(evt.localPosition, false);
    }

    protected override void OnPointerMove(PointerMoveEvent evt) {
      base.OnPointerMove(evt);
      if (Active) SetFromLocalPosition(evt.localPosition, false);
    }

    protected override void OnPointerUp(PointerUpEvent evt) {
      if (!Active) return;
      SetFromLocalPosition(evt.localPosition, true);
      this.Disable(State.Active);
      base.OnPointerUp(evt);
    }

    protected override void Cancel(EventBase evt, int pointerId) {
      this.Disable(State.Active);
      base.Cancel(evt, pointerId);
    }

    private void OnKeyDown(KeyDownEvent evt) {
      if (controller == null || !controller.enabled) return;
      var options = HXSliderElement.NormalizeOptions(in controller.options);
      var direction = evt.keyCode switch {
        KeyCode.LeftArrow => options.axis == Axis.Horizontal ? -1 : 0,
        KeyCode.RightArrow => options.axis == Axis.Horizontal ? 1 : 0,
        KeyCode.DownArrow => options.axis == Axis.Vertical ? 1 : 0,
        KeyCode.UpArrow => options.axis == Axis.Vertical ? -1 : 0,
        _ => 0
      };
      if (direction == 0) return;
      if (options.reverse) direction = -direction;

      var amount = options.step > 0f
        ? options.step
        : Mathf.Max((options.max - options.min) * 0.01f, Mathf.Epsilon);
      var value = HXSliderElement.ClampAndSnap(controller.PeekValue() + direction * amount, in options);
      if (!Mathf.Approximately(controller.PeekValue(), value)) { controller.SetUserValue(Node, value, true); }
      evt.StopPropagation();
    }

    private void SetFromLocalPosition(Vector2 localPosition, bool commit) {
      if (controller == null) return;
      var options = HXSliderElement.NormalizeOptions(in controller.options);
      var style = props.style ?? ThemeProperties.Slider[Node];
      var length = options.axis == Axis.Horizontal ? Node.contentRect.width : Node.contentRect.height;
      var thumbSize = HXSliderElement.ResolveThumbSize(length, in options, style.thumbSize);
      var available = Mathf.Max(0f, length - thumbSize);
      if (available <= 0f) return;

      var position = options.axis == Axis.Horizontal ? localPosition.x : localPosition.y;
      var start = options.axis == Axis.Horizontal ? Node.contentRect.xMin : Node.contentRect.yMin;
      var normalized = Mathf.Clamp01((position - start - thumbSize * 0.5f) / available);
      if (options.reverse) normalized = 1f - normalized;

      var value = HXSliderElement.ClampAndSnap(Mathf.Lerp(options.min, options.max, normalized), in options);
      controller.SetUserValue(Node, value, commit);
    }

    private static void ComposeProgress(
      ref Composition cx,
      Composable<State> progress,
      State state,
      float normalized,
      Axis axis
    ) {
      if (progress == null) return;
      using (cx.Container()) {
        cx.CURSOR
          .Absolute()
          .Position(StyleLength4.Zero)
          .Width(axis == Axis.Horizontal ? normalized.NormalizedPercent() : 100f.Percent())
          .Height(axis == Axis.Vertical ? normalized.NormalizedPercent() : 100f.Percent())
          .Focusable(false, pickingMode: PickingMode.Ignore);
        progress.Invoke(ref cx, state);
      }
    }
  }
}