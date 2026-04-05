using HELIX.Extensions;
using HELIX.Widgets.Theming;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using RowElement = HELIX.Widgets.Elements.RowElement;

namespace HELIX.Widgets.Editor {
    [CustomPropertyDrawer(typeof(ThemeOptional<>))]
    public class ThemeOptionalDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var row = new RowElement();
            var toggle = new Toggle { style = { marginRight = 10 } }.AddTo(row);
            toggle.BindProperty(property.FindPropertyRelative("hasValue"));

            new PropertyField(property.FindPropertyRelative("value"), preferredLabel) { style = { flexGrow = 1 } }
                .AddTo(row);

            return row;
        }
    }
}