using System.Diagnostics;
using Unity.Profiling;

namespace HELIX.Compose {
  public static class HXComposeProfiling {
    public static readonly ProfilerCategory HelixCategory = new("HELIX", ProfilerCategoryColor.UI);
    public static readonly ProfilerMarker LookupContextMarker = new(HelixCategory, "Lookup Context");
    public static readonly ProfilerMarker RecompositionMarker = new(HelixCategory, "Recomposition");
    public static readonly ProfilerMarker DisposeOrphansMarker = new(HelixCategory, "Dispose Orphans");

    private static readonly ProfilerCounterValue<int> _activeScmCount = new(
      HelixCategory,
      "Active SCMs",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame
    );

    private static readonly ProfilerCounterValue<int> _activeLookupCacheCount = new(
      HelixCategory,
      "Active LookupCaches",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame
    );

    private static readonly ProfilerCounterValue<int> _toplevelRecompositionCount = new(
      HelixCategory,
      "Recompositions",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _composableResets = new(
      HelixCategory,
      "Composable Resets",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _hierarchyMovements = new(
      HelixCategory,
      "Hierarchy Movements",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _hierarchyDeletions = new(
      HelixCategory,
      "Hierarchy Deletions",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _batchRequestCount = new(
      HelixCategory,
      "Batch Requests",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _activeBoundariesCount = new(
      HelixCategory,
      "Active Boundaries",
      ProfilerMarkerDataUnit.Count
    );

    private static readonly ProfilerCounterValue<int> _disposedBoundariesCount = new(
      HelixCategory,
      "Disposed Boundaries",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );

    private static readonly ProfilerCounterValue<int> _disposalDiscoveredCount = new(
      HelixCategory,
      "Disposal Discovered",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );
    private static readonly ProfilerCounterValue<int> _disposalDiscardedCount = new(
      HelixCategory,
      "Disposal Discarded",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
    );


    [Conditional("ENABLE_PROFILER")]
    public static void TrackActive() {
      _activeScmCount.Value = SparseContextMap.Pool.CountActive;
      _activeLookupCacheCount.Value = LookupCache.Pool.CountActive;

      _activeBoundariesCount.Value = HXComposer.Boundaries.Count;
      _activeBoundariesCount.Sample();
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackBatchRequest(int count = 1) {
      _batchRequestCount.Value += count;
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackToplevelRecomposition(int count = 1) {
      _toplevelRecompositionCount.Value += count;
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackHierarchyDeletion(int count = 1) {
      _hierarchyDeletions.Value += count;
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackHierarchyMovement(int count = 1) {
      _hierarchyMovements.Value += count;
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackComposableReset(int count = 1) {
      _composableResets.Value += count;
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackDisposalDiscovery(int discovered) {
      _disposalDiscoveredCount.Value += discovered;
      // if (discovered > 0) Debug.Log($"Discovered {discovered} boundaries for disposal at frame {Time.frameCount}");
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackDisposalStats(int discarded, int disposed) {
      _disposalDiscardedCount.Value += discarded;
      _disposedBoundariesCount.Value += disposed;
      // if (discarded + disposed > 0) Debug.Log($"Discarded {discarded} and disposed {disposed} boundaries at frame {Time.frameCount}");
    }
  }
}