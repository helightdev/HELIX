using System;
using Unity.Mathematics;

namespace MaterialColorUtilities {
    /// <summary>
    /// In traditional color spaces, a color can be identified solely by the
    /// observer's measurement of the color. Color appearance models such as CAM16
    /// also use information about the environment where the color was observed,
    /// known as the viewing conditions.
    /// </summary>
    public sealed class ViewingConditions {
        public static readonly ViewingConditions SRgb = Make();
        public static readonly ViewingConditions Standard = SRgb;

        public double3 WhitePoint { get; }
        public double AdaptingLuminance { get; }
        public double BackgroundLstar { get; }
        public double Surround { get; }
        public bool DiscountingIlluminant { get; }

        public double BackgroundYTowhitePointY { get; }
        public double Aw { get; }
        public double Nbb { get; }
        public double Ncb { get; }
        public double C { get; }
        public double NC { get; }
        public double3 DrgbInverse { get; }
        public double3 RgbD { get; }
        public double Fl { get; }
        public double FLRoot { get; }
        public double Z { get; }

        private ViewingConditions(
            double3 whitePoint,
            double adaptingLuminance,
            double backgroundLstar,
            double surround,
            bool discountingIlluminant,
            double backgroundYTowhitePointY,
            double aw,
            double nbb,
            double ncb,
            double c,
            double nC,
            double3 drgbInverse,
            double3 rgbD,
            double fl,
            double fLRoot,
            double z
        ) {
            WhitePoint = whitePoint;
            AdaptingLuminance = adaptingLuminance;
            BackgroundLstar = backgroundLstar;
            Surround = surround;
            DiscountingIlluminant = discountingIlluminant;
            BackgroundYTowhitePointY = backgroundYTowhitePointY;
            Aw = aw;
            Nbb = nbb;
            Ncb = ncb;
            C = c;
            NC = nC;
            DrgbInverse = drgbInverse;
            RgbD = rgbD;
            Fl = fl;
            FLRoot = fLRoot;
            Z = z;
        }

        /// <summary>
        /// Convenience constructor for ViewingConditions.
        /// </summary>
        public static ViewingConditions Make(
            double3? whitePoint = null,
            double adaptingLuminance = -1.0,
            double backgroundLstar = 50.0,
            double surround = 2.0,
            bool discountingIlluminant = false
        ) {
            double3 wp = whitePoint ?? ColorUtils.WhitePointD65();

            if (adaptingLuminance <= 0.0) {
                adaptingLuminance = (200.0 / math.PI) * ColorUtils.YFromLstar(50.0) / 100.0;
            }

            backgroundLstar = math.max(0.1, backgroundLstar);

            double rW = (wp.x * 0.401288) + (wp.y * 0.650173) + (wp.z * -0.051461);
            double gW = (wp.x * -0.250268) + (wp.y * 1.204414) + (wp.z * 0.045854);
            double bW = (wp.x * -0.002079) + (wp.y * 0.048952) + (wp.z * 0.953127);

            double f = 0.8 + (surround / 10.0);
            double c = f >= 0.9
                ? MathUtils.Lerp(0.59, 0.69, (f - 0.9) * 10.0)
                : MathUtils.Lerp(0.525, 0.59, (f - 0.8) * 10.0);

            double d = discountingIlluminant
                ? 1.0
                : f * (1.0 - ((1.0 / 3.6) * math.exp((-adaptingLuminance - 42.0) / 92.0)));

            d = math.clamp(d, 0.0, 1.0);

            double nc = f;

            double3 rgbD = new double3(
                d * (100.0 / rW) + 1.0 - d,
                d * (100.0 / gW) + 1.0 - d,
                d * (100.0 / bW) + 1.0 - d
            );

            double k = 1.0 / ((5.0 * adaptingLuminance) + 1.0);
            double k4 = k * k * k * k;
            double k4F = 1.0 - k4;

            double fl = (k4 * adaptingLuminance)
                      + (0.1 * k4F * k4F * math.pow(5.0 * adaptingLuminance, 1.0 / 3.0));

            double n = ColorUtils.YFromLstar(backgroundLstar) / wp.y;
            double z = 1.48 + math.sqrt(n);
            double nbb = 0.725 / math.pow(n, 0.2);
            double ncb = nbb;

            double3 rgbAFactors = new double3(
                math.pow(fl * rgbD.x * rW / 100.0, 0.42),
                math.pow(fl * rgbD.y * gW / 100.0, 0.42),
                math.pow(fl * rgbD.z * bW / 100.0, 0.42)
            );

            double3 rgbA = new double3(
                (400.0 * rgbAFactors.x) / (rgbAFactors.x + 27.13),
                (400.0 * rgbAFactors.y) / (rgbAFactors.y + 27.13),
                (400.0 * rgbAFactors.z) / (rgbAFactors.z + 27.13)
            );

            double aw = (((40.0 * rgbA.x) + (20.0 * rgbA.y) + rgbA.z) / 20.0) * nbb;

            return new ViewingConditions(
                wp,
                adaptingLuminance,
                backgroundLstar,
                surround,
                discountingIlluminant,
                n,
                aw,
                nbb,
                ncb,
                c,
                nc,
                double3.zero,
                rgbD,
                fl,
                math.pow(fl, 0.25),
                z
            );
        }
    }

