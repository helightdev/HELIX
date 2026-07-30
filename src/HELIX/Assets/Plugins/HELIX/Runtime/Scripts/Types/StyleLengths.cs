using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
  public class StyleLengths {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StyleLength Add(StyleLength a, StyleLength b) {
      return Precondition(a, b, out var escape) ? escape : new StyleLength(Add(a.value, b.value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StyleLength Subtract(StyleLength a, StyleLength b) {
      return Precondition(a, b, out var escape) ? escape : new StyleLength(Subtract(a.value, b.value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StyleLength Multiply(StyleLength a, StyleLength b) {
      return Precondition(a, b, out var escape) ? escape : new StyleLength(Multiply(a.value, b.value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StyleLength Divide(StyleLength a, StyleLength b) {
      return Precondition(a, b, out var escape) ? escape : new StyleLength(Divide(a.value, b.value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StyleLength Negate(StyleLength a) {
      return a.keyword != StyleKeyword.Undefined ? a : new StyleLength(Negate(a.value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StyleLength Abs(StyleLength a) {
      return a.keyword != StyleKeyword.Undefined ? a : new StyleLength(Abs(a.value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StyleLength Interpolate(StyleLength a, StyleLength b, float t) {
      return Precondition(a, b, out var escape) ? escape : new StyleLength(Interpolate(a.value, b.value, t));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Precondition(StyleLength a, StyleLength b, out StyleLength escape) {
      escape = default;
      if (a.keyword != StyleKeyword.Undefined) {
        escape = b;
        return true;
      }
      if (b.keyword != StyleKeyword.Undefined) {
        escape = a;
        return true;
      }
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckUnits(Length a, Length b) {
      if (a.unit == b.unit) return true;
      Debug.LogWarning($"Cannot operate on Lengths with different units: {a} and {b}.");
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Add(Length a, Length b) {
      return CheckUnits(a, b) ? new Length(a.value + b.value, a.unit) : a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Subtract(Length a, Length b) {
      return CheckUnits(a, b) ? new Length(a.value - b.value, a.unit) : a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Multiply(Length a, Length b) {
      return CheckUnits(a, b) ? new Length(a.value * b.value, a.unit) : a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Divide(Length a, Length b) {
      return CheckUnits(a, b) ? new Length(a.value / b.value, a.unit) : a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Negate(Length a) => new(-a.value, a.unit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Abs(Length a) => new(Mathf.Abs(a.value), a.unit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Interpolate(Length a, Length b, float t) {
      return CheckUnits(a, b) ? new Length(Mathf.Lerp(a.value, b.value, t), a.unit) : a;
    }
  }
}