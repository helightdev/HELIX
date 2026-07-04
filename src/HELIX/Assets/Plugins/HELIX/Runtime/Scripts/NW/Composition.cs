using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public delegate void ScopeCompletionCallback(BoundaryCell cell, in ScopeHandle handle);

  public ref struct Composition {
    public readonly IBoundary boundary;
    public ElementRef APPLY;
    public CompositionAuthoring AUTHORING;

    public Composition(IBoundary initiator) : this() {
      boundary = initiator;

      AUTHORING.cell = initiator.Cell ?? BoundaryCell.Shared;
      AUTHORING.cell.cursor = 0;
      AUTHORING.cell.localId = LocalId.Initial;
      AUTHORING.cell.current = initiator;
      AUTHORING.cursor = initiator.Element;

      APPLY.element = boundary.Element;
      APPLY.composable = boundary;
    }

    public bool Conditional(bool condition, ushort count = 1) {
      if (condition) return true;
      AUTHORING.cell.localId.index += count;
      return false;
    }

    // public KeyScope Key(ushort key) {
    //   return new KeyScope(AUTHORING.cell, key);
    // }

    public T ReadContext<T>(ContextKey<T> key) {
      if (RecompositionScope.TryGetContext(key, out var read)) return read.value;
      return default;
    }

    public T ReadContextOrDefault<T>(ContextKey<T> key, T defaultValue = default) {
      if (RecompositionScope.TryGetContext(key, out var read)) return read.value;
      return defaultValue;
    }

    public ContextReference<T> ReadContextReference<T>(ContextKey<T> key) {
      if (RecompositionScope.TryGetContext(key, out var read)) return new ContextReference<T>(key, read);
      return default;
    }

    public ContextData<T> GetWrittenContext<T>(ContextKey<T> key) {
      var context = boundary.AcquireContext();
      context.TryGet(key, out var data);
      return data;
    }

    public ContextScope<T> WriteContext<T>(ContextKey<T> key, T value) {
      var context = boundary.AcquireContext();
      context.TryGet(key, out var written);
      written ??= new ContextData<T>();
      written.Update(value);

      var existed = RecompositionScope.TryGetContext(key, out var previous);
      context.Put(key, written);
      RecompositionScope.PutContext(key, written);
      return new ContextScope<T>(key, previous, existed);
    }

    public ContextScope<T> WritableContext<T>(ContextKey<T> key, out ContextData<T> data) {
      var context = boundary.AcquireContext();
      context.TryGet(key, out data);
      data ??= new ContextData<T>();

      var existed = RecompositionScope.TryGetContext(key, out var previous);
      context.Put(key, data);
      RecompositionScope.PutContext(key, data);
      return new ContextScope<T>(key, previous, existed);
    }
  }

  public readonly struct ContextScope<T> : IDisposable {
    private readonly ContextKey<T> _key;
    private readonly ContextData _previous;
    private readonly bool _existed;

    public ContextScope(ContextKey<T> key, ContextData previous, bool existed) {
      _key = key;
      _previous = previous;
      _existed = existed;
    }

    public void Dispose() {
      RecompositionScope.PutPrevious(_key, _previous, _existed);
    }
  }

  public static class CompositionInternals {
    public static void EnterComposition(ref Composition cx, ushort composition, ref TransferData transfer) {
      transfer.compositionId = cx.AUTHORING.id.composition;
      cx.AUTHORING.id.composition = composition;
    }

    // public static void EnterLocalComposition(
    //   ref Composition cx,
    //   ushort composition,
    //   ref LocalTransferData transfer
    // ) {
    //   transfer.local = cx.AUTHORING.id.local;
    //   transfer.compositionId = cx.AUTHORING.id.composition;
    //   cx.AUTHORING.id.local = LocalId.Initial;
    //   cx.AUTHORING.id.composition = composition;
    // }

    public static void ExitComposition(ref Composition cx, ref TransferData transfer) {
      cx.AUTHORING.id.composition = transfer.compositionId;
    }

    // public static void ExitLocalComposition(ref Composition cx, ref LocalTransferData transfer) {
    //   cx.AUTHORING.id.local = transfer.local;
    //   cx.AUTHORING.id.composition = transfer.compositionId;
    // }

    public ref struct TransferData {
      public ushort compositionId;
    }

    public ref struct LocalTransferData {
      public LocalId local;
      public ushort compositionId;
    }
  }

  [StructLayout(LayoutKind.Sequential, Size = 4)]
  public struct LocalId {
    public ushort index;
    public ushort depth;

    public static readonly LocalId Initial;

    public override string ToString() {
      return $"L{index}@{depth}";
    }

    public static LocalId FromData(int data) {
      var short01 = (ushort)(data & 0xFFFF);
      var short23 = (ushort)((data >> 16) & 0xFFFF);
      return new LocalId {
        index = short01,
        depth = short23
      };
    }
  }

  public static class CompositionIdRegistry {

    public static readonly List<CompositionIdEntry> CompositionIds = new();
    public static readonly List<TypeIdEntry> TypeIds = new();

    [Conditional("UNITY_EDITOR")]
    public static void RegisterTypeId(string name) {
      TypeIds.Add(new TypeIdEntry { name = name});
    }

    [Conditional("UNITY_EDITOR")]
    public static void RegisterCompositionId(string name, string location) {
      CompositionIds.Add(new CompositionIdEntry { name = name, location = location});
    }

    public static string GetCompositionName(ushort id) {
      if (id == 0 || id > CompositionIds.Count) return null;
      return CompositionIds[id - 1].name;
    }

    public static string GetCompositionLocation(ushort id) {
      if (id == 0 || id > CompositionIds.Count) return null;
      return CompositionIds[id - 1].location;
    }

    public static string GetTypeName(ushort id) {
      if (id == 0 || id > TypeIds.Count) return null;
      return TypeIds[id - 1].name;
    }

    public struct CompositionIdEntry {
      public string name;
      public string location;
    }

    public struct TypeIdEntry {
      public string name;
    }

  }

  [StructLayout(LayoutKind.Explicit, Size = 8)]
  public struct CompositionId {
    private static ushort _compositionIdCounter = 1;
    private static ushort _typeIdCounter = 1;

    public static ushort GeneratedTypeId = GetTypeId("Hash");

    [FieldOffset(0)]
    public LocalId local;
    [FieldOffset(4)]
    public ushort composition;
    [FieldOffset(6)]
    public ushort type;
    [FieldOffset(0)]
    public ulong packed;

    public static ushort GetCompositionId(string name = null, string location = null) {
      if (_compositionIdCounter == ushort.MaxValue) {
        throw new InvalidOperationException("CompositionId counter overflow!");
      }
      CompositionIdRegistry.RegisterCompositionId(name, location);
      return _compositionIdCounter++;
    }

    public static ushort GetTypeId(string name = null) {
      if (_typeIdCounter == ushort.MaxValue) {
        throw new InvalidOperationException("TypeId counter overflow!");
      }
      CompositionIdRegistry.RegisterTypeId(name);
      return _typeIdCounter++;
    }

    public override string ToString() {
      return $"{local}T{type}C{composition}";
    }

    public static CompositionId Generated(ushort composition, int data) {
      var local = LocalId.FromData(data);
      return new CompositionId {
        local = local,
        composition = composition,
        type = GeneratedTypeId
      };
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
    private readonly LocalId _local;
    private readonly ScopeCompletionCallback _callback;

    private ScopeHandle(BoundaryCell cell, ScopeCompletionCallback callback) {
      _cell = cell;
      _return = cell.current;
      _callback = callback;
      _cursor = cell.cursor;
      _local = cell.localId;
      cell.cursor = 0;
      cell.localId = new LocalId { index = 0, depth = (ushort)(_local.depth + 1) };
    }

    public void Dispose() {
      try {
        _callback?.Invoke(_cell, this);
      } finally {
        _cell.current = _return;
        _cell.cursor = _cursor;
        _cell.localId = _local;
      }
    }

    public ref ElementRef Apply(VisualElement given, IComposable composable, ref ElementRef element) {
      Dispose();
      element.element = given;
      element.composable = composable;
      return ref element;
    }

    internal static ScopeHandle Push(BoundaryCell cell, IComposable next, ScopeCompletionCallback callback) {
      var scope = new ScopeHandle(cell, callback);
      cell.current = next;
      return scope;
    }
  }
}