using System;
using System.Text;
using UnityEngine.UIElements;

namespace HELIX.Types {
    public readonly struct BoxConstraints : IEquatable<BoxConstraints> {
        public readonly StyleLength2 preferred;
        public readonly StyleLength2 min;
        public readonly StyleLength2 max;

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
            var builder = new StringBuilder();
            builder.Append("BoxConstraints(");
            builder.Append("width: ");
            builder.Append(FormatPart(min.w, preferred.w, max.w));
            builder.Append(", height: ");
            builder.Append(FormatPart(min.h, preferred.h, max.h));
            builder.Append(")");
            return builder.ToString();
        }

        private string FormatPart(StyleLength a, StyleLength b, StyleLength c) {
            if (a.keyword is StyleKeyword.Initial or StyleKeyword.Auto &&
                c.keyword is StyleKeyword.Initial or StyleKeyword.Auto ||
                a == c && a == b) return b.FormatStyleValue();
            return a.FormatStyleValue() + " ≤ " + b.FormatStyleValue() + " ≤ " + c.FormatStyleValue();
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

        public static BoxConstraints Min(StyleLength2 min) => new(min, min, StyleLength2.Initial);

        public static readonly BoxConstraints Initial = new(
            StyleLength2.Initial,
            StyleLength2.Initial,
            StyleLength2.Initial
        );
    }
}