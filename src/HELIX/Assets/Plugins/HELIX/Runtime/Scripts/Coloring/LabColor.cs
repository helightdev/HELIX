using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Coloring {
    [Serializable]
    public struct LabColor : IEquatable<LabColor> {
        public float l, a, b;

        public LabColor(float l, float a, float b) {
            this.l = l;
            this.a = a;
            this.b = b;
        }

        public LabColor(Color linear) : this() {
            var rgb = new float3(linear.r, linear.g, linear.b);
            var components = OkLabHelper.FromLinearRgb(rgb);
            l = components.x;
            a = components.y;
            b = components.z;
        }

        public Color ToGamma() {
            var color = (Color)this;
            return color.gamma;
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(LabColor other) {
            return l == other.l && a == other.a && b == other.b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object o) {
            return o is LabColor converted && Equals(converted);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return (int)math.hash(this);
        }

        public static LabColor Lerp(LabColor a, LabColor b, float t) {
            return math.lerp(a, b, t);
        }
        
        public static explicit operator Color(LabColor color) {
            var comp = OkLabHelper.ToLinearRgb(color);
            return new Color(comp.x, comp.y, comp.z);
        }

        public static explicit operator LchColor(LabColor color) {
            return OkLabHelper.ToLch(color);
        }

        public static implicit operator float3(LabColor color) {
            return new float3(color.l, color.a, color.b);
        }

        public static implicit operator LabColor(float3 components) {
            return new LabColor(components.x, components.y, components.z);
        }
    }
}