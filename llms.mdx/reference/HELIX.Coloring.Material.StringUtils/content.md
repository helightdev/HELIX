# StringUtils (/reference/HELIX.Coloring.Material.StringUtils)

# StringUtils

```
public static class StringUtils
```

## HexFromArgb(int, bool)

```
public static string HexFromArgb(int argb, bool leadingHashSign = true)
```

## ArgbFromHex(string)

```
public static int? ArgbFromHex(string hex)
```

Preserves the original Dart behavior as closely as possible:
strips '#' and parses the remaining hex directly.
Note that this does NOT force alpha to 0xFF for 6-digit RGB input.
For example, "#FF0000" becomes 0x00FF0000 numerically, matching the source behavior.