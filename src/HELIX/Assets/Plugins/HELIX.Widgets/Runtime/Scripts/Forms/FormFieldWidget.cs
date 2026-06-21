using System;
using System.Collections.Generic;
using HELIX.Widgets.Universal.Controllers;

namespace HELIX.Widgets.Forms {
  public abstract class FormFieldWidget<T, V> : StatefulWidget<T> where T : FormFieldWidget<T, V> {
    public readonly string path;
    public readonly IEnumerable<IFormValidator> validators;
    public readonly ValidationMode validationMode;
    public readonly V initialValue;
    public readonly bool enabled;

    public readonly IEqualityComparer<object> comparer;
    public readonly FormController controller;

    protected FormFieldWidget(
      string path,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnChange,
      V initialValue = default,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      FormController controller = null,
      Key key = default,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, modifiers) {
      this.path = path;
      this.validators = validators;
      this.validationMode = validationMode;
      this.initialValue = initialValue;
      this.comparer = comparer;
      this.enabled = enabled;
      this.controller = controller;
    }

    protected FormFieldWidget(
      FieldSpec<V> hSpec,
      IEqualityComparer<object> comparer = null,
      FormController controller = null,
      Key key = default,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, modifiers) {
      this.comparer = comparer;
      this.controller = controller;
      initialValue = hSpec.DefaultValue;
      path = hSpec.Path;
      validators = hSpec.Validators;
      validationMode = hSpec.ValidationMode;
      enabled = hSpec.Enabled;
    }
  }

  public abstract class FormFieldState<T, V> : State<T>, IFormField where T : FormFieldWidget<T, V> {
    protected FormController form;
    protected string path;
    protected WidgetStateController widgetState;
    protected bool isFormSyncing;

    protected FieldData GetFieldData() {
      return form?.GetFieldData(path);
    }

    public override bool CanReconcile(T oldWidget) {
      return base.CanReconcile(oldWidget) && oldWidget.controller == widget.controller;
    }

    public override void Configure(ConfigureContext context) {
      var formContext = FormContext.Resolve(mount);
      form = widget.controller ?? formContext?.controller;
      RefreshPath(formContext);
      if (form == null)
        throw new InvalidOperationException(
          $"{typeof(T).Name} must be built below an HForm or have a controller provided."
        );
      if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path cannot be empty", nameof(widget.path));
      RegisterField();
      SyncWidgetState();
    }

    private void RefreshPath(FormContext context) {
      var previousPath = path;
      path = context?.ResolvePath(widget.path) ?? widget.path;
      if (path != previousPath && previousPath != null) UnregisterField();
    }

    protected virtual void RegisterField() {
      form.RegisterField(
        path,
        this,
        widget.validators,
        widget.validationMode,
        widget.initialValue,
        widget.comparer,
        widget.enabled
      );
    }

    protected virtual void UnregisterField() {
      form?.UnregisterField(path, this);
    }

    protected virtual void OnFormFieldChanged(V value, bool hasValue) { }

    protected void NotifyUserValueChanged(V value) {
      if (isFormSyncing || !mount.IsMounted) return;
      form.SetValue(path, value, FormChangeReason.User);
    }

    private void SyncWidgetState() {
      if (widgetState == null) return;
      var fieldData = GetFieldData();
      var disabled = !widget.enabled || fieldData?.HasFlag(FieldFlags.Disabled) == true;
      var error = fieldData?.HasFlag(FieldFlags.Error) == true;
      // widgetState.Toggle(WidgetState.Disabled, disabled);
      // widgetState.Toggle(WidgetState.Error, error);
    }

    public void OnFormFieldChanged(FormController formArg, string pathArg) {
      if (!mount.IsMounted || form == null || !ReferenceEquals(formArg, form) || pathArg != path) return;
      var value = form.GetValue(path, widget.initialValue);
      isFormSyncing = true;
      try {
        OnFormFieldChanged(value, form.HasValue(path));
      } finally {
        isFormSyncing = false;
      }

      SyncWidgetState();
    }
  }
}