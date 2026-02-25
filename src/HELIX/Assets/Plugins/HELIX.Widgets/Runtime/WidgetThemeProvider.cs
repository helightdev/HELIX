using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
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
            OnGlobalThemeChanged += ListenerNotifyThemeUpdate;
            if (_parent != null) _parent.OnThemeUpdated += ListenerNotifyThemeUpdate;
        }

        protected override void OnDetached(DetachFromPanelEvent evt) {
            base.OnDetached(evt);
            OnGlobalThemeChanged -= ListenerNotifyThemeUpdate;
            if (_parent != null) _parent.OnThemeUpdated -= ListenerNotifyThemeUpdate;
            _parent = null;
        }

        private void ListenerNotifyThemeUpdate() {
            NotifyThemeUpdate(true);
        }

        public void NotifyThemeUpdate(bool fromListener = false) {
            _cachedThemeValues.Clear();
            OnThemeUpdated?.Invoke();
        }

        public static void NotifyGlobalThemeUpdate() {
            OnGlobalThemeChanged?.Invoke();
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

        public void Unset(ThemeProperty property, bool notify = true) {
            if (ThemeValues.Remove(property.key) && notify) NotifyThemeUpdate();
        }

        public void Set<T>(ThemeProperty<T> property, T value, bool notify = true) {
            ThemeValues[property.key] = value;
            if (notify) NotifyThemeUpdate();
        }

        public void SetGlobal(ThemeProperty property, object value, bool notify = true) {
            GlobalThemeValues[property.key] = value;
            if (notify) OnGlobalThemeChanged?.Invoke();
        }

        public void UnsetGlobal(ThemeProperty property, bool notify = true) {
            if (GlobalThemeValues.Remove(property.key) && notify) OnGlobalThemeChanged?.Invoke();
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
        public virtual void Apply(Dictionary<string, object> dict, bool clearExisting = false) {
            foreach (var info in GetType().GetFields()) {
                var attribute = info.GetCustomAttributes<UxmlAttributeAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                var value = info.GetValue(this);
                ApplyValue(attribute.name, value, dict, clearExisting);
            }
        }

        protected static void ApplyValue(string key, object value, Dictionary<string, object> dict, bool clearExisting) {
            switch (value) {
                case IWidgetFactoryReference reference:
                    var widgetFactory = reference.GetFactory();
                    if (widgetFactory != null) dict[key] = widgetFactory;
                    else if (clearExisting) dict.Remove(key);
                    break;
                case ThemeOverride overrides:
                    if (overrides.TryGetOverride(out var overrideValue)) {
                        dict[key] = overrideValue;
                    } else if (clearExisting) dict.Remove(key);

                    break;
                case ThemeOptional optional:
                    if (optional.TryGetValue(out var optionalValue)) {
                        dict[key] = optionalValue;
                    } else if (clearExisting) dict.Remove(key);

                    break;
                default:
                    dict[key] = value;
                    break;
            }
        }

        public void ApplyGlobal(bool clearExisting = true) {
            Apply(WidgetThemeProvider.GlobalThemeValues, clearExisting);
            WidgetThemeProvider.NotifyGlobalThemeUpdate();
        }
    }
}