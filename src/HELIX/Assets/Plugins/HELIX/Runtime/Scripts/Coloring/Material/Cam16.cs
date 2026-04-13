using Unity.Mathematics;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     CAM16, a color appearance model.
    /// </summary>
    public sealed class Cam16 {
        // XYZ -> CAM16 RGB "cone" response matrix.
        // Stored transposed for Unity's column-major double3x3 so math.mul(M, xyz)
        // matches the Dart row-based equations.
        private static readonly double3x3 _xyzToCam16Rgb = new(
            new double3(0.401288, -0.250268, -0.002079),
            new double3(0.650173, 1.204414, 0.048952),
            new double3(-0.051461, 0.045854, 0.953127)
        );

        // CAM16 RGB -> XYZ matrix.
        // Stored transposed for Unity's column-major double3x3.
        private static readonly double3x3 _cam16RgbToXyz = new(
            new double3(1.86206786, 0.38752654, -0.01584150),
            new double3(-1.01125463, 0.62144744, -0.03412294),
            new double3(0.14918677, -0.00897398, 1.04996444)
        );

        public Cam16(
            double hue,
            double chroma,
            double j,
            double q,
            double m,
            double s,
            double jstar,
            double astar,
            double bstar
        ) {
            Hue = hue;
            Chroma = chroma;
            J = j;
            Q = q;
            M = m;
            S = s;
            Jstar = jstar;
            Astar = astar;
            Bstar = bstar;
        }

        public double Hue { get; }
        public double Chroma { get; }
        public double J { get; }
        public double Q { get; }
        public double M { get; }
        public double S { get; }
        public double Jstar { get; }
        public double Astar { get; }
        public double Bstar { get; }

        public double Distance(Cam16 other) {
            var dJ = Jstar - other.Jstar;
            var dA = Astar - other.Astar;
            var dB = Bstar - other.Bstar;
            var dEPrime = math.sqrt(dJ * dJ + dA * dA + dB * dB);
            return 1.41 * math.pow(dEPrime, 0.63);
        }

        public static Cam16 FromInt(int argb) {
            return FromIntInViewingConditions(argb, ViewingConditions.SRgb);
        }

        public static Cam16 FromIntInViewingConditions(int argb, ViewingConditions viewingConditions) {
            var xyz = MaterialColorUtils.XyzFromArgb(argb);
            return FromXyzInViewingConditions(xyz.x, xyz.y, xyz.z, viewingConditions);
        }

        public static Cam16 FromXyzInViewingConditions(
            double x,
            double y,
            double z,
            ViewingConditions viewingConditions
        ) {
            var rgbC = math.mul(_xyzToCam16Rgb, new double3(x, y, z));
            var rgbD = rgbC * viewingConditions.RgbD;

            var rAf = math.pow(viewingConditions.Fl * math.abs(rgbD.x) / 100.0, 0.42);
            var gAf = math.pow(viewingConditions.Fl * math.abs(rgbD.y) / 100.0, 0.42);
            var bAf = math.pow(viewingConditions.Fl * math.abs(rgbD.z) / 100.0, 0.42);

            var rA = MathUtils.Signum(rgbD.x) * 400.0 * rAf / (rAf + 27.13);
            var gA = MathUtils.Signum(rgbD.y) * 400.0 * gAf / (gAf + 27.13);
            var bA = MathUtils.Signum(rgbD.z) * 400.0 * bAf / (bAf + 27.13);

            var a = (11.0 * rA + -12.0 * gA + bA) / 11.0;
            var b = (rA + gA - 2.0 * bA) / 9.0;

            var u = (20.0 * rA + 20.0 * gA + 21.0 * bA) / 20.0;
            var p2 = (40.0 * rA + 20.0 * gA + bA) / 20.0;

            var atanDegrees = math.degrees(math.atan2(b, a));
            var hue = MathUtils.SanitizeDegreesDouble(atanDegrees);
            var hueRadians = math.radians(hue);

            var ac = p2 * viewingConditions.Nbb;

            var j = 100.0 * math.pow(ac / viewingConditions.Aw, viewingConditions.C * viewingConditions.Z);
            var q = 4.0 / viewingConditions.C
                  * math.sqrt(j / 100.0)
                  * (viewingConditions.Aw + 4.0)
                  * viewingConditions.FlRoot;

            var huePrime = hue < 20.14 ? hue + 360.0 : hue;
            var eHue = 0.25 * (math.cos(math.radians(huePrime) + 2.0) + 3.8);
            var p1 = 50000.0 / 13.0 * eHue * viewingConditions.Nc * viewingConditions.Ncb;
            var t = p1 * math.sqrt(a * a + b * b) / (u + 0.305);

            var alpha = math.pow(t, 0.9)
                      * math.pow(1.64 - math.pow(0.29, viewingConditions.BackgroundYTowhitePointY), 0.73);

            var c = alpha * math.sqrt(j / 100.0);
            var m = c * viewingConditions.FlRoot;
            var s = 50.0 * math.sqrt(alpha * viewingConditions.C / (viewingConditions.Aw + 4.0));

            var jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
            var mstar = math.log(1.0 + 0.0228 * m) / 0.0228;
            var astar = mstar * math.cos(hueRadians);
            var bstar = mstar * math.sin(hueRadians);

            return new Cam16(hue, c, j, q, m, s, jstar, astar, bstar);
        }

        public static Cam16 FromJch(double j, double c, double h) {
            return FromJchInViewingConditions(j, c, h, ViewingConditions.SRgb);
        }

        public static Cam16 FromJchInViewingConditions(
            double j,
            double c,
            double h,
            ViewingConditions viewingConditions
        ) {
            var q = 4.0 / viewingConditions.C
                  * math.sqrt(j / 100.0)
                  * (viewingConditions.Aw + 4.0)
                  * viewingConditions.FlRoot;

            var m = c * viewingConditions.FlRoot;
            var alpha = c / math.sqrt(j / 100.0);
            var s = 50.0 * math.sqrt(alpha * viewingConditions.C / (viewingConditions.Aw + 4.0));

            var hueRadians = math.radians(h);
            var jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
            var mstar = math.log(1.0 + 0.0228 * m) / 0.0228;
            var astar = mstar * math.cos(hueRadians);
            var bstar = mstar * math.sin(hueRadians);

            return new Cam16(h, c, j, q, m, s, jstar, astar, bstar);
        }

        public static Cam16 FromUcs(double jstar, double astar, double bstar) {
            return FromUcsInViewingConditions(jstar, astar, bstar, ViewingConditions.Standard);
        }

        public static Cam16 FromUcsInViewingConditions(
            double jstar,
            double astar,
            double bstar,
            ViewingConditions viewingConditions
        ) {
            var m = math.sqrt(astar * astar + bstar * bstar);
            var bigM = (math.exp(m * 0.0228) - 1.0) / 0.0228;
            var c = bigM / viewingConditions.FlRoot;

            var h = math.degrees(math.atan2(bstar, astar));
            if (h < 0.0) h += 360.0;

            var j = jstar / (1.0 - (jstar - 100.0) * 0.007);
            return FromJchInViewingConditions(j, c, h, viewingConditions);
        }

        public int ToInt() {
            return Viewed(ViewingConditions.SRgb);
        }

        public int Viewed(ViewingConditions viewingConditions) {
            var xyz = XyzInViewingConditions(viewingConditions);
            return MaterialColorUtils.ArgbFromXyz(xyz.x, xyz.y, xyz.z);
        }

        public double3 XyzInViewingConditions(ViewingConditions viewingConditions) {
            var alpha = Chroma == 0.0 || J == 0.0
                ? 0.0
                : Chroma / math.sqrt(J / 100.0);

            var t = math.pow(
                alpha / math.pow(
                    1.64 - math.pow(0.29, viewingConditions.BackgroundYTowhitePointY),
                    0.73
                ),
                1.0 / 0.9
            );

            var hRad = math.radians(Hue);
            var eHue = 0.25 * (math.cos(hRad + 2.0) + 3.8);
            var ac = viewingConditions.Aw
                   * math.pow(J / 100.0, 1.0 / viewingConditions.C / viewingConditions.Z);

            var p1 = eHue * (50000.0 / 13.0) * viewingConditions.Nc * viewingConditions.Ncb;
            var p2 = ac / viewingConditions.Nbb;

            var hSin = math.sin(hRad);
            var hCos = math.cos(hRad);

            var gamma = 23.0 * (p2 + 0.305) * t
                      / (23.0 * p1 + 11.0 * t * hCos + 108.0 * t * hSin);

            var a = gamma * hCos;
            var b = gamma * hSin;

            var rA = (460.0 * p2 + 451.0 * a + 288.0 * b) / 1403.0;
            var gA = (460.0 * p2 - 891.0 * a - 261.0 * b) / 1403.0;
            var bA = (460.0 * p2 - 220.0 * a - 6300.0 * b) / 1403.0;

            var rCBase = math.max(0.0, 27.13 * math.abs(rA) / (400.0 - math.abs(rA)));
            var gCBase = math.max(0.0, 27.13 * math.abs(gA) / (400.0 - math.abs(gA)));
            var bCBase = math.max(0.0, 27.13 * math.abs(bA) / (400.0 - math.abs(bA)));

            var rC = MathUtils.Signum(rA) * (100.0 / viewingConditions.Fl) * math.pow(rCBase, 1.0 / 0.42);
            var gC = MathUtils.Signum(gA) * (100.0 / viewingConditions.Fl) * math.pow(gCBase, 1.0 / 0.42);
            var bC = MathUtils.Signum(bA) * (100.0 / viewingConditions.Fl) * math.pow(bCBase, 1.0 / 0.42);

            var rgbF = new double3(
                rC / viewingConditions.RgbD.x,
                gC / viewingConditions.RgbD.y,
                bC / viewingConditions.RgbD.z
            );

            return math.mul(_cam16RgbToXyz, rgbF);
        }
    }
}