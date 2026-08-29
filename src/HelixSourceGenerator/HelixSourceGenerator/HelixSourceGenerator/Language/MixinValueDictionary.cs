using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HELIX.SourceGen.Expressions;

/// <summary>Object-compatible dictionary whose storage is always strongly typed.</summary>
internal sealed class MixinValueDictionary : IDictionary<string, object>, IReadOnlyDictionary<string, object> {
  private readonly Dictionary<MixinString, IMixinValue> _values;
  private readonly MixinStringPool _pool;

  internal MixinValueDictionary(MixinStringPool pool = null) {
    _pool = pool ?? new MixinStringPoolBuilder().Freeze();
    _values = new Dictionary<MixinString, IMixinValue>();
  }
  internal MixinValueDictionary(
    IEnumerable<KeyValuePair<string, object>> values, MixinStringPool pool = null
  ) : this(pool) {
    foreach (var item in values) this[item.Key] = item.Value;
  }

  private MixinString Key(string key) => _pool.Get(key);

  private string Name(MixinString key) => key.Resolve(_pool);

  public object this[string key] {
    get => _values[Key(key)].BackingValue;
    set => _values[Key(key)] = Close(MixinValue.From(value));
  }

  public ICollection<string> Keys => _values.Keys.Select(Name).ToArray();
  IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => _values.Keys.Select(Name);
  public ICollection<object> Values => _values.Values.Select(item => item.BackingValue).ToArray();
  IEnumerable<object> IReadOnlyDictionary<string, object>.Values => _values.Values.Select(item => item.BackingValue);
  public int Count => _values.Count;
  public bool IsReadOnly => false;
  public void Add(string key, object value) => _values.Add(Key(key), Close(MixinValue.From(value)));
  public void Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);
  public bool ContainsKey(string key) => _values.ContainsKey(Key(key));
  public bool Remove(string key) => _values.Remove(Key(key));
  public bool TryGetValue(string key, out object value) {
    if (_values.TryGetValue(Key(key), out var typed)) { value = typed.BackingValue; return true; }
    value = null; return false;
  }
  internal bool TryGetMixinValue(string key, out IMixinValue value) =>
    _values.TryGetValue(Key(key), out value);

  private static IMixinValue Close(IMixinValue value) {
    if (value.BackingValue is MixinExpressionTable table) table.Close();
    return value;
  }
  public void Clear() => _values.Clear();
  public bool Contains(KeyValuePair<string, object> item) =>
    TryGetValue(item.Key, out var value) && Equals(value, item.Value);
  public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
    foreach (var item in this) array[arrayIndex++] = item;
  }
  public bool Remove(KeyValuePair<string, object> item) => Contains(item) && Remove(item.Key);
  public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values
    .Select(item => new KeyValuePair<string, object>(Name(item.Key), item.Value.BackingValue)).GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
