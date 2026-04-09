using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Extensions;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    public abstract class BaseElement : VisualElement, IElement {
        public static readonly string UssClassName = "helix-widget";
        private readonly List<ThemeValue> _themeValues = new();
        private readonly List<WidgetFactorySlot> _widgetFactorySlots = new();

        protected BaseElement() {
            AddToClassList(UssClassName);
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        public int HierarchyDepth { get; protected set; } = -1;

        public WidgetThemeProvider ThemeProvider { get; private set; }

        public VisualElement Element => this;

        ~BaseElement() {
            if (this is IHierarchyDisposable disposable) disposable.Dispose();
        }

        protected ThemeValue<T> ThemeValue<T>(ThemeProperty<T> property) {
            var themeValue = new ThemeValue<T>(this, property);
            return RegisterThemeValue(themeValue);
        }

        protected ThemeValue<T> ThemeValue<T>(
            ThemeProperty<T> property,
            ThemeValue<T>.OnValueChangedDelegate onValueChanged
        ) {
            var themeValue = new ThemeValue<T>(this, property, onValueChanged);
            return RegisterThemeValue(themeValue);
        }

        protected virtual ThemeValue<T> RegisterThemeValue<T>(ThemeValue<T> themeValue) {
            _themeValues.Add(themeValue);
            return themeValue;
        }

        protected WidgetFactorySlot<T> WidgetFactorySlot<T>(
            WidgetFactorySlot<T>.OnElementCreatedDelegate onCreated = null,
            WidgetFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null,
            ElementFactory<T> fallback = null
        ) where T : VisualElement {
            var slot = new WidgetFactorySlot<T>(this);
            if (fallback != null) slot.SetFallback(fallback);
            _widgetFactorySlots.Add(slot);
            if (onCreated != null) slot.OnElementCreated += onCreated;
            if (onDestroyed != null) slot.OnElementDestroyed += onDestroyed;
            if (panel != null) slot.TryCreate();
            return slot;
        }

        protected WidgetFactorySlot<T> WidgetFactorySlot<T>(
            ThemeProperty<ElementFactory<T>> property,
            WidgetFactorySlot<T>.OnElementCreatedDelegate onCreated = null,
            WidgetFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null,
            ElementFactory<T> fallback = null
        ) where T : VisualElement {
            var slot = new WidgetFactorySlot<T>(this, property);
            if (fallback != null) slot.SetFallback(fallback);
            _widgetFactorySlots.Add(slot);
            if (onCreated != null) slot.OnElementCreated += onCreated;
            if (onDestroyed != null) slot.OnElementDestroyed += onDestroyed;
            if (panel != null) {
                slot.ApplyReferenceFromTheme();
                slot.TryCreate();
            }

            return slot;
        }

        public bool DeleteFactorySlot(WidgetFactorySlot slot) {
            if (!_widgetFactorySlots.Remove(slot)) return false;
            slot.Destroy();
            return true;
        }

        protected virtual void OnAttached(AttachToPanelEvent evt) {
            HierarchyDepth = this.GetDepth();
            ThemeProvider = WidgetThemeProvider.Get(this);
            if (ThemeProvider != null) ThemeProvider.OnThemeUpdated += OnThemeUpdated;
            OnThemeUpdated();

            if (this is IHierarchyDisposable disposable) ModificationBarrier.RemoveHierarchyDisposable(disposable);
        }

        protected virtual void OnDetached(DetachFromPanelEvent evt) {
            if (ThemeProvider != null) ThemeProvider.OnThemeUpdated -= OnThemeUpdated;
            ThemeProvider = null;

            if (this is IHierarchyDisposable disposable)
                ModificationBarrier.Run(() => { ModificationBarrier.EnqueueHierarchyDisposable(disposable); });
        }

        protected virtual void OnThemeUpdated() {
            foreach (var value in _themeValues) value.ReloadStyles();

            foreach (var factorySlot in _widgetFactorySlots) {
                factorySlot.ApplyReferenceFromTheme();
                factorySlot.TryCreate();
            }
        }
    }

    public abstract class SingleChildContainerElement : BaseElement, ISingleChildContainer {
        public virtual VisualElement Child {
            get => Children().FirstOrDefault();
            set {
                Clear();
                if (value != null) Add(value);
            }
        }
    }

    public abstract class MultiChildContainerElement : BaseElement, IMultiChildContainer {
        public virtual IEnumerable<VisualElement> Childs {
            get => Children();
            set {
                Clear();
                if (value == null) return;
                foreach (var child in value) Add(child);
            }
        }
    }
}