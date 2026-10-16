using System;
using System.Collections.Generic;

namespace HELIX.Prose {
  /// <summary>Lifecycle implemented by reusable, struct-based Prose frames.</summary>
  public interface IProseFrame : IDisposable {
    void Initialize(IProseScope scope);
    void Reset();
  }

  /// <summary>
  /// Array-backed stack that initializes frame structs in place and retains their reusable storage between uses.
  /// A popped frame remains leased until <see cref="Release"/> is called, allowing it to be inspected after the
  /// logical stack depth has decreased.
  /// </summary>
  public sealed class ProseFrameStack<TFrame> where TFrame : struct, IProseFrame {
    private TFrame[] _items;
    private int _leasedIndex = -1;

    public ProseFrameStack(int initialCapacity = 16) {
      if (initialCapacity < 1) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
      _items = new TFrame[initialCapacity];
    }

    public int Count { get; private set; }
    public ref TFrame Current => ref this[Count - 1];
    public ref TFrame this[int index] {
      get {
        if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
        return ref _items[index];
      }
    }

    public ref TFrame Push(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      EnsureReleased();
      if (Count == _items.Length) Array.Resize(ref _items, _items.Length * 2);
      ref var frame = ref _items[Count++];
      frame.Reset();
      frame.Initialize(scope);
      return ref frame;
    }

    public ref TFrame Pop() {
      EnsureReleased();
      if (Count == 0) throw new InvalidOperationException("The Prose frame stack is empty.");
      _leasedIndex = --Count;
      return ref _items[_leasedIndex];
    }

    public void Release() {
      if (_leasedIndex < 0) throw new InvalidOperationException("There is no popped Prose frame to release.");
      ref var frame = ref _items[_leasedIndex];
      frame.Dispose();
      frame.Reset();
      _leasedIndex = -1;
    }

    public void Clear() {
      EnsureReleased();
      for (var i = Count - 1; i >= 0; i--) {
        _items[i].Dispose();
        _items[i].Reset();
      }
      Count = 0;
    }

    private void EnsureReleased() {
      if (_leasedIndex >= 0)
        throw new InvalidOperationException("The previously popped Prose frame must be released first.");
    }
  }

  public delegate void ProseWriterDelegateCompletion(IProseWriter owner, IProseWriter delegated);

  public readonly struct ProseWriterDelegate {
    public ProseWriterDelegate(IProseWriter writer, ProseWriterDelegateCompletion completion = null) {
      Writer = writer ?? throw new ArgumentNullException(nameof(writer));
      Completion = completion;
    }

    public IProseWriter Writer { get; }
    public ProseWriterDelegateCompletion Completion { get; }
  }

  public interface IProseWriterDelegateProvider {
    bool TryCreate(IProseWriter owner, IProseScope scope, out ProseWriterDelegate delegation);
  }

  /// <summary>Ordered scope-to-writer routing shared by any composite Prose writer.</summary>
  public sealed class ProseWriterDelegates {
    private readonly List<IProseWriterDelegateProvider> _providers = new();

    public ProseWriterDelegates Add(IProseWriterDelegateProvider provider) {
      if (provider == null) throw new ArgumentNullException(nameof(provider));
      _providers.Add(provider);
      return this;
    }

    public ProseWriterDelegates Delegate<TScope>(
      Func<TScope, IProseWriter> writerFactory,
      ProseWriterDelegateCompletion completion = null
    ) where TScope : IProseScope {
      if (writerFactory == null) throw new ArgumentNullException(nameof(writerFactory));
      return Add(new TypedProseWriterDelegateProvider<TScope>(writerFactory, completion));
    }

    public bool Remove(IProseWriterDelegateProvider provider) => provider != null && _providers.Remove(provider);
    public void Clear() => _providers.Clear();

    internal bool TryCreate(IProseWriter owner, IProseScope scope, out ProseWriterDelegate delegation) {
      for (var i = 0; i < _providers.Count; i++)
        if (_providers[i].TryCreate(owner, scope, out delegation))
          return true;
      delegation = default;
      return false;
    }
  }

  internal sealed class TypedProseWriterDelegateProvider<TScope> : IProseWriterDelegateProvider
    where TScope : IProseScope {
    private readonly Func<TScope, IProseWriter> _writerFactory;
    private readonly ProseWriterDelegateCompletion _completion;

    public TypedProseWriterDelegateProvider(
      Func<TScope, IProseWriter> writerFactory, ProseWriterDelegateCompletion completion
    ) {
      _writerFactory = writerFactory;
      _completion = completion;
    }

    public bool TryCreate(IProseWriter owner, IProseScope scope, out ProseWriterDelegate delegation) {
      if (scope is not TScope typed) {
        delegation = default;
        return false;
      }
      var writer = _writerFactory(typed) ??
                   throw new InvalidOperationException("A Prose writer delegate factory returned null.");
      if (ReferenceEquals(owner, writer))
        throw new InvalidOperationException("A Prose writer cannot delegate a scope to itself.");
      delegation = new ProseWriterDelegate(writer, _completion);
      return true;
    }
  }

