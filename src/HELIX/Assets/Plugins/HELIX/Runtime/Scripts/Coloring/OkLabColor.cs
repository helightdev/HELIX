using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Coloring {
  [Serializable]
  public struct OkLabColor : IEquatable<OkLabColor> {
    public float l, a, b;

    public OkLabColor(float l, float a, float b) {
      this.l = l;
      this.a = a;
      this.b = b;
    }

    public OkLabColor(Color linear) : this() {
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
    public bool Equals(OkLabColor other) {
      return l == other.l && a == other.a && b == other.b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object o) {
      return o is OkLabColor converted && Equals(converted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() {
      return (int)math.hash(this);
    }

    public static OkLabColor Lerp(OkLabColor a, OkLabColor b, float t) {
      return math.lerp(a, b, t);
    }

    public static explicit operator Color(OkLabColor color) {
      var comp = OkLabHelper.ToLinearRgb(color);
      return new Color(comp.x, comp.y, comp.z);
    }

    public static explicit operator OkLchColor(OkLabColor color) {
      return OkLabHelper.ToLch(color);
    }

    public static implicit operator float3(OkLabColor color) {
      return new float3(color.l, color.a, color.b);
    }

    public static implicit operator OkLabColor(float3 components) {
      return new OkLabColor(components.x, components.y, components.z);
    }
  }
}