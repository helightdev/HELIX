using System;
using System.Collections.Generic;
using UnityEngine;

namespace HELIX.Widgets.Theming {
    public static class RuntimeReflectionThemeLookup {
        private static readonly Dictionary<string, ThemeProperty> _lookupCache = new();

        public static ThemeProperty GetProperty(string reference) {
            if (reference == "None") return null;
            if (_lookupCache.TryGetValue(reference, out var cachedProperty)) {
                return cachedProperty;
            }

            var arr = reference.Split(":");
            if (arr.Length != 2) {
                Debug.LogWarning($"Invalid theme property reference format: {reference}");
                return null;
            }
            var property = Type.GetType(arr[0])?.GetField(arr[1])?.GetValue(null);
            if (property == null) {
                Debug.LogWarning($"Cannot find theme property for reference {reference}");
                return null;
            }

            var themeProperty = property as ThemeProperty;
            if (themeProperty == null) {
                Debug.LogWarning($"Theme property reference {reference} is not a valid ThemeProperty");
                return null;
            }

            _lookupCache[reference] = themeProperty;
            return themeProperty;
        }

        public static ThemeProperty<T> GetProperty<T>(string reference) {
            return GetProperty(reference) as ThemeProperty<T>;
        }
    }
}