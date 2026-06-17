# DislikeAnalyzer (/reference/HELIX.Coloring.Material.DislikeAnalyzer)

# DislikeAnalyzer

```
public static class DislikeAnalyzer
```

Check and/or fix universally disliked colors.

## IsDisliked(Hct)

```
public static bool IsDisliked(Hct hct)
```

Returns true if hct is disliked.

## FixIfDisliked(Hct)

```
public static Hct FixIfDisliked(Hct hct)
```

If hct is disliked, lighten it to make it likable.