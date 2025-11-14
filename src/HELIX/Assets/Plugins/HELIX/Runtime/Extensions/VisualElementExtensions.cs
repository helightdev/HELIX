using System;
using System.Runtime.CompilerServices;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        // public static float ResolveLength<T>(this T element, StyleLength styleLength, Axis axis)
        //     where T : VisualElement {
        //     if (styleLength.keyword != StyleKeyword.Undefined) {
        //         throw new InvalidOperationException($"Cannot resolve length with keyword {styleLength.keyword}.");
        //     }
        //
        //     var length = styleLength.value;
        //     if (length.unit != LengthUnit.Percent) return length.value;
        //     if (element.parent == null)
        //         throw new InvalidOperationException("Cannot resolve percentage length without a parent element.");
        //     var parentSize = element.parent.layout;
        //     if (axis == Axis.Horizontal) {
        //         return parentSize.width * (length.value / 100f);
        //     }
        //
        //     return parentSize.height * (length.value / 100f);
        // }

        public static T AddClasses<T>(this T element, params string[] classNames) where T : VisualElement {
            foreach (var className in classNames) {
                element.AddToClassList(className);
            }
            return element;
        }
        public static T WithClasses<T>(this T element, params string[] classNames) where T : VisualElement {
            element.ClearClassList();
            return element.AddClasses(classNames);
        }
        
        public static T NoPaddingAndMargin<T>(this T element) where T : VisualElement => element.NoBorder().NoMargin();
    }
}