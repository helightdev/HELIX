using System;
using System.Collections.Generic;
using HELIX.Coloring.Material;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace HELIX.Widgets.Tests {
  public class MaterialColorUtilitiesFixtureTests {
    private static List<SeedFixture> _fixtures;

    [OneTimeSetUp]
    public void OneTimeSetUp() {
      var textAsset = Resources.Load<TextAsset>("materialColorFixture");
      _fixtures = JsonConvert.DeserializeObject<List<SeedFixture>>(textAsset.text);
      Assert.NotNull(_fixtures, "Failed to deserialize fixture JSON.");
      Assert.IsNotEmpty(_fixtures, "Fixture JSON contained no entries.");
    }

    [Test]
    public void TonalPalettes_MatchFlutterFixtures() {
      foreach (var fixture in _fixtures) {
        var seedArgb = unchecked((int)fixture.seed);
        var seedHct = Hct.FromInt(seedArgb);
        var palette = TonalPalette.FromHct(seedHct);

        foreach (var kvp in fixture.palette) {
          var tone = int.Parse(kvp.Key);
          var expectedArgb = unchecked((int)kvp.Value);
          var actualArgb = palette.Get(tone);

          AssertArgbEqual(
            expectedArgb,
            actualArgb,
            $"Palette mismatch for seed {Hex(seedArgb)}, tone {tone}"
          );
        }
      }
    }

    [Test]
    public void MaterialColorSwatches_MatchFlutterFixtures() {
      foreach (var fixture in _fixtures) {
        var seedArgb = unchecked((int)fixture.seed);
        var materialColor = new TonalMaterialColor(seedArgb);

        foreach (var kvp in fixture.swatch) {
          var weight = int.Parse(kvp.Key);
          var expectedArgb = unchecked((int)kvp.Value);
          var actualArgb = materialColor[weight].ToArgb();

          AssertArgbEqual(
            expectedArgb,
            actualArgb,
            $"Swatch mismatch for seed {Hex(seedArgb)}, weight {weight}"
          );
        }
      }
    }

    [Test]
    public void DynamicSchemes_MatchFlutterFixtures() {
      foreach (var fixture in _fixtures) {
        var seedArgb = unchecked((int)fixture.seed);
        var seedHct = Hct.FromInt(seedArgb);

        foreach (var schemeFixture in fixture.schemes) {
          var variant = ParseVariant(schemeFixture.variant);
          var scheme = CreateScheme(
            seedHct,
            variant,
            schemeFixture.isDark,
            schemeFixture.contrast
          );

          foreach (var roleKvp in schemeFixture.roles) {
            var roleName = roleKvp.Key;
            var expectedArgb = unchecked((int)roleKvp.Value);
            var actualArgb = ResolveRoleArgb(scheme, roleName);

            AssertArgbEqual(
              expectedArgb,
              actualArgb,
              $"Role mismatch for seed {Hex(seedArgb)}, variant {schemeFixture.variant}, dark={schemeFixture.isDark}, contrast={schemeFixture.contrast}, role={roleName}"
            );
          }
        }
      }
    }

    private static DynamicScheme CreateScheme(Hct seedHct, Variant variant, bool isDark, double contrastLevel) {
      return variant switch {
        Variant.Monochrome => new SchemeMonochrome(seedHct, isDark, contrastLevel),
        Variant.Neutral => new SchemeNeutral(seedHct, isDark, contrastLevel),
        Variant.TonalSpot => new SchemeTonalSpot(seedHct, isDark, contrastLevel),
        Variant.Vibrant => new SchemeVibrant(seedHct, isDark, contrastLevel),
        Variant.Expressive => new SchemeExpressive(seedHct, isDark, contrastLevel),
        Variant.Content => new SchemeContent(seedHct, isDark, contrastLevel),
        Variant.Fidelity => new SchemeFidelity(seedHct, isDark, contrastLevel),
        Variant.Rainbow => new SchemeRainbow(seedHct, isDark, contrastLevel),
        Variant.FruitSalad => new SchemeFruitSalad(seedHct, isDark, contrastLevel),
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
      };
    }

    private static Variant ParseVariant(string variant) {
      return variant switch {
        "monochrome" => Variant.Monochrome,
        "neutral" => Variant.Neutral,
        "tonalSpot" => Variant.TonalSpot,
        "vibrant" => Variant.Vibrant,
        "expressive" => Variant.Expressive,
        "content" => Variant.Content,
        "fidelity" => Variant.Fidelity,
        "rainbow" => Variant.Rainbow,
        "fruitSalad" => Variant.FruitSalad,
        _ => throw new ArgumentException($"Unknown variant '{variant}'")
      };
    }

    private static int ResolveRoleArgb(DynamicScheme scheme, string roleName) {
      return roleName switch {
        "primary" => scheme.Primary,
        "onPrimary" => scheme.OnPrimary,
        "primaryContainer" => scheme.PrimaryContainer,
        "onPrimaryContainer" => scheme.OnPrimaryContainer,
        "secondary" => scheme.Secondary,
        "onSecondary" => scheme.OnSecondary,
        "secondaryContainer" => scheme.SecondaryContainer,
        "onSecondaryContainer" => scheme.OnSecondaryContainer,
        "tertiary" => scheme.Tertiary,
        "onTertiary" => scheme.OnTertiary,
        "tertiaryContainer" => scheme.TertiaryContainer,
        "onTertiaryContainer" => scheme.OnTertiaryContainer,
        "background" => scheme.Background,
        "onBackground" => scheme.OnBackground,
        "surface" => scheme.Surface,
        "onSurface" => scheme.OnSurface,
        "surfaceVariant" => scheme.SurfaceVariant,
        "onSurfaceVariant" => scheme.OnSurfaceVariant,
        "outline" => scheme.Outline,
        "error" => scheme.Error,
        "onError" => scheme.OnError,
        _ => throw new ArgumentException($"Unknown role '{roleName}'")
      };
    }

    private static void AssertArgbEqual(int expectedArgb, int actualArgb, string context) {
      if (expectedArgb == actualArgb) return;

      var ea = MaterialColorUtils.AlphaFromArgb(expectedArgb);
      var er = MaterialColorUtils.RedFromArgb(expectedArgb);
      var eg = MaterialColorUtils.GreenFromArgb(expectedArgb);
      var eb = MaterialColorUtils.BlueFromArgb(expectedArgb);

      var aa = MaterialColorUtils.AlphaFromArgb(actualArgb);
      var ar = MaterialColorUtils.RedFromArgb(actualArgb);
      var ag = MaterialColorUtils.GreenFromArgb(actualArgb);
      var ab = MaterialColorUtils.BlueFromArgb(actualArgb);

      Assert.Fail(
        $"{context}\n" +
        $"Expected: {Hex(expectedArgb)}  (A:{ea} R:{er} G:{eg} B:{eb})\n" +
        $"Actual:   {Hex(actualArgb)}  (A:{aa} R:{ar} G:{ag} B:{ab})\n" +
        $"Delta:                 (A:{aa - ea} R:{ar - er} G:{ag - eg} B:{ab - eb})"
      );
    }

    private static string Hex(int argb) {
      return $"0x{unchecked((uint)argb):X8}";
    }

    [Serializable]
    private sealed class SeedFixture {
      public long seed;
      public List<SchemeFixture> schemes;
      public Dictionary<string, long> palette;
      public Dictionary<string, long> swatch;
    }

    [Serializable]
    private sealed class SchemeFixture {
      public string variant;
      public bool isDark;
      public double contrast;
      public Dictionary<string, long> roles;
    }
  }
}