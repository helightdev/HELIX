# RoundedRectPathBuilder (/reference/HELIX.Widgets.Visual.PathBuilders.RoundedRectPathBuilder)

# RoundedRectPathBuilder

```
[UxmlObject]
public class RoundedRectPathBuilder : ScriptablePathBuilder
```

## Rect

```
[Header("Rounded Rectangle")]
[UxmlAttribute]
public Rect Rect { get; set; }
```

## Mode

```
[UxmlAttribute]
public RectConstructionMode Mode { get; set; }
```

## Corners

```
[UxmlAttribute]
public Rect Corners { get; set; }
```

## Radii

```
public Vector4 Radii { get; set; }
```

## Build(IPathBuilder, Rect)

```
public override void Build(IPathBuilder builder, Rect bounds)
```