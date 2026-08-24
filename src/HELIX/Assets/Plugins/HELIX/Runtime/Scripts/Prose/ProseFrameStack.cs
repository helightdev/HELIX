using System;
using Unity.Collections;

namespace HELIX.Prose {
  /// <summary>
  /// Array-backed stack that initializes frame structs in place and retains their reusable storage between uses.
  /// Popped frames are values owned by the caller until returned through <see cref="Release"/>. Active and pooled
  /// frames use separate storage, so finalization may reenter the stack safely.
  /// </summary>
  public sealed class ProseFrameStack<TFrame> where TFrame : struct, IProseFrame {
    private TFrame[] _items;
    private TFrame[] _pooled;
    private int _pooledCount;

    public ProseFrameStack(int initialCapacity = 16) {
      if (initialCapacity < 1) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
      _items = new TFrame[initialCapacity];
      _pooled = new TFrame[initialCapacity];
    }

    public int Count { get; private set; }
    public ref TFrame Current => ref this[Count - 1];
    public ref TFrame this[int index] {
      get {
        if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
        return ref _items[index];
      }
    }

    public ref TFrame Push() {
      if (Count == _items.Length) Array.Resize(ref _items, _items.Length * 2);
      var frame = TakePooled();
      frame.Reset();
      try {
        frame.Initialize();
      } catch {
        ReturnPooled(ref frame);
        throw;
      }
      _items[Count] = frame;
      return ref _items[Count++];
    }

    public TFrame Pop() {
      if (Count == 0) throw new InvalidOperationException("The Prose frame stack is empty.");
      var index = --Count;
      var frame = _items[index];
      _items[index] = default;
      return frame;
    }

    public void Release(ref TFrame frame) {
      try {
        frame.Dispose();
      } finally {
        ReturnPooled(ref frame);
      }
    }

    public void Clear() {
      while (Count > 0) {
        var frame = Pop();
        Release(ref frame);
      }
    }

    private TFrame TakePooled() {
      if (_pooledCount == 0) return default;
      var index = --_pooledCount;
      var frame = _pooled[index];
      _pooled[index] = default;
      return frame;
    }

    private void ReturnPooled(ref TFrame frame) {
      try {
        frame.Reset();
      } catch {
        frame = default;
        throw;
      }
      if (_pooledCount == _pooled.Length) Array.Resize(ref _pooled, _pooled.Length * 2);
      _pooled[_pooledCount++] = frame;
      frame = default;
    }
  }

  /// <summary>Lifecycle implemented by reusable, struct-based Prose frames.</summary>
  public interface IProseFrame : IDisposable {
    void Initialize();
    void Reset();
  }
}