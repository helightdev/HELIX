using System;
using System.Collections.Generic;

namespace HELIX.Prose {
  /// <summary>Maps leaf writes to an intermediate type and reduces completed frames into that same type.</summary>
  public abstract class ProseReducer<TReduced> {
    public virtual bool Accepts(IProseScope scope) => true;

    public abstract bool TryMap(
      IProse prose, IReadOnlyList<IProseModifier> modifiers, out TReduced result
    );

    public abstract bool TryMap(
      string text, IReadOnlyList<IProseModifier> modifiers, out TReduced result
    );

    public abstract bool TryMap<T>(
      T value, IDatatype<T> datatype, IReadOnlyList<IProseModifier> modifiers,
      out TReduced result
    );

    public virtual TReduced Reduce(
      IProseScope scope, IReadOnlyList<TReduced> children, IReadOnlyList<IProseModifier> modifiers
    ) => Reduce(children);

    public abstract TReduced Reduce(IReadOnlyList<TReduced> children);
    public virtual bool IsEmpty(TReduced value) => value is null;
  }

  public interface IProseScopeHandler<out TReduced> {
    bool TryCreate(IProseScope scope, out IProseWriter writer);
    TReduced Finish(IProseWriter writer);
  }

  /// <summary>Reduces immediate writes and completed scopes to one common intermediate type.</summary>
  public class ReducingProseWriter<TReduced> : ProseWriter {
    private struct Frame : IProseFrame {
      public IProseScope scope;
      public bool active;
      public List<IProseModifier> modifiers;
      public List<TReduced> children;

      public void Initialize() {
        modifiers ??= new List<IProseModifier>();
        children ??= new List<TReduced>();
      }

      public void Dispose() { }

      public void Reset() {
        scope = null;
        active = false;
        modifiers?.Clear();
        children?.Clear();
      }
    }

    private readonly ProseFrameStack<Frame> _frames = new();
    private readonly List<TReduced> _root = new();
    private readonly List<IProseModifier> _writeModifiers = new();
    private readonly List<DelegatedFrame> _delegated = new();

    private struct DelegatedFrame {
      public IProseWriter writer;
      public IProseScopeHandler<TReduced> handler;
      public bool root;
    }

    public ReducingProseWriter(
      ProseReducer<TReduced> reducer,
      ProseReducerChain<TReduced> delegates = null
    ) {
      Reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
      Delegates = delegates ?? new ProseReducerChain<TReduced>();
    }

    public ProseReducer<TReduced> Reducer { get; }
    public ProseReducerChain<TReduced> Delegates { get; }
    public int FrameCount => _frames.Count;
    protected bool HasActiveDelegation => _delegated.Count != 0;

    private bool IsInactive => _frames.Count > 0 && !_frames.Current.active;

    public virtual TReduced Build() {
      if (_delegated.Count != 0)
        throw new InvalidOperationException("All delegated Prose frames must be ended before building the writer.");
      if (_frames.Count != 0)
        throw new InvalidOperationException("All Prose frames must be ended before building the writer.");
      return Reducer.Reduce(_root.ToArray());
    }

    public virtual void Reset() {
      if (_delegated.Count != 0)
        throw new InvalidOperationException("All delegated Prose frames must be ended before resetting the writer.");
      if (_frames.Count != 0)
        throw new InvalidOperationException("All Prose frames must be ended before resetting the writer.");
      _root.Clear();
      _writeModifiers.Clear();
    }

    public override bool TryBegin(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      BeforeBegin(scope);
      if (_delegated.Count > 0) {
        var current = _delegated[^1];
        if (!current.writer.TryBegin(scope)) return false;
        _delegated.Add(new DelegatedFrame { writer = current.writer, handler = current.handler });
        return true;
      }
      if (IsInactive) return false;
      if (Delegates.TryCreate(scope, out var handler, out var writer)) {
        if (ReferenceEquals(writer, this))
          throw new InvalidOperationException("A Prose writer cannot delegate a scope to itself.");
        if (!writer.TryBegin(scope)) return false;
        for (var i = 0; i < _frames.Count; i++)
        for (var j = 0; j < _frames[i].modifiers.Count; j++)
          writer.Push(_frames[i].modifiers[j]);
        _delegated.Add(new DelegatedFrame { writer = writer, handler = handler, root = true });
        return true;
      }
      if (!Reducer.Accepts(scope)) return false;
      ref var frame = ref _frames.Push();
      frame.scope = scope;
      frame.active = true;
      return true;
    }

    public override void Begin(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      BeforeBegin(scope);
      if (_delegated.Count > 0) {
        var current = _delegated[^1];
        current.writer.Begin(scope);
        _delegated.Add(new DelegatedFrame { writer = current.writer, handler = current.handler });
        return;
      }
      if (!IsInactive && Delegates.TryCreate(scope, out var handler, out var writer)) {
        if (ReferenceEquals(writer, this))
          throw new InvalidOperationException("A Prose writer cannot delegate a scope to itself.");
        writer.Begin(scope);
        for (var i = 0; i < _frames.Count; i++)
        for (var j = 0; j < _frames[i].modifiers.Count; j++)
          writer.Push(_frames[i].modifiers[j]);
        _delegated.Add(new DelegatedFrame { writer = writer, handler = handler, root = true });
        return;
      }
      if (!IsInactive && Reducer.Accepts(scope)) {
        ref var accepted = ref _frames.Push();
        accepted.scope = scope;
        accepted.active = true;
        return;
      }
      ref var frame = ref _frames.Push();
      frame.scope = scope;
    }