    /// <summary>
    /// CAM16, a color appearance model.
    /// </summary>
    public sealed class Cam16 {
        // XYZ -> CAM16 RGB "cone" response matrix.
        // Stored transposed for Unity's column-major double3x3 so math.mul(M, xyz)
        // matches the Dart row-based equations.
        private static readonly double3x3 XyzToCam16Rgb = new double3x3(
            new double3(0.401288, -0.250268, -0.002079),
            new double3(0.650173, 1.204414, 0.048952),
            new double3(-0.051461, 0.045854, 0.953127)
        );

        // CAM16 RGB -> XYZ matrix.
        // Stored transposed for Unity's column-major double3x3.
        private static readonly double3x3 Cam16RgbToXyz = new double3x3(
            new double3(1.86206786, 0.38752654, -0.01584150),
            new double3(-1.01125463, 0.62144744, -0.03412294),
            new double3(0.14918677, -0.00897398, 1.04996444)
        );

        public double Hue { get; }
        public double Chroma { get; }
        public double J { get; }
        public double Q { get; }
        public double M { get; }
        public double S { get; }
        public double Jstar { get; }
        public double Astar { get; }
        public double Bstar { get; }

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

        public double Distance(Cam16 other) {
            double dJ = Jstar - other.Jstar;
            double dA = Astar - other.Astar;
            double dB = Bstar - other.Bstar;
            double dEPrime = math.sqrt((dJ * dJ) + (dA * dA) + (dB * dB));
            return 1.41 * math.pow(dEPrime, 0.63);
        }

        public static Cam16 FromInt(int argb) {
            return FromIntInViewingConditions(argb, ViewingConditions.SRgb);
        }

        public static Cam16 FromIntInViewingConditions(int argb, ViewingConditions viewingConditions) {
            double3 xyz = ColorUtils.XyzFromArgb(argb);
            return FromXyzInViewingConditions(xyz.x, xyz.y, xyz.z, viewingConditions);
        }

