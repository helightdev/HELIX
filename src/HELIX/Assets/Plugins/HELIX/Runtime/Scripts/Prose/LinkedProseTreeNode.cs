using System;
using System.Collections;
using System.Collections.Generic;

namespace HELIX.Prose {
  public abstract class LinkedProseTreeNode<T> : IReadOnlyList<T> where T : LinkedProseTreeNode<T> {
    public T parent;
    public T nextSibling;
    public T firstChild;
    public T lastChild;
    public int childCount;

    public int Count => childCount;

    public T this[int index] {
      get {
        if ((uint)index >= (uint)childCount) throw new ArgumentOutOfRangeException(nameof(index));
        var child = firstChild;
        while (index-- > 0) child = child.nextSibling;
        return child;
      }
    }

    public void Add(T child) {
      if (child == null) throw new ArgumentNullException(nameof(child));
      if (child.parent != null) throw new InvalidOperationException("The child node already has a parent.");
      for (var ancestor = (T)this; ancestor != null; ancestor = ancestor.parent) {
        if (ancestor == child) throw new InvalidOperationException("Adding the child would create a cycle.");
      }
      child.parent = (T)this;
      if (firstChild == null) firstChild = child;
      else lastChild.nextSibling = child;
      lastChild = child;
      childCount++;
    }

    public void Remove(T child) {
      if (child == null) throw new ArgumentNullException(nameof(child));
      if (child.parent != this) throw new InvalidOperationException("The child node is not a child of this node.");
      if (firstChild == child) {
        firstChild = child.nextSibling;
        if (lastChild == child) lastChild = null;
      }
      else {
        var prev = firstChild;
        while (prev != null && prev.nextSibling != child) prev = prev.nextSibling;
        if (prev == null) throw new InvalidOperationException("The child node is not a child of this node.");
        prev.nextSibling = child.nextSibling;
        if (lastChild == child) lastChild = prev;
      }
      child.parent = null;
      child.nextSibling = null;
      childCount--;
    }
    
    public Enumerator GetEnumerator() => new(firstChild);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T> {

      private readonly T _firstChild;
      private T _next;
      public T Current { get; private set; }

      internal Enumerator(T firstChild) {
        _firstChild = firstChild;
        _next = firstChild;
        Current = null;
      }

      public bool MoveNext() {
        if (_next == null) {
          Current = null;
          return false;
        }
        Current = _next;
        _next = Current.nextSibling;
        return true;
      }

      public void Reset() {
        Current = null;
        _next = _firstChild;
      }
      object IEnumerator.Current => Current;


      public void Dispose() { }
    }
  }
}
