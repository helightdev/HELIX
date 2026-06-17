# MaterialAccentColor (/reference/HELIX.Coloring.Material.MaterialAccentColor)

# MaterialAccentColor

```
public abstract class MaterialAccentColor : MaterialSwatch
```

Base class for accent swatches with weights:
100, 200, 400, 700.

## MaterialAccentColor(int, string)

```
protected MaterialAccentColor(int primaryArgb, string name = null)
```

## MaterialAccentColor(uint, string)

```
protected MaterialAccentColor(uint primaryArgb, string name = null)
```

## Weights

```
public override ReadOnlySpan<int> Weights { get; }
```

Ordered list of valid shade weights for this swatch.

## Shade100

```
public Color Shade100 { get; }
```

## Shade200

```
public Color Shade200 { get; }
```

## Shade400

```
public Color Shade400 { get; }
```

## Shade700

```
public Color Shade700 { get; }
```