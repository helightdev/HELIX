namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A class containing a value that changes with the contrast level.
    /// </summary>
    public sealed class ContrastCurve {
    public ContrastCurve(double low, double normal, double medium, double high) {
      Low = low;
      Normal = normal;
      Medium = medium;
      High = high;
    }

    public double Low { get; }
    public double Normal { get; }
    public double Medium { get; }
    public double High { get; }

    public double Get(double contrastLevel) {
      if (contrastLevel <= -1.0) return Low;

      if (contrastLevel < 0.0) return MathUtils.Lerp(Low, Normal, (contrastLevel - -1.0) / 1.0);

      if (contrastLevel < 0.5) return MathUtils.Lerp(Normal, Medium, contrastLevel / 0.5);

      if
        (contrastLevel < 1.0) return MathUtils.Lerp(Medium, High, (contrastLevel - 0.5) / 0.5);

      return High;
    }
  }
}