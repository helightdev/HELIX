# BaseElement (/reference/HELIX.Widgets.Elements.BaseElement)

# BaseElement

```
public abstract class BaseElement : VisualElement, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IElement
```

## UssClassName

```
public static readonly string UssClassName
```

## BaseElement()

```
protected BaseElement()
```

## HierarchyDepth

```
public int HierarchyDepth { get; protected set; }
```

## ThemeProviderElement

```
public ThemeProviderElement ThemeProviderElement { get; }
```

## Element

```
public VisualElement Element { get; }
```

## ~BaseElement()

```
protected ~BaseElement()
```

## ThemeValue<T>(BaseThemeProperty<T>)

```
protected ThemeValue<T> ThemeValue<T>(BaseThemeProperty<T> property)
```

## ThemeValue<T>(BaseThemeProperty<T>, OnValueChangedDelegate, bool)

```
protected ThemeValue<T> ThemeValue<T>(BaseThemeProperty<T> property, ThemeValue<T>.OnValueChangedDelegate onValueChanged, bool applyCurrentValue = true)
```

## ContainsThemeValue(ThemeProperty)

```
protected bool ContainsThemeValue(ThemeProperty property)
```

## RemoveThemeValue(ThemeValue)

```
protected void RemoveThemeValue(ThemeValue themeValue)
```

## RemoveThemeValue(ThemeProperty)

```
protected void RemoveThemeValue(ThemeProperty property)
```

## RegisterThemeValue<T>(ThemeValue<T>)

```
protected virtual ThemeValue<T> RegisterThemeValue<T>(ThemeValue<T> themeValue)
```

## WidgetFactorySlot<T>(OnElementCreatedDelegate, OnElementDestroyedDelegate, ElementFactory<T>)

```
protected ElementFactorySlot<T> WidgetFactorySlot<T>(ElementFactorySlot<T>.OnElementCreatedDelegate onCreated = null, ElementFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null, ElementFactory<T> fallback = null) where T : VisualElement
```

## WidgetFactorySlot<T>(BaseThemeProperty<ElementFactory<T>>, OnElementCreatedDelegate, OnElementDestroyedDelegate, ElementFactory<T>)

```
protected ElementFactorySlot<T> WidgetFactorySlot<T>(BaseThemeProperty<ElementFactory<T>> property, ElementFactorySlot<T>.OnElementCreatedDelegate onCreated = null, ElementFactorySlot<T>.OnElementDestroyedDelegate onDestroyed = null, ElementFactory<T> fallback = null) where T : VisualElement
```

## DeleteFactorySlot(ElementFactorySlot)

```
public bool DeleteFactorySlot(ElementFactorySlot slot)
```

## OnAttached(AttachToPanelEvent)

```
protected virtual void OnAttached(AttachToPanelEvent evt)
```

## OnDetached(DetachFromPanelEvent)

```
protected virtual void OnDetached(DetachFromPanelEvent evt)
```

## OnThemeUpdated()

```
protected virtual void OnThemeUpdated()
```

## OnWatchedThemeUpdated(ThemeProperty, object)

```
protected virtual void OnWatchedThemeUpdated(ThemeProperty property, object value)
```