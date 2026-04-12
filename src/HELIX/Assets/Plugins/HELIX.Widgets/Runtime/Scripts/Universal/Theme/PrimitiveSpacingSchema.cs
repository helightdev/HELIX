using System;

namespace HELIX.Widgets.Universal.Theme {
    [Serializable]
    public class PrimitiveSpacingSchema {
        public static PrimitiveSpacingSchema Default = new();

        public float basis = 5f;
        public float factor = 1f;

        public virtual float Space1 => basis * factor;
        public virtual float Space2 => basis * factor * 2;
        public virtual float Space3 => basis * factor * 3;
        public virtual float Space4 => basis * factor * 4;

        public virtual float Space5 => basis * factor * 6;
        public virtual float Space6 => basis * factor * 8;
        public virtual float Space7 => basis * factor * 10;
        public virtual float Space8 => basis * factor * 12;
        public virtual float Space9 => basis * factor * 16;
    }
}