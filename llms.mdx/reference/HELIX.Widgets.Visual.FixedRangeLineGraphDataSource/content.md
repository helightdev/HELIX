# FixedRangeLineGraphDataSource (/reference/HELIX.Widgets.Visual.FixedRangeLineGraphDataSource)

# FixedRangeLineGraphDataSource

```
public class FixedRangeLineGraphDataSource : ILineGraphDataSource
```

## FixedRangeLineGraphDataSource(Vector2, Vector2)

```
public FixedRangeLineGraphDataSource(Vector2 valueRange, Vector2 timeRange)
```

## FixedRangeLineGraphDataSource(IReadOnlyCollection<Vector2>, Vector2, Vector2, Vector2)

```
public FixedRangeLineGraphDataSource(IReadOnlyCollection<Vector2> initialPoints, Vector2 valueRange, Vector2 timeRange, Vector2 offset = default)
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