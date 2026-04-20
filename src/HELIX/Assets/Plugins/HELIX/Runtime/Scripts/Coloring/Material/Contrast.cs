using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Utility methods for calculating contrast given two colors,
    ///   or calculating a color given one color and a contrast ratio.
    /// </summary>
    public static class Contrast {
        /// <summary>
        ///   Returns a contrast ratio, which ranges from 1 to 21.
        /// </summary>
        public static double RatioOfTones(double toneA, double toneB) {
      toneA = MathUtils.ClampDouble(0.0, 100.0, toneA);
      toneB = MathUtils.ClampDouble(0.0, 100.0, toneB);

      return RatioOfYs(
        MaterialColorUtils.YFromLstar(toneA),
        MaterialColorUtils.YFromLstar(toneB)
      );
    }

    private static double RatioOfYs(double y1, double y2) {
      var lighter = y1 > y2 ? y1 : y2;
      var darker = MathUtils.ApproximatelyEqual(lighter, y2) ? y1 : y2;
      return (lighter + 5.0) / (darker + 5.0);
    }

        /// <summary>
        ///   Returns a tone &gt;= tone that ensures ratio.
        ///   Returns -1 if ratio cannot be achieved.
        /// </summary>
        public static double Lighter(double tone, double ratio) {
      if (tone < 0.0 || tone > 100.0) return -1.0;

      var darkY = MaterialColorUtils.YFromLstar(tone);
      var lightY = ratio * (darkY + 5.0) - 5.0;
      var realContrast = RatioOfYs(lightY, darkY);
      var delta = math.abs(realContrast - ratio);

      if (realContrast < ratio && delta > 0.04) return -1.0;

      var returnValue = MaterialColorUtils.LstarFromY(lightY) + 0.4;
      if (returnValue < 0.0 || returnValue > 100.0) return -1.0;

      return returnValue;
    }

        /// <summary>
        ///   Returns a tone &lt;= tone that ensures ratio.
        ///   Returns -1 if ratio cannot be achieved.
        /// </summary>
        public static double Darker(double tone, double ratio) {
      if (tone < 0.0 || tone > 100.0) return -1.0;

      var lightY = MaterialColorUtils.YFromLstar(tone);
      var darkY = (lightY + 5.0) / ratio - 5.0;
      var realContrast = RatioOfYs(lightY, darkY);
      var delta = math.abs(realContrast - ratio);

      if (realContrast < ratio && delta > 0.04) return -1.0;

      var returnValue = MaterialColorUtils.LstarFromY(darkY) - 0.4;
      if (returnValue < 0.0 || returnValue > 100.0) return -1.0;

      return returnValue;
    }

        /// <summary>
        ///   Unsafe lighter version. Returns 100 if ratio cannot be achieved.
        /// </summary>
        public static double LighterUnsafe(double tone, double ratio) {
      var lighterSafe = Lighter(tone, ratio);
      return lighterSafe < 0.0 ? 100.0 : lighterSafe;
    }

        /// <summary>
        ///   Unsafe darker version. Returns 0 if ratio cannot be achieved.
        /// </summary>
        public static double DarkerUnsafe(double tone, double ratio) {
      var darkerSafe = Darker(tone, ratio);
      return darkerSafe < 0.0 ? 0.0 : darkerSafe;
    }
  }
}