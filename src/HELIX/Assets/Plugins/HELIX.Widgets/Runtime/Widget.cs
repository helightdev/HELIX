using System;
using System.Collections.Generic;
using HELIX.Painting;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class BaseWidget : VisualElement {

        private readonly List<ThemeValue> _themeValues;
        
        protected BaseWidget() {
            _themeValues = new List<ThemeValue>();
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
            RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
        }

        protected ThemeValue<T> ThemeValue<T>(ThemeProperty<T> property) {
            var themeValue = new ThemeValue<T>(property);
            return RegisterThemeValue(themeValue);
        }

        protected ThemeValue<T> ThemeValue<T>(ThemeProperty<T> property, ThemeValue<T>.OnValueChangedDelegate onValueChanged) {
            var themeValue = new ThemeValue<T>(property, onValueChanged);
            return RegisterThemeValue(themeValue);
        }
        
        protected virtual ThemeValue<T> RegisterThemeValue<T>(ThemeValue<T> themeValue) {
            _themeValues.Add(themeValue);
            return themeValue;
        }

        protected virtual void OnAttached(AttachToPanelEvent evt) {
            
        }

        protected virtual void OnDetached(DetachFromPanelEvent evt) {
            
        }

        protected virtual void OnStyleResolved(CustomStyleResolvedEvent evt) {
            foreach (var value in _themeValues) {
                value.ReloadStyle(evt.customStyle);
            }
        }
    }
    
    public abstract class PaintingWidget : BaseWidget {
        protected PaintingWidget() : base() {
            generateVisualContent += OnGenerateVisualContent;
        }
        protected virtual void OnGenerateVisualContent(MeshGenerationContext mgc) {
            var context = new PaintCanvas(mgc);
            Paint(context, context.canvasRect);
        }

        protected override ThemeValue<T> RegisterThemeValue<T>(ThemeValue<T> themeValue) {
            themeValue.OnValueChanged += _ => MarkDirtyRepaint();
            return base.RegisterThemeValue(themeValue);
        }

        public abstract void Paint(PaintCanvas canvas, Rect bounds);

    }
    
    public static class MyThemes {
        public static readonly ThemeProperty<Color> PrimaryColor = new("c-primary", Color.white);
    }
    
    [UxmlElement]
    public partial class Example : BaseWidget {

        public Example() {
            ThemeValue(MyThemes.PrimaryColor, OnPrimaryColorChanged);
        }
        
        private void OnPrimaryColorChanged(Color newValue) {
            style.backgroundColor = newValue;
        }
    }
}