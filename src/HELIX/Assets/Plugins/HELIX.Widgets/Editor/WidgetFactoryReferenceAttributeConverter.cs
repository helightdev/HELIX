using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public class WidgetFactoryReferenceAttributeConverter<T> : UxmlAttributeConverter<WidgetFactoryReference<T>>
        where T : VisualElement {
        public override WidgetFactoryReference<T> FromString(string value) {
            return new WidgetFactoryReference<T>(value);
        }

        public override string ToString(WidgetFactoryReference<T> value) {
            return value.factoryName;
        }
    }
}