using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Coloring {
    public struct OkLchColor : IEquatable<OkLchColor> {
        public float l, c, h;

        public OkLchColor(float l, float c, float h) {
            this.l = l;
            this.c = c;
            this.h = h;
        }
        
        public OkLchColor(Color linear) : this() {
            var rgb = new float3(linear.r, linear.g, linear.b);
            var components = OkLabHelper.ToLch(OkLabHelper.FromLinearRgb(rgb));
            l = components.x;
            c = components.y;
            h = components.z;
        }
        
        public Color ToGamma() {
            var color = (Color)this;
            return color.gamma;
        }

        public override string ToString() {
            return $"{nameof(l)}: {l}, {nameof(c)}: {c}, {nameof(h)}: {h}";
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(OkLchColor other) {
            return l == other.l && c == other.c && h == other.h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object o) {
            return o is OkLchColor converted && Equals(converted);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return (int)math.hash(this);
        }

        public static explicit operator OkLabColor(OkLchColor color) {
            return OkLabHelper.FromLch(color);
        }

        public static explicit operator Color(OkLchColor color) {
            var lab = OkLabHelper.FromLch(color);
            var comp = OkLabHelper.ToLinearRgb(lab);
            return new Color(comp.x, comp.y, comp.z);
        }

        public static implicit operator float3(OkLchColor color) {
            return new float3(color.l, color.c, color.h);
        }

        public static implicit operator OkLchColor(float3 components) {
            return new OkLchColor(components.x, components.y, components.z);
        }
    }
}