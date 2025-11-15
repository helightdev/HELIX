using System;
using System.Collections.Generic;
using HELIX.Painting;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.Scripting;
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

        protected virtual void OnAttached(AttachToPanelEvent evt) { }

        protected virtual void OnDetached(DetachFromPanelEvent evt) { }

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

    public class ThemePropertyCollectionAttribute : Attribute { }
    public class ThemePropertyReferenceAttribute : PropertyAttribute { }

    [ThemePropertyCollection]
    public static class MyThemes {
        public static readonly ThemeProperty<Color> PrimaryColor = ThemeProperty.Pure("c-primary", Color.white);
        public static readonly ThemeProperty<Color> PrimaryWashedColor = ThemeProperty.Pure("c-primary-washed", Color.white);
    }

    [UxmlElement]
    public partial class Example : BaseWidget {
        private readonly ThemeValue<Color> _primaryColor;

        [UxmlAttribute]
        public ThemeOverride<Color> Override {
            get => _primaryColor.Override;
            set => _primaryColor.Override = value;
        }

        public Example() {
            _primaryColor = ThemeValue(MyThemes.PrimaryColor, OnPrimaryColorChanged);
        }

        private void OnPrimaryColorChanged(Color newValue) {
            style.backgroundColor = newValue;
        }
    }
}