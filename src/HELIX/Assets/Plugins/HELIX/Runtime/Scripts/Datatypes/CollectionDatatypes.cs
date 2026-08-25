using System;
using System.Collections.Generic;
using HELIX.Prose;
using HELIX.Serialization;
using UnityEngine.Pool;

namespace HELIX {
  /// <summary>Shared introspection, prose and serialization behavior for mutable collection-like values.</summary>
  public class CollectionDatatype<TCollection, TItem, TAccumulator> :
    ICollectionDatatype<TCollection>, ISerializableDatatype<TCollection> where TCollection : class {
    public CollectionDatatype(
      ICollectionProxy<TCollection, TItem, TAccumulator> collectionProxy,
      IDatatype<TItem> itemDatatype = null,
      string nullText = ProseLiterals.Null,
      string emptyText = "[]",
      string prefix = "[",
      string separator = ", ",
      string suffix = "]"
    ) {
      CollectionProxy = collectionProxy ?? throw new ArgumentNullException(nameof(collectionProxy));
      ItemDatatype = itemDatatype ?? Datatypes.Object<TItem>();
      NullText = nullText;
      EmptyText = emptyText;
      Prefix = prefix;
      Separator = separator;
      Suffix = suffix;
    }

    public IDatatype<TItem> ItemDatatype { get; set; }
    object ICollectionDatatype.ItemDatatype => ItemDatatype;
    public ICollectionProxy<TCollection, TItem, TAccumulator> CollectionProxy { get; }
    ICollectionProxy ICollectionDatatype.CollectionProxy => CollectionProxy;
    public string NullText { get; set; }
    public string EmptyText { get; set; }
    public string Prefix { get; set; }
    public string Separator { get; set; }
    public string Suffix { get; set; }
    public int ComponentCount => 0;
    public string GetComponentName(int index) => $"[{index}]";
    public Type GetComponentType(int index) => typeof(TItem);
    public object GetComponentDatatype(int index) => ItemDatatype;
    public object GetComponentValue(object value, int index) => CollectionProxy.GetItem((TCollection)value, index);
    public object SetComponentValue(object value, int index, object componentValue) =>
      CollectionProxy.SetItem((TCollection)value, index, (TItem)componentValue);
    public void WriteComponent(IProseWriter writer, object value, int index) =>
      writer.Write(CollectionProxy.GetItem((TCollection)value, index), ItemDatatype);

    public void ToProse(IProseWriter writer, TCollection collection) {
      if (collection == null) { writer.Write(NullText); return; }
      var count = CollectionProxy.GetItemCount(collection);
      if (count == 0 && EmptyText != null) { writer.Write(EmptyText); return; }
      if (Prefix != null) writer.Write(Prefix);
      for (var i = 0; i < count; i++) {
        if (i != 0 && Separator != null) writer.Write(Separator);
        writer.Write(CollectionProxy.GetItem(collection, i), ItemDatatype);
      }
      if (Suffix != null) writer.Write(Suffix);
    }

    public bool TryRead(IUniversalReader reader, string name, out TCollection value) {
      value = null;
      if (!reader.TryReadNull(name, out var isNull)) return false;
      if (isNull) return true;
      if (ItemDatatype is not ISerializableDatatype<TItem> serializable) throw NotSerializable();
      if (!reader.TryEnterArray(name, out var length)) return false;
      var accumulator = CollectionProxy.AcquireAccumulator(Math.Max(0, length));
      try {
        if (length >= 0) {
          for (var i = 0; i < length; i++) {
            if (!serializable.TryRead(reader, null, out var item)) return false;
            CollectionProxy.Add(ref accumulator, item);
          }
        } else {
          while (true) {
            if (!reader.TryReadArrayEnd(out var isEnd)) return false;
            if (isEnd) break;
            if (!serializable.TryRead(reader, null, out var item)) return false;
            CollectionProxy.Add(ref accumulator, item);
          }
        }
        if (!reader.TryExitArray()) return false;
        value = CollectionProxy.Create(ref accumulator);
        return true;
      } finally {
        CollectionProxy.ReleaseAccumulator(ref accumulator);
      }
    }

    public bool TryWrite(IUniversalWriter writer, string name, TCollection value) {
      var isNull = value == null;
      if (!writer.TryWriteNull(name, isNull)) return false;
      if (isNull) return true;
      if (ItemDatatype is not ISerializableDatatype<TItem> serializable) throw NotSerializable();
      var count = CollectionProxy.GetItemCount(value);
      if (!writer.TryBeginArray(name, count)) return false;
      for (var i = 0; i < count; i++)
        if (!serializable.TryWrite(writer, null, CollectionProxy.GetItem(value, i))) return false;
      return writer.TryEndArray();
    }

    private NotSupportedException NotSerializable() => new(
      $"Collection item datatype '{ItemDatatype.GetType().Name}' does not support serialization. " +
      $"Implement {nameof(ISerializableDatatype<TItem>)} on the item datatype."
    );
  }

