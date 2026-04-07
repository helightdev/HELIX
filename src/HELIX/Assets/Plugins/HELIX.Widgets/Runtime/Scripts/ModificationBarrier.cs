using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Widgets.Utilities;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace HELIX.Widgets {
    public static class ModificationBarrier {
        private static bool _barrier = false;
        private static bool _isFinalizing = false;
        private static bool _isProcessingRebuilds = false;
        private static readonly HashSet<IHierarchyDisposable> _hierarchyDisposables = new();
        private static readonly IndexedReferencePriorityQueue<IWidgetElement, int> _pendingRebuilds = new();
        
        public static bool InsideModification => _barrier;
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run(Action action) {
            if (_isFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            if (_barrier) {
                action();
                return;
            }

            try {
                _barrier = true;
                action();
            } finally {
                _isFinalizing = true;
                Sweep();
                _isFinalizing = false;
                _barrier = false;
                RunTail();
            }
        }

        private static void RunTail() {
            if (_isProcessingRebuilds) return;
            if (_pendingRebuilds.Count <= 0) return;
            try {
                _isProcessingRebuilds = true;
                Profiler.BeginSample("ModificationBarrier.RunRebuilds");
                Run(() => {
                    while (_pendingRebuilds.TryDequeue(out var element)) {
                        try {
                            if (element.Element.panel != null) element.Rebuild();
                        } catch (Exception e) {
                            Debug.LogError($"Error while rebuilding element {element}: {e}");
                        }
                    } 
                });
                Profiler.EndSample();
                //Debug.Log($"Processed pending rebuilds in {stopwatch.Elapsed.TotalMilliseconds} ms");
            } finally {
                _isProcessingRebuilds = false;
            }
        }

        public static void EnqueueHierarchyDisposable(IHierarchyDisposable disposable) {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (_isFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            _hierarchyDisposables.Add(disposable);
        }

        public static void EnqueueRebuild(IWidgetElement element) {
            if (element == null) throw new ArgumentNullException(nameof(element));
            _pendingRebuilds.Enqueue(element, element.HierarchyDepth);
        }

        public static void RemoveHierarchyDisposable(IHierarchyDisposable disposable) {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (_isFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            _hierarchyDisposables.Remove(disposable);
        }

        public static void RemoveRebuild(IWidgetElement element) {
            if (element == null) throw new ArgumentNullException(nameof(element));
            _pendingRebuilds.Remove(element);
        }

        public static void RunRebuild(IWidgetElement element) {
            Run(() => { EnqueueRebuild(element); });
        }

        private static void Sweep() {
            foreach (var disposable in _hierarchyDisposables) {
                try { disposable.Dispose(); } catch (Exception e) {
                    Debug.LogError($"Error while disposing hierarchy disposable: {e}");
                }
            }

            _hierarchyDisposables.Clear();
        }
    }

    public interface IHierarchyDisposable : IDisposable { }
}