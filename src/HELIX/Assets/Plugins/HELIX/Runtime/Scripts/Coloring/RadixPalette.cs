using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace HELIX.Coloring {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class RadixPalette {
    public readonly Brightness brightness;
    public readonly Color C1; // Background
    public readonly Color C10;
    public readonly Color C11; // Text
    public readonly Color C12;
    public readonly Color C2;
    public readonly Color C3; // Interactive
    public readonly Color C4;
    public readonly Color C5;
    public readonly Color C6; // Borders
    public readonly Color C7;
    public readonly Color C8;
    public readonly Color C9; // Solid

    public RadixPalette(Color[] colors, Brightness brightness) {
      if (colors == null || colors.Length != RadixSwatch.StepCount)
        throw new ArgumentException($"Brightness swatch must contain exactly {RadixSwatch.StepCount} colors.");

      C1 = colors[0];
      C2 = colors[1];
      C3 = colors[2];
      C4 = colors[3];
      C5 = colors[4];
      C6 = colors[5];
      C7 = colors[6];
      C8 = colors[7];
      C9 = colors[8];
      C10 = colors[9];
      C11 = colors[10];
      C12 = colors[11];
      this.brightness = brightness;
    }
  }
}