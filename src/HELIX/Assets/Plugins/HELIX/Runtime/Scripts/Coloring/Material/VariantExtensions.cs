namespace HELIX.Coloring.Material {
  public static class VariantExtensions {
    public static string Label(this Variant variant) {
      return variant switch {
        Variant.Monochrome => "monochrome",
        Variant.Neutral => "neutral",
        Variant.TonalSpot => "tonal spot",
        Variant.Vibrant => "vibrant",
        Variant.Expressive => "expressive",
        Variant.Content => "content",
        Variant.Fidelity => "fidelity",
        Variant.Rainbow => "rainbow",
        Variant.FruitSalad => "fruit salad",
        _ => variant.ToString()
      };
    }

    public static string Description(this Variant variant) {
      return variant switch {
        Variant.Monochrome => "All colors are grayscale, no chroma.",
        Variant.Neutral => "Close to grayscale, a hint of chroma.",
        Variant.TonalSpot =>
          "Pastel tokens, low chroma palettes (32).\nDefault Material You theme at 2021 launch.",
        Variant.Vibrant =>
          "Pastel colors, high chroma palettes. (max).\nThe primary palette's chroma is at maximum.\nUse Fidelity instead if tokens should alter their tone to match the palette vibrancy.",
        Variant.Expressive =>
          "Pastel colors, medium chroma palettes.\nThe primary palette's hue is different from source color, for variety.",
        Variant.Content =>
          "Almost identical to Fidelity.\nTokens and palettes match source color.\nPrimary Container is source color, adjusted to ensure contrast with surfaces.\n\nTertiary palette is analogue of source color.\nFound by dividing color wheel by 6, then finding the 2 colors adjacent to source.\nThe one that increases hue is used.",
        Variant.Fidelity =>
          "Tokens and palettes match source color.\nPrimary Container is source color, adjusted to ensure contrast with surfaces.\nFor example, if source color is black, it is lightened so it doesn't match surfaces in dark mode.\n\nTertiary palette is complement of source color.",
        Variant.Rainbow => "A playful theme - the source color's hue does not appear in the theme.",
        Variant.FruitSalad => "A playful theme - the source color's hue does not appear in the theme.",
        _ => string.Empty
      };
    }
  }
}