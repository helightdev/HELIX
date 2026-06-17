# MaterialColor (/reference/HELIX.Coloring.Material.MaterialColor)

# MaterialColor

```
public abstract class MaterialColor : MaterialSwatch
```

Base class for standard Material-style swatches with weights:
50, 100, 200, 300, 400, 500, 600, 700, 800, 900.

## MaterialColor(int, string)

```
protected MaterialColor(int primaryArgb, string name = null)
```

## MaterialColor(uint, string)

```
protected MaterialColor(uint primaryArgb, string name = null)
```

## Weights

```
public override ReadOnlySpan<int> Weights { get; }
```

Ordered list of valid shade weights for this swatch.

## Shade50

```
public Color Shade50 { get; }
```

## Shade100

```
public Color Shade100 { get; }
```

## Shade200

```
public Color Shade200 { get; }
```

## Shade300

```
public Color Shade300 { get; }
```

## Shade400

```
public Color Shade400 { get; }
```

## Shade500

```
public Color Shade500 { get; }
```

## Shade600

```
public Color Shade600 { get; }
```

## Shade700

```
public Color Shade700 { get; }
```

## Shade800

```
public Color Shade800 { get; }
```

## Shade900

```
public Color Shade900 { get; }
```