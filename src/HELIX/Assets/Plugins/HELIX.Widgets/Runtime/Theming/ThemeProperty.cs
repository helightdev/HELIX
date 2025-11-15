using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeProperty {
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
        public UnthemedProperty(T defaultValue) : base("unthemed", defaultValue) {
        }

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