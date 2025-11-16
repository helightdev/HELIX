using HELIX.Widgets.Theming;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Editor {
    [CustomPropertyDrawer(typeof(WidgetFactoryReference<>))]
    public class WidgetFactoryDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var genericArgument = fieldInfo.FieldType.GetGenericArguments()[0];
            var strings = WidgetFactoryCollection.GetCollectionsOfType(genericArgument);

            var field = new DropdownField(preferredLabel, strings, 0, FormatItem, FormatItem);
            field.RegisterValueChangedCallback(evt => { 
                property.FindPropertyRelative("factoryName").stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });
            field.value = property.FindPropertyRelative("factoryName").stringValue;
            field.BindProperty(property.FindPropertyRelative("factoryName"));
            return field;
        }

        public static string FormatItem(string value) {
            var values = value.Split(":");
            if (values.Length == 1) return values[0];
            return values[0].Split(".")[^1] + "." + values[1];
        }
    }
}