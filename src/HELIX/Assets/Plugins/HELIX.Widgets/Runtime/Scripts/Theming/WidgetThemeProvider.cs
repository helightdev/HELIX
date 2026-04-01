using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    [UxmlElement]
    public partial class WidgetThemeProvider : MultiChildContainerWidget {
        public static readonly Dictionary<string, object> GlobalThemeValues = new();
        private readonly Dictionary<string, object> _cachedThemeValues = new();
        private readonly Dictionary<string, object> _componentValues = new();
        private List<WidgetThemeComponent> _components = new();
        private WidgetThemeProvider _parent;

        public WidgetThemeProvider() {
            RegisterCallback<CustomStyleResolvedEvent>(_ => { NotifyThemeUpdate(); });
        }

        public Dictionary<string, object> ThemeValues { get; } = new();

        [UxmlObjectReference]
        public List<WidgetThemeComponent> Components {
            get => _components;
            set {
                _components = value;
                _componentValues.Clear();
                foreach (var component in _components) component.Apply(_componentValues);

                NotifyThemeUpdate();
            }
        }

        public event Action OnThemeUpdated;

        public static event Action OnGlobalThemeChanged;

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
                cachedValue is T typedCachedValue) return typedCachedValue;

            var resolvedValue = ResolveInternal(property);
            _cachedThemeValues[property.key] = resolvedValue;
            return resolvedValue;
        }

        private T ResolveInternal<T>(ThemeProperty<T> property) {
            if (ThemeValues.TryGetValue(property.key, out var value) && value is T typedValue) return typedValue;

            if (_componentValues.TryGetValue(property.key, out var componentValue) &&
                componentValue is T typedComponentValue) return typedComponentValue;

            if (property.Resolve(customStyle, out var resolvedValue)) return resolvedValue;

            if (_parent != null) return _parent.Resolve(property);

            if (GlobalThemeValues.TryGetValue(property.key, out var globalValue) && globalValue is T typedGlobalValue)
                return typedGlobalValue;

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
            if (GlobalThemeValues.TryGetValue(property.key, out var value) && value is T typedValue) return typedValue;
            return property.defaultValue;
        }
    }

    [UxmlObject, RequireDerived]
    public abstract partial class WidgetThemeComponent : ICloneable {
        public virtual bool Resolve(string key, out object value) {
            value = null;
            var companion = Companion.Get(GetType());
            if (!companion.TryGetField(key, out var accessor)) return false;
            value = accessor.getter(this);
            return Companion.TryExtractValue(value, out value);
        }

        public virtual void Apply(Dictionary<string, object> dict, bool clearExisting = false) {
            Companion.Get(GetType()).Apply(this, dict, clearExisting);
        }

        public void ApplyGlobal(bool clearExisting = true) {
            Apply(WidgetThemeProvider.GlobalThemeValues, clearExisting);
            WidgetThemeProvider.NotifyGlobalThemeUpdate();
        }

        public class Companion {
            private static readonly Dictionary<Type, Companion> _cache = new();

            public static Companion Get(Type type) {
                if (_cache.TryGetValue(type, out var resolver)) return resolver;
                resolver = new Companion(type);
                _cache[type] = resolver;
                return resolver;
            }

            public readonly FieldAccessors[] fieldAccessors;

            private Companion(Type type) {
                var fields = type.GetFields().Where(info => info.IsPublic && !info.IsStatic).ToArray();
                fieldAccessors = new FieldAccessors[fields.Length];

                for (var index = 0; index < fields.Length; index++) {
                    var info = fields[index];
                    var attributeName = info.Name;

                    var attributeAttr = info.GetCustomAttributes<UxmlAttributeAttribute>().FirstOrDefault();
                    if (attributeAttr != null) { attributeName = attributeAttr.name; }

                    var objectReferenceAttr = info.GetCustomAttributes<UxmlObjectReferenceAttribute>().FirstOrDefault();
                    if (objectReferenceAttr != null) { attributeName = objectReferenceAttr.name; }

                    fieldAccessors[index] = new FieldAccessors {
                        name = attributeName,
                        getter = info.GetValue,
                    };
                }
            }

            public bool TryGetField(string name, out FieldAccessors accessor) {
                foreach (var fieldAccessor in fieldAccessors) {
                    if (fieldAccessor.name != name) continue;
                    accessor = fieldAccessor;
                    return true;
                }

                accessor = default;
                return false;
            }

            public void Apply(object instance, Dictionary<string, object> dict, bool clearExisting = false) {
                foreach (var accessor in fieldAccessors) {
                    var value = accessor.getter(instance);
                    ApplyValue(accessor.name, value, dict, clearExisting);
                }
            }

            public static bool TryExtractValue(object value, out object extracted) {
                extracted = null;
                switch (value) {
                    case IWidgetFactoryReference reference:
                        var widgetFactory = reference.LookupFactory();
                        if (widgetFactory == null) return false;
                        extracted = widgetFactory;
                        return true;
                    case ThemeOverride overrides:
                        if (!overrides.TryGetOverride(out var overrideValue)) return false;
                        extracted = overrideValue;
                        return true;
                    case ThemeOptional optional:
                        if (!optional.TryGetValue(out var optionalValue)) return false;
                        extracted = optionalValue;
                        return true;
                    case null: return false;
                    default:
                        extracted = value;
                        return true;
                }
            }

            public static void ApplyValue(
                string key,
                object value,
                Dictionary<string, object> dict,
                bool clearExisting
            ) {
                if (TryExtractValue(value, out var extracted)) {
                    dict[key] = extracted;
                } else if (clearExisting) {
                    dict.Remove(key);
                }
            }

            public struct FieldAccessors {
                public string name;
                public Func<object, object> getter;
            }
        }

        public virtual object Clone() {
            return MemberwiseClone();
        }
    }
}