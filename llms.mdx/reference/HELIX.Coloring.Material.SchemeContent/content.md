# SchemeContent (/reference/HELIX.Coloring.Material.SchemeContent)

# SchemeContent

```
public sealed class SchemeContent : DynamicScheme
```

A scheme that places the source color in PrimaryContainer.
Primary Container is the source color, adjusted for color relativity.
Tertiary Container is an analogous color, specifically the analog found
by increasing hue on a 6-division wheel.

## SchemeContent(Hct, bool, double)

```
public SchemeContent(Hct sourceColorHct, bool isDark, double contrastLevel = 0)
```