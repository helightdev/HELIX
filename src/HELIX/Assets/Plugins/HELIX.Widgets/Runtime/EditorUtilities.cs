using System.Runtime.CompilerServices;
using UnityEngine;

namespace HELIX.Widgets {
    internal abstract class EditorUtilities {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect SwizzleToRect(Vector4 vector) {
            return new Rect(vector.x, vector.y, vector.w, vector.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SwizzleToVector4(Rect rect) {
            return new Vector4(rect.x, rect.y, rect.height, rect.width);
        }
    }
}