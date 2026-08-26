namespace HELIX {
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public sealed class TableEntry<TLeft, TRight, TData> {
    public TLeft Left { get; }
    public TRight Right { get; }
    public TData Data { get; set; }

    public TableEntry(TLeft left, TRight right, TData data) {
      Left = left;
      Right = right;
      Data = data;
    }
  }

  public sealed class Table<TLeft, TRight, TData>
    : IEnumerable<TableEntry<TLeft, TRight, TData>> {
    private readonly Dictionary<TLeft, TableEntry<TLeft, TRight, TData>> _left;
    private readonly Dictionary<TRight, TableEntry<TLeft, TRight, TData>> _right;

    public int Count => _left.Count;

    public Table()
      : this(null, null) { }

    public Table(
      IEqualityComparer<TLeft> leftComparer,
      IEqualityComparer<TRight> rightComparer
    ) {
      _left = new Dictionary<TLeft, TableEntry<TLeft, TRight, TData>>(
        leftComparer
      );

      _right = new Dictionary<TRight, TableEntry<TLeft, TRight, TData>>(
        rightComparer
      );
    }

    public bool ContainsLeft(TLeft left) {
      return _left.ContainsKey(left);
    }

    public bool ContainsRight(TRight right) {
      return _right.ContainsKey(right);
    }

    public bool Contains(TLeft left, TRight right) {
      return _left.TryGetValue(left, out var leftEntry)
        && _right.TryGetValue(right, out var rightEntry)
        && ReferenceEquals(leftEntry, rightEntry);
    }

    public bool TryGetLeft(
      TLeft left,
      out TableEntry<TLeft, TRight, TData> entry
    ) {
      return _left.TryGetValue(left, out entry);
    }

    public bool TryGetRight(
      TRight right,
      out TableEntry<TLeft, TRight, TData> entry
    ) {
      return _right.TryGetValue(right, out entry);
    }

    public bool TryAdd(
      TLeft left,
      TRight right,
      TData data,
      out TableEntry<TLeft, TRight, TData> entry
    ) {
      if (_left.ContainsKey(left) || _right.ContainsKey(right)) {
        entry = null;
        return false;
      }

      entry = new TableEntry<TLeft, TRight, TData>(left, right, data);

      _left.Add(left, entry);
      _right.Add(right, entry);

      return true;
    }

    public bool TryRemoveLeft(
      TLeft left,
      out TableEntry<TLeft, TRight, TData> entry
    ) {
      if (!_left.Remove(left, out entry)) {
        entry = null;
        return false;
      }

      _right.Remove(entry.Right);

      return true;
    }

    public bool TryRemoveRight(
      TRight right,
      out TableEntry<TLeft, TRight, TData> entry
    ) {
      if (!_right.Remove(right, out entry)) {
        entry = null;
        return false;
      }

      _left.Remove(entry.Left);

      return true;
    }

    public bool Remove(TableEntry<TLeft, TRight, TData> entry) {
      if (entry == null) return false;

      if (!_left.TryGetValue(entry.Left, out var left)
        || !_right.TryGetValue(entry.Right, out var right)
        || !ReferenceEquals(entry, left)
        || !ReferenceEquals(entry, right)) {
        return false;
      }

      _left.Remove(entry.Left);
      _right.Remove(entry.Right);

      return true;
    }

    public void Clear() {
      _left.Clear();
      _right.Clear();
    }

    // Direct foreach resolves this method and keeps Enumerator unboxed.
    public Enumerator GetEnumerator() {
      return new Enumerator(_left.Values.GetEnumerator());
    }

    // Enumeration through these interfaces boxes the struct enumerator.
    IEnumerator<TableEntry<TLeft, TRight, TData>>
      IEnumerable<TableEntry<TLeft, TRight, TData>>.GetEnumerator() {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public struct Enumerator
      : IEnumerator<TableEntry<TLeft, TRight, TData>> {
      private Dictionary<
        TLeft,
        TableEntry<TLeft, TRight, TData>
      >.ValueCollection.Enumerator _enumerator;

      internal Enumerator(
        Dictionary<
          TLeft,
          TableEntry<TLeft, TRight, TData>
        >.ValueCollection.Enumerator enumerator
      ) {
        _enumerator = enumerator;
      }

      public TableEntry<TLeft, TRight, TData> Current => _enumerator.Current;

      object IEnumerator.Current => Current;

      public bool MoveNext() {
        return _enumerator.MoveNext();
      }

      public void Dispose() {
        _enumerator.Dispose();
      }

      void IEnumerator.Reset() {
        throw new NotSupportedException();
      }
    }
  }
}