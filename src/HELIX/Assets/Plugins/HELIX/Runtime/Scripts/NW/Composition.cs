using System;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace HELIX.NW {
  [AttributeUsage(AttributeTargets.Method)]
  public class CompositionAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Method)]
  public class CompositionBoundaryAttribute : Attribute {
    public Type Base { get; set; }
  }

  [AttributeUsage(AttributeTargets.Parameter)]
  public class PropAttribute : Attribute { }

  public delegate void Composable(ref Composition ctx);

  public delegate void InlineComposable(ref Composition ctx);

  public delegate void InlineComposable<in T>(ref Composition ctx, T value);

  public delegate void ScopeCompletionCallback(BoundaryCell cell, in ScopeHandle handle);

  public ref struct Composition {
    public readonly IBoundary boundary;
    public ElementRef APPLY;
    public CompositionAuthoring AUTHORING;

    public Composition(IBoundary initiator) : this() {
      boundary = initiator;

      AUTHORING.cell = initiator.Cell ?? BoundaryCell.Shared;
      AUTHORING.cell.cursor = 0;
      AUTHORING.cell.key = 0;
      AUTHORING.cell.current = initiator;
      AUTHORING.cursor = initiator.Element;

      APPLY.element = boundary.Element;
      APPLY.composable = boundary;
    }

    public bool Conditional(bool condition, ushort count = 1) {
      if (condition) return true;
      AUTHORING.id.localIndex += count;
      return false;
    }

    public KeyScope Key(ushort key) {
      return new KeyScope(AUTHORING.cell, key);
    }

    public T ReadContext<T>() where T : class, IContextData {
      var key = typeof(T);
      RecompositionScope.TryGetContext(key, out var read);
      return read as T;
    }

    public ContextScope<T> WriteContext<T>() where T : class, IContextData, new() {
      var key = typeof(T);
      var context = boundary.AcquireContext();
      context.TryGetValue(key, out var read);
      var state = read as T ?? new T();

      var existed = RecompositionScope.TryGetContext(key, out var previous);
      context[key] = state;
      RecompositionScope.PutContext(key, state);
      return new ContextScope<T>(key, previous, existed, state);
    }

    public T ReadWrittenContext<T>() where T : class, IContextData {
      var key = typeof(T);
      var context = boundary.AcquireContext();
      context.TryGetValue(key, out var read);
      return read as T;
    }

    public ContextScope<T> WriteContext<T>(T value) where T : class, IContextData {
      var key = typeof(T);
      var context = boundary.AcquireContext();
      var existed = RecompositionScope.TryGetContext(key, out var previous);
      context[key] = value;
      RecompositionScope.PutContext(key, value);
      return new ContextScope<T>(key, previous, existed, value);
    }
  }

  public readonly struct ContextScope<T> : IDisposable where T : class, IContextData {
    private readonly Type _key;
    private readonly IContextData _previous;
    private readonly bool _existed;
    public readonly T value;

    public ContextScope(Type key, IContextData previous, bool existed, T value) {
      this.value = value;
      _key = key;
      _previous = previous;
      _existed = existed;
    }

    public void Dispose() {
      RecompositionScope.PutPrevious(_key, _previous, _existed);
    }

    public static implicit operator T(ContextScope<T> scope) => scope.value;
  }

  public static class CompositionInternals {
    public static void EnterComposition(ref Composition cx, ushort composition, ref TransferData transfer) {
      transfer.compositionId = cx.AUTHORING.id.composition;
      cx.AUTHORING.id.composition = composition;
    }

    public static void EnterLocalComposition(
      ref Composition cx,
      ushort composition,
      ref LocalTransferData transfer
    ) {
      transfer.localIndex = (ushort)(cx.AUTHORING.id.localIndex + 1);
      transfer.compositionId = cx.AUTHORING.id.composition;
      cx.AUTHORING.id.localIndex = 0;
      cx.AUTHORING.id.composition = composition;
    }

    public static void ExitComposition(ref Composition cx, ref TransferData transfer) {
      cx.AUTHORING.id.composition = transfer.compositionId;
    }

    public static void ExitLocalComposition(ref Composition cx, ref LocalTransferData transfer) {
      cx.AUTHORING.id.localIndex = transfer.localIndex;
      cx.AUTHORING.id.composition = transfer.compositionId;
    }

    public ref struct TransferData {
      public ushort compositionId;
    }

    public ref struct LocalTransferData {
      public ushort localIndex;
      public ushort compositionId;
    }
  }

  [StructLayout(LayoutKind.Explicit, Size = 8)]
  public struct CompositionId {
    private static ushort _compositionIdCounter = 1;
    private static ushort _typeIdCounter = 1;

    [FieldOffset(0)]
    public ushort localIndex;
    [FieldOffset(2)]
    public ushort key;
    [FieldOffset(4)]
    public ushort composition;
    [FieldOffset(6)]
    public ushort type;
    [FieldOffset(0)]
    public ulong packed;

    public static ushort GetCompositionId() {
      if (_compositionIdCounter == ushort.MaxValue) {
        throw new InvalidOperationException("CompositionId counter overflow!");
      }
      return _compositionIdCounter++;
    }

    public static ushort GetTypeId() {
      if (_typeIdCounter == ushort.MaxValue) {
        throw new InvalidOperationException("TypeId counter overflow!");
      }
      return _typeIdCounter++;
    }
  }

  public ref struct ElementRef {
    public VisualElement element;
    public IComposable composable;

    public bool IsValid => element != null;
  }

  public readonly struct ScopeHandle : IDisposable {
    private readonly BoundaryCell _cell;
    private readonly IComposable _return;
    private readonly int _cursor;
    private readonly ScopeCompletionCallback _callback;

    private ScopeHandle(BoundaryCell cell, ScopeCompletionCallback callback) {
      _cell = cell;
      _return = cell.current;
      _callback = callback;
      _cursor = cell.cursor;
      cell.cursor = 0;
    }

    public void Dispose() {
      try {
        _callback?.Invoke(_cell, this);
      } finally {
        _cell.current = _return;
        _cell.cursor = _cursor;
      }
    }

    internal static ScopeHandle Push(BoundaryCell cell, IComposable next, ScopeCompletionCallback callback) {
      var scope = new ScopeHandle(cell, callback);
      cell.current = next;
      return scope;
    }
  }

  public readonly struct KeyScope : IDisposable {
    private readonly BoundaryCell _cell;
    private readonly ushort _previous;

    public KeyScope(BoundaryCell cell, ushort key) {
      _cell = cell;
      _previous = cell.key;
      cell.key = key;
    }

    public void Dispose() {
      _cell.key = _previous;
    }
  }
}