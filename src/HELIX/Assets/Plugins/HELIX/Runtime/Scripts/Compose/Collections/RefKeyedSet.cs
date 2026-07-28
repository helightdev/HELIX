using System;
using System.Runtime.CompilerServices;

namespace HELIX.Compose.Collections {
  public struct RefKeyedSet<T, TKey>
    where T : struct, IRefKeyed<TKey>
    where TKey : unmanaged, IEquatable<TKey> {
    private T[] _items;
    private int _count;
    private const int _initialCapacity = 5;

    public RefKeyedSet(int capacity) {
      _items = capacity > 0 ? new T[capacity] : null;
      _count = 0;
    }

    public int Count {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _count;
    }

    public bool IsEmpty {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _count == 0;
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
      for (var i = 0; i < _count; i++) {
        if (items[i].Key.Equals(key)) return i;
      }
      return -1;
    }

    public bool Contains(TKey key) => IndexOf(key) >= 0;

    public int FindIndex(TKey key) => IndexOf(key);

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
      EnsureCapacity(_count + 1);
      _items[_count] = default;
      return _count++;
    }

    public void AddUnchecked(in T item) {
      EnsureCapacity(_count + 1);
      _items[_count++] = item;
    }

    public bool RemoveSwapBack(TKey key) {
      var i = IndexOf(key);
      if (i < 0) return false;
      RemoveAtSwapBack(i);
      return true;
    }

    public void RemoveAtSwapBack(int index) {
      var last = --_count;
      if (index != last) _items[index] = _items[last];
      _items[last] = default;
    }

    public void Clear() {
      if (_items != null) Array.Clear(_items, 0, _count);
      _count = 0;
    }

    public void EnsureCapacity(int min) {
      if (_items == null) {
        _items = new T[Math.Max(_initialCapacity, min)];
      } else if (_items.Length < min) {
        var newCap = _items.Length * 2;
        if (newCap < min) newCap = min;
        var newArr = new T[newCap];
        Array.Copy(_items, newArr, _count);
        _items = newArr;
      }
    }
  }

  public interface IRefKeyed<out TKey> where TKey : unmanaged, IEquatable<TKey> {
    TKey Key { get; }
  }
}