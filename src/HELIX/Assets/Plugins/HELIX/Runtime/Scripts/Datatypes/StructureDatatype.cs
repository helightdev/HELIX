using System;
using System.Collections.Generic;
using HELIX.Prose;
using HELIX.Serialization;

namespace HELIX {
  public delegate TValue StructurePropertyGetter<T, out TValue>(ref T structure);

  public delegate void StructurePropertySetter<T, in TValue>(ref T structure, TValue value);

  /// <summary>Visits a heterogeneous structure property without boxing its value.</summary>
  public interface IStructurePropertyVisitor<T> {
    void Visit<TValue>(ref T structure, StructurePropertyDatatype<T, TValue> property);
  }

  public interface IDatatypeReference {
    IDatatype Datatype { get; }
  }

  public class ConfigurableStructureDatatype<T> {
    private readonly StructureDatatype<T> _datatype;
    private readonly Action<StructureDatatype<T>> _configure;
    private bool _isConfigured;

    public ConfigurableStructureDatatype(
      StructureDatatype<T> datatype,
      Action<StructureDatatype<T>> configure = null
    ) {
      _datatype = datatype;
      _configure = configure;
    }

    public StructureDatatype<T> Datatype {
      get {
        if (_isConfigured) return _datatype;
        _configure?.Invoke(_datatype);
        _isConfigured = true;
        return _datatype;
      }
    }
  }

  /// <summary>A generated or manually declared property of a structured datatype.</summary>
  public abstract class StructurePropertyDatatype<T> : IPropertyDatatype<T> {
    protected StructurePropertyDatatype(
      string fieldName, IList<IProseModifier> modifiers = null,
      bool required = true, object defaultValue = null
    ) {
      if (string.IsNullOrWhiteSpace(fieldName))
        throw new ArgumentException("A field name is required.", nameof(fieldName));
      FieldName = fieldName;
      Modifiers = modifiers == null ? new List<IProseModifier>() : new List<IProseModifier>(modifiers);
      Required = required;
      DefaultValue = defaultValue;
    }

    public string FieldName { get; }
    public string Key => FieldName;
    public bool Required { get; }
    public object DefaultValue { get; }
    public IList<IProseModifier> Modifiers { get; }
    public abstract Type ValueType { get; }
    public abstract IDatatype ValueDatatype { get; set; }
    internal abstract object GetBoxed(ref T structure);
    internal abstract void SetBoxed(ref T structure, object value);

    public abstract void Visit<TVisitor>(ref T structure, ref TVisitor visitor)
    where TVisitor : IStructurePropertyVisitor<T>;

    public abstract bool TryRead(IUniversalReader reader, ref T structure);
    public abstract bool TryWrite(IUniversalWriter writer, ref T structure);
    public abstract void ToProse(IProseWriter writer, ref T value);
    void IDatatype<T>.ToProse(IProseWriter writer, T value) => ToProse(writer, ref value);

    protected void PushModifiers(IProseWriter writer) {
      for (var i = 0; i < Modifiers.Count; i++) writer.Push(Modifiers[i]);
    }
  }

  /// <summary>A strongly typed property descriptor used by generated structure datatype wrappers.</summary>
  public sealed class StructurePropertyDatatype<T, TValue> : StructurePropertyDatatype<T> {
    private readonly StructurePropertyGetter<T, TValue> _getter;
    private readonly StructurePropertySetter<T, TValue> _setter;

    public StructurePropertyDatatype(
      string fieldName, IDatatype<TValue> datatype,
      StructurePropertyGetter<T, TValue> getter, StructurePropertySetter<T, TValue> setter,
      IList<IProseModifier> modifiers = null,
      bool required = true, object defaultValue = null
    ) : base(fieldName, modifiers, required, defaultValue) {
      Datatype = datatype ?? throw new ArgumentNullException(nameof(datatype));
      _getter = getter ?? throw new ArgumentNullException(nameof(getter));
      _setter = setter ?? throw new ArgumentNullException(nameof(setter));
    }

    public IDatatype<TValue> Datatype { get; set; }
    public override Type ValueType => typeof(TValue);
    public override IDatatype ValueDatatype { get => Datatype; set => Datatype = value as IDatatype<TValue>; }
    public TValue GetValue(ref T structure) => _getter(ref structure);
    public void SetValue(ref T structure, TValue value) => _setter(ref structure, value);
    internal override object GetBoxed(ref T structure) => _getter(ref structure);
    internal override void SetBoxed(ref T structure, object value) => _setter(ref structure, (TValue)value);
    public override void Visit<TVisitor>(ref T structure, ref TVisitor visitor) => visitor.Visit(ref structure, this);

    public override bool TryRead(IUniversalReader reader, ref T structure) {
      if (Datatype is not ISerializableDatatype<TValue> serializable) throw NotSerializable();
      if (!serializable.TryRead(reader, FieldName, out var value)) return false;
      _setter(ref structure, value);
      return true;
    }

