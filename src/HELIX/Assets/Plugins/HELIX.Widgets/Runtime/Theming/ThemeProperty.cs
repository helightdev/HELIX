using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeProperty {
        public static readonly Dictionary<Type, IThemeStyleValueLoader> Loaders = new() {
            [typeof(float)] = new FloatThemeStyleValueLoader(),
            [typeof(int)] = new IntThemeStyleValueLoader(),
            [typeof(string)] = new StringThemeStyleValueLoader(),
            [typeof(bool)] = new BoolThemeStyleValueLoader(),
            [typeof(Color)] = new ColorThemeStyleValueLoader(),
            [typeof(Texture2D)] = new Texture2DThemeStyleValueLoader(),
            [typeof(Sprite)] = new SpriteThemeStyleValueLoader(),
            [typeof(VectorImage)] = new VectorImageThemeStyleValueLoader(),
            [typeof(Vector2)] = new Vector2ThemeStyleValueLoader(),
            [typeof(Vector3)] = new Vector3ThemeStyleValueLoader(),
            [typeof(Vector4)] = new Vector4ThemeStyleValueLoader(),
        };

        public readonly string key;
        public string StyleKey => $"--{key}";

        protected ThemeProperty(string key) {
            this.key = key;
        }

        public static UnthemedProperty<T> Unthemed<T>(T defaultValue) {
            return new UnthemedProperty<T>(defaultValue);
        }

        public static ThemeProperty<T, T> Theme<T>(string key, T defaultValue) {
            return new ThemeProperty<T, T>(key, defaultValue);
        }

        public static WidgetFactoryProperty<T> WidgetFactory<T>(string key, WidgetFactory<T> defaultFactory)
            where T : VisualElement {
            return new WidgetFactoryProperty<T>(key, defaultFactory);
        }

        public static WidgetFactoryProperty<T> WidgetFactory<T>(string key, Type defaultFactory)
            where T : VisualElement {
            return new WidgetFactoryProperty<T>(key, defaultFactory.FullName);
        }

        public static WidgetFactoryProperty<T> WidgetFactory<T>(string key) where T : VisualElement {
            return new WidgetFactoryProperty<T>(key, "None");
        }

        public static implicit operator string(ThemeProperty property) {
            return property.StyleKey;
        }
    }

    public abstract class ThemeProperty<T> : ThemeProperty {
        public readonly T defaultValue;

        protected ThemeProperty(string key, T defaultValue) : base(key) {
            this.defaultValue = defaultValue;
        }

        public abstract bool Resolve(ICustomStyle customStyle, out T result);
    }


    public class UnthemedProperty<T> : ThemeProperty<T> {
        public UnthemedProperty(T defaultValue) : base("unthemed", defaultValue) { }

        public override bool Resolve(ICustomStyle customStyle, out T result) {
            result = defaultValue;
            return true;
        }
    }

    public class WidgetFactoryProperty<T> : ThemeProperty<WidgetFactory<T>> where T : VisualElement {
        public WidgetFactoryProperty(string key, WidgetFactory<T> defaultValue) : base(key, defaultValue) { }

        public WidgetFactoryProperty(string key, string defaultFactoryName) : base(key,
            RuntimeReflectionThemeLookup.GetFactory(defaultFactoryName) as WidgetFactory<T>) { }

        public override bool Resolve(ICustomStyle customStyle, out WidgetFactory<T> result) {
            var property = new CustomStyleProperty<string>(StyleKey);
            if (customStyle.TryGetValue(property, out var referenceKey)) {
                if (RuntimeReflectionThemeLookup.GetFactory(referenceKey) is WidgetFactory<T> factory) {
                    result = factory;
                    return true;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    public class ThemeProperty<T, TS> : ThemeProperty<T> {
        public readonly CustomStyleProperty<TS> style;

        public ThemeProperty(string key, T defaultValue) : base(key, defaultValue) {
            style = new CustomStyleProperty<TS>(StyleKey);
        }


        public override bool Resolve(ICustomStyle customStyle, out T result) {
            if (!Loaders.TryGetValue(typeof(T), out var loader)) {
                Debug.LogWarning($"No loader registered for ThemeProperty type: {typeof(T)}");
                result = defaultValue;
                return true;
            }

            if (loader is not IThemeStyleValueLoader<T> typedLoader) {
                Debug.LogWarning($"Loader for ThemeProperty type: {typeof(T)} is not of expected type.");
                result = defaultValue;
                return true;
            }

            if (typedLoader.Load(style.name, customStyle, out result)) return true;
            result = defaultValue;
            return false;
        }
    }
}