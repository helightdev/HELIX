using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NoBorder<T>(this T element) where T : VisualElement {
            element.style.borderTopWidth = 0;
            element.style.borderLeftWidth = 0;
            element.style.borderRightWidth = 0;
            element.style.borderBottomWidth = 0;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Border<T>(this T element, float value) where T : VisualElement {
            element.style.borderTopWidth = value;
            element.style.borderLeftWidth = value;
            element.style.borderRightWidth = value;
            element.style.borderBottomWidth = value;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Border<T>(this T element, Vector2 value) where T : VisualElement {
            element.style.borderTopWidth = value.y;
            element.style.borderBottomWidth = value.y;
            element.style.borderLeftWidth = value.x;
            element.style.borderRightWidth = value.x;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Border<T>(this T element, Vector4 value) where T : VisualElement {
            element.style.borderTopWidth = value.x;
            element.style.borderRightWidth = value.y;
            element.style.borderBottomWidth = value.z;
            element.style.borderLeftWidth = value.w;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Border<T>(this T element) where T : VisualElement {
            return new Vector4(
                element.style.borderLeftWidth.value,
                element.style.borderTopWidth.value,
                element.style.borderRightWidth.value,
                element.style.borderBottomWidth.value
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NoBorderRadius<T>(this T element) where T : VisualElement {
            element.style.borderTopLeftRadius = 0;
            element.style.borderTopRightRadius = 0;
            element.style.borderBottomLeftRadius = 0;
            element.style.borderBottomRightRadius = 0;
            return element;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T BorderRadius<T>(this T element, float value) where T : VisualElement {
            element.style.borderTopLeftRadius = value;
            element.style.borderTopRightRadius = value;
            element.style.borderBottomLeftRadius = value;
            element.style.borderBottomRightRadius = value;
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T BorderRadius<T>(this T element, Vector4 value) where T : VisualElement {
            element.style.borderTopLeftRadius = value.x;
            element.style.borderTopRightRadius = value.y;
            element.style.borderBottomRightRadius = value.z;
            element.style.borderBottomLeftRadius = value.w;
            return element;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T BorderColor<T>(this T element, Color color) where T : VisualElement {
            element.style.borderTopColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomColor = color;
            return element;
        }
    }
}