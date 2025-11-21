using System;
using System.Runtime.CompilerServices;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        public static T Flexible<T>(this T element, float grow = 1, float shrink = 1) where T : VisualElement {
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
            return element;
        }
        
        public static T Tight<T>(this T element) where T : VisualElement {
            element.style.flexGrow = 0;
            element.style.flexShrink = 0;
            return element;
        }

        public static T FlexContainer<T>(this T element,
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