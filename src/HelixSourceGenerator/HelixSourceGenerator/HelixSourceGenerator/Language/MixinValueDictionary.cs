using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HelixSourceGenerator.Language;

/// <summary>Object-compatible dictionary whose storage is always strongly typed.</summary>
internal sealed class MixinValueDictionary : IDictionary<string, object>, IReadOnlyDictionary<string, object> {
  private readonly MixinStringPool _pool;
  private readonly Dictionary<MixinString, IMixinValue> _values;

  internal MixinValueDictionary(MixinStringPool pool = null) {
    _pool = pool ?? new MixinStringPoolBuilder().Freeze();
    _values = new Dictionary<MixinString, IMixinValue>();
  }

  internal MixinValueDictionary(
    IEnumerable<KeyValuePair<string, object>> values, MixinStringPool pool = null
  ) : this(pool) {
    foreach (var item in values) this[item.Key] = item.Value;
  }

  internal IEnumerable<KeyValuePair<MixinString, IMixinValue>> TypedValues => _values;

  public object this[string key] {
    get => _values[Key(key)].Value;
    set => _values[Key(key)] = Close(MixinValue.From(value));
  }

  public ICollection<string> Keys => [.. _values.Keys.Select(Name)];
  public ICollection<object> Values => [.. _values.Values.Select(item => item.Value)];
  public int Count => _values.Count;
  public bool IsReadOnly => false;

  public void Add(string key, object value) {
    _values.Add(Key(key), Close(MixinValue.From(value)));
  }

  public void Add(KeyValuePair<string, object> item) {
    Add(item.Key, item.Value);
  }

  public bool ContainsKey(string key) {
    return _values.ContainsKey(Key(key));
  }

  public bool Remove(string key) {
    return _values.Remove(Key(key));
  }

  public bool TryGetValue(string key, out object value) {
    if (_values.TryGetValue(Key(key), out var typed)) {
      value = typed.Value;
      return true;
    }
    value = null;
    return false;
  }

  public void Clear() {
    _values.Clear();
  }

  public bool Contains(KeyValuePair<string, object> item) {
    return TryGetValue(item.Key, out var value) && Equals(value, item.Value);
  }

  public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
    foreach (var item in this) array[arrayIndex++] = item;
  }

  public bool Remove(KeyValuePair<string, object> item) {
    return Contains(item) && Remove(item.Key);
  }

  public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
    return _values
      .Select(item => new KeyValuePair<string, object>(Name(item.Key), item.Value.Value)).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

  IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => _values.Keys.Select(Name);
  IEnumerable<object> IReadOnlyDictionary<string, object>.Values => _values.Values.Select(item => item.Value);

  private MixinString Key(string key) {
    return _pool.Get(key);
  }

  private string Name(MixinString key) {
    return key.Resolve(_pool);
  }

  internal bool TryGetMixinValue(string key, out IMixinValue value) {
    return _values.TryGetValue(Key(key), out value);
  }

  private static IMixinValue Close(IMixinValue value) {
    if (value.Value is MixinExpressionTable table) table.Close();
    return value;
  }
}