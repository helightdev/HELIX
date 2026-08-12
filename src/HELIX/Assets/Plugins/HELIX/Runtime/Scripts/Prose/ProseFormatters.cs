using System;
using System.Globalization;

namespace HELIX.Prose {
  /// <summary>A semantic property descriptor that can expand itself into property scopes as a fallback.</summary>
  public interface IProsePropertyFormatter<in T> : IProseFormatter<T> {
    string Key { get; }
  }

  public sealed class ProsePropertyFormatter<T> : IProsePropertyFormatter<T> {
    public ProsePropertyFormatter(string key, IProseFormatter<T> valueFormatter) {
      Key = key ?? throw new ArgumentNullException(nameof(key));
      ValueFormatter = valueFormatter ?? throw new ArgumentNullException(nameof(valueFormatter));
    }

    public string Key { get; }
    public IProseFormatter<T> ValueFormatter { get; }

    public void ToProse(IProseWriter writer, T value) {
      if (!writer.BeginFrame(ProseProperty.Instance)) return;
      try {
        if (writer.BeginFrame(ProsePropertyKey.Instance)) {
          try { writer.Write(Key); } finally { writer.PopFrame(); }
        }

        if (writer.BeginFrame(ProsePropertyValue.Instance)) {
          try { writer.Write(value, ValueFormatter); } finally { writer.PopFrame(); }
        }
      } finally {
        writer.PopFrame();
      }
    }
  }

  public sealed class ProseStringFormatter : IProseFormatter<string> {
    public static readonly ProseStringFormatter Instance = new();
    public ProseStringFormatter(string nullText = ProseLiterals.Null, string prefix = null, string suffix = null) {
      NullText = nullText;
      Prefix = prefix;
      Suffix = suffix;
    }
    public string NullText { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public void ToProse(IProseWriter writer, string value) {
      if (Prefix != null) writer.Write(Prefix);
      writer.Write(value ?? NullText);
      if (Suffix != null) writer.Write(Suffix);
    }
  }

  public sealed class ProseIntFormatter : IProseFormatter<int> {
    public static readonly ProseIntFormatter Instance = new();
    public ProseIntFormatter(
      string format = null,
      int? min = null,
      int? max = null,
      string prefix = null,
      string suffix = null
    ) {
      Format = format;
      Min = min;
      Max = max;
      Prefix = prefix;
      Suffix = suffix;
    }
    public string Format { get; }
    public int? Min { get; }
    public int? Max { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public void ToProse(IProseWriter writer, int value) {
      if (Prefix != null) writer.Write(Prefix);
      writer.Write(value.ToString(Format, CultureInfo.InvariantCulture));
      if (Suffix != null) writer.Write(Suffix);
    }
  }

  public sealed class ProseLongFormatter : IProseFormatter<long> {
    public static readonly ProseLongFormatter Instance = new();
    public ProseLongFormatter(string format = null) => Format = format;
    public string Format { get; }
    public void ToProse(IProseWriter writer, long value) =>
      writer.Write(value.ToString(Format, CultureInfo.InvariantCulture));
  }

  public sealed class ProseFloatFormatter : IProseFormatter<float> {
    public static readonly ProseFloatFormatter Instance = new();
    public ProseFloatFormatter(string format = "R") => Format = format;
    public string Format { get; }
    public void ToProse(IProseWriter writer, float value) =>
      writer.Write(value.ToString(Format, CultureInfo.InvariantCulture));
  }

  public sealed class ProseDoubleFormatter : IProseFormatter<double> {
    public static readonly ProseDoubleFormatter Instance = new();
    public ProseDoubleFormatter(string format = "R") => Format = format;
    public string Format { get; }
    public void ToProse(IProseWriter writer, double value) =>
      writer.Write(value.ToString(Format, CultureInfo.InvariantCulture));
  }

  public sealed class ProseBoolFormatter : IProseFormatter<bool> {
    public static readonly ProseBoolFormatter Instance = new();
    public ProseBoolFormatter(string trueText = "true", string falseText = "false") {
      TrueText = trueText;
      FalseText = falseText;
    }
    public string TrueText { get; }
    public string FalseText { get; }
    public void ToProse(IProseWriter writer, bool value) => writer.Write(value ? TrueText : FalseText);
  }

  public sealed class ProseEnumFormatter<T> : IProseFormatter<T> where T : struct, Enum {
    public static readonly ProseEnumFormatter<T> Instance = new();
    public ProseEnumFormatter() { }
    public void ToProse(IProseWriter writer, T value) => writer.Write(value.ToString());
  }

  public sealed class ProseObjectFormatter<T> : IProseFormatter<T> {
    public static readonly ProseObjectFormatter<T> Instance = new();
    public ProseObjectFormatter() { }

    public void ToProse(IProseWriter writer, T value) {
      writer.Write(value == null ? ProseLiterals.Null : value.ToString());
    }
  }
}
