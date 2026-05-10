using System;
using System.Collections.Generic;
using HELIX.Widgets.Utilities;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
  [UxmlObject]
  [RequireDerived]
  public abstract partial class ThemeComponent : ICloneable {
    protected IReadOnlyList<ThemeProperty> lookupScope;

    public virtual object Clone() {
      return MemberwiseClone();
    }

    public virtual void Apply(IdentityDictionary<ThemeProperty, object> dict, bool clearExisting = false) {
      Companion.Apply(this, dict, clearExisting);
    }

    public void ApplyGlobal(bool clearExisting = true) {
      Apply(ThemeProviderElement.GlobalThemeValues, clearExisting);
      ThemeProviderElement.NotifyGlobalThemeUpdate();
    }

    public static class Companion {
      public static void Apply(
        ThemeComponent instance,
        IdentityDictionary<ThemeProperty, object> dict,
        bool clearExisting = false
      ) {
        var type = instance.GetType();
        foreach (var property in instance.lookupScope) {
          var match = property.TryExtractComponent(type, instance, out var wrapped);
          if (!match) continue;
          ApplyValue(property, wrapped, dict, clearExisting);
        }
      }

      public static bool TryUnwrapValue(object value, out object extracted) {
        extracted = null;
        switch (value) {
          case IMaybeThemeValue optional:
            if (!optional.TryGetThemeValue(out var optionalValue)) return false;
            extracted = optionalValue;
            return true;
          case null: return false;
          default:
            extracted = value;
            return true;
        }
      }

      public static void ApplyValue(
        ThemeProperty key,
        object value,
        IdentityDictionary<ThemeProperty, object> dict,
        bool clearExisting
      ) {
        if (TryUnwrapValue(value, out var extracted)) dict[key] = extracted;
        else if (clearExisting) dict.Remove(key);
      }
    }
  }
}