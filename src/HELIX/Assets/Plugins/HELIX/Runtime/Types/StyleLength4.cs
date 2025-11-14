using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
    public struct StyleLength4 : IEquatable<StyleLength4> {
        public StyleLength t;
        public StyleLength r;
        public StyleLength b;
        public StyleLength l;

        public StyleLength4(StyleLength t, StyleLength r, StyleLength b, StyleLength l) {
            this.t = t;
            this.r = r;
            this.b = b;
            this.l = l;
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

        public bool Equals(StyleLength4 other) {
            return t.Equals(other.t) && r.Equals(other.r) && b.Equals(other.b) && l.Equals(other.l);
        }

        public override bool Equals(object obj) {
            return obj is StyleLength4 other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(t, r, b, l);
        }
        
        public override string ToString() {
            return $"({t}, {r}, {b}, {l})";
        }
    }
}