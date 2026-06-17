# LayoutPathBuilder (/reference/HELIX.Widgets.Visual.PathBuilders.LayoutPathBuilder)

# LayoutPathBuilder

```
[UxmlObject]
public class LayoutPathBuilder : ScriptablePathBuilder
```

## Height

```
[UxmlAttribute]
public StyleLength Height { get; set; }
```

## Width

```
[UxmlAttribute]
public StyleLength Width { get; set; }
```

## WidthConstraints

```
[UxmlAttribute]
public Vector2 WidthConstraints { get; set; }
```

## HeightConstraints

```
[UxmlAttribute]
public Vector2 HeightConstraints { get; set; }
```

## Alignment

```
[UxmlAttribute]
public Alignment Alignment { get; set; }
```

## Builder

```
[UxmlObjectReference]
public ScriptablePathBuilder Builder { get; set; }
```

## Build(IPathBuilder, Rect)

```
public override void Build(IPathBuilder builder, Rect bounds)
```