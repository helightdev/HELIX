using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HelixSourceGenerator.Language.Compiler;

public readonly struct MixinString : IEquatable<MixinString> {
  private readonly bool _initialized;

  private MixinString(int id, string dynamicValue) {
    Id = id;
    DynamicValue = dynamicValue;
    _initialized = true;
  }

  public int Id { get; }
  public string DynamicValue { get; }
  public bool IsInterned => _initialized && Id >= 0;

  internal static MixinString Interned(int id) {
    return new MixinString(id, null);
  }

  public static MixinString Dynamic(string value) {
    return new MixinString(-1, value);
  }

  public string Resolve(MixinStringPool pool) {
    return IsInterned ? pool[Id] : DynamicValue;
  }

  public bool Equals(MixinString other) {
    return IsInterned == other.IsInterned && (IsInterned
      ? Id == other.Id
      : string.Equals(DynamicValue, other.DynamicValue, StringComparison.Ordinal));
  }

  public override bool Equals(object value) {
    return value is MixinString other && Equals(other);
  }

  public override int GetHashCode() {
    return IsInterned ? Id : StringComparer.Ordinal.GetHashCode(DynamicValue ?? "");
  }

  public static bool operator ==(MixinString left, MixinString right) {
    return left.Equals(right);
  }

  public static bool operator !=(MixinString left, MixinString right) {
    return !left.Equals(right);
  }
}

public sealed class MixinStringPool {
  private readonly int _baseCount;
  private readonly Dictionary<string, int> _ids;
  private readonly MixinStringPool _parent;
  private readonly List<string> _values;

  internal MixinStringPool(string[] values, IReadOnlyDictionary<string, int> ids) {
    _baseCount = 0;
    _values = [.. values ?? []];
    _ids = new Dictionary<string, int>(StringComparer.Ordinal);
    if (ids is not null)
      foreach (var item in ids)
        _ids.Add(item.Key, item.Value);
  }

  private MixinStringPool(MixinStringPool parent) {
    _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    _baseCount = parent.Count;
    _values = [];
    _ids = new Dictionary<string, int>(StringComparer.Ordinal);
  }

  public int Count => _baseCount + _values.Count;
  public string this[int id] => id < _baseCount ? _parent[id] : _values[id - _baseCount];

  public bool TryGetId(string value, out int id) {
    value ??= "";
    return _ids.TryGetValue(value, out id) || (_parent is not null && _parent.TryGetId(value, out id));
  }

  public MixinString Get(string value) {
    return Intern(value);
  }

  public MixinString Intern(string value) {
    value ??= "";
    if (TryGetId(value, out var id)) return MixinString.Interned(id);
    id = Count;
    _values.Add(value);
    _ids.Add(value, id);
    return MixinString.Interned(id);
  }

  internal MixinStringPool Fork() {
    return new MixinStringPool(this);
  }
}

public sealed class MixinStringPoolBuilder {
  private readonly Dictionary<string, int> _ids = new(StringComparer.Ordinal);
  private readonly List<string> _values = [];

  public MixinString Intern(string value) {
    value ??= "";
    if (_ids.TryGetValue(value, out var id)) return MixinString.Interned(id);
    id = _values.Count;
    _values.Add(value);
    _ids.Add(value, id);
    return MixinString.Interned(id);
  }

  public MixinStringPool Freeze() {
    return new MixinStringPool(
      [.. _values], new Dictionary<string, int>(_ids, StringComparer.Ordinal)
    );
  }
}

public sealed class MixinStringDictionary<T> : IDictionary<string, T>, IReadOnlyDictionary<string, T> {
  private readonly MixinStringPool _pool;
  private readonly Dictionary<MixinString, T> _values = new();

  private MixinStringDictionary(MixinStringPool pool) {
    _pool = pool ?? throw new ArgumentNullException(nameof(pool));
  }

  internal MixinStringDictionary(IEnumerable<KeyValuePair<string, T>> values, MixinStringPool pool)
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

  private MixinString Key(string key) {
    return _pool.Get(key);
  }
}