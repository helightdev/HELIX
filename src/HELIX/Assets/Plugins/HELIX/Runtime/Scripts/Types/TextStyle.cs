using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
    public class TextStyle : IEquatable<TextStyle> {
        public StyleFont font = StyleKeyword.Initial;
        public StyleLength fontSize = StyleKeyword.Initial;
        public StyleColor color = StyleKeyword.Initial;
        public StyleEnum<TextAnchor> align = StyleKeyword.Initial;
        public StyleEnum<FontStyle> style = StyleKeyword.Initial;
        public StyleEnum<WhiteSpace> wrap = StyleKeyword.Initial;
        public StyleColor outlineColor = StyleKeyword.Initial;
        public StyleFloat outlineWidth = StyleKeyword.Initial;
        public StyleLength letterSpacing = StyleKeyword.Initial;
        public StyleLength wordSpacing = StyleKeyword.Initial;
        public StyleLength paragraphSpacing = StyleKeyword.Initial;
        public StyleEnum<TextOverflow> overflow = StyleKeyword.Initial;
        public StyleEnum<TextOverflowPosition> overflowPosition = StyleKeyword.Initial;
        public StyleTextShadow shadow = StyleKeyword.Initial;
        public StyleTextAutoSize autoSize = StyleKeyword.Initial;
        public StyleEnum<TextGeneratorType> generator = StyleKeyword.Initial;

        public void Apply(VisualElement element) {
            element.style.unityFont = font;
            element.style.fontSize = fontSize;
            element.style.color = color;
            element.style.unityTextAlign = align;
            element.style.unityTextOutlineColor = outlineColor;
            element.style.unityTextOutlineWidth = outlineWidth;
            element.style.letterSpacing = letterSpacing;
            element.style.wordSpacing = wordSpacing;
            element.style.textOverflow = overflow;
            element.style.unityFontStyleAndWeight = style;
            element.style.unityParagraphSpacing = paragraphSpacing;
            element.style.unityTextOverflowPosition = overflowPosition;
            element.style.whiteSpace = wrap;
            element.style.textShadow = shadow;
            element.style.unityTextGenerator = generator;
            if (generator.keyword != StyleKeyword.Initial && generator.value == TextGeneratorType.Advanced)
                element.style.unityTextAutoSize = autoSize;
        }

        public void Merge(TextStyle overrides) {
            if (overrides.style.keyword != StyleKeyword.Initial) style = overrides.style;
            if (overrides.font.keyword != StyleKeyword.Initial) font = overrides.font;
            if (overrides.fontSize.keyword != StyleKeyword.Initial) fontSize = overrides.fontSize;
            if (overrides.color.keyword != StyleKeyword.Initial) color = overrides.color;
            if (overrides.align.keyword != StyleKeyword.Initial) align = overrides.align;
            if (overrides.wrap.keyword != StyleKeyword.Initial) wrap = overrides.wrap;
            if (overrides.outlineColor.keyword != StyleKeyword.Initial) outlineColor = overrides.outlineColor;
            if (overrides.outlineWidth.keyword != StyleKeyword.Initial) outlineWidth = overrides.outlineWidth;
            if (overrides.letterSpacing.keyword != StyleKeyword.Initial) letterSpacing = overrides.letterSpacing;
            if (overrides.wordSpacing.keyword != StyleKeyword.Initial) wordSpacing = overrides.wordSpacing;
            if (overrides.paragraphSpacing.keyword != StyleKeyword.Initial)
                paragraphSpacing = overrides.paragraphSpacing;
            if (overrides.overflow.keyword != StyleKeyword.Initial) overflow = overrides.overflow;
            if (overrides.overflowPosition.keyword != StyleKeyword.Initial)
                overflowPosition = overrides.overflowPosition;
            if (overrides.shadow.keyword != StyleKeyword.Initial) shadow = overrides.shadow;
            if (overrides.autoSize.keyword != StyleKeyword.Initial) autoSize = overrides.autoSize;
            if (overrides.generator.keyword != StyleKeyword.Initial) generator = overrides.generator;
        }

        public bool Equals(TextStyle other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return font.Equals(other.font) && fontSize.Equals(other.fontSize) && color.Equals(other.color) &&
                   align.Equals(other.align) && style.Equals(other.style) && wrap.Equals(other.wrap) &&
                   outlineColor.Equals(other.outlineColor) && outlineWidth.Equals(other.outlineWidth) &&
                   letterSpacing.Equals(other.letterSpacing) && wordSpacing.Equals(other.wordSpacing) &&
                   paragraphSpacing.Equals(other.paragraphSpacing) && overflow.Equals(other.overflow) &&
                   overflowPosition.Equals(other.overflowPosition) && shadow.Equals(other.shadow) &&
                   autoSize.Equals(other.autoSize) && generator.Equals(other.generator);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TextStyle)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            var hashCode = new HashCode();
            hashCode.Add(font);
            hashCode.Add(fontSize);
            hashCode.Add(color);
            hashCode.Add(align);
            hashCode.Add(style);
            hashCode.Add(wrap);
            hashCode.Add(outlineColor);
            hashCode.Add(outlineWidth);
            hashCode.Add(letterSpacing);
            hashCode.Add(wordSpacing);
            hashCode.Add(paragraphSpacing);
            hashCode.Add(overflow);
            hashCode.Add(overflowPosition);
            hashCode.Add(shadow);
            hashCode.Add(autoSize);
            hashCode.Add(generator);
            return hashCode.ToHashCode();
        }

        public static readonly TextStyle Default = new();
        public static readonly TextStyle AlignCenter = new() { align = TextAnchor.MiddleCenter };
        public static readonly TextStyle AlignLeft = new() { align = TextAnchor.MiddleLeft };
        public static readonly TextStyle AlignRight = new() { align = TextAnchor.MiddleRight };
    }
}