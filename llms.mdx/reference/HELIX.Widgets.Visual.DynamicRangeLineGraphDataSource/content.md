# DynamicRangeLineGraphDataSource (/reference/HELIX.Widgets.Visual.DynamicRangeLineGraphDataSource)

# DynamicRangeLineGraphDataSource

```
public class DynamicRangeLineGraphDataSource : ILineGraphDataSource
```

## DynamicRangeLineGraphDataSource()

```
public DynamicRangeLineGraphDataSource()
```

## DynamicRangeLineGraphDataSource(IReadOnlyCollection<Vector2>, Vector2)

```
public DynamicRangeLineGraphDataSource(IReadOnlyCollection<Vector2> initialPoints, Vector2 offset = default)
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