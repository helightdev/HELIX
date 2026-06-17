# HelixFormattingHelper (/reference/HELIX.HelixFormattingHelper)

# HelixFormattingHelper

```
public static class HelixFormattingHelper
```

## BuildQuadruple<T>(string, T, T, T, T, string, string, string, string, bool, bool)

```
public static string BuildQuadruple<T>(string name, T l, T t, T r, T b, string nameLeft = "left", string nameTop = "top", string nameRight = "right", string nameBottom = "bottom", bool summarize = true, bool showNames = true) where T : IEquatable<T>
```

## FormatStyleValue<T>(IStyleValue<T>)

```
public static string FormatStyleValue<T>(this IStyleValue<T> value)
```

## FormatStyleValue(StyleLength)

```
public static string FormatStyleValue(this StyleLength length)
```

## FormatStyleValue<T>(StyleKeyword, T)

```
public static string FormatStyleValue<T>(StyleKeyword keyword, T value)
```

## FormatLength(Length)

```
public static string FormatLength(Length length)
```