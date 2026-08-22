using System;
using System.Collections.Generic;

namespace HELIX.Prose {
  /// <summary>Maps leaf writes to an intermediate type and reduces completed frames into that same type.</summary>
  public abstract class ProseReducer<TReduced> {
    public virtual bool AcceptFrame(IProseScope scope) => true;

    public abstract bool TryMap(
      IProse prose, IReadOnlyList<IProseModifier> modifiers, out TReduced result
    );

    public abstract bool TryMap(
      string text, IReadOnlyList<IProseModifier> modifiers, out TReduced result
    );

    public abstract bool TryMap<T>(
      T value, IProseFormatter<T> formatter, IReadOnlyList<IProseModifier> modifiers,
      out TReduced result
    );

    public virtual TReduced Reduce(
      IProseScope scope, IReadOnlyList<TReduced> children, IReadOnlyList<IProseModifier> modifiers
    ) => Reduce(children);

    public abstract TReduced Reduce(IReadOnlyList<TReduced> children);
    public virtual bool IsEmpty(TReduced value) => value is null;
  }

  public interface IProseScopeHandler<TReduced> {
    bool TryCreate(IProseScope scope, out IProseWriter writer);
    TReduced Finish(IProseWriter writer);
  }

  public sealed class ProseScopeDelegates<TReduced> {
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

    public ProseScopeDelegates<TReduced> Add(IProseScopeHandler<TReduced> handler) {
      if (handler == null) throw new ArgumentNullException(nameof(handler));
      _handlers.Add(handler);
      return this;
    }

    public ProseScopeDelegates<TReduced> Delegate<TScope, TWriter>(
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
      ProseScopeDelegates<TReduced> delegates = null
    ) {
      Reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
      Delegates = delegates ?? new ProseScopeDelegates<TReduced>();
    }

    public ProseReducer<TReduced> Reducer { get; }
    public ProseScopeDelegates<TReduced> Delegates { get; }
    public int FrameCount => _frames.Count;
    protected bool HasActiveDelegation => _delegated.Count != 0;

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

    public override bool TryBeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      BeforeBeginFrame(scope);
      if (_delegated.Count > 0) {
        var current = _delegated[^1];
        if (!current.writer.TryBeginFrame(scope)) return false;
        _delegated.Add(new DelegatedFrame { writer = current.writer, handler = current.handler });
        return true;
      }
      if (IsInactive) return false;
      if (Delegates.TryCreate(scope, out var handler, out var writer)) {
        if (ReferenceEquals(writer, this))
          throw new InvalidOperationException("A Prose writer cannot delegate a scope to itself.");
        if (!writer.TryBeginFrame(scope)) return false;
        for (var i = 0; i < _frames.Count; i++)
        for (var j = 0; j < _frames[i].modifiers.Count; j++)
          writer.PushModifier(_frames[i].modifiers[j]);
        _delegated.Add(new DelegatedFrame { writer = writer, handler = handler, root = true });
        return true;
      }
      if (!Reducer.AcceptFrame(scope)) return false;
      ref var frame = ref _frames.Push();
      frame.scope = scope;
      frame.active = true;
      return true;
    }

    public override void BeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      BeforeBeginFrame(scope);
      if (_delegated.Count > 0) {
        var current = _delegated[^1];
        current.writer.BeginFrame(scope);
        _delegated.Add(new DelegatedFrame { writer = current.writer, handler = current.handler });
        return;
      }
      if (!IsInactive && Delegates.TryCreate(scope, out var handler, out var writer)) {
        if (ReferenceEquals(writer, this))
          throw new InvalidOperationException("A Prose writer cannot delegate a scope to itself.");
        writer.BeginFrame(scope);
        for (var i = 0; i < _frames.Count; i++)
        for (var j = 0; j < _frames[i].modifiers.Count; j++)
          writer.PushModifier(_frames[i].modifiers[j]);
        _delegated.Add(new DelegatedFrame { writer = writer, handler = handler, root = true });
        return;
      }
      if (!IsInactive && Reducer.AcceptFrame(scope)) {
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
        if (delegated.root) AddReduced(delegated.handler.Finish(delegated.writer));
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
      AddReduced(result);
    }

    public override void PushModifier(IProseModifier modifier) {
      if (modifier == null) throw new ArgumentNullException(nameof(modifier));
      if (_delegated.Count > 0) {
        _delegated[^1].writer.PushModifier(modifier);
        return;
      }
      PushModifierDirect(modifier);
    }

    protected virtual void PushModifierDirect(IProseModifier modifier) {
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
      if (Reducer.TryMap(prose, _writeModifiers, out var result)) AddReduced(result);
    }

    public override void Write<T>(T value, IProseFormatter<T> formatter) {
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      if (_delegated.Count > 0) {
        _delegated[^1].writer.Write(value, formatter);
        return;
      }
      if (IsInactive) return;
      CollectWriteModifiers();
      if (Reducer.TryMap(value, formatter, _writeModifiers, out var result)) AddReduced(result);
    }

    public override void Write(string text) {
      if (_delegated.Count > 0) {
        _delegated[^1].writer.Write(text);
        return;
      }
      if (text == null || IsInactive) return;
      CollectWriteModifiers();
      if (Reducer.TryMap(text, _writeModifiers, out var result)) AddReduced(result);
    }

    protected virtual void AddReduced(TReduced value) {
      if (Reducer.IsEmpty(value)) return;
      if (_frames.Count == 0) _root.Add(value);
      else _frames.Current.children.Add(value);
    }

    private bool IsInactive => _frames.Count > 0 && !_frames.Current.active;
    protected virtual void BeforeBeginFrame(IProseScope scope) { }

    private void CollectWriteModifiers() {
      _writeModifiers.Clear();
      for (var i = 0; i < _frames.Count; i++) _writeModifiers.AddRange(_frames[i].modifiers);
    }
  }
}