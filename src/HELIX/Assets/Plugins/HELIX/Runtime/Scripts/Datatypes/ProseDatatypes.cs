using System;
using System.Collections.Generic;
using System.Globalization;
using HELIX.Coloring;
using HELIX.Prose;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global

namespace HELIX.Datatypes {
  public interface IRange<T> where T : struct {
    T? Min { get; }
    T? Max { get; }
    T? Step { get; }
  }

  public interface IUnit {
    string Unit { get; }
  }

  public interface IAffix {
    string Prefix { get; }
    string Suffix { get; }
  }

  public interface IPattern {
    string Pattern { get; }
  }

  public interface IReadOnly {
    bool ReadOnly { get; }
  }

  public interface IChoice {
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

  public interface IDefault<out T> {
    bool HasDefaultValue { get; }
    T DefaultValue { get; }
  }

  /// <summary>Shared instances of the default, immutable Prose value formatters.</summary>
  public static class ProseDatatypes {
    public static readonly ProseStringDatatype String = new();
    public static readonly ProseIntDatatype Int = new();
    public static readonly ProseLongDatatype Long = new();
    public static readonly ProseFloatDatatype Float = new();
    public static readonly ProseDoubleDatatype Double = new();
    public static readonly ProseBoolDatatype Bool = new();
    public static readonly ProseColorDatatype Color = new();
    public static readonly ProseFloatDatatype Percent = new(format: "0.0", suffix: "%");
    public static readonly ProseFloatDatatype PercentNormalized = new(format: "0.0", suffix: "%", scale: 100f);

    public static ProseEnumDatatype<T> Enum<T>() where T : struct, Enum => EnumCache<T>.Instance;
    public static ProseObjectDatatype<T> Object<T>() => ObjectCache<T>.Instance;

    private static class EnumCache<T> where T : struct, Enum {
      internal static readonly ProseEnumDatatype<T> Instance = new();
    }

    private static class ObjectCache<T> {
      internal static readonly ProseObjectDatatype<T> Instance = new();
    }
  }

  /// <summary>A semantic property descriptor that can expand itself into property scopes as a fallback.</summary>
  public interface IProsePropertyDatatype<in T> : IProseDatatype<T> {
    string Key { get; }
  }

  public sealed class ProsePropertyDatatype<T> : IProsePropertyDatatype<T> {
    public ProsePropertyDatatype(
      string key,
      IProseDatatype<T> valueDatatype,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false,
      bool hideName = false,
      bool hideSeparator = false,
      string description = null,
      object defaultValue = null
    ) {
      Key = key ?? throw new ArgumentNullException(nameof(key));
      ValueDatatype = valueDatatype ?? throw new ArgumentNullException(nameof(valueDatatype));
      Level = level;
      Hidden = hidden;
      NoWrap = noWrap;
      HideName = hideName;
      HideSeparator = hideSeparator;
      Description = description;
      DefaultValue = defaultValue;
    }

    public string Key { get; }
    public IProseDatatype<T> ValueDatatype { get; }
    public ProseLevel Level { get; }
    public bool Hidden { get; }
    public bool NoWrap { get; }
    public bool HideName { get; }
    public bool HideSeparator { get; }
    public string Description { get; }
    public object DefaultValue { get; }

    public void ToProse(IProseWriter writer, T value) => writer.Property(
      Key, value, ValueDatatype, Level, Hidden, NoWrap, HideName, HideSeparator,
      Description, DefaultValue
    );
  }

