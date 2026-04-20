using System;
using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
  public abstract class ThemeProperty : DiagnosticableBase {
    public static readonly Dictionary<Type, IThemeStyleValueLoader> Loaders = new() {
      [typeof(float)] = new FloatThemeStyleValueLoader(),
      [typeof(int)] = new IntThemeStyleValueLoader(),
      [typeof(string)] = new StringThemeStyleValueLoader(),
      [typeof(bool)] = new BoolThemeStyleValueLoader(),
      [typeof(Color)] = new ColorThemeStyleValueLoader(),
      [typeof(Texture2D)] = new Texture2DThemeStyleValueLoader(),
      [typeof(Sprite)] = new SpriteThemeStyleValueLoader(),
      [typeof(VectorImage)] = new VectorImageThemeStyleValueLoader(),
      [typeof(Vector2)] = new Vector2ThemeStyleValueLoader(),
      [typeof(Vector3)] = new Vector3ThemeStyleValueLoader(),
      [typeof(Vector4)] = new Vector4ThemeStyleValueLoader()
    };

    protected readonly string key;
    protected bool isDefaultValid;

    protected ThemeProperty(string key) {
      this.key = key;
    }

    protected ThemeProperty() { }

    public virtual string Key => key;
    public virtual string ComputedStyleName => Key == null ? null : $"--{Key}";
    public virtual object DefaultValue => null;
    public virtual bool IsDefaultValid => isDefaultValid;

    public virtual bool TryExtractComponent(Type type, ThemeComponent component, out object value) {
      value = null;
      return false;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      properties.Add(new StringProperty("key", Key));
      properties.Add(new DiagnosticsProperty<object>("defaultValue", DefaultValue));
      properties.Add(new FlagProperty("isDefaultValid", IsDefaultValid, ifFalse: "Invalid"));
    }

    public static ThemeProperty<T> ExtractMaybe<T, V>(
      V value,
      Func<V, IMaybeThemeValue<T>> resolver
    ) where V : ThemeComponent {
      return new ThemeProperty<T>().ComponentExtractorDefault(value, resolver);
    }

    public static ThemeProperty<T> ExtractMaybe<T, V>(
      string key,
      V value,
      Func<V, IMaybeThemeValue<T>> resolver
    ) where V : ThemeComponent {
      return new ThemeProperty<T>(key).ComponentExtractorDefault(value, resolver);
    }

    public static ThemeProperty<T> Extract<T, V>(
      V value,
      Func<V, T> resolver
    ) where V : ThemeComponent {
      return new ThemeProperty<T>().ComponentExtractorDefault(value, resolver);
    }

    public static ThemeProperty<T> Extract<T, V>(
      string key,
      V value,
      Func<V, T> resolver
    ) where V : ThemeComponent {
      return new ThemeProperty<T>(key).ComponentExtractorDefault(value, resolver);
    }
  }

  public abstract class BaseThemeProperty<T> : ThemeProperty {
    protected T defaultValue;

    protected BaseThemeProperty(string key, T defaultValue) : base(key) {
      this.defaultValue = defaultValue;
      isDefaultValid = true;
    }

    protected BaseThemeProperty(string key) : base(key) { }

    protected BaseThemeProperty() { }

    public override object DefaultValue => defaultValue;

    public T TypedDefaultValue => defaultValue;

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      properties.Add(new StringProperty("key", Key));
      properties.Add(new DiagnosticsProperty<T>("defaultValue", defaultValue));
      properties.Add(new FlagProperty("isDefaultValid", IsDefaultValid, ifFalse: "InvalidDefaultValue"));
    }

    public virtual bool ResolveStyle(ICustomStyle customStyle, out T result) {
      result = defaultValue;
      return false;
    }

    public virtual bool TryCompute(ThemeProviderElement provider, out T result) {
      result = defaultValue;
      return false;
    }
  }

  public class ThemeProperty<T> : BaseThemeProperty<T> {
    private Dictionary<Type, Func<object, object>> _componentExtractors;
    private Func<ThemeProviderElement, T> _computeFunc;
    private IThemeStyleValueLoader<T> _styleLoader;
    private string _styleName;

    public ThemeProperty(
      string key = null,
      object defaultValue = null,
      string styleName = null
    ) : base(key) {
      _styleName = styleName;
      if (defaultValue is T typed) {
        this.defaultValue = typed;
        isDefaultValid = true;
      }
    }

    public ThemeProperty<T> StyleLoader(string styleName = null, IThemeStyleValueLoader<T> loader = null) {
      if (styleName != null) _styleName = styleName;

      if (loader != null) {
        _styleLoader = loader;
        return this;
      }

      var hasLoader = Loaders.TryGetValue(typeof(T), out var resolved);
      if (!hasLoader) throw new ArgumentException($"No style loader found for type: {typeof(T)}");
      if (resolved is not IThemeStyleValueLoader<T> typedLoader) {
        throw new ArgumentException(
          $"Style loader {resolved} for type: {typeof(T)} has invalid type signature."
        );
      }

      _styleLoader = typedLoader;
      return this;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      var styleLoaderLevel = _styleLoader != null ? DiagnosticLevel.Info : DiagnosticLevel.Fine;
      properties.Add(new StringProperty("styleName", _styleName ?? ComputedStyleName, level: styleLoaderLevel));
      properties.Add(
        new DiagnosticsProperty<IThemeStyleValueLoader<T>>("styleLoader", _styleLoader, level: styleLoaderLevel)
      );
      properties.Add(new IterableProperty<Type>("extractors", _componentExtractors.Keys, ifEmpty: "None"));
    }

    public ThemeProperty<T> ComponentExtractor<TypeOfComponent>(Func<TypeOfComponent, T> extractor) {
      _componentExtractors ??= new Dictionary<Type, Func<object, object>>();
      _componentExtractors[typeof(TypeOfComponent)] = component => extractor((TypeOfComponent)component);
      return this;
    }

    public ThemeProperty<T> ComponentExtractor<TypeOfComponent, TypeOfMaybe>(
      Func<TypeOfComponent, TypeOfMaybe> extractor
    ) where TypeOfMaybe : IMaybeThemeValue<T> {
      _componentExtractors ??= new Dictionary<Type, Func<object, object>>();
      _componentExtractors[typeof(TypeOfComponent)] = component => extractor((TypeOfComponent)component);
      return this;
    }

    public ThemeProperty<T> ComponentExtractorDefault<TypeOfComponent>(
      TypeOfComponent value,
      Func<TypeOfComponent, T> extractor
    ) {
      ComponentExtractor(extractor);
      defaultValue = extractor(value);
      return this;
    }

    public ThemeProperty<T> ComponentExtractorDefault<TypeOfComponent, TypeOfMaybe>(
      TypeOfComponent value,
      Func<TypeOfComponent, TypeOfMaybe> extractor
    ) where TypeOfMaybe : IMaybeThemeValue<T> {
      ComponentExtractor(extractor);
      var maybeThemeValue = extractor(value);
      isDefaultValid = maybeThemeValue?.TryGetThemeValueTyped(out defaultValue) ?? false;
      return this;
    }

    public ThemeProperty<T> Compute(Func<ThemeProviderElement, T> computeFunc) {
      _computeFunc = computeFunc;
      return this;
    }

    public override bool ResolveStyle(ICustomStyle customStyle, out T result) {
      if (_styleLoader == null) {
        result = defaultValue;
        return false;
      }

      var styleName = _styleName ?? ComputedStyleName;
      if (string.IsNullOrEmpty(styleName)) {
        Debug.LogWarning(
          $"No style name or valid key provided for ThemeProperty: {this}, but style loader is set. Unable to resolve style value."
        );
        result = defaultValue;
        return false;
      }

      if (_styleLoader.Load(styleName, customStyle, out result)) return true;
      result = defaultValue;
      return false;
    }

    public override bool TryCompute(ThemeProviderElement provider, out T result) {
      if (_computeFunc != null) {
        result = _computeFunc(provider);
        return true;
      }

      result = defaultValue;
      return false;
    }

    public override bool TryExtractComponent(Type type, ThemeComponent component, out object value) {
      value = null;
      if (_componentExtractors == null) return false;
      if (!_componentExtractors.TryGetValue(type, out var extractor)) return false;
      value = extractor(component);
      return true;
    }
  }
}