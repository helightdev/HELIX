using System;
using System.Collections.Generic;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;

namespace HELIX.Widgets.Forms {
  public sealed class HFormTextField : StatefulWidget<HFormTextField> {
    public readonly string path;
    public readonly IEnumerable<IFormValidator> validators;
    public readonly ValidationMode validationMode;
    public readonly string initialValue;
    public readonly IEqualityComparer<object> comparer;
    public readonly Key focusKey;
    public readonly HTextFieldStyle style;
    public readonly bool multiline;
    public readonly bool autocorrect;
    public readonly bool isReadOnly;
    public readonly bool isPasswordField;
    public readonly bool isDelayed;
    public readonly bool hideMobileInput;
    public readonly TouchScreenKeyboardType keyboardType;
    public readonly char maskChar;
    public readonly int maxLength;
    public readonly bool enabled;
    public readonly Action<string> onChanged;
    public readonly Action<string> onSubmitted;

    public HFormTextField(
      string path,
      IEnumerable<IFormValidator> validators = null,
      ValidationMode validationMode = ValidationMode.OnSubmit,
      string initialValue = "",
      IEqualityComparer<object> comparer = null,
      Key focusKey = default,
      HTextFieldStyle style = null,
      bool multiline = false,
      bool autocorrect = true,
      bool isReadOnly = false,
      bool isPasswordField = false,
      bool isDelayed = false,
      bool hideMobileInput = true,
      TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default,
      char maskChar = '*',
      int maxLength = -1,
      bool enabled = true,
      Action<string> onChanged = null,
      Action<string> onSubmitted = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.path = path;
      this.validators = validators;
      this.validationMode = validationMode;
      this.initialValue = initialValue;
      this.comparer = comparer;
      this.focusKey = focusKey;
      this.style = style;
      this.multiline = multiline;
      this.autocorrect = autocorrect;
      this.isReadOnly = isReadOnly;
      this.isPasswordField = isPasswordField;
      this.isDelayed = isDelayed;
      this.hideMobileInput = hideMobileInput;
      this.keyboardType = keyboardType;
      this.maskChar = maskChar;
      this.maxLength = maxLength;
      this.enabled = enabled;
      this.onChanged = onChanged;
      this.onSubmitted = onSubmitted;
    }

    public override State<HFormTextField> CreateState() {
      return new HFormTextFieldState();
    }
  }

  public sealed class HFormTextFieldState : State<HFormTextField>, IFormField {
    private TextEditingController _controller;
    private WidgetStateController _widgetState;
    private FormController _form;
    private string _path;
    private bool _syncingFromForm;
    private bool _disposed;

    public override void InitState() {
      base.InitState();
      _widgetState = AddDisposable(new WidgetStateController());
      _controller = AddDisposable(new TextEditingController(_widgetState));
      _controller.onChanged += OnChanged;
      _controller.onEndEditing += OnFinishedEditing;
      _controller.onSubmitted += OnSubmitted;
      ResolveAndRegister();
    }

    public override void DidUpdateWidget(HFormTextField oldWidget) {
      ResolveAndRegister();
    }

    public override bool CanReconcile(HFormTextField oldWidget) {
      return base.CanReconcile(oldWidget) && widget.path == oldWidget.path;
    }

    public override void Dispose() {
      _disposed = true;
      if (_controller != null) {
        _controller.onChanged -= OnChanged;
        _controller.onEndEditing -= OnFinishedEditing;
        _controller.onSubmitted -= OnSubmitted;
      }

      _form?.UnregisterField(_path, this, false);
      _form = null;
      _path = null;
      base.Dispose();
    }

    public void OnFormFieldChanged(FormController form, string path) {
      if (_disposed || _controller == null || !ReferenceEquals(form, _form) || path != _path) return;
      SyncWidgetState();
      var value = form.GetValue<string>(path, widget.initialValue ?? string.Empty) ?? string.Empty;
      if (string.Equals(_controller.PeekValue(), value, StringComparison.InvariantCulture)) return;

      _syncingFromForm = true;
      try {
        _controller.SetValue(value);
      } finally {
        _syncingFromForm = false;
      }
    }

    public override Widget Build(BuildContext context) {
      ResolveAndRegister();
      return new HTextField(
        controller: _controller,
        focusKey: widget.focusKey,
        style: widget.style,
        multiline: widget.multiline,
        autocorrect: widget.autocorrect,
        isReadOnly: widget.isReadOnly,
        isPasswordField: widget.isPasswordField,
        isDelayed: widget.isDelayed,
        hideMobileInput: widget.hideMobileInput,
        keyboardType: widget.keyboardType,
        maskChar: widget.maskChar,
        maxLength: widget.maxLength,
        enabled: IsTextInputEnabled()
      );
    }

    private void ResolveAndRegister() {
      if (_disposed) return;
      var formContext = FormContext.Require(mount, nameof(HFormTextField));
      var fullPath = formContext.ResolvePath(widget.path);
      if (ReferenceEquals(_form, formContext.Controller) && _path == fullPath) {
        _form.RegisterField(
          _path,
          this,
          widget.validators,
          widget.validationMode,
          widget.initialValue,
          widget.comparer,
          widget.enabled
        );
        SyncWidgetState();
        return;
      }

      _form?.UnregisterField(_path, this);
      _form = formContext.Controller;
      _path = fullPath;
      _form.RegisterField(
        _path,
        this,
        widget.validators,
        widget.validationMode,
        widget.initialValue,
        widget.comparer,
        widget.enabled
      );
      SyncWidgetState();
    }

    private void SyncWidgetState() {
      if (_widgetState == null) return;
      var fieldData = _form?.GetFieldData(_path);
      var disabled = !widget.enabled || fieldData?.HasFlag(FieldFlags.Disabled) == true;
      var error = fieldData?.HasFlag(FieldFlags.Error) == true;
      _widgetState.Toggle(WidgetState.Disabled, disabled);
      _widgetState.Toggle(WidgetState.Error, error);
    }

    private bool IsTextInputEnabled() {
      if (!widget.enabled) return false;
      return _form?.GetFieldData(_path)?.HasFlag(FieldFlags.Disabled) != true;
    }

    private void OnChanged(string value) {
      if (_disposed || _syncingFromForm || _form == null || _path == null) return;
      _form.SetValue(_path, value, FormChangeReason.User);
      widget.onChanged?.Invoke(value);
    }

    private void OnSubmitted(string value) {
      if (_disposed) return;
      _form?.MarkFinishedEditing(_path);
      widget.onSubmitted?.Invoke(value);
    }

    private void OnFinishedEditing() {
      if (_disposed) return;
      _form?.MarkFinishedEditing(_path);
    }
  }
}
