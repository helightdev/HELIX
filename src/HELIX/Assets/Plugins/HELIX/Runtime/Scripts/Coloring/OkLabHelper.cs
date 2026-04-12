using Unity.Mathematics;

namespace HELIX.Coloring {
    public static class OkLabHelper {
        private static readonly float3x3 _rgbToLms = new(
            0.4122214708f,
            0.5363325363f,
            0.0514459929f,
            0.2119034982f,
            0.6806995451f,
            0.1073969566f,
            0.0883024619f,
            0.2817188376f,
            0.6299787005f
        );

        private static readonly float3x3 _lmsToLab = new(
            0.2104542553f,
            0.7936177850f,
            -0.0040720468f,
            1.9779984951f,
            -2.4285922050f,
            0.4505937099f,
            0.0259040371f,
            0.7827717662f,
            -0.8086757660f
        );

        private static readonly float3x3 _labToLms = new(
            1.0f,
            +0.3963377774f,
            +0.2158037573f,
            1.0f,
            -0.1055613458f,
            -0.0638541728f,
            1.0f,
            -0.0894841775f,
            -1.2914855480f
        );

        private static readonly float3x3 _lmsToRGB = new(
            +4.0767416621f,
            -3.3077115913f,
            +0.2309699292f,
            -1.2684380046f,
            +2.6097574011f,
            -0.3413193965f,
            -0.0041960863f,
            -0.7034186147f,
            +1.7076147010f
        );

        public static float3 FromLinearRgb(float3 rgb) {
            var lms = math.mul(_rgbToLms, rgb);
            var nonLinear = math.pow(lms, 1f / 3f);
            return math.mul(_lmsToLab, nonLinear);
        }

        public static float3 ToLinearRgb(float3 lab) {
            var nonLinear = math.mul(_labToLms, lab);
            var lms = nonLinear * nonLinear * nonLinear;
            return math.mul(_lmsToRGB, lms);
        }

        public static float3 ToLch(float3 lab) {
            var degrees = math.degrees(math.atan2(lab.z, lab.y));
            return new float3(lab.x, math.length(lab.yz), degrees);
        }

        public static float3 FromLch(float3 lch) {
            var rad = math.radians(lch.z);
            return new float3(lch.x, lch.y * math.cos(rad), lch.y * math.sin(rad));
        }
    }
}