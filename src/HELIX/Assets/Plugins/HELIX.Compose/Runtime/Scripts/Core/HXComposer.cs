using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public static class HXComposer {
    public static bool UseEventLoop = false;
    public static bool AutoDisposeOrphans = true;
    public static int MaxRecompositionDepth = 1024;

    public static readonly IndexedReferencePriorityQueue<IBoundary, int> Dirty = new();
    public static readonly IndexedReferencePriorityQueue<IBoundary, int> DisposalQueue = new();
    public static readonly HashSet<IBoundary> Boundaries = new(new ReferenceEqualityComparer<IBoundary>());
    public static bool IsScoped;
    public static bool IsProcessing;
    private static bool _processRequested;
    public static IBoundary CurrentBoundary;
    public static int RecompositionDepth;

    public static bool IsBoundaryDirty(IBoundary boundary) {
      return boundary != null && Dirty.Contains(boundary);
    }

    public static bool IsBoundaryPendingDisposal(IBoundary boundary) {
      return boundary != null && DisposalQueue.Contains(boundary);
    }

    public static void RegisterActiveBoundary(IBoundary boundary) {
      Boundaries.Add(boundary);

      var hadDisposal = DisposalQueue.Remove(boundary);
#if ENABLE_PROFILER
      if (hadDisposal) HXComposeProfiling.TrackDisposalStats(1, 0);
#endif
    }

    public static void UnregisterBoundary(IBoundary boundary) {
      Dirty.Remove(boundary);
      Boundaries.Remove(boundary);
      DisposalQueue.Remove(boundary);
    }

    public static void MarkDirty(IBoundary boundary, bool mayInline = true) {
      if (!IsScoped) {
        if (boundary.Element?.panel is null)
          throw new InvalidOperationException("Cannot mark dirty boundary that is not in scope and has no panel");

        using (BeginBatch()) {
          Dirty.Enqueue(boundary, boundary.TreeDepth);
          return;
        }
      }

      // If we descend the tree forward, we can skip the queue.
      // Initially, this was for static dictionary initialization, but now it's even saver.
      // However, for safety to possibly prevent some issues, we don't do it for non forward compositions.
      if (mayInline && IsProcessing && CurrentBoundary == boundary.Parent) {
        // _inlinedRecompositionCount.Value++;
        // _inlinedRecompositionCount.Sample();
        Dirty.Remove(boundary);
        Recompose(boundary);
        return;
      }

      /*
       TODO: This does not work for multiple children and I currently don't know a good way to fix that.
       For now, I'll just remove it until I have figured out a way determine guaranteed forward composition.
       My current idea is just pushing this to the queue handler to check if the last processed boundary was the parent,
       in which case I can just skip populate context in this case. Note: Need to consider batch eligibility.
      */

      Dirty.Enqueue(boundary, boundary.TreeDepth);
    }

    public static void EnqueueDirty(IBoundary boundary) {
      Dirty.Enqueue(boundary, boundary.TreeDepth);
    }

    public static void RemoveDirty(IBoundary boundary) {
      Dirty.Remove(boundary);
    }

    public static void NotifyDetach(IBoundary boundary) {
      if (!IsScoped) return;

      // We are removed while in scope, if we don't reattach while in scope, we can assume that the removal is final
      DisposalQueue.Enqueue(boundary, boundary.TreeDepth);
      HXComposeProfiling.TrackDisposalDiscovery(1);
    }

    public static void DirtyChildren(IBoundary boundary) {
      using (BeginBatch()) {
        foreach (var current in Boundaries)
          if (current.Parent == boundary)
            Dirty.Enqueue(current, current.TreeDepth);
      }
    }

    public static bool IsEligibleForDisposal(IBoundary boundary) {
      if (boundary.IsDisposed) return true;
      if (boundary.Element?.panel is null) return true;
      return false;
    }

    public static void DisposeOrphanedBoundaries() {
      if (IsProcessing) throw new InvalidOperationException("Recomposition scope is currently processing");
      var discardCount = 0;
      var disposedCount = 0;
#if ENABLE_PROFILER
      using (HXComposeProfiling.DisposeOrphansMarker.Auto()) {
#endif
        try {
          IsProcessing = true;
          while (DisposalQueue.TryDequeueTail(out var boundary)) {
            if (!IsEligibleForDisposal(boundary)) {
              discardCount++;
              continue;
            }
            disposedCount++;
            UnregisterBoundary(boundary);
            var element = boundary.Element;
            if (element != null) DisposeSectionRecursively(element);
          }
        } finally {
          IsProcessing = false;
          HXComposeProfiling.TrackDisposalStats(discardCount, disposedCount);
        }
#if ENABLE_PROFILER
      }
#endif
    }

    public static void DisposeSectionRecursively(VisualElement root) {
      if (root == null) throw new ArgumentNullException(nameof(root));
      try {
        // Reverse should the child for some reason decide to remove itself
        for (var i = root.childCount - 1; i >= 0; i--) {
          var child = root[i];
          if (child is IBoundary) continue; // Skip descendant boundaries
          DisposeSectionRecursively(child);
        }
      } catch (Exception ex) {
        Debug.LogException(ex);
      }
      if (root is IDisposable disposable) disposable.DisposeSafe();
      if (root.userData is IDisposable dataDisposable) dataDisposable.DisposeSafe();
    }

    internal static void ProcessDirty() {
      if (IsProcessing) {
        _processRequested = true;
        return;
      }

      var discoveredCount = 0;
      foreach (var boundary in Boundaries) {
        boundary.CheckModified();

        // We are in scope which is not done for unmanaged movements.
        // If the boundary is still disposable at the end, delayed disposal can be skipped.
        if (IsEligibleForDisposal(boundary)) {
          DisposalQueue.Enqueue(boundary, boundary.TreeDepth);
          discoveredCount++;
        }
      }
      HXComposeProfiling.TrackDisposalDiscovery(discoveredCount);

#if ENABLE_PROFILER
      using (HXComposeProfiling.RecompositionMarker.Auto()) {
#endif
        try {
          IsProcessing = true;
          RecompositionDepth = 0; // Maybe conflicting, but I don't wanna hardlock errored states
          var maxIterations = 1024;
          while (Dirty.TryDequeue(out var boundary) && maxIterations-- > 0) {
            HXComposeProfiling.TrackToplevelRecomposition();
            Recompose(boundary);
          }
          if (maxIterations == 0) Debug.LogWarning("Maximum recomposition iterations reached.");
        } finally {
          IsProcessing = false;
          IsScoped = false;
        }
#if ENABLE_PROFILER
      }
      HXComposeProfiling.TrackActive();
#endif

      if (AutoDisposeOrphans) DisposeOrphanedBoundaries();

      if (!_processRequested) return;
      _processRequested = false;
      IsScoped = false;
      ProcessDirty();
    }

    private static void Recompose(IBoundary boundary) {
      var previousBoundary = CurrentBoundary;
      try {
        RecompositionDepth++;
        if (RecompositionDepth > MaxRecompositionDepth) {
          RecompositionDepth = 0;
          throw new InvalidOperationException(
            $"Maximum recomposition depth of {MaxRecompositionDepth} exceeded. This may indicate an infinite loop in the composition logic."
          );
        }
        CurrentBoundary = boundary;
        boundary.Recompose();
      } catch (Exception e) {
        Debug.LogException(e);
      } finally {
        CurrentBoundary = previousBoundary;
        RecompositionDepth--;
      }
    }

    public static void Poll() {
      if (!UseEventLoop) throw new InvalidOperationException("NotificationScope is not using event loop");
      ProcessDirty();
    }

    public static RecompositionScope BeginBatch() {
      HXComposeProfiling.TrackBatchRequest();
      if (IsScoped) return new RecompositionScope(false);
      IsScoped = true;
      // Originally, I checked IsProcessing here, but that clashed sometimes with events.
      // Generally, mostly nothing should happen, so I'll just allow it unless I find major issues.
      return new RecompositionScope(!UseEventLoop);
    }
  }
}