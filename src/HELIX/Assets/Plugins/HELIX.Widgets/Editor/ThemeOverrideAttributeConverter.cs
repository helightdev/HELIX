using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public static class PolymorphicUxmlConverter {
        private static MethodInfo _convertToStringMethod;
        private static MethodInfo _convertFromStringMethod;
        private static bool _isLoaded;

        private static void Load() {
            if (_isLoaded) return;
            
            var type = Type.GetType(
                "UnityEditor.UIElements.UxmlAttributeConverter, UnityEditor.UIElementsModule");
            if (type == null) {
                throw new InvalidOperationException("Cannot find UxmlAttributeConverter type via reflection.");
            }

            _convertFromStringMethod = type.GetMethods()
                .FirstOrDefault(x => x.Name == "TryConvertFromString"
                                     && x.IsStatic
                                     && x.ReturnParameter?.ParameterType ==
                                     typeof(bool)
                                     && x.GetParameters().Length == 4);
            if (_convertFromStringMethod == null)
                throw new InvalidOperationException(
                    "Cannot find UxmlAttributeConverter.TryConvertFromString method via reflection.");

            _convertToStringMethod = type.GetMethods()
                .FirstOrDefault(x => x.Name == "TryConvertToString"
                                     && x.IsStatic
                                     && x.ReturnParameter?.ParameterType ==
                                     typeof(bool)
                                     && x.GetParameters().Length == 3);

            if (_convertToStringMethod == null) {
                throw new InvalidOperationException(
                    "Cannot find UxmlAttributeConverter.TryConvertToString method via reflection.");
            }
            _isLoaded = true;
        }
        
        public static bool TryConvertFromString(Type type, string value, out object result) {
            Load();
            var args = new object[] { type, value, CreationContext.Default, null };
            var success = (bool)_convertFromStringMethod.Invoke(null, args);
            result = success ? args[3] : null;
            return success;
        }
        
        public static bool TryConvertToString(object value, out string result) {
            Load();
            var args = new object[] { value, null, null };
            var success = (bool)_convertToStringMethod.Invoke(null, args);
            result = success ? (string)args[2] : null;
            return success;
        }
        
    }

    public class ThemeOverrideAttributeConverter<T> : UxmlAttributeConverter<ThemeOverride<T>> {
        public override ThemeOverride<T> FromString(string value) {
            value = value.Trim();
            if (value == "none") {
                return new ThemeOverride<T> { type = ThemeOverrideType.None };
            }

            if (value.StartsWith("value:")) {
                var constString = value["value:".Length..].TrimStart();
                var success = PolymorphicUxmlConverter.TryConvertFromString(typeof(T), constString, out var result);
                if (!success) {
                    throw new InvalidOperationException($"Cannot convert constant value string to type {typeof(T)}.");
                }

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
        }

        public override string ToString(ThemeOverride<T> value) {
            switch (value.type) {
                case ThemeOverrideType.None:
                    return "none";
                case ThemeOverrideType.Value:
                    var success = PolymorphicUxmlConverter.TryConvertToString(value.constantValue, out var result);
                    if (success) {
                        return "value: " + result;
                    }
                    throw new InvalidOperationException(
                        $"Cannot convert constant value of type {typeof(T)} to string.");
                case ThemeOverrideType.PropertyReference:
                    return "ref: " + value.propertyReference;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}