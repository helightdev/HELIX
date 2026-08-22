using System;
using System.Collections.Generic;
using System.Globalization;
using HELIX.Coloring;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Prose {
  public interface IProseRangeFormatter<T> where T : struct {
    T? Min { get; }
    T? Max { get; }
    T? Step { get; }
  }

  public interface IProseUnitFormatter { string Unit { get; } }
  public interface IProsePatternFormatter { string Pattern { get; } }
  public interface IProseReadOnlyFormatter { bool ReadOnly { get; } }

  public interface IProseChoiceFormatter {
    int ChoiceCount { get; }
    object GetChoiceValue(int index);
    string GetChoiceLabel(int index);
    bool IsChoiceEnabled(int index);
  }

  public readonly struct ProseChoice<T> {
    public ProseChoice(T value, string label, bool enabled = true) {
      Value = value;
      Label = label;
      Enabled = enabled;
    }
    public T Value { get; }
    public string Label { get; }
    public bool Enabled { get; }
  }

  public interface IProseDefaultFormatter<out T> {
    bool HasDefaultValue { get; }
    T DefaultValue { get; }
  }

  /// <summary>Shared instances of the default, immutable Prose value formatters.</summary>
  public static class ProseFormatters {
    public static readonly ProseStringFormatter String = new();
    public static readonly ProseIntFormatter Int = new();
    public static readonly ProseLongFormatter Long = new();
    public static readonly ProseFloatFormatter Float = new();
    public static readonly ProseDoubleFormatter Double = new();
    public static readonly ProseBoolFormatter Bool = new();
    public static readonly ProseColorFormatter Color = new();
    public static readonly ProseFloatFormatter Percent = new(format: "0.0", suffix: "%");
    public static readonly ProseFloatFormatter PercentNormalized = new(format: "0.0", suffix: "%", scale: 100f);

    public static ProseEnumFormatter<T> Enum<T>() where T : struct, Enum => EnumCache<T>.Instance;
    public static ProseObjectFormatter<T> Object<T>() => ObjectCache<T>.Instance;

    private static class EnumCache<T> where T : struct, Enum {
      internal static readonly ProseEnumFormatter<T> Instance = new();
    }

    private static class ObjectCache<T> {
      internal static readonly ProseObjectFormatter<T> Instance = new();
    }
  }

  /// <summary>A semantic property descriptor that can expand itself into property scopes as a fallback.</summary>
  public interface IProsePropertyFormatter<in T> : IProseFormatter<T> {
    string Key { get; }
  }

  public sealed class ProsePropertyFormatter<T> : IProsePropertyFormatter<T> {
    public ProsePropertyFormatter(
      string key,
      IProseFormatter<T> valueFormatter,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false,
      bool hideName = false,
      bool hideSeparator = false,
      string description = null,
      object defaultValue = null
    ) {
      Key = key ?? throw new ArgumentNullException(nameof(key));
      ValueFormatter = valueFormatter ?? throw new ArgumentNullException(nameof(valueFormatter));
      Level = level;
      Hidden = hidden;
      NoWrap = noWrap;
      HideName = hideName;
      HideSeparator = hideSeparator;
      Description = description;
      DefaultValue = defaultValue;
    }

    public string Key { get; }
    public IProseFormatter<T> ValueFormatter { get; }
    public ProseLevel Level { get; }
    public bool Hidden { get; }
    public bool NoWrap { get; }
    public bool HideName { get; }
    public bool HideSeparator { get; }
    public string Description { get; }
    public object DefaultValue { get; }

    public void ToProse(IProseWriter writer, T value) => writer.Property(
      Key, value, ValueFormatter, Level, Hidden, NoWrap, HideName, HideSeparator,
      Description, DefaultValue
    );
  }

  public sealed class ProseStringFormatter :
    IProseFormatter<string>, IProsePatternFormatter, IProseReadOnlyFormatter {
    public ProseStringFormatter(
      string nullText = ProseLiterals.Null,
      string prefix = null,
      string suffix = null,
      string emptyText = null,
      bool quoted = false,
      string quote = "\"",
      string pattern = null,
      bool readOnly = false
    ) {
      NullText = nullText;
      Prefix = prefix;
      Suffix = suffix;
      EmptyText = emptyText;
      Quoted = quoted;
      Quote = quote ?? string.Empty;
      Pattern = pattern;
      ReadOnly = readOnly;
    }

    public string NullText { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public string EmptyText { get; }
    public bool Quoted { get; }
    public string Quote { get; }
    public string Pattern { get; }
    public bool ReadOnly { get; }

    public void ToProse(IProseWriter writer, string value) {
      if (Prefix != null) writer.Write(Prefix);
      if (value == null) writer.Write(NullText);
      else if (value.Length == 0 && EmptyText != null) writer.Write(EmptyText);
      else {
        if (Quoted) writer.Write(Quote);
        writer.Write(value);
        if (Quoted) writer.Write(Quote);
      }
      if (Suffix != null) writer.Write(Suffix);
    }
  }

  public sealed class ProseIntFormatter :
    IProseFormatter<int>, IProseFormatter<int?>, IProseRangeFormatter<int>, IProseUnitFormatter {
    public ProseIntFormatter(
      string format = null,
      int? min = null,
      int? max = null,
      string prefix = null,
      string suffix = null,
      string nullText = ProseLiterals.Null,
      string unit = null,
      int? step = null
    ) {
      Format = format;
      Min = min;
      Max = max;
      Prefix = prefix;
      Suffix = suffix;
      NullText = nullText;
      Unit = unit;
      Step = step;
    }

    public string Format { get; }
    public int? Min { get; }
    public int? Max { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public string NullText { get; }
    public string Unit { get; }
    public int? Step { get; }

    public void ToProse(IProseWriter writer, int value) {
      WriteNumber(writer, value.ToString(Format, CultureInfo.InvariantCulture));
    }

    public void ToProse(IProseWriter writer, int? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }

    private void WriteNumber(IProseWriter writer, string number) =>
      ProseFormatterUtility.WriteDecorated(writer, number, Prefix, Suffix, Unit);
  }

  public sealed class ProseLongFormatter :
    IProseFormatter<long>, IProseFormatter<long?>, IProseRangeFormatter<long>, IProseUnitFormatter {
    public ProseLongFormatter(
      string format = null, long? min = null, long? max = null,
      string prefix = null, string suffix = null,
      string nullText = ProseLiterals.Null, string unit = null, long? step = null
    ) {
      Format = format;
      Min = min;
      Max = max;
      Prefix = prefix;
      Suffix = suffix;
      NullText = nullText;
      Unit = unit;
      Step = step;
    }

    public string Format { get; }
    public long? Min { get; }
    public long? Max { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public string NullText { get; }
    public string Unit { get; }
    public long? Step { get; }

    public void ToProse(IProseWriter writer, long value) => ProseFormatterUtility.WriteDecorated(
      writer, value.ToString(Format, CultureInfo.InvariantCulture), Prefix, Suffix, Unit
    );

    public void ToProse(IProseWriter writer, long? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class ProseFloatFormatter :
    IProseFormatter<float>, IProseFormatter<float?>, IProseRangeFormatter<float>, IProseUnitFormatter {
    public ProseFloatFormatter(
      string format = "R", float? min = null, float? max = null,
      string prefix = null, string suffix = null,
      string nullText = ProseLiterals.Null, string unit = null,
      bool compact = false, float scale = 1f, bool clamp = false, float? step = null
    ) {
      Format = format;
      Min = min;
      Max = max;
      Prefix = prefix;
      Suffix = suffix;
      NullText = nullText;
      Unit = unit;
      Compact = compact;
      Scale = scale;
      Clamp = clamp;
      Step = step;
    }

    public string Format { get; }
    public float? Min { get; }
    public float? Max { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public string NullText { get; }
    public string Unit { get; }
    public bool Compact { get; }
    public float Scale { get; }
    public bool Clamp { get; }
    public float? Step { get; }

    public void ToProse(IProseWriter writer, float value) {
      if (Clamp) value = Math.Max(Min ?? float.MinValue, Math.Min(Max ?? float.MaxValue, value));
      value *= Scale;
      ProseFormatterUtility.WriteDecorated(
        writer,
        Compact
          ? ProseFormatterUtility.FormatCompact(value)
          : value.ToString(Format, CultureInfo.InvariantCulture),
        Prefix, Suffix, Unit
      );
    }

    public void ToProse(IProseWriter writer, float? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class ProseDoubleFormatter :
    IProseFormatter<double>, IProseFormatter<double?>, IProseRangeFormatter<double>, IProseUnitFormatter {
    public ProseDoubleFormatter(
      string format = "R", double? min = null, double? max = null,
      string prefix = null, string suffix = null,
      string nullText = ProseLiterals.Null, string unit = null,
      bool compact = false, double? step = null
    ) {
      Format = format;
      Min = min;
      Max = max;
      Prefix = prefix;
      Suffix = suffix;
      NullText = nullText;
      Unit = unit;
      Compact = compact;
      Step = step;
    }

    public string Format { get; }
    public double? Min { get; }
    public double? Max { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public string NullText { get; }
    public string Unit { get; }
    public bool Compact { get; }
    public double? Step { get; }

    public void ToProse(IProseWriter writer, double value) => ProseFormatterUtility.WriteDecorated(
      writer,
      Compact ? ProseFormatterUtility.FormatCompact(value) : value.ToString(Format, CultureInfo.InvariantCulture),
      Prefix, Suffix, Unit
    );

    public void ToProse(IProseWriter writer, double? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class ProseBoolFormatter : IProseFormatter<bool>, IProseFormatter<bool?> {
    public ProseBoolFormatter(
      string trueText = "true", string falseText = "false",
      string nullText = ProseLiterals.Null
    ) {
      TrueText = trueText;
      FalseText = falseText;
      NullText = nullText;
    }

    public string TrueText { get; }
    public string FalseText { get; }
    public string NullText { get; }
    public void ToProse(IProseWriter writer, bool value) => writer.Write(value ? TrueText : FalseText);

    public void ToProse(IProseWriter writer, bool? value) =>
      writer.Write(value.HasValue ? value.Value ? TrueText : FalseText : NullText);
  }

  /// <summary>Maps nullable flag states to configured text.</summary>
  public sealed class ProseFlagFormatter : IProseFormatter<bool>, IProseFormatter<bool?> {
    public ProseFlagFormatter(
      string ifTrue = null, string ifFalse = null, string ifNull = ProseLiterals.Null
    ) {
      if (ifTrue == null && ifFalse == null)
        throw new ArgumentException("At least one flag value must be configured.");
      IfTrue = ifTrue;
      IfFalse = ifFalse;
      IfNull = ifNull;
    }

    public string IfTrue { get; }
    public string IfFalse { get; }
    public string IfNull { get; }
    public void ToProse(IProseWriter writer, bool value) => writer.Write(value ? IfTrue : IfFalse);

    public void ToProse(IProseWriter writer, bool? value) =>
      writer.Write(value.HasValue ? value.Value ? IfTrue : IfFalse : IfNull);
  }

  public sealed class ProseEnumFormatter<T> :
    IProseFormatter<T>, IProseFormatter<T?>, IProseChoiceFormatter where T : struct, Enum {
    private static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
    public ProseEnumFormatter(
      string nullText = ProseLiterals.Null, string prefix = null, string suffix = null
    ) {
      NullText = nullText;
      Prefix = prefix;
      Suffix = suffix;
    }

    public string NullText { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public int ChoiceCount => Values.Length;
    public object GetChoiceValue(int index) => Values[index];
    public string GetChoiceLabel(int index) => Values[index].ToString();
    public bool IsChoiceEnabled(int index) => true;

    public void ToProse(IProseWriter writer, T value) =>
      ProseFormatterUtility.WriteDecorated(writer, value.ToString(), Prefix, Suffix);

    public void ToProse(IProseWriter writer, T? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  /// <summary>Describes a finite set of choices for values that are not CLR enums.</summary>
  public sealed class ProseChoiceFormatter<T> : IProseFormatter<T>, IProseChoiceFormatter {
    private readonly IReadOnlyList<ProseChoice<T>> _choices;

    public ProseChoiceFormatter(IReadOnlyList<ProseChoice<T>> choices) =>
      _choices = choices ?? throw new ArgumentNullException(nameof(choices));

    public IReadOnlyList<ProseChoice<T>> Choices => _choices;
    public int ChoiceCount => _choices.Count;
    public object GetChoiceValue(int index) => _choices[index].Value;
    public string GetChoiceLabel(int index) => _choices[index].Label;
    public bool IsChoiceEnabled(int index) => _choices[index].Enabled;

    public void ToProse(IProseWriter writer, T value) {
      for (var i = 0; i < _choices.Count; i++) {
        if (!EqualityComparer<T>.Default.Equals(_choices[i].Value, value)) continue;
        writer.Write(_choices[i].Label);
        return;
      }
      writer.Write(value is null ? ProseLiterals.Null : value.ToString());
    }
  }

  public sealed class ProseObjectFormatter<T> : IProseFormatter<T> {
    public ProseObjectFormatter(
      string nullText = ProseLiterals.Null, string prefix = null, string suffix = null
    ) {
      NullText = nullText;
      Prefix = prefix;
      Suffix = suffix;
    }

    public string NullText { get; }
    public string Prefix { get; }
    public string Suffix { get; }

    public void ToProse(IProseWriter writer, T value) {
      ProseFormatterUtility.WriteDecorated(
        writer, value == null ? NullText : value.ToString(), Prefix, Suffix
      );
    }
  }

  /// <summary>Maps object presence to configured text.</summary>
  public sealed class ProseObjectFlagFormatter<T> : IProseFormatter<T> {
    public ProseObjectFlagFormatter(string ifPresent = null, string ifNull = null) {
      if (ifPresent == null && ifNull == null)
        throw new ArgumentException("At least one object state must be configured.");
      IfPresent = ifPresent;
      IfNull = ifNull;
    }

    public string IfPresent { get; }
    public string IfNull { get; }
    public void ToProse(IProseWriter writer, T value) => writer.Write(value == null ? IfNull : IfPresent);
  }

  /// <summary>Formats Unity colors in the same compact form as diagnostics color properties.</summary>
  public sealed class ProseColorFormatter : IProseFormatter<Color> {
    public ProseColorFormatter(
      string transparentText = "transparent", string prefix = null, string suffix = null
    ) {
      TransparentText = transparentText;
      Prefix = prefix;
      Suffix = suffix;
    }

    public string TransparentText { get; }
    public string Prefix { get; }
    public string Suffix { get; }

    public void ToProse(IProseWriter writer, Color value) => ProseFormatterUtility.WriteDecorated(
      writer, value.a == 0 ? TransparentText : value.ToHex(), Prefix, Suffix
    );
  }

  /// <summary>Formats UI Toolkit style keywords or delegates formatting of their resolved value.</summary>
  public sealed class ProseStyleValueFormatter<T> : IProseFormatter<IStyleValue<T>> {
    public ProseStyleValueFormatter(
      IProseFormatter<T> valueFormatter = null,
      string nullText = ProseLiterals.Null,
      string autoText = "<auto>",
      string noneText = "<none>",
      string initialText = "<initial>"
    ) {
      ValueFormatter = valueFormatter ?? ProseFormatters.Object<T>();
      NullText = nullText;
      AutoText = autoText;
      NoneText = noneText;
      InitialText = initialText;
    }

    public IProseFormatter<T> ValueFormatter { get; }
    public string NullText { get; }
    public string AutoText { get; }
    public string NoneText { get; }
    public string InitialText { get; }

    public void ToProse(IProseWriter writer, IStyleValue<T> value) {
      if (value == null) {
        writer.Write(NullText);
        return;
      }
      switch (value.keyword) {
        case StyleKeyword.Null: writer.Write(NullText); break;
        case StyleKeyword.Auto: writer.Write(AutoText); break;
        case StyleKeyword.None: writer.Write(NoneText); break;
        case StyleKeyword.Initial: writer.Write(InitialText); break;
        default: writer.Write(value.value, ValueFormatter); break;
      }
    }
  }

  /// <summary>Adapts a reusable delegate to the semantic formatter contract.</summary>
  public sealed class ProseFormattingFormatter<T> : IProseFormatter<T> {
    private readonly Func<T, string> _formatter;

    public ProseFormattingFormatter(
      Func<T, string> formatter, string nullText = ProseLiterals.Null,
      string prefix = null, string suffix = null
    ) {
      _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
      NullText = nullText;
      Prefix = prefix;
      Suffix = suffix;
    }

    public string NullText { get; }
    public string Prefix { get; }
    public string Suffix { get; }

    public void ToProse(IProseWriter writer, T value) => ProseFormatterUtility.WriteDecorated(
      writer, value == null ? NullText : _formatter(value), Prefix, Suffix
    );
  }

  /// <summary>Streams iterable items directly to a writer without materializing an intermediate list.</summary>
  public sealed class ProseIterableFormatter<T> : IProseFormatter<IEnumerable<T>> {
    public ProseIterableFormatter(
      IProseFormatter<T> itemFormatter = null,
      string nullText = ProseLiterals.Null,
      string emptyText = "[]",
      string prefix = "[",
      string separator = ", ",
      string suffix = "]"
    ) {
      ItemFormatter = itemFormatter ?? ProseFormatters.Object<T>();
      NullText = nullText;
      EmptyText = emptyText;
      Prefix = prefix;
      Separator = separator;
      Suffix = suffix;
    }

    public IProseFormatter<T> ItemFormatter { get; }
    public string NullText { get; }
    public string EmptyText { get; }
    public string Prefix { get; }
    public string Separator { get; }
    public string Suffix { get; }

    public void ToProse(IProseWriter writer, IEnumerable<T> values) {
      if (values == null) {
        writer.Write(NullText);
        return;
      }

      using var enumerator = values.GetEnumerator();
      if (!enumerator.MoveNext()) {
        if (EmptyText != null) writer.Write(EmptyText);
        else {
          if (Prefix != null) writer.Write(Prefix);
          if (Suffix != null) writer.Write(Suffix);
        }
        return;
      }

      if (Prefix != null) writer.Write(Prefix);
      writer.Write(enumerator.Current, ItemFormatter);
      while (enumerator.MoveNext()) {
        if (Separator != null) writer.Write(Separator);
        writer.Write(enumerator.Current, ItemFormatter);
      }
      if (Suffix != null) writer.Write(Suffix);
    }
  }

  internal static class ProseFormatterUtility {
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
