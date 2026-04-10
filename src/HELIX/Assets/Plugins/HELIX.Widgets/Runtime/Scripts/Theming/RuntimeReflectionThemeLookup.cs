using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HELIX.Widgets.Theming {
    public static class RuntimeReflectionThemeLookup {
        private static readonly Dictionary<string, ThemeProperty> _lookupCache = new();
        private static readonly Dictionary<string, ElementFactory> _elementFactoryCache = new();

        public static Type FindType(string typeName) {
            if (string.IsNullOrWhiteSpace(typeName)) return null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var t = asm.GetType(typeName, false, false);
                if (t != null) return t;
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    var t = asm.GetTypes().FirstOrDefault(x => x.FullName == typeName)
                         ?? asm.GetTypes().FirstOrDefault(x => x.Name == typeName);
                    if (t != null) return t;
                } catch (ReflectionTypeLoadException) { }
            }

            return null;
        }

        public static ElementFactory GetFactory(string reference) {
            if (string.IsNullOrWhiteSpace(reference) || reference == "None") return null;
            if (_elementFactoryCache.TryGetValue(reference, out var cachedFactory)) return cachedFactory;

            var type = FindType(reference);
            if (type == null) {
                Debug.LogWarning($"Cannot find widget factory for reference {reference}");
                return null;
            }

            if (Activator.CreateInstance(type) is not ElementFactory factory) {
                Debug.LogWarning($"Widget factory reference {reference} is not a valid WidgetFactory");
                return null;
            }

            _elementFactoryCache[reference] = factory;
            return factory;
        }

        public static ThemeProperty GetProperty(string reference) {
            if (reference is null or "None") return null;
            if (_lookupCache.TryGetValue(reference, out var cachedProperty)) return cachedProperty;

            var arr = reference.Split(":");
            if (arr.Length != 2) {
                Debug.LogWarning($"Invalid theme property reference format: {reference}");
                return null;
            }

            var property = FindType(arr[0])?.GetField(arr[1])?.GetValue(null);
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

        public static BaseThemeProperty<T> GetProperty<T>(string reference) {
            return GetProperty(reference) as BaseThemeProperty<T>;
        }
    }
}