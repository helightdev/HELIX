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

        protected ThemeProperty(string key) {
            this.key = key;
        }

        public static UnthemedProperty<T> Unthemed<T>(T defaultValue) {
            return new UnthemedProperty<T>(defaultValue);
        }

        public static ThemeProperty<T, T> Theme<T>(string key, T defaultValue) {
            return new ThemeProperty<T, T>(key, defaultValue);
        }

        public static ThemeProperty<T, string> Serialized<T>(string key, T defaultValue) {
            return new ThemeProperty<T, string>(key, defaultValue);
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

    public class ThemeProperty<T, TS> : ThemeProperty<T> {
        public readonly CustomStyleProperty<TS> style;

        public ThemeProperty(string key, T defaultValue) : base(key, defaultValue) {
            style = new CustomStyleProperty<TS>($"--{key}");
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

            switch (this) {
                case ThemeProperty<float, float> floatProperty: {
                    if (customStyle.TryGetValue(floatProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<int, int> intProperty: {
                    if (customStyle.TryGetValue(intProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<string, string> stringProperty: {
                    if (customStyle.TryGetValue(stringProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<bool, bool> boolProperty: {
                    if (customStyle.TryGetValue(boolProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<Color, Color> colorProperty: {
                    if (customStyle.TryGetValue(colorProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<Texture2D, Texture2D> textureProperty: {
                    if (customStyle.TryGetValue(textureProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<Sprite, Sprite> spriteProperty: {
                    if (customStyle.TryGetValue(spriteProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<VectorImage, VectorImage> vectorImageProperty: {
                    if (customStyle.TryGetValue(vectorImageProperty.style, out var value)) {
                        result = (T)(object)value;
                        return true;
                    }

                    break;
                }
                case ThemeProperty<T, string> serializedProperty: {
                    if (customStyle.TryGetValue(serializedProperty.style, out var value)) {
                        throw new NotImplementedException("Deserialization for ThemeProperty is not implemented.");
                    }

                    break;
                }
                default:

                    Debug.LogWarning($"Unsupported ThemeProperty type: {typeof(T)}");
                    break;
            }

            result = default;
            return false;
        }
    }
}