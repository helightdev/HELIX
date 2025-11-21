using System.Runtime.CompilerServices;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NoMargin<T>(this T element) where T : VisualElement {
            element.style.marginTop = 0;
            element.style.marginLeft = 0;
            element.style.marginRight = 0;
            element.style.marginBottom = 0;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Margin<T>(this T element, float margin) where T : VisualElement {
            element.style.marginTop = margin;
            element.style.marginLeft = margin;
            element.style.marginRight = margin;
            element.style.marginBottom = margin;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Margin<T>(this T element, StyleLength margin) where T : VisualElement {
            element.style.marginTop = margin;
            element.style.marginLeft = margin;
            element.style.marginRight = margin;
            element.style.marginBottom = margin;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Margin<T>(this T element, StyleLength2 margin) where T : VisualElement {
            element.style.marginTop = margin.h;
            element.style.marginBottom = margin.h;
            element.style.marginLeft = margin.w;
            element.style.marginRight = margin.w;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Margin<T>(this T element, StyleLength4 margin) where T : VisualElement {
            element.style.marginTop = margin.t;
            element.style.marginRight = margin.r;
            element.style.marginBottom = margin.b;
            element.style.marginLeft = margin.l;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StyleLength4 Margin<T>(this T element) where T : VisualElement {
            return new StyleLength4(
                element.style.marginTop,
                element.style.marginRight,
                element.style.marginBottom,
                element.style.marginLeft
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Margined<T>(this T element, float top = 0f, float right = 0f, float bottom = 0f,
            float left = 0f) where T : VisualElement {
            element.style.marginTop = top;
            element.style.marginRight = right;
            element.style.marginBottom = bottom;
            element.style.marginLeft = left;
            return element;
        }
    }
}