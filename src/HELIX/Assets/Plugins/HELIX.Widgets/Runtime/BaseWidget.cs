using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HELIX.Abstractions;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class BaseWidget : VisualElement {
        public static readonly string UssClassName = "helix-widget";
        private readonly List<ThemeValue> _themeValues = new();
        private readonly List<WidgetFactorySlot> _widgetFactorySlots = new();
        public WidgetThemeProvider ThemeProvider { get; private set; }

        protected BaseWidget() {
            AddToClassList(UssClassName);
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        protected ThemeValue<T> ThemeValue<T>(ThemeProperty<T> property) {
            var themeValue = new ThemeValue<T>(this, property);
            return RegisterThemeValue(themeValue);
        }

        protected ThemeValue<T> ThemeValue<T>(ThemeProperty<T> property,
            ThemeValue<T>.OnValueChangedDelegate onValueChanged) {
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
            WidgetFactoryReference<T> reference = default
        ) where T : VisualElement {
            var slot = new WidgetFactorySlot<T>(this) { Reference = reference };
            _widgetFactorySlots.Add(slot);
            if (onCreated != null) slot.OnElementCreated += onCreated;
            if (onDestroyed != null) slot.OnElementDestroyed += onDestroyed;
            if (panel != null) slot.TryCreate();
            return slot;
        }

        protected WidgetFactorySlot<T> WidgetFactorySlot<T>(
            ThemeProperty<WidgetFactory<T>> property,
            WidgetFactorySlot<T>.OnElementCreatedDelegate onCreated = null,
            WidgetFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null,
            WidgetFactoryReference<T> reference = default
        ) where T : VisualElement {
            var slot = new WidgetFactorySlot<T>(this, property) { Reference = reference };
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
            ThemeProvider = WidgetThemeProvider.Get(this);
            if (ThemeProvider != null) ThemeProvider.OnThemeUpdated += OnThemeUpdated;
            foreach (var value in _themeValues) value.ReloadStyles();
            foreach (var factorySlot in _widgetFactorySlots) factorySlot.Recreate();
        }

        protected virtual void OnDetached(DetachFromPanelEvent evt) {
            if (ThemeProvider != null) ThemeProvider.OnThemeUpdated -= OnThemeUpdated;
            ThemeProvider = null;
        }

        private void OnThemeUpdated() {
            foreach (var value in _themeValues) value.ReloadStyles();

            foreach (var factorySlot in _widgetFactorySlots) {
                factorySlot.ApplyReferenceFromTheme();
                factorySlot.TryCreate();
            }
        }
    }

    public abstract class SingleChildContainerWidget : BaseWidget, ISingleChildContainer {
        protected SingleChildContainerWidget() : base() { }

        public virtual VisualElement Child {
            get => Children().FirstOrDefault();
            set {
                Clear();
                if (value != null) {
                    Add(value);
                }
            }
        }
    }

    public abstract class MultiChildContainerWidget : BaseWidget, IMultiChildContainer {
        protected MultiChildContainerWidget() : base() { }

        public virtual IEnumerable<VisualElement> Childs {
            get => Children();
            set {
                Clear();
                if (value == null) return;
                foreach (var child in value) {
                    Add(child);
                }
            }
        }
    }

    [UxmlElement]
    public partial class WidgetThemeProvider : MultiChildContainerWidget {
        public static readonly Dictionary<string, object> GlobalThemeValues = new();
        private readonly Dictionary<string, object> _cachedThemeValues = new();
        private readonly Dictionary<string, object> _componentValues = new();
        private WidgetThemeProvider _parent;
        private List<WidgetThemeComponent> _components = new();
        public Dictionary<string, object> ThemeValues { get; } = new();

        public event Action OnThemeUpdated;


        [UxmlObjectReference]
        public List<WidgetThemeComponent> Components {
            get => _components;
            set {
                _components = value;
                _componentValues.Clear();
                foreach (var component in _components) {
                    component.Apply(_componentValues);
                }

                NotifyThemeUpdate();
            }
        }

        public static event Action OnGlobalThemeChanged;

        public WidgetThemeProvider() {
            RegisterCallback<CustomStyleResolvedEvent>(_ => { NotifyThemeUpdate(); });
        }

        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            _parent = Get(this);
            OnGlobalThemeChanged += NotifyThemeUpdate;
            if (_parent != null) _parent.OnThemeUpdated += NotifyThemeUpdate;
        }

        protected override void OnDetached(DetachFromPanelEvent evt) {
            base.OnDetached(evt);
            OnGlobalThemeChanged -= NotifyThemeUpdate;
            if (_parent != null) _parent.OnThemeUpdated -= NotifyThemeUpdate;
            _parent = null;
        }

        public void NotifyThemeUpdate() {
            _cachedThemeValues.Clear();
            if (panel == null) return;
            OnThemeUpdated?.Invoke();
        }

        public T Resolve<T>(ThemeProperty<T> property) {
            if (_cachedThemeValues.TryGetValue(property.key, out var cachedValue) &&
                cachedValue is T typedCachedValue) {
                return typedCachedValue;
            }

            var resolvedValue = ResolveInternal(property);
            _cachedThemeValues[property.key] = resolvedValue;
            return resolvedValue;
        }

        private T ResolveInternal<T>(ThemeProperty<T> property) {
            if (ThemeValues.TryGetValue(property.key, out var value) && value is T typedValue) {
                return typedValue;
            }

            if (_componentValues.TryGetValue(property.key, out var componentValue) &&
                componentValue is T typedComponentValue) {
                return typedComponentValue;
            }

            if (property.Resolve(customStyle, out var resolvedValue)) {
                return resolvedValue;
            }

            if (_parent != null) {
                return _parent.Resolve(property);
            }

            if (GlobalThemeValues.TryGetValue(property.key, out var globalValue) && globalValue is T typedGlobalValue) {
                return typedGlobalValue;
            }

            return property.defaultValue;
        }

        public void Set(ThemeProperty property, object value, bool notify = true) {
            ThemeValues[property.key] = value;
            if (notify) NotifyThemeUpdate();
        }

        public void Set<T>(ThemeProperty<T> property, T value, bool notify = true) {
            ThemeValues[property.key] = value;
            if (notify) NotifyThemeUpdate();
        }

        public void SetGlobal(ThemeProperty property, object value, bool notify = true) {
            GlobalThemeValues[property.key] = value;
            if (notify) OnGlobalThemeChanged?.Invoke();
        }

        public void SetGlobal<T>(ThemeProperty<T> property, T value, bool notify = true) {
            GlobalThemeValues[property.key] = value;
            if (notify) OnGlobalThemeChanged?.Invoke();
        }

        public static WidgetThemeProvider Get(VisualElement element) {
            return element.GetFirstAncestorOfType<WidgetThemeProvider>();
        }

        public static T Resolve<T>(WidgetThemeProvider provider, ThemeProperty<T> property) {
            if (provider != null) return provider.Resolve(property);
            if (GlobalThemeValues.TryGetValue(property.key, out var globalValue) && globalValue is T typedGlobalValue) {
                return typedGlobalValue;
            }

            return property.defaultValue;
        }
    }

    [UxmlObject]
    public abstract partial class WidgetThemeComponent {
        public virtual void Apply(Dictionary<string, object> dict) {
            foreach (var info in GetType().GetFields()) {
                var attribute = info.GetCustomAttributes<UxmlAttributeAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                var value = info.GetValue(this);
                switch (value) {
                    case IWidgetFactoryReference reference:
                        var widgetFactory = reference.GetFactory();
                        if (widgetFactory != null) dict[attribute.name] = widgetFactory;
                        break;
                    case ThemeOverride overrides:
                        if (overrides.TryGetOverride(out var overrideValue)) {
                            dict[attribute.name] = overrideValue;
                        }

                        break;
                    case ThemeOptional optional:
                        if (optional.TryGetValue(out var optionalValue)) {
                            dict[attribute.name] = optionalValue;
                        }

                        break;
                    default:
                        dict[attribute.name] = value;
                        break;
                }
            }
        }
    }
}