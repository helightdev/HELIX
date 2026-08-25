using System;
using System.Collections.Generic;
using System.Globalization;
using HELIX.Coloring;
using HELIX.Prose;
using HELIX.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global

namespace HELIX {
  public interface IDatatypeRange<T> where T : struct {
    T? Min { get; }
    T? Max { get; }
    T? Step { get; }
  }

  public interface IDatatypeUnit {
    string Unit { get; }
  }

  public interface IDatatypeAffix {
    string Prefix { get; }
    string Suffix { get; }
  }

  public interface IDatatypePattern {
    string Pattern { get; }
  }

  public interface IDatatypeReadOnly {
    bool ReadOnly { get; }
  }

  public interface IDatatypeChoice {
    int ChoiceCount { get; }
    object GetChoiceValue(int index);
    string GetChoiceLabel(int index);
    bool IsChoiceEnabled(int index);
  }

  public interface IDatatypeChoice<out T> : IDatatypeChoice {
    T GetTypedChoiceValue(int index);
  }

  public readonly struct DatatypeChoice<T> {
    public DatatypeChoice(T value, string label, bool enabled = true) {
      Value = value;
      Label = label;
      Enabled = enabled;
    }

    public T Value { get; }
    public string Label { get; }
    public bool Enabled { get; }
  }

  public interface IDatatypeDefault<out T> {
    bool HasDefaultValue { get; }
    T DefaultValue { get; }
  }

  public interface ICompositeDatatype : IDatatype {
    int ComponentCount { get; }
    string GetComponentName(int index);
    Type GetComponentType(int index);
    object GetComponentDatatype(int index);
    object GetComponentValue(object value, int index);
    object SetComponentValue(object value, int index, object componentValue);
    void WriteComponent(IProseWriter writer, object value, int index);
  }

  public interface ICompositeDatatype<T> : IDatatype<T>, ICompositeDatatype { }

  public interface ICollectionProxy {
    Type ItemType { get; }
    int GetItemCount(object collection);
    object GetItem(object collection, int index);
    object SetItem(object collection, int index, object item);
    object AddItem(object collection, object item);
    object RemoveItem(object collection, int index);
  }

  public interface ICollectionProxy<TCollection, TItem, TAccumulator> : ICollectionProxy {
    int GetItemCount(TCollection collection);
    TItem GetItem(TCollection collection, int index);
    TCollection SetItem(TCollection collection, int index, TItem item);
    TCollection AddItem(TCollection collection, TItem item);
    TCollection RemoveItem(TCollection collection, int index);
    TAccumulator AcquireAccumulator(int capacity);
    void Add(ref TAccumulator accumulator, TItem item);
    TCollection Create(ref TAccumulator accumulator);
    void ReleaseAccumulator(ref TAccumulator accumulator);
  }

  public interface ICollectionDatatype : ICompositeDatatype {
    object ItemDatatype { get; }
    ICollectionProxy CollectionProxy { get; }
  }

  public interface ICollectionDatatype<TCollection> : IDatatype<TCollection>, ICollectionDatatype { }

  public interface ICompositeDatatypeComponent<T> {
    string Name { get; }
    Type ValueType { get; }
    object Datatype { get; }
    object GetValue(T value);
    T SetValue(T value, object componentValue);
    bool TryRead(IUniversalReader reader, ref T value);
    bool TryWrite(IUniversalWriter writer, T value);
    void ToProse(IProseWriter writer, T value);
  }

  public sealed class CompositeDatatypeComponent<T, TValue> : ICompositeDatatypeComponent<T> {
    private readonly Func<T, TValue> _getter;
    private readonly Func<T, TValue, T> _setter;

    public CompositeDatatypeComponent(
      string name, IDatatype<TValue> datatype, Func<T, TValue> getter, Func<T, TValue, T> setter
    ) {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Datatype = datatype ?? throw new ArgumentNullException(nameof(datatype));
      _getter = getter ?? throw new ArgumentNullException(nameof(getter));
      _setter = setter ?? throw new ArgumentNullException(nameof(setter));
    }

