using System;
using System.Collections.Generic;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Constructed by a set of values representing the current UI state and
    ///   provides a set of TonalPalettes.
    /// </summary>
    public class DynamicScheme {
    public DynamicScheme(
      Hct sourceColorHct,
      Variant variant,
      bool isDark,
      TonalPalette primaryPalette,
      TonalPalette secondaryPalette,
      TonalPalette tertiaryPalette,
      TonalPalette neutralPalette,
      TonalPalette neutralVariantPalette,
      double contrastLevel = 0.0,
      TonalPalette errorPalette = null
    ) {
      SourceColorHct = sourceColorHct;
      SourceColorArgb = sourceColorHct.ToInt();
      Variant = variant;
      ContrastLevel = contrastLevel;
      IsDark = isDark;
      PrimaryPalette = primaryPalette;
      SecondaryPalette = secondaryPalette;
      TertiaryPalette = tertiaryPalette;
      NeutralPalette = neutralPalette;
      NeutralVariantPalette = neutralVariantPalette;
      ErrorPalette = errorPalette ?? TonalPalette.Of(25.0, 84.0);
    }

    public int SourceColorArgb { get; }
    public Hct SourceColorHct { get; }
    public Variant Variant { get; }
    public bool IsDark { get; }
    public double ContrastLevel { get; }

    public TonalPalette PrimaryPalette { get; }
    public TonalPalette SecondaryPalette { get; }
    public TonalPalette TertiaryPalette { get; }
    public TonalPalette NeutralPalette { get; }
    public TonalPalette NeutralVariantPalette { get; }
    public TonalPalette ErrorPalette { get; }

    public int PrimaryPaletteKeyColor => GetArgb(MaterialDynamicColors.PrimaryPaletteKeyColor);
    public int SecondaryPaletteKeyColor => GetArgb(MaterialDynamicColors.SecondaryPaletteKeyColor);
    public int TertiaryPaletteKeyColor => GetArgb(MaterialDynamicColors.TertiaryPaletteKeyColor);
    public int NeutralPaletteKeyColor => GetArgb(MaterialDynamicColors.NeutralPaletteKeyColor);
    public int NeutralVariantPaletteKeyColor => GetArgb(MaterialDynamicColors.NeutralVariantPaletteKeyColor);
    public int Background => GetArgb(MaterialDynamicColors.Background);
    public int OnBackground => GetArgb(MaterialDynamicColors.OnBackground);
    public int Surface => GetArgb(MaterialDynamicColors.Surface);
    public int SurfaceDim => GetArgb(MaterialDynamicColors.SurfaceDim);
    public int SurfaceBright => GetArgb(MaterialDynamicColors.SurfaceBright);
    public int SurfaceContainerLowest => GetArgb(MaterialDynamicColors.SurfaceContainerLowest);
    public int SurfaceContainerLow => GetArgb(MaterialDynamicColors.SurfaceContainerLow);
    public int SurfaceContainer => GetArgb(MaterialDynamicColors.SurfaceContainer);
    public int SurfaceContainerHigh => GetArgb(MaterialDynamicColors.SurfaceContainerHigh);
    public int SurfaceContainerHighest => GetArgb(MaterialDynamicColors.SurfaceContainerHighest);
    public int OnSurface => GetArgb(MaterialDynamicColors.OnSurface);
    public int SurfaceVariant => GetArgb(MaterialDynamicColors.SurfaceVariant);
    public int OnSurfaceVariant => GetArgb(MaterialDynamicColors.OnSurfaceVariant);
    public int InverseSurface => GetArgb(MaterialDynamicColors.InverseSurface);
    public int InverseOnSurface => GetArgb(MaterialDynamicColors.InverseOnSurface);
    public int Outline => GetArgb(MaterialDynamicColors.Outline);
    public int OutlineVariant => GetArgb(MaterialDynamicColors.OutlineVariant);
    public int Shadow => GetArgb(MaterialDynamicColors.Shadow);
    public int Scrim => GetArgb(MaterialDynamicColors.Scrim);
    public int SurfaceTint => GetArgb(MaterialDynamicColors.SurfaceTint);
    public int Primary => GetArgb(MaterialDynamicColors.Primary);
    public int OnPrimary => GetArgb(MaterialDynamicColors.OnPrimary);
    public int PrimaryContainer => GetArgb(MaterialDynamicColors.PrimaryContainer);
    public int OnPrimaryContainer => GetArgb(MaterialDynamicColors.OnPrimaryContainer);
    public int InversePrimary => GetArgb(MaterialDynamicColors.InversePrimary);
    public int Secondary => GetArgb(MaterialDynamicColors.Secondary);
    public int OnSecondary => GetArgb(MaterialDynamicColors.OnSecondary);
    public int SecondaryContainer => GetArgb(MaterialDynamicColors.SecondaryContainer);
    public int OnSecondaryContainer => GetArgb(MaterialDynamicColors.OnSecondaryContainer);
    public int Tertiary => GetArgb(MaterialDynamicColors.Tertiary);
    public int OnTertiary => GetArgb(MaterialDynamicColors.OnTertiary);
    public int TertiaryContainer => GetArgb(MaterialDynamicColors.TertiaryContainer);
    public int OnTertiaryContainer => GetArgb(MaterialDynamicColors.OnTertiaryContainer);
    public int Error => GetArgb(MaterialDynamicColors.Error);
    public int OnError => GetArgb(MaterialDynamicColors.OnError);
    public int ErrorContainer => GetArgb(MaterialDynamicColors.ErrorContainer);
    public int OnErrorContainer => GetArgb(MaterialDynamicColors.OnErrorContainer);
    public int PrimaryFixed => GetArgb(MaterialDynamicColors.PrimaryFixed);
    public int PrimaryFixedDim => GetArgb(MaterialDynamicColors.PrimaryFixedDim);
    public int OnPrimaryFixed => GetArgb(MaterialDynamicColors.OnPrimaryFixed);
    public int OnPrimaryFixedVariant => GetArgb(MaterialDynamicColors.OnPrimaryFixedVariant);
    public int SecondaryFixed => GetArgb(MaterialDynamicColors.SecondaryFixed);
    public int SecondaryFixedDim => GetArgb(MaterialDynamicColors.SecondaryFixedDim);
    public int OnSecondaryFixed => GetArgb(MaterialDynamicColors.OnSecondaryFixed);
    public int OnSecondaryFixedVariant => GetArgb(MaterialDynamicColors.OnSecondaryFixedVariant);
    public int TertiaryFixed => GetArgb(MaterialDynamicColors.TertiaryFixed);
    public int TertiaryFixedDim => GetArgb(MaterialDynamicColors.TertiaryFixedDim);
    public int OnTertiaryFixed => GetArgb(MaterialDynamicColors.OnTertiaryFixed);
    public int OnTertiaryFixedVariant => GetArgb(MaterialDynamicColors.OnTertiaryFixedVariant);

    public static double GetRotatedHue(
      Hct sourceColor,
      IReadOnlyList<double> hues,
      IReadOnlyList<double> rotations
    ) {
      var sourceHue = sourceColor.Hue;

      if (hues.Count != rotations.Count) throw new ArgumentException("hues.length must equal rotations.length");

      if (rotations.Count == 1) return MathUtils.SanitizeDegreesDouble(sourceColor.Hue + rotations[0]);

      var size = hues.Count;
      for (var i = 0; i <= size - 2; i++) {
        var thisHue = hues[i];
        var nextHue = hues[i + 1];
        if (thisHue < sourceHue && sourceHue < nextHue)
          return MathUtils.SanitizeDegreesDouble(sourceHue + rotations[i]);
      }

      return sourceHue;
    }

    public Hct GetHct(DynamicColor dynamicColor) {
      return dynamicColor.GetHct(this);
    }

    public int GetArgb(DynamicColor dynamicColor) {
      return dynamicColor.GetArgb(this);
    }
  }
}