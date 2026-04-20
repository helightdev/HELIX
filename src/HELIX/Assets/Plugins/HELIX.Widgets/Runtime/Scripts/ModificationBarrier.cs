using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Abstractions;
using HELIX.Widgets.Utilities;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace HELIX.Widgets {
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
        } // ReSharper disable Unity.PerformanceAnalysis
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
        // ReSharper disable Unity.PerformanceAnalysis
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

        public static void Rebuild(IWidgetElement element) {
            Run(() => { EnqueueRebuild(element); });
        }

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

    public interface IHierarchyDisposable : IDisposable, IElement { }
}