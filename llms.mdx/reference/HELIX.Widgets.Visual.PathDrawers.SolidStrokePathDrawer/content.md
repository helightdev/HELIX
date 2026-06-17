# SolidStrokePathDrawer (/reference/HELIX.Widgets.Visual.PathDrawers.SolidStrokePathDrawer)

# SolidStrokePathDrawer

```
[UxmlObject]
public class SolidStrokePathDrawer : ScriptablePathDrawer
```

## Color

```
[Header("Solid Stroke")]
[UxmlAttribute]
public Color Color { get; set; }
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