# WidgetBaseElement<T> (/reference/HELIX.Widgets.Elements.WidgetBaseElement-1)

# WidgetBaseElement<T>

```
public abstract class WidgetBaseElement<T> : WidgetBaseElement, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider where T : Widget
```

## TypedDescriptor

```
public T TypedDescriptor { get; set; }
```

## CanReconcile(Widget)

```
public override bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public override bool Reconcile(Widget updated)
```

## Apply(T, T)

```
public virtual void Apply(T previous, T widget)
```