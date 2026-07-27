using System;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.NW {
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

  public sealed class SliderStyle {
    public static readonly SliderStyle Default = BuildDefault(HXThemes.DefaultDark);
    public static readonly ContextKey<SliderStyle> Context = new("SliderStyle", Default);

    public readonly InputFieldStyle box;
    public readonly StateComposable track;
    public readonly StateComposable progress;
    public readonly StateComposable thumb;
    public readonly float trackSize;
    public readonly float thumbSize;

    public SliderStyle(
      InputFieldStyle box,
      StateComposable track,
      StateComposable progress,
      StateComposable thumb,
      float trackSize = 4f,
      float thumbSize = 16f
    ) {
      this.box = box ?? InputFieldStyle.Default;
      this.track = track;
      this.progress = progress;
      this.thumb = thumb;
      this.trackSize = Mathf.Max(0f, trackSize);
      this.thumbSize = Mathf.Max(0f, thumbSize);
    }

    public static SliderStyle BuildDefault(ThemeData theme) {
      var box = new InputFieldStyle(
        padding: StyleLength4.Zero,
        constraints: BoxConstraints.Min(new StyleLength2(32f))
      );

      var progress = new StatePropertyMap<Color>();
      progress[StateFlag.Disabled] = theme.GetColor(ColorRoles.OnSurfaceDisabledLow);
      progress[StateFlag.None] = theme.GetColor(ColorRoles.Primary);

      var thumb = new StatePropertyMap<Color>();
      thumb[StateFlag.Disabled] = theme.GetColor(ColorRoles.OnSurfaceDisabledHigh);
      thumb[StateFlag.Pressed | StateFlag.ModAny] = theme.GetColor(ColorRoles.OnPrimary);
      thumb[StateFlag.Focused] = theme.GetColor(ColorRoles.Focus);
      thumb[StateFlag.None] = theme.GetColor(ColorRoles.Primary);

      return new SliderStyle(
        box,
        new DrawSolidBoxStyle(
          radius: BorderRadius.All(2f),
          color: theme.GetColor(ColorRoles.Outline)
        ).Bake(),
        new DrawSolidBoxStyle(
          radius: BorderRadius.All(2f),
          color: progress
        ).Bake(),
        new DrawSolidBoxStyle(
          radius: BorderRadius.All(8f),
          color: thumb
        ).Bake()
      );
    }
  }

  internal sealed class SliderInputElement : VisualElement, IComposable {
    private readonly CompositionBoundaryNode _background;
    private readonly CompositionBoundaryNode _track;
    private readonly CompositionBoundaryNode _progress;
    private readonly CompositionBoundaryNode _thumb;

    private SliderOptions _options = SliderOptions.Default;
    private SliderStyle _sliderStyle;
    private StateFlag _inputState;
    private CompositionAction<float> _onChanged;
    private CompositionAction<float> _onCommitted;
    private IBoundary _callbackBoundary;
    private float _value;
    private bool _enabled = true;
    private int _pointerId = -1;

    public SliderInputElement() {
      focusable = true;
      pickingMode = PickingMode.Position;
      this.MakeRelative();

      _background = new CompositionBoundaryNode {
        name = "SliderVisual",
        composable = ComposeBackground,
        pickingMode = PickingMode.Ignore
      }.Stretched();
      _track = CreatePart("Track", ComposeTrack);
      _progress = CreatePart("Progress", ComposeProgress);
      _thumb = CreatePart("Thumb", ComposeThumb);
      hierarchy.Add(_background);
      hierarchy.Add(_track);
      hierarchy.Add(_progress);
      hierarchy.Add(_thumb);

      RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
      RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
      RegisterCallback<PointerDownEvent>(OnPointerDown);
      RegisterCallback<PointerMoveEvent>(OnPointerMove);
      RegisterCallback<PointerUpEvent>(OnPointerUp);
      RegisterCallback<PointerCancelEvent>(OnPointerCancel);
      RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      RegisterCallback<FocusInEvent>(OnFocusIn);
      RegisterCallback<FocusOutEvent>(OnFocusOut);
      RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    public VisualElement Element => this;
    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }

    public void Update(
      float value,
      in SliderOptions options,
      bool enabled,
      bool error,
      SliderStyle style,
      IBoundary callbackBoundary,
      CompositionAction<float> onChanged,
      CompositionAction<float> onCommitted
    ) {
      _callbackBoundary = callbackBoundary;
      _onChanged = onChanged;
      _onCommitted = onCommitted;

      var visualChanged = false;
      var normalizedOptions = NormalizeOptions(options);
      if (!_options.Equals(normalizedOptions)) {
        _options = normalizedOptions;
        visualChanged = true;
      }

      var normalizedValue = ClampAndSnap(value);
      if (!Mathf.Approximately(_value, normalizedValue)) {
        _value = normalizedValue;
        visualChanged = true;
      }

      visualChanged |= SetState(StateFlag.Disabled, !enabled);
      visualChanged |= SetState(StateFlag.Error, error);
      if (_enabled != enabled) {
        _enabled = enabled;
        focusable = enabled;
      }

      if (!ReferenceEquals(_sliderStyle, style)) {
        if (Flag != UssFlag.None) {
          Flag.ClearFlags(this);
          Flag = UssFlag.None;
        }
        _sliderStyle = style;
        visualChanged = true;
      }

      if (visualChanged) ApplyVisuals();
    }

    public void Reset() {
      _options = SliderOptions.Default;
      _sliderStyle = null;
      _inputState = StateFlag.None;
      _onChanged = null;
      _onCommitted = null;
      _callbackBoundary = null;
      _value = 0f;
      _enabled = true;
      _pointerId = -1;
      focusable = true;
      ApplyVisuals();
    }

    private static CompositionBoundaryNode CreatePart(string name, Composable composable) {
      var part = new CompositionBoundaryNode {
        name = name,
        composable = composable,
        pickingMode = PickingMode.Ignore
      };
      return part.MakeAbsolute();
    }

    private SliderOptions NormalizeOptions(in SliderOptions options) {
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

    private float ClampAndSnap(float value) {
      var result = Mathf.Clamp(value, _options.min, _options.max);
      if (_options.step <= 0f || result <= _options.min || result >= _options.max) return result;

      var snapped = _options.min +
                    Mathf.Round((result - _options.min) / _options.step) * _options.step;
      snapped = Mathf.Clamp(snapped, _options.min, _options.max);

      // The maximum remains a valid endpoint even when the range is not an
      // exact multiple of step.
      return _options.max - result <= Mathf.Abs(snapped - result)
        ? _options.max
        : snapped;
    }

    private float NormalizeValue() {
      var range = _options.max - _options.min;
      if (Mathf.Approximately(range, 0f)) return 0f;
      var normalized = Mathf.Clamp01((_value - _options.min) / range);
      return _options.reverse ? 1f - normalized : normalized;
    }

    private void SetFromLocalPosition(Vector2 localPosition, bool commit) {
      var length = _options.axis == Axis.Horizontal ? contentRect.width : contentRect.height;
      var thumbMainSize = ResolveThumbMainSize(length);
      var available = Mathf.Max(0f, length - thumbMainSize);
      if (available <= 0f) return;
      var position = _options.axis == Axis.Horizontal ? localPosition.x : localPosition.y;
      var start = _options.axis == Axis.Horizontal ? contentRect.xMin : contentRect.yMin;
      var normalized = Mathf.Clamp01((position - start - thumbMainSize * 0.5f) / available);
      if (_options.reverse) normalized = 1f - normalized;
      var next = ClampAndSnap(Mathf.Lerp(_options.min, _options.max, normalized));
      if (!Mathf.Approximately(_value, next)) {
        _value = next;
        ApplyVisuals();
        _onChanged?.Call(_callbackBoundary, next);
      }
      if (commit) _onCommitted?.Call(_callbackBoundary, _value);
    }

    private float ResolveThumbMainSize(float length) {
      var minimum = (_sliderStyle ?? SliderStyle.Default).thumbSize;
      if (_options.thumbRange <= 0f) return Mathf.Min(length, minimum);

      var valueRange = Mathf.Max(0f, _options.max - _options.min);
      var totalRange = valueRange + _options.thumbRange;
      if (totalRange <= 0f) return length;
      return Mathf.Clamp(length * (_options.thumbRange / totalRange), minimum, length);
    }

    private void ApplyVisuals() {
      var style = _sliderStyle ?? SliderStyle.Default;
      style.box.ApplyLayout(_inputState, this);
      _background.MarkDirty();
      var normalized = NormalizeValue();
      var length = _options.axis == Axis.Horizontal ? contentRect.width : contentRect.height;
      var thumbMainSize = ResolveThumbMainSize(length);
      var thumbCrossSize = style.thumbSize;
      var halfMain = thumbMainSize * 0.5f;
      var halfCross = thumbCrossSize * 0.5f;

      _track.MarkDirty();
      _progress.MarkDirty();
      _thumb.MarkDirty();

      if (_options.axis == Axis.Horizontal) {
        var start = contentRect.xMin;
        var center = contentRect.yMin + contentRect.height * 0.5f;
        var available = Mathf.Max(0f, contentRect.width - thumbMainSize);
        var thumbOffset = normalized * available;
        _track.style.left = start;
        _track.style.width = contentRect.width;
        _track.style.top = center - style.trackSize * 0.5f;
        _track.style.height = style.trackSize;
        _progress.style.left = start;
        _progress.style.top = center - style.trackSize * 0.5f;
        _progress.style.width = thumbOffset + halfMain;
        _progress.style.height = style.trackSize;
        _thumb.style.left = start + thumbOffset;
        _thumb.style.top = center - halfCross;
        _thumb.Sized(thumbMainSize, thumbCrossSize);
      } else {
        var start = contentRect.yMin;
        var center = contentRect.xMin + contentRect.width * 0.5f;
        var available = Mathf.Max(0f, contentRect.height - thumbMainSize);
        var thumbOffset = normalized * available;
        _track.style.top = start;
        _track.style.height = contentRect.height;
        _track.style.left = center - style.trackSize * 0.5f;
        _track.style.width = style.trackSize;
        _progress.style.top = start;
        _progress.style.left = center - style.trackSize * 0.5f;
        _progress.style.height = thumbOffset + halfMain;
        _progress.style.width = style.trackSize;
        _thumb.style.top = start + thumbOffset;
        _thumb.style.left = center - halfCross;
        _thumb.Sized(thumbCrossSize, thumbMainSize);
      }
    }

    private void ComposeBackground(ref Composition cx) {
      (_sliderStyle ?? SliderStyle.Default).box.RenderBackground(ref cx, _inputState);
    }

    private void ComposeTrack(ref Composition cx) {
      (_sliderStyle ?? SliderStyle.Default).track?.Invoke(ref cx, _inputState);
    }

    private void ComposeProgress(ref Composition cx) {
      (_sliderStyle ?? SliderStyle.Default).progress?.Invoke(ref cx, _inputState);
    }

    private void ComposeThumb(ref Composition cx) {
      (_sliderStyle ?? SliderStyle.Default).thumb?.Invoke(ref cx, _inputState);
    }

    private bool SetState(StateFlag state, bool value) {
      var previous = _inputState;
      if (value) _inputState |= state;
      else _inputState &= ~state;
      return previous != _inputState;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) => ApplyVisuals();

    private void OnPointerEnter(PointerEnterEvent evt) {
      if (SetState(StateFlag.Hovered, true)) ApplyVisuals();
    }

    private void OnPointerLeave(PointerLeaveEvent evt) {
      if (SetState(StateFlag.Hovered, false)) ApplyVisuals();
    }

    private void OnPointerDown(PointerDownEvent evt) {
      if (!_enabled || _pointerId != -1 || evt.button != (int)MouseButton.LeftMouse) return;
      _pointerId = evt.pointerId;
      this.CapturePointer(evt.pointerId);
      Focus();
      SetState(StateFlag.Pressed | StateFlag.Dragged, true);
      SetFromLocalPosition(evt.localPosition, false);
      evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt) {
      if (evt.pointerId != _pointerId) return;
      SetFromLocalPosition(evt.localPosition, false);
      evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt) {
      if (evt.pointerId != _pointerId) return;
      SetFromLocalPosition(evt.localPosition, true);
      this.ReleasePointer(evt.pointerId);
      _pointerId = -1;
      SetState(StateFlag.Pressed | StateFlag.Dragged, false);
      ApplyVisuals();
      evt.StopPropagation();
    }

    private void OnPointerCancel(PointerCancelEvent evt) {
      if (evt.pointerId != _pointerId) return;
      this.ReleasePointer(evt.pointerId);
      _pointerId = -1;
      SetState(StateFlag.Pressed | StateFlag.Dragged, false);
      ApplyVisuals();
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt) {
      if (evt.pointerId != _pointerId) return;
      _pointerId = -1;
      SetState(StateFlag.Pressed | StateFlag.Dragged, false);
      ApplyVisuals();
    }

    private void OnFocusIn(FocusInEvent evt) {
      if (SetState(StateFlag.Focused, true)) ApplyVisuals();
    }

    private void OnFocusOut(FocusOutEvent evt) {
      if (SetState(StateFlag.Focused, false)) ApplyVisuals();
    }

    private void OnKeyDown(KeyDownEvent evt) {
      if (!_enabled) return;
      var direction = evt.keyCode switch {
        KeyCode.LeftArrow => _options.axis == Axis.Horizontal ? -1 : 0,
        KeyCode.RightArrow => _options.axis == Axis.Horizontal ? 1 : 0,
        KeyCode.DownArrow => _options.axis == Axis.Vertical ? 1 : 0,
        KeyCode.UpArrow => _options.axis == Axis.Vertical ? -1 : 0,
        _ => 0
      };
      if (direction == 0) return;
      if (_options.reverse) direction = -direction;
      var amount = _options.step > 0f
        ? _options.step
        : Mathf.Max((_options.max - _options.min) * 0.01f, Mathf.Epsilon);
      var next = ClampAndSnap(_value + direction * amount);
      if (!Mathf.Approximately(_value, next)) {
        _value = next;
        ApplyVisuals();
        _onChanged?.Call(_callbackBoundary, next);
        _onCommitted?.Call(_callbackBoundary, next);
      }
      evt.StopPropagation();
    }
  }

  public sealed class CheckboxStyle {
    public static readonly CheckboxStyle Default = BuildDefault(HXThemes.DefaultDark);
    public static readonly ContextKey<CheckboxStyle> Context = new("CheckboxStyle", Default);

    public readonly ControlBoxStyle box;
    public readonly StateComposable indicator;
    public readonly StateComposable fill;
    public readonly float indicatorSize;
    public readonly StyleLength4 indicatorPadding;
    public readonly StyleLength4 indicatorMargin;
    public readonly float gap;

    public CheckboxStyle(
      ControlBoxStyle box,
      StateComposable indicator,
      float indicatorSize = 18f,
      float gap = 8f,
      StateComposable fill = null,
      StyleLength4? indicatorPadding = null,
      StyleLength4? indicatorMargin = null
    ) {
      this.box = box;
      this.indicator = indicator;
      this.fill = fill;
      this.indicatorSize = indicatorSize;
      this.indicatorPadding = indicatorPadding ?? StyleLength4.Zero;
      this.indicatorMargin = indicatorMargin ?? StyleLength4.Zero;
      this.gap = gap;
    }

    public static CheckboxStyle BuildDefault(ThemeData theme) {
      var indicatorBorder = new StatePropertyMap<Border> {
        [StateFlag.Disabled] = Border.All(
          1f,
          theme.GetColor(ColorRoles.OnSurfaceDisabledHigh)
        ),
        [StateFlag.Focused] = Border.All(2f, theme.GetColor(ColorRoles.Focus)),
        [StateFlag.Hovered] = Border.All(1f, theme.GetColor(ColorRoles.OnSurface)),
        [StateFlag.None] = Border.All(
          1f,
          theme.GetColor(ColorRoles.OnSurface).WithOpacity(0.75f)
        )
      };
      var fillColor = new StatePropertyMap<Color> {
        [StateFlag.Disabled | StateFlag.Selected] =
          theme.GetColor(ColorRoles.OnSurfaceDisabledHigh),
        [StateFlag.Selected] = theme.GetColor(ColorRoles.Primary),
        [StateFlag.None] = Colors.Transparent
      };
      return new CheckboxStyle(
        new ControlBoxStyle(
          alignment: Alignment.CenterLeft,
          textStyle: new StatePropertyMap<TextStyle> {
            [StateFlag.Disabled] = new TextStyle(
              color: theme.GetColor(ColorRoles.OnSurfaceDisabledHigh)
            ),
            [StateFlag.None] = new TextStyle(
              color: theme.GetColor(ColorRoles.OnSurface)
            )
          }
        ),
        new DrawSolidBoxStyle(
          color: Colors.Transparent,
          border: indicatorBorder,
          radius: BorderRadius.All(3f)
        ).Bake(),
        fill: new DrawSolidBoxStyle(
          color: fillColor,
          radius: BorderRadius.All(1f)
        ).Bake(),
        indicatorPadding: StyleLength4.All(3f)
      );
    }
  }

  public static partial class CheckboxFillDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef CheckboxFill(
      ref this Composition cx,
      [Prop] StateFlag state,
      [Prop] StateComposable visual
    );

    public partial class CheckboxFillComposable {
      protected override void OnRecompose(ref Composition cx) {
        cx.APPLY
          .Flexible()
          .AlignSelf(Align.Stretch)
          .Focusable(false, pickingMode: PickingMode.Ignore);
        Props.Visual?.Invoke(ref cx, Props.State);
      }
    }
  }

  public static partial class CheckboxIndicatorDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef CheckboxIndicator(
      ref this Composition cx,
      [Prop] StateFlag state,
      [Prop] StateComposable visual,
      [Prop] StateComposable fill,
      [Prop] float size = 18f,
      [Prop] StyleLength4 padding = default,
      [Prop] StyleLength4 margin = default
    );

    public partial class CheckboxIndicatorComposable {
      protected override void OnRecompose(ref Composition cx) {
        cx.APPLY
          .Size(BoxConstraints.Tight(Props.Size, Props.Size))
          .Padding(Props.Padding)
          .Margin(Props.Margin)
          .Focusable(false, pickingMode: PickingMode.Ignore);
        Props.Visual?.Invoke(ref cx, Props.State);
        cx.CheckboxFill(Props.State, Props.Fill);
      }
    }
  }

  public static partial class CheckboxLabelDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef CheckboxLabel(
      ref this Composition cx,
      [Prop] string text,
      [Prop] float leadingMargin = 0f
    );

    public partial class CheckboxLabelComposable {
      protected override void OnRecompose(ref Composition cx) {
        cx.APPLY
          .Margin(StyleLength4.Only(left: Props.LeadingMargin))
          .Focusable(false, pickingMode: PickingMode.Ignore);

        cx.Text(Props.Text ?? string.Empty);
      }
    }
  }

  public static partial class HXBuiltins {
    private static readonly ushort _sliderId = CompositionId.GetTypeId("Slider");

    public static ref ElementRef Slider(
      this ref Composition cx,
      float value,
      CompositionAction<float> onChanged = null,
      CompositionAction<float> onCommitted = null,
      SliderOptions? options = null,
      bool enabled = true,
      bool error = false,
      SliderStyle style = null
    ) {
      if (!cx.AUTHORING.RequireComposable<SliderInputElement>(_sliderId, out var slider, out _)) {
        slider = new SliderInputElement();
      }

      var resolvedOptions = options ?? SliderOptions.Default;
      slider.Update(
        value,
        in resolvedOptions,
        enabled,
        error,
        style ?? cx.ReadContextOrDefault(SliderStyle.Context, SliderStyle.Default),
        cx.boundary,
        onChanged,
        onCommitted
      );
      return ref cx.AUTHORING.YieldElement(ref cx, slider);
    }
  }

  public static partial class ToggleDefinition {
    [CompositionBoundary(Base = typeof(InputClickableComposable<>))]
    public static partial ref ElementRef Toggle(
      ref this Composition cx,
      [Prop] bool value,
      [Prop] Composable content,
      [Prop] CompositionAction<bool> onChanged = null,
      [Prop] bool enabled = true,
      [Prop] ControlBoxStyle? style = null
    );

    public partial class ToggleComposable {
      protected override void OnRecompose(ref Composition cx) {
        this.Toggle(StateFlag.Selected, Props.Value);
        this.Toggle(StateFlag.Disabled, !Props.Enabled);
        Node.SetEnabled(Props.Enabled);
        cx.APPLY.Focusable(Props.Enabled);
        (Props.Style ?? ControlBoxStyle.Default).RenderBoundary(ref cx, InputState);
        Props.Content?.Invoke(ref cx);
      }

      protected override void OnClick(EventBase evt) {
        if (Props.Enabled) Props.OnChanged.Call(Node, !Props.Value);
      }
    }
  }

  public static partial class CheckboxDefinition {
    [CompositionBoundary(Base = typeof(InputClickableComposable<>))]
    public static partial ref ElementRef Checkbox(
      ref this Composition cx,
      [Prop] bool value,
      [Prop] PrefixLabelSuffixSpec? presentation = null,
      [Prop] CompositionAction<bool> onChanged = null,
      [Prop] bool enabled = true,
      [Prop] CheckboxStyle style = null,
      [Prop] bool error = false
    );

    public partial class CheckboxComposable {
      protected override void OnRecompose(ref Composition cx) {
        this.Toggle(StateFlag.Selected, Props.Value);
        this.Toggle(StateFlag.Disabled, !Props.Enabled);
        this.Toggle(StateFlag.Error, Props.Error);
        Node.SetEnabled(Props.Enabled);
        cx.APPLY.Focusable(Props.Enabled);

        var style = Props.Style ?? cx.ReadContextOrDefault(CheckboxStyle.Context, CheckboxStyle.Default);
        style.box.RenderBoundary(ref cx, InputState);
        cx.APPLY.AlignSelf(Align.FlexStart);
        using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
          cx.CheckboxIndicator(
            InputState,
            style.indicator,
            style.fill,
            style.indicatorSize,
            style.indicatorPadding,
            style.indicatorMargin
          );
          if (Props.Presentation.HasValue) {
            cx.Gap(style.gap);
            var presentation = Props.Presentation.Value;
            cx.PrefixLabelSuffix(in presentation);
          }
        }
      }

      protected override void OnClick(EventBase evt) {
        if (Props.Enabled) Props.OnChanged.Call(Node, !Props.Value);
      }
    }
  }
}