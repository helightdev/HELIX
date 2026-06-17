# Blend (/reference/HELIX.Coloring.Material.Blend)

# Blend

```
public static class Blend
```

Functions for blending in HCT and CAM16.

## Harmonize(int, int)

```
public static int Harmonize(int designColor, int sourceColor)
```

Blend the design color's HCT hue towards the key color's HCT hue.

## HctHue(int, int, double)

```
public static int HctHue(int from, int to, double amount)
```

Blends hue from one color into another. Chroma and tone of the original are maintained.

## Cam16Ucs(int, int, double)

```
public static int Cam16Ucs(int from, int to, double amount)
```

Blend in CAM16-UCS space.