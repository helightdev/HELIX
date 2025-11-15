using System;
using HELIX.Widgets.Theming;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Editor {
    [CustomPropertyDrawer(typeof(ThemeOverrides), true)]
    public class ThemePropertyOverrideDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var type = fieldInfo.FieldType.GetGenericArguments()[0];
            var element = new PropertyOverrideEditor(preferredLabel, property, type);
            element.Load();
            return element;
        }
    }

    public class PropertyOverrideEditor : BaseField<ThemeOverrides> {
        private readonly EnumField _typeField;
        private readonly PropertyField _valueField;
        private readonly DropdownField _referenceField;
        private readonly SerializedProperty _property;
        private readonly VisualElement _element;
        private readonly Type _propertyType;

        public PropertyOverrideEditor(string label, SerializedProperty property, Type type)
            : this(label, property, new VisualElement(), type) {}

        private PropertyOverrideEditor(string label, SerializedProperty property, VisualElement element, Type type) : base(label,
            element) {
            _propertyType = type;
            _element = element;
            _property = property;
            _typeField = new EnumField(ThemeOverrideType.None);
            _valueField = new PropertyField(property.FindPropertyRelative("constantValue"), "") {
                style = { flexGrow = 1 }
            };
            _referenceField = new DropdownField(ThemePropertyCollection.GetCollectionsOfType(type), 0,
                ThemePropertyDrawer.FormatItem, ThemePropertyDrawer.FormatItem) { style = { flexGrow = 1 } };

            element.Add(_typeField);
            element.Add(_valueField);
            element.Add(_referenceField);

            _typeField.RegisterValueChangedCallback(evt => Push());
            _valueField.RegisterValueChangeCallback(evt => Push());
            _referenceField.RegisterValueChangedCallback(evt => Push());
            
            element.style.flexDirection = FlexDirection.Row;
        }

        public void Load() {
            _typeField.value = (ThemeOverrideType)_property.FindPropertyRelative("type").enumValueIndex;
            _referenceField.value = _property.FindPropertyRelative("propertyReference").stringValue;
            Refresh();
        }

        private void Push() {
            Refresh();
            _property.FindPropertyRelative("type").enumValueIndex = (int)(ThemeOverrideType)_typeField.value;
            _property.FindPropertyRelative("propertyReference").stringValue = _referenceField.value;
            _property.serializedObject.ApplyModifiedProperties();
        }

        private void Refresh() {
            _valueField.style.display = (ThemeOverrideType)_typeField.value == ThemeOverrideType.Value
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            _referenceField.style.display = (ThemeOverrideType)_typeField.value == ThemeOverrideType.PropertyReference
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            _element.style.flexDirection = (ThemeOverrideType)_typeField.value == ThemeOverrideType.Value
                ? FlexDirection.Column
                : FlexDirection.Row;
        }
    }
}