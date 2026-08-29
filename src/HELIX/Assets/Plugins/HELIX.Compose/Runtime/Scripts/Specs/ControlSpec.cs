using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  internal interface IControlSpec {
    Type ValueType { get; }
    object Value { get; }
    object Datatype { get; }
    bool Enabled { get; }
    bool Error { get; }
    void Change(CompositionContext context, object value);
    void Commit(CompositionContext context);
  }

  internal interface IControlDatatypeWrapper {
    object Datatype { get; }
  }

  internal interface ITextControlFormatter : IControlDatatypeWrapper {
    Composable Prefix { get; }
    Composable Suffix { get; }
  }

  public readonly struct ControlSpec<T> : ISpec, IControlSpec {
    public readonly T value;
    public readonly IDatatype<T> datatype;
    public readonly CompositionAction<T> onChanged;
    public readonly CompositionAction onCommitted;
    public readonly bool enabled, error;

    public ControlSpec(
      T value,
      IDatatype<T> datatype,
      CompositionAction<T> onChanged = null,
      CompositionAction onCommitted = null,
      bool enabled = true,
      bool error = false
    ) {
      this.value = value;
      this.datatype = datatype ?? throw new ArgumentNullException(nameof(datatype));
      this.onChanged = onChanged;
      this.onCommitted = onCommitted;
      this.enabled = enabled;
      this.error = error;
    }

    Type IControlSpec.ValueType => typeof(T);
    object IControlSpec.Value => value;
    object IControlSpec.Datatype => datatype;
    bool IControlSpec.Enabled => enabled;
    bool IControlSpec.Error => error;

    void IControlSpec.Change(CompositionContext context, object changed) {
      onChanged?.Call(context.boundary, (T)changed);
    }

    void IControlSpec.Commit(CompositionContext context) {
      onCommitted?.Call(context.boundary);
    }
  }

  public sealed class ChoiceControlSpecHandler : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      return spec is IControlSpec { Datatype: IDatatypeChoice } ? ControlFactory<T>.Choice : null;
    }
  }

  public sealed class CompositeControlSpecHandler : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      return spec is IControlSpec { Datatype: ICompositeDatatype }
        ? ControlFactory<T>.Composite
        : null;
    }
  }

  public sealed class CollectionControlSpecHandler : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      return spec is IControlSpec { Datatype: ICollectionDatatype }
        ? ControlFactory<T>.Collection
        : null;
    }
  }

  public abstract class ControlSpecHandler<TValue> : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      return spec is IControlSpec { ValueType: var type } && type == typeof(TValue) ? Factory<T>() : null;
    }

    protected abstract ReadComposable<T> Factory<T>() where T : struct, ISpec;
  }

  public sealed class TextControlSpecHandler : ControlSpecHandler<string> {
    protected override ReadComposable<T> Factory<T>() {
      return ControlFactory<T>.Text;
    }
  }

  public sealed class IntegerControlSpecHandler : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      if (spec is not IControlSpec { ValueType: var type } control || type != typeof(int)) return null;
      var range = control.Datatype as IDatatypeRange<int>;
      return range?.Min.HasValue == true && range.Max.HasValue
        ? ControlFactory<T>.Integer
        : ControlFactory<T>.IntegerText;
    }
  }

  public sealed class FloatControlSpecHandler : ISpecHandler {
    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      if (spec is not IControlSpec { ValueType: var type } control || type != typeof(float)) return null;
      var range = Unwrap(control.Datatype) as IDatatypeRange<float>;
      return control.Datatype is not ITextControlFormatter &&
        range?.Min.HasValue == true && range.Max.HasValue
          ? ControlFactory<T>.Float
          : ControlFactory<T>.FloatText;
    }

    private static object Unwrap(object datatype) {
      return datatype is IControlDatatypeWrapper wrapper ? wrapper.Datatype : datatype;
    }
  }

  public sealed class CheckboxControlSpecHandler : ControlSpecHandler<bool> {
    protected override ReadComposable<T> Factory<T>() {
      return ControlFactory<T>.Checkbox;
    }
  }

  internal static class ControlFactory<T> where T : struct, ISpec {
    internal static readonly ReadComposable<T> Choice = ControlSpecFactories.Choice;
    internal static readonly ReadComposable<T> Collection = ControlSpecFactories.Collection;
    internal static readonly ReadComposable<T> Composite = ControlSpecFactories.Composite;
    internal static readonly ReadComposable<T> Text = ControlSpecFactories.Text;
    internal static readonly ReadComposable<T> Integer = ControlSpecFactories.Integer;
    internal static readonly ReadComposable<T> IntegerText = ControlSpecFactories.IntegerText;
    internal static readonly ReadComposable<T> Float = ControlSpecFactories.Float;
    internal static readonly ReadComposable<T> FloatText = ControlSpecFactories.FloatText;
    internal static readonly ReadComposable<T> Checkbox = ControlSpecFactories.Checkbox;
  }

  [Mixable]
  [BoundaryElementMixin(cacheLookups: true)]
  internal partial class ControlSpecBoundary {
    private UntypedChoiceDatatype _choiceDatatype;

    public IControlSpec Control => props.control;

    public IDatatypeChoice<object> ChoiceDatatype(IDatatypeChoice datatype) {
      _choiceDatatype ??= new UntypedChoiceDatatype();
      _choiceDatatype.Synchronize(datatype);
      return _choiceDatatype;
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      props.content?.Invoke(ref cx);
    }

    public partial struct Props {
      public Composable content;
      [Prop(null, Equatable = false)] public IControlSpec control;
    }
  }

  internal sealed class UntypedChoiceDatatype : IDatatypeChoice<object> {
    private IDatatypeChoice _datatype;
    private bool[] _enabled;
    private string[] _labels;
    private object[] _values;

    public int ChoiceCount => _values?.Length ?? 0;

    public object GetChoiceValue(int index) {
      return _values[index];
    }

    public object GetTypedChoiceValue(int index) {
      return _values[index];
    }

    public string GetChoiceLabel(int index) {
      return _labels[index];
    }

    public bool IsChoiceEnabled(int index) {
      return _enabled[index];
    }

    public void Synchronize(IDatatypeChoice datatype) {
      var count = datatype.ChoiceCount;
      if (ReferenceEquals(_datatype, datatype) && _values?.Length == count) return;
      _datatype = datatype;
      _values = new object[count];
      _labels = new string[count];
      _enabled = new bool[count];
      for (var i = 0; i < count; i++) {
        _values[i] = datatype.GetChoiceValue(i);
        _labels[i] = datatype.GetChoiceLabel(i) ?? string.Empty;
        _enabled[i] = datatype.IsChoiceEnabled(i);
      }
    }
  }

  public static class ControlSpecFactories {
    internal static void Choice<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeChoice);
    }

    internal static void Composite<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeComposite);
    }

    internal static void Collection<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeCollection);
    }

    internal static void Text<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeText);
    }

    internal static void Integer<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeInteger);
    }

    internal static void IntegerText<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeIntegerText);
    }

    internal static void Float<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeFloat);
    }

    internal static void FloatText<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeFloatText);
    }

    internal static void Checkbox<T>(ref Composition cx, in T spec) where T : struct, ISpec {
      Compose(ref cx, in spec, ComposeCheckbox);
    }

    private static void Compose<T>(ref Composition cx, in T spec, Composable content) where T : struct, ISpec {
      ControlSpecBoundary.ComposeBoundary(ref cx, content, (IControlSpec)spec);
    }

    private static void ComposeText(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var readOnly = control.Datatype is IDatatypeReadOnly { ReadOnly: true };
      Decorations(control.Datatype, out var prefix, out var suffix);
      cx.DatatypeTextField(
        (string)control.Value ?? "",
        (IStringConvertible<string>)control.Datatype,
        TextChanged,
        TextEditingEnded,
        enabled: control.Enabled && !readOnly,
        options: new TextInputOptions(readOnly: readOnly),
        prefix: prefix,
        suffix: suffix
      );
    }

    private static void ComposeInteger(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var range = control.Datatype as IDatatypeRange<int>;
      Decorations(control.Datatype, out var prefix, out var suffix);
      cx.DatatypeFieldSlider(
        (int)control.Value,
        (IDatatype<int>)control.Datatype,
        IntegerChanged,
        NumericCommitted,
        range?.Min ?? 0,
        range?.Max ?? 100,
        range?.Step ?? 1,
        control.Enabled,
        control.Error,
        prefix,
        suffix
      );
    }

    private static void ComposeIntegerText(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var formatter = Unwrap(control.Datatype);
      Decorations(control.Datatype, out var prefix, out var suffix);
      cx.DatatypeTextField(
        (int)control.Value,
        (IStringConvertible<int>)formatter,
        onEditingEnded: IntegerTextEditingEnded,
        enabled: control.Enabled,
        prefix: prefix,
        suffix: suffix
      );
    }

    private static void ComposeFloat(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var range = control.Datatype as IDatatypeRange<float>;
      Decorations(control.Datatype, out var prefix, out var suffix);
      cx.DatatypeFieldSlider(
        (float)control.Value,
        (IDatatype<float>)control.Datatype,
        FloatChanged,
        NumericCommitted,
        range?.Min ?? 0f,
        range?.Max ?? 1f,
        range?.Step ?? 0f,
        control.Enabled,
        control.Error,
        prefix,
        suffix
      );
    }

    private static void ComposeFloatText(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var formatter = Unwrap(control.Datatype);
      Decorations(control.Datatype, out var prefix, out var suffix);
      cx.DatatypeTextField(
        (float)control.Value,
        (IStringConvertible<float>)formatter,
        onEditingEnded: FloatTextEditingEnded,
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
      var boundary = cx.Lookup<ControlSpecBoundary>();
      var control = boundary.Control;
      var formatter = (IDatatypeChoice)control.Datatype;
      var datatype = boundary.ChoiceDatatype(formatter);
      Decorations(control.Datatype, out var prefix, out var suffix);
      if (prefix == null && suffix == null) {
        RenderChoice(ref cx, control, datatype);
        return;
      }
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        prefix?.Invoke(ref cx);
        RenderChoice(ref cx, control, datatype);
        cx.CURSOR.Flexible();
        suffix?.Invoke(ref cx);
      }
    }

    private static void ComposeComposite(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var datatype = (ICompositeDatatype)control.Datatype;
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        for (var i = 0; i < datatype.ComponentCount; i++) {
          if (i != 0) cx.Spacing(1);
          cx.Text(datatype.GetComponentName(i));
          cx.Gap(ThemeProperties.TextGap[in cx]);
          cx.Spec(new CompositeComponentControlSpec(control, datatype, i));
          cx.CURSOR.Flexible(1f, 0f);
        }
      }
    }

    private static void ComposeCollection(ref Composition cx) {
      var control = cx.Lookup<ControlSpecBoundary>().Control;
      var datatype = (ICollectionDatatype)control.Datatype;
      var proxy = datatype.CollectionProxy;
      var count = control.Value == null ? 0 : proxy.GetItemCount(control.Value);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        for (var i = 0; i < count; i++) {
          if (i != 0) cx.Spacing(1);
          using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
            cx.Text(datatype.GetComponentName(i));
            cx.Gap(ThemeProperties.TextGap[in cx]);
            cx.Spec(new CollectionItemControlSpec(control, datatype, i));
            cx.CURSOR.Flexible(1f, 0f);
          }
        }
      }
    }

    private static void RenderChoice(
      ref Composition cx,
      IControlSpec control,
      IDatatypeChoice<object> datatype
    ) {
      if (Unwrap(control.Datatype) is FlagDatatype) {
        cx.SegmentedChoice(
          control.Value,
          datatype,
          ChoiceChanged,
          control.Enabled,
          control.Error
        );
      } else {
        cx.ChoiceSpinbox(
          control.Value,
          datatype,
          ChoiceChanged,
          control.Enabled,
          control.Error
        );
      }
    }

    private static void TextChanged(CompositionContext context, string value) {
      Control(context).Change(context, value);
    }

    private static void IntegerChanged(CompositionContext context, int value) {
      Control(context).Change(context, value);
    }

    private static void FloatChanged(CompositionContext context, float value) {
      Control(context).Change(context, value);
    }

    private static void IntegerTextEditingEnded(
      CompositionContext context,
      int parsed,
      TextEditEndReason reason
    ) {
      if (reason == TextEditEndReason.Cancelled) return;
      var control = Control(context);
      var range = Unwrap(control.Datatype) as IDatatypeRange<int>;
      if (range?.Step is > 0) {
        var origin = range.Min ?? 0;
        parsed = origin + Mathf.RoundToInt((parsed - origin) / (float)range.Step.Value) * range.Step.Value;
      }
      if (range?.Min.HasValue == true) parsed = Mathf.Max(parsed, range.Min.Value);
      if (range?.Max.HasValue == true) parsed = Mathf.Min(parsed, range.Max.Value);
      control.Change(context, parsed);
      control.Commit(context);
    }

    private static void FloatTextEditingEnded(
      CompositionContext context,
      float parsed,
      TextEditEndReason reason
    ) {
      if (reason == TextEditEndReason.Cancelled) return;
      var control = Control(context);
      var formatter = Unwrap(control.Datatype);
      var range = formatter as IDatatypeRange<float>;
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
      control.Commit(context);
    }

    private static void BoolChanged(CompositionContext context, bool value) {
      Control(context).Change(context, value);
    }

    private static void ChoiceChanged(CompositionContext context, object value) {
      Control(context).Change(context, value);
    }

    private static void NumericCommitted(CompositionContext context) {
      Control(context).Commit(context);
    }

    private static void TextEditingEnded(CompositionContext context, string _, TextEditEndReason reason) {
      if (reason != TextEditEndReason.Cancelled) Control(context).Commit(context);
    }

    private static IControlSpec Control(CompositionContext context) {
      return context.Lookup<ControlSpecBoundary>().Control;
    }

    private static object Unwrap(object formatter) {
      return formatter is IControlDatatypeWrapper wrapper ? wrapper.Datatype : formatter;
    }

    internal static void Decorations(object formatter, out Composable prefix, out Composable suffix) {
      if (formatter is ITextControlFormatter text && (text.Prefix != null || text.Suffix != null)) {
        prefix = text.Prefix;
        suffix = text.Suffix;
        return;
      }
      formatter = Unwrap(formatter);
      var affixes = formatter as IDatatypeAffix;
      prefix = Text(affixes?.Prefix);
      var suffixText = affixes?.Suffix;
      if (formatter is IDatatypeUnit unit) suffixText += unit.Unit;
      suffix = Text(suffixText);
    }

    private static Composable Text(string text) {
      return string.IsNullOrEmpty(text)
        ? null
        : (ref Composition cx) => cx.Text(text);
    }
  }

  internal readonly struct CompositeComponentControlSpec : ISpec, IControlSpec {
    private readonly IControlSpec _parent;
    private readonly ICompositeDatatype _datatype;
    private readonly int _index;

    public CompositeComponentControlSpec(IControlSpec parent, ICompositeDatatype datatype, int index) {
      _parent = parent;
      _datatype = datatype;
      _index = index;
    }

    Type IControlSpec.ValueType => _datatype.GetComponentType(_index);
    object IControlSpec.Value => _datatype.GetComponentValue(_parent.Value, _index);
    object IControlSpec.Datatype => _datatype.GetComponentDatatype(_index);
    bool IControlSpec.Enabled => _parent.Enabled;
    bool IControlSpec.Error => _parent.Error;

    void IControlSpec.Change(CompositionContext context, object value) {
      _parent.Change(
        context,
        _datatype.SetComponentValue(_parent.Value, _index, value)
      );
    }

    void IControlSpec.Commit(CompositionContext context) {
      _parent.Commit(context);
    }
  }

  internal readonly struct CollectionItemControlSpec : ISpec, IControlSpec {
    private readonly IControlSpec _parent;
    private readonly ICollectionDatatype _datatype;
    private readonly int _index;

    public CollectionItemControlSpec(IControlSpec parent, ICollectionDatatype datatype, int index) {
      _parent = parent;
      _datatype = datatype;
      _index = index;
    }

    Type IControlSpec.ValueType => _datatype.CollectionProxy.ItemType;
    object IControlSpec.Value => _datatype.CollectionProxy.GetItem(_parent.Value, _index);
    object IControlSpec.Datatype => _datatype.ItemDatatype;
    bool IControlSpec.Enabled => _parent.Enabled;
    bool IControlSpec.Error => _parent.Error;

    void IControlSpec.Change(CompositionContext context, object value) {
      _parent.Change(
        context,
        _datatype.CollectionProxy.SetItem(_parent.Value, _index, value)
      );
    }

    void IControlSpec.Commit(CompositionContext context) {
      _parent.Commit(context);
    }
  }
}