using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
  public static class EasingExtensions {
    public static long ToMilliseconds(this TimeValue value) {
      return value.unit == TimeUnit.Millisecond ? (long)value.value : (long)(value.value * 1000);
    }

      /// <summary>
      ///   Evaluates an easing function at normalized time t (typically 0..1),
      ///   matching how Unity UI Toolkit / web (CSS) easings behave:
      ///   - Linear, ease, ease-in, ease-out, ease-in-out use CSS cubic-bezier curves.
      ///   - The named Sine/Cubic/Circ/Elastic/Back/Bounce use the same standard formulas
      ///   you’ll find in common web easing references (e.g., easings.net style).
      /// </summary>
      public static float Eval(this EasingMode mode, float t) {
      t = Mathf.Clamp01(t);
      return mode switch {
        // CSS timing functions:
        // https://developer.mozilla.org/en-US/docs/Web/CSS/easing-function
        EasingMode.Linear => t,
        EasingMode.Ease => CubicBezierYFromX(t, 0.25f, 0.10f, 0.25f, 1.00f),
        EasingMode.EaseIn => CubicBezierYFromX(t, 0.42f, 0.00f, 1.00f, 1.00f),
        EasingMode.EaseOut => CubicBezierYFromX(t, 0.00f, 0.00f, 0.58f, 1.00f),
        EasingMode.EaseInOut => CubicBezierYFromX(t, 0.42f, 0.00f, 0.58f, 1.00f),
        EasingMode.EaseInSine => 1f - Mathf.Cos(t * Mathf.PI * 0.5f),
        EasingMode.EaseOutSine => Mathf.Sin(t * Mathf.PI * 0.5f),
        EasingMode.EaseInOutSine => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f,
        EasingMode.EaseInCubic => t * t * t,
        EasingMode.EaseOutCubic => 1f - Mathf.Pow(1f - t, 3f),
        EasingMode.EaseInOutCubic => EaseInOutCubic(t),
        EasingMode.EaseInCirc => 1f - Mathf.Sqrt(1f - t * t),
        EasingMode.EaseOutCirc => Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f)),
        EasingMode.EaseInOutCirc => EaseInOutCirc(t),
        EasingMode.EaseInElastic => EaseInElastic(t),
        EasingMode.EaseOutElastic => EaseOutElastic(t),
        EasingMode.EaseInOutElastic => EaseInOutElastic(t),
        EasingMode.EaseInBack => EaseInBack(t),
        EasingMode.EaseOutBack => EaseOutBack(t),
        EasingMode.EaseInOutBack => EaseInOutBack(t),
        EasingMode.EaseInBounce => 1f - EaseOutBounce(1f - t),
        EasingMode.EaseOutBounce => EaseOutBounce(t),
        EasingMode.EaseInOutBounce => EaseInOutBounce(t),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };
    }

    // -----------------------
    // CSS cubic-bezier solver
    // -----------------------
    // We’re given x in [0..1] (time), need y.
    // Bezier endpoints are (0,0) and (1,1) with control points (x1,y1), (x2,y2).
    private static float CubicBezierYFromX(float x, float x1, float y1, float x2, float y2) {
      // Handle the common fast-path: linear-ish curves where x ≈ t
      if (Mathf.Approximately(x1, y1) && Mathf.Approximately(x2, y2)) return x;

      // Solve for parameter u where BezierX(u)=x, then return BezierY(u)
      var u = SolveBezierParameterForX(x, x1, x2);
      return Bezier(u, y1, y2);
    }

    private static float SolveBezierParameterForX(float x, float x1, float x2) {
      // Newton-Raphson with bisection fallback (standard web approach)
      const int newtonIters = 8;
      const float epsilon = 1e-6f;

      var u = x; // good initial guess for monotonic curves
      for (var i = 0; i < newtonIters; i++) {
        var xAtU = Bezier(u, x1, x2);
        var dx = xAtU - x;
        if (Mathf.Abs(dx) < epsilon) return u;

        var d = BezierDerivative(u, x1, x2);
        if (Mathf.Abs(d) < 1e-6f) break;

        u -= dx / d;
        u = u switch {
          < 0f => 0f,
          > 1f => 1f,
          _ => u
        };
      }

      // Bisection
      float lo = 0f, hi = 1f;
      u = x;
      for (var i = 0; i < 16; i++) {
        var xAtU = Bezier(u, x1, x2);
        var dx = xAtU - x;
        if (Mathf.Abs(dx) < epsilon) return u;

        if (dx > 0f) hi = u;
        else lo = u;

        u = (lo + hi) * 0.5f;
      }

      return u;
    }

    // Bezier for one dimension with endpoints 0 and 1 and control points p1, p2
    private static float Bezier(float u, float p1, float p2) {
      var inv = 1f - u;
      // 3(1-u)^2 u p1 + 3(1-u) u^2 p2 + u^3
      return 3f * inv * inv * u * p1 + 3f * inv * u * u * p2 + u * u * u;
    }

    private static float BezierDerivative(float u, float p1, float p2) {
      var inv = 1f - u;
      // derivative of: 3(1-u)^2 u p1 + 3(1-u) u^2 p2 + u^3
      return 3f * inv * inv * p1
             + 6f * inv * u * (p2 - p1)
             + 3f * u * u * (1f - p2);
    }

    // -----------------------
    // Elastic / Back / Bounce
    // -----------------------
    private static float EaseInElastic(float t) {
      if (t == 0f) return 0f;
      if (Mathf.Approximately(t, 1f)) return 1f;
      const float c4 = 2f * Mathf.PI / 3f;
      return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4);
    }

    private static float EaseOutElastic(float t) {
      if (t == 0f) return 0f;
      if (Mathf.Approximately(t, 1f)) return 1f;
      const float c4 = 2f * Mathf.PI / 3f;
      return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    private static float EaseInOutElastic(float t) {
      if (t == 0f) return 0f;
      if (Mathf.Approximately(t, 1f)) return 1f;
      const float c5 = 2f * Mathf.PI / 4.5f;
      if (t < 0.5f) return -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) * 0.5f;
      return Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5) * 0.5f + 1f;
    }

    private static float EaseInBack(float t) {
      const float c1 = 1.70158f;
      const float c3 = c1 + 1f;
      return c3 * t * t * t - c1 * t * t;
    }

    private static float EaseOutBack(float t) {
      const float c1 = 1.70158f;
      const float c3 = c1 + 1f;
      var u = t - 1f;
      return 1f + c3 * (u * u * u) + c1 * (u * u);
    }

    private static float EaseInOutBack(float t) {
      const float c1 = 1.70158f;
      const float c2 = c1 * 1.525f;

      if (t < 0.5f) {
        var u = 2f * t;
        return u * u * ((c2 + 1f) * u - c2) * 0.5f;
      } else {
        var u = 2f * t - 2f;
        return (u * u * ((c2 + 1f) * u + c2) + 2f) * 0.5f;
      }
    }

    private static float EaseOutBounce(float t) {
      const float n1 = 7.5625f;
      const float d1 = 2.75f;

      switch (t) {
        case < 1f / d1: return n1 * t * t;
        case < 2f / d1:
          t -= 1.5f / d1;
          return n1 * t * t + 0.75f;
        case < 2.5f / d1:
          t -= 2.25f / d1;
          return n1 * t * t + 0.9375f;
        default:
          t -= 2.625f / d1;
          return n1 * t * t + 0.984375f;
      }
    }

    private static float EaseInOutBounce(float t) {
      return t < 0.5f
        ? (1f - EaseOutBounce(1f - 2f * t)) * 0.5f
        : (1f + EaseOutBounce(2f * t - 1f)) * 0.5f;
    }

    private static float EaseInOutCirc(float t) {
      return t < 0.5f
        ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) * 0.5f
        : (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) * 0.5f;
    }

    private static float EaseInOutCubic(float t) {
      return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
    }
  }
}