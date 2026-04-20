using System;
using System.Diagnostics.CodeAnalysis;
using HELIX.Coloring.Material;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Styles {
    public class BackgroundStyle : DiagnosticableBase, IEquatable<BackgroundStyle> {
        public static readonly BackgroundStyle Default = new();
        public StyleColor color = StyleKeyword.Initial;
        public StyleBackgroundSize fit = StyleKeyword.Initial;
        public StyleBackground image = StyleKeyword.Initial;
        public StyleColor imageTintColor = StyleKeyword.Initial;
        public StyleBackgroundRepeat repeat = StyleKeyword.Initial;
        public StyleInt4 slice = StyleKeyword.Initial;
        public StyleFloat sliceScale = StyleKeyword.Initial;
        public StyleEnum<SliceType> sliceType = StyleKeyword.Initial;
        public StyleBackgroundPosition x = StyleKeyword.Initial;
        public StyleBackgroundPosition y = StyleKeyword.Initial;

        public bool Equals(BackgroundStyle other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return color.Equals(other.color) && image.Equals(other.image) && fit.Equals(other.fit) &&
                   repeat.Equals(other.repeat) && imageTintColor.Equals(other.imageTintColor) && x.Equals(other.x) &&
                   y.Equals(other.y) && slice.Equals(other.slice) && sliceScale.Equals(other.sliceScale) &&
                   sliceType.Equals(other.sliceType);
        }

        public static implicit operator BackgroundStyle(Color color) {
            return new BackgroundStyle { color = color };
        }

        public static implicit operator BackgroundStyle(StyleColor color) {
            return new BackgroundStyle { color = color };
        }

        public static implicit operator BackgroundStyle(MaterialColor color) {
            return new BackgroundStyle { color = color };
        }

        public void Apply(VisualElement element) {
            element.style.backgroundColor = color;
            element.style.backgroundImage = image;
            element.style.backgroundSize = fit;
            element.style.backgroundRepeat = repeat;
            element.style.unityBackgroundImageTintColor = imageTintColor;
            element.style.backgroundPositionX = x;
            element.style.backgroundPositionY = y;
            element.style.unitySliceLeft = slice.L;
            element.style.unitySliceTop = slice.T;
            element.style.unitySliceRight = slice.R;
            element.style.unitySliceBottom = slice.B;
            element.style.unitySliceScale = sliceScale;
            element.style.unitySliceType = sliceType;
        }

        public void Merge(BackgroundStyle overrides) {
            if (overrides.color.keyword != StyleKeyword.Initial) color = overrides.color;
            if (overrides.image.keyword != StyleKeyword.Initial) image = overrides.image;
            if (overrides.fit.keyword != StyleKeyword.Initial) fit = overrides.fit;
            if (overrides.repeat.keyword != StyleKeyword.Initial) repeat = overrides.repeat;
            if (overrides.imageTintColor.keyword != StyleKeyword.Initial) imageTintColor = overrides.imageTintColor;
            if (overrides.x.keyword != StyleKeyword.Initial) x = overrides.x;
            if (overrides.y.keyword != StyleKeyword.Initial) y = overrides.y;
            if (overrides.slice.keyword != StyleKeyword.Initial) slice = overrides.slice;
            if (overrides.sliceScale.keyword != StyleKeyword.Initial) sliceScale = overrides.sliceScale;
            if (overrides.sliceType.keyword != StyleKeyword.Initial) sliceType = overrides.sliceType;
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BackgroundStyle)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            var hashCode = new HashCode();
            hashCode.Add(color);
            hashCode.Add(image);
            hashCode.Add(fit);
            hashCode.Add(repeat);
            hashCode.Add(imageTintColor);
            hashCode.Add(x);
            hashCode.Add(y);
            hashCode.Add(slice);
            hashCode.Add(sliceScale);
            hashCode.Add(sliceType);
            return hashCode.ToHashCode();
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new StyleValueProperty<Color>("color", color));
            properties.Add(new StyleValueProperty<Background>("image", image));
            properties.Add(new StyleValueProperty<BackgroundSize>("fit", fit));
            properties.Add(new StyleValueProperty<BackgroundRepeat>("repeat", repeat));
            properties.Add(new StyleValueProperty<Color>("imageTintColor", imageTintColor));
            properties.Add(new StyleValueProperty<BackgroundPosition>("x", x));
            properties.Add(new StyleValueProperty<BackgroundPosition>("y", y));
            properties.Add(new StyleValueProperty<int4>("slice", slice));
            properties.Add(new StyleValueProperty<float>("sliceScale", sliceScale));
            properties.Add(new StyleValueProperty<SliceType>("sliceType", sliceType));
        }
    }
}