        public static Cam16 FromXyzInViewingConditions(
            double x,
            double y,
            double z,
            ViewingConditions viewingConditions
        ) {
            double3 rgbC = math.mul(XyzToCam16Rgb, new double3(x, y, z));
            double3 rgbD = rgbC * viewingConditions.RgbD;

            double rAF = math.pow(viewingConditions.Fl * math.abs(rgbD.x) / 100.0, 0.42);
            double gAF = math.pow(viewingConditions.Fl * math.abs(rgbD.y) / 100.0, 0.42);
            double bAF = math.pow(viewingConditions.Fl * math.abs(rgbD.z) / 100.0, 0.42);

            double rA = MathUtils.Signum(rgbD.x) * 400.0 * rAF / (rAF + 27.13);
            double gA = MathUtils.Signum(rgbD.y) * 400.0 * gAF / (gAF + 27.13);
            double bA = MathUtils.Signum(rgbD.z) * 400.0 * bAF / (bAF + 27.13);

            double a = ((11.0 * rA) + (-12.0 * gA) + bA) / 11.0;
            double b = (rA + gA - (2.0 * bA)) / 9.0;

            double u = ((20.0 * rA) + (20.0 * gA) + (21.0 * bA)) / 20.0;
            double p2 = ((40.0 * rA) + (20.0 * gA) + bA) / 20.0;

            double atanDegrees = math.degrees(math.atan2(b, a));
            double hue = MathUtils.SanitizeDegreesDouble(atanDegrees);
            double hueRadians = math.radians(hue);

            double ac = p2 * viewingConditions.Nbb;

            double j = 100.0 * math.pow(ac / viewingConditions.Aw, viewingConditions.C * viewingConditions.Z);
            double q = (4.0 / viewingConditions.C)
                     * math.sqrt(j / 100.0)
                     * (viewingConditions.Aw + 4.0)
                     * viewingConditions.FLRoot;

            double huePrime = hue < 20.14 ? hue + 360.0 : hue;
            double eHue = 0.25 * (math.cos(math.radians(huePrime) + 2.0) + 3.8);
            double p1 = (50000.0 / 13.0) * eHue * viewingConditions.NC * viewingConditions.Ncb;
            double t = p1 * math.sqrt((a * a) + (b * b)) / (u + 0.305);

            double alpha = math.pow(t, 0.9)
                         * math.pow(1.64 - math.pow(0.29, viewingConditions.BackgroundYTowhitePointY), 0.73);

            double c = alpha * math.sqrt(j / 100.0);
            double m = c * viewingConditions.FLRoot;
            double s = 50.0 * math.sqrt((alpha * viewingConditions.C) / (viewingConditions.Aw + 4.0));

            double jstar = ((1.0 + (100.0 * 0.007)) * j) / (1.0 + (0.007 * j));
            double mstar = math.log(1.0 + (0.0228 * m)) / 0.0228;
            double astar = mstar * math.cos(hueRadians);
            double bstar = mstar * math.sin(hueRadians);

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
            double q = (4.0 / viewingConditions.C)
                     * math.sqrt(j / 100.0)
                     * (viewingConditions.Aw + 4.0)
                     * viewingConditions.FLRoot;

            double m = c * viewingConditions.FLRoot;
            double alpha = c / math.sqrt(j / 100.0);
            double s = 50.0 * math.sqrt((alpha * viewingConditions.C) / (viewingConditions.Aw + 4.0));

            double hueRadians = math.radians(h);
            double jstar = ((1.0 + (100.0 * 0.007)) * j) / (1.0 + (0.007 * j));
            double mstar = math.log(1.0 + (0.0228 * m)) / 0.0228;
            double astar = mstar * math.cos(hueRadians);
            double bstar = mstar * math.sin(hueRadians);

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
            double m = math.sqrt((astar * astar) + (bstar * bstar));
            double bigM = (math.exp(m * 0.0228) - 1.0) / 0.0228;
            double c = bigM / viewingConditions.FLRoot;

            double h = math.degrees(math.atan2(bstar, astar));
            if (h < 0.0) { h += 360.0; }

            double j = jstar / (1.0 - ((jstar - 100.0) * 0.007));
            return FromJchInViewingConditions(j, c, h, viewingConditions);
        }

        public int ToInt() {
            return Viewed(ViewingConditions.SRgb);
        }

        public int Viewed(ViewingConditions viewingConditions) {
            double3 xyz = XyzInViewingConditions(viewingConditions);
            return ColorUtils.ArgbFromXyz(xyz.x, xyz.y, xyz.z);
        }

