# KeyColor (/reference/HELIX.Coloring.Material.KeyColor)

# KeyColor

```
public sealed class KeyColor
```

Key color is a color that represents the hue and chroma of a tonal palette.

## KeyColor(double, double)

```
public KeyColor(double hue, double requestedChroma)
```

## Hue

```
public double Hue { get; }
```

## RequestedChroma

```
public double RequestedChroma { get; }
```

## Create()

```
public Hct Create()
```

Creates a key color from a hue and a requestedChroma.