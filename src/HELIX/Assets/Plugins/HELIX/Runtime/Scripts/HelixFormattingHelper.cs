using System;
using System.Globalization;
using System.Text;
using UnityEngine.UIElements;

namespace HELIX {
    public static class HelixFormattingHelper {
        public static string BuildQuadruple<T>(
            string name,
            T l, T t, T r, T b,
            string nameLeft = "left",
            string nameTop = "top",
            string nameRight = "right",
            string nameBottom = "bottom",
            bool summarize = true,
            bool showNames = true
        ) where T: IEquatable<T> {
            var builder = new StringBuilder();
            builder.Append(name);
            builder.Append("(");
            if (summarize && l.Equals(r) &&
                t.Equals(b) &&
                l.Equals(t)) { builder.Append(l); } else {
                if (showNames) builder.Append(nameLeft + ": ");
                builder.Append(l);
                builder.Append(", ");
                if (showNames) builder.Append(nameTop + ": ");
                builder.Append(t);
                builder.Append(", ");
                if (showNames) builder.Append(nameRight + ": ");
                builder.Append(r);
                builder.Append(", ");
                if (showNames) builder.Append(nameBottom + ": ");
                builder.Append(b);
            }
            
            builder.Append(")");
            return builder.ToString();
        }

        public static string FormatStyleValue<T>(this IStyleValue<T> value) {
            return FormatStyleValue(value.keyword, value.value);
        }
        public static string FormatStyleValue(this StyleLength length) {
            return FormatStyleValue(length.keyword, length.value);
        }

        public static string FormatStyleValue<T>(StyleKeyword keyword, T value) {
            return keyword switch {
                StyleKeyword.Initial => "<initial>",
                StyleKeyword.Auto    => "<auto>",
                StyleKeyword.None    => "<none>",
                StyleKeyword.Null    => "<null>",
                _                    => value.ToString()
            };
        }

        public static string FormatLength(Length length) {
            return length.unit == LengthUnit.Percent ? (length.value / 100f).ToString("P", CultureInfo.InvariantCulture) : length.value + "px";
        }
    }
}