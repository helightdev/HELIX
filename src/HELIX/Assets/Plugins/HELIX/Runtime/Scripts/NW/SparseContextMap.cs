using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Pool;

namespace HELIX.NW {
  public class SparseContextMap {
    private const int _defaultCapacity = 3;
    private static readonly ObjectPool<SparseContextMap> _pool = new(
      () => new SparseContextMap(),
      null,
      static (obj) => obj.Clear(),
      null,
      false,
      10,
      256
    );

    public readonly List<Publication> publications;
    public readonly List<Publication> subscriptions;

    public RefArrayList<Publication> publicationRefs;
    public RefArrayList<Publication> subscriptionRefs;

    public static SparseContextMap Get() {
      return _pool.Get();
    }

    public static void Release(SparseContextMap contextMap) {
      _pool.Release(contextMap);
      var r = new RefArrayList<Publication>();

    }


    public SparseContextMap(int capacity) {
      publications = new List<Publication>(capacity);
      subscriptions = new List<Publication>(capacity);
      publicationRefs = new RefArrayList<Publication>(capacity);
      subscriptionRefs = new RefArrayList<Publication>(capacity);
    }

    public SparseContextMap() : this(_defaultCapacity) { }

    public int PublicationCount => publications.Count;

    public void Clear() {
      publications.Clear();
      subscriptions.Clear();
    }

    public void Put(int key, ContextData value) {
      var newEntry = new Publication(key, value, true);
      for (var i = publications.Count - 1; i >= 0; i--) {
        var entry = publications[i];
        if (entry.key != key) continue;
        publications[i] = newEntry;
        return;
      }
      publications.Add(newEntry);
    }

    public bool Remove(int key) {
      for (var i = publications.Count - 1; i >= 0; i--) {
        var entry = publications[i];
        if (entry.key != key) continue;
        publications.RemoveAt(i);
        return true;
      }
      return false;
    }

    public bool TryGet(int key, out ContextData value) {
      for (var i = publications.Count - 1; i >= 0; i--) {
        var entry = publications[i];
        if (key != entry.key) continue;
        value = entry.value;
        return true;
      }
      value = null;
      return false;
    }

    public bool TryGet<T>(ContextKey<T> key, out ContextData<T> value) {
      for (var i = publications.Count - 1; i >= 0; i--) {
        var entry = publications[i];
        if (key.id != entry.key) continue;
        value = entry.value as ContextData<T>;
        return true;
      }
      value = null;
      return false;
    }

    public void LoadInto(Dictionary<int, ContextData> context) {
      for (var i = 0; i < publications.Count; i++) {
        var entry = publications[i];
        context.TryAdd(entry.key, entry.value);
      }
    }

    public void ResetMarkers() {
      for (var i = publications.Count - 1; i >= 0; i--) {
        var entry = publications[i];
        entry.marked = false;
        publications[i] = entry;
      }
    }

    public void PrunePublications() {
      for (var i = publications.Count - 1; i >= 0; i--) {
        var entry = publications[i];
        if (entry.marked) continue;
        entry.value.Dispose();
        publications.RemoveAt(i);
      }
    }

    public struct Publication {
      public readonly int key;
      public readonly ContextData value;
      public bool marked;

      public Publication(int key, ContextData value, bool marked) {
        this.key = key;
        this.value = value;
        this.marked = marked;
      }
    }

    public struct Subscription {
      public int id;
      public ContextData source;
      public ContextVersion version;
    }
  }

  public struct RefArrayList<T> where T : struct {
    private T[] _items;
    private int _count;

    public RefArrayList(int capacity = 4) {
      if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
      _items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
      _count = 0;
    }

    public int Count => _count;

    public int Capacity => _items.Length;

    public ref T this[int index] {
      get {
        if ((uint)index >= (uint)_count)
          throw new ArgumentOutOfRangeException(nameof(index));

        return ref _items[index];
      }
    }

    public void Add(T item) {
      if (_count == _items.Length) Grow(_count + 1);
      _items[_count++] = item;
    }

    public ref T AddDefault() {
      if (_count == _items.Length)
        Grow(_count + 1);

      _items[_count] = default;
      return ref _items[_count++];
    }

    public void RemoveAt(int index) {
      if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));

      var moveCount = _count - index - 1;

      if (moveCount > 0) Array.Copy(_items, index + 1, _items, index, moveCount);
      _count--;
      _items[_count] = default;
    }

    public void Clear() {
      Array.Clear(_items, 0, _count);
      _count = 0;
    }

    public void EnsureCapacity(int capacity) {
      if (capacity < 0)
        throw new ArgumentOutOfRangeException(nameof(capacity));

      if (_items.Length < capacity)
        Grow(capacity);
    }

    public Span<T> AsSpan() {
      return _items.AsSpan(0, _count);
    }

    public Enumerator GetEnumerator() {
      return new Enumerator(this);
    }

    private void Grow(int minimumCapacity) {
      int newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;

      if (newCapacity < minimumCapacity)
        newCapacity = minimumCapacity;

      Array.Resize(ref _items, newCapacity);
    }

    public ref struct Enumerator {
      private readonly Span<T> _span;
      private int _index;

      internal Enumerator(RefArrayList<T> list) {
        _span = list._items.AsSpan(0, list._count);
        _index = -1;
      }

      public bool MoveNext() {
        var next = _index + 1;

        if ((uint)next >= (uint)_span.Length) return false;

        _index = next;
        return true;
      }

      public ref T Current => ref _span[_index];
    }
  }
}