using Unity.Mathematics;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Check and/or fix universally disliked colors.
    /// </summary>
    public static class DislikeAnalyzer {
        /// <summary>
        ///     Returns true if hct is disliked.
        /// </summary>
        public static bool IsDisliked(Hct hct) {
            var huePasses = math.round(hct.Hue) >= 90.0 && math.round(hct.Hue) <= 111.0;
            var chromaPasses = math.round(hct.Chroma) > 16.0;
            var tonePasses = math.round(hct.Tone) < 65.0;

            return huePasses && chromaPasses && tonePasses;
        }

        /// <summary>
        ///     If hct is disliked, lighten it to make it likable.
        /// </summary>
        public static Hct FixIfDisliked(Hct hct) {
            if (IsDisliked(hct)) return Hct.From(hct.Hue, hct.Chroma, 70.0);

            return hct;
        }
    }
}