using System;

namespace HELIX.Widgets.Universal.Theme {
    [Serializable]
    public class PrimitiveTypographySchema {
        public static PrimitiveTypographySchema Default = new();

        public float factor = 1.5f;
        public float lineHeightFactor = 1.25f;
        public float em = 16f;

        public virtual float FontSize1 => factor * 12f; // Caption
        public virtual float FontSize2 => factor * 14f;
        public virtual float FontSize3 => factor * 16f; // Body
        public virtual float FontSize4 => factor * 18f;
        public virtual float FontSize5 => factor * 20f;
        public virtual float FontSize6 => factor * 24f; // Heading
        public virtual float FontSize7 => factor * 28f;
        public virtual float FontSize8 => factor * 35f;
        public virtual float FontSize9 => factor * 60f; // Display

        public virtual float LetterSpacing1 => em * 0.0025f * factor;
        public virtual float LetterSpacing2 => 0;
        public virtual float LetterSpacing3 => 0;
        public virtual float LetterSpacing4 => em * -0.0025f * factor;
        public virtual float LetterSpacing5 => em * -0.005f * factor;
        public virtual float LetterSpacing6 => em * -0.00625f * factor;
        public virtual float LetterSpacing7 => em * -0.0075f * factor;
        public virtual float LetterSpacing8 => em * -0.01f * factor;
        public virtual float LetterSpacing9 => em * -0.025f * factor;

        public virtual float LineHeight1 => factor * lineHeightFactor * 16f;
        public virtual float LineHeight2 => factor * lineHeightFactor * 20f;
        public virtual float LineHeight3 => factor * lineHeightFactor * 24f;
        public virtual float LineHeight4 => factor * lineHeightFactor * 26f;
        public virtual float LineHeight5 => factor * lineHeightFactor * 28f;
        public virtual float LineHeight6 => factor * lineHeightFactor * 30f;
        public virtual float LineHeight7 => factor * lineHeightFactor * 36f;
        public virtual float LineHeight8 => factor * lineHeightFactor * 40f;
        public virtual float LineHeight9 => factor * lineHeightFactor * 60f;
    }
}