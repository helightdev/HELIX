using System.Collections.Generic;
using HELIX.NW;
using Unity.Profiling;

namespace HELIX {
  // ReSharper disable once InconsistentNaming
  public static class HX {
    public static IReadOnlyCollection<IBoundary> Boundaries => RecompositionScope.Boundaries;

    public static bool InBatchScope => RecompositionScope.IsScoped;
    public static IBoundary ComposingBoundary => RecompositionScope.CurrentBoundary;

    public static RecompositionScope BatchScope() {
      return RecompositionScope.Auto();
    }
  }

  public static class HelixProfiling {
    public static ProfilerCategory HelixCategory = new("HELIX", ProfilerCategoryColor.UI);
  }
}