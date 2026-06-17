# TimePollingLineGraph (/reference/HELIX.Widgets.Visual.TimePollingLineGraph)

# TimePollingLineGraph

```
public class TimePollingLineGraph : LineGraph, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IElement
```

## discardOnOffscreen

```
[UxmlAttribute]
public bool discardOnOffscreen
```

## pollingInterval

```
public float pollingInterval
```

## timeframe

```
public float timeframe
```

## TimePollingLineGraph(Func<float>, float, float, ILineGraphDataSource)

```
public TimePollingLineGraph(Func<float> sampleFunction, float pollingInterval, float timeframe, ILineGraphDataSource source = null)
```