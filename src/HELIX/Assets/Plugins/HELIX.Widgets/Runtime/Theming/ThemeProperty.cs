using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public class ThemeProperty<T> {
        public readonly string key;
        public readonly T defaultValue;
        public readonly CustomStyleProperty<T> style;

        public ThemeProperty(string key, T defaultValue) {
            this.key = key;
            this.defaultValue = defaultValue;
            style = new CustomStyleProperty<T>($"--{key}");
        }

        public T Resolve(ICustomStyle customStyle) {
            switch (this) {
                case ThemeProperty<float> floatProperty: {
                    if (customStyle.TryGetValue(floatProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<int> intProperty: {
                    if (customStyle.TryGetValue(intProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<string> stringProperty: {
                    if (customStyle.TryGetValue(stringProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<bool> boolProperty: {
                    if (customStyle.TryGetValue(boolProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<Color> colorProperty: {
                    if (customStyle.TryGetValue(colorProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<Texture2D> textureProperty: {
                    if (customStyle.TryGetValue(textureProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<Sprite> spriteProperty: {
                    if (customStyle.TryGetValue(spriteProperty.style, out var value)) return (T)(object)value;
                    break;
                }
                case ThemeProperty<VectorImage> vectorImageProperty: {
                    if (customStyle.TryGetValue(vectorImageProperty.style, out var value)) return (T)(object)value;
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