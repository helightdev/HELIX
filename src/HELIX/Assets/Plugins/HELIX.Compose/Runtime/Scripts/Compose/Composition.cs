using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HELIX.Signals;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public delegate void ScopeCompletionCallback(BoundaryCell cell, in ScopeHandle handle);

  public ref struct Composition {
    /// <summary>
    /// The boundary to which the current composition belongs to.
    /// </summary>
    public readonly IBoundary boundary;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Access to the current cursor element in the composition.
    /// This will usually be the last element that has been composed.
    /// </summary>
    public ElementRef CURSOR;

    public ElementRef SCOPE => CURSOR.composable != boundary.Cell.scope
      ? throw new InvalidOperationException("Cursor composable does not match the current scope.")
      : CURSOR;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Low-Level access to composition authoring related data.
    /// Mainly useful for implementing custom composable types or special functionality.
    /// </summary>
    public CompositionAuthoring AUTHORING;

    /// <summary>
    /// The most recent <see cref="ComposableSlot"/> in the composition if there is one.
    /// </summary>
    public ComposableSlot Slot { get => AUTHORING.cell.slot; set => AUTHORING.cell.slot = value; }

    public bool CursorRetained => CURSOR.IsRetained;
    public bool CursorDirty => CURSOR.IsDirty;
    public bool Skipped => AUTHORING.cell.skip;

    /// <summary>
    /// QOL Accessor to the <see cref="BoundaryCell"/> of the composition.
    /// </summary>
    public BoundaryCell Cell => AUTHORING.cell;

    public Composition(IBoundary initiator) : this() {
      boundary = initiator;

      AUTHORING.cell = initiator.Cell ?? BoundaryCell.Shared;
      AUTHORING.cell.cursor = 0;
      AUTHORING.cell.localId = LocalId.Initial;
      AUTHORING.cell.scope = initiator;
      AUTHORING.cursor = initiator.Element;

      ReplaceCursor(boundary);
    }

    public Composition(IBoundary boundary, CompositionId id, IComposable composable) : this() {
      this.boundary = boundary;
      AUTHORING.cell = boundary.Cell ?? BoundaryCell.Shared;
      AUTHORING.cell.cursor = 0;
      AUTHORING.cell.localId = LocalId.Initial;
      AUTHORING.cell.scope = composable;
      AUTHORING.cursor = composable.Element;
      AUTHORING.id = id;
      ReplaceCursor(composable);
    }

    /// <summary>
    /// Conditionally composes a visual element without affecting positional indices of following elements should the
    /// element be removed from the composition.
    /// </summary>
    /// <param name="condition">Whether to progress the positional indices.</param>
    /// <param name="count">How many composables are skipped if true.</param>
    /// <returns>Whether the condition has matched.</returns>
    [ContractAnnotation("condition:true => true; condition:false => false")]
    public bool Conditional(bool condition, ushort count = 1) {
      if (condition) return true;
      AUTHORING.cell.localId.index += count;
      return false;
    }

    public void SkipScope() {
      Cell.skip = true;
    }

    public bool Stateless() {
      if (CursorRetained) {
        SkipScope();
        return false;
      }
      return true;
    }


    public void SubscribeTo(Signal signal) {
      boundary.SubscribeToContextData(signal.contextKey, signal);
    }

    public void ReplaceCursor(IComposable replacement, CompositionRetention ret = CompositionRetention.Undefined) {
      CURSOR = new ElementRef(replacement.Element, replacement, ret);
    }

    /// <summary>
    /// Begins a context modification scope, allowing for writing to the context of the current composable scope.
    /// </summary>
    /// <param name="context">Accessor to the context store.</param>
    /// <returns>A transactional disposable handle.</returns>
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
      if (!ContextData.TryLookup(CURSOR.element, key.id, out var data, includeSelf)) return false;
      if (data is not ContextData<T> typedData) return false;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      value = typedData.value;
      return true;
    }

    public readonly bool TryReadContextData<T>(
      ContextKey<T> key, out ContextData<T> data, bool listen = true, bool includeSelf = true
    ) {
      data = null;
      if (!ContextData.TryLookup(CURSOR.element, key.id, out var found, includeSelf)) return false;
      if (found is not ContextData<T> typedData) return false;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      data = typedData;
      return true;
    }

    public readonly T ReadContext<T>(ContextKey<T> key, bool listen = true, bool includeSelf = true) {
      if (!ContextData.TryLookup(CURSOR.element, key.id, out var data, includeSelf)) return key.defaultValue;
      if (data is not ContextData<T> typedData) return key.defaultValue;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      return typedData.value;
    }

    public readonly T ReadContextOrDefault<T>(
      ContextKey<T> key, T defaultValue = default, bool listen = true, bool includeSelf = true
    ) {
      if (!ContextData.TryLookup(CURSOR.element, key.id, out var data, includeSelf)) return defaultValue;
      if (data is not ContextData<T> typedData) return defaultValue;
      if (listen) boundary.SubscribeToContextData(key, typedData);
      return typedData.value;
    }

    public readonly T LookupLocalAncestor<T>(VisualElement element = null) {
      var cursor = element ?? CURSOR.composable.Element;
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

    public static IComposable Promote(VisualElement element) {
      if (element is IComposable composable) {
        return composable;
      } else {
        return (UserdataTracker)(element.userData ??= new UserdataTracker {
          PackedId = 0,
          Flag = UssFlag.None,
          Element = element
        });
      }
    }

    public static IStateAttachmentHolder PromoteHolder(VisualElement element) {
      var composable = Promote(element);
      return composable is not IStateAttachmentHolder holder
        ? throw new InvalidOperationException($"Element {element} is not a state attachment holder.")
        : holder;
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
      return $"L{index}@{depth}D";
    }

    public static LocalId FromData(int data) {
      unchecked {
        var mixed = (uint)data;
        mixed ^= mixed >> 16;
        mixed *= 0x7FEB352Du;
        mixed ^= mixed >> 15;
        mixed *= 0x846CA68Bu;
        mixed ^= mixed >> 16;

        var index = (ushort)mixed;
        var depth = (ushort)(mixed >> 16);
        return new LocalId {
          index = index,
          depth = depth
        };
      }
    }

    public static LocalId FromDataUnmixed(int data) {
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
    private static int _generalIdCounter = 1;

    public static ushort GeneratedTypeId = GetTypeId("Hash");
    public static ushort SlotTypeId = GetTypeId("Slot");
    public static ushort GeneratedCompositionId = GetCompositionId("Hash");

    [FieldOffset(0)]
    public LocalId local;
    [FieldOffset(4)]
    public ushort composition;
    [FieldOffset(6)]
    public ushort type;
    [FieldOffset(0)]
    public ulong packed;


    public CompositionId(LocalId local, ushort composition, ushort type) : this() {
      this.local = local;
      this.composition = composition;
      this.type = type;
    }

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

    public static int GetGeneralId() {
      unchecked { return _generalIdCounter++; }
    }

    public override string ToString() {
      return $"{local}T{type}C{composition}";
    }

    public static CompositionId GeneratedWithComposition(ushort composition, int data) {
      var local = LocalId.FromData(data);
      return new CompositionId { local = local, composition = composition, type = GeneratedTypeId };
    }

    public static CompositionId GeneratedWithType(ushort type, int data) {
      var local = LocalId.FromData(data);
      return new CompositionId { local = local, composition = GeneratedCompositionId, type = type };
    }

    public static CompositionId Generated(int data) {
      var local = LocalId.FromData(data);
      return new CompositionId { local = local, composition = GeneratedCompositionId, type = GeneratedTypeId };
    }

    public static CompositionId Generated(LocalId id) {
      return new CompositionId { local = id, composition = GeneratedCompositionId, type = GeneratedTypeId };
    }
  }

  public readonly ref struct ElementRef {
    public readonly VisualElement element;
    public readonly IComposable composable;
    public readonly CompositionRetention retention;
    public readonly IStyle style;

    public ElementRef(VisualElement element, IComposable composable, CompositionRetention retention) {
      this.element = element;
      this.composable = composable;
      this.retention = retention;
      this.style = element?.style;
    }

    public ElementRef(IComposable composable) : this(composable.Element, composable, CompositionRetention.Undefined) { }
    public void MarkFlag(UssFlag flag) => composable.MarkFlag(flag);

    public bool IsRetained => retention == CompositionRetention.Retained;
    public bool IsDirty => !IsRetained;

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
    private readonly bool _skipCallback;
    public readonly IComposable current;

    private ScopeHandle(BoundaryCell cell, IComposable current, ScopeCompletionCallback callback) {
      _cell = cell;
      _return = cell.scope;
      _callback = callback;
      _cursor = cell.cursor;
      _local = cell.localId;
      _slot = cell.slot;
      _skipCallback = cell.skip;
      this.current = current;

      cell.skip = false;
      cell.cursor = 0;
      var localDepth = _local.depth;
      unchecked { localDepth++; }
      cell.localId = new LocalId { index = 0, depth = localDepth };
    }


    public void Dispose() {
      try {
        if (!_cell.skip) _callback?.Invoke(_cell, this);
      } finally {
        _cell.scope = _return;
        _cell.cursor = _cursor;
        _cell.localId = _local;
        _cell.slot = _slot;
        _cell.skip = _skipCallback;
      }
    }

    public ref ElementRef Apply(VisualElement given, IComposable composable, ref ElementRef element) {
      Dispose();
      element = new ElementRef(given, composable, element.retention);
      return ref element;
    }

    internal static ScopeHandle Push(BoundaryCell cell, IComposable next, ScopeCompletionCallback callback) {
      var scope = new ScopeHandle(cell, next, callback);
      cell.scope = next;
      return scope;
    }
  }
}