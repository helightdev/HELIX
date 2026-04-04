using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HELIX.Widgets {
    public static class HierarchyModificationBarrier {
        public static bool Barrier = false;
        public static bool IsFinalizing = false;
        private static readonly HashSet<IHierarchyDisposable> _hierarchyDisposables = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run(Action action) {
            if (IsFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            if (Barrier) { action(); } else {
                try {
                    Barrier = true;
                    action();
                } finally {
                    IsFinalizing = true;
                    Sweep();
                    IsFinalizing = false;
                    Barrier = false;
                }
            }
        }

        public static void EnqueueHierarchyDisposable(IHierarchyDisposable disposable) {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (IsFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            _hierarchyDisposables.Add(disposable);
        }

        public static void RemoveHierarchyDisposable(IHierarchyDisposable disposable) {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (IsFinalizing) throw new InvalidOperationException("Cannot modify hierarchy while finalizing.");
            _hierarchyDisposables.Remove(disposable);
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