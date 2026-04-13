using Unity.Mathematics;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Color science utilities.
    ///     Utility methods for color science constants and color space
    ///     conversions that aren't HCT or CAM16.
    /// </summary>
    public static class MaterialColorUtils {
        // Stored transposed to match the Dart row-list representation while using Unity's column-major matrices.
        private static readonly double3x3 _srgbToXyz = new(
            new double3(0.41233895, 0.2126, 0.01932141),
            new double3(0.35762064, 0.7152, 0.11916382),
            new double3(0.18051042, 0.0722, 0.95034478)
        );

        // Stored transposed to match the Dart row-list representation while using Unity's column-major matrices.
        private static readonly double3x3 _xyzToSrgb = new(
            new double3(3.2413774792388685, -0.9691452513005321, 0.05562093689691305),
            new double3(-1.5376652402851851, 1.8758853451067872, -0.20395524564742123),
            new double3(-0.49885366846268053, 0.04156585616912061, 1.0571799111220335)
        );

        private static readonly double3 _whitePointD65Value = new(95.047, 100.0, 108.883);
        
        /// <summary>
        ///     Converts a color from RGB components to ARGB format.
        /// </summary>
        public static int ArgbFromRgb(int red, int green, int blue) {
            return (255 << 24)
                 | ((red & 255) << 16)
                 | ((green & 255) << 8)
                 | (blue & 255);
        }

        /// <summary>
        ///     Converts a color from linear RGB components to ARGB format.
        ///     Expects linear RGB in the same 0..100 domain as the Dart implementation.
        /// </summary>
        public static int ArgbFromLinrgb(double3 linrgb) {
            var r = Delinearized(linrgb.x);
            var g = Delinearized(linrgb.y);
            var b = Delinearized(linrgb.z);
            return ArgbFromRgb(r, g, b);
        }

        /// <summary>
        ///     Returns the alpha component of a color in ARGB format.
        /// </summary>
        public static int AlphaFromArgb(int argb) {
            return (argb >> 24) & 255;
        }

        /// <summary>
        ///     Returns the red component of a color in ARGB format.
        /// </summary>
        public static int RedFromArgb(int argb) {
            return (argb >> 16) & 255;
        }

        /// <summary>
        ///     Returns the green component of a color in ARGB format.
        /// </summary>
        public static int GreenFromArgb(int argb) {
            return (argb >> 8) & 255;
        }

        /// <summary>
        ///     Returns the blue component of a color in ARGB format.
        /// </summary>
        public static int BlueFromArgb(int argb) {
            return argb & 255;
        }

        /// <summary>
        ///     Returns whether a color in ARGB format is opaque.
        /// </summary>
        public static bool IsOpaque(int argb) {
            return AlphaFromArgb(argb) >= 255;
        }

        /// <summary>
        ///     Converts a color from XYZ to ARGB.
        /// </summary>
        public static int ArgbFromXyz(double x, double y, double z) {
            var xyz = new double3(x, y, z);
            var linearRgb = math.mul(_xyzToSrgb, xyz);

            var r = Delinearized(linearRgb.x);
            var g = Delinearized(linearRgb.y);
            var b = Delinearized(linearRgb.z);

            return ArgbFromRgb(r, g, b);
        }

        /// <summary>
        ///     Converts a color from ARGB to XYZ.
        /// </summary>
        public static double3 XyzFromArgb(int argb) {
            var r = Linearized(RedFromArgb(argb));
            var g = Linearized(GreenFromArgb(argb));
            var b = Linearized(BlueFromArgb(argb));

            return MathUtils.MatrixMultiply(new double3(r, g, b), _srgbToXyz);
        }

        /// <summary>
        ///     Converts a color represented in Lab color space into an ARGB integer.
        /// </summary>
        public static int ArgbFromLab(double l, double a, double b) {
            var fy = (l + 16.0) / 116.0;
            var fx = a / 500.0 + fy;
            var fz = fy - b / 200.0;

            var xNormalized = LabInvf(fx);
            var yNormalized = LabInvf(fy);
            var zNormalized = LabInvf(fz);

            var x = xNormalized * _whitePointD65Value.x;
            var y = yNormalized * _whitePointD65Value.y;
            var z = zNormalized * _whitePointD65Value.z;

            return ArgbFromXyz(x, y, z);
        }

        /// <summary>
        ///     Converts a color from ARGB representation to L*a*b* representation.
        /// </summary>
        public static double3 LabFromArgb(int argb) {
            var linearR = Linearized(RedFromArgb(argb));
            var linearG = Linearized(GreenFromArgb(argb));
            var linearB = Linearized(BlueFromArgb(argb));

            var xyz = math.mul(_srgbToXyz, new double3(linearR, linearG, linearB));

            var xNormalized = xyz.x / _whitePointD65Value.x;
            var yNormalized = xyz.y / _whitePointD65Value.y;
            var zNormalized = xyz.z / _whitePointD65Value.z;

            var fx = LabF(xNormalized);
            var fy = LabF(yNormalized);
            var fz = LabF(zNormalized);

            var l = 116.0 * fy - 16.0;
            var a = 500.0 * (fx - fy);
            var b = 200.0 * (fy - fz);

            return new double3(l, a, b);
        }

        /// <summary>
        ///     Converts an L* value to an ARGB representation.
        ///     Returns a grayscale color with matching lightness.
        /// </summary>
        public static int ArgbFromLstar(double lstar) {
            var y = YFromLstar(lstar);
            var component = Delinearized(y);
            return ArgbFromRgb(component, component, component);
        }

        /// <summary>
        ///     Computes the L* value of a color in ARGB representation.
        /// </summary>
        public static double LstarFromArgb(int argb) {
            var y = XyzFromArgb(argb).y;
            return 116.0 * LabF(y / 100.0) - 16.0;
        }

        /// <summary>
        ///     Converts an L* value to a Y value.
        /// </summary>
        public static double YFromLstar(double lstar) {
            return 100.0 * LabInvf((lstar + 16.0) / 116.0);
        }

        /// <summary>
        ///     Converts a Y value to an L* value.
        /// </summary>
        public static double LstarFromY(double y) {
            return LabF(y / 100.0) * 116.0 - 16.0;
        }

        /// <summary>
        ///     Linearizes an RGB component.
        ///     Input:  0..255
        ///     Output: 0..100
        /// </summary>
        public static double Linearized(int rgbComponent) {
            var normalized = rgbComponent / 255.0;

            if (normalized <= 0.040449936) return normalized / 12.92 * 100.0;

            return math.pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
        }

        /// <summary>
        ///     Delinearizes an RGB component.
        ///     Input:  0..100
        ///     Output: 0..255
        /// </summary>
        public static int Delinearized(double rgbComponent) {
            var normalized = rgbComponent / 100.0;
            double delinearized;

            if (normalized <= 0.0031308) delinearized = normalized * 12.92;
            else delinearized = 1.055 * math.pow(normalized, 1.0 / 2.4) - 0.055;

            return MathUtils.ClampInt(0, 255, (int)math.round(delinearized * 255.0));
        }

        /// <summary>
        ///     Returns the standard white point; white on a sunny day.
        /// </summary>
        public static double3 WhitePointD65() {
            return _whitePointD65Value;
        }

        private static double LabF(double t) {
            const double e = 216.0 / 24389.0;
            const double kappa = 24389.0 / 27.0;

            if (t > e) return math.pow(t, 1.0 / 3.0);

            return (kappa * t + 16.0) / 116.0;
        }

        private static double LabInvf(double ft) {
            const double e = 216.0 / 24389.0;
            const double kappa = 24389.0 / 27.0;

            var ft3 = ft * ft * ft;
            if (ft3 > e) return ft3;

            return (116.0 * ft - 16.0) / kappa;
        }
    }
}