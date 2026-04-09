using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
    [Serializable]
    public struct StyleLength4 : IEquatable<StyleLength4>, IStyleLength4 {
        public StyleLength l, t, r, b;

        public StyleLength4(StyleLength l, StyleLength t, StyleLength r, StyleLength b) {
            this.l = l;
            this.t = t;
            this.r = r;
            this.b = b;
        }

        public StyleLength4(StyleLength2 xy, StyleLength2 zw) {
            t = xy.w;
            r = xy.h;
            b = zw.w;
            l = zw.h;
        }

        public StyleLength4(StyleLength v) {
            t = v;
            r = v;
            b = v;
            l = v;
        }

        public static implicit operator StyleLength2(StyleLength4 sl4) {
            return new StyleLength2(sl4.t, sl4.r);
        }

        public static implicit operator StyleLength4(Vector4 v) {
            return new StyleLength4(v.x, v.y, v.z, v.w);
        }

        public static implicit operator StyleLength4(StyleLength2 xy) {
            return new StyleLength4(xy, xy);
        }

        public static implicit operator StyleLength4(StyleLength v) {
            return new StyleLength4(v);
        }

        public static implicit operator StyleLength4(float v) {
            return new StyleLength4(v);
        }

        public bool Equals(StyleLength4 other) {
            return t.Equals(other.t) && r.Equals(other.r) && b.Equals(other.b) && l.Equals(other.l);
        }

        public override bool Equals(object obj) {
            return obj is StyleLength4 other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(l, t, r, b);
        }

        public override string ToString() {
            return HelixFormattingHelper.BuildQuadruple(
                "StyleLength4",
                l.FormatStyleValue(),
                t.FormatStyleValue(),
                r.FormatStyleValue(),
                b.FormatStyleValue()
            );
        }

        public StyleLength4 ToStyleLength4() => this;

        public static StyleLength4 Only(
            StyleLength? left = null,
            StyleLength? right = null,
            StyleLength? top = null,
            StyleLength? bottom = null
        ) {
            return new StyleLength4(
                left ?? StyleKeyword.Initial,
                top ?? StyleKeyword.Initial,
                right ?? StyleKeyword.Initial,
                bottom ?? StyleKeyword.Initial
            );
        }

        public static StyleLength4 Symmetric(StyleLength? horizontal = null, StyleLength? vertical = null) {
            return new StyleLength4(
                horizontal ?? StyleKeyword.Initial,
                vertical ?? StyleKeyword.Initial,
                horizontal ?? StyleKeyword.Initial,
                vertical ?? StyleKeyword.Initial
            );
        }

        public static StyleLength4 All(StyleLength value) {
            return new StyleLength4(value);
        }

        public static readonly StyleLength4 Zero = new StyleLength4(0);
        public static readonly StyleLength4 Initial = new StyleLength4(StyleKeyword.Initial);
        public static readonly StyleLength4 Auto = new StyleLength4(StyleKeyword.Auto);
    }

    public interface IStyleLength4 {
        StyleLength4 ToStyleLength4();
    }
}