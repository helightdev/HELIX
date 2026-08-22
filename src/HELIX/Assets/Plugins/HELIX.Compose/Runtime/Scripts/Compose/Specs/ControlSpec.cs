using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace HELIX.Compose {
  public readonly struct ControlSpec<T> : ISpec {
    public readonly T value;
    public readonly CompositionAction<T> onChanged;
    public readonly CompositionAction onCommitted;
    public readonly bool enabled, error;

    public ControlSpec(T value, CompositionAction<T> onChanged = null, CompositionAction onCommitted = null,
      bool enabled = true, bool error = false) {
      this.value = value;
      this.onChanged = onChanged;
      this.onCommitted = onCommitted;
      this.enabled = enabled;
      this.error = error;
    }
  }

  public readonly struct StringControlSpec : ISpec {
    public readonly ControlSpec<string> control;
    public readonly string placeholder;
    public readonly bool multiline, readOnly;
    public StringControlSpec(ControlSpec<string> control, string placeholder = null, bool multiline = false,
      bool readOnly = false) {
      this.control = control; this.placeholder = placeholder; this.multiline = multiline; this.readOnly = readOnly;
    }
  }

  public readonly struct IntControlSpec : ISpec {
    public readonly ControlSpec<int> control;
    public readonly HXOptional<int> min, max, step;
    public IntControlSpec(ControlSpec<int> control, HXOptional<int> min = default, HXOptional<int> max = default,
      HXOptional<int> step = default) {
      this.control = control; this.min = min; this.max = max; this.step = step;
    }
  }

  public readonly struct FloatControlSpec : ISpec {
    public readonly ControlSpec<float> control;
    public readonly HXOptional<float> min, max, step;
    public readonly NumericControlPresentation presentation;
    public readonly NumericFormatSettings formatting;
    public readonly Composable prefix, suffix;
    public FloatControlSpec(ControlSpec<float> control, HXOptional<float> min = default,
      HXOptional<float> max = default, HXOptional<float> step = default,
      NumericControlPresentation presentation = NumericControlPresentation.Default,
      NumericFormatSettings formatting = default, Composable prefix = null, Composable suffix = null) {
      this.control = control; this.min = min; this.max = max; this.step = step; this.presentation = presentation;
      this.formatting = formatting; this.prefix = prefix; this.suffix = suffix;
    }
  }

  public enum NumericControlPresentation : byte { Default, Text }

  public readonly struct NumericFormatSettings {
    public readonly string format;
    public readonly IFormatProvider provider;
    public readonly float scale;
    public NumericFormatSettings(string format = null, IFormatProvider provider = null, float scale = 1f) {
      this.format = format; this.provider = provider; this.scale = scale == 0f ? 1f : scale;
    }
    public string Format(float value) =>
      (value * (scale == 0f ? 1f : scale)).ToString(format, provider ?? CultureInfo.InvariantCulture);
    public bool TryParse(string text, out float value) {
      if (float.TryParse(text, NumberStyles.Float, provider ?? CultureInfo.InvariantCulture, out value)) {
        value /= scale == 0f ? 1f : scale;
        return true;
      }
      return false;
    }
  }

  public readonly struct BoolControlSpec : ISpec {
    public readonly ControlSpec<bool> control;
    public BoolControlSpec(ControlSpec<bool> control) => this.control = control;
  }

  public readonly struct EnumControlSpec : ISpec {
    public readonly ControlSpec<object> control;
    public readonly IReadOnlyList<DropdownOption<object>> values;

    public EnumControlSpec(ControlSpec<object> control, IReadOnlyList<DropdownOption<object>> values) {
      this.control = control;
      this.values = values;
    }
  }

  [BoundaryComposable(Extension = false, UseLookupCache = true)]
  internal partial class ControlSpecBoundary {
    public partial struct Props {
      public Composable content;
      [Prop(null, Equatable = false)] public object metadata;
    }

    public T Metadata<T>() => props.metadata is T value ? value : default;
    protected override void OnRecompose(ref Composition cx) => props.content?.Invoke(ref cx);
  }

  public static class ControlSpecFactories {
    public static void Text(ref Composition cx, in ControlSpec<string> spec) =>
      Compose(ref cx, spec, ComposeText);
    public static void Integer(ref Composition cx, in ControlSpec<int> spec) =>
      Compose(ref cx, spec, ComposeInteger);
    public static void Float(ref Composition cx, in ControlSpec<float> spec) =>
      Compose(ref cx, spec, ComposeFloat);
    public static void Checkbox(ref Composition cx, in ControlSpec<bool> spec) =>
      Compose(ref cx, spec, ComposeCheckbox);
    public static void Text(ref Composition cx, in StringControlSpec spec) =>
      Compose(ref cx, spec, ComposeSpecializedText);
    public static void Integer(ref Composition cx, in IntControlSpec spec) =>
      Compose(ref cx, spec, ComposeSpecializedInteger);
    public static void Float(ref Composition cx, in FloatControlSpec spec) =>
      Compose(ref cx, spec, ComposeSpecializedFloat);
    public static void Checkbox(ref Composition cx, in BoolControlSpec spec) =>
      Compose(ref cx, spec, ComposeSpecializedCheckbox);
    public static void Enum(ref Composition cx, in EnumControlSpec spec) =>
      Compose(ref cx, spec, ComposeEnum);

    private static void Compose<T>(ref Composition cx, T metadata, Composable content) where T : struct, ISpec =>
      ControlSpecBoundary.ComposeBoundary(ref cx, content, metadata);

    private static void ComposeText(ref Composition cx) =>
      RenderText(ref cx, cx.Lookup<ControlSpecBoundary>().Metadata<ControlSpec<string>>(), false, false);
    private static void ComposeSpecializedText(ref Composition cx) {
      var spec = cx.Lookup<ControlSpecBoundary>().Metadata<StringControlSpec>();
      RenderText(ref cx, spec.control, spec.multiline, spec.readOnly);
    }
    private static void RenderText(
      ref Composition cx, ControlSpec<string> spec, bool multiline, bool readOnly
    ) => cx.TextField(
      value: new TextEditingValue(spec.value ?? ""), onChanged: TextChanged, onEditingEnded: EditingEnded,
      enabled: spec.enabled && !readOnly,
      options: new TextInputOptions(multiline: multiline, readOnly: readOnly)
    );

    private static void ComposeInteger(ref Composition cx) =>
      RenderInteger(ref cx, cx.Lookup<ControlSpecBoundary>().Metadata<ControlSpec<int>>());
    private static void ComposeSpecializedInteger(ref Composition cx) =>
      RenderInteger(ref cx, cx.Lookup<ControlSpecBoundary>().Metadata<IntControlSpec>().control);
    private static void RenderInteger(ref Composition cx, ControlSpec<int> spec) => cx.TextField(
      value: new TextEditingValue(spec.value.ToString(CultureInfo.InvariantCulture)),
      onChanged: IntegerChanged, onEditingEnded: EditingEnded, enabled: spec.enabled
    );

    private static void ComposeFloat(ref Composition cx) =>
      RenderFloat(ref cx, cx.Lookup<ControlSpecBoundary>().Metadata<ControlSpec<float>>());
    private static void ComposeSpecializedFloat(ref Composition cx) {
      var spec = cx.Lookup<ControlSpecBoundary>().Metadata<FloatControlSpec>();
      if (spec.presentation == NumericControlPresentation.Text) {
        RenderFloat(ref cx, spec.control);
        return;
      }
      cx.FieldSlider(
        spec.control.value,
        spec.control.onChanged,
        spec.control.onCommitted,
        spec.min.hasValue ? spec.min.value : 0f,
        spec.max.hasValue ? spec.max.value : 1f,
        spec.step.hasValue ? spec.step.value : 0f,
        spec.control.enabled,
        spec.control.error,
        spec.formatting,
        spec.prefix,
        spec.suffix
      );
    }
    private static void RenderFloat(ref Composition cx, ControlSpec<float> spec) {
      var detailed = cx.Lookup<ControlSpecBoundary>().Metadata<FloatControlSpec>();
      cx.TextField(
        value: new TextEditingValue(detailed.formatting.Format(spec.value)),
        onEditingEnded: FloatEditingEnded,
        enabled: spec.enabled,
        prefix: detailed.prefix,
        suffix: detailed.suffix
      );
    }

    private static void ComposeCheckbox(ref Composition cx) =>
      RenderCheckbox(ref cx, cx.Lookup<ControlSpecBoundary>().Metadata<ControlSpec<bool>>());
    private static void ComposeSpecializedCheckbox(ref Composition cx) =>
      RenderCheckbox(ref cx, cx.Lookup<ControlSpecBoundary>().Metadata<BoolControlSpec>().control);
    private static void RenderCheckbox(ref Composition cx, ControlSpec<bool> spec) =>
      cx.Checkbox(spec.value, onChanged: BoolChanged, enabled: spec.enabled, error: spec.error);

    private static void ComposeEnum(ref Composition cx) {
      var spec = cx.Lookup<ControlSpecBoundary>().Metadata<EnumControlSpec>();
      cx.DropdownButton(spec.control.value, spec.values, onChanged: EnumChanged,
        enabled: spec.control.enabled, error: spec.control.error);
    }

    private static void TextChanged(CompositionContext context, TextEditingValue value) =>
      GetStringControl(context.Lookup<ControlSpecBoundary>()).onChanged?.Call(context.boundary, value.text);
    private static void IntegerChanged(CompositionContext context, TextEditingValue value) {
      var boundary = context.Lookup<ControlSpecBoundary>();
      if (int.TryParse(value.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        GetIntControl(boundary).onChanged?.Call(context.boundary, parsed);
    }
    private static void FloatEditingEnded(
      CompositionContext context,
      TextEditingValue value,
      TextEditEndReason reason
    ) {
      if (reason != TextEditEndReason.Submitted) return;
      var boundary = context.Lookup<ControlSpecBoundary>();
      var spec = boundary.Metadata<FloatControlSpec>();
      if (spec.formatting.TryParse(value.text, out var parsed)) {
        if (spec.min.hasValue && spec.max.hasValue) {
          var options = HXSliderElement.NormalizeOptions(new SliderOptions(
            spec.min.value,
            spec.max.value,
            spec.step.hasValue ? spec.step.value : 0f
          ));
          parsed = HXSliderElement.ClampAndSnap(parsed, in options);
        } else {
          if (spec.step.hasValue && spec.step.value > 0f) {
            var origin = spec.min.hasValue ? spec.min.value : 0f;
            parsed = origin + Mathf.Round((parsed - origin) / spec.step.value) * spec.step.value;
          }
          if (spec.min.hasValue) parsed = Mathf.Max(parsed, spec.min.value);
          if (spec.max.hasValue) parsed = Mathf.Min(parsed, spec.max.value);
        }
        GetFloatControl(boundary).onChanged?.Call(context.boundary, parsed);
      }
      GetFloatControl(boundary).onCommitted?.Call(context.boundary);
    }
    private static void BoolChanged(CompositionContext context, bool value) {
      var boundary = context.Lookup<ControlSpecBoundary>();
      var direct = boundary.Metadata<ControlSpec<bool>>();
      var control = direct.onChanged != null ? direct : boundary.Metadata<BoolControlSpec>().control;
      control.onChanged?.Call(context.boundary, value);
    }
    private static void EnumChanged(CompositionContext context, object value) =>
      context.Lookup<ControlSpecBoundary>()?.Metadata<EnumControlSpec>().control.onChanged?.Call(context.boundary, value);
    private static void EditingEnded(CompositionContext context, TextEditingValue _, TextEditEndReason __) {
      var boundary = context.Lookup<ControlSpecBoundary>();
      boundary.Metadata<ControlSpec<string>>().onCommitted?.Call(context.boundary);
      boundary.Metadata<StringControlSpec>().control.onCommitted?.Call(context.boundary);
      GetIntControl(boundary).onCommitted?.Call(context.boundary);
    }
    private static ControlSpec<int> GetIntControl(ControlSpecBoundary boundary) {
      var direct = boundary.Metadata<ControlSpec<int>>();
      return direct.onChanged != null || direct.onCommitted != null ? direct : boundary.Metadata<IntControlSpec>().control;
    }
    private static ControlSpec<string> GetStringControl(ControlSpecBoundary boundary) {
      var direct = boundary.Metadata<ControlSpec<string>>();
      return direct.onChanged != null || direct.onCommitted != null
        ? direct
        : boundary.Metadata<StringControlSpec>().control;
    }
    private static ControlSpec<float> GetFloatControl(ControlSpecBoundary boundary) {
      var direct = boundary.Metadata<ControlSpec<float>>();
      return direct.onChanged != null || direct.onCommitted != null ? direct : boundary.Metadata<FloatControlSpec>().control;
    }
  }
}
