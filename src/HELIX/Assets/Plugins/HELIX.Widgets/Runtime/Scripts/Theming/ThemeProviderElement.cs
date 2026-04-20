using System;
using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Utilities;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    [UxmlElement]
    public partial class ThemeProviderElement : SingleChildWidgetBaseElement<HThemeProvider>, IThemeProvider {

        public static readonly IdentityDictionary<ThemeProperty, object> GlobalThemeValues = new();
        private static readonly IdentityDictionary<ThemeProperty, object> _globalComputedThemeValues = new();

        private readonly IdentityDictionary<ThemeProperty, object> _cachedThemeValues = new();
        private readonly IdentityDictionary<ThemeProperty, object> _componentValues = new();
        private readonly IdentityDictionary<ThemeProperty, object> _computedThemeValues = new();
        private List<ThemeComponent> _components = new();
        private ThemeProviderElement _parent;

        public ThemeProviderElement() {
            RegisterCallback<CustomStyleResolvedEvent>(_ => { NotifyThemeUpdate(); });
        }

        public Dictionary<ThemeProperty, object> ThemeValues { get; } = new();

        [UxmlObjectReference]
        public List<ThemeComponent> Components {
            get => _components;
            set {
                if (_components == null || value == null) return;
                _components = value;
                _componentValues.Clear();
                foreach (var component in value) component?.Apply(_componentValues);

                NotifyThemeUpdate();
            }
        }

        public override T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true) {
            return Resolve(property);
        }

        public override bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true) {
            return TryResolve(property, out value);
        }

        public new event Action OnThemeUpdated;

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
            _computedThemeValues.Clear();
            ModificationBarrier.Run(() => { OnThemeUpdated?.Invoke(); });
        }

        public static void NotifyGlobalThemeUpdate() {
            ModificationBarrier.Run(() => { OnGlobalThemeChanged?.Invoke(); });
        }

        public T Resolve<T>(BaseThemeProperty<T> property, bool computed = true) {
            if (TryResolve(property, out var value, computed)) return value;
            return property.TypedDefaultValue;
        }

        public bool TryResolve<T>(BaseThemeProperty<T> property, out T value, bool computed = true) {
            var success = TryResolveNoCompute(property, out value);
            if (!computed || success) return success;

            if (_computedThemeValues.TryGetValue(property, out var cachedValue)) {
                if (cachedValue is not T typedCachedValue) return false;
                value = typedCachedValue;
                return true;
            }

            if (property.TryCompute(this, out var computedValue)) {
                _computedThemeValues[property] = computedValue;
                value = computedValue;
                return true;
            }
            _cachedThemeValues[property] = null;
            return false;
        }

        private bool TryResolveNoCompute<T>(BaseThemeProperty<T> property, out T value) {
            value = property.TypedDefaultValue;
            if (_cachedThemeValues.TryGetValue(property, out var cachedValue)) {
                if (cachedValue is not T typedCachedValue) return false;
                value = typedCachedValue;
                return true;
            }

            var resolvedValue = ResolveInternal(property);
            _cachedThemeValues[property] = resolvedValue;
            if (resolvedValue is not T typedResolvedValue) return false;
            value = typedResolvedValue;
            return true;
        }

        private object ResolveInternal<T>(BaseThemeProperty<T> property) {
            if (ThemeValues.TryGetValue(property, out var value)) return value;

            if (_cachedThemeValues != null && _componentValues.TryGetValue(property, out var componentValue))
                return componentValue;

            if (property.ResolveStyle(customStyle, out var resolvedValue)) return resolvedValue;

            if (_parent != null) {
                var success = _parent.TryResolve(property, out var parentResolvedValue, false);
                return success ? parentResolvedValue : null;
            }

            if (GlobalThemeValues.TryGetValue(property, out var globalValue)) return globalValue;

            if (!property.IsDefaultValid) return null;
            return property.TypedDefaultValue;
        }

        public void Set(ThemeProperty property, object value, bool notify = true) {
            ThemeValues[property] = value;
            if (notify) NotifyThemeUpdate();
        }

        public void Unset(ThemeProperty property, bool notify = true) {
            if (ThemeValues.Remove(property) && notify) NotifyThemeUpdate();
        }

        public void Set<T>(BaseThemeProperty<T> property, T value, bool notify = true) {
            ThemeValues[property] = value;
            if (notify) NotifyThemeUpdate();
        }

        public override void Apply(HThemeProvider previous, HThemeProvider widget) {
            ThemeValues.Clear();
            if (widget.properties != null)
                foreach (var kvp in widget.properties)
                    ThemeValues[kvp.Key] = kvp.Value;

            Components = widget.components ?? new List<ThemeComponent>(); // This will also update the theme
        }

        public static void SetGlobal(ThemeProperty property, object value, bool notify = true) {
            GlobalThemeValues[property] = value;
            _globalComputedThemeValues.Clear();
            if (notify) OnGlobalThemeChanged?.Invoke();
        }

        public static void UnsetGlobal(ThemeProperty property, bool notify = true) {
            if (!GlobalThemeValues.Remove(property)) return;
            _globalComputedThemeValues.Clear();
            if (notify) OnGlobalThemeChanged?.Invoke();
        }

        public static void SetGlobal<T>(BaseThemeProperty<T> property, T value, bool notify = true) {
            GlobalThemeValues[property] = value;
            _globalComputedThemeValues.Clear();
            if (notify) OnGlobalThemeChanged?.Invoke();
        }

        public static ThemeProviderElement Get(VisualElement element) {
            return element.GetFirstAncestorOfType<ThemeProviderElement>();
        }

        public static T Resolve<T>(ThemeProviderElement providerElement, BaseThemeProperty<T> property) {
            TryResolve(providerElement, property, out var value);
            return value;
        }

        public static bool TryResolve<T>(
            ThemeProviderElement providerElement,
            BaseThemeProperty<T> property,
            out T value
        ) {
            if (providerElement != null) return providerElement.TryResolve(property, out value);
            if (GlobalThemeValues.TryGetValue(property, out var globalValue) && globalValue is T typedGlobalValue) {
                value = typedGlobalValue;
                return true;
            }

            if (_globalComputedThemeValues.TryGetValue(property, out var cachedValue)) {
                if (cachedValue is T typedCachedValue) {
                    value = typedCachedValue;
                    return true;
                }
                value = property.TypedDefaultValue;
                return false;
            }

            if (property.TryCompute(null, out var computedValue)) {
                _globalComputedThemeValues[property] = computedValue;
                value = computedValue;
                return true;
            }

            _globalComputedThemeValues[property] = null;
            value = property.TypedDefaultValue;
            return false;
        }

    }

    public interface IThemeProvider {

        T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true);
        bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true);

    }

    public class FallbackThemeProvider : IThemeProvider {

        public static readonly FallbackThemeProvider Instance = new();

        public T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true) {
            return ThemeProviderElement.Resolve(null, property);
        }

        public bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true) {
            return ThemeProviderElement.TryResolve(null, property, out value);
        }

    }
}