  /// <summary>
  /// Routes complete semantic subtrees to other writers. Destination writers receive ordinary balanced calls and
  /// remain unaware that they are participating in a composite writer.
  /// </summary>
  public abstract class DelegatingProseWriter : ProseWriter {
    private struct DelegatedFrame : IProseFrame {
      public IProseScope scope;
      public ProseWriterDelegate delegation;
      public bool root;

      public void Initialize(IProseScope value) => scope = value;
      public void Dispose() { }
      public void Reset() {
        scope = null;
        delegation = default;
        root = false;
      }
    }

    private readonly ProseFrameStack<DelegatedFrame> _delegatedFrames = new();

    protected DelegatingProseWriter(ProseWriterDelegates delegates = null) =>
      Delegates = delegates ?? new ProseWriterDelegates();

    public ProseWriterDelegates Delegates { get; }
    protected bool IsDelegating => _delegatedFrames.Count > 0;

    public sealed override bool TryBeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      OnBeforeBeginFrame(scope);
      if (IsDelegating) {
        var delegation = _delegatedFrames.Current.delegation;
        if (!delegation.Writer.TryBeginFrame(scope)) return false;
        ref var frame = ref _delegatedFrames.Push(scope);
        frame.delegation = delegation;
        return true;
      }
      if (Delegates.TryCreate(this, scope, out var created)) {
        if (!created.Writer.TryBeginFrame(scope)) return false;
        ref var frame = ref _delegatedFrames.Push(scope);
        frame.delegation = created;
        frame.root = true;
        return true;
      }
      return TryBeginFrameLocal(scope);
    }

    public sealed override void BeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      OnBeforeBeginFrame(scope);
      if (IsDelegating) {
        var delegation = _delegatedFrames.Current.delegation;
        delegation.Writer.BeginFrame(scope);
        ref var frame = ref _delegatedFrames.Push(scope);
        frame.delegation = delegation;
        return;
      }
      if (Delegates.TryCreate(this, scope, out var created)) {
        created.Writer.BeginFrame(scope);
        ref var frame = ref _delegatedFrames.Push(scope);
        frame.delegation = created;
        frame.root = true;
        return;
      }
      BeginFrameLocal(scope);
    }

    public sealed override void End() {
      if (!IsDelegating) {
        EndLocal();
        return;
      }
      ref var frame = ref _delegatedFrames.Pop();
      var delegation = frame.delegation;
      var root = frame.root;
      try {
        delegation.Writer.End();
      } finally {
        _delegatedFrames.Release();
      }
      if (root) delegation.Completion?.Invoke(this, delegation.Writer);
    }

    public sealed override void PushModifier(IProseModifier modifier) {
      if (modifier == null) throw new ArgumentNullException(nameof(modifier));
      if (IsDelegating) _delegatedFrames.Current.delegation.Writer.PushModifier(modifier);
      else PushModifierLocal(modifier);
    }

    public sealed override void Write(IProse prose) {
      if (IsDelegating) _delegatedFrames.Current.delegation.Writer.Write(prose);
      else WriteLocal(prose);
    }

    public sealed override void Write<T>(T value, IProseFormatter<T> formatter) {
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      if (IsDelegating) _delegatedFrames.Current.delegation.Writer.Write(value, formatter);
      else WriteLocal(value, formatter);
    }

    public sealed override void Write(string text) {
      if (IsDelegating) _delegatedFrames.Current.delegation.Writer.Write(text);
      else WriteLocal(text);
    }

    protected void ClearDelegatedFrames() => _delegatedFrames.Clear();
    protected virtual void OnBeforeBeginFrame(IProseScope scope) { }
    protected abstract bool TryBeginFrameLocal(IProseScope scope);
    protected abstract void BeginFrameLocal(IProseScope scope);
    protected abstract void EndLocal();
    protected abstract void PushModifierLocal(IProseModifier modifier);
    protected abstract void WriteLocal(IProse prose);
    protected abstract void WriteLocal<T>(T value, IProseFormatter<T> formatter);
    protected abstract void WriteLocal(string text);
  }
}
