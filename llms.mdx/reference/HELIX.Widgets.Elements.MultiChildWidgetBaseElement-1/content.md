# MultiChildWidgetBaseElement<T> (/reference/HELIX.Widgets.Elements.MultiChildWidgetBaseElement-1)

# MultiChildWidgetBaseElement<T>

```
public abstract class MultiChildWidgetBaseElement<T> : WidgetBaseElement<T>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, IMultiChildContainer, IWidgetElementCollection where T : MultiChildWidget
```

## Childs

```
public virtual IEnumerable<VisualElement> Childs { get; set; }
```

## LoadWidgetElements(List<IWidgetElement>)

```
public virtual void LoadWidgetElements(List<IWidgetElement> elements)
```

## UpdateWidgetElements(IWidgetElement[], ReconcilerCollectionDelta[])

```
public virtual void UpdateWidgetElements(IWidgetElement[] result, ReconcilerCollectionDelta[] deltas)
```

## CanReconcile(Widget)

```
public override bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public override bool Reconcile(Widget updated)
```

## DebugDescribeChildren()

```
public override List<DiagnosticsNode> DebugDescribeChildren()
```