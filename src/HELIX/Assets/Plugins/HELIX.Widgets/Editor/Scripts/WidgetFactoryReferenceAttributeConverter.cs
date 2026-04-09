using HELIX.Widgets.Theming;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Editor {
    public class WidgetFactoryReferenceAttributeConverter<T> : UxmlAttributeConverter<ElementFactoryReference<T>>
        where T : VisualElement {
        public override ElementFactoryReference<T> FromString(string value) {
            return new ElementFactoryReference<T>(value);
        }

        public override string ToString(ElementFactoryReference<T> value) {
            return value.factoryName;
        }
    }
}