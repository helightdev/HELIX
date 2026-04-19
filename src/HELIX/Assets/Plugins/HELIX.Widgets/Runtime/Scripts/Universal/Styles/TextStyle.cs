using System;
using System.Diagnostics.CodeAnalysis;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Styles {
    public class TextStyle : DiagnosticableBase, IEquatable<TextStyle> {
        public StyleFont font = StyleKeyword.Null;
        public StyleLength fontSize = StyleKeyword.Null;
        public StyleColor color = StyleKeyword.Null;
        public StyleEnum<TextAnchor> align = StyleKeyword.Null;
        public StyleEnum<FontStyle> style = StyleKeyword.Null;
        public StyleEnum<WhiteSpace> wrap = StyleKeyword.Null;
        public StyleColor outlineColor = StyleKeyword.Null;
        public StyleFloat outlineWidth = StyleKeyword.Null;
        public StyleLength letterSpacing = StyleKeyword.Null;
        public StyleLength wordSpacing = StyleKeyword.Null;
        public StyleLength paragraphSpacing = StyleKeyword.Null;
        public StyleEnum<TextOverflow> overflow = StyleKeyword.Null;
        public StyleEnum<TextOverflowPosition> overflowPosition = StyleKeyword.Null;
        public StyleTextShadow shadow = StyleKeyword.Null;
        public StyleTextAutoSize autoSize = StyleKeyword.Null;
        public StyleEnum<TextGeneratorType> generator = StyleKeyword.Null;

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
            if (overrides.style.keyword != StyleKeyword.Null) style = overrides.style;
            if (overrides.font.keyword != StyleKeyword.Null) font = overrides.font;
            if (overrides.fontSize.keyword != StyleKeyword.Null) fontSize = overrides.fontSize;
            if (overrides.color.keyword != StyleKeyword.Null) color = overrides.color;
            if (overrides.align.keyword != StyleKeyword.Null) align = overrides.align;
            if (overrides.wrap.keyword != StyleKeyword.Null) wrap = overrides.wrap;
            if (overrides.outlineColor.keyword != StyleKeyword.Null) outlineColor = overrides.outlineColor;
            if (overrides.outlineWidth.keyword != StyleKeyword.Null) outlineWidth = overrides.outlineWidth;
            if (overrides.letterSpacing.keyword != StyleKeyword.Null) letterSpacing = overrides.letterSpacing;
            if (overrides.wordSpacing.keyword != StyleKeyword.Null) wordSpacing = overrides.wordSpacing;
            if (overrides.paragraphSpacing.keyword != StyleKeyword.Null)
                paragraphSpacing = overrides.paragraphSpacing;
            if (overrides.overflow.keyword != StyleKeyword.Null) overflow = overrides.overflow;
            if (overrides.overflowPosition.keyword != StyleKeyword.Null)
                overflowPosition = overrides.overflowPosition;
            if (overrides.shadow.keyword != StyleKeyword.Null) shadow = overrides.shadow;
            if (overrides.autoSize.keyword != StyleKeyword.Null) autoSize = overrides.autoSize;
            if (overrides.generator.keyword != StyleKeyword.Null) generator = overrides.generator;
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

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new StyleValueProperty<Font>("font", font));
            properties.Add(new StyleValueProperty<Length>("fontSize", fontSize));
            properties.Add(new StyleValueProperty<Color>("color", color));
            properties.Add(new StyleValueProperty<TextAnchor>("align", align));
            properties.Add(new StyleValueProperty<FontStyle>("style", style));
            properties.Add(new StyleValueProperty<WhiteSpace>("wrap", wrap));
            properties.Add(new StyleValueProperty<Color>("outlineColor", outlineColor));
            properties.Add(new StyleValueProperty<float>("outlineWidth", outlineWidth));
            properties.Add(new StyleValueProperty<Length>("letterSpacing", letterSpacing));
            properties.Add(new StyleValueProperty<Length>("wordSpacing", wordSpacing));
            properties.Add(new StyleValueProperty<Length>("paragraphSpacing", paragraphSpacing));
            properties.Add(new StyleValueProperty<TextOverflow>("overflow", overflow));
            properties.Add(new StyleValueProperty<TextOverflowPosition>("overflowPosition", overflowPosition));
            properties.Add(new StyleValueProperty<TextShadow>("shadow", shadow));
            properties.Add(new StyleValueProperty<TextAutoSize>("autoSize", autoSize));
            properties.Add(new StyleValueProperty<TextGeneratorType>("generator", generator));
        }
    }
}