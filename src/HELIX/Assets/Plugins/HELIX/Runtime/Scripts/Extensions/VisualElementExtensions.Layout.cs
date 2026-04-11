using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        public static T Flexible<T>(this T element, float grow = 1, float shrink = 1, Align selfAlign = Align.Auto) where T : VisualElement {
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
            element.style.alignSelf = selfAlign;
            return element;
        }

        public static T Fill<T>(this T element, Align selfAlign = Align.Stretch) where T : VisualElement {
            element.style.flexGrow = 1;
            element.style.flexShrink = 1;
            element.style.alignSelf = selfAlign;
            return element;
        }
        
        public static T Tight<T>(this T element) where T : VisualElement {
            element.style.flexGrow = 0;
            element.style.flexShrink = 0;
            return element;
        }

        public static T TightStretch<T>(this T element, Align selfAlign = Align.Stretch) where T : VisualElement {
            element.style.flexGrow = 0;
            element.style.flexShrink = 0;
            element.style.alignSelf = selfAlign;
            return element;
        }

        public static T FlexContainer<T>(
            this T element,
            Axis axis = Axis.Vertical,
            Justify mainAxisAlign = Justify.FlexStart,
            Align crossAxisAlign = Align.Center,
            Wrap wrap = Wrap.NoWrap,
            bool reverse = false
        ) where T : VisualElement {
            element.style.display = DisplayStyle.Flex;
            element.style.flexDirection = axis.ToFlexDirection(reverse);
            element.style.flexWrap = wrap;
            element.style.justifyContent = mainAxisAlign;
            element.style.alignItems = crossAxisAlign;
            return element;
        }
    }
}