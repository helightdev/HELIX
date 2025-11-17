using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class BaseWidget : VisualElement {
        public static readonly string UssClassName = "helix-widget";
        private readonly List<ThemeValue> _themeValues;
        private readonly List<WidgetFactorySlot> _widgetFactorySlots = new();
        protected bool ResolveTemplateContainerOnAttach { get; set; } = true;

        protected BaseWidget() {
            AddToClassList(UssClassName);
            _themeValues = new List<ThemeValue>();
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
            RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
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
            WidgetFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null
        ) where T : VisualElement {
            var slot = new WidgetFactorySlot<T>(this);
            _widgetFactorySlots.Add(slot);
            if (onCreated != null) {
                slot.OnElementCreated += onCreated;
            }
            if (onDestroyed != null) {
                slot.OnElementDestroyed += onDestroyed;
            }
            return slot;
        }
        
        protected WidgetFactorySlot<T> WidgetFactorySlot<T>(
            ThemeProperty<WidgetFactory<T>> property,
            WidgetFactorySlot<T>.OnElementCreatedDelegate onCreated = null,
            WidgetFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null
        ) where T : VisualElement {
            var slot = new WidgetFactorySlot<T>(this, property);
            _widgetFactorySlots.Add(slot);
            if (onCreated != null) {
                slot.OnElementCreated += onCreated;
            }
            if (onDestroyed != null) {
                slot.OnElementDestroyed += onDestroyed;
            }
            return slot;
        }


        protected virtual void OnAttached(AttachToPanelEvent evt) {
            foreach (var value in _themeValues) {
                value.ReloadStyles();
            }
            
            foreach (var factorySlot in _widgetFactorySlots) {
                factorySlot.Recreate();
            }
            
        }

        protected virtual void OnDetached(DetachFromPanelEvent evt) {
        }

        protected virtual void OnStyleResolved(CustomStyleResolvedEvent evt) {
            foreach (var value in _themeValues) {
                value.ReloadStyles();
            }
            foreach (var factorySlot in _widgetFactorySlots) {
                if (factorySlot.HasElement) continue;
                factorySlot.ApplyReferenceFromStyle(evt.customStyle);
                factorySlot.TryCreate();
            }
        }
        
    }

    public interface ISingleChildContainer {
        VisualElement Child { get; set; }
    }
    public interface IMultiChildContainer {
        IEnumerable<VisualElement> Childs { get; set; }
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
}