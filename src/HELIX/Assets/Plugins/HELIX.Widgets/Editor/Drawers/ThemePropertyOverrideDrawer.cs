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
            var row = new VisualElement();

            var element = new PropertyOverrideEditor(preferredLabel, property);
            element.Load();
            return element;
        }
    }

    public class PropertyOverrideEditor : BaseField<ThemeOverrides> {
        public PropertyOverrideEditor(string label, SerializedProperty property) : this(label, property,
            new VisualElement()) { }

        public EnumField typeField;
        public PropertyField valueField;
        public DropdownField referenceField;
        public SerializedProperty property;

        public PropertyOverrideEditor(string label, SerializedProperty property, VisualElement element) : base(label,
            element) {
            element.style.flexDirection = FlexDirection.Row;
            
            this.property = property;
            typeField = new EnumField(ThemeOverrideType.None);
            valueField = new PropertyField(property.FindPropertyRelative("constantValue"), "") {
                style = { flexGrow = 1 }
            };
            referenceField = new DropdownField(ThemePropertyCollection.LoadedCollections, 0,
                ThemePropertyDrawer.FormatItem, ThemePropertyDrawer.FormatItem) {
                style = { flexGrow = 1 }
            };

            element.Add(typeField);
            element.Add(valueField);
            element.Add(referenceField);

            typeField.RegisterValueChangedCallback(evt => Push());
            valueField.RegisterValueChangeCallback(evt => Push());
            referenceField.RegisterValueChangedCallback(evt => Push());
        }

        public void Load() {
            typeField.value = (ThemeOverrideType)property.FindPropertyRelative("type").enumValueIndex;
            referenceField.value = property.FindPropertyRelative("propertyReference").stringValue;
            Refresh();
        }

        private void Push() {
            Refresh();
            property.FindPropertyRelative("type").enumValueIndex = (int)(ThemeOverrideType)typeField.value;
            property.FindPropertyRelative("propertyReference").stringValue = referenceField.value;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void Refresh() {
            valueField.style.display = (ThemeOverrideType)typeField.value == ThemeOverrideType.Value
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            referenceField.style.display = (ThemeOverrideType)typeField.value == ThemeOverrideType.PropertyReference
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}