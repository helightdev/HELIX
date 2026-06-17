# PathBuilder (/reference/HELIX.Painting.Paths.PathBuilder)

# PathBuilder

```
public class PathBuilder : IPathBuilder
```

## MoveTo(Vector2)

```
public void MoveTo(Vector2 pos)
```

## LineTo(Vector2)

```
public void LineTo(Vector2 pos)
```

## ArcTo(Vector2, Vector2, float)

```
public void ArcTo(Vector2 control, Vector2 end, float radius)
```

## Arc(Vector2, float, Angle, Angle, ArcDirection)

```
public void Arc(Vector2 center, float radius, Angle startRad, Angle endRad, ArcDirection dir)
```

## BezierCurveTo(Vector2, Vector2, Vector2)

```
public void BezierCurveTo(Vector2 c1, Vector2 c2, Vector2 end)
```

## QuadraticCurveTo(Vector2, Vector2)

```
public void QuadraticCurveTo(Vector2 control, Vector2 end)
```

## ClosePath()

```
public void ClosePath()
```

## Build()

```
public Path Build()
```