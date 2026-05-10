namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Comprises foundational palettes to build a color scheme.
    /// </summary>
    public sealed class CorePalettes {
    public CorePalettes(
      TonalPalette primary,
      TonalPalette secondary,
      TonalPalette tertiary,
      TonalPalette neutral,
      TonalPalette neutralVariant
    ) {
      Primary = primary;
      Secondary = secondary;
      Tertiary = tertiary;
      Neutral = neutral;
      NeutralVariant = neutralVariant;
    }

    public TonalPalette Primary { get; }
    public TonalPalette Secondary { get; }
    public TonalPalette Tertiary { get; }
    public TonalPalette Neutral { get; }
    public TonalPalette NeutralVariant { get; }
  }
}