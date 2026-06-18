# WidgetBaseElement (/reference/HELIX.Widgets.Elements.WidgetBaseElement)

# WidgetBaseElement

```
[SuppressMessage("ReSharper", "ParameterHidesMember")]
public abstract class WidgetBaseElement : BaseElement, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, IReconcileScheduler, IScheduledReconcileRunner
```

## Descriptor

```
public Widget Descriptor { get; set; }
```

## ParentContext

```
public BuildContext ParentContext { get; set; }
```

## CanReconcile(Widget)

```
public abstract bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public abstract bool Reconcile(Widget updated)
```

## OnAttached(AttachToPanelEvent)

```
protected override void OnAttached(AttachToPanelEvent evt)
```

## ScheduleReconcile(Widget, ReconcileMode)

```
public void ScheduleReconcile(Widget descriptor, ReconcileMode mode = ReconcileMode.AfterParent)
```

## TryRunScheduledReconcile(ReconcileMode)

```
public bool TryRunScheduledReconcile(ReconcileMode mode)
```

## DebugDescribeChildren()

```
public virtual List<DiagnosticsNode> DebugDescribeChildren()
```

## GetThemed<T>(BaseThemeProperty<T>, bool)

```
public virtual T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true)
```

Resolves the theme value for the given property.

## TryGetThemed<S>(BaseThemeProperty<S>, out S, bool)

```
public virtual bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true)
```

Tries to resolve the theme value for the given property.

## ToStringDeep(string, string, DiagnosticLevel, int)

```
public virtual string ToStringDeep(string prefixLineOne = "", string prefixOtherLines = null, DiagnosticLevel minLevel = DiagnosticLevel.Debug, int wrapWidth = 65)
```

## ToStringShallow(string, DiagnosticLevel)

```
public virtual string ToStringShallow(string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug)
```

## ToDiagnosticsNode(string, DiagnosticsTreeStyle?)

```
public virtual DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null)
```

## ToStringShort()

```
public virtual string ToStringShort()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public virtual void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## ToString()

```
public override string ToString()
```