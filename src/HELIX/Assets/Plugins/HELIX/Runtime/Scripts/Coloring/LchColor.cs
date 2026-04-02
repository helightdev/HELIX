using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Coloring {
    public struct LchColor : IEquatable<LchColor> {
        public float l, c, h;

        public LchColor(float l, float c, float h) {
            this.l = l;
            this.c = c;
            this.h = h;
        }
        
        public LchColor(Color linear) : this() {
            var rgb = new float3(linear.r, linear.g, linear.b);
            var components = OkLabHelper.ToLch(OkLabHelper.FromSrgb(rgb));
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
        public bool Equals(LchColor other) {
            return l == other.l && c == other.c && h == other.h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object o) {
            return o is LchColor converted && Equals(converted);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return (int)math.hash(this);
        }

        public static explicit operator LabColor(LchColor color) {
            return OkLabHelper.FromLch(color);
        }

        public static explicit operator Color(LchColor color) {
            var comp = OkLabHelper.ToSrgb(OkLabHelper.FromLch(color));
            return new Color(comp.x, comp.y, comp.z);
        }

        public static implicit operator float3(LchColor color) {
            return new float3(color.l, color.c, color.h);
        }

        public static implicit operator LchColor(float3 components) {
            return new LchColor(components.x, components.y, components.z);
        }
    }
}