# SuperSingleChildWidgetBaseElement<T> (/reference/HELIX.Widgets.Elements.SuperSingleChildWidgetBaseElement-1)

# SuperSingleChildWidgetBaseElement<T>

```
public abstract class SuperSingleChildWidgetBaseElement<T> : WidgetBaseElement<T>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, ISingleChildContainer where T : Widget
```

## Child

```
public virtual VisualElement Child { get; set; }
```

## CanReconcile(Widget)

```
public override bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public override bool Reconcile(Widget updated)
```

## GetChildFromWidget(T, T)

```
protected abstract Widget GetChildFromWidget(T previous, T widget)
```

## DebugDescribeChildren()

```
public override List<DiagnosticsNode> DebugDescribeChildren()
```