        public double3 XyzInViewingConditions(ViewingConditions viewingConditions) {
            double alpha = (Chroma == 0.0 || J == 0.0)
                ? 0.0
                : Chroma / math.sqrt(J / 100.0);

            double t = math.pow(
                alpha / math.pow(
                    1.64 - math.pow(0.29, viewingConditions.BackgroundYTowhitePointY),
                    0.73
                ),
                1.0 / 0.9
            );

            double hRad = math.radians(Hue);
            double eHue = 0.25 * (math.cos(hRad + 2.0) + 3.8);
            double ac = viewingConditions.Aw
                      * math.pow(J / 100.0, 1.0 / viewingConditions.C / viewingConditions.Z);

            double p1 = eHue * (50000.0 / 13.0) * viewingConditions.NC * viewingConditions.Ncb;
            double p2 = ac / viewingConditions.Nbb;

            double hSin = math.sin(hRad);
            double hCos = math.cos(hRad);

            double gamma = 23.0 * (p2 + 0.305) * t
                         / ((23.0 * p1) + (11.0 * t * hCos) + (108.0 * t * hSin));

            double a = gamma * hCos;
            double b = gamma * hSin;

            double rA = ((460.0 * p2) + (451.0 * a) + (288.0 * b)) / 1403.0;
            double gA = ((460.0 * p2) - (891.0 * a) - (261.0 * b)) / 1403.0;
            double bA = ((460.0 * p2) - (220.0 * a) - (6300.0 * b)) / 1403.0;

            double rCBase = math.max(0.0, (27.13 * math.abs(rA)) / (400.0 - math.abs(rA)));
            double gCBase = math.max(0.0, (27.13 * math.abs(gA)) / (400.0 - math.abs(gA)));
            double bCBase = math.max(0.0, (27.13 * math.abs(bA)) / (400.0 - math.abs(bA)));

            double rC = MathUtils.Signum(rA) * (100.0 / viewingConditions.Fl) * math.pow(rCBase, 1.0 / 0.42);
            double gC = MathUtils.Signum(gA) * (100.0 / viewingConditions.Fl) * math.pow(gCBase, 1.0 / 0.42);
            double bC = MathUtils.Signum(bA) * (100.0 / viewingConditions.Fl) * math.pow(bCBase, 1.0 / 0.42);

            double3 rgbF = new double3(
                rC / viewingConditions.RgbD.x,
                gC / viewingConditions.RgbD.y,
                bC / viewingConditions.RgbD.z
            );

            return math.mul(Cam16RgbToXyz, rgbF);
        }
    }

    /// <summary>
    /// HCT, hue, chroma, and tone.
    /// </summary>
    public sealed class Hct : IEquatable<Hct> {
        private double _hue;
        private double _chroma;
        private double _tone;
        private int _argb;

        private Hct(int argb) {
            _argb = argb;
            Cam16 cam16 = Cam16.FromInt(argb);
            _hue = cam16.Hue;
            _chroma = cam16.Chroma;
            _tone = ColorUtils.LstarFromArgb(_argb);
        }

        public static Hct From(double hue, double chroma, double tone) {
            int argb = HctSolver.SolveToInt(hue, chroma, tone);
            return new Hct(argb);
        }

        public static Hct FromInt(int argb) {
            return new Hct(argb);
        }

        public int ToInt() {
            return _argb;
        }

        public double Hue {
            get => _hue;
            set {
                _argb = HctSolver.SolveToInt(value, Chroma, Tone);
                Cam16 cam16 = Cam16.FromInt(_argb);
                _hue = cam16.Hue;
                _chroma = cam16.Chroma;
                _tone = ColorUtils.LstarFromArgb(_argb);
            }
        }

        public double Chroma {
            get => _chroma;
            set {
                _argb = HctSolver.SolveToInt(Hue, value, Tone);
                Cam16 cam16 = Cam16.FromInt(_argb);
                _hue = cam16.Hue;
                _chroma = cam16.Chroma;
                _tone = ColorUtils.LstarFromArgb(_argb);
            }
        }

        public double Tone {
            get => _tone;
            set {
                _argb = HctSolver.SolveToInt(Hue, Chroma, value);
                Cam16 cam16 = Cam16.FromInt(_argb);
                _hue = cam16.Hue;
                _chroma = cam16.Chroma;
                _tone = ColorUtils.LstarFromArgb(_argb);
            }
        }

        public Hct InViewingConditions(ViewingConditions vc) {
            Cam16 cam16 = Cam16.FromInt(ToInt());
            double3 viewedInVc = cam16.XyzInViewingConditions(vc);

            Cam16 recastInVc = Cam16.FromXyzInViewingConditions(
                viewedInVc.x,
                viewedInVc.y,
                viewedInVc.z,
                ViewingConditions.Make()
            );

            return From(
                recastInVc.Hue,
                recastInVc.Chroma,
                ColorUtils.LstarFromY(viewedInVc.y)
            );
        }

        public bool Equals(Hct other) {
            return !(other is null) && other._argb == _argb;
        }

        public override bool Equals(object obj) {
            return obj is Hct other && Equals(other);
        }

        public override int GetHashCode() {
            return _argb;
        }

        public override string ToString() {
            return $"H{math.round(_hue)} C{math.round(_chroma)} T{math.round(_tone)}";
        }
    }

