using System;
using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Utilities;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {

  public interface ThemeProviderNode : IThemeProvider {
    public static readonly IdentityDictionary<ThemeProperty, object> GlobalThemeValues = new();
    private static readonly IdentityDictionary<ThemeProperty, object> _globalComputedThemeValues = new();

    ThemeProviderNode Parent { get; }

    event Action OnThemeUpdated;
    static event Action OnGlobalThemeChanged;

    bool TryResolve<T>(BaseThemeProperty<T> property, out T value, bool computed = true);

    static ThemeProviderNode Get(VisualElement element) {
      return element.GetFirstAncestorOfType<ThemeProviderNode>();
    }

    static T Resolve<T>(ThemeProviderNode providerNode, BaseThemeProperty<T> property) {
      TryResolve(providerNode, property, out var value);
      return value;
    }

    static bool TryResolve<T>(
      ThemeProviderNode providerNode,
      BaseThemeProperty<T> property,
      out T value
    ) {
      if (providerNode != null) return providerNode.TryResolve(property, out value);
      if (GlobalThemeValues.TryGetValue(property, out var globalValue) && globalValue is T typedGlobalValue) {
        value = typedGlobalValue;
        return true;
      }

      if (_globalComputedThemeValues.TryGetValue(property, out var cachedValue)) {
        if (cachedValue is T typedCachedValue) {
          value = typedCachedValue;
          return true;
        }

        value = property.TypedDefaultValue;
        return false;
      }

      if (property.TryCompute(null, out var computedValue)) {
        _globalComputedThemeValues[property] = computedValue;
        value = computedValue;
        return true;
      }

      _globalComputedThemeValues[property] = null;
      value = property.TypedDefaultValue;
      return false;
    }

    static void SetGlobal(ThemeProperty property, object value, bool notify = true) {
      GlobalThemeValues[property] = value;
      _globalComputedThemeValues.Clear();
      if (notify) OnGlobalThemeChanged?.Invoke();
    }

    static void UnsetGlobal(ThemeProperty property, bool notify = true) {
      if (!GlobalThemeValues.Remove(property)) return;
      _globalComputedThemeValues.Clear();
      if (notify) OnGlobalThemeChanged?.Invoke();
    }

    static void SetGlobal<T>(BaseThemeProperty<T> property, T value, bool notify = true) {
      GlobalThemeValues[property] = value;
      _globalComputedThemeValues.Clear();
      if (notify) OnGlobalThemeChanged?.Invoke();
    }

    static void NotifyGlobalThemeUpdate() {
      ModificationBarrier.Run(() => { OnGlobalThemeChanged?.Invoke(); });
    }
  }

  public abstract class ThemeProviderNodeBase<TWidget> : SingleChildWidgetBaseElement<TWidget>, ThemeProviderNode
    where TWidget : SingleChildWidget {

    protected readonly IdentityDictionary<ThemeProperty, object> cachedThemeValues = new();
    protected readonly IdentityDictionary<ThemeProperty, object> componentValues = new();
    protected readonly IdentityDictionary<ThemeProperty, object> computedThemeValues = new();
    protected readonly IdentityDictionary<ThemeProperty, object> themeValues  = new();

    public ThemeProviderNode Parent { get; set; } = null;
    public event Action OnThemeUpdated;


    public ThemeProviderNodeBase() {
      RegisterCallback<CustomStyleResolvedEvent>(_ => { NotifyThemeUpdate(); });
    }

    public override T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true) {
      return Resolve(property);
    }

    public override bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true) {
      return TryResolve(property, out value);
    }

    protected override void OnAttached(AttachToPanelEvent evt) {
      base.OnAttached(evt);
      Parent = ThemeProviderNode.Get(this);
      ThemeProviderNode.OnGlobalThemeChanged += ListenerNotifyThemeUpdate;
      if (Parent != null) Parent.OnThemeUpdated += ListenerNotifyThemeUpdate;
    }

    protected override void OnDetached(DetachFromPanelEvent evt) {
      base.OnDetached(evt);
      ThemeProviderNode.OnGlobalThemeChanged -= ListenerNotifyThemeUpdate;
      if (Parent != null) Parent.OnThemeUpdated -= ListenerNotifyThemeUpdate;
      Parent = null;
    }

    protected virtual void ListenerNotifyThemeUpdate() {
      NotifyThemeUpdate(true);
    }

    public void NotifyThemeUpdate(bool fromListener = false) {
      cachedThemeValues.Clear();
      computedThemeValues.Clear();
      ModificationBarrier.Run(RunThemeUpdate);
    }

    private void RunThemeUpdate() {
      OnThemeUpdated?.Invoke();
    }

    public T Resolve<T>(BaseThemeProperty<T> property, bool computed = true) {
      if (TryResolve(property, out var value, computed)) return value;
      return property.TypedDefaultValue;
    }

    public bool TryResolve<T>(BaseThemeProperty<T> property, out T value, bool computed = true) {
      var success = TryResolveNoCompute(property, out value);
      if (!computed || success) return success;


      if (computedThemeValues.TryGetValue(property, out var cachedValue)) {
        if (cachedValue is not T typedCachedValue) return false;
        value = typedCachedValue;
        return true;
      }

      if (property.TryCompute(this, out var computedValue)) {
        computedThemeValues[property] = computedValue;
        value = computedValue;
        return true;
      }

      cachedThemeValues[property] = null;
      return false;
    }

    private bool TryResolveNoCompute<T>(BaseThemeProperty<T> property, out T value) {
      value = property.TypedDefaultValue;
      if (cachedThemeValues.TryGetValue(property, out var cachedValue)) {
        if (cachedValue is not T typedCachedValue) return false;
        value = typedCachedValue;
        return true;
      }

      var resolvedValue = ResolveInternal(property);
      cachedThemeValues[property] = resolvedValue;
      if (resolvedValue is not T typedResolvedValue) return false;
      value = typedResolvedValue;
      return true;
    }

    private object ResolveInternal<T>(BaseThemeProperty<T> property) {
      if (themeValues.TryGetValue(property, out var value)) return value;

      if (cachedThemeValues != null && componentValues.TryGetValue(property, out var componentValue))
        return componentValue;

      if (property.ResolveStyle(customStyle, out var resolvedValue)) return resolvedValue;

      if (Parent != null) {
        var success = Parent.TryResolve(property, out var parentResolvedValue, false);
        return success ? parentResolvedValue : null;
      }

      if (ThemeProviderNode.GlobalThemeValues.TryGetValue(property, out var globalValue)) return globalValue;

      if (!property.IsDefaultValid) return null;
      return property.TypedDefaultValue;
    }

    public void Set(ThemeProperty property, object value, bool notify = true) {
      themeValues[property] = value;
      if (notify) NotifyThemeUpdate();
    }

    public void Unset(ThemeProperty property, bool notify = true) {
      if (themeValues.Remove(property) && notify) NotifyThemeUpdate();
    }

    public void Set<T>(BaseThemeProperty<T> property, T value, bool notify = true) {
      themeValues[property] = value;
      if (notify) NotifyThemeUpdate();
    }
  }

  [UxmlElement]
  public partial class ThemeProviderElement : ThemeProviderNodeBase<HThemeProvider> {

    private List<ThemeComponent> _components = new();

    [UxmlObjectReference]
    public List<ThemeComponent> Components {
      get => _components;
      set {
        if (_components == null || value == null) return;
        _components = value;
        componentValues.Clear();
        foreach (var component in value) component?.Apply(componentValues);

        NotifyThemeUpdate();
      }
    }

    public override void Apply(HThemeProvider previous, HThemeProvider widget) {
      themeValues.Clear();

      if (widget.properties != null) {
        foreach (var kvp in widget.properties)
          themeValues[kvp.Key] = kvp.Value;
      }

      Components = new List<ThemeComponent>(widget.components); // This will also update the theme
    }
  }

  /// <summary>
  /// Interface for a component that provides access to theme values.
  /// </summary>
  public interface IThemeProvider {

    /// <summary>
    /// Resolves the theme value for the given property.
    /// </summary>
    /// <param name="property">The theme property to resolve.</param>
    /// <param name="listen">Whether this provider should listen for theme updates.</param>
    /// <returns>The resolved theme value, or the default value if not found.</returns>
    /// <remarks>
    /// The returned value may be invalid for struct types. Use <see cref="TryGetThemed"/> in cases where you need to
    /// be sure that the value is valid and intentionally assigned. If you can't be sure that this provider is
    /// not null, use <see cref="WidgetExtensions.Get"/> instead.
    /// </remarks>
    T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true);

    /// <summary>
    /// Tries to resolve the theme value for the given property.
    /// </summary>
    /// <param name="property">The theme property to resolve.</param>
    /// <param name="value">The resolved theme value, or the default value if not found.</param>
    /// <param name="listen">Whether this provider should listen for theme updates.</param>
    /// <returns><c>true</c> if the value retrieved is valid, otherwise <c>false</c>.</returns>
    /// <remarks>
    /// If you can't be sure that this provider is not null, use <see cref="WidgetExtensions.TryGet"/> instead.
    ///</remarks>
    bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true);
  }

  public class FallbackThemeProvider : IThemeProvider {
    public static readonly FallbackThemeProvider Instance = new();

    public T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true) {
      return ThemeProviderNode.Resolve(null, property);
    }

    public bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true) {
      return ThemeProviderNode.TryResolve(null, property, out value);
    }
  }
}