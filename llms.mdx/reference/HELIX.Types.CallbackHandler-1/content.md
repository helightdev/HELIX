# CallbackHandler<T> (/reference/HELIX.Types.CallbackHandler-1)

# CallbackHandler<T>

```
public readonly struct CallbackHandler<T> : IEventHandler where T : EventBase<T>, new()
```

## CallbackHandler(EventCallback<T>)

```
public CallbackHandler(EventCallback<T> callback)
```

## CallbackHandler<T>(EventCallback<T>)

```
public static implicit operator CallbackHandler<T>(EventCallback<T> callback)
```

## Register(VisualElement)

```
public void Register(VisualElement element)
```

## Unregister(VisualElement)

```
public void Unregister(VisualElement element)
```