    public override bool TryWrite(IUniversalWriter writer, ref T structure) {
      if (Datatype is not ISerializableDatatype<TValue> serializable) throw NotSerializable();
      return serializable.TryWrite(writer, FieldName, _getter(ref structure));
    }

    public override void ToProse(IProseWriter writer, ref T value) {
      using (writer.Property()) {
        PushModifiers(writer);
        using (writer.PropertyKey()) writer.Write(FieldName);
        using (writer.PropertyValue()) writer.Write(_getter(ref value), Datatype);
      }
    }

    private NotSupportedException NotSerializable() => new(
      $"Field '{FieldName}' uses datatype '{Datatype.GetType().Name}', which does not support serialization. " +
      $"Implement {nameof(ISerializableDatatype<TValue>)} on the field datatype."
    );
  }

  /// <summary>
  /// Describes a prop-like structure as named, typed properties and provides its prose, control and serialization
  /// fallbacks without reflection.
  /// </summary>
  public sealed class StructureDatatype<T> : IPropertyDatatype<T>, ICompositeDatatype<T>, ISerializableDatatype<T> {
    private readonly Func<T> _factory;
    private readonly IList<StructurePropertyDatatype<T>> _properties;

    public StructureDatatype(
      string name,
      IList<StructurePropertyDatatype<T>> properties,
      Func<T> factory = null,
      IList<IProseModifier> modifiers = null
    ) {
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A structure name is required.", nameof(name));
      Name = name;
      if (properties == null) throw new ArgumentNullException(nameof(properties));
      _properties = new List<StructurePropertyDatatype<T>>(properties);
      _factory = factory ?? (() => default);
      Modifiers = modifiers == null ? new List<IProseModifier>() : new List<IProseModifier>(modifiers);
      for (var i = 0; i < _properties.Count; i++)
        if (_properties[i] == null)
          throw new ArgumentException("Structure properties cannot contain null.", nameof(properties));
    }

    public string Name { get; }
    public string Key => Name;
    public IList<IProseModifier> Modifiers { get; }
    public IList<StructurePropertyDatatype<T>> Properties => _properties;

    public void VisitComponent<TVisitor>(ref T value, int index, ref TVisitor visitor)
    where TVisitor : IStructurePropertyVisitor<T> => _properties[index].Visit(ref value, ref visitor);

    public void WriteComponent(IProseWriter writer, ref T value, int index) =>
      _properties[index].ToProse(writer, ref value);

    int ICompositeDatatype.ComponentCount => _properties.Count;
    string ICompositeDatatype.GetComponentName(int index) => _properties[index].FieldName;
    Type ICompositeDatatype.GetComponentType(int index) => _properties[index].ValueType;
    object ICompositeDatatype.GetComponentDatatype(int index) => _properties[index].ValueDatatype;

    object ICompositeDatatype.GetComponentValue(object value, int index) {
      var typed = (T)value;
      return _properties[index].GetBoxed(ref typed);
    }

    object ICompositeDatatype.SetComponentValue(object value, int index, object componentValue) {
      var typed = (T)value;
      _properties[index].SetBoxed(ref typed, componentValue);
      return typed;
    }

    void ICompositeDatatype.WriteComponent(IProseWriter writer, object value, int index) {
      var typed = (T)value;
      _properties[index].ToProse(writer, ref typed);
    }

    public StructurePropertyDatatype<T> GetProperty(string fieldName) {
      for (var i = 0; i < _properties.Count; i++)
        if (_properties[i].FieldName == fieldName)
          return _properties[i];
      return null;
    }

    public void ToProse(IProseWriter writer, T value) {
      using (writer.Tree()) {
        for (var i = 0; i < Modifiers.Count; i++) writer.Push(Modifiers[i]);
        for (var i = 0; i < _properties.Count; i++) _properties[i].ToProse(writer, ref value);
      }
    }

    public bool TryRead(IUniversalReader reader, string name, out T value) {
      if (reader == null) throw new ArgumentNullException(nameof(reader));
      value = default;
      if (!typeof(T).IsValueType) {
        if (!reader.TryReadNull(name, out var isNull)) return false;
        if (isNull) return true;
      }
      if (!reader.TryEnterObject(name)) return false;
      var result = _factory();
      var success = true;
      for (var i = 0; i < _properties.Count; i++)
        if (!_properties[i].TryRead(reader, ref result)) {
          success = false;
          break;
        }
      if (!reader.TryExitObject()) success = false;
      if (!success) return false;
      value = result;
      return true;
    }

    public bool TryWrite(IUniversalWriter writer, string name, T value) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!typeof(T).IsValueType) {
        var isNull = value is null;
        if (!writer.TryWriteNull(name, isNull)) return false;
        if (isNull) return true;
      }
      if (!writer.TryBeginObject(name)) return false;
      var success = true;
      for (var i = 0; i < _properties.Count; i++)
        if (!_properties[i].TryWrite(writer, ref value)) {
          success = false;
          break;
        }
      return writer.TryEndObject() && success;
    }
  }
}