    /// <summary>
    /// A class that solves the HCT equation.
    /// </summary>
    public static class HctSolver {
        // Stored transposed for Unity column-major matrices.
        private static readonly double3x3 ScaledDiscountFromLinrgb = new double3x3(
            new double3(0.001200833568784504, 0.0005891086651375999, 0.00010146692491640572),
            new double3(0.002389694492170889, 0.0029785502573438758, 0.0005364214359186694),
            new double3(0.0002795742885861124, 0.0003270666104008398, 0.0032979401770712076)
        );

        // Stored transposed for Unity column-major matrices.
        private static readonly double3x3 LinrgbFromScaledDiscount = new double3x3(
            new double3(1373.2198709594231, -271.815969077903, 1.9622899599665666),
            new double3(-1100.4251190754821, 559.6580465940733, -57.173814538844006),
            new double3(-7.278681089101213, -32.46047482791194, 308.7233197812385)
        );

        private static readonly double3 YFromLinrgb = new double3(0.2126, 0.7152, 0.0722);

        private static readonly double[] CriticalPlanes = new double[] {
            0.015176349177441876, 0.045529047532325624, 0.07588174588720938, 0.10623444424209313, 0.13658714259697685,
            0.16693984095186062, 0.19729253930674434, 0.2276452376616281, 0.2579979360165119, 0.28835063437139563,
            0.3188300904430532, 0.350925934958123, 0.3848314933096426, 0.42057480301049466, 0.458183274052838,
            0.4976837250274023, 0.5391024159806381, 0.5824650784040898, 0.6277969426914107, 0.6751227633498623,
            0.7244668422128921, 0.775853049866786, 0.829304845476233, 0.8848452951698498, 0.942497089126609,
            1.0022825574869039, 1.0642236851973577, 1.1283421258858297, 1.1946592148522128, 1.2631959812511864,
            1.3339731595349034, 1.407011200216447, 1.4823302800086415, 1.5599503113873272, 1.6398909516233677,
            1.7221716113234105, 1.8068114625156377, 1.8938294463134073, 1.9832442801866852, 2.075074464868551,
            2.1693382909216234, 2.2660538449872063, 2.36523901573795, 2.4669114995532007, 2.5710888059345764,
            2.6777882626779785, 2.7870270208169257, 2.898822059350997, 3.0131901897720907, 3.1301480604002863,
            3.2497121605402226, 3.3718988244681087, 3.4967242352587946, 3.624204428461639, 3.754355295633311,
            3.887192587735158, 4.022731918402185, 4.160988767090289, 4.301978482107941, 4.445716283538092,
            4.592217266055746, 4.741496401646282, 4.893568542229298, 5.048448422192488, 5.20615066083972,
            5.3666897647573375, 5.5300801301023865, 5.696336044816294, 5.865471690767354, 6.037501145825082,
            6.212438385869475, 6.390297286737924, 6.571091626112461, 6.7548350853498045, 6.941541251256611,
            7.131223617812143, 7.323895587840543, 7.5195704746346665, 7.7182615035334345, 7.919981813454504,
            8.124744458384042, 8.332562408825165, 8.543448553206703, 8.757415699253682, 8.974476575321063,
            9.194643831691977, 9.417930041841839, 9.644347703669503, 9.873909240696694, 10.106627003236781,
            10.342513269534024, 10.58158024687427, 10.8238400726681, 11.069304815507364, 11.317986476196008,
            11.569896988756009, 11.825048221409341, 12.083451977536606, 12.345119996613247, 12.610063955123938,
            12.878295467455942, 13.149826086772048, 13.42466730586372, 13.702830557985108, 13.984327217668513,
            14.269168601521828, 14.55736596900856, 14.848930523210871, 15.143873411576273, 15.44220572664832,
            15.743938506781891, 16.04908273684337, 16.35764934889634, 16.66964922287304, 16.985093187232053,
            17.30399201960269, 17.62635644741625, 17.95219714852476, 18.281524751807332, 18.614349837764564,
            18.95068293910138, 19.290534541298456, 19.633915083172692, 19.98083495742689, 20.331304511189067,
            20.685334046541502, 21.042933821039977, 21.404114048223256, 21.76888489811322, 22.137256497705877,
            22.50923893145328, 22.884842241736916, 23.264076429332462, 23.6469514538663, 24.033477234264016,
            24.42366364919083, 24.817520537484558, 25.21505769858089, 25.61628489293138, 26.021211842414342,
            26.429848230738664, 26.842203703840827, 27.258287870275353, 27.678110301598522, 28.10168053274597,
            28.529008062403893, 28.96010235337422, 29.39497283293396, 29.83362889318845, 30.276079891419332,
            30.722335150426627, 31.172403958865512, 31.62629557157785, 32.08401920991837, 32.54558406207592,
            33.010999283389665, 33.4802739966603, 33.953417292456834, 34.430438229418264, 34.911345834551085,
            35.39614910352207, 35.88485700094671, 36.37747846067349, 36.87402238606382, 37.37449765026789,
            37.87891309649659, 38.38727753828926, 38.89959975977785, 39.41588851594697, 39.93615253289054,
            40.460400508064545, 40.98864111053629, 41.520882981230194, 42.05713473317016, 42.597404951718396,
            43.141702194811224, 43.6900349931913, 44.24241185063697, 44.798841244188324, 45.35933162437017,
            45.92389141541209, 46.49252901546552, 47.065252796817916, 47.64207110610409, 48.22299226451468,
            48.808024568002054, 49.3971762874833, 49.9904556690408, 50.587870934119984, 51.189430279724725,
            51.79514187861014, 52.40501387947288, 53.0190544071392, 53.637271562750364, 54.259673423945976,
            54.88626804504493, 55.517063457223934, 56.15206766869424, 56.79128866487574, 57.43473440856916,
            58.08241284012621, 58.734331877617365, 59.39049941699807, 60.05092333227251, 60.715611475655585,
            61.38457167773311, 62.057811747619894, 62.7353394731159, 63.417162620860914, 64.10328893648692,
            64.79372614476921, 65.48848194977529, 66.18756403501224, 66.89098006357258, 67.59873767827808,
            68.31084450182222, 69.02730813691093, 69.74813616640164, 70.47333615344107, 71.20291564160104,
            71.93688215501312, 72.67524319850172, 73.41800625771542, 74.16517879925733, 74.9167682708136,
            75.67278210128072, 76.43322770089146, 77.1981124613393, 77.96744375590167, 78.74122893956174,
            79.51947534912904, 80.30219030335869, 81.08938110306934, 81.88105503125999, 82.67721935322541,
            83.4778813166706, 84.28304815182372, 85.09272707154808, 85.90692527145302, 86.72564993000343,
            87.54890820862819, 88.3767072518277, 89.2090541872801, 90.04595612594655, 90.88742016217518,
            91.73345337380438, 92.58406282226491, 93.43925555268066, 94.29903859396902, 95.16341895893969,
            96.03240364439274, 96.9059996312159, 97.78421388448044, 98.6670533535366, 99.55452497210776,
        };

