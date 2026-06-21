using System;
using System.Collections.Generic;
using HELIX.Widgets.Forms;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;

namespace HELIX.Widgets.Universal.Forms {
  public sealed class HFormTextField : FormFieldWidget<HFormTextField, string> {
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
    public readonly Action<string> onChanged;
    public readonly Action<string> onSubmitted;

    public HFormTextField(
      string path,
      Key focusKey = default, // Text field settings //
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
      Action<string> onChanged = null,
      Action<string> onSubmitted = null,
      IEnumerable<IFormValidator> validators = null, // Form settings //
      ValidationMode validationMode = ValidationMode.OnChange,
      string initialValue = "",
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      FormController controller = null,
      Key key = default, // Widget settings //
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(path, validators, validationMode, initialValue, comparer, enabled, controller, key, modifiers) {
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
      this.onChanged = onChanged;
      this.onSubmitted = onSubmitted;
    }

    public override State<HFormTextField> CreateState() {
      return new HFormTextFieldState();
    }
  }

  public sealed class HFormTextFieldState : FormFieldState<HFormTextField, string> {
    private TextEditingController _controller;

    public override void InitState() {
      base.InitState();
      widgetState = AddDisposable(new WidgetStateController());
      _controller = AddDisposable(new TextEditingController(widgetState));
      _controller.onChanged += OnChanged;
      _controller.onEndEditing += OnFinishedEditing;
      _controller.onSubmitted += OnSubmitted;
    }

    protected override void OnFormFieldChanged(string value, bool hasValue) {
      _controller.SetValue(value ?? string.Empty);
    }

    public override Widget Build(BuildContext context) {
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
        maxLength: widget.maxLength
      );
    }

    private void OnChanged(string value) {
      if (!mount.IsMounted) return;
      NotifyUserValueChanged(value);
    }

    private void OnSubmitted(string value) {
      if (!mount.IsMounted) return;
      form?.MarkFinishedEditing(path);
      // widget.onSubmitted?.Invoke(value);
    }

    private void OnFinishedEditing() {
      if (!mount.IsMounted) return;
      form.MarkFinishedEditing(path);
    }
  }
}