  public sealed class ListDatatype<T> : CollectionDatatype<List<T>, T, List<T>> {
    public ListDatatype(IDatatype<T> itemDatatype = null) : base(ListCollectionProxy<T>.Instance, itemDatatype) { }
  }

  public sealed class ArrayDatatype<T> : CollectionDatatype<T[], T, List<T>> {
    public ArrayDatatype(IDatatype<T> itemDatatype = null) : base(ArrayCollectionProxy<T>.Instance, itemDatatype) { }
  }

  public sealed class ListCollectionProxy<T> : ICollectionProxy<List<T>, T, List<T>> {
    public static readonly ListCollectionProxy<T> Instance = new();
    private ListCollectionProxy() { }
    public Type ItemType => typeof(T);
    public int GetItemCount(List<T> collection) => collection.Count;
    public T GetItem(List<T> collection, int index) => collection[index];
    public List<T> SetItem(List<T> collection, int index, T item) { collection[index] = item; return collection; }
    public List<T> AddItem(List<T> collection, T item) { collection.Add(item); return collection; }
    public List<T> RemoveItem(List<T> collection, int index) { collection.RemoveAt(index); return collection; }
    public List<T> AcquireAccumulator(int capacity) {
      var accumulator = ListPool<T>.Get();
      if (accumulator.Capacity < capacity) accumulator.Capacity = capacity;
      return accumulator;
    }
    public void Add(ref List<T> accumulator, T item) => accumulator.Add(item);
    public List<T> Create(ref List<T> accumulator) => new(accumulator);
    public void ReleaseAccumulator(ref List<T> accumulator) {
      ListPool<T>.Release(accumulator);
      accumulator = null;
    }
    int ICollectionProxy.GetItemCount(object collection) => GetItemCount((List<T>)collection);
    object ICollectionProxy.GetItem(object collection, int index) => GetItem((List<T>)collection, index);
    object ICollectionProxy.SetItem(object collection, int index, object item) =>
      SetItem((List<T>)collection, index, (T)item);
    object ICollectionProxy.AddItem(object collection, object item) => AddItem((List<T>)collection, (T)item);
    object ICollectionProxy.RemoveItem(object collection, int index) => RemoveItem((List<T>)collection, index);
  }

