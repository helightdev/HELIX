using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HELIX.Widgets.Editor {
    public static class ThemePropertyCollection {
        private static List<string> _loadedCollections;

        public static List<string> LoadedCollections {
            get {
                if (_loadedCollections == null) Load();
                return _loadedCollections;
            }
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
                    t.IsAbstract &&
                    t.IsSealed && // identifies static class
                    t.GetCustomAttribute<ThemePropertyCollectionAttribute>() != null)
                .ToArray();

            var strings = types.SelectMany(x => {
                var name = x.FullName;
                return x.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(field => name + ":" + field.Name);
            }).ToList();
            strings.Insert(0, "None");
            _loadedCollections = strings;
        }
    }
}