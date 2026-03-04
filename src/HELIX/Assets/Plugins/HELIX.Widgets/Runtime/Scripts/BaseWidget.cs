using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class BaseWidget : VisualElement {
        public static readonly string UssClassName = "helix-widget";
        private readonly List<ThemeValue> _themeValues = new();
        private readonly List<WidgetFactorySlot> _widgetFactorySlots = new();

        protected BaseWidget() {
            AddToClassList(UssClassName);
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        public WidgetThemeProvider ThemeProvider { get; private set; }

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
        public virtual VisualElement Child {
            get => Children().FirstOrDefault();
            set {
                Clear();
                if (value != null) Add(value);
            }
        }
    }

    public abstract class MultiChildContainerWidget : BaseWidget, IMultiChildContainer {
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