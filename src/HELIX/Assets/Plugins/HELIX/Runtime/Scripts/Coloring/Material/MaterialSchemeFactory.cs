using UnityEngine;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Unity-facing helpers and extensions for Material Color Utilities.
    ///   This file is intended to sit on top of the lower-level ported types:
    ///   Hct, Cam16, Blend, DynamicScheme, DynamicColor, TonalPalette,
    ///   MaterialDynamicColors, Contrast, Variant, and the scheme preset classes.
    /// </summary>
    internal static class MaterialSchemeFactory {
        /// <summary>
        ///   Creates a dynamic scheme from an ARGB seed color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
      int sourceArgb,
      Variant variant = Variant.TonalSpot,
      bool isDark = false,
      double contrastLevel = 0.0
    ) {
      return CreateScheme(Hct.FromInt(sourceArgb), variant, isDark, contrastLevel);
    }

        /// <summary>
        ///   Creates a dynamic scheme from a Unity gamma-space color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
      Color sourceColor,
      Variant variant = Variant.TonalSpot,
      bool isDark = false,
      double contrastLevel = 0.0
    ) {
      return CreateScheme(sourceColor.ToArgb(), variant, isDark, contrastLevel);
    }

        /// <summary>
        ///   Creates a dynamic scheme from an HCT seed color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
      Hct sourceColorHct,
      Variant variant = Variant.TonalSpot,
      bool isDark = false,
      double contrastLevel = 0.0
    ) {
      return variant switch {
        Variant.Monochrome => new SchemeMonochrome(sourceColorHct, isDark, contrastLevel),
        Variant.Neutral => new SchemeNeutral(sourceColorHct, isDark, contrastLevel),
        Variant.TonalSpot => new SchemeTonalSpot(sourceColorHct, isDark, contrastLevel),
        Variant.Vibrant => new SchemeVibrant(sourceColorHct, isDark, contrastLevel),
        Variant.Expressive => new SchemeExpressive(sourceColorHct, isDark, contrastLevel),
        Variant.Content => new SchemeContent(sourceColorHct, isDark, contrastLevel),
        Variant.Fidelity => new SchemeFidelity(sourceColorHct, isDark, contrastLevel),
        Variant.Rainbow => new SchemeRainbow(sourceColorHct, isDark, contrastLevel),
        Variant.FruitSalad => new SchemeFruitSalad(sourceColorHct, isDark, contrastLevel),
        _ => new SchemeTonalSpot(sourceColorHct, isDark, contrastLevel)
      };
    }
  }
}