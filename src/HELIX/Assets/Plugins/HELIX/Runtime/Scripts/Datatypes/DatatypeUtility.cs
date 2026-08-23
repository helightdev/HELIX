using System;
using System.Globalization;
using HELIX.Prose;

namespace HELIX {
  internal static class DatatypeUtility {
    internal static T ConvertFrom<T>(object value) {
      if (!typeof(T).IsEnum)
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
      if (value is string text) return (T)Enum.Parse(typeof(T), text);
      var underlying = Enum.GetUnderlyingType(typeof(T));
      return (T)Enum.ToObject(
        typeof(T), Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture)
      );
    }

    internal static void WriteDecorated(
      IProseWriter writer, string value, string prefix, string suffix, string unit = null
    ) {
      if (prefix != null) writer.Write(prefix);
      writer.Write(value);
      if (suffix != null) writer.Write(suffix);
      if (unit != null) writer.Write(unit);
    }

    internal static string FormatCompact(float value) => float.IsNaN(value) || float.IsInfinity(value)
      ? value.ToString(CultureInfo.InvariantCulture)
      : value.ToString("0.0###############", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');

    internal static string FormatCompact(double value) => double.IsNaN(value) || double.IsInfinity(value)
      ? value.ToString(CultureInfo.InvariantCulture)
      : value.ToString("0.0###############", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
  }
}