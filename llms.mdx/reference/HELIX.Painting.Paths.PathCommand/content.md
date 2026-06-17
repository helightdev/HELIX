# PathCommand (/reference/HELIX.Painting.Paths.PathCommand)

# PathCommand

```
public struct PathCommand
```

## type

```
public PathCommandType type
```

## p0

```
public Vector2 p0
```

## p1

```
public Vector2 p1
```

## p2

```
public Vector2 p2
```

## f0

```
public float f0
```

## f1

```
public float f1
```

## f2

```
public float f2
```

## flag

```
public bool flag
```

## MoveTo(Vector2)

```
public static PathCommand MoveTo(Vector2 pos)
```

## LineTo(Vector2)

```
public static PathCommand LineTo(Vector2 pos)
```

## ArcTo(Vector2, Vector2, float)

```
public static PathCommand ArcTo(Vector2 control, Vector2 end, float radius)
```

## Arc(Vector2, float, Angle, Angle, ArcDirection)

```
public static PathCommand Arc(Vector2 center, float radius, Angle startRad, Angle endRad, ArcDirection dir)
```

## BezierCurveTo(Vector2, Vector2, Vector2)

```
public static PathCommand BezierCurveTo(Vector2 c1, Vector2 c2, Vector2 end)
```

## QuadraticCurveTo(Vector2, Vector2)

```
public static PathCommand QuadraticCurveTo(Vector2 control, Vector2 end)
```

## ClosePath()

```
public static PathCommand ClosePath()
```

## Apply(Painter2D)

```
public void Apply(Painter2D painter)
```