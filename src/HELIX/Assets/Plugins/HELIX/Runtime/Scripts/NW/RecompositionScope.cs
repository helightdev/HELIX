using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace HELIX.NW {
  public readonly struct RecompositionScope : IDisposable {
    public static bool UseEventLoop = false;

    private static readonly IndexedReferencePriorityQueue<IBoundary, int> _dirty = new();
    private static readonly Dictionary<int, ContextData> _context = new();
    private static bool _isScoped = false;
    private static bool _isProcessing = false;

    private static readonly ProfilerMarker _marker = new("HELIX.NW.Recomposition");
    private static readonly ProfilerMarker _populateContext = new("HELIX.NW.PopulateContext");

    public static void MarkDirty(IBoundary boundary) {
      if (!_isScoped) {
        using (Auto()) {
          _dirty.Enqueue(boundary, boundary.TreeDepth);
          return;
        }
      }

      if (_isProcessing) {
        boundary.Recompose();
        return;
      }

      _dirty.Enqueue(boundary, boundary.TreeDepth);
    }

    public static void MarkClean(IBoundary boundary) {
      _dirty.Remove(boundary);
    }

    private static void PopulateContext(IBoundary boundary) {
#if ENABLE_PROFILER
      using (_populateContext.Auto()) {
#endif
        _context.Clear();
        for (var b = boundary; b != null; b = b.Parent) {
          // This does not overwrite existing keys, so the closest ancestor's context takes precedence
          b.ContributeContext(_context);
        }
#if ENABLE_PROFILER
      }
#endif
    }

    internal static void PutContext(int keyId, ContextData data) => _context[keyId] = data;

    internal static void PutPrevious(int keyId, ContextData data, bool existed) {
      if (existed) _context[keyId] = data;
      else _context.Remove(keyId);
    }

    public static bool TryGetContext(int keyId, out ContextData data) => _context.TryGetValue(keyId, out data);

    public static bool TryGetContext<T>(ContextKey<T> key, out ContextData<T> data) {
      _context.TryGetValue(key.id, out var value);
      if (value is ContextData<T> typed) {
        data = typed;
        return true;
      }
      data = null;
      return false;

    }

    private static void ProcessDirty() {
      if (_isProcessing) throw new InvalidOperationException("NotificationScope is already processing rebuilds");

#if ENABLE_PROFILER
      using (_marker.Auto()) {
#endif
        try {
          _isProcessing = true;
          var maxIterations = 1024;
          while (_dirty.TryDequeue(out var boundary) && maxIterations-- > 0) {
            try {
              PopulateContext(boundary.Parent); // Resume context from parents
              boundary.Recompose();
            } catch (Exception e) {
              Debug.LogException(e);
            }
          }
          if (maxIterations == 0) Debug.LogWarning("Maximum recomposition iterations reached.");
        } finally {
          _isProcessing = false;
          _isScoped = false;
        }
#if ENABLE_PROFILER
      }
#endif
    }

    private readonly bool _hasClaimed;

    private RecompositionScope(bool hasClaimed) {
      _hasClaimed = hasClaimed;
    }

    public void Dispose() {
      if (_hasClaimed) ProcessDirty();
    }

    public static RecompositionScope Auto() {
      if (_isProcessing) throw new InvalidOperationException("NotificationScope is processing rebuilds");
      if (_isScoped) return new RecompositionScope(false);
      _isScoped = true;
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