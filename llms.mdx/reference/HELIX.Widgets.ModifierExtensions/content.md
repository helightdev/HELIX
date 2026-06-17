# ModifierExtensions (/reference/HELIX.Widgets.ModifierExtensions)

# ModifierExtensions

```
public static class ModifierExtensions
```

## WithModifier<T>(T, Modifier)

```
public static T WithModifier<T>(this T element, Modifier modifier) where T : Widget
```

## Flexible<T>(T, StyleFloat?, StyleFloat?, Align)

```
public static T Flexible<T>(this T element, StyleFloat? grow = null, StyleFloat? shrink = null, Align selfCrossAxisAlign = Align.Auto) where T : Widget
```

## Fill<T>(T)

```
public static T Fill<T>(this T element) where T : Widget
```

## Shrink<T>(T)

```
public static T Shrink<T>(this T element) where T : Widget
```

## Tight<T>(T)

```
public static T Tight<T>(this T element) where T : Widget
```

## TightStretch<T>(T)

```
public static T TightStretch<T>(this T element) where T : Widget
```

## Expand<T>(T, float, Align)

```
public static T Expand<T>(this T element, float flex = 1, Align selfCrossAxisAlign = Align.Auto) where T : Widget
```

## Positioned<T>(T, StyleLength4?, Position)

```
public static T Positioned<T>(this T element, StyleLength4? offset = null, Position offsetType = Position.Absolute) where T : Widget
```

## Stretch<T>(T)

```
public static T Stretch<T>(this T element) where T : Widget
```

## Size<T>(T, StyleLength2?, StyleLength2?, StyleLength2?)

```
public static T Size<T>(this T element, StyleLength2? size = null, StyleLength2? minSize = null, StyleLength2? maxSize = null) where T : Widget
```

## Size<T>(T, StyleLength?, StyleLength?)

```
public static T Size<T>(this T element, StyleLength? width = null, StyleLength? height = null) where T : Widget
```

## Const<T>(T, params object[])

```
public static T Const<T>(this T element, params object[] values) where T : Widget
```

## Display<T>(T, bool)

```
public static T Display<T>(this T element, bool display) where T : Widget
```

## Visibility<T>(T, bool)

```
public static T Visibility<T>(this T element, bool visible) where T : Widget
```

## Opacity<T>(T, float)

```
public static T Opacity<T>(this T element, float opacity) where T : Widget
```

## Padding<T>(T, StyleLength4)

```
public static T Padding<T>(this T element, StyleLength4 padding) where T : Widget
```

## Margin<T>(T, StyleLength4)

```
public static T Margin<T>(this T element, StyleLength4 margin) where T : Widget
```

## Clip<T>(T)

```
public static T Clip<T>(this T element) where T : Widget
```

## Fallback<T>(T)

```
public static T Fallback<T>(this T modifier) where T : Modifier
```