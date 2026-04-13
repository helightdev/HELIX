using System;
using System.Globalization;
using Unity.Mathematics;

namespace MaterialColorUtilities {
    /// <summary>
    /// Utility methods for mathematical operations.
    /// </summary>
    public static class MathUtils {
        /// <summary>
        /// The signum function.
        /// Returns 1 if num &gt; 0, -1 if num &lt; 0, and 0 if num = 0.
        /// </summary>
        public static int Signum(double num) {
            if (num < 0.0) return -1;
            if (num == 0.0) return 0;
            return 1;
        }

        /// <summary>
        /// Linear interpolation.
        /// Returns start if amount = 0 and stop if amount = 1.
        /// </summary>
        public static double Lerp(double start, double stop, double amount) {
            return ((1.0 - amount) * start) + (amount * stop);
        }

        /// <summary>
        /// Clamps an integer between min and max.
        /// </summary>
        public static int ClampInt(int min, int max, int input) {
            return math.clamp(input, min, max);
        }

        /// <summary>
        /// Clamps a floating-point value between min and max.
        /// </summary>
        public static double ClampDouble(double min, double max, double input) {
            return math.clamp(input, min, max);
        }

        /// <summary>
        /// Sanitizes a degree measure as an integer to [0, 360).
        /// </summary>
        public static int SanitizeDegreesInt(int degrees) {
            degrees %= 360;
            if (degrees < 0) { degrees += 360; }

            return degrees;
        }

        /// <summary>
        /// Sanitizes a degree measure as a floating-point value to [0, 360).
        /// </summary>
        public static double SanitizeDegreesDouble(double degrees) {
            degrees %= 360.0;
            if (degrees < 0.0) { degrees += 360.0; }

            return degrees;
        }

        /// <summary>
        /// Sign of direction change needed to travel from one angle to another.
        /// Returns -1 if decreasing is shorter, 1 if increasing is shorter.
        /// Returns 1 for exactly 180 degrees apart.
        /// </summary>
        public static double RotationDirection(double from, double to) {
            double increasingDifference = SanitizeDegreesDouble(to - from);
            return increasingDifference <= 180.0 ? 1.0 : -1.0;
        }

        /// <summary>
        /// Distance of two points on a circle, represented in degrees.
        /// </summary>
        public static double DifferenceDegrees(double a, double b) {
            return 180.0 - math.abs(math.abs(a - b) - 180.0);
        }

        /// <summary>
        /// Multiplies a 1x3 row vector with a 3x3 matrix.
        ///
        /// This preserves the original Dart implementation exactly.
        /// Given a source matrix expressed as rows:
        ///   [r0]
        ///   [r1]
        ///   [r2]
        /// the result is:
        ///   [dot(row, r0), dot(row, r1), dot(row, r2)]
        ///
        /// The matrices in this port are stored transposed in Unity's column-major double3x3
        /// so math.mul(matrix, row) reproduces the Dart behavior exactly.
        /// </summary>
        public static double3 MatrixMultiply(double3 row, double3x3 matrix) {
            return math.mul(matrix, row);
        }
    }

    /// <summary>
    /// Color science utilities.
    ///
    /// Utility methods for color science constants and color space
    /// conversions that aren't HCT or CAM16.
    /// </summary>
    public static class ColorUtils {
        // Stored transposed to match the Dart row-list representation while using Unity's column-major matrices.
        private static readonly double3x3 SrgbToXyz = new double3x3(
            new double3(0.41233895, 0.2126, 0.01932141),
            new double3(0.35762064, 0.7152, 0.11916382),
            new double3(0.18051042, 0.0722, 0.95034478)
        );

        // Stored transposed to match the Dart row-list representation while using Unity's column-major matrices.
        private static readonly double3x3 XyzToSrgb = new double3x3(
            new double3(3.2413774792388685, -0.9691452513005321, 0.05562093689691305),
            new double3(-1.5376652402851851, 1.8758853451067872, -0.20395524564742123),
            new double3(-0.49885366846268053, 0.04156585616912061, 1.0571799111220335)
        );

