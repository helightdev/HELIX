using System.Runtime.CompilerServices;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NoPosition<T>(this T element) where T : VisualElement {
            element.style.top = 0;
            element.style.left = 0;
            element.style.right = 0;
            element.style.bottom = 0;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Position<T>(this T element, float position) where T : VisualElement {
            element.style.top = position;
            element.style.left = position;
            element.style.right = position;
            element.style.bottom = position;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Position<T>(this T element, StyleLength position) where T : VisualElement {
            element.style.top = position;
            element.style.left = position;
            element.style.right = position;
            element.style.bottom = position;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Position<T>(this T element, StyleLength2 position) where T : VisualElement {
            element.style.top = position.h;
            element.style.bottom = position.h;
            element.style.left = position.w;
            element.style.right = position.w;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Position<T>(this T element, StyleLength4 position) where T : VisualElement {
            element.style.top = position.t;
            element.style.right = position.r;
            element.style.bottom = position.b;
            element.style.left = position.l;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StyleLength4 Position<T>(this T element) where T : VisualElement {
            return new StyleLength4(
                element.style.top,
                element.style.right,
                element.style.bottom,
                element.style.left
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MakeRelative<T>(this T element) where T : VisualElement {
            element.style.position = UnityEngine.UIElements.Position.Relative;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MakeAbsolute<T>(this T element) where T : VisualElement {
            element.style.position = UnityEngine.UIElements.Position.Absolute;
            return element;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Positioned<T>(
            this T element, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null,
            StyleLength? left = null,
            Position type = UnityEngine.UIElements.Position.Absolute)
            where T : VisualElement {
            element.style.position = type;
            element.style.top = top.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.right = right.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.bottom = bottom.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            element.style.left = left.GetValueOrDefault(new StyleLength(StyleKeyword.Initial));
            return element;
        }
    }
}