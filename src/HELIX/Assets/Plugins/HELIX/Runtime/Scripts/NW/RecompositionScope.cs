using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace HELIX.NW {
  public readonly struct RecompositionScope : IDisposable {
    public static bool UseEventLoop = false;

    internal static readonly IndexedReferencePriorityQueue<IBoundary, int> Dirty = new();
    internal static readonly Dictionary<int, ContextData> SharedContext = new();
    internal static readonly Dictionary<int, ContextData> Context = new();
    internal static readonly HashSet<IBoundary> Boundaries = new(new ReferenceEqualityComparer<IBoundary>());
    internal static bool IsScoped = false;
    internal static bool IsProcessing = false;
    internal static IBoundary CurrentBoundary = null;

    private static readonly ProfilerMarker _marker = new(HelixProfiling.HelixCategory, "Recomposition");

    private static readonly ProfilerMarker _populateContext = new(
      HelixProfiling.HelixCategory,
      "Populate Context"
    );

    private static readonly ProfilerCounterValue<int> _toplevelRecompositionCount = new(
      HelixProfiling.HelixCategory,
      "Recompositions",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _inlinedRecompositionCount = new(
      HelixProfiling.HelixCategory,
      "Inlined Recompositions",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _activeBoundariesCount = new(
      HelixProfiling.HelixCategory,
      "Active Boundaries",
      ProfilerMarkerDataUnit.Count
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

    private static void PopulateContext(IBoundary boundary) {
#if ENABLE_PROFILER
      using (_populateContext.Auto()) {
#endif
        Context.Clear();
        for (var b = boundary; b != null; b = b.Parent) {
          // This does not overwrite existing keys, so the closest ancestor's context takes precedence
          b.ContributeContext(Context);
        }
#if ENABLE_PROFILER
      }
#endif
    }

    internal static void PutContext(int keyId, ContextData data) => Context[keyId] = data;

    internal static void RestoreContext(int keyId, ContextData data, bool existed) {
      if (existed) Context[keyId] = data;
      else Context.Remove(keyId);
    }

    public static bool TryGetContext(int keyId, out ContextData data) {
      if (Context.TryGetValue(keyId, out data)) return true;
      return SharedContext.TryGetValue(keyId, out data);
    }

    public static bool TryGetContext<T>(ContextKey<T> key, out ContextData<T> data) {
      Context.TryGetValue(key.id, out var value);
      if (value is ContextData<T> typed) {
        data = typed;
        return true;
      }
      SharedContext.TryGetValue(key.id, out value);
      if (value is ContextData<T> sharedTyped) {
        data = sharedTyped;
        return true;
      }
      data = null;
      return false;
    }

    private static void ProcessDirty() {
      if (IsProcessing) throw new InvalidOperationException("NotificationScope is already processing rebuilds");

      foreach (var boundary in Boundaries) {
        boundary.CheckModified();
      }

#if ENABLE_PROFILER
      SparseContextMap.TrackProfiling();

      using (_marker.Auto()) {
#endif
        try {
          IsProcessing = true;
          var maxIterations = 1024;
          while (Dirty.TryDequeue(out var boundary) && maxIterations-- > 0) {
            _toplevelRecompositionCount.Value++;
            PopulateContext(boundary.Parent); // Resume context from parents
            Recompose(boundary);
          }
          if (maxIterations == 0) Debug.LogWarning("Maximum recomposition iterations reached.");
        } finally {
          IsProcessing = false;
          IsScoped = false;
        }
#if ENABLE_PROFILER
      }

      _activeBoundariesCount.Value = Boundaries.Count;
      _activeBoundariesCount.Sample();
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

  public static class RecompositionExtensions {
    public static void MarkDirty(this IBoundary boundary) {
      RecompositionScope.MarkDirty(boundary);
    }
  }
}