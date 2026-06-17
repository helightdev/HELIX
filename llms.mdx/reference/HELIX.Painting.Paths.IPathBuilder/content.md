# IPathBuilder (/reference/HELIX.Painting.Paths.IPathBuilder)

# IPathBuilder

```
public interface IPathBuilder
```

## MoveTo(Vector2)

```
void MoveTo(Vector2 pos)
```

## LineTo(Vector2)

```
void LineTo(Vector2 pos)
```

## ArcTo(Vector2, Vector2, float)

```
void ArcTo(Vector2 control, Vector2 end, float radius)
```

## Arc(Vector2, float, Angle, Angle, ArcDirection)

```
void Arc(Vector2 center, float radius, Angle startRad, Angle endRad, ArcDirection dir)
```

## BezierCurveTo(Vector2, Vector2, Vector2)

```
void BezierCurveTo(Vector2 c1, Vector2 c2, Vector2 end)
```

## QuadraticCurveTo(Vector2, Vector2)

```
void QuadraticCurveTo(Vector2 control, Vector2 end)
```

## ClosePath()

```
void ClosePath()
```

## Record(Action<IPathBuilder>)

```
public static Path Record(Action<IPathBuilder> func)
```

## Draw(Painter2D, Action<IPathBuilder>)

```
public static void Draw(Painter2D painter, Action<IPathBuilder> func)
```