    public string Name { get; }
    public Type ValueType => typeof(TValue);
    public object Datatype { get; }
    public object GetValue(T value) => _getter(value);
    public T SetValue(T value, object componentValue) => _setter(value, (TValue)componentValue);
    public bool TryRead(IUniversalReader reader, ref T value) {
      if (Datatype is not ISerializableDatatype<TValue> serializable) throw new NotSupportedException(
        $"Component '{Name}' uses datatype '{Datatype.GetType().Name}', which does not support serialization."
      );
      if (!serializable.TryRead(reader, Name, out var component)) return false;
      value = _setter(value, component);
      return true;
    }
    public bool TryWrite(IUniversalWriter writer, T value) {
      if (Datatype is not ISerializableDatatype<TValue> serializable) throw new NotSupportedException(
        $"Component '{Name}' uses datatype '{Datatype.GetType().Name}', which does not support serialization."
      );
      return serializable.TryWrite(writer, Name, _getter(value));
    }
    public void ToProse(IProseWriter writer, T value) =>
      writer.Write(_getter(value), (IDatatype<TValue>)Datatype);
  }

  public sealed class CompositeDatatype<T> :
    ICompositeDatatype<T>, IStringConvertible<T>, ISerializableDatatype<T> {
    private readonly IReadOnlyList<ICompositeDatatypeComponent<T>> _components;
    private readonly Func<T, string> _toString;
    private readonly Func<string, T> _fromString;

    public CompositeDatatype(
      IReadOnlyList<ICompositeDatatypeComponent<T>> components,
      Func<T, string> toString,
      Func<string, T> fromString,
      string prefix = "(", string separator = ", ", string suffix = ")"
    ) {
      _components = components ?? throw new ArgumentNullException(nameof(components));
      _toString = toString ?? throw new ArgumentNullException(nameof(toString));
      _fromString = fromString ?? throw new ArgumentNullException(nameof(fromString));
      Prefix = prefix;
      Separator = separator;
      Suffix = suffix;
    }

    public int ComponentCount => _components.Count;
    public string Prefix { get; }
    public string Separator { get; }
    public string Suffix { get; }
    public string GetComponentName(int index) => _components[index].Name;
    public Type GetComponentType(int index) => _components[index].ValueType;
    public object GetComponentDatatype(int index) => _components[index].Datatype;
    public object GetComponentValue(object value, int index) => _components[index].GetValue((T)value);
    public object SetComponentValue(object value, int index, object componentValue) =>
      _components[index].SetValue((T)value, componentValue);
    public void WriteComponent(IProseWriter writer, object value, int index) =>
      _components[index].ToProse(writer, (T)value);
    public string ToString(T value) => _toString(value);
    public T FromString(string value) => _fromString(value);

    public bool TryRead(IUniversalReader reader, string name, out T value) {
      value = default;
      if (!reader.TryEnterObject(name)) return false;
      var success = true;
      for (var i = 0; i < _components.Count; i++)
        if (!_components[i].TryRead(reader, ref value)) { success = false; break; }
      return reader.TryExitObject() && success;
    }

    public bool TryWrite(IUniversalWriter writer, string name, T value) {
      if (!writer.TryBeginObject(name)) return false;
      var success = true;
      for (var i = 0; i < _components.Count; i++)
        if (!_components[i].TryWrite(writer, value)) { success = false; break; }
      return writer.TryEndObject() && success;
    }

    public void ToProse(IProseWriter writer, T value) {
      if (Prefix != null) writer.Write(Prefix);
      for (var i = 0; i < _components.Count; i++) {
        if (i != 0 && Separator != null) writer.Write(Separator);
        writer.Write(_components[i].Name);
        writer.Write(": ");
        _components[i].ToProse(writer, value);
      }
      if (Suffix != null) writer.Write(Suffix);
    }
  }

