using System.Collections.Generic;
using HELIX.Compose.Forms;

namespace HELIX.Compose {
  /// <summary>Shared visual and decorator configuration for form fields.</summary>
  public readonly struct HXFormFieldStyle {
    public static readonly HXFormFieldStyle Default = new();

    public readonly DecoratorLayout layout;
    public readonly DecoratorArrangement arrangement;
    public readonly HXDecoratorStyle? decoratorStyle;
    public readonly FormFieldDecorators decorators;

    public HXFormFieldStyle(
      DecoratorLayout layout = null,
      DecoratorArrangement arrangement = DecoratorArrangement.Stacked,
      HXDecoratorStyle? decoratorStyle = null,
      FormFieldDecorators decorators = default
    ) {
      this.layout = layout;
      this.arrangement = arrangement;
      this.decoratorStyle = decoratorStyle;
      this.decorators = decorators;
    }

    public void Compose(ref Composition cx, Composable field) {
      var selectedLayout = layout ?? HXDecorator.Layout[in cx];
      if (selectedLayout == DecoratorExtensions.ComposeDefaultDecorator)
        DecoratorExtensions.ComposeDefaultDecorator(
          ref cx,
          field,
          in decorators,
          arrangement,
          decoratorStyle
        );
      else selectedLayout(ref cx, field, in decorators);
    }
  }

  /// <summary>
  /// Retained registration and decoration boundary shared by Compose form controls. A control composed inside
  /// <see cref="Props.content"/> can use <c>cx.Lookup&lt;HXFormField&gt;()</c> to read or update its value.
  /// </summary>
  [EnableMixins]
  [MixinUsing("HELIX.Compose.Forms")]
  [BoundaryElementMixin(name: "FormField", extension: true, cacheLookups: true)]
  public partial class HXFormField : IFormField {
    public partial struct Props {
      public string path;
      public Composable content;
      [Prop(null)] public HXFormFieldStyle? style;
      [Prop(null, Equatable = false)] public IReadOnlyList<IFormValidator> validators;
      [Prop(ValidationMode.OnChange)] public ValidationMode validationMode;
      [Prop("FormController.NoInitialValue", PropInit.Deferred, Equatable = false)]
      public object initialValue;
      [Prop(null, Equatable = false)] public IEqualityComparer<object> comparer;
      [Prop(true)] public bool enabled;
      [Prop(null, Equatable = false)] public object metadata;
    }

    public static readonly ContextKey<HXFormFieldStyle> Style = new("FormFieldStyle", HXFormFieldStyle.Default);

    public FormController Controller { get; private set; }
    public FormPath Path { get; private set; }
    public FieldData FieldData => Controller?.GetFieldData(Path);
    public bool HasValue => Controller?.FieldHasValue(Path) == true;
    public object Value => Controller?.GetValue(Path);
    public T Metadata<T>() => props.metadata is T value ? value : default;

    private bool _registered;
    private IReadOnlyList<IFormValidator> _validators;
    private IEqualityComparer<object> _comparer;
    private ValidationMode _validationMode;
    private object _initialValue;
    private bool _enabled;

    public T GetValue<T>(T fallback = default) => Controller == null ? fallback : Controller.GetValue(Path, fallback);

    public void SetUserValue<T>(T value) {
      if (!_registered) throw new System.InvalidOperationException("Form field is not attached.");
      Controller.SetValue(Path, value, FormChangeReason.User);
    }

    public void MarkFinishedEditing() {
      if (_registered) Controller.MarkFinishedEditing(Path);
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      var context = cx.RequireForm();
      var path = context.Resolve(props.path);
      if (RegistrationChanged(context.controller, path)) Register(context.controller, path);

      var style = props.style ?? Style[in cx];
      style.Compose(ref cx, props.content);
    }

    private bool RegistrationChanged(FormController controller, FormPath path) => !_registered ||
      !ReferenceEquals(Controller, controller) || Path != path ||
      !ReferenceEquals(_validators, props.validators) || _validationMode != props.validationMode ||
      !ReferenceEquals(_initialValue, props.initialValue) || !ReferenceEquals(_comparer, props.comparer) ||
      _enabled != props.enabled;

    private void Register(FormController controller, FormPath path) {
      if (_registered) Controller.UnregisterField(Path, this, false);
      Controller = controller;
      Path = path;
      _validators = props.validators;
      _validationMode = props.validationMode;
      _initialValue = props.initialValue;
      _comparer = props.comparer;
      _enabled = props.enabled;
      _registered = true;
      Controller.RegisterField(
        Path,
        this,
        _validators,
        _validationMode,
        _initialValue,
        _comparer,
        _enabled
      );
    }

    public void OnFormFieldChanged(FormController form, FormPath path) {
      if (_registered && ReferenceEquals(form, Controller) && path == Path) MarkDirty();
    }

    [Hook]
    private void OnDispose() {
      if (_registered) Controller.UnregisterField(Path, this);
      _registered = false;
      Controller = null;
      Path = default;
    }
  }
}