    public override void End() {
      if (_delegated.Count > 0) {
        var index = _delegated.Count - 1;
        var delegated = _delegated[index];
        _delegated.RemoveAt(index);
        delegated.writer.End();
        if (delegated.root) Accumulate(delegated.handler.Finish(delegated.writer));
        return;
      }
      if (_frames.Count == 0) throw new InvalidOperationException("There is no Prose frame to pop.");
      var frame = _frames.Pop();
      if (!frame.active) {
        _frames.Release(ref frame);
        return;
      }
      TReduced result;
      try {
        result = Reducer.Reduce(frame.scope, frame.children.ToArray(), frame.modifiers.ToArray());
      } finally {
        _frames.Release(ref frame);
      }
      Accumulate(result);
    }

    public override void Push(IProseModifier modifier) {
      if (modifier == null) throw new ArgumentNullException(nameof(modifier));
      if (_delegated.Count > 0) {
        _delegated[^1].writer.Push(modifier);
        return;
      }
      PushDirect(modifier);
    }

    protected virtual void PushDirect(IProseModifier modifier) {
      if (_frames.Count == 0) throw new InvalidOperationException("A modifier requires an active Prose frame.");
      if (!IsInactive) _frames.Current.modifiers.Add(modifier);
    }

    public override void Write(IProse prose) {
      if (_delegated.Count > 0) {
        _delegated[^1].writer.Write(prose);
        return;
      }
      if (IsInactive) return;
      CollectWriteModifiers();
      if (Reducer.TryMap(prose, _writeModifiers, out var result)) Accumulate(result);
    }

    public override void Write<T>(T value, IDatatype<T> datatype) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      if (_delegated.Count > 0) {
        _delegated[^1].writer.Write(value, datatype);
        return;
      }
      if (IsInactive) return;
      CollectWriteModifiers();
      if (Reducer.TryMap(value, datatype, _writeModifiers, out var result)) Accumulate(result);
    }

    public override void Write(string text) {
      if (_delegated.Count > 0) {
        _delegated[^1].writer.Write(text);
        return;
      }
      if (text == null || IsInactive) return;
      CollectWriteModifiers();
      if (Reducer.TryMap(text, _writeModifiers, out var result)) Accumulate(result);
    }

    protected virtual void Accumulate(TReduced value) {
      if (Reducer.IsEmpty(value)) return;
      if (_frames.Count == 0) _root.Add(value);
      else _frames.Current.children.Add(value);
    }

    protected virtual void BeforeBegin(IProseScope scope) { }

    private void CollectWriteModifiers() {
      _writeModifiers.Clear();
      for (var i = 0; i < _frames.Count; i++) _writeModifiers.AddRange(_frames[i].modifiers);
    }
  }

  public sealed class ProseReducerChain<TReduced> {
    private sealed class Handler<TScope, TWriter> : IProseScopeHandler<TReduced>
    where TScope : IProseScope where TWriter : class, IProseWriter {
      private readonly Func<TScope, TWriter> _create;
      private readonly Func<TWriter, TReduced> _finish;

      public Handler(Func<TScope, TWriter> create, Func<TWriter, TReduced> finish) {
        _create = create;
        _finish = finish;
      }

      public bool TryCreate(IProseScope scope, out IProseWriter writer) {
        if (scope is not TScope typed) {
          writer = null;
          return false;
        }
        writer = _create(typed) ??
          throw new InvalidOperationException("A delegated Prose writer factory returned null.");
        return true;
      }

      public TReduced Finish(IProseWriter writer) => _finish((TWriter)writer);
    }

    private readonly List<IProseScopeHandler<TReduced>> _handlers = new();

    public ProseReducerChain<TReduced> Add(IProseScopeHandler<TReduced> handler) {
      if (handler == null) throw new ArgumentNullException(nameof(handler));
      _handlers.Add(handler);
      return this;
    }

    public ProseReducerChain<TReduced> Delegate<TScope, TWriter>(
      Func<TScope, TWriter> create, Func<TWriter, TReduced> finish
    ) where TScope : IProseScope where TWriter : class, IProseWriter {
      if (create == null) throw new ArgumentNullException(nameof(create));
      if (finish == null) throw new ArgumentNullException(nameof(finish));
      return Add(new Handler<TScope, TWriter>(create, finish));
    }

    public void Clear() => _handlers.Clear();

    internal bool TryCreate(
      IProseScope scope, out IProseScopeHandler<TReduced> handler, out IProseWriter writer
    ) {
      for (var i = 0; i < _handlers.Count; i++) {
        handler = _handlers[i];
        if (handler.TryCreate(scope, out writer)) return true;
      }
      handler = null;
      writer = null;
      return false;
    }
  }
}