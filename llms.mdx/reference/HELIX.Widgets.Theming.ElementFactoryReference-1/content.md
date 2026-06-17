# ElementFactoryReference<T> (/reference/HELIX.Widgets.Theming.ElementFactoryReference-1)

# ElementFactoryReference<T>

```
[Serializable]
public struct ElementFactoryReference<T> : IWidgetFactoryReference, IMaybeThemeValue<T>, IMaybeThemeValue, IEquatable<ElementFactoryReference<T>> where T : VisualElement
```

## factoryName

```
public string factoryName
```

## resolved

```
[NonSerialized]
public ElementFactory<T> resolved
```

## ElementFactoryReference(string)

```
public ElementFactoryReference(string factoryName)
```

## ElementFactoryReference(ElementFactory<T>)

```
public ElementFactoryReference(ElementFactory<T> resolved)
```

## ElementFactoryReference<T>(string)

```
public static implicit operator ElementFactoryReference<T>(string factoryName)
```

## ElementFactoryReference<T>(Type)

```
public static implicit operator ElementFactoryReference<T>(Type factoryType)
```

## ElementFactoryReference<T>(ElementFactory<T>)

```
public static implicit operator ElementFactoryReference<T>(ElementFactory<T> factory)
```

## string(ElementFactoryReference<T>)

```
public static implicit operator string(ElementFactoryReference<T> reference)
```

## ElementFactory(ElementFactoryReference<T>)

```
public static implicit operator ElementFactory(ElementFactoryReference<T> reference)
```

## ElementFactory<T>(ElementFactoryReference<T>)

```
public static implicit operator ElementFactory<T>(ElementFactoryReference<T> reference)
```

## LookupFactory()

```
public ElementFactory LookupFactory()
```

## Equals(ElementFactoryReference<T>)

```
public bool Equals(ElementFactoryReference<T> other)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```

## TryGetThemeValue(out object)

```
public bool TryGetThemeValue(out object result)
```