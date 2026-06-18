# MultiChildWidgetBaseElement<T> (/reference/HELIX.Widgets.Elements.MultiChildWidgetBaseElement-1)

# MultiChildWidgetBaseElement<T>

```
public abstract class MultiChildWidgetBaseElement<T> : WidgetBaseElement<T>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IReconcileScheduler, IScheduledReconcileRunner, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, IMultiChildContainer, IWidgetElementCollection where T : MultiChildWidget
```

## Childs

```
public virtual IEnumerable<VisualElement> Childs { get; set; }
```

## LoadWidgetElements(List<IWidgetElement>)

```
public virtual void LoadWidgetElements(List<IWidgetElement> elements)
```

## UpdateWidgetElements(Span<IWidgetElement>, Span<ReconcilerCollectionDelta>)

```
public virtual void UpdateWidgetElements(Span<IWidgetElement> result, Span<ReconcilerCollectionDelta> deltas)
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