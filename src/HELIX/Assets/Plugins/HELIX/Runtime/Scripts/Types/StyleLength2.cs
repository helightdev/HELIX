using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
    [Serializable]
    public struct StyleLength2 : IEquatable<StyleLength2> {
        public StyleLength w, h;

        public StyleLength2(StyleLength w, StyleLength h) {
            this.w = w;
            this.h = h;
        }

        public StyleLength2(StyleLength v) {
            w = v;
            h = v;
        }

        public static implicit operator StyleLength2(Vector2 v) {
            return new StyleLength2(new StyleLength(v.x), new StyleLength(v.y));
        }

        public bool Equals(StyleLength2 other) {
            return w.Equals(other.w) && h.Equals(other.h);
        }

        public override bool Equals(object obj) {
            return obj is StyleLength2 other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(w, h);
        }

        public override string ToString() {
            return $"StyleLength2(width: {w.FormatStyleValue()}, height: {h.FormatStyleValue()})";
        }
        
        public static readonly StyleLength2 Initial = new StyleLength2(StyleKeyword.Initial);
        public static readonly StyleLength2 Auto = new StyleLength2(StyleKeyword.Auto);
    }
}