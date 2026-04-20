using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Functions for blending in HCT and CAM16.
    /// </summary>
    public static class Blend {
        /// <summary>
        ///   Blend the design color's HCT hue towards the key color's HCT hue.
        /// </summary>
        public static int Harmonize(int designColor, int sourceColor) {
      var fromHct = Hct.FromInt(designColor);
      var toHct = Hct.FromInt(sourceColor);

      var differenceDegrees = MathUtils.DifferenceDegrees(fromHct.Hue, toHct.Hue);
      var rotationDegrees = math.min(differenceDegrees * 0.5, 15.0);

      var outputHue = MathUtils.SanitizeDegreesDouble(
        fromHct.Hue
        + rotationDegrees * MathUtils.RotationDirection(fromHct.Hue, toHct.Hue)
      );

      return Hct.From(outputHue, fromHct.Chroma, fromHct.Tone).ToInt();
    }

        /// <summary>
        ///   Blends hue from one color into another. Chroma and tone of the original are maintained.
        /// </summary>
        public static int HctHue(int from, int to, double amount) {
      var ucs = Cam16Ucs(from, to, amount);
      var ucsCam = Cam16.FromInt(ucs);
      var fromCam = Cam16.FromInt(from);

      var blended = Hct.From(
        ucsCam.Hue,
        fromCam.Chroma,
        MaterialColorUtils.LstarFromArgb(from)
      );

      return blended.ToInt();
    }

        /// <summary>
        ///   Blend in CAM16-UCS space.
        /// </summary>
        public static int Cam16Ucs(int from, int to, double amount) {
      var fromCam = Cam16.FromInt(from);
      var toCam = Cam16.FromInt(to);

      var jstar = fromCam.Jstar + (toCam.Jstar - fromCam.Jstar) * amount;
      var astar = fromCam.Astar + (toCam.Astar - fromCam.Astar) * amount;
      var bstar = fromCam.Bstar + (toCam.Bstar - fromCam.Bstar) * amount;

      return Cam16.FromUcs(jstar, astar, bstar).ToInt();
    }
  }
}