  /// <summary>Shared instances of the default, immutable Prose value formatters.</summary>
  public static class Datatypes {
    public static readonly StringDatatype String = new();
    public static readonly IntDatatype Int = new();
    public static readonly LongDatatype Long = new();
    public static readonly FloatDatatype Float = new();
    public static readonly DoubleDatatype Double = new();
    public static readonly BoolDatatype Bool = new();
    public static readonly ColorDatatype Color = new();
    public static readonly CompositeDatatype<Vector2> Vector2 = new(
      new ICompositeDatatypeComponent<Vector2>[] {
        new CompositeDatatypeComponent<Vector2, float>("X", Float, value => value.x,
          (value, component) => { value.x = component; return value; }),
        new CompositeDatatypeComponent<Vector2, float>("Y", Float, value => value.y,
          (value, component) => { value.y = component; return value; })
      },
      HelixConvert.ToUssString,
      value => HelixConvert.ToVector2(value, out var result)
        ? result : throw new FormatException($"Invalid Vector2 format: {value}")
    );
    public static readonly CompositeDatatype<Vector3> Vector3 = new(
      new ICompositeDatatypeComponent<Vector3>[] {
        new CompositeDatatypeComponent<Vector3, float>("X", Float, value => value.x,
          (value, component) => { value.x = component; return value; }),
        new CompositeDatatypeComponent<Vector3, float>("Y", Float, value => value.y,
          (value, component) => { value.y = component; return value; }),
        new CompositeDatatypeComponent<Vector3, float>("Z", Float, value => value.z,
          (value, component) => { value.z = component; return value; })
      },
      HelixConvert.ToUssString,
      value => HelixConvert.ToVector3(value, out var result)
        ? result : throw new FormatException($"Invalid Vector3 format: {value}")
    );
    public static readonly CompositeDatatype<Vector4> Vector4 = new(
      new ICompositeDatatypeComponent<Vector4>[] {
        new CompositeDatatypeComponent<Vector4, float>("X", Float, value => value.x,
          (value, component) => { value.x = component; return value; }),
        new CompositeDatatypeComponent<Vector4, float>("Y", Float, value => value.y,
          (value, component) => { value.y = component; return value; }),
        new CompositeDatatypeComponent<Vector4, float>("Z", Float, value => value.z,
          (value, component) => { value.z = component; return value; }),
        new CompositeDatatypeComponent<Vector4, float>("W", Float, value => value.w,
          (value, component) => { value.w = component; return value; })
      },
      HelixConvert.ToUssString,
      value => HelixConvert.ToVector4(value, out var result)
        ? result : throw new FormatException($"Invalid Vector4 format: {value}")
    );
    public static readonly FloatDatatype Percent = new(format: "0.0", suffix: "%", min: 0f, max: 100f);
    public static readonly FloatDatatype PercentNormalized = new(format: "0.0", suffix: "%", scale: 100f, min: 0f, max: 1f);

    public static EnumDatatype<T> Enum<T>() where T : struct, Enum => EnumCache<T>.Instance;
    public static ObjectDatatype<T> Object<T>() => ObjectCache<T>.Instance;

    private static class EnumCache<T> where T : struct, Enum {
      internal static readonly EnumDatatype<T> Instance = new();
    }

    private static class ObjectCache<T> {
      internal static readonly ObjectDatatype<T> Instance = new();
    }
  }

  /// <summary>A semantic property descriptor that can expand itself into property scopes as a fallback.</summary>
  public interface IPropertyDatatype<in T> : IDatatype<T> {
    string Key { get; }
  }

