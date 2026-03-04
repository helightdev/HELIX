using System;
using System.Linq;
using System.Reflection;
using HELIX.Widgets.Theming;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Editor {
    public static class PolymorphicUxmlConverter {
        private static MethodInfo _convertToStringMethod;
        private static MethodInfo _convertFromStringMethod;
        private static bool _isLoaded;

        private static void Load() {
            if (_isLoaded) return;

            var type = Type.GetType("UnityEditor.UIElements.UxmlAttributeConverter, UnityEditor.UIElementsModule");
            if (type == null) throw new InvalidOperationException("Cannot find UxmlAttributeConverter type via reflection.");

            _convertFromStringMethod = type.GetMethods()
                .FirstOrDefault(x => x.Name == "TryConvertFromString"
                                  && x.IsStatic
                                  && x.ReturnParameter?.ParameterType ==
                                     typeof(bool)
                                  && x.GetParameters().Length == 4
                );
            if (_convertFromStringMethod == null)
                throw new InvalidOperationException("Cannot find UxmlAttributeConverter.TryConvertFromString method via reflection.");

            _convertToStringMethod = type.GetMethods()
                .FirstOrDefault(x => x.Name == "TryConvertToString"
                                  && x.IsStatic
                                  && x.ReturnParameter?.ParameterType ==
                                     typeof(bool)
                                  && x.GetParameters().Length == 3
                );

            if (_convertToStringMethod == null)
                throw new InvalidOperationException("Cannot find UxmlAttributeConverter.TryConvertToString method via reflection.");

            _isLoaded = true;
        }

        public static bool TryConvertFromString(Type type, string value, out object result) {
            Load();
            var actualType = type;
            if (type == typeof(Texture2D) || type == typeof(Sprite) || type == typeof(VectorImage)) actualType = typeof(Background);

            var args = new object[] { actualType, value, CreationContext.Default, null };
            var success = (bool)_convertFromStringMethod.Invoke(null, args);
            result = success ? args[3] : null;
            switch (result) {
                case Background background when type == typeof(Texture2D):   result = background.texture; break;
                case Background background when type == typeof(Sprite):      result = background.sprite; break;
                case Background background when type == typeof(VectorImage): result = background.vectorImage; break;
            }

            return success;
        }

        public static bool TryConvertToString(object value, out string result) {
            Load();
            value = value switch {
                Texture2D texture       => Background.FromTexture2D(texture),
                Sprite sprite           => Background.FromSprite(sprite),
                VectorImage vectorImage => Background.FromVectorImage(vectorImage),
                _                       => value
            };

            var args = new[] { value, null, null };
            var success = (bool)_convertToStringMethod.Invoke(null, args);
            result = success ? (string)args[2] : null;
            return success;
        }
    }

    public class ThemeOverrideAttributeConverter<T> : UxmlAttributeConverter<ThemeOverride<T>> {
        public override ThemeOverride<T> FromString(string value) {
            try {
                value = value.Trim();
                if (value == "none") return new ThemeOverride<T> { type = ThemeOverrideType.None };

                if (value.StartsWith("value:")) {
                    var constString = value["value:".Length..].TrimStart();
                    var success = PolymorphicUxmlConverter.TryConvertFromString(typeof(T), constString, out var result);
                    if (!success) throw new InvalidOperationException($"Cannot convert constant value string to type {typeof(T)}.");

                    return new ThemeOverride<T> {
                        type = ThemeOverrideType.Value,
                        constantValue = (T)result
                    };
                }

                if (value.StartsWith("ref:")) {
                    var reference = value["ref:".Length..].Trim();
                    return new ThemeOverride<T> {
                        type = ThemeOverrideType.PropertyReference,
                        propertyReference = reference
                    };
                }

                throw new FormatException($"Invalid ThemeOverrides format: {value}");
            } catch (Exception e) {
                Debug.LogError($"Error parsing ThemeOverride from string: {e}");
                return new ThemeOverride<T> { type = ThemeOverrideType.None };
            }
        }

        public override string ToString(ThemeOverride<T> value) {
            switch (value.type) {
                case ThemeOverrideType.None: return "none";
                case ThemeOverrideType.Value:
                    try {
                        var success = PolymorphicUxmlConverter.TryConvertToString(value.constantValue, out var result);
                        if (success) return "value: " + result;

                        throw new InvalidOperationException($"Cannot convert constant value of type {typeof(T)} to string.");
                    } catch (Exception e) {
                        Debug.LogError($"Error converting constant value to string: {e}");
                        return "value: [unconvertible value]";
                    }
                case ThemeOverrideType.PropertyReference: return "ref: " + value.propertyReference;
                default:                                  throw new ArgumentOutOfRangeException();
            }
        }
    }
}