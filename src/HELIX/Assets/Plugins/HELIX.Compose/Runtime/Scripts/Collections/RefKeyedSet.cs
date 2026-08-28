using System;
using System.Runtime.CompilerServices;

namespace HELIX.Compose {
  public struct RefKeyedSet<T, TKey>
  where T : struct, IRefKeyed<TKey>
  where TKey : unmanaged, IEquatable<TKey> {
    private T[] _items;
    private const int _initialCapacity = 5;

    public RefKeyedSet(int capacity) {
      _items = capacity > 0 ? new T[capacity] : null;
      Count = 0;
    }

    public int Count { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    public bool IsEmpty {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Count == 0;
    }

    public bool IsNull {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _items == null;
    }

    public int Capacity {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _items?.Length ?? 0;
    }

    public ref T this[int index] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ref _items[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int IndexOf(TKey key) {
      var items = _items;
      for (var i = 0; i < Count; i++)
        if (items[i].Key.Equals(key))
          return i;
      return -1;
    }

    public bool Contains(TKey key) {
      return IndexOf(key) >= 0;
    }

    public int FindIndex(TKey key) {
      return IndexOf(key);
    }

    public bool TryGetValue(TKey key, out T item) {
      var i = IndexOf(key);
      if (i >= 0) {
        item = _items[i];
        return true;
      }
      item = default;
      return false;
    }

    public bool TryAdd(in T item) {
      if (IndexOf(item.Key) >= 0) return false;
      AddUnchecked(in item);
      return true;
    }

    public int GetOrAddIndex(TKey key, out bool existed) {
      var i = IndexOf(key);
      if (i >= 0) {
        existed = true;
        return i;
      }
      existed = false;
      EnsureCapacity(Count + 1);
      _items[Count] = default;
      return Count++;
    }

    public void AddUnchecked(in T item) {
      EnsureCapacity(Count + 1);
      _items[Count++] = item;
    }

    public bool RemoveSwapBack(TKey key) {
      var i = IndexOf(key);
      if (i < 0) return false;
      RemoveAtSwapBack(i);
      return true;
    }

    public void RemoveAtSwapBack(int index) {
      var last = --Count;
      if (index != last) _items[index] = _items[last];
      _items[last] = default;
    }

    public void Clear() {
      if (_items != null) Array.Clear(_items, 0, Count);
      Count = 0;
    }

    public void EnsureCapacity(int min) {
      if (_items == null) _items = new T[Math.Max(_initialCapacity, min)];
      else if (_items.Length < min) {
        var newCap = _items.Length * 2;
        if (newCap < min) newCap = min;
        var newArr = new T[newCap];
        Array.Copy(_items, newArr, Count);
        _items = newArr;
      }
    }
  }

  public interface IRefKeyed<out TKey> where TKey : unmanaged, IEquatable<TKey> {
    TKey Key { get; }
  }
}