using System;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Types {
    [Serializable]
    public struct Alignment {
        [Range(-1f, 1f)]
        public float x;

        [Range(-1f, 1f)]
        public float y;

        public Alignment(float x, float y) {
            this.x = Mathf.Clamp(x, -1f, 1f);
            this.y = Mathf.Clamp(y, -1f, 1f);
        }

        public Vector2 GetOffsetCoefficients() {
            var horizontal = math.remap(-1f, 1f, 0f, 1f, x);
            var vertical = math.remap(-1f, 1f, 0f, 1f, y);
            return new Vector2(horizontal, vertical);
        }

        public static readonly Alignment TopCenter = new Alignment(0f, -1f);
        public static readonly Alignment TopRight = new Alignment(1f, -1f);
        public static readonly Alignment TopLeft = new Alignment(-1f, -1f);
        public static readonly Alignment CenterLeft = new Alignment(-1f, 0f);
        public static readonly Alignment Center = new Alignment(0f, 0f);
        public static readonly Alignment CenterRight = new Alignment(1f, 0f);
        public static readonly Alignment BottomLeft = new Alignment(-1f, 1f);
        public static readonly Alignment BottomCenter = new Alignment(0f, 1f);
        public static readonly Alignment BottomRight = new Alignment(1f, 1f);

        public static implicit operator Alignment(Vector2 vec) => new(vec.x, vec.y);
        public static implicit operator Vector2(Alignment alignment) => new(alignment.x, alignment.y);
    }
}