  public sealed class ProseStringDatatype :
    IProseDatatype<string>, IStringConvertible<string>, IAffix, IPattern, IReadOnly {
    public ProseStringDatatype(
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

    public string ToString(string value) => value;
    public string FromString(string value) => value;

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

  public sealed class ProseIntDatatype :
    IProseDatatype<int>, IProseDatatype<int?>, IRange<int>, IUnit, IAffix,
    INumericConvertible<int>, IStringConvertible<int> {
    public ProseIntDatatype(
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

    public byte ToByte(int value) => Convert.ToByte(value);
    public int FromByte(byte value) => value;
    public short ToShort(int value) => Convert.ToInt16(value);
    public int FromShort(short value) => value;
    public int ToInt(int value) => value;
    public int FromInt(int value) => value;
    public long ToLong(int value) => value;
    public int FromLong(long value) => Convert.ToInt32(value);
    public float ToFloat(int value) => value;
    public int FromFloat(float value) => Convert.ToInt32(value);
    public double ToDouble(int value) => value;
    public int FromDouble(double value) => Convert.ToInt32(value);
    string IStringConvertible<int>.ToString(int value) => value.ToString(CultureInfo.InvariantCulture);

    int IStringConvertible<int>.FromString(string value) => int.Parse(
      value, NumberStyles.Integer, CultureInfo.InvariantCulture
    );

    public void ToProse(IProseWriter writer, int value) {
      WriteNumber(writer, value.ToString(Format, CultureInfo.InvariantCulture));
    }

    public void ToProse(IProseWriter writer, int? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }

    private void WriteNumber(IProseWriter writer, string number) =>
      ProseDatatypeUtility.WriteDecorated(writer, number, Prefix, Suffix, Unit);
  }

  public sealed class ProseLongDatatype :
    IProseDatatype<long>, IProseDatatype<long?>, IRange<long>, IUnit, IAffix,
    INumericConvertible<long>, IStringConvertible<long> {
    public ProseLongDatatype(
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

    public byte ToByte(long value) => Convert.ToByte(value);
    public long FromByte(byte value) => value;
    public short ToShort(long value) => Convert.ToInt16(value);
    public long FromShort(short value) => value;
    public int ToInt(long value) => Convert.ToInt32(value);
    public long FromInt(int value) => value;
    public long ToLong(long value) => value;
    public long FromLong(long value) => value;
    public float ToFloat(long value) => value;
    public long FromFloat(float value) => Convert.ToInt64(value);
    public double ToDouble(long value) => value;
    public long FromDouble(double value) => Convert.ToInt64(value);
    string IStringConvertible<long>.ToString(long value) => value.ToString(CultureInfo.InvariantCulture);

    long IStringConvertible<long>.FromString(string value) => long.Parse(
      value, NumberStyles.Integer, CultureInfo.InvariantCulture
    );

    public void ToProse(IProseWriter writer, long value) => ProseDatatypeUtility.WriteDecorated(
      writer, value.ToString(Format, CultureInfo.InvariantCulture), Prefix, Suffix, Unit
    );

    public void ToProse(IProseWriter writer, long? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class ProseFloatDatatype :
    IProseDatatype<float>, IProseDatatype<float?>, IRange<float>, IUnit, IAffix,
    INumericConvertible<float>, IStringConvertible<float> {
    public ProseFloatDatatype(
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

    public byte ToByte(float value) => Convert.ToByte(value);
    public float FromByte(byte value) => value;
    public short ToShort(float value) => Convert.ToInt16(value);
    public float FromShort(short value) => value;
    public int ToInt(float value) => Convert.ToInt32(value);
    public float FromInt(int value) => value;
    public long ToLong(float value) => Convert.ToInt64(value);
    public float FromLong(long value) => value;
    public float ToFloat(float value) => value;
    public float FromFloat(float value) => value;
    public double ToDouble(float value) => value;
    public float FromDouble(double value) => Convert.ToSingle(value);
    string IStringConvertible<float>.ToString(float value) => value.ToString("R", CultureInfo.InvariantCulture);

    float IStringConvertible<float>.FromString(string value) => float.Parse(
      value, NumberStyles.Float, CultureInfo.InvariantCulture
    );

    public void ToProse(IProseWriter writer, float value) {
      if (Clamp) value = Math.Max(Min ?? float.MinValue, Math.Min(Max ?? float.MaxValue, value));
      value *= Scale;
      ProseDatatypeUtility.WriteDecorated(
        writer,
        Compact
          ? ProseDatatypeUtility.FormatCompact(value)
          : value.ToString(Format, CultureInfo.InvariantCulture),
        Prefix, Suffix, Unit
      );
    }

    public void ToProse(IProseWriter writer, float? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class ProseDoubleDatatype :
    IProseDatatype<double>, IProseDatatype<double?>, IRange<double>, IUnit, IAffix,
    INumericConvertible<double>, IStringConvertible<double> {
    public ProseDoubleDatatype(
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

    public byte ToByte(double value) => Convert.ToByte(value);
    public double FromByte(byte value) => value;
    public short ToShort(double value) => Convert.ToInt16(value);
    public double FromShort(short value) => value;
    public int ToInt(double value) => Convert.ToInt32(value);
    public double FromInt(int value) => value;
    public long ToLong(double value) => Convert.ToInt64(value);
    public double FromLong(long value) => value;
    public float ToFloat(double value) => Convert.ToSingle(value);
    public double FromFloat(float value) => value;
    public double ToDouble(double value) => value;
    public double FromDouble(double value) => value;
    string IStringConvertible<double>.ToString(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    double IStringConvertible<double>.FromString(string value) => double.Parse(
      value, NumberStyles.Float, CultureInfo.InvariantCulture
    );

    public void ToProse(IProseWriter writer, double value) => ProseDatatypeUtility.WriteDecorated(
      writer,
      Compact ? ProseDatatypeUtility.FormatCompact(value) : value.ToString(Format, CultureInfo.InvariantCulture),
      Prefix, Suffix, Unit
    );

    public void ToProse(IProseWriter writer, double? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class ProseBoolDatatype :
    IProseDatatype<bool>, IProseDatatype<bool?>, IStringConvertible<bool> {
    public ProseBoolDatatype(
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
    public string ToString(bool value) => value.ToString();
    public bool FromString(string value) => bool.Parse(value);
    public void ToProse(IProseWriter writer, bool value) => writer.Write(value ? TrueText : FalseText);

    public void ToProse(IProseWriter writer, bool? value) =>
      writer.Write(value.HasValue ? value.Value ? TrueText : FalseText : NullText);
  }

  /// <summary>Maps nullable flag states to configured text.</summary>
  public sealed class ProseFlagDatatype : IProseDatatype<bool>, IProseDatatype<bool?> {
    public ProseFlagDatatype(
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

  public sealed class ProseEnumDatatype<T> :
    IProseDatatype<T>, IProseDatatype<T?>, IChoice, IAffix,
    IStringConvertible<T>, INumericConvertible<T> where T : struct, Enum {
    private static readonly T[] _values = (T[])Enum.GetValues(typeof(T));

    public ProseEnumDatatype(
      string nullText = ProseLiterals.Null, string prefix = null, string suffix = null
    ) {
      NullText = nullText;
      Prefix = prefix;
      Suffix = suffix;
    }

    public string NullText { get; }
    public string Prefix { get; }
    public string Suffix { get; }
    public int ChoiceCount => _values.Length;
    public object GetChoiceValue(int index) => _values[index];
    public string GetChoiceLabel(int index) => _values[index].ToString();
    public bool IsChoiceEnabled(int index) => true;
    public string ToString(T value) => value.ToString();
    public T FromString(string value) => (T)Enum.Parse(typeof(T), value);
    public byte ToByte(T value) => Convert.ToByte(value);
    public T FromByte(byte value) => (T)Enum.ToObject(typeof(T), value);
    public short ToShort(T value) => Convert.ToInt16(value);
    public T FromShort(short value) => (T)Enum.ToObject(typeof(T), value);
    public int ToInt(T value) => Convert.ToInt32(value);
    public T FromInt(int value) => (T)Enum.ToObject(typeof(T), value);
    public long ToLong(T value) => Convert.ToInt64(value);
    public T FromLong(long value) => (T)Enum.ToObject(typeof(T), value);
    public float ToFloat(T value) => Convert.ToSingle(value);
    public T FromFloat(float value) =>
      (T)Enum.ToObject(typeof(T), Convert.ToInt64(value));
    public double ToDouble(T value) => Convert.ToDouble(value);
    public T FromDouble(double value) =>
      (T)Enum.ToObject(typeof(T), Convert.ToInt64(value));

    public void ToProse(IProseWriter writer, T value) =>
      ProseDatatypeUtility.WriteDecorated(writer, value.ToString(), Prefix, Suffix);

    public void ToProse(IProseWriter writer, T? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  /// <summary>Describes a finite set of choices for values that are not CLR enums.</summary>
  public sealed class ProseChoiceDatatype<T> :
    IProseDatatype<T>, IChoice, IStringConvertible<T>, INumericConvertible<T> {
    private readonly IReadOnlyList<ProseChoice<T>> _choices;

    public ProseChoiceDatatype(IReadOnlyList<ProseChoice<T>> choices) =>
      _choices = choices ?? throw new ArgumentNullException(nameof(choices));

    public IReadOnlyList<ProseChoice<T>> Choices => _choices;
    public int ChoiceCount => _choices.Count;
    public object GetChoiceValue(int index) => _choices[index].Value;
    public string GetChoiceLabel(int index) => _choices[index].Label;
    public bool IsChoiceEnabled(int index) => _choices[index].Enabled;

    public string ToString(T value) {
      for (var i = 0; i < _choices.Count; i++)
        if (EqualityComparer<T>.Default.Equals(_choices[i].Value, value))
          return _choices[i].Label;
      return value is null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    public T FromString(string value) {
      for (var i = 0; i < _choices.Count; i++)
        if (_choices[i].Label == value) return _choices[i].Value;
      return ProseDatatypeUtility.ConvertFrom<T>(value);
    }

    public byte ToByte(T value) => Convert.ToByte(value);
    public T FromByte(byte value) => ProseDatatypeUtility.ConvertFrom<T>(value);
    public short ToShort(T value) => Convert.ToInt16(value);
    public T FromShort(short value) => ProseDatatypeUtility.ConvertFrom<T>(value);
    public int ToInt(T value) => Convert.ToInt32(value);
    public T FromInt(int value) => ProseDatatypeUtility.ConvertFrom<T>(value);
    public long ToLong(T value) => Convert.ToInt64(value);
    public T FromLong(long value) => ProseDatatypeUtility.ConvertFrom<T>(value);
    public float ToFloat(T value) => Convert.ToSingle(value);
    public T FromFloat(float value) => ProseDatatypeUtility.ConvertFrom<T>(value);
    public double ToDouble(T value) => Convert.ToDouble(value);
    public T FromDouble(double value) => ProseDatatypeUtility.ConvertFrom<T>(value);

    public void ToProse(IProseWriter writer, T value) {
      for (var i = 0; i < _choices.Count; i++) {
        if (!EqualityComparer<T>.Default.Equals(_choices[i].Value, value)) continue;
        writer.Write(_choices[i].Label);
        return;
      }
      writer.Write(value is null ? ProseLiterals.Null : value.ToString());
    }
  }

  public sealed class ProseObjectDatatype<T> :
    IProseDatatype<T>, IAffix, IStringConvertible<T> {
    public ProseObjectDatatype(
      string nullText = ProseLiterals.Null, string prefix = null, string suffix = null
    ) {
      NullText = nullText;
      Prefix = prefix;
      Suffix = suffix;
    }

    public string NullText { get; }
    public string Prefix { get; }
    public string Suffix { get; }

    public string ToString(T value) => value == null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);

    public T FromString(string value) => value == null
      ? default
      : (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);

    public void ToProse(IProseWriter writer, T value) {
      ProseDatatypeUtility.WriteDecorated(
        writer, value == null ? NullText : value.ToString(), Prefix, Suffix
      );
    }
  }

  /// <summary>Maps object presence to configured text.</summary>
  public sealed class ProseObjectFlagDatatype<T> : IProseDatatype<T> {
    public ProseObjectFlagDatatype(string ifPresent = null, string ifNull = null) {
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
  public sealed class ProseColorDatatype : IProseDatatype<Color>, IStringConvertible<Color> {
    public ProseColorDatatype(
      string transparentText = "transparent", string prefix = null, string suffix = null
    ) {
      TransparentText = transparentText;
      Prefix = prefix;
      Suffix = suffix;
    }

    public string TransparentText { get; }
    public string Prefix { get; }
    public string Suffix { get; }

    public string ToString(Color value) => value.a == 0 ? TransparentText : value.ToHex();

    public Color FromString(string value) {
      if (value == TransparentText) return Color.clear;
      if (ColorUtility.TryParseHtmlString(value, out var color)) return color;
      throw new FormatException($"Invalid color format: {value}");
    }

    public void ToProse(IProseWriter writer, Color value) => ProseDatatypeUtility.WriteDecorated(
      writer, value.a == 0 ? TransparentText : value.ToHex(), Prefix, Suffix
    );
  }

  /// <summary>Formats UI Toolkit style keywords or delegates formatting of their resolved value.</summary>
  public sealed class ProseStyleValueDatatype<T> : IProseDatatype<IStyleValue<T>> {
    public ProseStyleValueDatatype(
      IProseDatatype<T> valueDatatype = null,
      string nullText = ProseLiterals.Null,
      string autoText = "<auto>",
      string noneText = "<none>",
      string initialText = "<initial>"
    ) {
      ValueDatatype = valueDatatype ?? ProseDatatypes.Object<T>();
      NullText = nullText;
      AutoText = autoText;
      NoneText = noneText;
      InitialText = initialText;
    }

    public IProseDatatype<T> ValueDatatype { get; }
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
        default: writer.Write(value.value, ValueDatatype); break;
      }
    }
  }

  /// <summary>Adapts a reusable delegate to the semantic formatter contract.</summary>
  public sealed class ProseFormattingDatatype<T> : IProseDatatype<T> {
    private readonly Func<T, string> _formatter;

    public ProseFormattingDatatype(
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

    public void ToProse(IProseWriter writer, T value) => ProseDatatypeUtility.WriteDecorated(
      writer, value == null ? NullText : _formatter(value), Prefix, Suffix
    );
  }

  /// <summary>Streams iterable items directly to a writer without materializing an intermediate list.</summary>
  public sealed class ProseIterableDatatype<T> : IProseDatatype<IEnumerable<T>> {
    public ProseIterableDatatype(
      IProseDatatype<T> itemDatatype = null,
      string nullText = ProseLiterals.Null,
      string emptyText = "[]",
      string prefix = "[",
      string separator = ", ",
      string suffix = "]"
    ) {
      ItemDatatype = itemDatatype ?? ProseDatatypes.Object<T>();
      NullText = nullText;
      EmptyText = emptyText;
      Prefix = prefix;
      Separator = separator;
      Suffix = suffix;
    }

    public IProseDatatype<T> ItemDatatype { get; }
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
      writer.Write(enumerator.Current, ItemDatatype);
      while (enumerator.MoveNext()) {
        if (Separator != null) writer.Write(Separator);
        writer.Write(enumerator.Current, ItemDatatype);
      }
      if (Suffix != null) writer.Write(Suffix);
    }
  }

  internal static class ProseDatatypeUtility {
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
