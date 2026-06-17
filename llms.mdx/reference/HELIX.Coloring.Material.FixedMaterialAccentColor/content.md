# FixedMaterialAccentColor (/reference/HELIX.Coloring.Material.FixedMaterialAccentColor)

# FixedMaterialAccentColor

```
public sealed class FixedMaterialAccentColor : MaterialAccentColor
```

Exact pre-created accent swatch.
Use this when you need to match Flutter accent tables exactly.

## FixedMaterialAccentColor(int, int, int, int, int, string)

```
public FixedMaterialAccentColor(int primaryArgb, int shade100, int shade200, int shade400, int shade700, string name = null)
```

## FixedMaterialAccentColor(uint, uint, uint, uint, uint, string)

```
public FixedMaterialAccentColor(uint primaryArgb, uint shade100, uint shade200, uint shade400, uint shade700, string name = null)
```

## this[int]

```
public override Color this[int weight] { get; }
```

Resolves a shade by weight.

## GetArgb(int)

```
public override int GetArgb(int weight)
```

Resolves a shade ARGB by weight.