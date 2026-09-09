using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hix;

public readonly struct HixString : IEquatable<HixString> {
  private readonly bool _initialized;

  private HixString(int id, string dynamicValue) {
    Id = id;
    DynamicValue = dynamicValue;
    _initialized = true;
  }

  public int Id { get; }
  public string DynamicValue { get; }
  public bool IsInterned => _initialized && Id >= 0;
  public bool IsNull => !_initialized || (IsInterned ? Id < 0 : DynamicValue is null);

  internal static HixString Interned(int id) => new(id, null);

  public static HixString Dynamic(string value) => new(-1, value);

  public string Resolve(HixStringPool pool) => IsInterned ? pool[Id] : DynamicValue;

  public bool Equals(HixString other) {
    return IsInterned == other.IsInterned && (IsInterned
      ? Id == other.Id
      : string.Equals(DynamicValue, other.DynamicValue, StringComparison.Ordinal));
  }

  public override bool Equals(object value) => value is HixString other && Equals(other);

  public override int GetHashCode() => IsInterned ? Id : StringComparer.Ordinal.GetHashCode(DynamicValue ?? "");

  public static bool operator ==(HixString left, HixString right) => left.Equals(right);

  public static bool operator !=(HixString left, HixString right) => !left.Equals(right);
}

public sealed class HixStringPool {
  private readonly Dictionary<string, int> _ids;
  private readonly string[] _values;

  internal HixStringPool(string[] values, IReadOnlyDictionary<string, int> ids) {
    _values = values ?? [];
    _ids = new Dictionary<string, int>(StringComparer.Ordinal);
    if (ids is not null) {
      foreach (var item in ids)
        _ids.Add(item.Key, item.Value);
    }
  }

  public int Count => _values.Length;
  public string this[int id] => _values[id];

  public bool TryGetId(string value, out int id) {
    value ??= "";
    return _ids.TryGetValue(value, out id);
  }

  public HixString Get(string value) {
    value ??= "";
    if (TryGetId(value, out var id)) return HixString.Interned(id);
    return HixString.Dynamic(value);
  }
}

public sealed class HixStringPoolBuilder {
  private readonly Dictionary<string, int> _ids = new(StringComparer.Ordinal);
  private readonly List<string> _values = [];

  public HixStringPoolBuilder() { }

  internal HixStringPoolBuilder(HixStringPool seed) {
    if (seed is null) return;
    for (var id = 0; id < seed.Count; id++) Intern(seed[id]);
  }

  public HixString Intern(string value) {
    value ??= "";
    if (_ids.TryGetValue(value, out var id)) return HixString.Interned(id);
    id = _values.Count;
    _values.Add(value);
    _ids.Add(value, id);
    return HixString.Interned(id);
  }

  public HixStringPool Freeze() {
    return new HixStringPool(
      [.. _values], new Dictionary<string, int>(_ids, StringComparer.Ordinal)
    );
  }
}

public sealed class HixStringDictionary<T> : IDictionary<string, T>, IReadOnlyDictionary<string, T> {
  private readonly HixStringPool _pool;
  private readonly Dictionary<HixString, T> _values = new();

  private HixStringDictionary(HixStringPool pool) {
    _pool = pool ?? throw new ArgumentNullException(nameof(pool));
  }

  internal HixStringDictionary(IEnumerable<KeyValuePair<string, T>> values, HixStringPool pool)
    : this(pool) {
    foreach (var item in values) this[item.Key] = item.Value;
  }

  public T this[string key] {
    get => _values[Key(key)];
    set => _values[Key(key)] = value;
  }
  public ICollection<string> Keys => [.. _values.Keys.Select(key => key.Resolve(_pool))];
  public ICollection<T> Values => _values.Values;
  public int Count => _values.Count;
  public bool IsReadOnly => false;

  public void Add(string key, T value) {
    _values.Add(Key(key), value);
  }

  public bool ContainsKey(string key) {
    return _values.ContainsKey(Key(key));
  }

  public bool Remove(string key) {
    return _values.Remove(Key(key));
  }

  public bool TryGetValue(string key, out T value) {
    return _values.TryGetValue(Key(key), out value);
  }

  public void Add(KeyValuePair<string, T> item) {
    Add(item.Key, item.Value);
  }

  public void Clear() {
    _values.Clear();
  }

  public bool Contains(KeyValuePair<string, T> item) {
    return TryGetValue(item.Key, out var value) && EqualityComparer<T>.Default.Equals(value, item.Value);
  }

  public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) {
    foreach (var item in this) array[arrayIndex++] = item;
  }

  public bool Remove(KeyValuePair<string, T> item) {
    return Contains(item) && Remove(item.Key);
  }

  public IEnumerator<KeyValuePair<string, T>> GetEnumerator() {
    return _values.Select(item =>
      new KeyValuePair<string, T>(item.Key.Resolve(_pool), item.Value)
    ).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

  IEnumerable<string> IReadOnlyDictionary<string, T>.Keys => Keys;
  IEnumerable<T> IReadOnlyDictionary<string, T>.Values => Values;

  private HixString Key(string key) {
    return _pool.Get(key);
  }
}