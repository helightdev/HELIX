# StatelessWidgetElement<T> (/reference/HELIX.Widgets.StatelessWidgetElement-1)

# StatelessWidgetElement<T>

```
public class StatelessWidgetElement<T> : BuildingWidgetBaseElement<T>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IReconcileScheduler, IScheduledReconcileRunner, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IThemeProvider, ISingleChildContainer, IStatelessWidget, IHierarchyDisposable, IDisposable, IElement where T : StatelessWidget<T>
```

## Dispose()

```
public void Dispose()
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

## Reconcile(Widget)

```
public override bool Reconcile(Widget updated)
```

## GetBuildableForWidget(T, T)

```
protected override IBuildable GetBuildableForWidget(T previous, T widget)
```

## ToStringShort()

```
public override string ToStringShort()
```