        private static double SanitizeRadians(double angle) {
            return (angle + (math.PI * 8.0)) % (math.PI * 2.0);
        }

        private static double TrueDelinearized(double rgbComponent) {
            double normalized = rgbComponent / 100.0;
            double delinearized = normalized <= 0.0031308
                ? normalized * 12.92
                : (1.055 * math.pow(normalized, 1.0 / 2.4)) - 0.055;

            return delinearized * 255.0;
        }

        private static double ChromaticAdaptation(double component) {
            double af = math.pow(math.abs(component), 0.42);
            return MathUtils.Signum(component) * 400.0 * af / (af + 27.13);
        }

        private static double HueOf(double3 linrgb) {
            double3 scaledDiscount = MathUtils.MatrixMultiply(linrgb, ScaledDiscountFromLinrgb);

            double rA = ChromaticAdaptation(scaledDiscount.x);
            double gA = ChromaticAdaptation(scaledDiscount.y);
            double bA = ChromaticAdaptation(scaledDiscount.z);

            double a = ((11.0 * rA) + (-12.0 * gA) + bA) / 11.0;
            double b = (rA + gA - (2.0 * bA)) / 9.0;

            return math.atan2(b, a);
        }

        private static bool AreInCyclicOrder(double a, double b, double c) {
            double deltaAB = SanitizeRadians(b - a);
            double deltaAC = SanitizeRadians(c - a);
            return deltaAB < deltaAC;
        }

