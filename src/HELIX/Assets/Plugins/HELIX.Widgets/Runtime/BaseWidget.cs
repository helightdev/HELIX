using System.Collections.Generic;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class BaseWidget : VisualElement {
        public static readonly string UssClassName = "helix-widget";
        private readonly List<ThemeValue> _themeValues;
        internal TemplateContainer templateContainer;
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

        protected virtual void OnAttached(AttachToPanelEvent evt) {
            foreach (var value in _themeValues) {
                value.ReloadStyles();
            }

            if (ResolveTemplateContainerOnAttach) {
                templateContainer?.UnregisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
                templateContainer = GetFirstAncestorOfType<TemplateContainer>();
                templateContainer.RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);   
            }
        }

        protected virtual void OnDetached(DetachFromPanelEvent evt) {
            if (ResolveTemplateContainerOnAttach) templateContainer?.UnregisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
        }

        protected virtual void OnStyleResolved(CustomStyleResolvedEvent evt) {
            foreach (var value in _themeValues) {
                value.ReloadStyles();
            }
        }
    }
}