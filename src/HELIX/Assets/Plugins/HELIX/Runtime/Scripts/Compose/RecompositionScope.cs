using System;
using System.Collections.Generic;
using HELIX.Compose.Collections;
using Unity.Profiling;
using UnityEngine;

namespace HELIX.Compose {
  public readonly struct RecompositionScope : IDisposable {
    public static bool UseEventLoop = false;

    internal static readonly IndexedReferencePriorityQueue<IBoundary, int> Dirty = new();
    internal static readonly HashSet<IBoundary> Boundaries = new(new ReferenceEqualityComparer<IBoundary>());
    internal static bool IsScoped = false;
    internal static bool IsProcessing = false;
    internal static IBoundary CurrentBoundary = null;

    private static readonly ProfilerMarker _marker = new(HXProfiling.HelixCategory, "Recomposition");

    private static readonly ProfilerMarker _populateContext = new(
      HXProfiling.HelixCategory,
      "Populate Context"
    );

    public static void RegisterBoundary(IBoundary boundary) {
      Boundaries.Add(boundary);
    }

    public static void UnregisterBoundary(IBoundary boundary) {
      Dirty.Remove(boundary);
      Boundaries.Remove(boundary);
    }

    public static void MarkDirty(IBoundary boundary) {
      if (!IsScoped) {
        using (Auto()) {
          Dirty.Enqueue(boundary, boundary.TreeDepth);
          return;
        }
      }

      // // If we descend the tree forward, we can reuse the same context and avoid dictionary initialization.
      // if (IsProcessing && CurrentBoundary == boundary.Parent) {
      //   _inlinedRecompositionCount.Value++;
      //   _inlinedRecompositionCount.Sample();
      //   Recompose(boundary);
      //   return;
      // }
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

    public static void DirtyChildren(IBoundary boundary) {
      using (Auto()) {
        foreach (var current in Boundaries) {
          if (current.Parent == boundary) {
            Dirty.Enqueue(current, current.TreeDepth);
          }
        }
      }
    }

    public static void PopulateContext(IBoundary boundary, Dictionary<int, ContextData> context) {
#if ENABLE_PROFILER
      using (_populateContext.Auto()) {
#endif
        context.Clear();
        for (var b = boundary; b != null; b = b.Parent) {
          // This does not overwrite existing keys, so the closest ancestor's context takes precedence
          b.ContributeContext(context);
        }
#if ENABLE_PROFILER
      }
#endif
    }

    private static void ProcessDirty() {
      if (IsProcessing) throw new InvalidOperationException("NotificationScope is already processing rebuilds");

      foreach (var boundary in Boundaries) {
        boundary.CheckModified();
      }

#if ENABLE_PROFILER
      using (_marker.Auto()) {
#endif
        try {
          IsProcessing = true;
          var maxIterations = 1024;
          while (Dirty.TryDequeue(out var boundary) && maxIterations-- > 0) {
            HXProfiling.TrackToplevelRecomposition();
            Recompose(boundary);
          }
          if (maxIterations == 0) Debug.LogWarning("Maximum recomposition iterations reached.");
        } finally {
          IsProcessing = false;
          IsScoped = false;
        }
#if ENABLE_PROFILER
      }
      HXProfiling.TrackActive();
#endif
    }

    private static void Recompose(IBoundary boundary) {
      var previousBoundary = CurrentBoundary;
      try {
        CurrentBoundary = boundary;
        boundary.Recompose();
      } catch (Exception e) {
        Debug.LogException(e);
      } finally {
        CurrentBoundary = previousBoundary;
      }
    }

    private readonly bool _hasClaimed;

    private RecompositionScope(bool hasClaimed) {
      _hasClaimed = hasClaimed;
    }

    public void Dispose() {
      if (_hasClaimed) ProcessDirty();
    }

    public static RecompositionScope Auto() {
      if (IsProcessing) throw new InvalidOperationException("NotificationScope is processing rebuilds");
      if (IsScoped) return new RecompositionScope(false);
      IsScoped = true;
      return new RecompositionScope(!UseEventLoop);
    }

    public static void Poll() {
      if (!UseEventLoop) throw new InvalidOperationException("NotificationScope is not using event loop");
      ProcessDirty();
    }
  }
}