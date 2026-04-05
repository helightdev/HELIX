using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    [UxmlObject, RequireDerived]
    public abstract partial class WidgetThemeComponent : ICloneable {
        public virtual bool Resolve(string key, out object value) {
            value = null;
            var companion = Companion.Get(GetType());
            if (!companion.TryGetField(key, out var accessor)) return false;
            value = accessor.getter(this);
            return Companion.TryExtractValue(value, out value);
        }

        public virtual void Apply(Dictionary<string, object> dict, bool clearExisting = false) {
            Companion.Get(GetType()).Apply(this, dict, clearExisting);
        }

        public void ApplyGlobal(bool clearExisting = true) {
            Apply(WidgetThemeProvider.GlobalThemeValues, clearExisting);
            WidgetThemeProvider.NotifyGlobalThemeUpdate();
        }

        public class Companion {
            private static readonly Dictionary<Type, Companion> _cache = new();

            public static Companion Get(Type type) {
                if (_cache.TryGetValue(type, out var resolver)) return resolver;
                resolver = new Companion(type);
                _cache[type] = resolver;
                return resolver;
            }

            public readonly FieldAccessors[] fieldAccessors;

            private Companion(Type type) {
                var fields = type.GetFields().Where(info => info.IsPublic && !info.IsStatic).ToArray();
                fieldAccessors = new FieldAccessors[fields.Length];

                for (var index = 0; index < fields.Length; index++) {
                    var info = fields[index];
                    var attributeName = info.Name;

                    var attributeAttr = info.GetCustomAttributes<UxmlAttributeAttribute>().FirstOrDefault();
                    if (attributeAttr != null) { attributeName = attributeAttr.name; }

                    var objectReferenceAttr = info.GetCustomAttributes<UxmlObjectReferenceAttribute>().FirstOrDefault();
                    if (objectReferenceAttr != null) { attributeName = objectReferenceAttr.name; }

                    fieldAccessors[index] = new FieldAccessors {
                        name = attributeName,
                        getter = info.GetValue,
                    };
                }
            }

            public bool TryGetField(string name, out FieldAccessors accessor) {
                foreach (var fieldAccessor in fieldAccessors) {
                    if (fieldAccessor.name != name) continue;
                    accessor = fieldAccessor;
                    return true;
                }

                accessor = default;
                return false;
            }

            public void Apply(object instance, Dictionary<string, object> dict, bool clearExisting = false) {
                foreach (var accessor in fieldAccessors) {
                    var value = accessor.getter(instance);
                    ApplyValue(accessor.name, value, dict, clearExisting);
                }
            }

            public static bool TryExtractValue(object value, out object extracted) {
                extracted = null;
                switch (value) {
                    case IWidgetFactoryReference reference:
                        var widgetFactory = reference.LookupFactory();
                        if (widgetFactory == null) return false;
                        extracted = widgetFactory;
                        return true;
                    case ThemeOverride overrides:
                        if (!overrides.TryGetOverride(out var overrideValue)) return false;
                        extracted = overrideValue;
                        return true;
                    case ThemeOptional optional:
                        if (!optional.TryGetValue(out var optionalValue)) return false;
                        extracted = optionalValue;
                        return true;
                    case null: return false;
                    default:
                        extracted = value;
                        return true;
                }
            }

            public static void ApplyValue(
                string key,
                object value,
                Dictionary<string, object> dict,
                bool clearExisting
            ) {
                if (TryExtractValue(value, out var extracted)) { dict[key] = extracted; } else if (clearExisting) {
                    dict.Remove(key);
                }
            }

            public struct FieldAccessors {
                public string name;
                public Func<object, object> getter;
            }
        }

        public virtual object Clone() {
            return MemberwiseClone();
        }
    }
}