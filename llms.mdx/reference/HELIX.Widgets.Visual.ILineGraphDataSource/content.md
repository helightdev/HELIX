# ILineGraphDataSource (/reference/HELIX.Widgets.Visual.ILineGraphDataSource)

# ILineGraphDataSource

```
public interface ILineGraphDataSource
```

## HasData

```
bool HasData { get; }
```

## GetNormalizedPoints()

```
Vector2[] GetNormalizedPoints()
```

## Update(IReadOnlyCollection<Vector2>, Vector2)

```
void Update(IReadOnlyCollection<Vector2> points, Vector2 offset = default)
```