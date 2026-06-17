# RadialGradientGenerator (/reference/HELIX.Widgets.Visual.GradientGenerators.RadialGradientGenerator)

# RadialGradientGenerator

```
[UxmlObject]
public class RadialGradientGenerator : FillGradientGenerator
```

## InnerColor

```
[UxmlAttribute]
public Color InnerColor { get; set; }
```

## OuterColor

```
[UxmlAttribute]
public Color OuterColor { get; set; }
```

## Center

```
[UxmlAttribute]
public Vector2 Center { get; set; }
```

## Focus

```
[UxmlAttribute]
public Vector2 Focus { get; set; }
```

## Radius

```
[UxmlAttribute]
public float Radius { get; set; }
```

## Normalized

```
[UxmlAttribute]
public bool Normalized { get; set; }
```

## AddressMode

```
[UxmlAttribute]
public AddressMode AddressMode { get; set; }
```

## Generate(PaintCanvas)

```
public override FillGradient Generate(PaintCanvas canvas)
```