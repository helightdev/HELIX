using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Editor.Drawers {
    [CustomPropertyDrawer(typeof(StyleLength))]
    public class StyleLengthDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var field = new StyleLengthField(preferredLabel) { bindingPath = property.propertyPath };
            field.BindProperty(property);

            var enumValueIndex = property.FindPropertyRelative("m_Keyword").enumValueIndex;
            var initialStyle = new StyleLength { keyword = (StyleKeyword)enumValueIndex };
            if (initialStyle.keyword == StyleKeyword.Undefined) {
                initialStyle.value = new Length {
                    value = property.FindPropertyRelative("m_Value").FindPropertyRelative("m_Value").floatValue,
                    unit = (LengthUnit)property.FindPropertyRelative("m_Value").FindPropertyRelative("m_Unit")
                        .enumValueIndex
                };
            }
            field.Apply(initialStyle);
            field.RegisterCallback<ChangeEvent<StyleLength>>(evt => {
                property.boxedValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });
            return field;
        }
    }

    internal class StyleLengthField : BaseField<StyleLength> {
        private FloatField _valueField;
        private EnumField _keywordField;

        public StyleLengthField(string label, VisualElement element) : base(label, element) {
            _valueField = new FloatField() { style = { flexGrow = 1 } };
            _keywordField = new EnumField(StyleInputEnum.Initial);

            element.style.flexDirection = FlexDirection.Row;
            element.Add(_valueField);
            element.Add(_keywordField);

            _keywordField.RegisterValueChangedCallback(_ => { Push(); });
            _valueField.RegisterValueChangedCallback(_ => { Push(); });
        }


        public void Apply(StyleLength styleLength) {
            var enumValue = styleLength.keyword switch {
                StyleKeyword.Auto => StyleInputEnum.Auto,
                StyleKeyword.None => StyleInputEnum.None,
                StyleKeyword.Initial => StyleInputEnum.Initial,
                StyleKeyword.Undefined => styleLength.value.unit switch {
                    LengthUnit.Pixel => StyleInputEnum.Pixel,
                    LengthUnit.Percent => StyleInputEnum.Percent,
                    _ => StyleInputEnum.Initial
                },
                _ => StyleInputEnum.Initial
            };
            _keywordField.SetValueWithoutNotify(enumValue);
            _valueField.SetValueWithoutNotify(styleLength.value.value);
        }

        public void Push() {
            var type = (StyleInputEnum)_keywordField.value;
            var fieldValue = _valueField.value;
            var newValue = type switch {
                StyleInputEnum.Pixel when !Mathf.Approximately(fieldValue, 0) => new
                    StyleLength(StyleKeyword.Undefined) { value = new Length(fieldValue, LengthUnit.Pixel) },
                StyleInputEnum.Pixel => new StyleLength(StyleKeyword.Undefined),
                StyleInputEnum.Percent => new StyleLength(StyleKeyword.Undefined) {
                    value = new Length(fieldValue, LengthUnit.Percent)
                },
                _ => new StyleLength(type switch {
                    StyleInputEnum.Auto => StyleKeyword.Auto,
                    StyleInputEnum.None => StyleKeyword.None,
                    StyleInputEnum.Initial => StyleKeyword.Initial,
                    _ => throw new ArgumentOutOfRangeException()
                })
            };

            var before = value;
            SetValueWithoutNotify(newValue);
            using var evt = ChangeEvent<StyleLength>.GetPooled(before, newValue);
            evt.target = this;
            SendEvent(evt);
        }

        public StyleLengthField(string label) : this(label, new VisualElement()) { }
    }
}