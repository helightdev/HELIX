using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HELIX {
    public static class HelixConvert {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUssString(float floatValue) {
            return Convert.ToString(floatValue, CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToFloat(string ussString, out float result) {
            return float.TryParse(ussString, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUssString(Vector2 vector2) {
            return $"{ToUssString(vector2.x)},{ToUssString(vector2.y)}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUssString(Vector3 vector3) {
            return $"{ToUssString(vector3.x)},{ToUssString(vector3.y)},{ToUssString(vector3.z)}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUssString(Vector4 vector4) {
            return $"{ToUssString(vector4.x)},{ToUssString(vector4.y)},{ToUssString(vector4.z)},{ToUssString(vector4.w)}";
        }
        
        public static bool ToVector2(string ussString, out Vector2 result) {
            result = Vector2.zero;
            var parts = ussString.Split(',');
            if (parts.Length != 2) return false;
            if (!ToFloat(parts[0], out var x)) return false;
            if (!ToFloat(parts[1], out var y)) return false;
            result = new Vector2(x, y);
            return true;
        }
        
        public static bool ToVector3(string ussString, out Vector3 result) {
            result = Vector3.zero;
            var parts = ussString.Split(',');
            if (parts.Length != 3) return false;
            if (!ToFloat(parts[0], out var x)) return false;
            if (!ToFloat(parts[1], out var y)) return false;
            if (!ToFloat(parts[2], out var z)) return false;
            result = new Vector3(x, y, z);
            return true;
        }
        
        public static bool ToVector4(string ussString, out Vector4 result) {
            result = Vector4.zero;
            var parts = ussString.Split(',');
            if (parts.Length != 4) return false;
            if (!ToFloat(parts[0], out var x)) return false;
            if (!ToFloat(parts[1], out var y)) return false;
            if (!ToFloat(parts[2], out var z)) return false;
            if (!ToFloat(parts[3], out var w)) return false;
            result = new Vector4(x, y, z, w);
            return true;
        }
    }
}