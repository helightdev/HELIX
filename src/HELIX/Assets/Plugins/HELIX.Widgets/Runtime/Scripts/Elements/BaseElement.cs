using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Extensions;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Utilities;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    public abstract class BaseElement : VisualElement, IElement {
        public static readonly string UssClassName = "helix-widget";
        private IdentityDictionary<ThemeProperty, ThemeValue> _themeValues;
        private List<ElementFactorySlot> _widgetFactorySlots;

        protected BaseElement() {
            AddToClassList(UssClassName);
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        public int HierarchyDepth { get; protected set; } = -1;

        public ThemeProviderElement ThemeProviderElement { get; private set; }

        public VisualElement Element => this;

        ~BaseElement() {
            if (this is IHierarchyDisposable disposable) disposable.Dispose();
        }

        protected ThemeValue<T> ThemeValue<T>(BaseThemeProperty<T> property) {
            if (_themeValues != null && _themeValues.TryGetValue(property, out var existing))
                return (ThemeValue<T>)existing;
            var themeValue = new ThemeValue<T>(this, property);
            themeValue.OnValueChanged += value => OnWatchedThemeUpdated(property, value);
            return RegisterThemeValue(themeValue);
        }

        protected ThemeValue<T> ThemeValue<T>(
            BaseThemeProperty<T> property,
            ThemeValue<T>.OnValueChangedDelegate onValueChanged,
            bool applyCurrentValue = true
        ) {
            var themeValue = ThemeValue(property);
            themeValue.OnValueChanged += onValueChanged;
            if (themeValue.ThemeValueState != ThemeValueState.None && applyCurrentValue)
                onValueChanged(themeValue.Value);

            return themeValue;
        }

        protected bool ContainsThemeValue(ThemeProperty property) {
            return _themeValues != null && _themeValues.ContainsKey(property);
        }

        protected void RemoveThemeValue(ThemeValue themeValue) {
            _themeValues?.Remove(themeValue.ThemeProperty);
        }

        protected void RemoveThemeValue(ThemeProperty property) {
            _themeValues?.Remove(property);
        }

        protected virtual ThemeValue<T> RegisterThemeValue<T>(ThemeValue<T> themeValue) {
            _themeValues ??= new IdentityDictionary<ThemeProperty, ThemeValue>();
            _themeValues[themeValue.ThemeProperty] = themeValue;
            themeValue.ReloadStyles();
            return themeValue;
        }

        protected ElementFactorySlot<T> WidgetFactorySlot<T>(
            ElementFactorySlot<T>.OnElementCreatedDelegate onCreated = null,
            ElementFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null,
            ElementFactory<T> fallback = null
        ) where T : VisualElement {
            _widgetFactorySlots ??= new List<ElementFactorySlot>();

            var slot = new ElementFactorySlot<T>(this);
            if (fallback != null) slot.SetFallback(fallback);
            _widgetFactorySlots.Add(slot);
            if (onCreated != null) slot.OnElementCreated += onCreated;
            if (onDestroyed != null) slot.OnElementDestroyed += onDestroyed;
            if (panel != null) slot.TryCreate();
            return slot;
        }

        protected ElementFactorySlot<T> WidgetFactorySlot<T>(
            BaseThemeProperty<ElementFactory<T>> property,
            ElementFactorySlot<T>.OnElementCreatedDelegate onCreated = null,
            ElementFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null,
            ElementFactory<T> fallback = null
        ) where T : VisualElement {
            _widgetFactorySlots ??= new List<ElementFactorySlot>();

            var slot = new ElementFactorySlot<T>(this, property);
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

        public bool DeleteFactorySlot(ElementFactorySlot slot) {
            if (_widgetFactorySlots == null) return false;
            if (!_widgetFactorySlots.Remove(slot)) return false;
            slot.Destroy();
            return true;
        }

        protected virtual void OnAttached(AttachToPanelEvent evt) {
            HierarchyDepth = this.GetDepth();
            ThemeProviderElement = ThemeProviderElement.Get(this);
            if (ThemeProviderElement != null) ThemeProviderElement.OnThemeUpdated += OnThemeUpdated;
            OnThemeUpdated();

            if (this is IHierarchyDisposable disposable) ModificationBarrier.RemoveHierarchyDisposable(disposable);
        }

        protected virtual void OnDetached(DetachFromPanelEvent evt) {
            if (ThemeProviderElement != null) ThemeProviderElement.OnThemeUpdated -= OnThemeUpdated;
            ThemeProviderElement = null;
            if (this is IHierarchyDisposable disposable) ModificationBarrier.TryDisposeHierarchyDisposable(disposable);
        }

        protected virtual void OnThemeUpdated() {
            if (_themeValues != null)
                foreach (var value in _themeValues.Values)
                    value.ReloadStyles();
            if (_widgetFactorySlots != null) {
                foreach (var factorySlot in _widgetFactorySlots) {
                    factorySlot.ApplyReferenceFromTheme();
                    factorySlot.TryCreate();
                }
            }
        }

        protected virtual void OnWatchedThemeUpdated(ThemeProperty property, object value) { }
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