using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HELIX.Widgets.Utilities {
  public class FlatPseudoMap<TKey, TValue> : IDictionary<TKey, TValue> {
    private readonly List<KeyValuePair<TKey, TValue>> _entries;
    private readonly EqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;

    public FlatPseudoMap() {
      _entries = new List<KeyValuePair<TKey, TValue>>();
    }

    public FlatPseudoMap(EqualityComparer<TKey> comparer) : this() {
      _comparer = comparer;
    }

    public FlatPseudoMap(int capacity, EqualityComparer<TKey> comparer = null) {
      _entries = new List<KeyValuePair<TKey, TValue>>(capacity);
      _comparer = comparer ?? EqualityComparer<TKey>.Default;
    }

    public TValue this[TKey state] {
      get {
        for (var i = _entries.Count - 1; i >= 0; i--) {
          var entry = _entries[i];
          if (_comparer.Equals(entry.Key, state)) return entry.Value;
        }
        return default;
      }
      set => _entries.Add(new KeyValuePair<TKey, TValue>(state, value));
    }

    public KeyValuePair<TKey, TValue> this[int index] {
      get => _entries[index];
      set => _entries[index] = value;
    }

    public ICollection<TKey> Keys {
      get {
        var keys = new List<TKey>(_entries.Count);
        keys.AddRange(_entries.Select(entry => entry.Key));
        return keys;
      }
    }

    public ICollection<TValue> Values {
      get {
        var values = new List<TValue>(_entries.Count);
        values.AddRange(_entries.Select(entry => entry.Value));
        return values;
      }
    }

    public int Count => _entries.Count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value) => _entries.Add(new KeyValuePair<TKey, TValue>(key, value));

    public bool Remove(TKey key) => _entries.RemoveAll(e => _comparer.Equals(e.Key, key)) > 0;

    public bool ContainsKey(TKey key) => _entries.Exists(e => _comparer.Equals(e.Key, key));

    public bool TryGetValue(TKey key, out TValue value) {
      for (var i = _entries.Count - 1; i >= 0; i--) {
        var entry = _entries[i];
        if (!_comparer.Equals(entry.Key, key)) continue;
        value = entry.Value;
        return true;
      }
      value = default;
      return false;
    }

    public void Clear() => _entries.Clear();

    public void Add(KeyValuePair<TKey, TValue> item) {
      Add(item.Key, item.Value);
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
      return _entries.Any(entry =>
        _comparer.Equals(entry.Key, item.Key) && EqualityComparer<TValue>.Default.Equals(entry.Value, item.Value)
      );
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
      return _entries.RemoveAll(entry =>
        _comparer.Equals(entry.Key, item.Key) && EqualityComparer<TValue>.Default.Equals(entry.Value, item.Value)
      ) > 0;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      for (var i = 0; i < _entries.Count; i++) {
        array[arrayIndex + i] = _entries[i];
      }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      foreach (var entry in _entries) {
        yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}