  public sealed class PropertyDatatype<T> : IPropertyDatatype<T> {
    public PropertyDatatype(
      string key,
      IDatatype<T> valueDatatype,
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
    public IDatatype<T> ValueDatatype { get; }
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

  public sealed class StringDatatype :
    IDatatype<string>, IStringConvertible<string>, ISerializableDatatype<string>,
    IDatatypeAffix, IDatatypePattern, IDatatypeReadOnly {
    public StringDatatype(
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
    public bool TryRead(IUniversalReader reader, string name, out string value) {
      value = null;
      if (!reader.TryReadNull(name, out var isNull)) return false;
      if (isNull) return true;
      return reader.TryReadString(name, out value);
    }
    public bool TryWrite(IUniversalWriter writer, string name, string value) {
      var isNull = value == null;
      if (!writer.TryWriteNull(name, isNull)) return false;
      return isNull || writer.TryWriteString(name, value);
    }

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

  public sealed class IntDatatype :
    IDatatype<int>, IDatatype<int?>, IDatatypeRange<int>, IDatatypeUnit, IDatatypeAffix,
    INumericConvertible<int>, IStringConvertible<int>, ISerializableDatatype<int> {
    public IntDatatype(
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
    public bool TryRead(IUniversalReader reader, string name, out int value) =>
      reader.TryReadInt32(name, out value);
    public bool TryWrite(IUniversalWriter writer, string name, int value) =>
      writer.TryWriteInt32(name, value);

    public void ToProse(IProseWriter writer, int value) {
      WriteNumber(writer, value.ToString(Format, CultureInfo.InvariantCulture));
    }

    public void ToProse(IProseWriter writer, int? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }

    private void WriteNumber(IProseWriter writer, string number) =>
      DatatypeUtility.WriteDecorated(writer, number, Prefix, Suffix, Unit);
  }

  public sealed class LongDatatype :
    IDatatype<long>, IDatatype<long?>, IDatatypeRange<long>, IDatatypeUnit, IDatatypeAffix,
    INumericConvertible<long>, IStringConvertible<long>, ISerializableDatatype<long> {
    public LongDatatype(
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
    public bool TryRead(IUniversalReader reader, string name, out long value) =>
      reader.TryReadInt64(name, out value);
    public bool TryWrite(IUniversalWriter writer, string name, long value) =>
      writer.TryWriteInt64(name, value);

    public void ToProse(IProseWriter writer, long value) => DatatypeUtility.WriteDecorated(
      writer, value.ToString(Format, CultureInfo.InvariantCulture), Prefix, Suffix, Unit
    );

    public void ToProse(IProseWriter writer, long? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class FloatDatatype :
    IDatatype<float>, IDatatype<float?>, IDatatypeRange<float>, IDatatypeUnit, IDatatypeAffix,
    INumericConvertible<float>, IStringConvertible<float>, ISerializableDatatype<float> {
    public FloatDatatype(
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
    public float ToFloat(float value) => value * Scale;
    public float FromFloat(float value) => value / Scale;
    public double ToDouble(float value) => value;
    public float FromDouble(double value) => Convert.ToSingle(value);
    string IStringConvertible<float>.ToString(float value) {
      value *= Scale;
      return Compact
        ? DatatypeUtility.FormatCompact(value)
        : value.ToString(Format, CultureInfo.InvariantCulture);
    }

    float IStringConvertible<float>.FromString(string value) => float.Parse(
      value, NumberStyles.Float, CultureInfo.InvariantCulture
    ) / Scale;
    public bool TryRead(IUniversalReader reader, string name, out float value) =>
      reader.TryReadSingle(name, out value);
    public bool TryWrite(IUniversalWriter writer, string name, float value) =>
      writer.TryWriteSingle(name, value);

    public void ToProse(IProseWriter writer, float value) {
      if (Clamp) value = Math.Max(Min ?? float.MinValue, Math.Min(Max ?? float.MaxValue, value));
      value *= Scale;
      DatatypeUtility.WriteDecorated(
        writer,
        Compact
          ? DatatypeUtility.FormatCompact(value)
          : value.ToString(Format, CultureInfo.InvariantCulture),
        Prefix, Suffix, Unit
      );
    }

    public void ToProse(IProseWriter writer, float? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class DoubleDatatype :
    IDatatype<double>, IDatatype<double?>, IDatatypeRange<double>, IDatatypeUnit, IDatatypeAffix,
    INumericConvertible<double>, IStringConvertible<double>, ISerializableDatatype<double> {
    public DoubleDatatype(
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
    public bool TryRead(IUniversalReader reader, string name, out double value) =>
      reader.TryReadDouble(name, out value);
    public bool TryWrite(IUniversalWriter writer, string name, double value) =>
      writer.TryWriteDouble(name, value);

    public void ToProse(IProseWriter writer, double value) => DatatypeUtility.WriteDecorated(
      writer,
      Compact ? DatatypeUtility.FormatCompact(value) : value.ToString(Format, CultureInfo.InvariantCulture),
      Prefix, Suffix, Unit
    );

    public void ToProse(IProseWriter writer, double? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  public sealed class BoolDatatype :
    IDatatype<bool>, IDatatype<bool?>, IStringConvertible<bool>, ISerializableDatatype<bool> {
    public BoolDatatype(
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
    public bool TryRead(IUniversalReader reader, string name, out bool value) =>
      reader.TryReadBoolean(name, out value);
    public bool TryWrite(IUniversalWriter writer, string name, bool value) =>
      writer.TryWriteBoolean(name, value);
    public void ToProse(IProseWriter writer, bool value) => writer.Write(value ? TrueText : FalseText);

    public void ToProse(IProseWriter writer, bool? value) =>
      writer.Write(value.HasValue ? value.Value ? TrueText : FalseText : NullText);
  }

  /// <summary>Maps nullable flag states to configured text.</summary>
  public sealed class FlagDatatype : IDatatype<bool>, IDatatype<bool?>, IDatatypeChoice<bool> {
    public FlagDatatype(
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
    public int ChoiceCount => 2;
    public object GetChoiceValue(int index) => GetTypedChoiceValue(index);
    public bool GetTypedChoiceValue(int index) => index switch {
      0 => false,
      1 => true,
      _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
    public string GetChoiceLabel(int index) => index switch {
      0 => IfFalse ?? bool.FalseString,
      1 => IfTrue ?? bool.TrueString,
      _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
    public bool IsChoiceEnabled(int index) => index is 0 or 1;
    public void ToProse(IProseWriter writer, bool value) => writer.Write(value ? IfTrue : IfFalse);

    public void ToProse(IProseWriter writer, bool? value) =>
      writer.Write(value.HasValue ? value.Value ? IfTrue : IfFalse : IfNull);
  }

  public sealed class EnumDatatype<T> :
    IDatatype<T>, IDatatype<T?>, IDatatypeChoice<T>, IDatatypeAffix,
    IStringConvertible<T>, INumericConvertible<T>, ISerializableDatatype<T> where T : struct, Enum {
    private static readonly T[] _values = (T[])Enum.GetValues(typeof(T));

    public EnumDatatype(
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
    public T GetTypedChoiceValue(int index) => _values[index];
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
    public bool TryRead(IUniversalReader reader, string name, out T value) {
      value = default;
      if (!reader.TryReadInt64(name, out var encoded)) return false;
      value = (T)Enum.ToObject(typeof(T), encoded);
      return true;
    }
    public bool TryWrite(IUniversalWriter writer, string name, T value) =>
      writer.TryWriteInt64(name, Convert.ToInt64(value));

    public void ToProse(IProseWriter writer, T value) =>
      DatatypeUtility.WriteDecorated(writer, value.ToString(), Prefix, Suffix);

    public void ToProse(IProseWriter writer, T? value) {
      if (value.HasValue) ToProse(writer, value.Value);
      else writer.Write(NullText);
    }
  }

  /// <summary>Describes a finite set of choices for values that are not CLR enums.</summary>
  public sealed class DatatypeChoiceDatatype<T> :
    IDatatype<T>, IDatatypeChoice<T>, IStringConvertible<T>, INumericConvertible<T> {
    private readonly IReadOnlyList<DatatypeChoice<T>> _choices;

    public DatatypeChoiceDatatype(IReadOnlyList<DatatypeChoice<T>> choices) =>
      _choices = choices ?? throw new ArgumentNullException(nameof(choices));

    public IReadOnlyList<DatatypeChoice<T>> Choices => _choices;
    public int ChoiceCount => _choices.Count;
    public object GetChoiceValue(int index) => _choices[index].Value;
    public T GetTypedChoiceValue(int index) => _choices[index].Value;
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
      return DatatypeUtility.ConvertFrom<T>(value);
    }

    public byte ToByte(T value) => Convert.ToByte(value);
    public T FromByte(byte value) => DatatypeUtility.ConvertFrom<T>(value);
    public short ToShort(T value) => Convert.ToInt16(value);
    public T FromShort(short value) => DatatypeUtility.ConvertFrom<T>(value);
    public int ToInt(T value) => Convert.ToInt32(value);
    public T FromInt(int value) => DatatypeUtility.ConvertFrom<T>(value);
    public long ToLong(T value) => Convert.ToInt64(value);
    public T FromLong(long value) => DatatypeUtility.ConvertFrom<T>(value);
    public float ToFloat(T value) => Convert.ToSingle(value);
    public T FromFloat(float value) => DatatypeUtility.ConvertFrom<T>(value);
    public double ToDouble(T value) => Convert.ToDouble(value);
    public T FromDouble(double value) => DatatypeUtility.ConvertFrom<T>(value);

    public void ToProse(IProseWriter writer, T value) {
      for (var i = 0; i < _choices.Count; i++) {
        if (!EqualityComparer<T>.Default.Equals(_choices[i].Value, value)) continue;
        writer.Write(_choices[i].Label);
        return;
      }
      writer.Write(value is null ? ProseLiterals.Null : value.ToString());
    }
  }

  public sealed class ObjectDatatype<T> :
    IDatatype<T>, IDatatypeAffix, IStringConvertible<T>, ISerializableDatatype<T> {
    public ObjectDatatype(
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

    public bool TryRead(IUniversalReader reader, string name, out T value) {
      value = default;
      if (!typeof(T).IsValueType) {
        if (!reader.TryReadNull(name, out var isNull)) return false;
        if (isNull) return true;
      }
      return reader.SupportsCustomType(typeof(T)) && reader.TryReadCustom(name, out value);
    }

    public bool TryWrite(IUniversalWriter writer, string name, T value) {
      if (!typeof(T).IsValueType) {
        var isNull = value is null;
        if (!writer.TryWriteNull(name, isNull)) return false;
        if (isNull) return true;
      }
      return writer.SupportsCustomType(typeof(T)) && writer.TryWriteCustom(name, value);
    }

    public void ToProse(IProseWriter writer, T value) {
      DatatypeUtility.WriteDecorated(
        writer, value == null ? NullText : value.ToString(), Prefix, Suffix
      );
    }
  }

  /// <summary>Maps object presence to configured text.</summary>
  public sealed class ObjectFlagDatatype<T> : IDatatype<T> {
    public ObjectFlagDatatype(string ifPresent = null, string ifNull = null) {
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
  public sealed class ColorDatatype : IDatatype<Color>, IStringConvertible<Color> {
    public ColorDatatype(
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

    public void ToProse(IProseWriter writer, Color value) => DatatypeUtility.WriteDecorated(
      writer, value.a == 0 ? TransparentText : value.ToHex(), Prefix, Suffix
    );
  }

  /// <summary>Formats UI Toolkit style keywords or delegates formatting of their resolved value.</summary>
  public sealed class StyleValueDatatype<T> : IDatatype<IStyleValue<T>> {
    public StyleValueDatatype(
      IDatatype<T> valueDatatype = null,
      string nullText = ProseLiterals.Null,
      string autoText = "<auto>",
      string noneText = "<none>",
      string initialText = "<initial>"
    ) {
      ValueDatatype = valueDatatype ?? Datatypes.Object<T>();
      NullText = nullText;
      AutoText = autoText;
      NoneText = noneText;
      InitialText = initialText;
    }

    public IDatatype<T> ValueDatatype { get; }
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
  public sealed class FormattingDatatype<T> : IDatatype<T> {
    private readonly Func<T, string> _formatter;

    public FormattingDatatype(
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

    public void ToProse(IProseWriter writer, T value) => DatatypeUtility.WriteDecorated(
      writer, value == null ? NullText : _formatter(value), Prefix, Suffix
    );
  }

}
