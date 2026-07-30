using System.Collections.Generic;
using System.Diagnostics;
using HELIX.Compose;
using HELIX.Compose.Collections;
using Unity.Profiling;

namespace HELIX {
  // ReSharper disable once InconsistentNaming
  public static class HX {
    public static IReadOnlyCollection<IBoundary> Boundaries => RecompositionScope.Boundaries;

    public static bool InBatchScope => RecompositionScope.IsScoped;
    public static bool IsProcessing => RecompositionScope.IsProcessing;
    public static IBoundary ComposingBoundary => RecompositionScope.CurrentBoundary;

    public static RecompositionScope BatchScope() {
      return RecompositionScope.Auto();
    }
  }

  public struct HXOptional<T> {
    public readonly T value;
    public readonly bool hasValue;

    public HXOptional(T value) : this() {
      this.value = value;
      this.hasValue = true;
    }

    public HXOptional(T value, bool hasValue) {
      this.value = value;
      this.hasValue = hasValue;
    }

    public static readonly HXOptional<T> None = new(default, false);
    public static implicit operator HXOptional<T>(T value) => new(value, true);
  }

  public static class HXProfiling {
    public static readonly ProfilerCategory HelixCategory = new("HELIX", ProfilerCategoryColor.UI);
    public static readonly ProfilerMarker LookupContextMarker = new(HelixCategory, "Lookup Context");

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

    [Conditional("ENABLE_PROFILER")]
    public static void TrackActive() {
      _activeScmCount.Value = SparseContextMap.Pool.CountActive;
      _activeLookupCacheCount.Value = LookupCache.Pool.CountActive;

      _activeBoundariesCount.Value = RecompositionScope.Boundaries.Count;
      _activeBoundariesCount.Sample();
    }

    [Conditional("ENABLE_PROFILER")]
    public static void TrackBatchRequest(int count = 1) => _batchRequestCount.Value += count;

    [Conditional("ENABLE_PROFILER")]
    public static void TrackToplevelRecomposition(int count = 1) => _toplevelRecompositionCount.Value += count;

    [Conditional("ENABLE_PROFILER")]
    public static void TrackHierarchyDeletion(int count = 1) => _hierarchyDeletions.Value += count;

    [Conditional("ENABLE_PROFILER")]
    public static void TrackHierarchyMovement(int count = 1) => _hierarchyMovements.Value += count;
  }
}