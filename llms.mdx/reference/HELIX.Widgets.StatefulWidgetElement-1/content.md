# StatefulWidgetElement<T> (/reference/HELIX.Widgets.StatefulWidgetElement-1)

# StatefulWidgetElement<T>

```
public class StatefulWidgetElement<T> : BuildingWidgetBaseElement<T>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IReconcileScheduler, IScheduledReconcileRunner, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IThemeProvider, ISingleChildContainer, IHierarchyDisposable, IDisposable, IElement, IStatefulWidget where T : StatefulWidget<T>
```

## isDisposed

```
public bool isDisposed
```

## State

```
public State<T> State { get; set; }
```

## Dispose()

```
public void Dispose()
```

## CanReconcile(Widget)

```
public override bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public override bool Reconcile(Widget updated)
```

## UserScheduleRebuild()

```
public void UserScheduleRebuild()
```

## OnWatchedThemeUpdated(ThemeProperty, object)

```
protected override void OnWatchedThemeUpdated(ThemeProperty property, object value)
```

## GetThemed<S>(BaseThemeProperty<S>, bool)

```
public override S GetThemed<S>(BaseThemeProperty<S> property, bool listen = true)
```

Resolves the theme value for the given property.

## TryGetThemed<S>(BaseThemeProperty<S>, out S, bool)

```
public override bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true)
```

Tries to resolve the theme value for the given property.

## GetBuildableForWidget(T, T)

```
protected override IBuildable GetBuildableForWidget(T previous, T widget)
```

## BeforeBuild(T, T)

```
public override void BeforeBuild(T previous, T widget)
```

## Build(IBuildable, T, T)

```
public override Widget Build(IBuildable buildable, T previous, T widget)
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## ToStringShort()

```
public override string ToStringShort()
```