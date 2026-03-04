using System;
using HELIX.Extensions;
using HELIX.Widgets.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Input {
    [UxmlElement]
    public partial class GenericTextInput : BaseWidget {
        private const string _ussStyleLight = "helix-textfield-style-light";
        private const string _ussStyleLightNeutral = "helix-textfield-style-light-neutral";
        private const string _ussStyleDark = "helix-textfield-style-dark";
        private const string _ussStyleDarkNeutral = "helix-textfield-style-dark-neutral";
        private const string _ussStyleClear = "helix-textfield-style-clear";

        private readonly TextField _textField;

        private GenericTextInputStyle _textInputStyle = GenericTextInputStyle.Light;
        private Color _selectionColor = Color.red;
        private Color _cursorColor = Color.white;
        private bool _hadCustomColor;
        private bool _expands = true;

        public TextField BackingTextField => _textField;

        [UxmlAttribute,
         Tooltip("The visual style of the text input, which determines the selection and cursor colors.")]
        public GenericTextInputStyle TextInputStyle {
            get => _textInputStyle;
            set {
                _textInputStyle = value;
                ApplySelectionColors();
            }
        }

        [UxmlAttribute, Tooltip("Color of the text selection highlight. Only applies if TextInputStyle is set to Custom.")]
        public Color SelectionColor {
            get => _selectionColor;
            set {
                _selectionColor = value;
                ApplySelectionColors();
            }
        }

        [UxmlAttribute, Tooltip("Color of the text field's cursor. Only applies if TextInputStyle is set to Custom.")]
        public Color CursorColor {
            get => _cursorColor;
            set {
                _cursorColor = value;
                ApplySelectionColors();
            }
        }

        [UxmlAttribute,
         Tooltip("Whether the text field should stretch to fill available space. If false, the text field will size to its content.")]
        public bool Expands {
            get => _expands;
            set {
                _expands = value;
                if (value) _textField.Stretched();
                else _textField.Loosen();
            }
        }

        public event Action OnBeginEditing;
        public event Action OnEndEditing;
        public event Action<string> OnValueChanged;
        public event Action<string> OnSubmit;
        public event Action OnCancel;
        public event Action OnFocus;
        public event Action OnBlur;

        public GenericTextInput() {
            this.WithStylesheet(AuxiliaryStylesheets.Helix).AddClasses("helix-generic-text-input");
            _textField = new TextField().WithName("BackingTextField").Stretched().AddTo(this);
            ApplySelectionColors();
            _textField.RegisterCallback<CustomStyleResolvedEvent>(_ => { ApplySelectionColors(); });
            _textField.RegisterValueChangedCallback(evt => { OnValueChanged?.Invoke(evt.newValue); });
            _textField.RegisterCallback<FocusEvent>(_ => { OnFocus?.Invoke(); });
            _textField.RegisterCallback<BlurEvent>(_ => { OnBlur?.Invoke(); });
            
            var element = _textField.textEdition as VisualElement ?? _textField;
            element.RegisterCallback<FocusEvent>(_ => { OnBeginEditing?.Invoke(); });
            element.RegisterCallback<BlurEvent>(_ => { OnEndEditing?.Invoke(); });
            element.RegisterCallback<NavigationSubmitEvent>(_ => { OnSubmit?.Invoke(Value); });
            element.RegisterCallback<NavigationCancelEvent>(_ => { OnCancel?.Invoke(); });
        }

        private void ApplySelectionColors() {
            if (_hadCustomColor && _textInputStyle != GenericTextInputStyle.Custom) {
                _textField.customStyle.TryGetValue(new CustomStyleProperty<Color>("--unity-cursor-color"), out var cursorColor);
                _textField.customStyle.TryGetValue(new CustomStyleProperty<Color>("--unity-selection-color"), out var selectionColor);
#pragma warning disable CS0618 // Type or member is obsolete
                _textField.textSelection.selectionColor = selectionColor;
                _textField.textSelection.cursorColor = cursorColor;
#pragma warning restore CS0618 // Type or member is obsolete    
                _hadCustomColor = false;
            }

            switch (_textInputStyle) {
                case GenericTextInputStyle.Light:        _textField.WithClasses(_ussStyleLight); break;
                case GenericTextInputStyle.Dark:         _textField.WithClasses(_ussStyleDark); break;
                case GenericTextInputStyle.LightNeutral: _textField.WithClasses(_ussStyleLightNeutral); break;
                case GenericTextInputStyle.DarkNeutral:  _textField.WithClasses(_ussStyleDarkNeutral); break;
                case GenericTextInputStyle.Custom:
                    _textField.WithClasses(_ussStyleClear);
#pragma warning disable CS0618 // Type or member is obsolete
                    _textField.textSelection.selectionColor = _selectionColor;
                    _textField.textSelection.cursorColor = _cursorColor;
#pragma warning restore CS0618 // Type or member is obsolete        
                    _hadCustomColor = true;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        [Header("Delegated Properties"), UxmlAttribute]
        public string Value {
            get => _textField.value;
            set => _textField.value = value;
        }
        
        [UxmlAttribute]
        public bool Multiline {
            get => _textField.multiline;
            set => _textField.multiline = value;
        }

        [UxmlAttribute]
        public bool IsReadOnly {
            get => _textField.isReadOnly;
            set => _textField.isReadOnly = value;
        }

        [UxmlAttribute]
        public int MaxLength {
            get => _textField.maxLength;
            set => _textField.maxLength = value;
        }

        [UxmlAttribute]
        public bool IsPasswordField {
            get => _textField.isPasswordField;
            set => _textField.isPasswordField = value;
        }

        [UxmlAttribute]
        public char MaskChar {
            get => _textField.maskChar;
            set => _textField.maskChar = value;
        }

        [UxmlAttribute]
        public bool AutoCorrection {
            get => _textField.autoCorrection;
            set => _textField.autoCorrection = value;
        }

        [UxmlAttribute]
        public bool HideMobileInput {
            get => _textField.hideMobileInput;
            set => _textField.hideMobileInput = value;
        }

        [UxmlAttribute]
        public TouchScreenKeyboardType KeyboardType {
            get => _textField.keyboardType;
            set => _textField.keyboardType = value;
        }

        [UxmlAttribute]
        public bool IsDelayed {
            get => _textField.isDelayed;
            set => _textField.isDelayed = value;
        }
    }

    public enum GenericTextInputStyle {
        Light = 0,
        Dark = 1,
        Custom = 3,
        LightNeutral = 4,
        DarkNeutral = 5
    }
}