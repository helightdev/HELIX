using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeProperty {
        public readonly string key;
        protected ThemeProperty(string key) {
            this.key = key;
        }
        
        public static ThemeProperty<T,T> Pure<T>(string key, T defaultValue) {
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

        public abstract T Resolve(ICustomStyle customStyle);
    }

    public class ThemeProperty<T, TS> : ThemeProperty<T> {
        public readonly CustomStyleProperty<TS> style;

        public ThemeProperty(string key, T defaultValue) : base(key, defaultValue) {
            style = new CustomStyleProperty<TS>($"--{key}");
        }


        public override T Resolve(ICustomStyle customStyle) {
            switch (this) {
                case ThemeProperty<float, float> floatProperty: {
                    if (customStyle.TryGetValue(floatProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<int, int> intProperty: {
                    if (customStyle.TryGetValue(intProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<string, string> stringProperty: {
                    if (customStyle.TryGetValue(stringProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<bool, bool> boolProperty: {
                    if (customStyle.TryGetValue(boolProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<Color, Color> colorProperty: {
                    if (customStyle.TryGetValue(colorProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<Texture2D, Texture2D> textureProperty: {
                    if (customStyle.TryGetValue(textureProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<Sprite, Sprite> spriteProperty: {
                    if (customStyle.TryGetValue(spriteProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<VectorImage, VectorImage> vectorImageProperty: {
                    if (customStyle.TryGetValue(vectorImageProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<T, string> fontProperty: {
                    if (customStyle.TryGetValue(fontProperty.style, out var value)) {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }

                    break;
                }
                default:

                    Debug.LogWarning($"Unsupported ThemeProperty type: {typeof(T)}");
                    break;
            }

            return defaultValue;
        }
    }
}