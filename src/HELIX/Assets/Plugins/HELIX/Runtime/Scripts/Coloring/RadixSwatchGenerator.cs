using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Coloring {
    public static class RadixSwatchGenerator {
        private const int _radixStepCount = 12;
        private const int _radixPivotIndex = 8;

        [Serializable]
        public struct GenerationOptions {
            public float lightnessInfluence;
            public float chromaInfluence;
            public float subtleChromaMultiplier;
            public float textChromaMultiplier;
            public float chromaScale;
            public float textLightnessShift;
            public float subtleLightnessShift;
            public float generalLightnessShift;

            public static GenerationOptions Default =>
                new GenerationOptions {
                    lightnessInfluence = 0.65f,
                    chromaInfluence = 0.65f,
                    subtleChromaMultiplier = 0.90f,
                    textChromaMultiplier = 0.96f,
                    chromaScale = 1.0f,
                    textLightnessShift = 0.85f,
                    subtleLightnessShift = 0.90f,
                    generalLightnessShift = 0.80f
                };
        }

        public static Color[] Generate(
            LchColor seed,
            Brightness brightness,
            RadixSwatch template = null,
            GenerationOptions? options = null
        ) {
            template ??= PickClosestTemplate(seed, Colors.All);
            var opt = options ?? GenerationOptions.Default;
            var templateScale = template.GetLch(brightness);

            var templatePivot = templateScale[_radixPivotIndex];

            var pivotHue = StableHue(seed.h, templatePivot.h);
            var pivotLightness = math.lerp(templatePivot.l, seed.l, opt.lightnessInfluence);
            var pivotChroma = math.lerp(templatePivot.c, seed.c, opt.chromaInfluence);

            var result = new LchColor[_radixStepCount];

            for (var i = 0; i < _radixStepCount; i++) {
                var src = templateScale[i];
                var relativeLightness = src.l - templatePivot.l;
                var chromaRatio = templatePivot.c > 1e-5f ? src.c / templatePivot.c : 0f;

                var lightnessPressure = i switch {
                    <= 1  => opt.subtleLightnessShift,
                    >= 10 => opt.textLightnessShift,
                    _     => opt.generalLightnessShift
                };

                var l = math.lerp(pivotLightness + relativeLightness, src.l, lightnessPressure);
                var c = pivotChroma * chromaRatio * opt.chromaScale;
                var h = pivotHue;

                if (i <= 4) c *= opt.subtleChromaMultiplier;
                if (i >= 10) c *= opt.textChromaMultiplier;

                c = math.max(0f, c);
                l = math.clamp(l, 0f, 1f);

                result[i] = new LchColor(l, c, h);
            }

            return result.Select(x => x.ToGamma()).ToArray();
        }

        public static RadixSwatch PickClosestTemplate(
            LchColor seedSwatch,
            IReadOnlyList<RadixSwatch> templates = null
        ) {
            templates ??= Colors.All;

            var bestScore = float.MaxValue;
            var best = templates[0];

            foreach (var template in templates) {
                var score = PointScore(
                    seedSwatch,
                    template.lightLch[8],
                    hueWeight: 1.00f,
                    chromaWeight: 1.35f,
                    lightnessWeight: 0.45f
                );

                if (score < bestScore) {
                    bestScore = score;
                    best = template;
                }
            }

            return best;
        }

        private static float PointScore(
            LchColor seed,
            LchColor template,
            float hueWeight,
            float chromaWeight,
            float lightnessWeight
        ) {
            var hueDist = HueDistance(seed.h, template.h) / 180f;
            var chromaDist = math.abs(seed.c - template.c);
            var lightnessDist = math.abs(seed.l - template.l);

            var neutralFactor = math.saturate(math.max(seed.c, template.c) / 0.08f);
            hueDist *= math.lerp(0.08f, 1f, neutralFactor);

            return
                hueDist * hueWeight +
                chromaDist * chromaWeight +
                lightnessDist * lightnessWeight;
        }

        private static float StableHue(float seedHue, float fallbackHue) {
            if (float.IsNaN(seedHue) || float.IsInfinity(seedHue)) return fallbackHue;
            return WrapHue(seedHue);
        }

        private static float WrapHue(float h) {
            h %= 360f;
            if (h < 0f) h += 360f;
            return h;
        }

        private static float HueDistance(float a, float b) {
            var d = math.abs(WrapHue(a) - WrapHue(b));
            return math.min(d, 360f - d);
        }

        /*

                 public static Color[] Generate(
               ColorSwatch swatch,
               bool darkMode,
               RadixSwatch radixSwatch = null,
               GenerationOptions? options = null
           ) {
               radixSwatch ??= PickClosestTemplate(swatch, Templates.All);
               return Generate((LchColor)swatch.LabWeights[5], darkMode, radixSwatch, options);
           }


                 public static RadixSwatch PickClosestTemplate(
               ColorSwatch seedSwatch,
               IReadOnlyList<RadixSwatch> templates = null
           ) {
               templates ??= Templates.All;

               var bestScore = float.MaxValue;
               var best = templates[0];

               foreach (var template in templates) {
                   var score = ScoreTemplate(seedSwatch, template);

                   if (score < bestScore) {
                       bestScore = score;
                       best = template;
                   }
               }

               return best;
           }


                private static float ScoreTemplate(ColorSwatch seedSwatch, RadixSwatch radixSwatch) {
               var tpl = radixSwatch.lightLch;

               var s50 = (LchColor)seedSwatch.LabWeights[0];
               var s200 = (LchColor)seedSwatch.LabWeights[2];
               var s500 = (LchColor)seedSwatch.LabWeights[5];
               var s700 = (LchColor)seedSwatch.LabWeights[7];
               var s950 = (LchColor)seedSwatch.LabWeights[10];

               var t1 = tpl[0];
               var t3 = tpl[2];
               var t9 = tpl[8];
               var t11 = tpl[10];
               var t12 = tpl[11];

               var pivotScore = PointScore(
                   s500,
                   t9,
                   hueWeight: 1.00f,
                   chromaWeight: 1.35f,
                   lightnessWeight: 0.45f
               );

               var anchorScore =
                   PointScore(s50, t1, hueWeight: 0.20f, chromaWeight: 0.90f, lightnessWeight: 0.80f) * 0.80f +
                   PointScore(s200, t3, hueWeight: 0.30f, chromaWeight: 1.00f, lightnessWeight: 0.70f) * 1.00f +
                   PointScore(s700, t11, hueWeight: 0.55f, chromaWeight: 0.90f, lightnessWeight: 0.55f) * 0.90f +
                   PointScore(s950, t12, hueWeight: 0.45f, chromaWeight: 0.70f, lightnessWeight: 0.45f) * 0.65f;

               var seedChromaProfile = new[] { s50.c, s200.c, s500.c, s700.c, s950.c };
               var tplChromaProfile = new[] { t1.c, t3.c, t9.c, t11.c, t12.c };
               var chromaShapeScore = ProfileShapeScore(seedChromaProfile, tplChromaProfile);

               var seedLightProfile = new[] { s50.l, s200.l, s500.l, s700.l, s950.l };
               var tplLightProfile = new[] { t1.l, t3.l, t9.l, t11.l, t12.l };
               var lightShapeScore = ProfileShapeScore(seedLightProfile, tplLightProfile);

               var seedHueDrift = HuePathScore(s50.h, s200.h, s500.h, s700.h, s950.h);
               var tplHueDrift = HuePathScore(t1.h, t3.h, t9.h, t11.h, t12.h);
               var hueShapeScore = math.abs(seedHueDrift - tplHueDrift) / 180f;

               var seedPeakBias = PeakBias(seedChromaProfile);
               var tplPeakBias = PeakBias(tplChromaProfile);
               var peakBiasScore = math.abs(seedPeakBias - tplPeakBias);

               return pivotScore * 1.75f +
                      anchorScore * 1.10f +
                      chromaShapeScore * 0.95f +
                      lightShapeScore * 0.45f +
                      hueShapeScore * 0.55f +
                      peakBiasScore * 0.75f;
           }

                 private static float[] NormalizeProfile(float[] values) {
                 var min = values.Min();
                 var max = values.Max();
                 var range = math.max(1e-5f, max - min);

                 var result = new float[values.Length];
                 for (var i = 0; i < values.Length; i++) { result[i] = (values[i] - min) / range; }

                 return result;
             }

             private static float ProfileShapeScore(float[] a, float[] b) {
                 var an = NormalizeProfile(a);
                 var bn = NormalizeProfile(b);

                 var sum = 0f;
                 for (var i = 0; i < an.Length; i++) { sum += math.abs(an[i] - bn[i]); }

                 return sum / an.Length;
             }

             private static float HuePathScore(params float[] hues) {
                 var total = 0f;
                 for (var i = 0; i < hues.Length - 1; i++) { total += HueDistance(hues[i], hues[i + 1]); }

                 return total;
             }

             private static float PeakBias(float[] values) {
                 var sum = 0f;
                 var weighted = 0f;

                 for (var i = 0; i < values.Length; i++) {
                     sum += values[i];
                     weighted += values[i] * i;
                 }

                 if (sum <= 1e-5f) return 0f;
                 return weighted / sum / (values.Length - 1);
             }

           */
    }
}