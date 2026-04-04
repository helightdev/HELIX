using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using HELIX.Extensions;
using HELIX.Types;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Descriptors {
    public abstract class Widget {
        public Key key;
        protected readonly HashSet<Modifier> modifiers = new();

        public IReadOnlyList<Modifier> Modifiers {
            set {
                foreach (var modifier in value) { AddModifier(modifier); }
            }
        }

        public abstract IWidgetElement CreateElement();
        public HashSet<Modifier> GetModifiers() => modifiers;

        public void AddModifier(Modifier modifier) {
            if (modifiers.TryGetValue(modifier, out var existing)) {
                if (existing.isFallback) { modifiers.Remove(existing); } else {
                    Debug.LogWarning(
                        $"Modifier of type {modifier.GetType().Name} already exists on widget with key {key}. Modifiers must be unique per widget."
                    );
                    return;
                }
            }

            modifiers.Add(modifier);
        }
    }

    public readonly struct BuildContext {
        public readonly BaseElement element;

        public BuildContext(BaseElement element) {
            this.element = element;
        }
    }

    public interface IWidgetElement {
        VisualElement Element { get; }
        Widget Descriptor { get; }
        bool Reconcile(Widget updated);
    }

    public class HostElement : IWidgetElement {
        public VisualElement Element { get; }
        public Widget Descriptor { get; }

        public HostElement(VisualElement element) {
            Element = element;
            Descriptor = new HostElementDescriptor(this);
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
        void Update(IEnumerable<IWidgetElement> updated);
    }

    public class HierarchyDescriptionCollection : IWidgetElementCollection {
        public readonly VisualElement element;

        public HierarchyDescriptionCollection(VisualElement element) {
            this.element = element;
        }

        public IEnumerable<IWidgetElement> Elements => element.Children().Select(DefaultReconciler.ExpandElement);

        public void Update(IEnumerable<IWidgetElement> updated) {
            element.Clear();
            foreach (var e in updated) { element.Add(e.Element); }
        }
    }

    public static class DefaultReconciler {
        public static bool CanReuse(Widget previous, Widget current) {
            if (previous == null || current == null) return false;
            return previous.key == current.key && previous.GetType() == current.GetType();
        }

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
            if (descriptor == null) {
                parent.Clear();
                return;
            }

            IWidgetElement element = null;
            var child = parent.Children().FirstOrDefault();
            if (child != null) element = ExpandElement(child);

            if (element != null && CanReuse(element.Descriptor, descriptor)) {
                if (element.Reconcile(descriptor)) return;
            }

            var newElement = descriptor.CreateElement();
            parent.Clear();
            parent.Add(newElement.Element);
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

            if (element != null && CanReuse(element.Descriptor, descriptor)) {
                if (element.Reconcile(descriptor)) return;
            }

            var newElement = descriptor.CreateElement();
            if (element != null) parent.RemoveAt(keep);
            parent.Add(newElement.Element);
        }

        public static void ReconcileCollection(IWidgetElementCollection collection, IReadOnlyList<Widget> descriptors) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (descriptors == null) throw new ArgumentNullException(nameof(descriptors));

            var existing = collection.Elements?.ToList() ?? new List<IWidgetElement>();

            var keyed = new Dictionary<Key, IWidgetElement>(existing.Count);
            var unkeyed = new Queue<IWidgetElement>();

            for (var i = 0; i < existing.Count; i++) {
                var element = existing[i];
                var descriptor = element?.Descriptor;

                if (descriptor == null) { continue; }

                if (!descriptor.key.IsNone) {
                    if (!keyed.TryAdd(descriptor.key, element)) {
                        throw new InvalidOperationException(
                            $"Duplicate existing key '{descriptor.key}' in descriptor collection."
                        );
                    }
                } else { unkeyed.Enqueue(element); }
            }

            var updated = new List<IWidgetElement>(descriptors.Count);
            var isDirty = descriptors.Count != existing.Count;
            for (var i = 0; i < descriptors.Count; i++) {
                var descriptor = descriptors[i];
                if (descriptor == null) { throw new InvalidOperationException($"Descriptor at index {i} is null."); }

                IWidgetElement resolved = null;

                if (!descriptor.key.IsNone) {
                    if (keyed.Remove(descriptor.key, out var existingElement)) {
                        if (CanReuse(existingElement.Descriptor, descriptor)) {
                            if (existingElement.Reconcile(descriptor)) resolved = existingElement;
                        }
                    }
                } else {
                    while (unkeyed.Count > 0) {
                        var candidate = unkeyed.Dequeue();
                        if (!CanReuse(candidate.Descriptor, descriptor)) continue;
                        if (candidate.Reconcile(descriptor)) resolved = candidate;
                        break;
                    }
                }

                resolved ??= descriptor.CreateElement();
                if (!isDirty && resolved != existing[i]) isDirty = true;

                updated.Add(resolved);
            }

            if (isDirty) collection.Update(updated);
        }
    }

    public readonly struct Key : IEquatable<Key> {
        private readonly ulong _value;
        public static readonly Key None = new Key(0);

        public Key(ulong value) {
            _value = value;
        }

        public bool IsNone => _value == 0;

        public bool Equals(Key other) => _value == other._value;
        public override bool Equals(object obj) => obj is Key other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Key a, Key b) => a._value == b._value;
        public static bool operator !=(Key a, Key b) => a._value != b._value;

        public override string ToString() => IsNone ? "None" : _value.ToString();

        public static implicit operator Key(ulong value) => new(value);
        public static implicit operator Key(uint value) => new(value);
        public static implicit operator Key(int value) => new(unchecked((uint)value));
        public static implicit operator Key(string value) => new(HashString(value));

        private static ulong HashString(string value) {
            var source = Encoding.UTF8.GetBytes(value);
            return XxHash3.HashToUInt64(source);
        }
    }
}