using System;
using UnityEngine;

namespace HELIX.Types {
    [Serializable]
    public struct RRect : IEquatable<RRect> {
        public Rect rect;
        public Vector4 radii;
        
        public RRect(Rect rect, Vector4 radii) {
            this.rect = rect;
            this.radii = radii;
        }

        public RRect(Rect rect, float radius) : this() {
            this.rect = rect;
            radii = new Vector4(radius, radius, radius, radius);
        }

        public override string ToString() {
            return $"RRect(rect: {rect}, radii: {radii})";
        }

        public bool Equals(RRect other) {
            return rect.Equals(other.rect) && radii.Equals(other.radii);
        }

        public override bool Equals(object obj) {
            return obj is RRect other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(rect, radii);
        }
    }
}