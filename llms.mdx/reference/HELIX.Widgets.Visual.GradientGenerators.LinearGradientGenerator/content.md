# LinearGradientGenerator (/reference/HELIX.Widgets.Visual.GradientGenerators.LinearGradientGenerator)

# LinearGradientGenerator

```
[UxmlObject]
public class LinearGradientGenerator : FillGradientGenerator
```

## StartColor

```
[UxmlAttribute]
public Color StartColor { get; set; }
```

## EndColor

```
[UxmlAttribute]
public Color EndColor { get; set; }
```

## StartPoint

```
[UxmlAttribute]
public Vector2 StartPoint { get; set; }
```

## EndPoint

```
[UxmlAttribute]
public Vector2 EndPoint { get; set; }
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