# BuildingWidgetBaseElement<T> (/reference/HELIX.Widgets.Elements.BuildingWidgetBaseElement-1)

# BuildingWidgetBaseElement<T>

```
public abstract class BuildingWidgetBaseElement<T> : SuperSingleChildWidgetBaseElement<T>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, ISingleChildContainer where T : Widget
```

## LastBuildResult

```
public Widget LastBuildResult { get; protected set; }
```

## IsBuilding

```
public bool IsBuilding { get; protected set; }
```

## BeforeBuild(T, T)

```
public virtual void BeforeBuild(T previous, T widget)
```

## AfterBuild(T, T)

```
public virtual void AfterBuild(T previous, T widget)
```

## Build(IBuildable, T, T)

```
public virtual Widget Build(IBuildable buildable, T previous, T widget)
```

## GetChildFromWidget(T, T)

```
protected override Widget GetChildFromWidget(T previous, T widget)
```

## GetBuildableForWidget(T, T)

```
protected abstract IBuildable GetBuildableForWidget(T previous, T widget)
```