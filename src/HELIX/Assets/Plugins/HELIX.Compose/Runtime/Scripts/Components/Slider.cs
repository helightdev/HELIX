using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [Mixable, Structure] public readonly partial struct SliderOptions : IEquatable<SliderOptions> {
    public static readonly SliderOptions Default = new(0f, 1f);

    public readonly float min;
    public readonly float max;
    [Prop(0f)] public readonly float step;
    [Prop(Axis.Horizontal)] public readonly Axis axis;
    [Prop(false)] public readonly bool reverse;
    [Prop(0f)] public readonly float thumbRange;
  }

  [Mixable, Structure] public readonly partial struct SliderStyle : IEquatable<SliderStyle> {
    [Prop(Equatable = false)] public readonly HXControlBoxStyle box;
    public readonly Composable<State> track;
    public readonly Composable<State> progress;
    public readonly Composable<State> thumb;
    [Prop(4f)] public readonly float trackSize;
    [Prop(16f)] public readonly float thumbSize;
    [Prop(false)] public readonly bool hideWhenThumbCoversTrack;
  }

  [Mixable]
  [CustomBoundaryElement(false, extension: true, trimChildren: false, name: "Slider")]
  [InputStateListener]
  public sealed partial class HXSliderElement : VisualElement, ISlotHost {
    public static readonly UniqueStyleString ClassTrack = new("hx-slider-track");
    public static readonly UniqueStyleString ClassThumb = new("hx-slider-thumb");
    public readonly ComposableSlot thumb;

    public readonly ComposableSlot track;
    private bool _automaticallyHidden;
    private float _lastTrackLength;

    private SliderOptions _options;
    public SliderController controller;
    public bool isAutomaticController = true;

    public HXSliderElement() {
      pickingMode = PickingMode.Position;
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
      RegisterCallback<KeyDownEvent>(OnKeyDown);
      this.AddManipulator(new SliderManipulator(this));
      RegisterCallback<AttachToPanelEvent>(_ => AttachBoundary());
      RegisterCallback<DetachFromPanelEvent>(_ => DetachBoundary());
      PostConstruct();
    }

    public float Value { get; set; }
    public float TrackSize { get; set; }
    public float ThumbSize { get; set; }

    public SliderOptions Options {
      get => _options;
      set => _options = value;
    }

    public IBoundary Boundary => this;

    [Hook]
    private void OnCompose(ref Composition cx) {
      EnsureController(props.controller);
      cx.SubscribeTo(controller);

      var options = NormalizeOptions(in controller.options);
      var value = ClampAndSnap(controller.PeekValue(), in options);
      var sliderStyle = props.style ?? ThemeProperties.Slider[in cx];
      UpdateDisplay(in sliderStyle, in options);

      this.Toggle(State.Disabled, !controller.enabled);
      this.Toggle(State.Error, controller.error);
      var passedState = controller.State | InputState;
      this.Focusable(controller.enabled);
      sliderStyle.box.RenderBoundary(ref cx, passedState);

      Value = value;
      Options = options;
      TrackSize = sliderStyle.trackSize;
      ThumbSize = sliderStyle.thumbSize;
      ApplyLayout();

      var normalized = NormalizeValue(value, in options);
      using (track.Scope(ref cx)) {
        sliderStyle.track?.Invoke(ref cx, passedState);
        ComposeProgress(ref cx, sliderStyle.progress, passedState, normalized, options.axis);
      }
      using (thumb.Scope(ref cx)) sliderStyle.thumb?.Invoke(ref cx, passedState);
    }


    [Hook]
    private void OnReset() {
      track.Reset();
      thumb.Reset();
      Options = SliderOptions.Default;
      Value = 0f;
      TrackSize = 0f;
      ThumbSize = 0f;
    }

    [Hook]
    private void OnDispose() {
      if (_automaticallyHidden) style.display = DisplayStyle.Flex;
      _automaticallyHidden = false;
      _lastTrackLength = 0f;
      DisposeAutomaticController();
    }

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
        options.max,
        options.min,
        options.step,
        options.axis,
        options.reverse,
        options.thumbRange
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
      controller = null;
      isAutomaticController = false;
    }

    private void OnKeyDown(KeyDownEvent evt) {
      if (controller == null || !controller.enabled) return;
      var options = NormalizeOptions(in controller.options);
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
      var value = ClampAndSnap(controller.PeekValue() + direction * amount, in options);
      if (!Mathf.Approximately(controller.PeekValue(), value)) controller.SetUserValue(this, value, true);
      evt.StopPropagation();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
      ApplyLayout();
      if (controller == null) return;
      var options = NormalizeOptions(in controller.options);
      var sliderStyle = props.style ?? ThemeProperties.Slider[this];
      UpdateDisplay(in sliderStyle, in options);
    }

    private void UpdateDisplay(in SliderStyle sliderStyle, in SliderOptions options) {
      var length = options.axis == Axis.Horizontal ? contentRect.width : contentRect.height;
      if (length > 0f) _lastTrackLength = length;
      var hide = sliderStyle.hideWhenThumbCoversTrack && _lastTrackLength > 0f &&
        Mathf.Approximately(ResolveThumbSize(_lastTrackLength, in options, sliderStyle.thumbSize), _lastTrackLength);
      if (hide) {
        style.display = DisplayStyle.None;
        _automaticallyHidden = true;
      } else if (_automaticallyHidden) {
        style.display = DisplayStyle.Flex;
        _automaticallyHidden = false;
      }
    }

    private void SetFromLocalPosition(Vector2 localPosition, bool commit) {
      if (controller == null) return;
      var options = NormalizeOptions(in controller.options);
      var sliderStyle = props.style ?? ThemeProperties.Slider[this];
      var length = options.axis == Axis.Horizontal ? contentRect.width : contentRect.height;
      var thumbSize = ResolveThumbSize(length, in options, sliderStyle.thumbSize);
      var available = Mathf.Max(0f, length - thumbSize);
      if (available <= 0f) return;

      var position = options.axis == Axis.Horizontal ? localPosition.x : localPosition.y;
      var start = options.axis == Axis.Horizontal ? contentRect.xMin : contentRect.yMin;
      var normalized = Mathf.Clamp01((position - start - thumbSize * 0.5f) / available);
      if (options.reverse) normalized = 1f - normalized;
      var value = ClampAndSnap(Mathf.Lerp(options.min, options.max, normalized), in options);
      controller.SetUserValue(this, value, commit);
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

    public partial struct Props {
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

    private sealed class SliderManipulator : InputClickableManipulator {
      private readonly HXSliderElement _slider;

      public SliderManipulator(HXSliderElement slider) : base(slider, slider) {
        _slider = slider;
      }

      public override void OnPointerDown(PointerDownEvent evt) {
        if (_slider.controller?.enabled != true) return;
        base.OnPointerDown(evt);
        if (Active) _slider.SetFromLocalPosition(evt.localPosition, false);
      }

      public override void OnPointerMove(PointerMoveEvent evt) {
        base.OnPointerMove(evt);
        if (Active) _slider.SetFromLocalPosition(evt.localPosition, false);
      }

      public override void OnPointerUp(PointerUpEvent evt) {
        if (Active) _slider.SetFromLocalPosition(evt.localPosition, true);
        base.OnPointerUp(evt);
      }
    }
  }

  public class SliderController : Signal<float> {
    private float _value;
    public bool enabled = true;
    public bool error;

    public CompositionAction<float> onChanged;
    public CompositionAction<float> onCommitted;
    public SliderOptions options = SliderOptions.Default;

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

    public override float PeekValue() {
      return _value;
    }

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
}
