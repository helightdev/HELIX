using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///     Utility methods for mathematical operations.
    /// </summary>
    internal static class MathUtils {
        /// <summary>
        ///     The signum function.
        ///     Returns 1 if num &gt; 0, -1 if num &lt; 0, and 0 if num = 0.
        /// </summary>
        public static int Signum(double num) {
            if (num < 0.0) return -1;
            if (num == 0.0) return 0;
            return 1;
        }
        
        /// <summary>
        ///     Returns true if a and b are approximately equal.
        /// </summary>
        public static bool ApproximatelyEqual(double a, double b, double tolerance = math.EPSILON_DBL) {
            return math.abs(a - b) <= tolerance;
        }

        /// <summary>
        ///     Linear interpolation.
        ///     Returns start if amount = 0 and stop if amount = 1.
        /// </summary>
        public static double Lerp(double start, double stop, double amount) {
            return (1.0 - amount) * start + amount * stop;
        }

        /// <summary>
        ///     Clamps an integer between min and max.
        /// </summary>
        public static int ClampInt(int min, int max, int input) {
            return math.clamp(input, min, max);
        }

        /// <summary>
        ///     Clamps a floating-point value between min and max.
        /// </summary>
        public static double ClampDouble(double min, double max, double input) {
            return math.clamp(input, min, max);
        }

        /// <summary>
        ///     Sanitizes a degree measure as an integer to [0, 360).
        /// </summary>
        public static int SanitizeDegreesInt(int degrees) {
            degrees %= 360;
            if (degrees < 0) degrees += 360;

            return degrees;
        }

        /// <summary>
        ///     Sanitizes a degree measure as a floating-point value to [0, 360).
        /// </summary>
        public static double SanitizeDegreesDouble(double degrees) {
            degrees %= 360.0;
            if (degrees < 0.0) degrees += 360.0;

            return degrees;
        }

        /// <summary>
        ///     Sign of direction change needed to travel from one angle to another.
        ///     Returns -1 if decreasing is shorter, 1 if increasing is shorter.
        ///     Returns 1 for exactly 180 degrees apart.
        /// </summary>
        public static double RotationDirection(double from, double to) {
            var increasingDifference = SanitizeDegreesDouble(to - from);
            return increasingDifference <= 180.0 ? 1.0 : -1.0;
        }

        /// <summary>
        ///     Distance of two points on a circle, represented in degrees.
        /// </summary>
        public static double DifferenceDegrees(double a, double b) {
            return 180.0 - math.abs(math.abs(a - b) - 180.0);
        }

        /// <summary>
        ///     Multiplies a 1x3 row vector with a 3x3 matrix.
        ///     This preserves the original Dart implementation exactly.
        ///     Given a source matrix expressed as rows:
        ///     [r0]
        ///     [r1]
        ///     [r2]
        ///     the result is:
        ///     [dot(row, r0), dot(row, r1), dot(row, r2)]
        ///     The matrices in this port are stored transposed in Unity's column-major double3x3
        ///     so math.mul(matrix, row) reproduces the Dart behavior exactly.
        /// </summary>
        public static double3 MatrixMultiply(double3 row, double3x3 matrix) {
            return math.mul(matrix, row);
        }
    }
}