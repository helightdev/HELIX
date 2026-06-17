# GradientStrokePathDrawer (/reference/HELIX.Widgets.Visual.PathDrawers.GradientStrokePathDrawer)

# GradientStrokePathDrawer

```
[UxmlObject]
public class GradientStrokePathDrawer : ScriptablePathDrawer
```

## StartColor

```
[Header("Gradient Stroke")]
[UxmlAttribute]
public Color StartColor { get; set; }
```

## EndColor

```
[UxmlAttribute]
public Color EndColor { get; set; }
```

## Width

```
[UxmlAttribute]
public float Width { get; set; }
```

## MiterLimit

```
[UxmlAttribute]
public float MiterLimit { get; set; }
```

## LineJoin

```
[UxmlAttribute]
public LineJoin LineJoin { get; set; }
```

## LineCap

```
[UxmlAttribute]
public LineCap LineCap { get; set; }
```

## DashLength

```
[UxmlAttribute]
public float DashLength { get; set; }
```

## DashGap

```
[UxmlAttribute]
public float DashGap { get; set; }
```

## DashOffset

```
[UxmlAttribute]
public float DashOffset { get; set; }
```

## Draw(PaintCanvas)

```
public override void Draw(PaintCanvas canvas)
```