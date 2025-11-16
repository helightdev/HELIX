using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HELIX.Widgets.Theming;
using UnityEngine;

namespace HELIX.Widgets.Editor {
    public static class WidgetFactoryCollection {
        private static Dictionary<Type, List<string>> _loadedDictionary;

        public static List<string> GetCollectionsOfType(Type type) {
            if (_loadedDictionary == null) Load();
            return _loadedDictionary.GetValueOrDefault(type, new List<string> { "None" });
        }

        private static Type FindFactoryGenericArgument(Type t) {
            while (t != null && t != typeof(object)) {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(WidgetFactory<>))
                    return t.GetGenericArguments()[0];
                t = t.BaseType;
            }
            return null;
        }

        private static void Load() {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => {
                    try {
                        return a.GetTypes();
                    } catch (ReflectionTypeLoadException e) {
                        return e.Types.Where(t => t != null);
                    }
                })
                .Where(t =>
                    t.IsClass &&
                    t.GetCustomAttribute<UxmlWidgetFactoryAttribute>() != null)
                .ToArray();

            var dict = new Dictionary<Type, List<string>>();
            foreach (var type in types) {
                Debug.Log($"Found WidgetFactory: {type.FullName}");
                var associated = FindFactoryGenericArgument(type);
                if (associated == null) {
                    Debug.LogWarning($"WidgetFactory {type.FullName} does not inherit from WidgetFactory<T> and will be ignored.");
                    continue;
                }
                var typeList = dict.GetValueOrDefault(associated, new List<string>());
                typeList.Add(type.FullName);
                dict[associated] = typeList;
            }

            foreach (var list in dict.Values) {
                list.Sort();
                list.Insert(0, "None");
            }

            _loadedDictionary = dict;
        }
    }
}