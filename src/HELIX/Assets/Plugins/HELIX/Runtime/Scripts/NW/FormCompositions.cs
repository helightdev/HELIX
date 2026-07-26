using System;
using System.Collections.Generic;
using System.Globalization;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.NW.Forms;
using HELIX.NW.Overlays;
using HELIX.Types;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

namespace HELIX.NW.Forms {
  public readonly struct FormContextData : IEquatable<FormContextData> {
    public static readonly ContextKey<FormContextData> Key = new("NW.FormContext");

    public readonly FormController controller;
    public readonly string prefix;

    public FormContextData(FormController controller, string prefix) {
      this.controller = controller;
      this.prefix = FormPath.Normalize(prefix);
    }

    public string Resolve(string path) => FormPath.Compose(prefix, path);

    public bool Equals(FormContextData other) {
      return ReferenceEquals(controller, other.controller) &&
             string.Equals(prefix, other.prefix, StringComparison.Ordinal);
    }

    public override bool Equals(object obj) => obj is FormContextData other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(controller, prefix);
  }

  internal static class FormContextWriter {
    public static ContextScope<FormContextData> WriteStable(
      ref Composition cx,
      in FormContextData value
    ) {
      var scope = cx.WritableContext(FormContextData.Key, out var data);
      if (!data.GetValueRef().Equals(value)) {
        data.GetValueRef() = value;
        data.IncrementContextVersion(ContextFlags.None);
      }
      return scope;
    }
  }

  public abstract class FormFieldStateBase<TProps> : PropsNodeStateAttachmentBase<TProps>, IFormField
    where TProps : struct {
    private FormController _form;
    private string _path;
    private bool _binding;

    protected FormController Form => _form;
    protected string Path => _path;
    protected FieldData FieldData => _form?.GetFieldData(_path);
    protected bool FieldEnabled => FieldData?.HasFlag(FieldFlags.Disabled) != true;
    protected bool FieldHasError => FieldData?.HasFlag(FieldFlags.Error) == true;

    protected void Bind<T>(
      ref Composition cx,
      string localPath,
      IReadOnlyList<IFormValidator> validators,
      ValidationMode validationMode,
      T initialValue,
      IEqualityComparer<object> comparer,
      bool enabled,
      bool list = false,
      int initialListCount = 0
    ) {
      var context = cx.ReadContext(FormContextData.Key);
      if (context.controller == null) {
        throw new InvalidOperationException(
          $"{GetType().Name} must be composed below Form or FormScope."
        );
      }

      var resolvedPath = context.Resolve(localPath);
      if (!ReferenceEquals(_form, context.controller) ||
          !string.Equals(_path, resolvedPath, StringComparison.Ordinal)) {
        _form?.UnregisterField(_path, this, false);
        _form = context.controller;
        _path = FormPath.Require(resolvedPath);
      }

      _binding = true;
      try {
        if (list) {
          _form.RegisterListField(
            _path,
            this,
            validators,
            validationMode,
            enabled,
            initialListCount
          );
        } else {
          _form.RegisterField(
            _path,
            this,
            validators,
            validationMode,
            initialValue,
            comparer,
            enabled
          );
        }
      } finally {
        _binding = false;
      }
    }

    protected T ReadValue<T>(T fallback = default) {
      return _form == null ? fallback : _form.GetValue(_path, fallback);
    }

    protected void SetUserValue<T>(T value) {
      _form?.SetValue(_path, value, FormChangeReason.User);
    }

    protected void FinishEditing() {
      _form?.MarkFinishedEditing(_path);
    }

    public void OnFormFieldChanged(FormController form, string path) {
      if (_binding ||
          Node == null ||
          !ReferenceEquals(form, _form) ||
          !string.Equals(path, _path, StringComparison.Ordinal)) {
        return;
      }
      Node.MarkDirty();
    }

    protected override void OnDetach() {
      _form?.UnregisterField(_path, this, false);
      _form = null;
      _path = null;
      base.OnDetach();
    }
  }

  public static partial class FormDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef Form(
      ref this Composition cx,
      [Prop] Composable<FormController> content,
      [Prop] FormController controller = null
    );

    public partial class FormState {
      private FormController _ownedController;

      protected override void OnRecompose(ref Composition cx) {
        if (Props.Controller != null && _ownedController != null) {
          _ownedController.Dispose();
          _ownedController = null;
        }

        var controller = Props.Controller ?? (_ownedController ??= new FormController());
        var context = new FormContextData(controller, string.Empty);
        using (FormContextWriter.WriteStable(ref cx, in context)) {
          Props.Content?.Invoke(ref cx, controller);
        }
      }

