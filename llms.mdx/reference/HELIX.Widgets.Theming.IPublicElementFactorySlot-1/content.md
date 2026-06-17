# IPublicElementFactorySlot<T> (/reference/HELIX.Widgets.Theming.IPublicElementFactorySlot-1)

# IPublicElementFactorySlot<T>

```
public interface IPublicElementFactorySlot<T> where T : VisualElement
```

## Reference

```
ElementFactoryReference<T> Reference { get; set; }
```

## Element

```
T Element { get; }
```

## HasElement

```
bool HasElement { get; }
```

## SetMapped(ElementFactory<T>)

```
void SetMapped(ElementFactory<T> value)
```

## GetMapped<TMapped>()

```
TMapped GetMapped<TMapped>() where TMapped : ElementFactory<T>
```