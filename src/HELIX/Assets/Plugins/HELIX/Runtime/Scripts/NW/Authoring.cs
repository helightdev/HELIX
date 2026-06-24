using System;
using System.Runtime.CompilerServices;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong PrepareId(ushort typeId) {
      id.key = cell.key;
      id.type = typeId;
      return id.packed;
    }

    public bool RequireTracked<T>(ushort typeId, out T value, out bool retained) where T : VisualElement {
      var current = ReadCursor();
      var packed = PrepareId(typeId);
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
      var current = ReadCursor();
      var packed = PrepareId(typeId);
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
      var current = ReadCursor();
      var packed = PrepareId(typeId);
      if (current is CompositionNode typed) {
        node = typed;
        return !typed.EnsureIdentity(packed);
      }

      node = new CompositionNode {
        TypeId = packed
      };
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
      RecompositionScope.MarkDirty(given);
      return ref element;
    }

    public ref ElementRef YieldElement(ref Composition ctx, VisualElement given) {
      var container = cell.current.Element;
      var currentIndex = container.IndexOf(given);
      if (currentIndex == cell.cursor) goto complete;

      if (currentIndex != -1) {
        throw new InvalidOperationException(
          $"Element {given} is already in the container at index {currentIndex}, but the cursor is at {cell.cursor}."
        );
      }

      if (given.parent != null) {
        throw new InvalidOperationException(
          $"Element {given} is already parented to {given.parent}, but the cursor is at {cell.cursor}."
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
      id.localIndex++;
      cursor = null; // Clear authoring element
      return ref ctx.APPLY;
    }
  }
}