        private static double Intercept(double source, double mid, double target) {
            return (mid - source) / (target - source);
        }

        private static double3 LerpPoint(double3 source, double t, double3 target) {
            return source + ((target - source) * t);
        }

        private static double3 SetCoordinate(double3 source, double coordinate, double3 target, int axis) {
            double sourceAxis = axis == 0 ? source.x : axis == 1 ? source.y : source.z;
            double targetAxis = axis == 0 ? target.x : axis == 1 ? target.y : target.z;
            double t = Intercept(sourceAxis, coordinate, targetAxis);
            return LerpPoint(source, t, target);
        }

        private static bool IsBounded(double x) {
            return x >= 0.0 && x <= 100.0;
        }

        private static double3 NthVertex(double y, int n) {
            double kR = YFromLinrgb.x;
            double kG = YFromLinrgb.y;
            double kB = YFromLinrgb.z;

            double coordA = (n % 4) <= 1 ? 0.0 : 100.0;
            double coordB = (n & 1) == 0 ? 0.0 : 100.0;

            if (n < 4) {
                double g = coordA;
                double b = coordB;
                double r = (y - (g * kG) - (b * kB)) / kR;
                return IsBounded(r) ? new double3(r, g, b) : new double3(-1.0, -1.0, -1.0);
            } else if (n < 8) {
                double b = coordA;
                double r = coordB;
                double g = (y - (r * kR) - (b * kB)) / kG;
                return IsBounded(g) ? new double3(r, g, b) : new double3(-1.0, -1.0, -1.0);
            } else {
                double r = coordA;
                double g = coordB;
                double b = (y - (r * kR) - (g * kG)) / kB;
                return IsBounded(b) ? new double3(r, g, b) : new double3(-1.0, -1.0, -1.0);
            }
        }

        private static void BisectToSegment(double y, double targetHue, out double3 left, out double3 right) {
            left = new double3(-1.0, -1.0, -1.0);
            right = left;

            double leftHue = 0.0;
            double rightHue = 0.0;
            bool initialized = false;
            bool uncut = true;

            for (int n = 0; n < 12; n++) {
                double3 mid = NthVertex(y, n);
                if (mid.x < 0.0) { continue; }

                double midHue = HueOf(mid);

                if (!initialized) {
                    left = mid;
                    right = mid;
                    leftHue = midHue;
                    rightHue = midHue;
                    initialized = true;
                    continue;
                }

                if (uncut || AreInCyclicOrder(leftHue, midHue, rightHue)) {
                    uncut = false;

                    if (AreInCyclicOrder(leftHue, targetHue, midHue)) {
                        right = mid;
                        rightHue = midHue;
                    } else {
                        left = mid;
                        leftHue = midHue;
                    }
                }
            }
        }

        private static double3 Midpoint(double3 a, double3 b) {
            return (a + b) * 0.5;
        }

        private static int CriticalPlaneBelow(double x) {
            return (int)math.floor(x - 0.5);
        }

        private static int CriticalPlaneAbove(double x) {
            return (int)math.ceil(x - 0.5);
        }

        private static double3 BisectToLimit(double y, double targetHue) {
            BisectToSegment(y, targetHue, out double3 left, out double3 right);
            double leftHue = HueOf(left);

            for (int axis = 0; axis < 3; axis++) {
                double leftAxis = axis == 0 ? left.x : axis == 1 ? left.y : left.z;
                double rightAxis = axis == 0 ? right.x : axis == 1 ? right.y : right.z;

                if (leftAxis == rightAxis) { continue; }

                int lPlane;
                int rPlane;

                if (leftAxis < rightAxis) {
                    lPlane = CriticalPlaneBelow(TrueDelinearized(leftAxis));
                    rPlane = CriticalPlaneAbove(TrueDelinearized(rightAxis));
                } else {
                    lPlane = CriticalPlaneAbove(TrueDelinearized(leftAxis));
                    rPlane = CriticalPlaneBelow(TrueDelinearized(rightAxis));
                }

                for (int i = 0; i < 8; i++) {
                    if (math.abs(rPlane - lPlane) <= 1) { break; }

                    int mPlane = (int)math.floor((lPlane + rPlane) / 2.0);
                    double midPlaneCoordinate = CriticalPlanes[mPlane];
                    double3 mid = SetCoordinate(left, midPlaneCoordinate, right, axis);
                    double midHue = HueOf(mid);

                    if (AreInCyclicOrder(leftHue, targetHue, midHue)) {
                        right = mid;
                        rPlane = mPlane;
                    } else {
                        left = mid;
                        leftHue = midHue;
                        lPlane = mPlane;
                    }
                }
            }

            return Midpoint(left, right);
        }

