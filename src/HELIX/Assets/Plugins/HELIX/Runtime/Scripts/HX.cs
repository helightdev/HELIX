using System.Collections.Generic;
using HELIX.Compose;
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

    public static RecompositionScope DirtyScope(IBoundary boundary) {
      var scope = BatchScope();
      boundary.MarkDirty();
      return scope;
    }
    public static void Dirty(IBoundary boundary) => RecompositionScope.MarkDirty(boundary);

    public static void Dirty<T>(BoundaryComposable<T> composable)
      where T : BoundaryData => RecompositionScope.MarkDirty(composable.Node);
  }

  public static class HelixProfiling {
    public static ProfilerCategory HelixCategory = new("HELIX", ProfilerCategoryColor.UI);
  }
}