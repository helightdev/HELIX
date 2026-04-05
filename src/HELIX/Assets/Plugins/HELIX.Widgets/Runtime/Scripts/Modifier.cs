using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class Modifier {
        public bool isFallback = false;
        public abstract void Apply(VisualElement element);
        public abstract void Reset(VisualElement element);
        public virtual bool HasChanged([CanBeNull] Modifier previous) => true;

        public override bool Equals(object obj) {
            return obj is Modifier other && GetType() == other.GetType();
        }

        public bool DeepEquals(Modifier other) {
            if (ReferenceEquals(this, other)) return true;
            return !HasChanged(other);
        }

        public override int GetHashCode() => GetType().GetHashCode();

        public static void ApplyDelta(Widget previous, Widget next, VisualElement element) {
            ApplyDelta(previous?.GetModifiers(), next?.GetModifiers(), element);
        }

        public static void ApplyDelta(HashSet<Modifier> previous, HashSet<Modifier> current, VisualElement element) {
            if (current == null || element == null) { return; }

            if (previous != null) {
                foreach (var modifier in previous) {
                    if (!current.Contains(modifier)) { modifier.Reset(element); }
                }

                foreach (var modifier in current) {
                    if (previous.TryGetValue(modifier, out var prev) && modifier.DeepEquals(prev)) { continue; }

                    modifier.Apply(element);
                }
            } else {
                foreach (var modifier in current) { modifier.Apply(element); }
            }
        }
    }
    
        public static class ModifierExtensions {
        public static T WithModifier<T>(this T element, Modifier modifier) where T : Widget {
            element.AddModifier(modifier);
            return element;
        }

        public static T Flexible<T>(
            this T element,
            StyleFloat? grow = null,
            StyleFloat? shrink = null,
            Align selfCrossAxisAlign = Align.Auto
        ) where T : Widget {
            element.AddModifier(
                new FlexibleModifier(
                    selfCrossAxisAlign: selfCrossAxisAlign,
                    grow: grow ?? StyleKeyword.Initial,
                    shrink: shrink ?? StyleKeyword.Initial
                )
            );
            return element;
        }
        
        public static T Fill<T>(this T element) where T : Widget {
            return element.WithModifier(FlexibleModifier.Fill);
        }

        public static T Expand<T>(this T element, float flex = 1f, Align selfCrossAxisAlign = Align.Auto)
            where T : Widget {
            if (Mathf.Approximately(flex, 1f) && selfCrossAxisAlign == Align.Auto) {
                return element.WithModifier(FlexibleModifier.Expand);
            }

            return element.Flexible(flex, flex, selfCrossAxisAlign);
        }

        public static T Offset<T>(
            this T element,
            StyleLength4? offset = null,
            Position offsetType = Position.Relative
        ) where T : Widget {
            if (offset == null && offsetType == Position.Relative) { return element.WithModifier(OffsetModifier.None); }

            element.AddModifier(
                new OffsetModifier(
                    offset: offset ?? StyleLength4.Initial,
                    offsetType: offsetType
                )
            );
            return element;
        }

        public static T Stretch<T>(this T element) where T : Widget {
            return element.WithModifier(OffsetModifier.Stretch);
        }

        public static T Size<T>(
            this T element,
            StyleLength2? size = null,
            StyleLength2? minSize = null,
            StyleLength2? maxSize = null
        ) where T : Widget {
            if (size == null && minSize == null && maxSize == null) { return element.WithModifier(SizeModifier.None); }

            element.AddModifier(
                new SizeModifier(
                    size: size ?? StyleLength2.Initial,
                    minSize: minSize ?? StyleLength2.Initial,
                    maxSize: maxSize ?? StyleLength2.Initial
                )
            );
            return element;
        }
    }
}