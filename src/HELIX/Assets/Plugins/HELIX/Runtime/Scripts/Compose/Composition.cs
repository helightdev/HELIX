using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public delegate void ScopeCompletionCallback(BoundaryCell cell, in ScopeHandle handle);

  public ref struct Composition {
    public readonly IBoundary boundary;
    public ElementRef APPLY;
    public CompositionAuthoring AUTHORING;

    public ComposableSlot Slot { get => AUTHORING.cell.slot; set => AUTHORING.cell.slot = value; }

    public BoundaryCell Cell => AUTHORING.cell;

    public Composition(IBoundary initiator) : this() {
      boundary = initiator;

      AUTHORING.cell = initiator.Cell ?? BoundaryCell.Shared;
      AUTHORING.cell.cursor = 0;
      AUTHORING.cell.localId = LocalId.Initial;
      AUTHORING.cell.scope = initiator;
      AUTHORING.cursor = initiator.Element;

      APPLY.element = boundary.Element;
      APPLY.composable = boundary;
    }

    public Composition(IBoundary boundary, CompositionId id, IComposable composable) : this() {
      this.boundary = boundary;
      AUTHORING.cell = boundary.Cell ?? BoundaryCell.Shared;
      AUTHORING.cell.cursor = 0;
      AUTHORING.cell.localId = LocalId.Initial;
      AUTHORING.cell.scope = composable;
      AUTHORING.cursor = composable.Element;
      AUTHORING.id = id;
      APPLY.Replace(composable);
    }

    public bool Conditional(bool condition, ushort count = 1) {
      if (condition) return true;
      AUTHORING.cell.localId.index += count;
      return false;
    }

    public ContextModificationScope WriteContext(out ContextAccessor context) {
      if (AUTHORING.cell.scope is not IContextWriteable writeable) {
        throw new InvalidOperationException("Current scope is not context writeable.");
      }

      writeable.BeginContextModification();
      context = new ContextAccessor(writeable);
      return new ContextModificationScope(writeable);
    }

    public readonly bool TryReadContext<T>(
      ContextKey<T> key, out T value, bool listen = true, bool includeSelf = true
    ) {
      value = default;
      if (!ContextData.TryLookup(APPLY.element, key.id, out var data, includeSelf)) return false;
      if (data is not ContextData<T> typedData) return false;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      value = typedData.value;
      return true;
    }

    public readonly bool TryReadContextData<T>(
      ContextKey<T> key, out ContextData<T> data, bool listen = true, bool includeSelf = true
    ) {
      data = null;
      if (!ContextData.TryLookup(APPLY.element, key.id, out var found, includeSelf)) return false;
      if (found is not ContextData<T> typedData) return false;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      data = typedData;
      return true;
    }

    public readonly T ReadContext<T>(ContextKey<T> key, bool listen = true, bool includeSelf = true) {
      if (!ContextData.TryLookup(APPLY.element, key.id, out var data, includeSelf)) return key.defaultValue;
      if (data is not ContextData<T> typedData) return key.defaultValue;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      return typedData.value;
    }

    public readonly T ReadContextOrDefault<T>(
      ContextKey<T> key, T defaultValue = default, bool listen = true, bool includeSelf = true
    ) {
      if (!ContextData.TryLookup(APPLY.element, key.id, out var data, includeSelf)) return defaultValue;
      if (data is not ContextData<T> typedData) return defaultValue;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      return typedData.value;
    }

    public readonly T LookupLocalAncestor<T>(VisualElement element = null) {
      var cursor = element ?? APPLY.composable.Element;
      while (cursor != null) {
        if (ReferenceEquals(cursor, boundary)) break;
        if (cursor is T matched) return matched;
        cursor = cursor.parent;
      }
      return default;
    }
  }

  public static class CompositionInternals {
    public static void EnterComposition(ref Composition cx, ushort composition, ref TransferData transfer) {
      transfer.compositionId = cx.AUTHORING.id.composition;
      cx.AUTHORING.id.composition = composition;
    }

    public static void ExitComposition(ref Composition cx, ref TransferData transfer) {
      cx.AUTHORING.id.composition = transfer.compositionId;
    }

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
      return new LocalId { index = short01, depth = short23 };
    }
  }

  public static class CompositionIdRegistry {
    public static readonly List<CompositionIdEntry> CompositionIds = new();
    public static readonly List<TypeIdEntry> TypeIds = new();

    [Conditional("UNITY_EDITOR")]
    public static void RegisterTypeId(string name) {
      TypeIds.Add(new TypeIdEntry { name = name });
    }

    [Conditional("UNITY_EDITOR")]
    public static void RegisterCompositionId(string name, string location) {
      CompositionIds.Add(new CompositionIdEntry { name = name, location = location });
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
      return new CompositionId { local = local, composition = composition, type = GeneratedTypeId };
    }
  }

  public ref struct ElementRef {
    public VisualElement element;
    public IComposable composable;

    public void Replace(IComposable replacement) {
      composable = replacement;
      element = replacement.Element;
    }

    public static implicit operator VisualElement(ElementRef reference) => reference.element;

    public bool IsValid => element != null;
  }

  public readonly struct ScopeHandle : IDisposable {
    private readonly BoundaryCell _cell;
    private readonly IComposable _return;
    private readonly int _cursor;
    private readonly LocalId _local;
    private readonly ScopeCompletionCallback _callback;
    private readonly ComposableSlot _slot;

    private ScopeHandle(BoundaryCell cell, ScopeCompletionCallback callback) {
      _cell = cell;
      _return = cell.scope;
      _callback = callback;
      _cursor = cell.cursor;
      _local = cell.localId;
      _slot = cell.slot;

      cell.cursor = 0;
      cell.localId = new LocalId { index = 0, depth = (ushort)(_local.depth + 1) };
    }

    public void Dispose() {
      try {
        _callback?.Invoke(_cell, this);
      } finally {
        _cell.scope = _return;
        _cell.cursor = _cursor;
        _cell.localId = _local;
        _cell.slot = _slot;
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
      cell.scope = next;
      return scope;
    }
  }
}