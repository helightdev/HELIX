using System;
using System.Runtime.CompilerServices;
using HELIX.Compose.Collections;
using HELIX.Extensions;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace HELIX.Compose {
  public interface IBoundary : IContextComposable, IContextWriteable, IDirty, IDisposable, IPossiblyDisposed {
    IBoundary Parent { get; }
    BoundaryCell Cell { get; }

    int TreeDepth { get; }

    void RefreshHierarchy();

    void Recompose();
    void CheckModified() => BoundaryHelper.CheckModifiedDefault(this);

    void SubscribeToContextData(int key, ContextData data) {
      AcquireWriteableContext().Subscribe(key, data);
    }
  }

  public class BoundaryCell {
    public static readonly BoundaryCell Shared = new();

    public IComposable scope;
    public int cursor;
    public LocalId localId;
    public ComposableSlot slot;
    public bool skip;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VisualElement ReadCursor() {
      var element = scope.Element;
      return element.childCount <= cursor ? null : element.ElementAt(cursor);
    }

    public VisualElement ReadCursorOrFind(ulong typeId) {
      // Get the current cursor or finds the element after the current curser if it already exists
      var element = ReadCursor();
      if (element == null) return null;
      if (GetTypeId(element) == typeId) return element;
      for (var i = cursor + 1; i < scope.Element.childCount; i++) {
        var child = scope.Element.ElementAt(i);
        if (GetTypeId(child) == typeId) return child;
      }
      return element;
    }

    public ulong GetTypeId(VisualElement element) {
      if (element is IComposable composable) {
        return composable.PackedId;
      } else {
        if (element.userData is UserdataTracker tracker) return tracker.PackedId;
        return 0;
      }
    }

    public void TrimChildren() {
      var element = scope.Element;
      var overflow = element.childCount - cursor;
      for (var i = 0; i < overflow; i++) {
        //Debug.Log($"Removing child {i} from {element.name}");
        element.RemoveAt(element.childCount - 1);
      }
      HXComposeProfiling.TrackHierarchyDeletion(overflow);
      //if (overflow > 0) Debug.LogWarning($"Removed {overflow} children");
    }
  }

  public static class BoundaryHelper {
    public static void ContextBefore(IContextWriteable writeable) {
      writeable.WrittenContext?.ResetSubscriptionMarkers();
    }

    public static void CheckModifiedDefault(IBoundary boundary) {
      var context = boundary.WrittenContext;
      if (context != null && context.CheckSubscriptionsModified()) HXComposer.EnqueueDirty(boundary);
    }

    public static void ContextAfter(IContextWriteable writeable) {
      var written = writeable.WrittenContext;
      if (written == null) return;
      written.PruneSubscriptions();
      if (!written.IsUnused) return;
      SparseContextMap.Release(written);
      writeable.WrittenContext = null;
    }

    public static void Kill(IBoundary boundary) {
      HXComposer.RemoveDirty(boundary);
      HXComposer.UnregisterBoundary(boundary);
      boundary.Element.RemoveFromHierarchy();
    }

    public static int GetDepth(IBoundary boundary) => boundary.Element.GetDepth();
  }

  public interface IProps<T> where T : struct {
    ref T props { get; }
    void ReceiveProps(in T props);
  }

}
