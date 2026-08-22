using System;
using System.Globalization;
using HELIX.Prose;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  internal interface IControlSpec {
    Type ValueType { get; }
    object Value { get; }
    object Formatter { get; }
    bool Enabled { get; }
    bool Error { get; }
    void Change(CompositionContext context, object value);
    void Commit(CompositionContext context);
  }

  internal interface ITextControlFormatter {
    object Formatter { get; }
    Composable Prefix { get; }
    Composable Suffix { get; }
  }

  internal sealed class TextControlFormatter<T> : IProseFormatter<T>, ITextControlFormatter {
    private readonly IProseFormatter<T> _formatter;
    public TextControlFormatter(IProseFormatter<T> formatter, Composable prefix = null, Composable suffix = null) {
      _formatter = formatter;
      Prefix = prefix;
      Suffix = suffix;
    }
    object ITextControlFormatter.Formatter => _formatter;
    public Composable Prefix { get; }
    public Composable Suffix { get; }
    public void ToProse(IProseWriter writer, T value) => _formatter.ToProse(writer, value);
  }

  public readonly struct ControlSpec<T> : ISpec, IControlSpec {
    public readonly T value;
    public readonly IProseFormatter<T> formatter;
    public readonly CompositionAction<T> onChanged;
    public readonly CompositionAction onCommitted;
    public readonly bool enabled, error;

    public ControlSpec(
      T value,
      IProseFormatter<T> formatter,
      CompositionAction<T> onChanged = null,
      CompositionAction onCommitted = null,
      bool enabled = true,
      bool error = false
    ) {
      this.value = value;
      this.formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
      this.onChanged = onChanged;
      this.onCommitted = onCommitted;
      this.enabled = enabled;
      this.error = error;
    }

    Type IControlSpec.ValueType => typeof(T);
    object IControlSpec.Value => value;
    object IControlSpec.Formatter => formatter;
    bool IControlSpec.Enabled => enabled;
    bool IControlSpec.Error => error;

    void IControlSpec.Change(CompositionContext context, object changed) =>
      onChanged?.Call(context.boundary, (T)changed);
    void IControlSpec.Commit(CompositionContext context) => onCommitted?.Call(context.boundary);
  }

  public sealed class ChoiceControlSpecHandler : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec =>
      spec is IControlSpec { Formatter: IProseChoiceFormatter } ? ControlFactory<T>.Choice : null;
  }

  public abstract class ControlSpecHandler<TValue> : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec =>
      spec is IControlSpec { ValueType: var type } && type == typeof(TValue) ? Factory<T>() : null;

    protected abstract ReadComposable<T> Factory<T>() where T : struct, ISpec;
  }

  public sealed class TextControlSpecHandler : ControlSpecHandler<string> {
    protected override ReadComposable<T> Factory<T>() => ControlFactory<T>.Text;
  }

  public sealed class IntegerControlSpecHandler : ControlSpecHandler<int> {
    protected override ReadComposable<T> Factory<T>() => ControlFactory<T>.Integer;
  }

  public sealed class FloatControlSpecHandler : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      if (spec is not IControlSpec { ValueType: var type } control || type != typeof(float)) return null;
      return control.Formatter is ITextControlFormatter ? ControlFactory<T>.FloatText : ControlFactory<T>.Float;
    }
  }

  public sealed class CheckboxControlSpecHandler : ControlSpecHandler<bool> {
    protected override ReadComposable<T> Factory<T>() => ControlFactory<T>.Checkbox;
  }

  internal static class ControlFactory<T> where T : struct, ISpec {
    internal static readonly ReadComposable<T> Choice = ControlSpecFactories.Choice;
    internal static readonly ReadComposable<T> Text = ControlSpecFactories.Text;
    internal static readonly ReadComposable<T> Integer = ControlSpecFactories.Integer;
    internal static readonly ReadComposable<T> Float = ControlSpecFactories.Float;
    internal static readonly ReadComposable<T> FloatText = ControlSpecFactories.FloatText;
    internal static readonly ReadComposable<T> Checkbox = ControlSpecFactories.Checkbox;
  }

  public readonly struct NumericFormatSettings {
    public readonly string format;
    public readonly IFormatProvider provider;
    public readonly float scale;

    public NumericFormatSettings(string format = null, IFormatProvider provider = null, float scale = 1f) {
      this.format = format;
      this.provider = provider;
      this.scale = scale == 0f ? 1f : scale;
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

  [BoundaryComposable(Extension = false, UseLookupCache = true)]
  internal partial class ControlSpecBoundary {
    public partial struct Props {
      public Composable content;
      [Prop(null, Equatable = false)] public IControlSpec control;
    }

    public IControlSpec Control => props.control;
    protected override void OnRecompose(ref Composition cx) => props.content?.Invoke(ref cx);
  }

  public static class ControlSpecFactories {
    internal static void Choice<T>(ref Composition cx, in T spec) where T : struct, ISpec =>
      Compose(ref cx, in spec, ComposeChoice);

    internal static void Text<T>(ref Composition cx, in T spec) where T : struct, ISpec =>
      Compose(ref cx, in spec, ComposeText);

    internal static void Integer<T>(ref Composition cx, in T spec) where T : struct, ISpec =>
      Compose(ref cx, in spec, ComposeInteger);

    internal static void Float<T>(ref Composition cx, in T spec) where T : struct, ISpec =>
      Compose(ref cx, in spec, ComposeFloat);

    internal static void FloatText<T>(ref Composition cx, in T spec) where T : struct, ISpec =>
      Compose(ref cx, in spec, ComposeFloatText);

    internal static void Checkbox<T>(ref Composition cx, in T spec) where T : struct, ISpec =>
      Compose(ref cx, in spec, ComposeCheckbox);

    private static void Compose<T>(ref Composition cx, in T spec, Composable content) where T : struct, ISpec =>
      ControlSpecBoundary.ComposeBoundary(ref cx, content, (IControlSpec)spec);

    private static void ComposeText(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var readOnly = control.Formatter is IProseReadOnlyFormatter { ReadOnly: true };
      Decorations(control.Formatter, out var prefix, out var suffix);
      cx.TextField(
        value: new TextEditingValue((string)control.Value ?? ""),
        onChanged: TextChanged,
        onEditingEnded: EditingEnded,
        enabled: control.Enabled && !readOnly,
        options: new TextInputOptions(readOnly: readOnly),
        prefix: prefix,
        suffix: suffix
      );
    }

    private static void ComposeInteger(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var range = control.Formatter as IProseRangeFormatter<int>;
      Decorations(control.Formatter, out var prefix, out var suffix);
      cx.FieldSlider(
        (int)control.Value, IntegerChanged, NumericCommitted,
        range?.Min ?? 0, range?.Max ?? 100, range?.Step ?? 1,
        control.Enabled, control.Error,
        Formatting(control.Formatter), prefix, suffix, control.Formatter as IProseFormatter<float>
      );
    }

    private static void ComposeFloat(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var range = control.Formatter as IProseRangeFormatter<float>;
      Decorations(control.Formatter, out var prefix, out var suffix);
      cx.FieldSlider(
        (float)control.Value, FloatChanged, NumericCommitted,
        range?.Min ?? 0f, range?.Max ?? 1f, range?.Step ?? 0f,
        control.Enabled, control.Error,
        Formatting(control.Formatter), prefix, suffix, control.Formatter as IProseFormatter<float>
      );
    }

    private static void ComposeFloatText(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var formatter = Unwrap(control.Formatter);
      var formatting = Formatting(formatter);
      Decorations(control.Formatter, out var prefix, out var suffix);
      cx.TextField(
        value: new TextEditingValue(formatting.Format((float)control.Value)),
        onEditingEnded: FloatEditingEnded,
        enabled: control.Enabled,
        prefix: prefix,
        suffix: suffix
      );
    }

    private static void ComposeCheckbox(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      cx.Checkbox((bool)control.Value, onChanged: BoolChanged, enabled: control.Enabled, error: control.Error);
    }

    private static void ComposeChoice(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var formatter = (IProseChoiceFormatter)control.Formatter;
      var values = new DropdownOption<object>[formatter.ChoiceCount];
      for (var i = 0; i < values.Length; i++)
        values[i] = new DropdownOption<object>(
          formatter.GetChoiceValue(i),
          formatter.GetChoiceLabel(i),
          formatter.IsChoiceEnabled(i)
        );
      Decorations(control.Formatter, out var prefix, out var suffix);
      if (prefix == null && suffix == null) {
        RenderChoice(ref cx, control, values);
        return;
      }
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        prefix?.Invoke(ref cx);
        RenderChoice(ref cx, control, values);
        cx.CURSOR.Flexible();
        suffix?.Invoke(ref cx);
      }
    }

    private static void RenderChoice(
      ref Composition cx, IControlSpec control, DropdownOption<object>[] values
    ) => cx.DropdownButton(
      control.Value, values, onChanged: ChoiceChanged,
      enabled: control.Enabled, error: control.Error
    );

    private static void TextChanged(CompositionContext context, TextEditingValue value) =>
      Control(context).Change(context, value.text);

    private static void IntegerChanged(CompositionContext context, float value) =>
      Control(context).Change(context, (int)Math.Round(value));
    private static void FloatChanged(CompositionContext context, float value) =>
      Control(context).Change(context, value);

    private static void FloatEditingEnded(
      CompositionContext context,
      TextEditingValue value,
      TextEditEndReason reason
    ) {
      if (reason != TextEditEndReason.Submitted) return;
      var control = Control(context);
      var formatter = Unwrap(control.Formatter);
      if (Formatting(formatter).TryParse(value.text, out var parsed)) {
        var range = formatter as IProseRangeFormatter<float>;
        if (range?.Min.HasValue == true && range.Max.HasValue) {
          var options = HXSliderElement.NormalizeOptions(
            new SliderOptions(range.Min.Value, range.Max.Value, range.Step ?? 0f)
          );
          parsed = HXSliderElement.ClampAndSnap(parsed, in options);
        } else {
          if (range?.Step is > 0f) {
            var origin = range.Min ?? 0f;
            parsed = origin + Mathf.Round((parsed - origin) / range.Step.Value) * range.Step.Value;
          }
          if (range?.Min.HasValue == true) parsed = Mathf.Max(parsed, range.Min.Value);
          if (range?.Max.HasValue == true) parsed = Mathf.Min(parsed, range.Max.Value);
        }
        control.Change(context, parsed);
      }
      control.Commit(context);
    }

    private static void BoolChanged(CompositionContext context, bool value) => Control(context).Change(context, value);
    private static void ChoiceChanged(CompositionContext context, object value) => Control(context).Change(context, value);
    private static void NumericCommitted(CompositionContext context) => Control(context).Commit(context);
    private static void EditingEnded(CompositionContext context, TextEditingValue _, TextEditEndReason __) =>
      Control(context).Commit(context);
    private static IControlSpec Control(CompositionContext context) =>
      context.Lookup<ControlSpecBoundary>().Control;

    private static object Unwrap(object formatter) =>
      formatter is ITextControlFormatter text ? text.Formatter : formatter;

    private static NumericFormatSettings Formatting(object formatter) {
      formatter = Unwrap(formatter);
      if (formatter is ProseFloatFormatter number)
        return new NumericFormatSettings(number.Format, scale: number.Scale);
      if (formatter is ProseIntFormatter integer)
        return new NumericFormatSettings(integer.Format);
      return default;
    }

    private static void Decorations(object formatter, out Composable prefix, out Composable suffix) {
      if (formatter is ITextControlFormatter text && (text.Prefix != null || text.Suffix != null)) {
        prefix = text.Prefix;
        suffix = text.Suffix;
        return;
      }
      formatter = Unwrap(formatter);
      var affixes = formatter as IProseAffixFormatter;
      prefix = Text(affixes?.Prefix);
      var suffixText = affixes?.Suffix;
      if (formatter is IProseUnitFormatter unit) suffixText += unit.Unit;
      suffix = Text(suffixText);
    }

    private static Composable Text(string text) => string.IsNullOrEmpty(text)
      ? null
      : (ref Composition cx) => cx.Text(text);
  }
}
