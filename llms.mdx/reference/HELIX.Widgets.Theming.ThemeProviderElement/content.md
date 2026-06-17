# ThemeProviderElement (/reference/HELIX.Widgets.Theming.ThemeProviderElement)

# ThemeProviderElement

```
[UxmlElement]
public class ThemeProviderElement : SingleChildWidgetBaseElement<HThemeProvider>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, ISingleChildContainer, IThemeProvider
```

## GlobalThemeValues

```
public static readonly IdentityDictionary<ThemeProperty, object> GlobalThemeValues
```

## ThemeProviderElement()

```
public ThemeProviderElement()
```

## ThemeValues

```
public Dictionary<ThemeProperty, object> ThemeValues { get; }
```

## Components

```
[UxmlObjectReference]
public List<ThemeComponent> Components { get; set; }
```

## GetThemed<T>(BaseThemeProperty<T>, bool)

```
public override T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true)
```

Resolves the theme value for the given property.

## TryGetThemed<S>(BaseThemeProperty<S>, out S, bool)

```
public override bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true)
```

Tries to resolve the theme value for the given property.

## OnThemeUpdated

```
public event Action OnThemeUpdated
```

## OnGlobalThemeChanged

```
public static event Action OnGlobalThemeChanged
```

## OnAttached(AttachToPanelEvent)

```
protected override void OnAttached(AttachToPanelEvent evt)
```

## OnDetached(DetachFromPanelEvent)

```
protected override void OnDetached(DetachFromPanelEvent evt)
```

## NotifyThemeUpdate(bool)

```
public void NotifyThemeUpdate(bool fromListener = false)
```

## NotifyGlobalThemeUpdate()

```
public static void NotifyGlobalThemeUpdate()
```

## Resolve<T>(BaseThemeProperty<T>, bool)

```
public T Resolve<T>(BaseThemeProperty<T> property, bool computed = true)
```

## TryResolve<T>(BaseThemeProperty<T>, out T, bool)

```
public bool TryResolve<T>(BaseThemeProperty<T> property, out T value, bool computed = true)
```

## Set(ThemeProperty, object, bool)

```
public void Set(ThemeProperty property, object value, bool notify = true)
```

## Unset(ThemeProperty, bool)

```
public void Unset(ThemeProperty property, bool notify = true)
```

## Set<T>(BaseThemeProperty<T>, T, bool)

```
public void Set<T>(BaseThemeProperty<T> property, T value, bool notify = true)
```

## Apply(HThemeProvider, HThemeProvider)

```
public override void Apply(HThemeProvider previous, HThemeProvider widget)
```

## SetGlobal(ThemeProperty, object, bool)

```
public static void SetGlobal(ThemeProperty property, object value, bool notify = true)
```

## UnsetGlobal(ThemeProperty, bool)

```
public static void UnsetGlobal(ThemeProperty property, bool notify = true)
```

## SetGlobal<T>(BaseThemeProperty<T>, T, bool)

```
public static void SetGlobal<T>(BaseThemeProperty<T> property, T value, bool notify = true)
```

## Get(VisualElement)

```
public static ThemeProviderElement Get(VisualElement element)
```

## Resolve<T>(ThemeProviderElement, BaseThemeProperty<T>)

```
public static T Resolve<T>(ThemeProviderElement providerElement, BaseThemeProperty<T> property)
```

## TryResolve<T>(ThemeProviderElement, BaseThemeProperty<T>, out T)

```
public static bool TryResolve<T>(ThemeProviderElement providerElement, BaseThemeProperty<T> property, out T value)
```