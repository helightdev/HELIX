using System.Collections.Generic;
using HELIX.Compose.Forms;

namespace HELIX.Compose {
  public readonly struct FormField<T> : ISpec {
    public readonly string path;
    public readonly HXOptional<T> initialValue;
    public readonly FormFieldDecorators decorators;
    public readonly IReadOnlyList<IFormValidator> validators;
    public readonly ValidationMode validationMode;
    public readonly IEqualityComparer<object> comparer;
    public readonly bool enabled;

    public FormField(
      string path,
      HXOptional<T> initialValue = default,
      FormFieldDecorators decorators = default,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true
    ) {
      this.path = path;
      this.initialValue = initialValue;
      this.decorators = decorators;
      this.validators = validators;
      this.validationMode = validationMode;
      this.comparer = comparer;
      this.enabled = enabled;
    }
  }

  public readonly struct StringFormField : ISpec {
    public readonly FormField<string> field;
    public readonly string placeholder;
    public readonly bool multiline, readOnly;
    public StringFormField(FormField<string> field, string placeholder = null, bool multiline = false, bool readOnly = false) {
      this.field = field;
      this.placeholder = placeholder;
      this.multiline = multiline;
      this.readOnly = readOnly;
    }
  }

  public readonly struct IntFormField : ISpec {
    public readonly FormField<int> field;
    public readonly HXOptional<int> min, max, step;
    public IntFormField(FormField<int> field, HXOptional<int> min = default, HXOptional<int> max = default,
      HXOptional<int> step = default) {
      this.field = field; this.min = min; this.max = max; this.step = step;
    }
  }

  public readonly struct FloatFormField : ISpec {
    public readonly FormField<float> field;
    public readonly HXOptional<float> min, max, step;
    public readonly NumericFormatSettings formatting;
    public readonly Composable prefix, suffix;
    public FloatFormField(FormField<float> field, HXOptional<float> min = default,
      HXOptional<float> max = default, HXOptional<float> step = default,
      NumericFormatSettings formatting = default, Composable prefix = null, Composable suffix = null) {
      this.field = field; this.min = min; this.max = max; this.step = step;
      this.formatting = formatting; this.prefix = prefix; this.suffix = suffix;
    }
  }

  public readonly struct BoolFormField : ISpec {
    public readonly FormField<bool> field;
    public BoolFormField(FormField<bool> field) => this.field = field;
  }

  public readonly struct EnumFormField : ISpec {
    public readonly FormField<object> field;
    public readonly IReadOnlyList<DropdownOption<object>> values;
    public EnumFormField(FormField<object> field, IReadOnlyList<DropdownOption<object>> values) {
      this.field = field;
      this.values = values;
    }
  }

  public static class FormFieldFactories {
    public static void Text(ref Composition cx, in FormField<string> spec) =>
      Compose(ref cx, in spec, spec, ComposeText);
    public static void Integer(ref Composition cx, in FormField<int> spec) =>
      Compose(ref cx, in spec, spec, ComposeInteger);
    public static void Float(ref Composition cx, in FormField<float> spec) =>
      Compose(ref cx, in spec, spec, ComposeFloat);
    public static void Checkbox(ref Composition cx, in FormField<bool> spec) =>
      Compose(ref cx, in spec, spec, ComposeCheckbox, DecoratorArrangement.Inline);

    public static void Text(ref Composition cx, in StringFormField spec) =>
      Compose(ref cx, in spec.field, spec, ComposeSpecializedText);
    public static void Integer(ref Composition cx, in IntFormField spec) =>
      Compose(ref cx, in spec.field, spec, ComposeSpecializedInteger);
    public static void Float(ref Composition cx, in FloatFormField spec) =>
      Compose(ref cx, in spec.field, spec, ComposeSpecializedFloat);
    public static void Checkbox(ref Composition cx, in BoolFormField spec) =>
      Compose(ref cx, in spec.field, spec, ComposeCheckbox, DecoratorArrangement.Inline);
    public static void Enum(ref Composition cx, in EnumFormField spec) =>
      Compose(ref cx, in spec.field, spec, ComposeEnum);

    public static void Compose<T>(ref Composition cx, in FormField<T> field, object metadata,
      Composable content, DecoratorArrangement arrangement = DecoratorArrangement.Stacked) {
      cx.FormField(
        field.path, content,
        style: new HXFormFieldStyle(arrangement: arrangement, decorators: field.decorators),
        validators: field.validators, validationMode: field.validationMode,
        initialValue: field.initialValue.hasValue ? field.initialValue.value : FormController.NoInitialValue,
        comparer: field.comparer, enabled: field.enabled, metadata: metadata
      );
    }

    private static void ComposeText(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      cx.Spec(new ControlSpec<string>(field.GetValue(""), SetString, FinishEditing,
        IsEnabled(field), HasError(field)));
    }
    private static void ComposeSpecializedText(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      var spec = field.Metadata<StringFormField>();
      cx.Spec(new StringControlSpec(
        new ControlSpec<string>(field.GetValue(""), SetString, FinishEditing, IsEnabled(field), HasError(field)),
        spec.placeholder, spec.multiline, spec.readOnly
      ));
    }
    private static void ComposeInteger(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      cx.Spec(new ControlSpec<int>(field.GetValue(0), SetInt, FinishEditing, IsEnabled(field), HasError(field)));
    }
    private static void ComposeSpecializedInteger(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      var spec = field.Metadata<IntFormField>();
      cx.Spec(new IntControlSpec(
        new ControlSpec<int>(field.GetValue(0), SetInt, FinishEditing, IsEnabled(field), HasError(field)),
        spec.min, spec.max, spec.step
      ));
    }
    private static void ComposeFloat(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      cx.Spec(new ControlSpec<float>(field.GetValue(0f), SetFloat, FinishEditing,
        IsEnabled(field), HasError(field)));
    }
    private static void ComposeSpecializedFloat(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      var spec = field.Metadata<FloatFormField>();
      cx.Spec(new FloatControlSpec(
        new ControlSpec<float>(field.GetValue(0f), SetFloat, FinishEditing, IsEnabled(field), HasError(field)),
        spec.min, spec.max, spec.step, formatting: spec.formatting, prefix: spec.prefix, suffix: spec.suffix
      ));
    }
    private static void ComposeCheckbox(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      cx.Spec(new BoolControlSpec(new ControlSpec<bool>(
        field.GetValue(false), SetBool, enabled: IsEnabled(field), error: HasError(field)
      )));
    }
    private static void ComposeEnum(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      var spec = field.Metadata<EnumFormField>();
      cx.Spec(new EnumControlSpec(
        new ControlSpec<object>(field.Value, SetObject, enabled: IsEnabled(field), error: HasError(field)),
        spec.values
      ));
    }

    private static bool IsEnabled(HXFormField field) => field.FieldData?.HasFlag(FieldFlags.Disabled) != true;
    private static bool HasError(HXFormField field) => field.FieldData?.HasFlag(FieldFlags.Error) == true;
    private static void SetString(CompositionContext context, string value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetInt(CompositionContext context, int value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetFloat(CompositionContext context, float value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetBool(CompositionContext context, bool value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetObject(CompositionContext context, object value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void FinishEditing(CompositionContext context) =>
      context.Lookup<HXFormField>()?.MarkFinishedEditing();
  }
}
