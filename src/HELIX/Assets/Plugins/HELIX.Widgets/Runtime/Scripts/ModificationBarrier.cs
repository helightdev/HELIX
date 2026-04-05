using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Widgets.Utilities;
using UnityEngine;

namespace HELIX.Widgets {
    public static class ModificationBarrier {
        public static bool Barrier = false;
        public static bool IsFinalizing = false;
        private static readonly HashSet<IHierarchyDisposable> _hierarchyDisposables = new();
        private static readonly IndexedReferencePriorityQueue<IWidgetElement, int> _pendingRebuilds = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run(Action action, int depth = 0) {
            if (IsFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            if (Barrier) {
                action();
                return;
            }

            try {
                Barrier = true;
                action();
            } finally {
                IsFinalizing = true;
                Sweep();
                IsFinalizing = false;
                Barrier = false;
                RunTail(depth);
            }
        }

        private static void RunTail(int depth) {
            if (_pendingRebuilds.Count <= 0) return;
            if (depth > 1024) {
                Debug.LogError("Exceeded maximum hierarchy modification depth. Possible infinite loop detected.");
            } else {
                var element = _pendingRebuilds.Dequeue();
                Debug.Log("Running pending rebuild for element " + element);
                Run(element.Rebuild, depth + 1);
            }
        }

        public static void EnqueueHierarchyDisposable(IHierarchyDisposable disposable) {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (IsFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            _hierarchyDisposables.Add(disposable);
        }

        public static void EnqueueRebuild(IWidgetElement element) {
            if (element == null) throw new ArgumentNullException(nameof(element));
            _pendingRebuilds.Enqueue(element, element.HierarchyDepth);
        }

        public static void RemoveHierarchyDisposable(IHierarchyDisposable disposable) {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (IsFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            _hierarchyDisposables.Remove(disposable);
        }

        public static void RemoveRebuild(IWidgetElement element) {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (_pendingRebuilds.Remove(element)) { Debug.Log($"Removed pending rebuild for element {element}"); }
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