      protected override void OnDetach() {
        _ownedController?.Dispose();
        _ownedController = null;
        base.OnDetach();
      }
    }
  }

  public static partial class FormScopeDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef FormScope(
      ref this Composition cx,
      [Prop] string prefix,
      [Prop] Composable content
    );

    public partial class FormScopeState {
      private FormController _controller;
      private string _parentPrefix;
      private string _localPrefix;
      private string _resolvedPrefix;

      protected override void OnRecompose(ref Composition cx) {
        var parent = cx.ReadContext(FormContextData.Key);
        if (parent.controller == null) {
          throw new InvalidOperationException("FormScope must be composed below Form.");
        }

        if (!ReferenceEquals(_controller, parent.controller) ||
            !string.Equals(_parentPrefix, parent.prefix, StringComparison.Ordinal) ||
            !string.Equals(_localPrefix, Props.Prefix, StringComparison.Ordinal)) {
          _controller = parent.controller;
          _parentPrefix = parent.prefix;
          _localPrefix = Props.Prefix;
          _resolvedPrefix = FormPath.Compose(parent.prefix, Props.Prefix);
        }

        var context = new FormContextData(_controller, _resolvedPrefix);
        using (FormContextWriter.WriteStable(ref cx, in context)) {
          Props.Content?.Invoke(ref cx);
        }
      }
    }
  }

  public readonly struct FormFieldInput<T> {
    public readonly T value;
    public readonly bool enabled;
    public readonly bool error;
    public readonly Action<T, IBoundary> changed;
    public readonly Action<T, IBoundary> committed;
    public readonly Action<IBoundary> finishedEditing;

    internal FormFieldInput(
      T value,
      bool enabled,
      bool error,
      Action<T, IBoundary> changed,
      Action<T, IBoundary> committed,
      Action<IBoundary> finishedEditing
    ) {
      this.value = value;
      this.enabled = enabled;
      this.error = error;
      this.changed = changed;
      this.committed = committed;
      this.finishedEditing = finishedEditing;
    }
  }

  public delegate void FormInputComposable<TValue, TInput>(
    ref Composition cx,
    in FormFieldInput<TValue> field,
    in TInput input
  ) where TInput : struct;

  public delegate void FormInputComposable<TValue>(
    ref Composition cx,
    in FormFieldInput<TValue> field
  );

  public readonly struct FormInputSpec<TValue> {
    public readonly FormInputComposable<TValue> compose;

    public FormInputSpec(FormInputComposable<TValue> compose) {
      this.compose = compose;
    }
  }

  public enum FormFieldAnchor : byte { Prefix, Suffix, Before, Between, After }

  public enum DecoratorLayout { Field, Side }

  public readonly struct FormFieldDecorators {
    public readonly Composable prefix;
    public readonly Composable suffix;
    public readonly Composable before;
    public readonly Composable between;
    public readonly Composable after;

    public FormFieldDecorators(
      Composable prefix = null,
      Composable suffix = null,
      Composable before = null,
      Composable between = null,
      Composable after = null
    ) {
      this.prefix = prefix;
      this.suffix = suffix;
      this.before = before;
      this.between = between;
      this.after = after;
    }
  }

  public enum DecoratorSlotType {
    None,
    Before,
    After,
    Prefix,
    Element,
    Suffix,
    Between,
    Label,
    Descriptor
  }

  public static class DecoratorExtensions {
    public static StyleLength4 GetMargin(this DecoratorSlotType type, ThemeData data) {
      return type switch {
        DecoratorSlotType.Before => EdgeInsets.Only(bottom: CommonDecoratorElement.GapColumn[data]),
        DecoratorSlotType.After => EdgeInsets.Only(top: CommonDecoratorElement.GapColumn[data]),
        DecoratorSlotType.Prefix => EdgeInsets.Only(right: CommonDecoratorElement.GapRow[data]),
        DecoratorSlotType.Suffix => EdgeInsets.Only(left: CommonDecoratorElement.GapRow[data]),
        DecoratorSlotType.None or DecoratorSlotType.Element or DecoratorSlotType.Between or DecoratorSlotType.Label
          or DecoratorSlotType.Descriptor => EdgeInsets.Zero,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
    }

    public static TextStyle? GetTextStyle(this DecoratorSlotType type, ThemeData data) {
      return type switch {
        DecoratorSlotType.Label => CommonDecoratorElement.LabelStyle[data],
        DecoratorSlotType.Descriptor => CommonDecoratorElement.DescriptionStyle[data],
        DecoratorSlotType.Prefix => CommonDecoratorElement.PrefixStyle[data],
        DecoratorSlotType.Suffix => CommonDecoratorElement.SuffixStyle[data],
        DecoratorSlotType.Before or DecoratorSlotType.Between or DecoratorSlotType.After => CommonDecoratorElement.DecoratorStyle[data],
        _ => null
      };
    }

  }

  public ref struct DecoratorSlots {
    private readonly CommonDecoratorElement _element;
    private readonly Composition _composition;

    public DecoratorSlots(CommonDecoratorElement element, Composition composition) {
      _element = element;
      _composition = composition;
    }

    public ScopeHandle Element() => _element.element.Scope(_composition);
    public ScopeHandle Label() => _element.label.Scope(_composition);
    public ScopeHandle Description() => _element.description.Scope(_composition);
    public ScopeHandle Prefix() => _element.prefix.Scope(_composition);
    public ScopeHandle Suffix() => _element.suffix.Scope(_composition);
    public ScopeHandle Before() => _element.before.Scope(_composition);
    public ScopeHandle Between() => _element.between.Scope(_composition);
    public ScopeHandle After() => _element.after.Scope(_composition);
  }

  public class CommonDecoratorElement : VisualElement, IComposable, ISlotHost {
    public static ThemeProperty<float> GapColumn = new(data => data[SpacingRole.Spacing1]);
    public static ThemeProperty<float> GapRow = new(data => data[SpacingRole.Spacing1]);

    public static ThemeProperty<TextStyle> LabelStyle = new(data => data.GetTextStyleRef(TextRole.LabelLarge));
    public static ThemeProperty<TextStyle> DescriptionStyle = new(data => data.GetTextStyleRef(TextRole.LabelMedium));
    public static ThemeProperty<TextStyle> PrefixStyle = new(data => data.GetTextStyleRef(TextRole.BodyMedium));
    public static ThemeProperty<TextStyle> SuffixStyle = new(data => data.GetTextStyleRef(TextRole.BodyMedium));
    public static ThemeProperty<TextStyle> DecoratorStyle = new(data => data.GetTextStyleRef(TextRole.LabelMedium));

    public static readonly UniqueStyleString ClassElement = new("hx-decorator-element");
    public static readonly UniqueStyleString ClassLabel = new("hx-decorator-label");
    public static readonly UniqueStyleString ClassDescription = new("hx-decorator-description");
    public static readonly UniqueStyleString ClassPrefix = new("hx-decorator-prefix");
    public static readonly UniqueStyleString ClassSuffix = new("hx-decorator-suffix");
    public static readonly UniqueStyleString ClassBefore = new("hx-decorator-before");
    public static readonly UniqueStyleString ClassAfter = new("hx-decorator-after");
    public static readonly UniqueStyleString ClassBetween = new("hx-decorator-between");

    public static DecoratorSlotType GetType(HxSlot slot) {
      var id = slot.SlotType;
      if (id == ClassElement.id) return DecoratorSlotType.Element;
      if (id == ClassLabel.id) return DecoratorSlotType.Label;
      if (id == ClassDescription.id) return DecoratorSlotType.Descriptor;
      if (id == ClassPrefix.id) return DecoratorSlotType.Prefix;
      if (id == ClassSuffix.id) return DecoratorSlotType.Suffix;
      if (id == ClassBefore.id) return DecoratorSlotType.Before;
      if (id == ClassAfter.id) return DecoratorSlotType.After;
      if (id == ClassBetween.id) return DecoratorSlotType.Between;
      return DecoratorSlotType.None;
    }

    public static void ComposeScopedLabel(ref Composition cx, in LabelSpec spec) {
      if (cx.Slot == null) return;
      var type = GetType(cx.Slot);
      var data = ThemeData.Context.ReadScope();
      using (cx.Flex(Axis.Horizontal)) {
        cx.APPLY.AlignSelf(Align.FlexStart);
        cx.APPLY.Margin(type.GetMargin(data));
        type.GetTextStyle(data)?.Apply(cx.APPLY.composable);

        LabelSpec.Default(ref cx, in spec);
      }
    }

    public HxSlot before;
    public HxSlot after;
    public HxSlot prefix;
    public HxSlot element;
    public HxSlot suffix;
    public HxSlot between;

    public HxSlot label;
    public HxSlot description;

    public VisualElement column;
    public VisualElement row;

    public override VisualElement contentContainer => element;

    public CommonDecoratorElement() {
      column = new VisualElement().AddTo(hierarchy);

      label = new HxSlot(this, ClassLabel).WithClasses(ClassLabel).AddTo(column);
      before = new HxSlot(this, ClassBefore).WithClasses(ClassBefore).AddTo(column);
      row = new VisualElement().FlexContainer(Axis.Horizontal).AddTo(column).Flexible(0, 1, Align.Stretch);
      between = new HxSlot(this, ClassBetween).WithClasses(ClassBetween).AddTo(column);
      description = new HxSlot(this, ClassDescription).WithClasses(ClassDescription).AddTo(column);
      after = new HxSlot(this, ClassAfter).WithClasses(ClassAfter).AddTo(column);
      prefix = new HxSlot(this, ClassPrefix).WithClasses(ClassPrefix).AddTo(row);
      element = new HxSlot(this, ClassElement).WithClasses(ClassElement).AddTo(row);
      suffix = new HxSlot(this, ClassSuffix).WithClasses(ClassSuffix).AddTo(row);
    }

    public void Initialize(in Composition cx) {
      Boundary = cx.boundary;
    }

    public VisualElement Element => this;
    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }

    public void Reset() {
      before?.Reset();
      after?.Reset();
      prefix?.Reset();
      element?.Reset();
      suffix?.Reset();
      between?.Reset();
      label?.Reset();
      description?.Reset();
      Boundary = null;
    }

    public IBoundary Boundary { get; private set; }
  }

  public readonly struct FormFieldOptions {
    public readonly LabelSpec? label;
    public readonly LabelSpec? tooltip;
    public readonly LabelSpec? description;
    public readonly FormFieldDecorators decorators;

    public readonly FormFieldAnchor anchor;
    public readonly float gap;
    public readonly Align alignment;

    public bool IsEmpty => !label.HasValue && !tooltip.HasValue && !description.HasValue;

    public FormFieldOptions(
      LabelSpec? label = null,
      LabelSpec? tooltip = null,
      LabelSpec? description = null,
      FormFieldDecorators decorators = default,
      FormFieldAnchor anchor = FormFieldAnchor.Before,
      float gap = 4f,
      Align alignment = Align.Stretch
    ) {
      this.label = label;
      this.tooltip = tooltip;
      this.description = description;
      this.anchor = anchor;
      this.gap = gap;
      this.alignment = alignment;
      this.decorators = decorators;
    }

    public void Decorate(ref Composition cx, Composable input) {
      var layout = DecoratorLayout.Field;

      if (IsEmpty) {
        input?.Invoke(ref cx);
        return;
      }
      using (cx.Flex(Axis.Vertical)) {
        decorators.before?.Invoke(ref cx);
        using (cx.Flex(Axis.Horizontal)) {
          cx.APPLY.AlignSelf(Align.Stretch);
          decorators.prefix?.Invoke(ref cx);
          input?.Invoke(ref cx);
          if (layout == DecoratorLayout.Side) { }

          decorators.suffix?.Invoke(ref cx);
        }

        decorators.between?.Invoke(ref cx);

        decorators.after?.Invoke(ref cx);
      }


      // var axis = anchor is FormFieldAnchor.Prefix or FormFieldAnchor.Suffix
      //   ? Axis.Horizontal
      //   : Axis.Vertical;
      // using (cx.Flex(axis, cross: alignment)) {
      //   cx.APPLY.FlexShrink(0f);
      //   if (anchor is FormFieldAnchor.Prefix or FormFieldAnchor.Before) {
      //     var hadPrevious = false;
      //     ComposeLabels(ref cx, ref hadPrevious);
      //     if (hadPrevious) cx.Gap(gap);
      //     input?.Invoke(ref cx);
      //   } else if (anchor == FormFieldAnchor.Between) {
      //     var hadPrevious = false;
      //     ComposeLabel(ref cx, label, ref hadPrevious);
      //     input?.Invoke(ref cx);
      //     if (input != null) hadPrevious = true;
      //     ComposeLabel(ref cx, tooltip, ref hadPrevious);
      //     ComposeLabel(ref cx, description, ref hadPrevious);
      //   } else {
      //     input?.Invoke(ref cx);
      //     var hadPrevious = input != null;
      //     ComposeLabels(ref cx, ref hadPrevious);
      //   }
      // }
    }

    private void ComposeLabel(ref Composition cx, LabelSpec? content, ref bool hadPrevious) {
      if (!content.HasValue) return;
      if (hadPrevious) {
        cx.Gap(gap);
      }

      var value = content.Value;
      cx.Label(in value);
      hadPrevious = true;
    }

    private void ComposeLabels(ref Composition cx, ref bool hadPrevious) {
      ComposeLabel(ref cx, label, ref hadPrevious);
      ComposeLabel(ref cx, tooltip, ref hadPrevious);
      ComposeLabel(ref cx, description, ref hadPrevious);
    }
  }

  public struct FormFieldProps<TValue, TInput> where TInput : struct {
    public string path;
    public TValue initialValue;
    public IReadOnlyList<IFormValidator> validators;
    public ValidationMode validationMode;
    public IEqualityComparer<object> comparer;
    public bool enabled;
    public bool finishOnChange;
    public FormInputComposable<TValue, TInput> composeInput;
    public TInput input;
    public Action<TValue, IBoundary> onChanged;
    public Action<TValue, IBoundary> onCommitted;
    public FormFieldOptions? decorator;
  }

  internal static class FormFieldIdentity<TValue, TInput> where TInput : struct {
    public static readonly ushort TypeId = CompositionId.GetTypeId(
      $"FormField<{typeof(TValue).FullName},{typeof(TInput).FullName}>"
    );
  }

  public sealed class FormFieldState<TValue, TInput> :
    FormFieldStateBase<FormFieldProps<TValue, TInput>>
    where TInput : struct {
    private readonly Action<TValue, IBoundary> _changed;
    private readonly Action<TValue, IBoundary> _committed;
    private readonly Action<IBoundary> _finished;

    public FormFieldState() {
      _changed = HandleChanged;
      _committed = HandleCommitted;
      _finished = HandleFinished;
    }

    protected override void OnRecompose(ref Composition cx) {
      Bind(
        ref cx,
        Props.path,
        Props.validators,
        Props.validationMode,
        Props.initialValue,
        Props.comparer,
        Props.enabled
      );
      var field = new FormFieldInput<TValue>(
        ReadValue(Props.initialValue),
        FieldEnabled,
        FieldHasError,
        _changed,
        _committed,
        _finished
      );
      var input = Props.input;
      if (Props.decorator.HasValue) {
        Props.decorator.Value.Decorate(
          ref cx,
          (ref Composition inner) => Props.composeInput?.Invoke(ref inner, in field, in input)
        );
      } else {
        Props.composeInput?.Invoke(ref cx, in field, in input);
      }
    }

    private void HandleChanged(TValue value, IBoundary boundary) {
      var callback = Props.onChanged;
      SetUserValue(value);
      if (Props.finishOnChange) FinishEditing();
      callback?.Invoke(value, boundary);
    }

    private void HandleCommitted(TValue value, IBoundary boundary) {
      SetUserValue(value);
      FinishEditing();
      Props.onCommitted?.Invoke(value, boundary);
    }

    private void HandleFinished(IBoundary boundary) => FinishEditing();
  }

  public static class FormFieldDefinition {
    public static ref ElementRef FormField<TValue>(
      this ref Composition cx,
      string path,
      TValue initialValue,
      FormInputComposable<TValue> composeInput,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      bool finishOnChange = false,
      Action<TValue, IBoundary> onChanged = null,
      Action<TValue, IBoundary> onCommitted = null,
      FormFieldOptions? decorator = null
    ) {
      var input = new FormInputSpec<TValue>(composeInput);
      cx.FormField(
        path,
        initialValue,
        static (
          ref Composition cx,
          in FormFieldInput<TValue> field,
          in FormInputSpec<TValue> input
        ) => input.compose?.Invoke(ref cx, in field),
        in input,
        validators,
        validationMode,
        comparer,
        enabled,
        finishOnChange,
        onChanged,
        onCommitted,
        decorator
      );
      return ref cx.APPLY;
    }

    public static ref ElementRef FormField<TValue, TInput>(
      this ref Composition cx,
      string path,
      TValue initialValue,
      FormInputComposable<TValue, TInput> composeInput,
      in TInput input,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      bool finishOnChange = false,
      Action<TValue, IBoundary> onChanged = null,
      Action<TValue, IBoundary> onCommitted = null,
      FormFieldOptions? decorator = null
    ) where TInput : struct {
      cx.AUTHORING.PropsBoundaryStateNode<
        FormFieldState<TValue, TInput>,
        FormFieldProps<TValue, TInput>
      >(
        FormFieldIdentity<TValue, TInput>.TypeId,
        out var node,
        out _,
        out var attachment
      );
      attachment.ReceiveProps(
        new FormFieldProps<TValue, TInput> {
          path = path,
          initialValue = initialValue,
          validators = validators,
          validationMode = validationMode,
          comparer = comparer,
          enabled = enabled,
          finishOnChange = finishOnChange,
          composeInput = composeInput,
          input = input,
          onChanged = onChanged,
          onCommitted = onCommitted,
          decorator = decorator
        }
      );
      node.composable = null;
      return ref cx.AUTHORING.YieldBoundary(ref cx, node);
    }
  }

  public readonly struct FormTextInputSpec {
    public readonly TextInputOptions? options;
    public readonly InputFieldStyle style;

    public FormTextInputSpec(TextInputOptions? options, InputFieldStyle style) {
      this.options = options;
      this.style = style;
    }
  }

  public readonly struct FormNumericInputSpec {
    public readonly NumericInputOptions? options;
    public readonly InputFieldStyle style;

    public FormNumericInputSpec(NumericInputOptions? options, InputFieldStyle style) {
      this.options = options;
      this.style = style;
    }
  }

  public readonly struct FormSliderSpec {
    public readonly SliderOptions? options;
    public readonly SliderStyle style;

    public FormSliderSpec(SliderOptions? options, SliderStyle style) {
      this.options = options;
      this.style = style;
    }
  }

  public readonly struct FormCheckboxSpec {
    public readonly CheckboxStyle style;

    public FormCheckboxSpec(CheckboxStyle style) {
      this.style = style;
    }
  }

  public static class FormInputDefinitions {
    private static readonly FormInputComposable<string, FormTextInputSpec> _textInput =
      ComposeTextInput;
    private static readonly FormInputComposable<int, FormNumericInputSpec> _intInput =
      ComposeIntInput;
    private static readonly FormInputComposable<float, FormNumericInputSpec> _floatInput =
      ComposeFloatInput;
    private static readonly FormInputComposable<float, FormSliderSpec> _slider =
      ComposeSlider;
    private static readonly FormInputComposable<bool, FormCheckboxSpec> _checkbox =
      ComposeCheckbox;


    private static readonly ushort _decoratorId = CompositionId.GetTypeId();

    public static ScopeHandle Decorator(this ref Composition cx, out DecoratorSlots slots) {
      if (!cx.AUTHORING.RequireComposable<CommonDecoratorElement>(_decoratorId, out var decorator, out var retained)) {
        decorator = new CommonDecoratorElement();
      }
      if (!retained) decorator.Initialize(cx);

      slots = new DecoratorSlots(decorator, cx);
      return cx.AUTHORING.YieldScope(ref cx, decorator);
    }

    public static ref ElementRef FormTextInput(
      this ref Composition cx,
      string path,
      string initialValue = "",
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      TextInputOptions? options = null,
      InputFieldStyle style = null,
      Action<string, IBoundary> onChanged = null,
      Action<string, IBoundary> onSubmitted = null,
      FormFieldOptions? decorator = null
    ) {
      var input = new FormTextInputSpec(options, style);
      cx.FormField(
        path,
        initialValue ?? string.Empty,
        _textInput,
        in input,
        validators,
        validationMode,
        comparer,
        enabled,
        onChanged: onChanged,
        onCommitted: onSubmitted,
        decorator: decorator
      );
      return ref cx.APPLY;
    }

    public static ref ElementRef FormIntInput(
      this ref Composition cx,
      string path,
      int initialValue = 0,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      NumericInputOptions? options = null,
      InputFieldStyle style = null,
      Action<int, IBoundary> onChanged = null,
      Action<int, IBoundary> onSubmitted = null,
      FormFieldOptions? decorator = null
    ) {
      var input = new FormNumericInputSpec(options, style);
      cx.FormField(
        path,
        initialValue,
        _intInput,
        in input,
        validators,
        validationMode,
        comparer,
        enabled,
        onChanged: onChanged,
        onCommitted: onSubmitted,
        decorator: decorator
      );
      return ref cx.APPLY;
    }

    public static ref ElementRef FormFloatInput(
      this ref Composition cx,
      string path,
      float initialValue = 0f,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      NumericInputOptions? options = null,
      InputFieldStyle style = null,
      Action<float, IBoundary> onChanged = null,
      Action<float, IBoundary> onSubmitted = null,
      FormFieldOptions? decorator = null
    ) {
      var input = new FormNumericInputSpec(options, style);
      cx.FormField(
        path,
        initialValue,
        _floatInput,
        in input,
        validators,
        validationMode,
        comparer,
        enabled,
        onChanged: onChanged,
        onCommitted: onSubmitted,
        decorator: decorator
      );
      return ref cx.APPLY;
    }

    public static ref ElementRef FormSlider(
      this ref Composition cx,
      string path,
      float initialValue = 0f,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      SliderOptions? options = null,
      SliderStyle style = null,
      Action<float, IBoundary> onChanged = null,
      Action<float, IBoundary> onCommitted = null,
      FormFieldOptions? decorator = null
    ) {
      var input = new FormSliderSpec(options, style);
      cx.FormField(
        path,
        initialValue,
        _slider,
        in input,
        validators,
        validationMode,
        comparer,
        enabled,
        onChanged: onChanged,
        onCommitted: onCommitted,
        decorator: decorator
      );
      return ref cx.APPLY;
    }

    public static ref ElementRef FormCheckbox(
      this ref Composition cx,
      string path,
      bool initialValue = false,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      CheckboxStyle style = null,
      Action<bool, IBoundary> onChanged = null,
      FormFieldOptions? decorator = null
    ) {
      var input = new FormCheckboxSpec(style);
      cx.FormField(
        path,
        initialValue,
        _checkbox,
        in input,
        validators,
        validationMode,
        comparer,
        enabled,
        true,
        onChanged,
        decorator: decorator
      );
      return ref cx.APPLY;
    }

    private static void ComposeTextInput(
      ref Composition cx,
      in FormFieldInput<string> field,
      in FormTextInputSpec input
    ) {
      cx.TextInput(
        field.value,
        field.changed,
        field.committed,
        onEditingEnded: field.finishedEditing,
        options: input.options,
        enabled: field.enabled,
        error: field.error,
        style: input.style
      ).Flexible();
    }

    private static void ComposeIntInput(
      ref Composition cx,
      in FormFieldInput<int> field,
      in FormNumericInputSpec input
    ) {
      cx.IntInput(
        field.value,
        field.changed,
        field.committed,
        onEditingEnded: field.finishedEditing,
        options: input.options,
        enabled: field.enabled,
        error: field.error,
        style: input.style
      ).Flexible();
    }

    private static void ComposeFloatInput(
      ref Composition cx,
      in FormFieldInput<float> field,
      in FormNumericInputSpec input
    ) {
      cx.FloatInput(
        field.value,
        field.changed,
        field.committed,
        onEditingEnded: field.finishedEditing,
        options: input.options,
        enabled: field.enabled,
        error: field.error,
        style: input.style
      ).Flexible();
    }

    private static void ComposeSlider(
      ref Composition cx,
      in FormFieldInput<float> field,
      in FormSliderSpec input
    ) {
      cx.Slider(
        field.value,
        field.changed,
        field.committed,
        options: input.options,
        enabled: field.enabled,
        error: field.error,
        style: input.style
      ).Flexible();
    }

    private static void ComposeCheckbox(
      ref Composition cx,
      in FormFieldInput<bool> field,
      in FormCheckboxSpec input
    ) {
      cx.Checkbox(
        field.value,
        null,
        field.changed,
        field.enabled,
        input.style,
        field.error
      ).Flexible();
    }
  }

  public readonly struct FormDropdownSpec<T> {
    public readonly IReadOnlyList<DropdownOption<T>> options;
    public readonly string placeholder;
    public readonly ControlBoxStyle? style;
    public readonly OverlayPanelStyle menuStyle;
    public readonly OverlayOptions? overlayOptions;

    public FormDropdownSpec(
      IReadOnlyList<DropdownOption<T>> options,
      string placeholder,
      ControlBoxStyle? style,
      OverlayPanelStyle menuStyle,
      OverlayOptions? overlayOptions
    ) {
      this.options = options;
      this.placeholder = placeholder;
      this.style = style;
      this.menuStyle = menuStyle;
      this.overlayOptions = overlayOptions;
    }
  }

  internal static class FormDropdownInput<T> {
    public static readonly FormInputComposable<T, FormDropdownSpec<T>> Compose = ComposeInput;

    private static void ComposeInput(
      ref Composition cx,
      in FormFieldInput<T> field,
      in FormDropdownSpec<T> input
    ) {
      cx.Dropdown(
        field.value,
        input.options,
        field.changed,
        input.placeholder,
        field.enabled,
        field.error,
        input.style,
        input.menuStyle,
        input.overlayOptions
      );
    }
  }

  public static class FormDropdownDefinition {
    public static ref ElementRef FormDropdown<T>(
      this ref Composition cx,
      string path,
      T initialValue,
      IReadOnlyList<DropdownOption<T>> options,
      IReadOnlyList<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      string placeholder = null,
      ControlBoxStyle? style = null,
      OverlayPanelStyle menuStyle = null,
      OverlayOptions? overlayOptions = null,
      Action<T, IBoundary> onChanged = null,
      FormFieldOptions? decorator = null
    ) {
      var input = new FormDropdownSpec<T>(
        options,
        placeholder,
        style,
        menuStyle,
        overlayOptions
      );
      cx.FormField(
        path,
        initialValue,
        FormDropdownInput<T>.Compose,
        in input,
        validators,
        validationMode,
        comparer,
        enabled,
        true,
        onChanged,
        decorator: decorator
      );
      return ref cx.APPLY;
    }
  }

  public static partial class FormListDefinition {
    [CompositionBoundary(Base = typeof(FormFieldStateBase<>))]
    public static partial ref ElementRef FormList(
      ref this Composition cx,
      [Prop] string path,
      [Prop] int initialCount,
      [Prop] Composable<int> item,
      [Prop] IReadOnlyList<IFormValidator> validators = null,
      [Prop] ValidationMode validationMode = ValidationMode.OnSubmit,
      [Prop] bool enabled = true,
      [Prop] float? gap = null
    );

    public partial class FormListState {
      private readonly List<string> _itemPrefixes = new(4);
      private string _localPath;

      protected override void OnRecompose(ref Composition cx) {
        Bind<object>(
          ref cx,
          Props.Path,
          Props.Validators,
          Props.ValidationMode,
          null,
          null,
          Props.Enabled,
          list: true,
          initialListCount: Props.InitialCount
        );

        if (!string.Equals(_localPath, Props.Path, StringComparison.Ordinal)) {
          _localPath = Props.Path;
          _itemPrefixes.Clear();
        }

        var count = Form.GetListCount(Path);
        var gap = Props.Gap ?? ThemeData.Context.ReadScope()[SpacingRole.Spacing2];
        while (_itemPrefixes.Count < count) {
          var index = _itemPrefixes.Count;
          _itemPrefixes.Add(
            FormPath.Compose(
              Props.Path,
              index.ToString(CultureInfo.InvariantCulture)
            )
          );
        }
        for (var i = 0; i < count; i++) {
          if (i > 0) cx.Gap(gap);
          cx.FormIndexedScope(_itemPrefixes[i], i, Props.Item);
        }
      }
    }
  }

  public static partial class FormIndexedScopeDefinition {
    [CompositionBoundary]
    public static partial ref ElementRef FormIndexedScope(
      ref this Composition cx,
      [Prop] string prefix,
      [Prop] int index,
      [Prop] Composable<int> content
    );

    public partial class FormIndexedScopeState {
      private FormController _controller;
      private string _parentPrefix;
      private string _localPrefix;
      private string _resolvedPrefix;

      protected override void OnRecompose(ref Composition cx) {
        var parent = cx.ReadContext(FormContextData.Key);
        if (parent.controller == null) {
          throw new InvalidOperationException("FormIndexedScope must be composed below Form.");
        }

        if (!ReferenceEquals(_controller, parent.controller) ||
            !string.Equals(_parentPrefix, parent.prefix, StringComparison.Ordinal) ||
            !string.Equals(_localPrefix, Props.Prefix, StringComparison.Ordinal)) {
          _controller = parent.controller;
          _parentPrefix = parent.prefix;
          _localPrefix = Props.Prefix;
          _resolvedPrefix = FormPath.Compose(parent.prefix, Props.Prefix);
        }

        var context = new FormContextData(_controller, _resolvedPrefix);
        using (FormContextWriter.WriteStable(ref cx, in context)) {
          Props.Content?.Invoke(ref cx, Props.Index);
        }
      }
    }
  }

  public static partial class FormFeedbackDefinition {
    private static readonly Composable<FieldData> _firstErrorText = ComposeFirstError;

    [CompositionBoundary]
    public static partial ref ElementRef FormFeedback(
      ref this Composition cx,
      [Prop] string path,
      [Prop] Composable<FieldData> content,
      [Prop] bool onlyWhenError = true
    );

    public static ref ElementRef FormErrorText(
      this ref Composition cx,
      string path
    ) {
      return ref cx.FormFeedback(path, _firstErrorText);
    }

    private static void ComposeFirstError(ref Composition cx, FieldData data) {
      if (data?.errors == null || data.errors.Count == 0) return;
      cx.Text(data.errors[0]);
      var theme = cx.ReadContextOrDefault(ThemeData.Context, BuiltinThemes.DefaultDark);
      cx.APPLY.TextColor(theme.GetColor(ColorRoles.Error));
    }

    public partial class FormFeedbackState : IFormField {
      private FormController _form;
      private string _path;

      protected override void OnRecompose(ref Composition cx) {
        var context = cx.ReadContext(FormContextData.Key);
        if (context.controller == null) {
          throw new InvalidOperationException("FormFeedback must be composed below Form.");
        }
        var path = FormPath.Require(context.Resolve(Props.Path));
        if (!ReferenceEquals(_form, context.controller) ||
            !string.Equals(_path, path, StringComparison.Ordinal)) {
          _form?.UnobserveField(_path, this);
          _form = context.controller;
          _path = path;
          _form.ObserveField(_path, this);
        }

        var data = _form.GetFieldData(_path);
        if (data == null || (Props.OnlyWhenError && !data.HasFlag(FieldFlags.Error))) return;
        Props.Content?.Invoke(ref cx, data);
      }

      public void OnFormFieldChanged(FormController form, string path) {
        if (ReferenceEquals(form, _form) &&
            string.Equals(path, _path, StringComparison.Ordinal)) {
          if (Node != null) Node.MarkDirty();
        }
      }

      protected override void OnDetach() {
        _form?.UnobserveField(_path, this);
        _form = null;
        _path = null;
        base.OnDetach();
      }
    }
  }
}