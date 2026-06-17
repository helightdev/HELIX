# FixedMaterialColor (/reference/HELIX.Coloring.Material.FixedMaterialColor)

# FixedMaterialColor

```
public sealed class FixedMaterialColor : MaterialColor
```

Exact pre-created standard swatch.
Use this when you need to match Flutter's hard-coded tables exactly.

## FixedMaterialColor(int, int, int, int, int, int, int, int, int, int, int, string)

```
public FixedMaterialColor(int primaryArgb, int shade50, int shade100, int shade200, int shade300, int shade400, int shade500, int shade600, int shade700, int shade800, int shade900, string name = null)
```

## FixedMaterialColor(uint, uint, uint, uint, uint, uint, uint, uint, uint, uint, uint, string)

```
public FixedMaterialColor(uint primaryArgb, uint shade50, uint shade100, uint shade200, uint shade300, uint shade400, uint shade500, uint shade600, uint shade700, uint shade800, uint shade900, string name = null)
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