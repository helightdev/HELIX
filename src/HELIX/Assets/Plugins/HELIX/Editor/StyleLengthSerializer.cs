using System;
using System.Globalization;
using HELIX.Types;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HELIX.Editor {
    public static class StyleLengthSerializer {
        public static string ToString(StyleLength styleLength) {
            switch (styleLength.keyword) {
                case StyleKeyword.Undefined:
                    if (styleLength.value.value == 0) return "0";
                    var valueString = styleLength.value.value.ToString(CultureInfo.InvariantCulture);
                    return styleLength.value.unit switch {
                        LengthUnit.Pixel => $"{valueString}px",
                        LengthUnit.Percent => $"{valueString}%",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                case StyleKeyword.Null:
                    return "null";
                case StyleKeyword.Auto:
                    return "auto";
                case StyleKeyword.None:
                    return "none";
                case StyleKeyword.Initial:
                    return "initial";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static StyleLength FromString(string str) {
            str = str.Trim().ToLowerInvariant();
            switch (str) {
                case "null":
                    return new StyleLength(StyleKeyword.Null);
                case "auto":
                    return new StyleLength(StyleKeyword.Auto);
                case "none":
                    return new StyleLength(StyleKeyword.None);
                case "initial":
                    return new StyleLength(StyleKeyword.Initial);
            }

            if (str.Equals("0")) {
                return new StyleLength(new Length(0, LengthUnit.Pixel));
            } else if (str.EndsWith("%")) {
                var numberPart = str[..^1];
                return float.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var percentValue)
                    ? new StyleLength(new Length(percentValue, LengthUnit.Percent))
                    : throw new FormatException($"Invalid percentage value: {str}");
            } else if (str.EndsWith("px")) {
                var numberPart = str[..^2];
                return float.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var pixelValue)
                    ? new StyleLength(new Length(pixelValue, LengthUnit.Pixel))
                    : throw new FormatException($"Invalid pixel value: {str}");
            } 
            
            throw new FormatException($"Invalid StyleLength format: {str}");
        }
    }

    public class StyleLength2Converter : UxmlAttributeConverter<StyleLength2> {
        public override StyleLength2 FromString(string value) {
            var parts = value.Split(',');
            if (parts.Length != 2) {
                throw new FormatException("Invalid StyleLength2 format. Expected format: 'width,height'");
            }

            var width = StyleLengthSerializer.FromString(parts[0]);
            var height = StyleLengthSerializer.FromString(parts[1]);
            return new StyleLength2(width, height);
        }

        public override string ToString(StyleLength2 value) {
            return $"{StyleLengthSerializer.ToString(value.w)},{StyleLengthSerializer.ToString(value.h)}";
        }
    }

    public class StyleLength4Converter : UxmlAttributeConverter<StyleLength4> {
        public override StyleLength4 FromString(string value) {
            var parts = value.Split(',');
            if (parts.Length != 4) {
                throw new FormatException("Invalid StyleLength4 format. Expected format: 'top,right,bottom,left'");
            }

            var top = StyleLengthSerializer.FromString(parts[0]);
            var right = StyleLengthSerializer.FromString(parts[1]);
            var bottom = StyleLengthSerializer.FromString(parts[2]);
            var left = StyleLengthSerializer.FromString(parts[3]);
            return new StyleLength4(top, right, bottom, left);
        }

        public override string ToString(StyleLength4 value) {
            return $"{StyleLengthSerializer.ToString(value.t)},{StyleLengthSerializer.ToString(value.r)}," +
                   $"{StyleLengthSerializer.ToString(value.b)},{StyleLengthSerializer.ToString(value.l)}";
        }
    }
}