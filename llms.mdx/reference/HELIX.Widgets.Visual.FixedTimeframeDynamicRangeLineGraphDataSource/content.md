# FixedTimeframeDynamicRangeLineGraphDataSource (/reference/HELIX.Widgets.Visual.FixedTimeframeDynamicRangeLineGraphDataSource)

# FixedTimeframeDynamicRangeLineGraphDataSource

```
public class FixedTimeframeDynamicRangeLineGraphDataSource : ILineGraphDataSource
```

## FixedTimeframeDynamicRangeLineGraphDataSource(float)

```
public FixedTimeframeDynamicRangeLineGraphDataSource(float timeframe)
```

## FixedTimeframeDynamicRangeLineGraphDataSource(IReadOnlyCollection<Vector2>, float, Vector2)

```
public FixedTimeframeDynamicRangeLineGraphDataSource(IReadOnlyCollection<Vector2> initialPoints, float timeframe, Vector2 offset = default)
```

## HasData

```
public bool HasData { get; }
```

## GetNormalizedPoints()

```
public Vector2[] GetNormalizedPoints()
```

## Update(IReadOnlyCollection<Vector2>, Vector2)

```
public void Update(IReadOnlyCollection<Vector2> points, Vector2 offset = default)
```