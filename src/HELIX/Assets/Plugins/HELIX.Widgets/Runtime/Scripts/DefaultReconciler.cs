using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public static class DefaultReconciler {
        public static bool CanReuse(Widget previous, Widget current) {
            if (previous == null || current == null) return false;
            return previous.key == current.key && previous.GetType() == current.GetType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IWidgetElement ExpandElement(VisualElement child) {
            if (child is IWidgetElement de) return de;
            if (child.userData is IWidgetElement de2) return de2;
            return new HostElement(child);
        }

        public static void ReconcileSingleDirect(
            VisualElement parent,
            Widget descriptor
        ) {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (parent.panel == null)
                throw new InvalidOperationException(
                    "Parent element must be attached to a panel before reconciliation."
                );

            if (descriptor == null) {
                parent.Clear();
                return;
            }

            IWidgetElement element = null;
            if (parent.childCount > 0) { element = ExpandElement(parent.ElementAt(0)); }

            if (element != null) {
                try {
                    if (CanReuse(element.Descriptor, descriptor) && element.CanReconcile(descriptor)) {
                        PerformReconcile(element, descriptor);
                        return;
                    }
                    element.CallUnmounted();
                } catch (Exception ex) {
                    Debug.LogError($"Error reconciling element {element} with descriptor {descriptor}: {ex}");
                }
            }

            try {
                var newElement = descriptor.CreateElement();
                newElement.CallMounted(descriptor);
                parent.Clear();
                parent.Add(newElement.Element);
            } catch (Exception ex) { Debug.LogError($"Error creating element for descriptor {descriptor}: {ex}"); }
        }

        public static void ReconcileSingleDirectKeeping(
            VisualElement parent,
            Widget descriptor,
            int keep
        ) {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (descriptor == null) {
                if (parent.childCount == keep + 1) { parent.RemoveAt(keep); }

                return;
            }

            IWidgetElement element = null;
            if (parent.childCount > keep) {
                var child = parent.ElementAt(keep);
                if (child != null) element = ExpandElement(child);
            }

            if (element != null) {
                try {
                    if (CanReuse(element.Descriptor, descriptor) && element.CanReconcile(descriptor)) {
                        PerformReconcile(element, descriptor);
                        return;
                    }
                    element.CallUnmounted();
                } catch (Exception ex) {
                    Debug.LogError($"Error reconciling element {element} with descriptor {descriptor}: {ex}");
                }
            }

            try {
                var newElement = descriptor.CreateElement();
                if (element != null) parent.RemoveAt(keep);
                newElement.CallMounted(descriptor);
                parent.Add(newElement.Element);
            } catch (Exception ex) { Debug.LogError($"Error creating element for descriptor {descriptor}: {ex}"); }
        }

        private static readonly Dictionary<Key, IWidgetElement> _keyedScratch = new();
        private static readonly Queue<IWidgetElement> _unkeyedScratch = new();
        private static readonly Queue<ReconcilerEntry> _reconcileScratch = new();
        private static readonly List<IWidgetElement> _resultScratch = new();
        private static readonly List<ReconcilerCollectionDelta> _deltaScratch = new();
        private static readonly List<IWidgetElement> _existingScratch = new();
        private static bool _isProcessingQueue = false;

        public static void ReconcileCollection(IWidgetElementCollection collection, IReadOnlyList<Widget> descriptors) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (descriptors == null) throw new ArgumentNullException(nameof(descriptors));
            _keyedScratch.Clear();
            _unkeyedScratch.Clear();
            _resultScratch.Clear();
            _deltaScratch.Clear();
            _existingScratch.Clear();

            collection.FillElements(_existingScratch);

            for (var i = 0; i < _existingScratch.Count; i++) {
                var element = _existingScratch[i];
                var descriptor = element?.Descriptor;

                if (descriptor == null) { continue; }

                if (!descriptor.key.IsNone) {
                    if (!_keyedScratch.TryAdd(descriptor.key, element)) {
                        throw new InvalidOperationException(
                            $"Duplicate existing key '{descriptor.key}' in descriptor collection."
                        );
                    }
                } else { _unkeyedScratch.Enqueue(element); }
            }

            var isDirty = descriptors.Count != _existingScratch.Count;
            for (var i = 0; i < descriptors.Count; i++) {
                var descriptor = descriptors[i];
                if (descriptor == null) { throw new InvalidOperationException($"Descriptor at index {i} is null."); }

                IWidgetElement resolved = null;

                if (!descriptor.key.IsNone) {
                    if (_keyedScratch.Remove(descriptor.key, out var existingElement)) {
                        if (CanReuse(existingElement.Descriptor, descriptor)) {
                            if (existingElement.CanReconcile(descriptor)) {
                                resolved = existingElement;
                                _reconcileScratch.Enqueue(
                                    new ReconcilerEntry {
                                        element = existingElement,
                                        descriptor = descriptor
                                    }
                                );
                            }
                        }
                    }
                } else {
                    while (_unkeyedScratch.Count > 0) {
                        var candidate = _unkeyedScratch.Dequeue();
                        if (!CanReuse(candidate.Descriptor, descriptor)) {
                            _deltaScratch.Add(
                                new ReconcilerCollectionDelta {
                                    target = candidate,
                                    added = false
                                }
                            );
                            continue;
                        }

                        if (candidate.CanReconcile(descriptor)) {
                            resolved = candidate;
                            _reconcileScratch.Enqueue(
                                new ReconcilerEntry {
                                    element = candidate,
                                    descriptor = descriptor
                                }
                            );
                        }

                        break;
                    }
                }

                if (resolved == null) {
                    resolved = descriptor.CreateElement();
                    _deltaScratch.Add(
                        new ReconcilerCollectionDelta {
                            target = resolved,
                            added = true
                        }
                    );
                }

                if (!isDirty && resolved != _existingScratch[i]) isDirty = true;

                _resultScratch.Add(resolved);
            }

            if (isDirty) {
                foreach (var element in _unkeyedScratch) {
                    _deltaScratch.Add(
                        new ReconcilerCollectionDelta {
                            target = element,
                            added = false
                        }
                    );
                }

                foreach (var (_, value) in _keyedScratch) {
                    _deltaScratch.Add(
                        new ReconcilerCollectionDelta {
                            target = value,
                            added = false
                        }
                    );
                }

                try {
                    var resultArray = _resultScratch.ToArray();
                    var deltaArray = new ReconcilerCollectionDelta[_deltaScratch.Count];
                    for (var i = 0; i < _deltaScratch.Count; i++) {
                        var current = _deltaScratch[i];
                        if (!current.added) current.target.CallUnmounted();
                        deltaArray[i] = current;
                    }

                    foreach (var current in _deltaScratch) {
                        if (current.added) current.target.CallMounted(current.target.Descriptor);
                    }

                    collection.Update(resultArray, deltaArray);
                } catch (Exception ex) {
                    Debug.LogError(
                        $"Error updating collection {collection} with result {_resultScratch.Count} and deltas {_deltaScratch.Count}: {ex}"
                    );
                }
            }

            if (_isProcessingQueue) return;
            try {
                _isProcessingQueue = true;
                while (_reconcileScratch.TryDequeue(out var next)) {
                    try {
                        PerformReconcile(next.element, next.descriptor); //
                    } catch (Exception ex) {
                        Debug.LogError($"Error reconciling {next.element} with descriptor {next.descriptor}: {ex}");
                    }
                }
            } finally { _isProcessingQueue = false; }
        }

        public static void PerformReconcile(IWidgetElement element, Widget descriptor) {
            var previous = element.Descriptor;
            if (ReferenceEquals(previous, descriptor)) return;
            if (previous.constants != null && descriptor.constants != null) {
                if (previous.constants.SequenceEqual(descriptor.constants)) return;
            }

            element.Reconcile(descriptor);
        }

        private static void CallUnmounted(this IWidgetElement element) {
            element.Descriptor.key.details?.OnUnmounted(element);
        }

        private static void CallMounted(this IWidgetElement element, Widget descriptor) {
            descriptor?.key.details?.OnMounted(element);
        }
    }

    public struct ReconcilerCollectionDelta {
        public IWidgetElement target;
        public bool added;
    }

    public struct ReconcilerEntry {
        public IWidgetElement element;
        public Widget descriptor;
    }

    public class HostElement : IWidgetElement {
        public VisualElement Element { get; }
        public Widget Descriptor { get; }
        public int HierarchyDepth { get; private set; }

        public HostElement(VisualElement element) {
            Element = element;
            HierarchyDepth = element.GetDepth();
            Descriptor = new HostElementDescriptor(this);
        }

        public bool CanReconcile(Widget updated) {
            return updated is HostElementDescriptor hed && hed.host.Element == Element;
        }

        public bool Reconcile(Widget updated) {
            return updated is HostElementDescriptor hed && hed.host.Element == Element;
        }

        public class HostElementDescriptor : Widget {
            public readonly HostElement host;

            public HostElementDescriptor(HostElement host) {
                this.host = host;
                key = "Host" + host.GetHashCode();
            }

            public override IWidgetElement CreateElement() {
                if (host.Element.parent != null) { host.Element.RemoveFromHierarchy(); }

                return host;
            }
        }
    }

    public interface IWidgetElementCollection {
        IEnumerable<IWidgetElement> Elements { get; }
        void FillElements(List<IWidgetElement> elements);
        void Update(IWidgetElement[] result, ReconcilerCollectionDelta[] deltas);
    }

    public readonly struct HierarchyDescriptionCollection : IWidgetElementCollection {
        public readonly VisualElement element;

        public HierarchyDescriptionCollection(VisualElement element) {
            this.element = element;
        }

        public IEnumerable<IWidgetElement> Elements => element.Children().Select(DefaultReconciler.ExpandElement);

        private static readonly Dictionary<VisualElement, int> _lookupHelper = new();

        public void FillElements(List<IWidgetElement> elements) {
            for (var i = 0; i < element.hierarchy.childCount; i++) {
                var child = element.hierarchy.ElementAt(i);
                elements.Add(DefaultReconciler.ExpandElement(child));
            }
        }

        public void Update(IWidgetElement[] updated, ReconcilerCollectionDelta[] deltas) {
            foreach (var delta in deltas) {
                var child = delta.target.Element;
                if (delta.added) {
                    if (child.parent == null) element.hierarchy.Add(child);
                    else
                        Debug.LogWarning(
                            $"Attempted to add element {child} to hierarchy, but it already has a parent. This may indicate a problem with the reconciliation process."
                        );
                } else {
                    if (child.parent == element.contentContainer) child.RemoveFromHierarchy();
                    else
                        Debug.LogWarning(
                            $"Attempted to remove element {child} from hierarchy, but it is not a child of the target element. This may indicate a problem with the reconciliation process."
                        );
                }
            }

            // I would love to use "iterate reverse bring to front" but that also triggers a panel detach
            // All of this is to avoid problematic change events that may break things and interrupt animations
            _lookupHelper.Clear();
            for (var i = 0; i < updated.Length; i++) {
                var targetElement = updated[i].Element;
                _lookupHelper[targetElement] = i;
            }

            element.hierarchy.Sort((a, b) => {
                    var indexA = _lookupHelper.GetValueOrDefault(a, int.MaxValue);
                    var indexB = _lookupHelper.GetValueOrDefault(b, int.MaxValue);
                    return indexA.CompareTo(indexB);
                }
            );
        }
    }
}