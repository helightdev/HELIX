using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   In traditional color spaces, a color can be identified solely by the
    ///   observer's measurement of the color. Color appearance models such as CAM16
    ///   also use information about the environment where the color was observed,
    ///   known as the viewing conditions.
    /// </summary>
    public sealed class ViewingConditions {
    public static readonly ViewingConditions SRgb = Make();
    public static readonly ViewingConditions Standard = SRgb;

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
      Nc = nC;
      DrgbInverse = drgbInverse;
      RgbD = rgbD;
      Fl = fl;
      FlRoot = fLRoot;
      Z = z;
    }

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
    public double Nc { get; }
    public double3 DrgbInverse { get; }
    public double3 RgbD { get; }
    public double Fl { get; }
    public double FlRoot { get; }
    public double Z { get; }

        /// <summary>
        ///   Convenience constructor for ViewingConditions.
        /// </summary>
        public static ViewingConditions Make(
      double3? whitePoint = null,
      double adaptingLuminance = -1.0,
      double backgroundLstar = 50.0,
      double surround = 2.0,
      bool discountingIlluminant = false
    ) {
      var wp = whitePoint ?? MaterialColorUtils.WhitePointD65();

      if (adaptingLuminance <= 0.0) adaptingLuminance = 200.0 / math.PI * MaterialColorUtils.YFromLstar(50.0) / 100.0;

      backgroundLstar = math.max(0.1, backgroundLstar);

      var rW = wp.x * 0.401288 + wp.y * 0.650173 + wp.z * -0.051461;
      var gW = wp.x * -0.250268 + wp.y * 1.204414 + wp.z * 0.045854;
      var bW = wp.x * -0.002079 + wp.y * 0.048952 + wp.z * 0.953127;

      var f = 0.8 + surround / 10.0;
      var c = f >= 0.9
        ? MathUtils.Lerp(0.59, 0.69, (f - 0.9) * 10.0)
        : MathUtils.Lerp(0.525, 0.59, (f - 0.8) * 10.0);

      var d = discountingIlluminant
        ? 1.0
        : f * (1.0 - 1.0 / 3.6 * math.exp((-adaptingLuminance - 42.0) / 92.0));

      d = math.clamp(d, 0.0, 1.0);

      var nc = f;

      var rgbD = new double3(
        d * (100.0 / rW) + 1.0 - d,
        d * (100.0 / gW) + 1.0 - d,
        d * (100.0 / bW) + 1.0 - d
      );

      var k = 1.0 / (5.0 * adaptingLuminance + 1.0);
      var k4 = k * k * k * k;
      var k4F = 1.0 - k4;

      var fl = k4 * adaptingLuminance
               + 0.1 * k4F * k4F * math.pow(5.0 * adaptingLuminance, 1.0 / 3.0);

      var n = MaterialColorUtils.YFromLstar(backgroundLstar) / wp.y;
      var z = 1.48 + math.sqrt(n);
      var nbb = 0.725 / math.pow(n, 0.2);
      var ncb = nbb;

      var rgbAFactors = new double3(
        math.pow(fl * rgbD.x * rW / 100.0, 0.42),
        math.pow(fl * rgbD.y * gW / 100.0, 0.42),
        math.pow(fl * rgbD.z * bW / 100.0, 0.42)
      );

      var rgbA = new double3(
        400.0 * rgbAFactors.x / (rgbAFactors.x + 27.13),
        400.0 * rgbAFactors.y / (rgbAFactors.y + 27.13),
        400.0 * rgbAFactors.z / (rgbAFactors.z + 27.13)
      );

      var aw = (40.0 * rgbA.x + 20.0 * rgbA.y + rgbA.z) / 20.0 * nbb;

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
}