using System;
using UnityEngine.UIElements;

namespace HELIX.Types {
    public struct BoxConstraints : IEquatable<BoxConstraints> {
        public StyleLength2 preferred;
        public StyleLength2 min;
        public StyleLength2 max;

        public BoxConstraints(StyleLength2 preferred, StyleLength2 min, StyleLength2 max) {
            this.preferred = preferred;
            this.min = min;
            this.max = max;
        }

        public void Apply(VisualElement element) {
            element.style.width = preferred.w;
            element.style.height = preferred.h;
            element.style.minWidth = min.w;
            element.style.minHeight = min.h;
            element.style.maxWidth = max.w;
            element.style.maxHeight = max.h;
        }

        public bool Equals(BoxConstraints other) {
            return preferred.Equals(other.preferred) && min.Equals(other.min) && max.Equals(other.max);
        }

        public override bool Equals(object obj) {
            return obj is BoxConstraints other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(preferred, min, max);
        }

        public override string ToString() {
            return $"{nameof(preferred)}: {preferred}, {nameof(min)}: {min}, {nameof(max)}: {max}";
        }

        public static BoxConstraints Only(
            StyleLength2? preferred = null,
            StyleLength2? min = null,
            StyleLength2? max = null
        ) =>
            new(
                preferred.GetValueOrDefault(StyleLength2.Initial),
                min.GetValueOrDefault(StyleLength2.Initial),
                max.GetValueOrDefault(StyleLength2.Initial)
            );

        public static BoxConstraints Preferred(StyleLength2 preferred) =>
            new(preferred, StyleLength2.Initial, StyleLength2.Initial);

        public static BoxConstraints Preferred(StyleLength width, StyleLength height) =>
            Preferred(new StyleLength2(width, height));

        public static BoxConstraints Tight(StyleLength2 size) => new(size, size, size);

        public static BoxConstraints Tight(StyleLength width, StyleLength height) =>
            Tight(new StyleLength2(width, height));

        public static BoxConstraints Loose(StyleLength2 max) => new(StyleLength2.Initial, StyleLength2.Initial, max);

        public static readonly BoxConstraints Initial = new(
            StyleLength2.Initial,
            StyleLength2.Initial,
            StyleLength2.Initial
        );
    }
}