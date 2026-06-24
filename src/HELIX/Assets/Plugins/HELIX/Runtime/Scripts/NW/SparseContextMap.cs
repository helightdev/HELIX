using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace HELIX.NW {
  public class SparseContextMap : List<SparseContextMap.Entry> {
    private const int _defaultCapacity = 3;

    private static readonly IEqualityComparer<Type> _defaultComparer = new ReferenceEqualityComparer<Type>();

    public static SparseContextMap Get() {
      return CollectionPool<SparseContextMap, Entry>.Get();
    }

    public static void Release(SparseContextMap contextMap) {
      contextMap.Clear();
      CollectionPool<SparseContextMap, Entry>.Release(contextMap);
    }

    public SparseContextMap() : base(_defaultCapacity){ }

    public SparseContextMap(int capacity) : base(capacity) { }

    public IContextData this[Type state] {
      get {
        for (var i = Count - 1; i >= 0; i--) {
          var entry = this[i];
          if (_defaultComparer.Equals(entry.key, state)) return entry.value;
        }
        throw new KeyNotFoundException();
      }
      set => Add(state, value);
    }

    public void Add(Type key, IContextData value) {
      var newEntry = new Entry(key, value, true);
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (!_defaultComparer.Equals(entry.key, key)) continue;
        this[i] = newEntry;
        return;
      }
      base.Add(newEntry);
    }

    public bool Remove(Type key) {
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (!_defaultComparer.Equals(entry.key, key)) continue;
        RemoveAt(i);
        return true;
      }
      return false;
    }

    public bool ContainsKey(Type key) => TryGetValue(key, out _);

    public bool TryGetValue(Type key, out IContextData value) {
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (!_defaultComparer.Equals(entry.key, key)) continue;
        value = entry.value;
        return true;
      }
      value = default;
      return false;
    }

    public void LoadInto(Dictionary<Type, IContextData> context) {
      for (var i = 0; i < Count; i++) {
        var entry = this[i];
        context.TryAdd(entry.key, entry.value);
      }
    }

    public void ResetMarkers() {
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        entry.marked = false;
        this[i] = entry;
      }
    }

    public void Prune() {
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (entry.marked) continue;
        RemoveAt(i);
      }
    }

    public struct Entry {
      public readonly Type key;
      public readonly IContextData value;
      public bool marked;

      public Entry(Type key, IContextData value, bool marked) {
        this.key = key;
        this.value = value;
        this.marked = marked;
      }
    }
  }
}