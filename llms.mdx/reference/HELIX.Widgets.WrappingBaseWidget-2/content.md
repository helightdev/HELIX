# WrappingBaseWidget<S, T> (/reference/HELIX.Widgets.WrappingBaseWidget-2)

# WrappingBaseWidget<S, T>

```
public abstract class WrappingBaseWidget<S, T> : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IUserDataWidget<S, T> where S : WrappingBaseWidget<S, T> where T : VisualElement
```

## Apply(S, T)

```
public abstract void Apply(S previous, T element)
```

## Create()

```
public abstract T Create()
```

## WrappingBaseWidget(Key, object[], IReadOnlyCollection<Modifier>)

```
protected WrappingBaseWidget(Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.