        private static readonly double3 WhitePointD65Value = new double3(95.047, 100.0, 108.883);

        /// <summary>
        /// Converts a color from RGB components to ARGB format.
        /// </summary>
        public static int ArgbFromRgb(int red, int green, int blue) {
            return (255 << 24)
                 | ((red & 255) << 16)
                 | ((green & 255) << 8)
                 | (blue & 255);
        }

        /// <summary>
        /// Converts a color from linear RGB components to ARGB format.
        /// Expects linear RGB in the same 0..100 domain as the Dart implementation.
        /// </summary>
        public static int ArgbFromLinrgb(double3 linrgb) {
            int r = Delinearized(linrgb.x);
            int g = Delinearized(linrgb.y);
            int b = Delinearized(linrgb.z);
            return ArgbFromRgb(r, g, b);
        }

        /// <summary>
        /// Returns the alpha component of a color in ARGB format.
        /// </summary>
        public static int AlphaFromArgb(int argb) {
            return (argb >> 24) & 255;
        }

        /// <summary>
        /// Returns the red component of a color in ARGB format.
        /// </summary>
        public static int RedFromArgb(int argb) {
            return (argb >> 16) & 255;
        }

        /// <summary>
        /// Returns the green component of a color in ARGB format.
        /// </summary>
        public static int GreenFromArgb(int argb) {
            return (argb >> 8) & 255;
        }

        /// <summary>
        /// Returns the blue component of a color in ARGB format.
        /// </summary>
        public static int BlueFromArgb(int argb) {
            return argb & 255;
        }

        /// <summary>
        /// Returns whether a color in ARGB format is opaque.
        /// </summary>
        public static bool IsOpaque(int argb) {
            return AlphaFromArgb(argb) >= 255;
        }

        /// <summary>
        /// Converts a color from XYZ to ARGB.
        /// </summary>
        public static int ArgbFromXyz(double x, double y, double z) {
            double3 xyz = new double3(x, y, z);
            double3 linearRgb = math.mul(XyzToSrgb, xyz);

            int r = Delinearized(linearRgb.x);
            int g = Delinearized(linearRgb.y);
            int b = Delinearized(linearRgb.z);

            return ArgbFromRgb(r, g, b);
        }

        /// <summary>
        /// Converts a color from ARGB to XYZ.
        /// </summary>
        public static double3 XyzFromArgb(int argb) {
            double r = Linearized(RedFromArgb(argb));
            double g = Linearized(GreenFromArgb(argb));
            double b = Linearized(BlueFromArgb(argb));

            return MathUtils.MatrixMultiply(new double3(r, g, b), SrgbToXyz);
        }

        /// <summary>
        /// Converts a color represented in Lab color space into an ARGB integer.
        /// </summary>
        public static int ArgbFromLab(double l, double a, double b) {
            double fy = (l + 16.0) / 116.0;
            double fx = (a / 500.0) + fy;
            double fz = fy - (b / 200.0);

            double xNormalized = LabInvf(fx);
            double yNormalized = LabInvf(fy);
            double zNormalized = LabInvf(fz);

            double x = xNormalized * WhitePointD65Value.x;
            double y = yNormalized * WhitePointD65Value.y;
            double z = zNormalized * WhitePointD65Value.z;

            return ArgbFromXyz(x, y, z);
        }

        /// <summary>
        /// Converts a color from ARGB representation to L*a*b* representation.
        /// </summary>
        public static double3 LabFromArgb(int argb) {
            double linearR = Linearized(RedFromArgb(argb));
            double linearG = Linearized(GreenFromArgb(argb));
            double linearB = Linearized(BlueFromArgb(argb));

            double3 xyz = math.mul(SrgbToXyz, new double3(linearR, linearG, linearB));

            double xNormalized = xyz.x / WhitePointD65Value.x;
            double yNormalized = xyz.y / WhitePointD65Value.y;
            double zNormalized = xyz.z / WhitePointD65Value.z;

            double fx = LabF(xNormalized);
            double fy = LabF(yNormalized);
            double fz = LabF(zNormalized);

            double l = (116.0 * fy) - 16.0;
            double a = 500.0 * (fx - fy);
            double b = 200.0 * (fy - fz);

            return new double3(l, a, b);
        }