  public sealed class ArrayCollectionProxy<T> : ICollectionProxy<T[], T, List<T>> {
    public static readonly ArrayCollectionProxy<T> Instance = new();
    private ArrayCollectionProxy() { }
    public Type ItemType => typeof(T);
    public int GetItemCount(T[] collection) => collection.Length;
    public T GetItem(T[] collection, int index) => collection[index];
    public T[] SetItem(T[] collection, int index, T item) { collection[index] = item; return collection; }
    public T[] AddItem(T[] collection, T item) {
      var result = new T[collection.Length + 1];
      Array.Copy(collection, result, collection.Length);
      result[^1] = item;
      return result;
    }
    public T[] RemoveItem(T[] collection, int index) {
      var result = new T[collection.Length - 1];
      if (index > 0) Array.Copy(collection, 0, result, 0, index);
      if (index < result.Length) Array.Copy(collection, index + 1, result, index, result.Length - index);
      return result;
    }
    public List<T> AcquireAccumulator(int capacity) {
      var accumulator = ListPool<T>.Get();
      if (accumulator.Capacity < capacity) accumulator.Capacity = capacity;
      return accumulator;
    }
    public void Add(ref List<T> accumulator, T item) => accumulator.Add(item);
    public T[] Create(ref List<T> accumulator) => accumulator.ToArray();
    public void ReleaseAccumulator(ref List<T> accumulator) {
      ListPool<T>.Release(accumulator);
      accumulator = null;
    }
    int ICollectionProxy.GetItemCount(object collection) => GetItemCount((T[])collection);
    object ICollectionProxy.GetItem(object collection, int index) => GetItem((T[])collection, index);
    object ICollectionProxy.SetItem(object collection, int index, object item) =>
      SetItem((T[])collection, index, (T)item);
    object ICollectionProxy.AddItem(object collection, object item) => AddItem((T[])collection, (T)item);
    object ICollectionProxy.RemoveItem(object collection, int index) => RemoveItem((T[])collection, index);
  }

  /// <summary>Adapts arbitrary enumerable values, materializing mutations as a new list.</summary>
  public sealed class EnumerableCollectionProxy<T> : ICollectionProxy<IEnumerable<T>, T, List<T>> {
    public static readonly EnumerableCollectionProxy<T> Instance = new();
    private EnumerableCollectionProxy() { }
    public Type ItemType => typeof(T);
    public int GetItemCount(IEnumerable<T> collection) {
      if (collection is ICollection<T> counted) return counted.Count;
      if (collection is IReadOnlyCollection<T> readOnly) return readOnly.Count;
      var count = 0;
      using var enumerator = collection.GetEnumerator();
      while (enumerator.MoveNext()) count++;
      return count;
    }
    public T GetItem(IEnumerable<T> collection, int index) {
      if (collection is IList<T> list) return list[index];
      if (collection is IReadOnlyList<T> readOnly) return readOnly[index];
      using var enumerator = collection.GetEnumerator();
      for (var i = 0; i <= index; i++)
        if (!enumerator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(index));
      return enumerator.Current;
    }
    public IEnumerable<T> SetItem(IEnumerable<T> collection, int index, T item) {
      var result = new List<T>(collection);
      result[index] = item;
      return result;
    }
    public IEnumerable<T> AddItem(IEnumerable<T> collection, T item) {
      var result = new List<T>(collection) { item };
      return result;
    }
    public IEnumerable<T> RemoveItem(IEnumerable<T> collection, int index) {
      var result = new List<T>(collection);
      result.RemoveAt(index);
      return result;
    }
    public List<T> AcquireAccumulator(int capacity) {
      var accumulator = ListPool<T>.Get();
      if (accumulator.Capacity < capacity) accumulator.Capacity = capacity;
      return accumulator;
    }
    public void Add(ref List<T> accumulator, T item) => accumulator.Add(item);
    public IEnumerable<T> Create(ref List<T> accumulator) => new List<T>(accumulator);
    public void ReleaseAccumulator(ref List<T> accumulator) {
      ListPool<T>.Release(accumulator);
      accumulator = null;
    }
    int ICollectionProxy.GetItemCount(object collection) => GetItemCount((IEnumerable<T>)collection);
    object ICollectionProxy.GetItem(object collection, int index) => GetItem((IEnumerable<T>)collection, index);
    object ICollectionProxy.SetItem(object collection, int index, object item) =>
      SetItem((IEnumerable<T>)collection, index, (T)item);
    object ICollectionProxy.AddItem(object collection, object item) => AddItem((IEnumerable<T>)collection, (T)item);
    object ICollectionProxy.RemoveItem(object collection, int index) => RemoveItem((IEnumerable<T>)collection, index);
  }
}
