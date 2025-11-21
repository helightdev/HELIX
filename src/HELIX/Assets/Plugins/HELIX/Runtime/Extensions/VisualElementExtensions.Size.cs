using System.Runtime.CompilerServices;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Size<T>(this T element, StyleLength2 size) where T : VisualElement {
            element.style.width = size.w;
            element.style.height = size.h;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Constraints<T>(this T element, StyleLength2 width, StyleLength2 height) where T : VisualElement {
            element.style.minWidth = width.w;
            element.style.maxWidth = width.h;
            element.style.minHeight = height.w;
            element.style.maxHeight = height.h;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WidthConstraints<T>(this T element, StyleLength2 width) where T : VisualElement {
            element.style.minWidth = width.w;
            element.style.maxWidth = width.h;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T HeightConstraints<T>(this T element, StyleLength2 height) where T : VisualElement {
            element.style.minHeight = height.w;
            element.style.maxHeight = height.h;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sized<T>(this T element,
            StyleLength? width = null,
            StyleLength? height = null
        ) where T : VisualElement {
            element.style.width = width.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.height = height.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            return element;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Constrained<T>(this T element,
            StyleLength? preferredWidth = null,
            StyleLength? preferredHeight = null,
            StyleLength? minWidth = null,
            StyleLength? minHeight = null,
            StyleLength? maxWidth = null,
            StyleLength? maxHeight = null
        ) where T : VisualElement {
            element.style.width = preferredWidth.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.height = preferredHeight.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.minWidth = minWidth.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.minHeight = minHeight.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.maxWidth = maxWidth.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.maxHeight = maxHeight.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            return element;
        }
    }
}