        private static double InverseChromaticAdaptation(double adapted) {
            double adaptedAbs = math.abs(adapted);
            double @base = math.max(0.0, 27.13 * adaptedAbs / (400.0 - adaptedAbs));
            return MathUtils.Signum(adapted) * math.pow(@base, 1.0 / 0.42);
        }

        private static int FindResultByJ(double hueRadians, double chroma, double y) {
            double j = math.sqrt(y) * 11.0;

            ViewingConditions viewingConditions = ViewingConditions.Standard;
            double tInnerCoeff = 1.0 / math.pow(
                1.64 - math.pow(0.29, viewingConditions.BackgroundYTowhitePointY),
                0.73
            );

            double eHue = 0.25 * (math.cos(hueRadians + 2.0) + 3.8);
            double p1 = eHue * (50000.0 / 13.0) * viewingConditions.NC * viewingConditions.Ncb;
            double hSin = math.sin(hueRadians);
            double hCos = math.cos(hueRadians);

            for (int iterationRound = 0; iterationRound < 5; iterationRound++) {
                double jNormalized = j / 100.0;
                double alpha = (chroma == 0.0 || j == 0.0) ? 0.0 : chroma / math.sqrt(jNormalized);
                double t = math.pow(alpha * tInnerCoeff, 1.0 / 0.9);
                double ac = viewingConditions.Aw
                          * math.pow(jNormalized, 1.0 / viewingConditions.C / viewingConditions.Z);

                double p2 = ac / viewingConditions.Nbb;
                double gamma = 23.0 * (p2 + 0.305) * t
                             / ((23.0 * p1) + (11.0 * t * hCos) + (108.0 * t * hSin));

                double a = gamma * hCos;
                double b = gamma * hSin;

                double rA = ((460.0 * p2) + (451.0 * a) + (288.0 * b)) / 1403.0;
                double gA = ((460.0 * p2) - (891.0 * a) - (261.0 * b)) / 1403.0;
                double bA = ((460.0 * p2) - (220.0 * a) - (6300.0 * b)) / 1403.0;

                double3 linrgb = MathUtils.MatrixMultiply(
                    new double3(
                        InverseChromaticAdaptation(rA),
                        InverseChromaticAdaptation(gA),
                        InverseChromaticAdaptation(bA)
                    ),
                    LinrgbFromScaledDiscount
                );

                if (linrgb.x < 0.0 || linrgb.y < 0.0 || linrgb.z < 0.0) { return 0; }

                double fnj = math.dot(YFromLinrgb, linrgb);
                if (fnj <= 0.0) { return 0; }

                if (iterationRound == 4 || math.abs(fnj - y) < 0.002) {
                    if (linrgb.x > 100.01 || linrgb.y > 100.01 || linrgb.z > 100.01) { return 0; }

                    return ColorUtils.ArgbFromLinrgb(linrgb);
                }

                j = j - ((fnj - y) * j / (2.0 * fnj));
            }

            return 0;
        }

        public static int SolveToInt(double hueDegrees, double chroma, double lstar) {
            if (chroma < 0.0001 || lstar < 0.0001 || lstar > 99.9999) { return ColorUtils.ArgbFromLstar(lstar); }

            hueDegrees = MathUtils.SanitizeDegreesDouble(hueDegrees);
            double hueRadians = math.radians(hueDegrees);
            double y = ColorUtils.YFromLstar(lstar);

            int exactAnswer = FindResultByJ(hueRadians, chroma, y);
            if (exactAnswer != 0) { return exactAnswer; }

            double3 linrgb = BisectToLimit(y, hueRadians);
            return ColorUtils.ArgbFromLinrgb(linrgb);
        }

        public static Cam16 SolveToCam(double hueDegrees, double chroma, double lstar) {
            return Cam16.FromInt(SolveToInt(hueDegrees, chroma, lstar));
        }
    }
}