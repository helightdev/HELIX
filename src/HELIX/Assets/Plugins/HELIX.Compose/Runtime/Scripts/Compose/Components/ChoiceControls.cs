using System;
using System.Collections.Generic;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct SegmentedChoiceStyle {
    public readonly HXControlBoxStyle left;
    public readonly HXControlBoxStyle middle;
    public readonly HXControlBoxStyle right;
    public readonly Axis axis;
    public readonly float gap;

    public SegmentedChoiceStyle(
      HXControlBoxStyle left, HXControlBoxStyle middle, HXControlBoxStyle right,
      Axis axis = Axis.Horizontal, float gap = 0f
    ) {
      this.left = left;
      this.middle = middle;
      this.right = right;
      this.axis = axis;
      this.gap = gap;
    }
  }

  public readonly struct SpinboxChoiceStyle {
    public readonly HXControlBoxStyle value;
    public readonly HXControlBoxStyle left;
    public readonly HXControlBoxStyle right;
    public readonly HXControlBoxStyle up;
    public readonly HXControlBoxStyle down;
    public readonly bool wrap;
    public readonly bool vertical;
    public readonly float gap;

    public SpinboxChoiceStyle(
      HXControlBoxStyle value, HXControlBoxStyle left, HXControlBoxStyle right,
      HXControlBoxStyle up, HXControlBoxStyle down,
      bool wrap = true, bool vertical = false, float gap = 0f
    ) {
      this.value = value;
      this.left = left;
      this.right = right;
      this.up = up;
      this.down = down;
      this.wrap = wrap;
      this.vertical = vertical;
      this.gap = gap;
    }
  }

  internal interface IChoiceItemOwner {
    State State { get; }
    bool IsSelected(int index);
    void Select(int index);
  }

  [EnableMixins]
  [BoundaryComposableMixin(super: typeof(InputClickableComposable<>))]
  internal partial class SegmentedChoiceItem {
    public partial struct Props {
      public int index;
      public string label;
      public bool enabled;
      [Prop(null, Equatable = false)] public IChoiceItemOwner owner;
      [Prop(null)] public HXControlBoxStyle? style;
    }

    protected override void OnRecompose(ref Composition cx) {
      var selected = props.owner?.IsSelected(props.index) == true;
      this.Toggle(State.Selected, selected);
      this.Toggle(State.Disabled, !props.enabled);
      cx.CURSOR.Focusable(props.enabled);
      var style = props.style ?? ThemeProperties.ButtonToggle[in cx];
      style.RenderBoundary(
        ref cx, InputState | (props.owner?.State ?? State.None) |
                (selected ? State.Selected : State.None)
      );
      cx.Text(props.label ?? string.Empty);
    }

    protected override void OnClick(EventBase evt) {
      if (props.enabled) props.owner?.Select(props.index);
    }
  }

  public sealed class SegmentedChoice<T> : PropsBoundaryComposable<SegmentedChoice<T>.Props>, IChoiceItemOwner {
    public readonly struct Props {
      public readonly T value;
      public readonly IDatatypeChoice<T> datatype;
      public readonly CompositionAction<T> onChanged;
      public readonly bool enabled, error;
      public readonly SegmentedChoiceStyle? style;

      public Props(
        T value, IDatatypeChoice<T> datatype, CompositionAction<T> onChanged,
        bool enabled, bool error, SegmentedChoiceStyle? style
      ) {
        this.value = value;
        this.datatype = datatype;
        this.onChanged = onChanged;
        this.enabled = enabled;
        this.error = error;
        this.style = style;
      }
    }

    private static readonly EqualityComparer<T> _equality = EqualityComparer<T>.Default;
    private IDatatypeChoice<T> _cachedDatatype;
    private T[] _values;
    private string[] _labels;
    private bool[] _enabled;

    protected override void OnRecompose(ref Composition cx) {
      EnsureChoices();
      var style = props.style ?? ThemeProperties.SegmentedChoice[in cx];
      using (cx.Group(style.axis, cross: Align.Stretch)) {
        for (var i = 0; i < _values.Length; i++) {
          if (i > 0 && style.gap > 0f) cx.Gap(style.gap);
          SegmentedChoiceItem.ComposeBoundary(
            ref cx, i, _labels[i], props.enabled && _enabled[i], this,
            i == 0 ? style.left : i == _values.Length - 1 ? style.right : style.middle
          ).Flexible();
        }
      }
    }

    State IChoiceItemOwner.State =>
      (props.error ? State.Error : State.None) | (props.enabled ? State.None : State.Disabled);

    bool IChoiceItemOwner.IsSelected(int index) =>
      _equality.Equals(props.value, _values[index]);

    void IChoiceItemOwner.Select(int index) {
      var value = _values[index];
      if (!_equality.Equals(props.value, value)) props.onChanged?.Call(Node, value);
    }

    private void EnsureChoices() {
      var count = props.datatype.ChoiceCount;
      if (ReferenceEquals(_cachedDatatype, props.datatype) && _values?.Length == count) return;
      _cachedDatatype = props.datatype;
      _values = new T[count];
      _labels = new string[count];
      _enabled = new bool[count];
      for (var i = 0; i < count; i++) {
        _values[i] = props.datatype.GetTypedChoiceValue(i);
        _labels[i] = props.datatype.GetChoiceLabel(i) ?? string.Empty;
        _enabled[i] = props.datatype.IsChoiceEnabled(i);
      }
    }
  }

  public sealed class ChoiceSpinbox<T> : PropsBoundaryComposable<ChoiceSpinbox<T>.Props> {
    private static readonly Composable _left = (ref Composition cx) =>
      new ChevronSpec(ArrowPosition.Left, ThemeProperties.ChevronSize[in cx]).Compose(ref cx);
    private static readonly Composable _right = (ref Composition cx) =>
      new ChevronSpec(ArrowPosition.Right, ThemeProperties.ChevronSize[in cx]).Compose(ref cx);
    private static readonly Composable _up = (ref Composition cx) =>
      new ChevronSpec(ArrowPosition.Up, ThemeProperties.ChevronSize[in cx]).Compose(ref cx);
    private static readonly Composable _down = (ref Composition cx) =>
      new ChevronSpec(ArrowPosition.Down, ThemeProperties.ChevronSize[in cx]).Compose(ref cx);
    private static readonly Composable _selectedLabel = ComposeSelectedLabel;
    public readonly struct Props {
      public readonly T value;
      public readonly IDatatypeChoice<T> datatype;
      public readonly CompositionAction<T> onChanged;
      public readonly bool enabled, error;
      public readonly SpinboxChoiceStyle? style;

      public Props(
        T value, IDatatypeChoice<T> datatype, CompositionAction<T> onChanged,
        bool enabled, bool error, SpinboxChoiceStyle? style
      ) {
        this.value = value;
        this.datatype = datatype;
        this.onChanged = onChanged;
        this.enabled = enabled;
        this.error = error;
        this.style = style;
      }
    }

    private static readonly EqualityComparer<T> _equality = EqualityComparer<T>.Default;
    private IDatatypeChoice<T> _cachedDatatype;
    private T[] _values;
    private string[] _labels;
    private bool[] _enabled;

    protected override void OnRecompose(ref Composition cx) {
      EnsureChoices();
      var style = props.style ?? ThemeProperties.ChoiceSpinbox[in cx];
      var axis = style.vertical ? Axis.Vertical : Axis.Horizontal;
      using (cx.Group(axis, cross: Align.Stretch)) {
        ComposeStep(ref cx, -1, style.vertical ? ArrowPosition.Up : ArrowPosition.Left, in style);
        if (style.gap > 0f) cx.Gap(style.gap);
        cx.Button(
          _selectedLabel, enabled: props.enabled,
          style: style.value
        );
        cx.CURSOR.Flexible();
        if (style.gap > 0f) cx.Gap(style.gap);
        ComposeStep(ref cx, 1, style.vertical ? ArrowPosition.Down : ArrowPosition.Right, in style);
      }
    }

    private void ComposeStep(
      ref Composition cx, int direction, ArrowPosition arrow, in SpinboxChoiceStyle style
    ) {
      var enabled = props.enabled && FindNext(direction) >= 0;
      cx.Button(
        arrow switch {
          ArrowPosition.Left => _left,
          ArrowPosition.Right => _right,
          ArrowPosition.Up => _up,
          _ => _down
        },
        direction < 0 ? Previous : Next, enabled,
        style: arrow switch {
          ArrowPosition.Left => style.left,
          ArrowPosition.Right => style.right,
          ArrowPosition.Up => style.up,
          _ => style.down
        }
      );
    }

    private string SelectedLabel() {
      var index = SelectedIndex();
      return index < 0 ? string.Empty : _labels[index];
    }

    private static void ComposeSelectedLabel(ref Composition cx) {
      var self = cx.Lookup<ChoiceSpinbox<T>>();
      cx.Text(self?.SelectedLabel() ?? string.Empty);
    }

    private int SelectedIndex() {
      for (var i = 0; i < _values.Length; i++)
        if (_equality.Equals(props.value, _values[i])) return i;
      return -1;
    }

    private int FindNext(int direction) {
      var count = _values.Length;
      if (count == 0) return -1;
      var selected = SelectedIndex();
      if (selected < 0) selected = direction > 0 ? -1 : count;
      for (var offset = 1; offset <= count; offset++) {
        var index = selected + direction * offset;
        var style = props.style ?? ThemeProperties.ChoiceSpinbox[Node];
        if (style.wrap) index = (index % count + count) % count;
        else if (index < 0 || index >= count) return -1;
        if (_enabled[index]) return index;
      }
      return -1;
    }

    private static void Previous(CompositionContext context) => context.Lookup<ChoiceSpinbox<T>>()?.Change(-1);
    private static void Next(CompositionContext context) => context.Lookup<ChoiceSpinbox<T>>()?.Change(1);

    private void Change(int direction) {
      var index = FindNext(direction);
      if (index >= 0) props.onChanged?.Call(Node, _values[index]);
    }

    private void EnsureChoices() {
      var count = props.datatype.ChoiceCount;
      if (ReferenceEquals(_cachedDatatype, props.datatype) && _values?.Length == count) return;
      _cachedDatatype = props.datatype;
      _values = new T[count];
      _labels = new string[count];
      _enabled = new bool[count];
      for (var i = 0; i < count; i++) {
        _values[i] = props.datatype.GetTypedChoiceValue(i);
        _labels[i] = props.datatype.GetChoiceLabel(i) ?? string.Empty;
        _enabled[i] = props.datatype.IsChoiceEnabled(i);
      }
    }
  }

  public static class ChoiceControlExtensions {
    private static class Id<TComponent> {
      public static readonly ushort Value = CompositionId.GetTypeId(typeof(TComponent).Name);
    }

    public static ref ElementRef SegmentedChoice<T>(
      this ref Composition cx, T value, IDatatypeChoice<T> datatype,
      CompositionAction<T> onChanged = null,
      bool enabled = true, bool error = false, SegmentedChoiceStyle? style = null
    ) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      var props = new SegmentedChoice<T>.Props(
        value, datatype, onChanged, enabled, error, style
      );
      return ref DatatypeControlBoundary.Compose<SegmentedChoice<T>, SegmentedChoice<T>.Props>(
        ref cx, Id<SegmentedChoice<T>>.Value, props
      );
    }

    public static ref ElementRef ChoiceSpinbox<T>(
      this ref Composition cx, T value, IDatatypeChoice<T> datatype,
      CompositionAction<T> onChanged = null,
      bool enabled = true, bool error = false, SpinboxChoiceStyle? style = null
    ) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      var props = new ChoiceSpinbox<T>.Props(
        value, datatype, onChanged, enabled, error, style
      );
      return ref DatatypeControlBoundary.Compose<ChoiceSpinbox<T>, ChoiceSpinbox<T>.Props>(
        ref cx, Id<ChoiceSpinbox<T>>.Value, props
      );
    }
  }
}
