using System.Globalization;

namespace HELIX.Coloring.Material {
  public static class StringUtils {
    public static string HexFromArgb(int argb, bool leadingHashSign = true) {
      var red = MaterialColorUtils.RedFromArgb(argb);
      var green = MaterialColorUtils.GreenFromArgb(argb);
      var blue = MaterialColorUtils.BlueFromArgb(argb);

      return string.Concat(
        leadingHashSign ? "#" : string.Empty,
        red.ToString("X2", CultureInfo.InvariantCulture),
        green.ToString("X2", CultureInfo.InvariantCulture),
        blue.ToString("X2", CultureInfo.InvariantCulture)
      );
    }

      /// <summary>
      ///   Preserves the original Dart behavior as closely as possible:
      ///   strips '#' and parses the remaining hex directly.
      ///   Note that this does NOT force alpha to 0xFF for 6-digit RGB input.
      ///   For example, "#FF0000" becomes 0x00FF0000 numerically, matching the source behavior.
      /// </summary>
      public static int? ArgbFromHex(string hex) {
      if (string.IsNullOrEmpty(hex)) return null;

      var cleaned = hex.Replace("#", string.Empty);

      if (int.TryParse(
            cleaned,
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out var value
          )) return value;

      return null;
    }
  }
}