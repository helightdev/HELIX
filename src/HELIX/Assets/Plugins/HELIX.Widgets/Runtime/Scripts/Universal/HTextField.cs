using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HTextField : StatefulWidget<HTextField> {
        public readonly TextEditingController controller;
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
        public readonly string initialValue;
        public readonly Action<string> onChanged;
        public readonly Action<string> onSubmitted;

        public HTextField(
            TextEditingController controller = null,
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
            string initialValue = "",
            Action<string> onChanged = null,
            Action<string> onSubmitted = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
            this.controller = controller;
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
            this.initialValue = initialValue;
            this.onChanged = onChanged;
            this.onSubmitted = onSubmitted;
        }

        public override State<HTextField> CreateState() {
            return new HTextFieldState();
        }
    }

    public class HTextFieldState : State<HTextField> {
        private GenericTextInput _input;
        private TextEditingController _controller;
        private WidgetStateController _widgetStateController;
        private IDisposable _controllerSubscription;

        public override void InitState() {
            base.InitState();
            _input = new GenericTextInput();
            _input.OnValueChanged += OnTextFieldChanged;
            _input.OnBeginEditing += OnBeginEditing;
            _input.OnEndEditing += OnEndEditing;
            _input.OnSubmit += OnSubmit;
            _input.OnCancel += OnCancel;
            _input.OnBlur += UpdateFocus;
            _input.OnFocus += UpdateFocus;
            if (widget.controller == null) {
                _widgetStateController = AddDisposable(new WidgetStateController());
                _controller = AddDisposable(new TextEditingController(_widgetStateController));
                _controller.SetValue(widget.initialValue ?? "");

                if (widget.onChanged != null) { _controller.onChanged += widget.onChanged; }

                if (widget.onSubmitted != null) { _controller.onSubmitted += widget.onSubmitted; }
            } else {
                _widgetStateController = widget.controller.widgetState;
                _controller = widget.controller;
            }

            _widgetStateController ??= AddDisposable(new WidgetStateController());
            AddDisposable(_controller.AddObserver(OnTextChanged));
            _input.Value = _controller.Value;

            DidUpdateWidget(null);
        }

        public override void DidUpdateWidget(HTextField oldWidget) {
            if (widget.controller == null) { _widgetStateController?.Toggle(WidgetState.Disabled, !widget.enabled); }

            _input.Multiline = widget.multiline;
            _input.IsReadOnly = widget.isReadOnly;
            _input.IsPasswordField = widget.isPasswordField;
            _input.IsDelayed = widget.isDelayed;
            _input.AutoCorrection = widget.autocorrect;
            _input.HideMobileInput = widget.hideMobileInput;
            _input.KeyboardType = widget.keyboardType;
            _input.MaskChar = widget.maskChar;
            _input.MaxLength = widget.maxLength;
        }

        private void OnCancel() {
            _controller.onCanceled?.Invoke();
            UpdateFocus();
        }

        public override bool CanReconcile(HTextField oldWidget) {
            return base.CanReconcile(oldWidget) && widget.controller == oldWidget.controller;
        }

        private void OnSubmit(string obj) {
            _controller.onSubmitted?.Invoke(obj);
        }

        private void OnEndEditing() {
            _controller.onEndEditing?.Invoke();
            _widgetStateController?.Disable(WidgetState.Pressed);
        }

        private void OnBeginEditing() {
            _controller.onBeginEditing?.Invoke();
            _widgetStateController?.Enable(WidgetState.Navigated | WidgetState.Pressed);
        }

        private void UpdateFocus() {
            var isFocused = CheckFocus();
            _widgetStateController?.Toggle(WidgetState.Focused, isFocused);
        }

        private bool CheckFocus() {
            var focused = mount.Element.focusController.focusedElement;
            return focused is VisualElement ve && mount.Element.Contains(ve);
        }

        private void OnTextFieldChanged(string obj) {
            if (!string.Equals(obj, _controller.Value, StringComparison.InvariantCulture)) {
                _controller.SetValue(obj);
            }
        }

        private void OnTextChanged(string obj) {
            if (!string.Equals(obj, _input.Value, StringComparison.InvariantCulture)) { _input.Value = obj; }
        }

        public override Widget Build(BuildContext context) {
            var colors = context.GetThemed(PrimitiveBaseTheme.Colors);
            var effective = widget.style ?? context.GetThemed(PrimitiveTheme.TextField);

            var modifiers = WidgetStateProperties.Modifiers(
                effective.modifiers,
                WidgetStateProperties.Func(state => {
                        var set = new ModifierSet {
                            new SizeModifier(effective.constraints.ResolveOrDefault(state, BoxConstraints.Initial)),
                            new TextStyleModifier(effective.textStyle.ResolveOrDefault(state)),
                            new PaddingModifier(effective.padding.ResolveOrDefault(state, StyleLength4.Zero)),
                            FocusModifier.FocusableDelegates,
                            new WidgetStateModifier(_widgetStateController, false)
                        };
                        if (_widgetStateController != null) set.Add(new WidgetStateModifier(_widgetStateController));
                        return set;
                    }
                )
            );

            return new HSubstanceBox(
                controller: _widgetStateController,
                boxKey: widget.focusKey,
                boxModifiers: modifiers,
                alignment: effective.alignment,
                substances: effective.layers,
                builder: (_, state) => new FactoryWidget<GenericTextInput> {
                    creator = () => _input,
                    updater = input => {
                        input.TextInputStyle = effective.inputStyle.ResolveOrDefault(
                            state,
                            colors.brightness == Brightness.Light
                                ? GenericTextInputStyle.LightNeutral
                                : GenericTextInputStyle.DarkNeutral
                        );
                        input.CursorColor = effective.cursorColor.ResolveOrDefault(state, colors.surface.onMain);
                        input.SelectionColor = effective.selectionColor.ResolveOrDefault(state, colors.surface.onMain);
                    }
                }.Fill()
            );
        }
    }
}