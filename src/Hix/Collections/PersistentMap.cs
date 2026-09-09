using System;
using System.Collections;
using System.Collections.Generic;

namespace Hix.Collections;

/// <summary>Immutable unordered map: shared flat key shapes for small maps, bitmap HAMT for large maps.</summary>
public sealed class PersistentMap<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> {
  public const int DefaultThreshold = 16;
  private readonly IEqualityComparer<TKey> comparer;
  private readonly int threshold;
  private readonly TKey[] keys;
  private readonly TValue[] values;
  private readonly Node root;
  public int Count { get; }
  public bool IsFlat => root == null;

  public PersistentMap(IEqualityComparer<TKey> comparer = null, int threshold = DefaultThreshold) : this(
    comparer ?? EqualityComparer<TKey>.Default, threshold,
    Array.Empty<TKey>(), Array.Empty<TValue>(), null, 0
  ) {
    if (threshold < 0) throw new ArgumentOutOfRangeException(nameof(threshold));
  }

  private PersistentMap(
    IEqualityComparer<TKey> comparer, int threshold, TKey[] keys, TValue[] values, Node root, int count
  ) {
    this.comparer = comparer;
    this.threshold = threshold;
    this.keys = keys;
    this.values = values;
    this.root = root;
    Count = count;
  }

  public TValue this[TKey key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

  public IEnumerable<TKey> Keys {
    get {
      foreach (var entry in this) yield return entry.Key;
    }
  }

  public IEnumerable<TValue> Values {
    get {
      foreach (var entry in this) yield return entry.Value;
    }
  }

  public bool ContainsKey(TKey key) => TryGetValue(key, out _);

  public bool TryGetValue(TKey key, out TValue value) {
    if (key is null) throw new ArgumentNullException(nameof(key));
    if (root != null) return root.Find(key, unchecked((uint)comparer.GetHashCode(key)), 0, comparer, out value);
    for (var i = 0; i < Count; i++)
      if (comparer.Equals(keys[i], key)) {
        value = values[i];
        return true;
      }
    value = default;
    return false;
  }

  public PersistentMap<TKey, TValue> SetItem(TKey key, TValue value) {
    if (key is null) throw new ArgumentNullException(nameof(key));
    if (root != null) {
      var updated = root.Set(key, value, unchecked((uint)comparer.GetHashCode(key)), 0, comparer, out var added);
      return ReferenceEquals(updated, root)
        ? this
        : new(comparer, threshold, null, null, updated, Count + (added ? 1 : 0));
    }
    for (var i = 0; i < Count; i++)
      if (comparer.Equals(keys[i], key)) {
        if (EqualityComparer<TValue>.Default.Equals(values[i], value)) return this;
        var updated = (TValue[])values.Clone();
        updated[i] = value;
        return new(comparer, threshold, keys, updated, null, Count);
      }
    if (Count < threshold)
      return new(comparer, threshold, Insert(keys, Count, key), Insert(values, Count, value), null, Count + 1);
    Node tree = new Leaf(unchecked((uint)comparer.GetHashCode(key)), new[] { key }, new[] { value });
    for (var i = 0; i < Count; i++)
      tree = tree.Set(keys[i], values[i], unchecked((uint)comparer.GetHashCode(keys[i])), 0, comparer, out _);
    return new(comparer, threshold, null, null, tree, Count + 1);
  }

  public PersistentMap<TKey, TValue> Remove(TKey key) {
    if (key is null) throw new ArgumentNullException(nameof(key));
    if (root != null) {
      var updated = root.Remove(key, unchecked((uint)comparer.GetHashCode(key)), 0, comparer, out var removed);
      if (!removed) return this;
      if (updated == null) return new(comparer, threshold);
      // Keep nonempty HAMTs as HAMTs to avoid churn around the threshold.
      return new(comparer, threshold, null, null, updated, Count - 1);
    }
    for (var i = 0; i < Count; i++)
      if (comparer.Equals(keys[i], key))
        return new(comparer, threshold, Delete(keys, i), Delete(values, i), null, Count - 1);
    return this;
  }

  public Enumerator GetEnumerator() => new(this);
  IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  /// <summary>Traversal allocates at most one small pair of stacks, not an iterator per node.</summary>
  public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
    private readonly PersistentMap<TKey, TValue> map;
    private readonly Node[] nodes;
    private readonly int[] positions;
    private int depth;
    private int flatIndex;
    public KeyValuePair<TKey, TValue> Current { get; private set; }
    object IEnumerator.Current => Current;

    internal Enumerator(PersistentMap<TKey, TValue> map) {
      this.map = map;
      Current = default;
      depth = 0;
      flatIndex = 0;
      nodes = map.root == null ? null : new Node[8];
      positions = map.root == null ? null : new int[8];
      if (nodes != null) nodes[0] = map.root;
    }

    public bool MoveNext() {
      if (nodes == null) {
        if (flatIndex >= map.Count) return false;
        Current = new(map.keys[flatIndex], map.values[flatIndex]);
        flatIndex++;
        return true;
      }
      while (depth >= 0) {
        if (nodes[depth] is Leaf leaf) {
          if (positions[depth] < leaf.Count) {
            Current = leaf.Entry(positions[depth]++);
            return true;
          }
          depth--;
        } else {
          var branch = (Branch)nodes[depth];
          if (positions[depth] == branch.Count) {
            depth--;
            continue;
          }
          var child = branch.Child(positions[depth]++);
          depth++;
          nodes[depth] = child;
          positions[depth] = 0;
        }
      }
      return false;
    }

    public void Reset() => throw new NotSupportedException();
    public void Dispose() { }
  }

