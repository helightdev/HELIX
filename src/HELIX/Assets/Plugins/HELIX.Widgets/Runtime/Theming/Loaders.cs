using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public interface IThemeStyleValueLoader { }

    public interface IThemeStyleValueLoader<T> : IThemeStyleValueLoader {
        bool Load(string key, ICustomStyle customStyle, out T result);
    }

    public class FloatThemeStyleValueLoader : IThemeStyleValueLoader<float> {
        public bool Load(string key, ICustomStyle customStyle, out float result) {
            result = 0;
            return customStyle.TryGetValue(new CustomStyleProperty<float>(key), out result);
        }
    }

    public class IntThemeStyleValueLoader : IThemeStyleValueLoader<int> {
        public bool Load(string key, ICustomStyle customStyle, out int result) {
            result = 0;
            return customStyle.TryGetValue(new CustomStyleProperty<int>(key), out result);
        }
    }

    public class StringThemeStyleValueLoader : IThemeStyleValueLoader<string> {
        public bool Load(string key, ICustomStyle customStyle, out string result) {
            result = null;
            return customStyle.TryGetValue(new CustomStyleProperty<string>(key), out result);
        }
    }

    public class BoolThemeStyleValueLoader : IThemeStyleValueLoader<bool> {
        public bool Load(string key, ICustomStyle customStyle, out bool result) {
            result = false;
            return customStyle.TryGetValue(new CustomStyleProperty<bool>(key), out result);
        }
    }

    public class ColorThemeStyleValueLoader : IThemeStyleValueLoader<Color> {
        public bool Load(string key, ICustomStyle customStyle, out Color result) {
            result = Color.clear;
            return customStyle.TryGetValue(new CustomStyleProperty<Color>(key), out result);
        }
    }

    public class Texture2DThemeStyleValueLoader : IThemeStyleValueLoader<Texture2D> {
        public bool Load(string key, ICustomStyle customStyle, out Texture2D result) {
            result = null;
            return customStyle.TryGetValue(new CustomStyleProperty<Texture2D>(key), out result);
        }
    }

    public class SpriteThemeStyleValueLoader : IThemeStyleValueLoader<Sprite> {
        public bool Load(string key, ICustomStyle customStyle, out Sprite result) {
            result = null;
            return customStyle.TryGetValue(new CustomStyleProperty<Sprite>(key), out result);
        }
    }

    public class VectorImageThemeStyleValueLoader : IThemeStyleValueLoader<VectorImage> {
        public bool Load(string key, ICustomStyle customStyle, out VectorImage result) {
            result = null;
            return customStyle.TryGetValue(new CustomStyleProperty<VectorImage>(key), out result);
        }
    }

    public class Vector2ThemeStyleValueLoader : IThemeStyleValueLoader<Vector2> {
        public bool Load(string key, ICustomStyle customStyle, out Vector2 result) {
            result = Vector2.zero;
            return customStyle.TryGetValue(new CustomStyleProperty<string>(key), out var stringResult) &&
                   HelixConvert.ToVector2(stringResult, out result);
        }
    }
    
    public class Vector3ThemeStyleValueLoader : IThemeStyleValueLoader<Vector3> {
        public bool Load(string key, ICustomStyle customStyle, out Vector3 result) {
            result = Vector3.zero;
            return customStyle.TryGetValue(new CustomStyleProperty<string>(key), out var stringResult) &&
                   HelixConvert.ToVector3(stringResult, out result);
        }
    }
    
    public class Vector4ThemeStyleValueLoader : IThemeStyleValueLoader<Vector4> {
        public bool Load(string key, ICustomStyle customStyle, out Vector4 result) {
            result = Vector4.zero;
            return customStyle.TryGetValue(new CustomStyleProperty<string>(key), out var stringResult) &&
                   HelixConvert.ToVector4(stringResult, out result);
        }
    }
}