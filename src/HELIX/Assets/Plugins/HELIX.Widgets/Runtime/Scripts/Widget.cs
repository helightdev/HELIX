using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class Widget : IWidgetListCandidate {
        public Key key;
        public object[] constants;
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

        public T GetThemed<T>(ThemeProperty<T> property) {
            return element.ThemeProvider.Resolve(property);
        }
    }

    public interface IWidgetElement {
        VisualElement Element { get; }
        Widget Descriptor { get; }
        int HierarchyDepth { get; }
        bool CanReconcile(Widget updated);
        bool Reconcile(Widget updated);

        void Rebuild() {
            if (Descriptor != null) Reconcile(Descriptor);
        }
    }
}