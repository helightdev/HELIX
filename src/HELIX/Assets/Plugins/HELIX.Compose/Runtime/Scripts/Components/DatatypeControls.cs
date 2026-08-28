using System;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  internal static class DatatypeControlBoundary {
    public static ref ElementRef Compose<TComponent, TProps>(
      ref Composition cx,
      ushort typeId,
      TProps props
    ) where TComponent : GeneratedBoundaryBase, IProps<TProps>, new() where TProps : struct {
      if (!cx.AUTHORING.RequireComposable<TComponent>(typeId, out var component, out _))
        component = new TComponent();
      component.ReceiveProps(in props);
      return ref cx.AUTHORING.YieldBoundary(ref cx, component);
    }
  }

  [EnableMixins]
  [BoundaryElementMixin]
  public sealed partial class DatatypeSlider<T> {
    [Hook]
    private void OnCompose(ref Composition cx) {
      cx.Slider(
        props.datatype.ToFloat(props.value),
        onChanged: Changed,
        onCommitted: Committed,
        options: props.options,
        enabled: props.enabled,
        error: props.error,
        style: props.style
      );
    }

    private static void Changed(CompositionContext context, float value) {
      var self = context.Lookup<DatatypeSlider<T>>();
      self?.props.onChanged?.Call(self, self.props.datatype.FromFloat(value));
    }

    private static void Committed(CompositionContext context, float value) {
      var self = context.Lookup<DatatypeSlider<T>>();
      self?.props.onCommitted?.Call(self, self.props.datatype.FromFloat(value));
    }

    public partial struct Props {
      public readonly T value;
      public readonly INumericConvertible<T> datatype;
      public readonly CompositionAction<T> onChanged;
      public readonly CompositionAction<T> onCommitted;
      public readonly SliderOptions options;
      public readonly bool enabled, error;
      public readonly SliderStyle? style;
    }
  }

  [EnableMixins]
  [BoundaryElementMixin]
  public sealed partial class DatatypeTextField<T> {
    [Hook]
    private void OnCompose(ref Composition cx) {
      cx.TextField(
        value: new TextEditingValue(props.datatype.ToString(props.value) ?? string.Empty),
        onChanged: Changed,
        onEditingEnded: EditingEnded,
        enabled: props.enabled,
        options: props.options,
        prefix: props.prefix,
        suffix: props.suffix,
        selectionStyle: props.selectionStyle,
        style: props.style
      );
    }

    private static void Changed(CompositionContext context, TextEditingValue value) {
      var self = context.Lookup<DatatypeTextField<T>>();
      if (self == null || !TryConvert(self.props.datatype, value.text, out var converted)) return;
      self.props.onChanged?.Call(self, converted);
    }

    private static void EditingEnded(
      CompositionContext context,
      TextEditingValue value,
      TextEditEndReason reason
    ) {
      var self = context.Lookup<DatatypeTextField<T>>();
      if (self == null || !TryConvert(self.props.datatype, value.text, out var converted)) return;
      self.props.onEditingEnded?.Call(self, converted, reason);
      if (reason != TextEditEndReason.Cancelled) self.props.onCommitted?.Call(self);
    }

    private static bool TryConvert(IStringConvertible<T> datatype, string text, out T value) {
      try {
        value = datatype.FromString(text);
        return true;
      } catch (Exception exception) when (
        exception is FormatException or OverflowException or ArgumentException
      ) {
        value = default;
        return false;
      }
    }

    public partial struct Props {
      public readonly T value;
      public readonly IStringConvertible<T> datatype;
      public readonly CompositionAction<T> onChanged;
      public readonly CompositionAction<T, TextEditEndReason> onEditingEnded;
      public readonly CompositionAction onCommitted;
      public readonly bool enabled;
      public readonly TextInputOptions options;
      public readonly Composable prefix, suffix;
      public readonly TextSelectionStyle? selectionStyle;
      public readonly HXControlBoxStyle? style;
    }
  }

  [EnableMixins]
  [BoundaryElementMixin]
  public sealed partial class DatatypeFieldSlider<T> where T : struct {
    [Hook]
    private void OnCompose(ref Composition cx) {
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        cx.DatatypeSlider(
          props.value,
          props.numeric,
          Changed,
          SliderCommitted,
          new SliderOptions(
            props.numeric.ToFloat(props.min),
            props.numeric.ToFloat(props.max),
            props.numeric.ToFloat(props.step)
          ),
          props.enabled,
          props.error
        ).Flexible();
        cx.Gap(ThemeProperties.TextGap[in cx]);
        cx.DatatypeTextField(
          props.value,
          props.text,
          onEditingEnded: TextCommitted,
          enabled: props.enabled,
          prefix: props.prefix,
          suffix: props.suffix
        );
        cx.CURSOR.Width(ThemeProperties.CompanionFieldWidth[in cx]);
      }
    }

    private static void Changed(CompositionContext context, T value) {
      var self = context.Lookup<DatatypeFieldSlider<T>>();
      if (self == null) return;
      self.props.onChanged?.Call(self, self.Normalize(value));
    }

    private static void SliderCommitted(CompositionContext context, T _) {
      var self = context.Lookup<DatatypeFieldSlider<T>>();
      self?.props.onCommitted?.Call(self);
    }

    private static void TextCommitted(CompositionContext context, T value, TextEditEndReason reason) {
      if (reason == TextEditEndReason.Cancelled) return;
      var self = context.Lookup<DatatypeFieldSlider<T>>();
      if (self == null) return;
      self.props.onChanged?.Call(self, self.Normalize(value));
      self.props.onCommitted?.Call(self);
    }

    private T Normalize(T value) {
      var options = HXSliderElement.NormalizeOptions(
        new SliderOptions(
          props.numeric.ToFloat(props.min),
          props.numeric.ToFloat(props.max),
          props.numeric.ToFloat(props.step)
        )
      );
      return props.numeric.FromFloat(
        HXSliderElement.ClampAndSnap(props.numeric.ToFloat(value), in options)
      );
    }

    public partial struct Props {
      public readonly T value, min, max, step;
      public readonly IDatatype<T> datatype;
      public readonly INumericConvertible<T> numeric;
      public readonly IStringConvertible<T> text;
      public readonly CompositionAction<T> onChanged;
      public readonly CompositionAction onCommitted;
      public readonly bool enabled, error;
      public readonly Composable prefix, suffix;
    }
  }

  [EnableMixins]
  [BoundaryElementMixin]
  public sealed partial class DatatypeDropdown<T> {
    private DropdownController<T> _controller;
    private DropdownOption<T>[] _options;

    private IDatatypeChoice<T> _optionsDatatype;

    [Hook]
    private void OnCompose(ref Composition cx) {
      if (!ReferenceEquals(_optionsDatatype, props.datatype) || _options?.Length != props.datatype.ChoiceCount) {
        _optionsDatatype = props.datatype;
        _options = new DropdownOption<T>[props.datatype.ChoiceCount];
        for (var i = 0; i < _options.Length; i++) {
          _options[i] = new DropdownOption<T>(
            props.datatype.GetTypedChoiceValue(i),
            props.datatype.GetChoiceLabel(i),
            props.datatype.IsChoiceEnabled(i)
          );
        }
      }
      _controller ??= new DropdownController<T>();
      _controller.Synchronize(
        props.value,
        _options,
        Changed,
        props.placeholder,
        props.enabled,
        props.error
      );
      cx.DropdownButton(_controller, props.style);
    }

    [Hook]
    private void OnDispose() {
      _controller?.Dispose();
      _controller = null;
    }

    private static void Changed(CompositionContext context, T value) {
      var self = context.Lookup<DatatypeDropdown<T>>();
      self?.props.onChanged?.Call(self, value);
    }

    public partial struct Props {
      public readonly T value;
      public readonly IDatatypeChoice<T> datatype;
      public readonly CompositionAction<T> onChanged;
      public readonly string placeholder;
      public readonly bool enabled, error;
      public readonly PopupMenuStyle? style;
    }
  }

  public static class DatatypeControlExtensions {
    public static ref ElementRef DatatypeSlider<T>(
      this ref Composition cx,
      T value,
      INumericConvertible<T> datatype,
      CompositionAction<T> onChanged = null,
      CompositionAction<T> onCommitted = null,
      SliderOptions? options = null,
      bool enabled = true,
      bool error = false,
      SliderStyle? style = null
    ) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      var resolvedOptions = options ?? SliderOptions.Default;
      var props = new DatatypeSlider<T>.Props(
        value,
        datatype,
        onChanged,
        onCommitted,
        resolvedOptions,
        enabled,
        error,
        style
      );
      return ref DatatypeControlBoundary.Compose<DatatypeSlider<T>, DatatypeSlider<T>.Props>(
        ref cx,
        Id<DatatypeSlider<T>>.Value,
        props
      );
    }

    public static ref ElementRef DatatypeTextField<T>(
      this ref Composition cx,
      T value,
      IStringConvertible<T> datatype,
      CompositionAction<T> onChanged = null,
      CompositionAction<T, TextEditEndReason> onEditingEnded = null,
      CompositionAction onCommitted = null,
      bool enabled = true,
      TextInputOptions? options = null,
      Composable prefix = null,
      Composable suffix = null,
      TextSelectionStyle? selectionStyle = null,
      HXControlBoxStyle? style = null
    ) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      var resolvedOptions = options ?? TextInputOptions.Default;
      var props = new DatatypeTextField<T>.Props(
        value,
        datatype,
        onChanged,
        onEditingEnded,
        onCommitted,
        enabled,
        resolvedOptions,
        prefix,
        suffix,
        selectionStyle,
        style
      );
      return ref DatatypeControlBoundary.Compose<DatatypeTextField<T>, DatatypeTextField<T>.Props>(
        ref cx,
        Id<DatatypeTextField<T>>.Value,
        props
      );
    }

    public static ref ElementRef DatatypeFieldSlider<T>(
      this ref Composition cx,
      T value,
      IDatatype<T> datatype,
      CompositionAction<T> onChanged = null,
      CompositionAction onCommitted = null,
      T min = default,
      T max = default,
      T step = default,
      bool enabled = true,
      bool error = false,
      Composable prefix = null,
      Composable suffix = null
    ) where T : struct {
      if (datatype is not INumericConvertible<T> numeric)
        throw new ArgumentException("The datatype must support numeric conversion.", nameof(datatype));
      if (datatype is not IStringConvertible<T> text)
        throw new ArgumentException("The datatype must support string conversion.", nameof(datatype));
      var props = new DatatypeFieldSlider<T>.Props(
        value,
        min,
        max,
        step,
        datatype,
        numeric,
        text,
        onChanged,
        onCommitted,
        enabled,
        error,
        prefix,
        suffix
      );
      return ref DatatypeControlBoundary.Compose<DatatypeFieldSlider<T>, DatatypeFieldSlider<T>.Props>(
        ref cx,
        Id<DatatypeFieldSlider<T>>.Value,
        props
      );
    }

    public static ref ElementRef DatatypeDropdown<T>(
      this ref Composition cx,
      T value,
      IDatatypeChoice<T> datatype,
      CompositionAction<T> onChanged = null,
      string placeholder = null,
      bool enabled = true,
      bool error = false,
      PopupMenuStyle? style = null
    ) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      var props = new DatatypeDropdown<T>.Props(
        value,
        datatype,
        onChanged,
        placeholder,
        enabled,
        error,
        style
      );
      return ref DatatypeControlBoundary.Compose<DatatypeDropdown<T>, DatatypeDropdown<T>.Props>(
        ref cx,
        Id<DatatypeDropdown<T>>.Value,
        props
      );
    }

    private static class Id<TComponent> {
      public static readonly ushort Value = CompositionId.GetTypeId(typeof(TComponent).Name);
    }
  }
}