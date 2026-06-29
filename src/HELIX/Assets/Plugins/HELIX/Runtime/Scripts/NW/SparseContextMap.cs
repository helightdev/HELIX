using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace HELIX.NW {
  public class SparseContextMap : List<SparseContextMap.Entry> {
    private const int _defaultCapacity = 3;

    public static SparseContextMap Get() {
      return CollectionPool<SparseContextMap, Entry>.Get();
    }

    public static void Release(SparseContextMap contextMap) {
      contextMap.Clear();
      CollectionPool<SparseContextMap, Entry>.Release(contextMap);
    }

    public SparseContextMap() : base(_defaultCapacity){ }

    public SparseContextMap(int capacity) : base(capacity) { }

    public void Put(int key, ContextData value) {
      var newEntry = new Entry(key, value, true);
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (entry.key != key) continue;
        this[i] = newEntry;
        return;
      }
      base.Add(newEntry);
    }

    public bool Remove(int key) {
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (entry.key != key) continue;
        RemoveAt(i);
        return true;
      }
      return false;
    }

    public bool TryGet(int key, out ContextData value) {
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (key != entry.key) continue;
        value = entry.value;
        return true;
      }
      value = null;
      return false;
    }

    public bool TryGet<T>(ContextKey<T> key, out ContextData<T> value) {
      for (var i = Count - 1; i >= 0; i--) {
        var entry = this[i];
        if (key.id != entry.key) continue;
        value = entry.value as ContextData<T>;
        return true;
      }
      value = null;
      return false;
    }

    public void LoadInto(Dictionary<int, ContextData> context) {
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
      public readonly int key;
      public readonly ContextData value;
      public bool marked;

      public Entry(int key, ContextData value, bool marked) {
        this.key = key;
        this.value = value;
        this.marked = marked;
      }
    }
  }
}