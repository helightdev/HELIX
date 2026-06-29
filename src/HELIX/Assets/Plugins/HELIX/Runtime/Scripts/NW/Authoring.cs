using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public ref partial struct CompositionAuthoring {
    public VisualElement cursor;
    public BoundaryCell cell;
    public CompositionId id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VisualElement ReadCursor() {
      cursor = cell.ReadCursor();
      return cursor;
    }

    public LocalId GetCurrentLocalId() {
      return cell.localId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong PrepareId(ushort typeId) {
      //id.key = cell.key;
      id.type = typeId;
      id.local = cell.localId;
      return id.packed;
    }

    public bool RequireTracked<T>(ushort typeId, out T value, out bool retained) where T : VisualElement {
      var packed = PrepareId(typeId);
      var current = cell.ReadCursorOrFind(packed);
      if (current is T typed) {
        value = typed;

        var userData = current.userData;
        if (userData is not UserdataTracker tracker) {
          value = null;
          retained = false;
          return false;
        } // Not valid

        retained = tracker.EnsureIdentity(packed);
        return true;
      }

      value = null;
      retained = false;
      return false;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RequireComposable<T>(ushort typeId, out T value, out bool retained)
      where T : VisualElement, IComposable {
      var packed = PrepareId(typeId);
      var current = cell.ReadCursorOrFind(packed);
      if (current is T typed) {
        value = typed;
        retained = typed.EnsureIdentity(packed);
        return true;
      }

      value = null;
      retained = false;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RequireBoundaryNode(ushort typeId, out CompositionBoundaryNode node, out bool retained) {
      if (RequireComposable(typeId, out node, out retained)) {
        return true;
      }

      node = new CompositionBoundaryNode { TypeId = id.packed };
      return false;
    }

    public bool InitializeNode(ushort typeId, out CompositionNode node) {
      var packed = PrepareId(typeId);
      var current = cell.ReadCursorOrFind(packed);
      if (current is CompositionNode typed) {
        node = typed;
        return !typed.EnsureIdentity(packed);
      }

      node = new CompositionNode { TypeId = packed };
      return true;
    }

    public ScopeHandle YieldScope(ref Composition ctx, VisualElement given, ScopeCompletionCallback callback = null) {
      if (given is not IComposable composable) throw new InvalidOperationException("Element is not composable");
      YieldElement(ref ctx, given);
      return ScopeHandle.Push(cell, composable, callback);
    }

    public ref ElementRef YieldBoundary<T>(ref Composition ctx, T given) where T : VisualElement, IBoundary {
      ref var element = ref YieldElement(ref ctx, given);
      given.RefreshHierarchy();
      var scope = ScopeHandle.Push(cell, given, null);
      try {
        RecompositionScope.MarkDirty(given);
      } catch (Exception e) {
        Debug.LogException(e);
      }
      return ref scope.Apply(given, given, ref element);
    }

    public ref ElementRef YieldElement(ref Composition ctx, VisualElement given) {
      var container = cell.current.Element;
      var currentIndex = container.IndexOf(given);
      if (currentIndex == cell.cursor) goto complete;

      if (currentIndex != -1 && given.parent == container) {
        container.hierarchy.RemoveAt(currentIndex);
        container.Insert(cell.cursor, given);
        // TODO: Maybe do this using Sort() to prevent animation interruptions
        goto complete;
      }

      if (given.parent != null) {
        throw new InvalidOperationException(
          $"Element {given} is already parented to {given.parent}, but the cursor is at {cell.cursor} in {container.name}."
        );
      }

      if (cell.cursor < container.childCount) {
        container.Insert(cell.cursor, given);
      } else {
        container.Add(given);
      }

      complete:
      if (given is IComposable composable) {
        ctx.APPLY.composable = composable;
      } else {
        composable = (UserdataTracker)(given.userData ??= new UserdataTracker {
          TypeId = id.packed,
          DirtyFlags = UssDirtyFlags.None,
          Element = given
        });
      }

      ctx.APPLY.element = composable.Element;
      ctx.APPLY.composable = composable;
      cell.cursor++;
      cell.localId.index++;
      cursor = null; // Clear authoring element
      return ref ctx.APPLY;
    }
  }
}