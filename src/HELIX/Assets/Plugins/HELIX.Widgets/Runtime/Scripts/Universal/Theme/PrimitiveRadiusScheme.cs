using System;

namespace HELIX.Widgets.Universal.Theme
{
    [Serializable]
    public class PrimitiveRadiusScheme {
        public static PrimitiveRadiusScheme Default = new();

        public float factor = 1.5f;

        public virtual float Radius1 => 3 * factor;
        public virtual float Radius2 => 4 * factor;
        public virtual float Radius3 => 6 * factor;
        public virtual float Radius4 => 8 * factor;
        public virtual float Radius5 => 12 * factor;
        public virtual float Radius6 => 16 * factor;
    }
}