        /// <summary>
        /// Converts an L* value to an ARGB representation.
        /// Returns a grayscale color with matching lightness.
        /// </summary>
        public static int ArgbFromLstar(double lstar) {
            double y = YFromLstar(lstar);
            int component = Delinearized(y);
            return ArgbFromRgb(component, component, component);
        }

        /// <summary>
        /// Computes the L* value of a color in ARGB representation.
        /// </summary>
        public static double LstarFromArgb(int argb) {
            double y = XyzFromArgb(argb).y;
            return (116.0 * LabF(y / 100.0)) - 16.0;
        }

        /// <summary>
        /// Converts an L* value to a Y value.
        /// </summary>
        public static double YFromLstar(double lstar) {
            return 100.0 * LabInvf((lstar + 16.0) / 116.0);
        }

        /// <summary>
        /// Converts a Y value to an L* value.
        /// </summary>
        public static double LstarFromY(double y) {
            return (LabF(y / 100.0) * 116.0) - 16.0;
        }

        /// <summary>
        /// Linearizes an RGB component.
        ///
        /// Input:  0..255
        /// Output: 0..100
        /// </summary>
        public static double Linearized(int rgbComponent) {
            double normalized = rgbComponent / 255.0;

            if (normalized <= 0.040449936) { return (normalized / 12.92) * 100.0; }

            return math.pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
        }

        /// <summary>
        /// Delinearizes an RGB component.
        ///
        /// Input:  0..100
        /// Output: 0..255
        /// </summary>
        public static int Delinearized(double rgbComponent) {
            double normalized = rgbComponent / 100.0;
            double delinearized;

            if (normalized <= 0.0031308) { delinearized = normalized * 12.92; } else {
                delinearized = (1.055 * math.pow(normalized, 1.0 / 2.4)) - 0.055;
            }

            return MathUtils.ClampInt(0, 255, (int)math.round(delinearized * 255.0));
        }

        /// <summary>
        /// Returns the standard white point; white on a sunny day.
        /// </summary>
        public static double3 WhitePointD65() {
            return WhitePointD65Value;
        }

        private static double LabF(double t) {
            const double e = 216.0 / 24389.0;
            const double kappa = 24389.0 / 27.0;

            if (t > e) { return math.pow(t, 1.0 / 3.0); }

            return ((kappa * t) + 16.0) / 116.0;
        }

        private static double LabInvf(double ft) {
            const double e = 216.0 / 24389.0;
            const double kappa = 24389.0 / 27.0;

            double ft3 = ft * ft * ft;
            if (ft3 > e) { return ft3; }

            return ((116.0 * ft) - 16.0) / kappa;
        }
    }

    public static class StringUtils {
        public static string HexFromArgb(int argb, bool leadingHashSign = true) {
            int red = ColorUtils.RedFromArgb(argb);
            int green = ColorUtils.GreenFromArgb(argb);
            int blue = ColorUtils.BlueFromArgb(argb);

            return string.Concat(
                leadingHashSign ? "#" : string.Empty,
                red.ToString("X2", CultureInfo.InvariantCulture),
                green.ToString("X2", CultureInfo.InvariantCulture),
                blue.ToString("X2", CultureInfo.InvariantCulture)
            );
        }

        /// <summary>
        /// Preserves the original Dart behavior as closely as possible:
        /// strips '#' and parses the remaining hex directly.
        ///
        /// Note that this does NOT force alpha to 0xFF for 6-digit RGB input.
        /// For example, "#FF0000" becomes 0x00FF0000 numerically, matching the source behavior.
        /// </summary>
        public static int? ArgbFromHex(string hex) {
            if (string.IsNullOrEmpty(hex)) { return null; }

            string cleaned = hex.Replace("#", string.Empty);

            if (int.TryParse(cleaned, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value)) {
                return value;
            }

            return null;
        }
    }
}