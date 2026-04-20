using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A class that solves the HCT equation.
    /// </summary>
    public static class HctSolver {
    // Stored transposed for Unity column-major matrices.
    private static readonly double3x3 _scaledDiscountFromLinrgb = new(
      new double3(0.001200833568784504, 0.0005891086651375999, 0.00010146692491640572),
      new double3(0.002389694492170889, 0.0029785502573438758, 0.0005364214359186694),
      new double3(0.0002795742885861124, 0.0003270666104008398, 0.0032979401770712076)
    );

    // Stored transposed for Unity column-major matrices.
    private static readonly double3x3 _linrgbFromScaledDiscount = new(
      new double3(1373.2198709594231, -271.815969077903, 1.9622899599665666),
      new double3(-1100.4251190754821, 559.6580465940733, -57.173814538844006),
      new double3(-7.278681089101213, -32.46047482791194, 308.7233197812385)
    );

    private static readonly double3 _yFromLinrgb = new(0.2126, 0.7152, 0.0722);

    private static readonly double[] _criticalPlanes = {
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
      96.03240364439274, 96.9059996312159, 97.78421388448044, 98.6670533535366, 99.55452497210776
    };

    private static double SanitizeRadians(double angle) {
      return (angle + math.PI * 8.0) % (math.PI * 2.0);
    }

    private static double TrueDelinearized(double rgbComponent) {
      var normalized = rgbComponent / 100.0;
      var delinearized = normalized <= 0.0031308
        ? normalized * 12.92
        : 1.055 * math.pow(normalized, 1.0 / 2.4) - 0.055;

      return delinearized * 255.0;
    }

    private static double ChromaticAdaptation(double component) {
      var af = math.pow(math.abs(component), 0.42);
      return MathUtils.Signum(component) * 400.0 * af / (af + 27.13);
    }

    private static double HueOf(double3 linrgb) {
      var scaledDiscount = MathUtils.MatrixMultiply(linrgb, _scaledDiscountFromLinrgb);

      var rA = ChromaticAdaptation(scaledDiscount.x);
      var gA = ChromaticAdaptation(scaledDiscount.y);
      var bA = ChromaticAdaptation(scaledDiscount.z);

      var a = (11.0 * rA + -12.0 * gA + bA) / 11.0;
      var b = (rA + gA - 2.0 * bA) / 9.0;

      return math.atan2(b, a);
    }

    private static bool AreInCyclicOrder(double a, double b, double c) {
      var deltaAb = SanitizeRadians(b - a);
      var deltaAc = SanitizeRadians(c - a);
      return deltaAb < deltaAc;
    }

    private static double Intercept(double source, double mid, double target) {
      return (mid - source) / (target - source);
    }

    private static double3 LerpPoint(double3 source, double t, double3 target) {
      return source + (target - source) * t;
    }

    private static double3 SetCoordinate(double3 source, double coordinate, double3 target, int axis) {
      var sourceAxis = axis == 0 ? source.x : axis == 1 ? source.y : source.z;
      var targetAxis = axis == 0 ? target.x : axis == 1 ? target.y : target.z;
      var t = Intercept(sourceAxis, coordinate, targetAxis);
      return LerpPoint(source, t, target);
    }

    private static bool IsBounded(double x) {
      return x >= 0.0 && x <= 100.0;
    }

    private static double3 NthVertex(double y, int n) {
      var kR = _yFromLinrgb.x;
      var kG = _yFromLinrgb.y;
      var kB = _yFromLinrgb.z;

      var coordA = n % 4 <= 1 ? 0.0 : 100.0;
      var coordB = (n & 1) == 0 ? 0.0 : 100.0;

      if (n < 4) {
        var g = coordA;
        var b = coordB;
        var r = (y - g * kG - b * kB) / kR;
        return IsBounded(r) ? new double3(r, g, b) : new double3(-1.0, -1.0, -1.0);
      }

      if (n < 8) {
        var b = coordA;
        var r = coordB;
        var g = (y - r * kR - b * kB) / kG;
        return IsBounded(g) ? new double3(r, g, b) : new double3(-1.0, -1.0, -1.0);
      } else {
        var r = coordA;
        var g = coordB;
        var b = (y - r * kR - g * kG) / kB;
        return IsBounded(b) ? new double3(r, g, b) : new double3(-1.0, -1.0, -1.0);
      }
    }

    private static void BisectToSegment(double y, double targetHue, out double3 left, out double3 right) {
      left = new double3(-1.0, -1.0, -1.0);
      right = left;

      var leftHue = 0.0;
      var rightHue = 0.0;
      var initialized = false;
      var uncut = true;

      for (var n = 0; n < 12; n++) {
        var mid = NthVertex(y, n);
        if (mid.x < 0.0) continue;

        var midHue = HueOf(mid);

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
      BisectToSegment(y, targetHue, out var left, out var right);
      var leftHue = HueOf(left);

      for (var axis = 0; axis < 3; axis++) {
        var leftAxis = axis == 0 ? left.x : axis == 1 ? left.y : left.z;
        var rightAxis = axis == 0 ? right.x : axis == 1 ? right.y : right.z;

        if (MathUtils.ApproximatelyEqual(leftAxis, rightAxis)) continue;

        int lPlane;
        int rPlane;

        if (leftAxis < rightAxis) {
          lPlane = CriticalPlaneBelow(TrueDelinearized(leftAxis));
          rPlane = CriticalPlaneAbove(TrueDelinearized(rightAxis));
        } else {
          lPlane = CriticalPlaneAbove(TrueDelinearized(leftAxis));
          rPlane = CriticalPlaneBelow(TrueDelinearized(rightAxis));
        }

        for (var i = 0; i < 8; i++) {
          if (math.abs(rPlane - lPlane) <= 1) break;

          var mPlane = (int)math.floor((lPlane + rPlane) / 2.0);
          var midPlaneCoordinate = _criticalPlanes[mPlane];
          var mid = SetCoordinate(left, midPlaneCoordinate, right, axis);
          var midHue = HueOf(mid);

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
      var adaptedAbs = math.abs(adapted);
      var @base = math.max(0.0, 27.13 * adaptedAbs / (400.0 - adaptedAbs));
      return MathUtils.Signum(adapted) * math.pow(@base, 1.0 / 0.42);
    }

    private static int FindResultByJ(double hueRadians, double chroma, double y) {
      var j = math.sqrt(y) * 11.0;

      var viewingConditions = ViewingConditions.Standard;
      var tInnerCoeff = 1.0 / math.pow(
        1.64 - math.pow(0.29, viewingConditions.BackgroundYTowhitePointY),
        0.73
      );

      var eHue = 0.25 * (math.cos(hueRadians + 2.0) + 3.8);
      var p1 = eHue * (50000.0 / 13.0) * viewingConditions.Nc * viewingConditions.Ncb;
      var hSin = math.sin(hueRadians);
      var hCos = math.cos(hueRadians);

      for (var iterationRound = 0; iterationRound < 5; iterationRound++) {
        var jNormalized = j / 100.0;
        var alpha = chroma == 0.0 || j == 0.0 ? 0.0 : chroma / math.sqrt(jNormalized);
        var t = math.pow(alpha * tInnerCoeff, 1.0 / 0.9);
        var ac = viewingConditions.Aw
                 * math.pow(jNormalized, 1.0 / viewingConditions.C / viewingConditions.Z);

        var p2 = ac / viewingConditions.Nbb;
        var gamma = 23.0 * (p2 + 0.305) * t
                    / (23.0 * p1 + 11.0 * t * hCos + 108.0 * t * hSin);

        var a = gamma * hCos;
        var b = gamma * hSin;

        var rA = (460.0 * p2 + 451.0 * a + 288.0 * b) / 1403.0;
        var gA = (460.0 * p2 - 891.0 * a - 261.0 * b) / 1403.0;
        var bA = (460.0 * p2 - 220.0 * a - 6300.0 * b) / 1403.0;

        var linrgb = MathUtils.MatrixMultiply(
          new double3(
            InverseChromaticAdaptation(rA),
            InverseChromaticAdaptation(gA),
            InverseChromaticAdaptation(bA)
          ),
          _linrgbFromScaledDiscount
        );

        if (linrgb.x < 0.0 || linrgb.y < 0.0 || linrgb.z < 0.0) return 0;

        var fnj = math.dot(_yFromLinrgb, linrgb);
        if (fnj <= 0.0) return 0;

        if (iterationRound == 4 || math.abs(fnj - y) < 0.002) {
          if (linrgb.x > 100.01 || linrgb.y > 100.01 || linrgb.z > 100.01) return 0;

          return MaterialColorUtils.ArgbFromLinrgb(linrgb);
        }

        j = j - (fnj - y) * j / (2.0 * fnj);
      }

      return 0;
    }

    public static int SolveToInt(double hueDegrees, double chroma, double lstar) {
      if (chroma < 0.0001 || lstar < 0.0001 || lstar > 99.9999) return MaterialColorUtils.ArgbFromLstar(lstar);

      hueDegrees = MathUtils.SanitizeDegreesDouble(hueDegrees);
      var hueRadians = math.radians(hueDegrees);
      var y = MaterialColorUtils.YFromLstar(lstar);

      var exactAnswer = FindResultByJ(hueRadians, chroma, y);
      if (exactAnswer != 0) return exactAnswer;

      var linrgb = BisectToLimit(y, hueRadians);
      return MaterialColorUtils.ArgbFromLinrgb(linrgb);
    }

    public static Cam16 SolveToCam(double hueDegrees, double chroma, double lstar) {
      return Cam16.FromInt(SolveToInt(hueDegrees, chroma, lstar));
    }
  }
}