  /// <summary>Owns mutable construction state; publishing never exposes its dictionary.</summary>
  public sealed class Builder {
    private readonly Dictionary<TKey, TValue> items;
    private readonly int threshold;

    public Builder(IEqualityComparer<TKey> comparer = null, int threshold = DefaultThreshold) {
      if (threshold < 0) throw new ArgumentOutOfRangeException(nameof(threshold));
      items = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
      this.threshold = threshold;
    }

    public void SetItem(TKey key, TValue value) => items[key] = value;
    public bool Remove(TKey key) => items.Remove(key);

    public PersistentMap<TKey, TValue> ToImmutable() {
      if (items.Count <= threshold) {
        var keys = new TKey[items.Count];
        var values = new TValue[items.Count];
        var i = 0;
        foreach (var entry in items) {
          keys[i] = entry.Key;
          values[i++] = entry.Value;
        }
        return new(items.Comparer, threshold, keys, values, null, items.Count);
      }
      var entries = new HashedEntry[items.Count];
      var index = 0;
      foreach (var entry in items)
        entries[index++] = new(entry.Key, entry.Value, unchecked((uint)items.Comparer.GetHashCode(entry.Key)));
      return new(
        items.Comparer, threshold, null, null, Build(entries, new HashedEntry[entries.Length], 0, entries.Length, 0),
        items.Count
      );
    }
  }

  private readonly record struct HashedEntry(TKey Key, TValue Value, uint Hash);

  private static Node Build(HashedEntry[] entries, HashedEntry[] scratch, int start, int count, int shift) {
    var hash = entries[start].Hash;
    var sameHash = true;
    for (var i = start + 1; i < start + count; i++)
      if (entries[i].Hash != hash) {
        sameHash = false;
        break;
      }
    if (sameHash) {
      var keys = new TKey[count];
      var values = new TValue[count];
      for (var i = 0; i < count; i++) {
        keys[i] = entries[start + i].Key;
        values[i] = entries[start + i].Value;
      }
      return new Leaf(hash, keys, values);
    }
    var counts = new int[32];
    var positions = new int[32];
    for (var i = start; i < start + count; i++) counts[(entries[i].Hash >> shift) & 31]++;
    uint bitmap = 0;
    var childCount = 0;
    var offset = start;
    for (var i = 0; i < 32; i++) {
      positions[i] = offset;
      offset += counts[i];
      if (counts[i] != 0) {
        bitmap |= 1u << i;
        childCount++;
      }
    }
    for (var i = start; i < start + count; i++) scratch[positions[(entries[i].Hash >> shift) & 31]++] = entries[i];
    Array.Copy(scratch, start, entries, start, count);
    var children = new Node[childCount];
    offset = start;
    var childIndex = 0;
    for (var i = 0; i < 32; i++)
      if (counts[i] != 0) {
        children[childIndex++] = Build(entries, scratch, offset, counts[i], shift + 5);
        offset += counts[i];
      }
    return new Branch(bitmap, children);
  }

  private static T[] Insert<T>(T[] source, int index, T value) {
    var result = new T[source.Length + 1];
    Array.Copy(source, 0, result, 0, index);
    result[index] = value;
    Array.Copy(source, index, result, index + 1, source.Length - index);
    return result;
  }

  private static T[] Delete<T>(T[] source, int index) {
    var result = new T[source.Length - 1];
    Array.Copy(source, 0, result, 0, index);
    Array.Copy(source, index + 1, result, index, source.Length - index - 1);
    return result;
  }

  private static uint Bit(uint hash, int shift) => 1u << (int)((hash >> shift) & 31);

  private static int Slot(uint bitmap, uint bit) {
    var value = bitmap & (bit - 1);
    value -= (value >> 1) & 0x55555555u;
    value = (value & 0x33333333u) + ((value >> 2) & 0x33333333u);
    return unchecked((int)((((value + (value >> 4)) & 0x0F0F0F0Fu) * 0x01010101u) >> 24));
  }

