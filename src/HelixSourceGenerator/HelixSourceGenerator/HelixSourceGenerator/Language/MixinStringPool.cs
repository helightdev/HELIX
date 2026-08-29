using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace HELIX.SourceGen.Expressions;

public readonly struct MixinString : IEquatable<MixinString> {
  private MixinString(int id, string dynamicValue) {
    Id = id;
    DynamicValue = dynamicValue;
  }

  public int Id { get; }
  public string DynamicValue { get; }
  public bool IsInterned => Id >= 0;

  internal static MixinString Interned(int id) => new(id, null);
  public static MixinString Dynamic(string value) => new(-1, value);
  public string Resolve(MixinStringPool pool) => IsInterned ? pool[Id] : DynamicValue;

  public bool Equals(MixinString other) => IsInterned == other.IsInterned &&
    (IsInterned ? Id == other.Id : string.Equals(
      DynamicValue, other.DynamicValue, StringComparison.Ordinal
    ));
  public override bool Equals(object value) => value is MixinString other && Equals(other);
  public override int GetHashCode() => IsInterned
    ? Id
    : StringComparer.Ordinal.GetHashCode(DynamicValue ?? "");
  public static bool operator ==(MixinString left, MixinString right) => left.Equals(right);
  public static bool operator !=(MixinString left, MixinString right) => !left.Equals(right);
}

public sealed class MixinStringPool {
  private readonly string[] _values;
  private readonly IReadOnlyDictionary<string, int> _ids;

  internal MixinStringPool(string[] values, IReadOnlyDictionary<string, int> ids) {
    _values = values ?? Array.Empty<string>();
    _ids = ids ?? new Dictionary<string, int>(StringComparer.Ordinal);
  }

  public int Count => _values.Length;
  public string this[int id] => _values[id];
  public bool TryGetId(string value, out int id) => _ids.TryGetValue(value ?? "", out id);
  public MixinString Get(string value) => TryGetId(value, out var id)
    ? MixinString.Interned(id)
    : MixinString.Dynamic(value);
}

internal sealed class MixinStringPoolBuilder {
  private readonly Dictionary<string, int> _ids = new(StringComparer.Ordinal);
  private readonly List<string> _values = new();

  internal MixinString Intern(string value) {
    value ??= "";
    if (_ids.TryGetValue(value, out var id)) return MixinString.Interned(id);
    id = _values.Count;
    _values.Add(value);
    _ids.Add(value, id);
    return MixinString.Interned(id);
  }

  internal MixinStringPool Freeze() => new(
    _values.ToArray(), new Dictionary<string, int>(_ids, StringComparer.Ordinal)
  );
}

internal sealed class MixinStringDictionary<T> : IDictionary<string, T>, IReadOnlyDictionary<string, T> {
  private readonly Dictionary<MixinString, T> _values = new();
  private readonly MixinStringPool _pool;

  internal MixinStringDictionary(MixinStringPool pool) {
    _pool = pool ?? throw new ArgumentNullException(nameof(pool));
  }

  internal MixinStringDictionary(IEnumerable<KeyValuePair<string, T>> values, MixinStringPool pool)
    : this(pool) {
    foreach (var item in values) this[item.Key] = item.Value;
  }

  private MixinString Key(string key) => _pool.Get(key);
  public T this[string key] { get => _values[Key(key)]; set => _values[Key(key)] = value; }
  public ICollection<string> Keys => _values.Keys.Select(key => key.Resolve(_pool)).ToArray();
  IEnumerable<string> IReadOnlyDictionary<string, T>.Keys => Keys;
  public ICollection<T> Values => _values.Values;
  IEnumerable<T> IReadOnlyDictionary<string, T>.Values => Values;
  public int Count => _values.Count;
  public bool IsReadOnly => false;
  public void Add(string key, T value) => _values.Add(Key(key), value);
  public bool ContainsKey(string key) => _values.ContainsKey(Key(key));
  public bool Remove(string key) => _values.Remove(Key(key));
  public bool TryGetValue(string key, out T value) => _values.TryGetValue(Key(key), out value);
  public void Add(KeyValuePair<string, T> item) => Add(item.Key, item.Value);
  public void Clear() => _values.Clear();
  public bool Contains(KeyValuePair<string, T> item) =>
    TryGetValue(item.Key, out var value) && EqualityComparer<T>.Default.Equals(value, item.Value);
  public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) {
    foreach (var item in this) array[arrayIndex++] = item;
  }
  public bool Remove(KeyValuePair<string, T> item) => Contains(item) && Remove(item.Key);
  public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _values.Select(item =>
    new KeyValuePair<string, T>(item.Key.Resolve(_pool), item.Value)
  ).GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
