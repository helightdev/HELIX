# ElementFactorySlot<T> (/reference/HELIX.Widgets.Theming.ElementFactorySlot-1)

# ElementFactorySlot<T>

```
public class ElementFactorySlot<T> : ElementFactorySlot, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IPublicElementFactorySlot<T> where T : VisualElement
```

## ElementFactorySlot(BaseElement)

```
public ElementFactorySlot(BaseElement widget)
```

## ElementFactorySlot(BaseElement, BaseThemeProperty<ElementFactory<T>>)

```
public ElementFactorySlot(BaseElement widget, BaseThemeProperty<ElementFactory<T>> baseThemeProperty)
```

## Reference

```
public ElementFactoryReference<T> Reference { get; set; }
```

## SetMapped(ElementFactory<T>)

```
public void SetMapped(ElementFactory<T> value)
```

## GetMapped<TMapped>()

```
public TMapped GetMapped<TMapped>() where TMapped : ElementFactory<T>
```

## Element

```
public T Element { get; }
```

## HasElement

```
public override bool HasElement { get; }
```

## OnElementCreated

```
public event ElementFactorySlot<T>.OnElementCreatedDelegate OnElementCreated
```

## OnElementDestroyed

```
public event ElementFactorySlot<T>.OnElementDestroyedDelegate OnElementDestroyed
```

## ApplyReferenceFromTheme()

```
public override void ApplyReferenceFromTheme()
```

## TryCreate()

```
public override void TryCreate()
```

## Recreate()

```
public override void Recreate()
```

## Destroy()

```
public override void Destroy()
```

## SetFallback(ElementFactory)

```
public void SetFallback(ElementFactory fallback)
```