  private abstract class Node {
    internal abstract bool Find(TKey key, uint hash, int shift, IEqualityComparer<TKey> comparer, out TValue value);

    internal abstract Node Set(
      TKey key, TValue value, uint hash, int shift, IEqualityComparer<TKey> comparer, out bool added
    );

    internal abstract Node Remove(TKey key, uint hash, int shift, IEqualityComparer<TKey> comparer, out bool removed);
  }

  // A leaf also represents a full-hash collision bucket.
  private sealed class Leaf(uint hash, TKey[] keys, TValue[] values) : Node {
    internal override bool Find(
      TKey key, uint requested, int shift, IEqualityComparer<TKey> comparer, out TValue value
    ) {
      if (hash == requested)
        for (var i = 0; i < keys.Length; i++)
          if (comparer.Equals(keys[i], key)) {
            value = values[i];
            return true;
          }
      value = default;
      return false;
    }

    internal override Node Set(
      TKey key, TValue value, uint requested, int shift, IEqualityComparer<TKey> comparer, out bool added
    ) {
      if (hash != requested) {
        added = true;
        return Merge(this, hash, new Leaf(requested, new[] { key }, new[] { value }), requested, shift);
      }
      for (var i = 0; i < keys.Length; i++)
        if (comparer.Equals(keys[i], key)) {
          added = false;
          if (EqualityComparer<TValue>.Default.Equals(values[i], value)) return this;
          var updated = (TValue[])values.Clone();
          updated[i] = value;
          return new Leaf(hash, keys, updated);
        }
      added = true;
      return new Leaf(hash, Insert(keys, keys.Length, key), Insert(values, values.Length, value));
    }

    internal override Node Remove(
      TKey key, uint requested, int shift, IEqualityComparer<TKey> comparer, out bool removed
    ) {
      if (hash == requested)
        for (var i = 0; i < keys.Length; i++)
          if (comparer.Equals(keys[i], key)) {
            removed = true;
            return keys.Length == 1 ? null : new Leaf(hash, Delete(keys, i), Delete(values, i));
          }
      removed = false;
      return this;
    }

    internal int Count => keys.Length;
    internal KeyValuePair<TKey, TValue> Entry(int index) => new(keys[index], values[index]);
  }

  private static Node Merge(Node left, uint leftHash, Node right, uint rightHash, int shift) {
    var a = Bit(leftHash, shift);
    var b = Bit(rightHash, shift);
    return a == b
      ? new Branch(a, new[] { Merge(left, leftHash, right, rightHash, shift + 5) })
      : new Branch(a | b, a < b ? new[] { left, right } : new[] { right, left });
  }

  private sealed class Branch(uint bitmap, Node[] children) : Node {
    internal override bool Find(TKey key, uint hash, int shift, IEqualityComparer<TKey> comparer, out TValue value) {
      var bit = Bit(hash, shift);
      if ((bitmap & bit) != 0) return children[Slot(bitmap, bit)].Find(key, hash, shift + 5, comparer, out value);
      value = default;
      return false;
    }

    internal override Node Set(
      TKey key, TValue value, uint hash, int shift, IEqualityComparer<TKey> comparer, out bool added
    ) {
      var bit = Bit(hash, shift);
      var index = Slot(bitmap, bit);
      if ((bitmap & bit) == 0) {
        added = true;
        return new Branch(bitmap | bit, Insert<Node>(children, index, new Leaf(hash, new[] { key }, new[] { value })));
      }
      var child = children[index].Set(key, value, hash, shift + 5, comparer, out added);
      if (ReferenceEquals(child, children[index])) return this;
      var updated = (Node[])children.Clone();
      updated[index] = child;
      return new Branch(bitmap, updated);
    }

    internal override Node Remove(TKey key, uint hash, int shift, IEqualityComparer<TKey> comparer, out bool removed) {
      var bit = Bit(hash, shift);
      var index = Slot(bitmap, bit);
      if ((bitmap & bit) == 0) {
        removed = false;
        return this;
      }
      var child = children[index].Remove(key, hash, shift + 5, comparer, out removed);
      if (!removed) return this;
      if (child == null) {
        if (children.Length == 1) return null;
        var remaining = Delete(children, index);
        return remaining.Length == 1 && remaining[0] is Leaf ? remaining[0] : new Branch(bitmap & ~bit, remaining);
      }
      if (children.Length == 1 && child is Leaf) return child;
      var updated = (Node[])children.Clone();
      updated[index] = child;
      return new Branch(bitmap, updated);
    }

    internal int Count => children.Length;
    internal Node Child(int index) => children[index];
  }
}