using System.Runtime.CompilerServices;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NoPadding<T>(this T element) where T : VisualElement {
            element.style.paddingTop = 0;
            element.style.paddingLeft = 0;
            element.style.paddingRight = 0;
            element.style.paddingBottom = 0;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Padding<T>(this T element, float padding) where T : VisualElement {
            element.style.paddingTop = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
            element.style.paddingBottom = padding;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Padding<T>(this T element, StyleLength padding) where T : VisualElement {
            element.style.paddingTop = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
            element.style.paddingBottom = padding;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Padding<T>(this T element, StyleLength2 padding) where T : VisualElement {
            element.style.paddingTop = padding.h;
            element.style.paddingBottom = padding.h;
            element.style.paddingRight = padding.w;
            element.style.paddingLeft = padding.w;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Padding<T>(this T element, StyleLength4 padding) where T : VisualElement {
            element.style.paddingTop = padding.t;
            element.style.paddingRight = padding.r;
            element.style.paddingBottom = padding.b;
            element.style.paddingLeft = padding.l;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StyleLength4 Padding<T>(this T element) where T : VisualElement {
            return new StyleLength4(
                element.style.paddingTop,
                element.style.paddingRight,
                element.style.paddingBottom,
                element.style.paddingLeft
            );
        }
    }
}