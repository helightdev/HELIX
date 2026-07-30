using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public enum CompositionRetention {
    Undefined = 0,
    Retained,
    Reset,
    New
  }


  public ref partial struct CompositionAuthoring {
    public VisualElement cursor;
    public BoundaryCell cell;
    public CompositionId id;
    public CompositionRetention retention ;

    public void SetId(CompositionId given) {
      id = given;
      cell.localId = given.local;
    }

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
          retention = CompositionRetention.New;
          return false;
        } // Not valid

        retained = tracker.EnsureIdentity(packed);
        retention = retained ? CompositionRetention.Retained : CompositionRetention.Reset;
        return true;
      }

      value = null;
      retained = false;
      retention = CompositionRetention.New;
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
        retention = retained ? CompositionRetention.Retained : CompositionRetention.Reset;
        return true;
      }

      value = null;
      retained = false;
      retention = CompositionRetention.New;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RequireCompositionBoundaryNode(ushort typeId, out CompositionBoundaryNode node, out bool retained) {
      if (RequireComposable(typeId, out node, out retained)) {
        retention = retained ? CompositionRetention.Retained : CompositionRetention.Reset;
        return true;
      }

      node = new CompositionBoundaryNode { TypeId = id.packed };
      retention = CompositionRetention.New;
      return false;
    }

    public bool RequireBoundaryStateComposable<TData, TStateComposable>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out TData data,
      out TStateComposable attachment
    ) where TData : BoundaryData, new() where TStateComposable : IBoundaryComposable, new() {
      if (RequireCompositionBoundaryNode(typeId, out node, out var retained)) {
        if (node.Data is not TData currentProps) {
          data = new TData();
          attachment = new TStateComposable();
          node.SetComposable(data, attachment);
          retention = CompositionRetention.Reset;
          return false;
        }
        if (node.BoundaryComposable is not TStateComposable currentAttachment || !retained) {
          attachment = new TStateComposable();
          data = currentProps;
          node.SwapComposable(attachment);
          retention = CompositionRetention.Reset;
          return false;
        }

        attachment = currentAttachment;
        data = currentProps;
        retention = CompositionRetention.Retained;
        return true;
      }

      data = new TData();
      attachment = new TStateComposable();
      node.SetComposable(data, attachment);
      retention = CompositionRetention.New;
      return false;
    }

    public bool InitializeNode(ushort typeId, out CompositionNode node) {
      var packed = PrepareId(typeId);
      var current = cell.ReadCursorOrFind(packed);
      if (current is CompositionNode typed) {
        node = typed;
        var isEqual = typed.EnsureIdentity(packed);
        retention = isEqual ? CompositionRetention.Retained : CompositionRetention.Reset;
        return !isEqual;
      }

      node = new CompositionNode { TypeId = packed };
      retention = CompositionRetention.New;
      return true;
    }

    public ScopeHandle YieldScope(ref Composition ctx, VisualElement given, ScopeCompletionCallback callback = null) {
      ref var elementRef = ref YieldElement(ref ctx, given);
      return ScopeHandle.Push(cell, elementRef.composable, callback);
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
      var container = cell.scope.Element.contentContainer;
      var currentIndex = container.IndexOf(given);
      if (currentIndex == cell.cursor) goto complete;

      // If the identified element is already present, move it into position and complete
      if (currentIndex != -1 && given.parent == container) {
        HXProfiling.TrackHierarchyMovement();
        // container.RemoveAt(currentIndex);
        // container.Insert(cell.cursor, given);
        // TODO: Maybe do this using Sort() to prevent animation interruptions

        VisualElementOrdering.Move(container.hierarchy, given, cell.cursor);
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
      if (given is IComposable composable) { } else { composable = CompositionInternals.Promote(given); }
      if (composable.TypeId == 0) composable.TypeId = id.packed;
      ctx.CURSOR.Replace(composable, retention);
      cell.cursor++;
      cell.localId.index++;
      cursor = null; // Clear authoring element
      retention = CompositionRetention.Undefined;
      return ref ctx.CURSOR;
    }
  }


  public static class VisualElementOrdering {
    private static readonly Dictionary<VisualElement, int> _indices = new();
    private static readonly Comparison<VisualElement> _comparer = Compare;

    private static VisualElement _movedElement = null!;
    private static int _sourceIndex;
    private static int _targetIndex;

    public static void Move(
      VisualElement.Hierarchy hierarchy,
      VisualElement element,
      int targetIndex
    ) {
      var count = hierarchy.childCount;
      if (count < 2) return;
      var sourceIndex = hierarchy.IndexOf(element);

      if (sourceIndex < 0)
        throw new ArgumentException(
          "The element does not belong to the hierarchy.",
          nameof(element)
        );

      if (targetIndex < 0) targetIndex = 0;
      else if (targetIndex >= count) targetIndex = count - 1;

      if (sourceIndex == targetIndex) return;

      _movedElement = element;
      _sourceIndex = sourceIndex;
      _targetIndex = targetIndex;

      _indices.Clear();
      _indices.EnsureCapacity(count);
      for (var i = 0; i < count; i++) _indices.Add(hierarchy[i], i);
      hierarchy.Sort(_comparer);
      _indices.Clear();
    }

    private static int Compare(VisualElement left, VisualElement right) {
      return GetDestinationIndex(left) - GetDestinationIndex(right);
    }

    private static int GetDestinationIndex(VisualElement element) {
      if (ReferenceEquals(element, _movedElement)) return _targetIndex;
      var index = _indices[element];

      if (_sourceIndex < _targetIndex) {
        if (index > _sourceIndex && index <= _targetIndex) return index - 1;
      } else {
        if (index >= _targetIndex && index < _sourceIndex) return index + 1;
      }
      return index;
    }
  }
}