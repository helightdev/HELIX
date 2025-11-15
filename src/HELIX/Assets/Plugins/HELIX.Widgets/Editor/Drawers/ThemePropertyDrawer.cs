using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Editor {
    [CustomPropertyDrawer(typeof(ThemePropertyReferenceAttribute))]
    public class ThemePropertyDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var strings = ThemePropertyCollection.LoadedCollections;
            var field = new DropdownField(preferredLabel, strings, 0, FormatItem, FormatItem);
            field.BindProperty(property);
            return field;
        }

        public static string FormatItem(string value) {
            var values = value.Split(":");
            if (values.Length == 1) return values[0];
            return values[0].Split(".")[^1] + "." + values[1];
        }
    }
}