using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Abstractions;
using HELIX.Widgets.Utilities;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace HELIX.Widgets {
  /// <summary>
  /// <para>
  /// Provides a mechanism for managing and restricting modifications to the widget hierarchy while
  /// ensuring proper handling of callbacks, rebuilds, and disposals during a modification frame.
  /// </para>
  /// <para>
  /// The barrier represents a reentrant context in which <see cref="Reconciler"/> actions, disposals,
  /// and frame callbacks are collected and executed once the outermost modification frame completes.
  /// </para>
  /// </summary>
  public static class ModificationBarrier {
    private static bool _isFinalizing;
    private static bool _insideTail;

    private static readonly HashSet<IHierarchyDisposable> _hierarchyDisposables = new();
    private static readonly IndexedReferencePriorityQueue<IWidgetElement, int> _pendingRebuilds = new();
    private static readonly Queue<Action> _postFrameCallbacks = new();

    public static int MaxCallbacksPerFrame = 512;
    public static int MaxRebuildsPerFrame = 512;
    public static int MaxDisposalsPerFrame = 512;
    public static bool UseRuntimeHelper = false;

    public static bool InsideModification { get; private set; }

    /// <summary>Runs the provided action within the barrier context.</summary>
    /// <param name="action">The action to run within the barrier context.</param>
    /// <exception cref="InvalidOperationException"> When called from within a finalization context.</exception>
    /// <seealso cref="ModificationBarrier"/>
    /// <remarks>
    /// This method is safe to call from within another <see cref="Run"/> or a <see cref="Rebuild"/>, but not from
    /// within a disposal. However, you may enqueue additional disposals from within a disposal callback
    /// or add a post-frame callback.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Run(Action action) {
      if (_isFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
      if (InsideModification) {
        action();
        return;
      }

      try {
        InsideModification = true;
        action();
        RunCallbacks();
      } finally {
        _isFinalizing = true;
        Sweep();
        _isFinalizing = false;
        InsideModification = false;
        RunTail();
      }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private static void RunCallbacks() {
      if (_postFrameCallbacks.Count == 0) return;
      var maxDepth = MaxCallbacksPerFrame;
      while (_postFrameCallbacks.Count > 0 && maxDepth > 0) {
        var callback = _postFrameCallbacks.Dequeue();
        try { callback(); } catch (Exception e) {
          Debug.LogError($"Error while running post-frame callback: {e}");
        } finally { maxDepth--; }
      }

      if (maxDepth != 0) return;
      Debug.LogWarning("Maximum post-frame callback depth reached, delaying until next frame.");
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private static void RunTail() {
      if (_insideTail) return;
      if (_pendingRebuilds.Count == 0) return;
      if (UseRuntimeHelper) {
        try { HelixRuntimeHelper.EnsureRunning(); } catch (Exception e) {
          Debug.LogError($"Error while ensuring runtime helper is running: {e}");
          return;
        }
      }

      try {
        _insideTail = true;
        Profiler.BeginSample("ModificationBarrier.RunRebuilds");
        Run(() => {
            var maxDepth = MaxRebuildsPerFrame;
            var open = _pendingRebuilds.Count;
            while (_pendingRebuilds.TryDequeue(out var element) && maxDepth > 0) {
              try {
                maxDepth--;
                var panel = element.Element.panel;
                if (panel != null) Reconciler.Reconcile(element, element.Descriptor);

                Reconciler.Reconcile(element, element.Descriptor);
              } catch (Exception e) { Debug.LogError($"Error while rebuilding element {element}: {e}"); }
            }

            if (maxDepth == 0) {
              Debug.LogWarning(
                "Maximum rebuild depth reached, delaying remaining rebuilds until next frame."
              );
            }
          }
        );
        Profiler.EndSample();
      } finally { _insideTail = false; }
    }


    /// <summary>
    /// Enqueues a disposable to be disposed at the end of the current frame.
    /// </summary>
    public static void EnqueueHierarchyDisposable(IHierarchyDisposable disposable) {
      if (disposable == null) throw new ArgumentNullException(nameof(disposable));
      _hierarchyDisposables.Add(disposable);
    }

    private static void FutureEnqueueHierarchyDisposable(IHierarchyDisposable disposable) {
      _postFrameCallbacks.Enqueue(() => {
          var element = disposable.Element;
          if (element.parent == null || element.panel == null) EnqueueHierarchyDisposable(disposable);
        }
      );
    }

    /// <summary>
    /// <para>Tries to dispose of the provided disposable at the earliest possible time.</para>
    /// <para>
    /// If currently inside a finalization context, the disposable will be disposed of immediately.
    /// Otherwise, it will be enqueued for disposal at the end of the current frame if <see cref="InsideModification"/>
    /// or at the end of the next future frame.
    /// </para>
    /// </summary>
    public static void TryDisposeHierarchyDisposable(IHierarchyDisposable disposable) {
      if (_isFinalizing) {
        disposable?.Dispose();
        return;
      }

      if (!InsideModification) FutureEnqueueHierarchyDisposable(disposable);
      else EnqueueHierarchyDisposable(disposable);
    }

    private static void EnqueueRebuild(IWidgetElement element) {
      if (element == null) throw new ArgumentNullException(nameof(element));
      _pendingRebuilds.Enqueue(element, element.HierarchyDepth);
    }

    public static void RemoveHierarchyDisposable(IHierarchyDisposable disposable) {
      if (disposable == null) throw new ArgumentNullException(nameof(disposable));
      _hierarchyDisposables.Remove(disposable);
    }

    public static void RemoveRebuild(IWidgetElement element) {
      if (element == null) throw new ArgumentNullException(nameof(element));
      _pendingRebuilds.Remove(element);
    }


    /// <summary>
    /// Enqueues an element for rebuild.
    /// </summary>
    /// <remarks>
    /// Will start a new modification frame using <see cref="Run"/> if not already inside one.
    /// </remarks>
    public static void Rebuild(IWidgetElement element) {
      Run(() => { EnqueueRebuild(element); });
    }

    /// <summary>
    /// Enqueues a callback to be run at the end of the current frame.
    /// </summary>
    /// <param name="callback">The action to run at the end of the current frame.</param>
    /// <exception cref="InvalidOperationException">Thrown if not called from within a modification context.</exception>
    public static void AddPostFrameCallback(Action callback) {
      if (callback == null) throw new ArgumentNullException(nameof(callback));
      if (!InsideModification) {
        throw new InvalidOperationException(
          "Post-frame callbacks can only be added inside a modification context."
        );
      }

      _postFrameCallbacks.Enqueue(callback);
    }

    private static void Sweep() {
      var maxDepth = MaxDisposalsPerFrame;
      while (_hierarchyDisposables.Count > 0 && maxDepth > 0) {
        maxDepth--;
        var disposable = _hierarchyDisposables.First();
        _hierarchyDisposables.Remove(disposable);
        try { disposable.Dispose(); } catch (Exception e) {
          Debug.LogError($"Error while disposing hierarchy disposable: {e}");
        }

        if (disposable is IWidgetElement element) RemoveRebuild(element);
      }

      if (maxDepth == 0) {
        Debug.LogWarning(
          "Maximum hierarchy disposable depth reached, delaying remaining disposals until next frame."
        );
      }
    }
  }

  /// <summary>
  /// Can be implemented by an <see cref="IElement"/> to use the <see cref="ModificationBarrier"/> to dispose of it.
  /// </summary>
  public interface IHierarchyDisposable : IDisposable, IElement { }
}