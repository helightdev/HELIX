using System;
using HELIX.Extensions;
using HELIX.Widgets.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    [UxmlElement]
    public partial class GenericTextInput : BaseElement {
        private const string _ussStyleLight = "helix-textfield-style-light";
        private const string _ussStyleLightNeutral = "helix-textfield-style-light-neutral";
        private const string _ussStyleDark = "helix-textfield-style-dark";
        private const string _ussStyleDarkNeutral = "helix-textfield-style-dark-neutral";
        private const string _ussStyleClear = "helix-textfield-style-clear";

        private Color _cursorColor = Color.white;
        private bool _expands = true;
        private bool _hadCustomColor;
        private Color _selectionColor = Color.red;

        private GenericTextInputStyle _textInputStyle = GenericTextInputStyle.Light;

        public TextField BackingTextField { get; }

        public GenericTextInput() {
            this.WithStylesheet(AuxiliaryStylesheets.Helix).AddClasses("helix-generic-text-input");
            BackingTextField = new TextField().WithName("BackingTextField").Stretched().AddTo(this);
            ApplySelectionColors();
            BackingTextField.RegisterCallback<CustomStyleResolvedEvent>(_ => { ApplySelectionColors(); });
            BackingTextField.RegisterValueChangedCallback(evt => { OnValueChanged?.Invoke(evt.newValue); });
            BackingTextField.RegisterCallback<FocusEvent>(_ => { OnFocus?.Invoke(); });
            BackingTextField.RegisterCallback<BlurEvent>(_ => { OnBlur?.Invoke(); });

            var element = BackingTextField.textEdition as VisualElement ?? BackingTextField;
            element.RegisterCallback<FocusEvent>(_ => { OnBeginEditing?.Invoke(); });
            element.RegisterCallback<BlurEvent>(_ => { OnEndEditing?.Invoke(); });
            element.RegisterCallback<NavigationSubmitEvent>(_ => { OnSubmit?.Invoke(Value); });
            element.RegisterCallback<NavigationCancelEvent>(_ => { OnCancel?.Invoke(); });
        }
        
        public void RequestEditingFocus() {
            BackingTextField.schedule.Execute(() => BackingTextField.Focus()).ExecuteLater(0);
        }

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
                if (value) BackingTextField.Stretched();
                else BackingTextField.Loosen();
            }
        }

        [Header("Delegated Properties"), UxmlAttribute]
        public string Value {
            get => BackingTextField.value;
            set => BackingTextField.value = value;
        }

        [UxmlAttribute]
        public bool Multiline {
            get => BackingTextField.multiline;
            set => BackingTextField.multiline = value;
        }

        [UxmlAttribute]
        public bool IsReadOnly {
            get => BackingTextField.isReadOnly;
            set => BackingTextField.isReadOnly = value;
        }

        [UxmlAttribute]
        public int MaxLength {
            get => BackingTextField.maxLength;
            set => BackingTextField.maxLength = value;
        }

        [UxmlAttribute]
        public bool IsPasswordField {
            get => BackingTextField.isPasswordField;
            set => BackingTextField.isPasswordField = value;
        }

        [UxmlAttribute]
        public char MaskChar {
            get => BackingTextField.maskChar;
            set => BackingTextField.maskChar = value;
        }

        [UxmlAttribute]
        public bool AutoCorrection {
            get => BackingTextField.autoCorrection;
            set => BackingTextField.autoCorrection = value;
        }

        [UxmlAttribute]
        public bool HideMobileInput {
            get => BackingTextField.hideMobileInput;
            set => BackingTextField.hideMobileInput = value;
        }

        [UxmlAttribute]
        public TouchScreenKeyboardType KeyboardType {
            get => BackingTextField.keyboardType;
            set => BackingTextField.keyboardType = value;
        }

        [UxmlAttribute]
        public bool IsDelayed {
            get => BackingTextField.isDelayed;
            set => BackingTextField.isDelayed = value;
        }

        public event Action OnBeginEditing;
        public event Action OnEndEditing;
        public event Action<string> OnValueChanged;
        public event Action<string> OnSubmit;
        public event Action OnCancel;
        public event Action OnFocus;
        public event Action OnBlur;

        private void ApplySelectionColors() {
            if (_hadCustomColor && _textInputStyle != GenericTextInputStyle.Custom) {
                BackingTextField.customStyle.TryGetValue(new CustomStyleProperty<Color>("--unity-cursor-color"), out var cursorColor);
                BackingTextField.customStyle.TryGetValue(new CustomStyleProperty<Color>("--unity-selection-color"), out var selectionColor);
#pragma warning disable CS0618 // Type or member is obsolete
                BackingTextField.textSelection.selectionColor = selectionColor;
                BackingTextField.textSelection.cursorColor = cursorColor;
#pragma warning restore CS0618 // Type or member is obsolete    
                _hadCustomColor = false;
            }

            switch (_textInputStyle) {
                case GenericTextInputStyle.Light:        BackingTextField.WithClasses(_ussStyleLight); break;
                case GenericTextInputStyle.Dark:         BackingTextField.WithClasses(_ussStyleDark); break;
                case GenericTextInputStyle.LightNeutral: BackingTextField.WithClasses(_ussStyleLightNeutral); break;
                case GenericTextInputStyle.DarkNeutral:  BackingTextField.WithClasses(_ussStyleDarkNeutral); break;
                case GenericTextInputStyle.Custom:
                    BackingTextField.WithClasses(_ussStyleClear);
#pragma warning disable CS0618 // Type or member is obsolete
                    BackingTextField.textSelection.selectionColor = _selectionColor;
                    BackingTextField.textSelection.cursorColor = _cursorColor;
#pragma warning restore CS0618 // Type or member is obsolete        
                    _hadCustomColor = true;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
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