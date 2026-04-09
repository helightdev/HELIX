using System;
using HELIX.Coloring;
using UnityEngine;

namespace HELIX.Types {
    public struct BorderSide : IEquatable<BorderSide> {
        public float width;
        public Color color;

        public BorderSide(float width, Color color) {
            this.width = width;
            this.color = color;
        }

        public bool Equals(BorderSide other) {
            return width.Equals(other.width) && color.Equals(other.color);
        }

        public override bool Equals(object obj) {
            return obj is BorderSide other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(width, color);
        }

        public override string ToString() {
            return $"BorderSide({nameof(width)}: {width}, {nameof(color)}: {color.ToHex()})";
        }
    }
}