using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HELIX.Widgets.Editor {
    public static class ThemePropertyCollection {
        private static List<string> _loadedCollections;
        private static Dictionary<Type, List<string>> _loadedDictionary;

        public static List<string> LoadedCollections {
            get {
                if (_loadedCollections == null) Load();
                return _loadedCollections;
            }
        }
        
        public static List<string> GetCollectionsOfType(Type type) {
            if (_loadedDictionary == null) Load();
            return _loadedDictionary.GetValueOrDefault(type, new List<string> { "None" });
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

            var allTypes = new List<string>();
            var dict = new Dictionary<Type, List<string>>();
            foreach (var type in types) {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static)) {
                    var name = $"{type.FullName}:{field.Name}";
                    allTypes.Add(name);
                    var generic = field.FieldType.GetGenericArguments()[0];
                    var typeList = dict.GetValueOrDefault(generic, new List<string>());
                    typeList.Add(name);
                    dict[generic] = typeList;
                }
            }
            allTypes.Sort();
            allTypes.Insert(0, "None");
            foreach (var list in dict.Values) {
                list.Sort();
                list.Insert(0, "None");
            }
            
            _loadedCollections = allTypes;
            _loadedDictionary = dict;
        }
    }
}