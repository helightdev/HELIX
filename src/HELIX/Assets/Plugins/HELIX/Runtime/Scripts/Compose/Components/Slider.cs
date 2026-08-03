using System;
using HELIX.Extensions;
using HELIX.Signals;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct SliderOptions : IEquatable<SliderOptions> {
    public static readonly SliderOptions Default = new(0f, 1f);

    public readonly float min;
    public readonly float max;
    public readonly float step;
    public readonly float thumbRange;
    public readonly Axis axis;
    public readonly bool reverse;

    public SliderOptions(
      float min,
      float max,
      float step = 0f,
      Axis axis = Axis.Horizontal,
      bool reverse = false,
      float thumbRange = 0f
    ) {
      this.min = min;
      this.max = max;
      this.step = Mathf.Max(0f, step);
      this.thumbRange = Mathf.Max(0f, thumbRange);
      this.axis = axis;
      this.reverse = reverse;
    }

    public bool Equals(SliderOptions other) {
      return min.Equals(other.min) &&
             max.Equals(other.max) &&
             step.Equals(other.step) &&
             thumbRange.Equals(other.thumbRange) &&
             axis == other.axis &&
             reverse == other.reverse;
    }

    public override bool Equals(object obj) => obj is SliderOptions other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(min, max, step, thumbRange, (int)axis, reverse);
  }

  public readonly struct SliderStyle {
    public readonly HXControlBoxStyle box;
    public readonly Composable<State> track;
    public readonly Composable<State> progress;
    public readonly Composable<State> thumb;
    public readonly float trackSize;
    public readonly float thumbSize;

    public SliderStyle(
      HXControlBoxStyle box,
      Composable<State> track,
      Composable<State> progress,
      Composable<State> thumb,
      float trackSize = 4f,
      float thumbSize = 16f
    ) {
      this.box = box;
      this.track = track;
      this.progress = progress;
      this.thumb = thumb;
      this.trackSize = Mathf.Max(0f, trackSize);
      this.thumbSize = Mathf.Max(0f, thumbSize);
    }
  }

  public static class SliderElementExtensions {
    private static readonly ushort _sliderElementId = CompositionId.GetTypeId("SliderElement");

    public static ScopeHandle SliderElement(
      this ref Composition cx,
      out SliderElementSlots slots,
      float value,
      in SliderOptions options,
      float trackSize,
      float thumbSize
    ) {
      if (!cx.AUTHORING.RequireComposable<HXSliderElement>(_sliderElementId, out var element, out var retained)) {
        element = new HXSliderElement();
      }
      if (!retained) element.Initialize(cx);

      element.Update(value, in options, trackSize, thumbSize);
      slots = new SliderElementSlots(element, cx);
      return cx.AUTHORING.YieldScope(ref cx, element);
    }
  }

  public readonly ref struct SliderElementSlots {
    private readonly HXSliderElement _element;
    private readonly Composition _composition;

    public SliderElementSlots(HXSliderElement element, Composition composition) {
      _element = element;
      _composition = composition;
    }

    public ScopeHandle Track() => _element.track.Scope(_composition);
    public ScopeHandle Thumb() => _element.thumb.Scope(_composition);
  }

  public sealed class HXSliderElement : VisualElement, ISlotHost {
    public static readonly UniqueStyleString ClassTrack = new("hx-slider-track");
    public static readonly UniqueStyleString ClassThumb = new("hx-slider-thumb");

    public readonly ComposableSlot track;
    public readonly ComposableSlot thumb;

    private SliderOptions _options;
    private float _value;
    private float _trackSize;
    private float _thumbSize;

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

    public VisualElement Element => this;
    public UssFlag Flag { get; set; }
    public ulong PackedId { get; set; }
    public IBoundary Boundary { get; private set; }
    public SliderOptions Options => _options;

    public void Initialize(in Composition cx) {
      Boundary = cx.boundary;
    }

    public void Update(float value, in SliderOptions options, float trackSize, float thumbSize) {
      _options = NormalizeOptions(in options);
      _value = ClampAndSnap(value, in _options);
      _trackSize = Mathf.Max(0f, trackSize);
      _thumbSize = Mathf.Max(0f, thumbSize);
      ApplyLayout();
    }

    public void Reset() {
      track.Reset();
      thumb.Reset();
      Boundary = null;
      _options = SliderOptions.Default;
      _value = 0f;
      _trackSize = 0f;
      _thumbSize = 0f;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) => ApplyLayout();

    private void ApplyLayout() {
      var normalized = NormalizeValue(_value, in _options);
      if (_options.axis == Axis.Horizontal) {
        var thumbMainSize = ResolveThumbSize(contentRect.width, in _options, _thumbSize);
        var offset = normalized * Mathf.Max(0f, contentRect.width - thumbMainSize);
        track.style.left = contentRect.xMin;
        track.style.top = contentRect.yMin + (contentRect.height - _trackSize) * 0.5f;
        track.style.width = contentRect.width;
        track.style.height = _trackSize;
        thumb.style.left = contentRect.xMin + offset;
        thumb.style.top = contentRect.yMin + (contentRect.height - _thumbSize) * 0.5f;
        thumb.style.width = thumbMainSize;
        thumb.style.height = _thumbSize;
      } else {
        var thumbMainSize = ResolveThumbSize(contentRect.height, in _options, _thumbSize);
        var offset = normalized * Mathf.Max(0f, contentRect.height - thumbMainSize);
        track.style.left = contentRect.xMin + (contentRect.width - _trackSize) * 0.5f;
        track.style.top = contentRect.yMin;
        track.style.width = _trackSize;
        track.style.height = contentRect.height;
        thumb.style.left = contentRect.xMin + (contentRect.width - _thumbSize) * 0.5f;
        thumb.style.top = contentRect.yMin + offset;
        thumb.style.width = _thumbSize;
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

  public sealed class SliderController : Signal<float> {
    private float _value;

    public CompositionAction<float> onChanged;
    public CompositionAction<float> onCommitted;
    public SliderOptions options = SliderOptions.Default;
    public bool enabled = true;
    public bool error;

    public SliderController(float initialValue = 0f) : base("SliderController", typeof(SliderController)) {
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

    private void NotifyListeners() {
      NotifyDirty();
      NotifyObservers();
    }
  }

  [BoundaryComposable(Base = typeof(InputClickableComposable<>), Extension = true)]
  public partial class Slider {
    public partial struct Props {
      // Keep value first to preserve the cx.Slider(value, ...) call shape.
      [PropDefault(null)] public float? value;
      [PropDefault(null)] public SliderController controller;
      [PropDefault(null)] public float? initialValue;
      [PropDefault(null)] public CompositionAction<float> onChanged;
      [PropDefault(null)] public CompositionAction<float> onCommitted;
      [PropDefault("SliderOptions.Default", PropInit.Deferred)] public SliderOptions options;
      [PropDefault(true)] public bool enabled;
      [PropDefault(false)] public bool error;
      [PropDefault(null)] public SliderStyle? style;
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
      using (cx.SliderElement(
        out var slots, value, in options, style.trackSize, style.thumbSize
      )) {
        cx.CURSOR.Flexible().AlignSelf(Align.Stretch).Focusable(false, pickingMode: PickingMode.Ignore);

        using (slots.Track()) {
          style.track?.Invoke(ref cx, passedState);
          ComposeProgress(ref cx, style.progress, passedState, normalized, options.axis);
        }

        using (slots.Thumb()) {
          style.thumb?.Invoke(ref cx, passedState);
        }
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