using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HELIX.Widgets.Utilities {
  public readonly struct SimpleClosedHashMap<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IEquatable<TKey> {
    private struct Bucket {
      public TKey key;
      public TValue value;
      public bool occupied;
    }

    private readonly Bucket[] _buckets;
    private readonly int _mask;

    public SimpleClosedHashMap(int capacity) {
      if ((capacity & (capacity - 1)) != 0) {
        capacity = 1 << Mathf.CeilToInt(Mathf.Log(capacity, 2));
      }

      _buckets = new Bucket[capacity];
      _mask = capacity - 1;
    }

    public int Count {
      get {
        var count = 0;
        for (var index = 0; index < _buckets.Length; index++) {
          var bucket = _buckets[index];
          if (bucket.occupied) count++;
        }
        return count;
      }
    }

    public bool IsReadOnly => false;

    public TValue this[TKey key] {
      get => TryGetValue(key, out var value) ? value : default;
      set => TryAdd(key, value);
    }


    public void Add(TKey key, TValue value) {
      TryAdd(key, value);
    }
    public bool ContainsKey(TKey key) {
      return TryGetValue(key, out _);
    }
    public bool Remove(TKey key) {
      throw new NotImplementedException("Removal is not supported in this implementation.");
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
      throw new NotImplementedException("Removal is not supported in this implementation.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetValue(TKey key, out TValue value) {
      var hash = (uint)key.GetHashCode();
      var index = (int)(hash & _mask);

      while (_buckets[index].occupied) {
        if (_buckets[index].key.Equals(key)) {
          value = _buckets[index].value;
          return true;
        }
        index = (index + 1) & _mask;
      }

      value = default;
      return false;
    }


    public ICollection<TKey> Keys {
      get {
        var keys = new List<TKey>(_buckets.Length);
        foreach (var bucket in _buckets) {
          if (bucket.occupied) keys.Add(bucket.key);
        }
        return keys;
      }
    }

    public ICollection<TValue> Values {
      get {
        var values = new List<TValue>(_buckets.Length);
        foreach (var bucket in _buckets) {
          if (bucket.occupied) values.Add(bucket.value);
        }
        return values;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(TKey key, TValue value) {
      var hash = (uint)key.GetHashCode();
      var index = (int)(hash & _mask);

      while (_buckets[index].occupied) {
        if (_buckets[index].key.Equals(key)) {
          return false;
        }
        index = (index + 1) & _mask;
      }

      _buckets[index] = new Bucket {
        key = key,
        value = value,
        occupied = true
      };
      return true;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      foreach (var bucket in _buckets) {
        if (bucket.occupied) {
          yield return new KeyValuePair<TKey, TValue>(bucket.key, bucket.value);
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item) {
      TryAdd(item.Key, item.Value);
    }

    public void Clear() {
      for (var i = 0; i < _buckets.Length; i++) {
        _buckets[i] = default;
      }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
      return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      for (var i = 0; i < _buckets.Length; i++) {
        var bucket = _buckets[i];
        if (!bucket.occupied) continue;
        array[arrayIndex++] = new KeyValuePair<TKey, TValue>(bucket.key, bucket.value);
      }
    }
  }
}