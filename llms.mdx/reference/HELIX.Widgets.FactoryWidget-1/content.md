# FactoryWidget<T> (/reference/HELIX.Widgets.FactoryWidget-1)

# FactoryWidget<T>

```
public class FactoryWidget<T> : WrappingBaseWidget<FactoryWidget<T>, T>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IUserDataWidget<FactoryWidget<T>, T> where T : VisualElement
```

## creator

```
public Func<T> creator
```

## updater

```
public Action<T> updater
```

## Create()

```
public override T Create()
```

## Apply(FactoryWidget<T>, T)

```
public override void Apply(FactoryWidget<T> previous, T element)
```