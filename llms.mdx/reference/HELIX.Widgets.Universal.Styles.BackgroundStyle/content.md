# BackgroundStyle (/reference/HELIX.Widgets.Universal.Styles.BackgroundStyle)

# BackgroundStyle

```
public class BackgroundStyle : DiagnosticableBase, IDiagnosticable, IEquatable<BackgroundStyle>
```

## Default

```
public static readonly BackgroundStyle Default
```

## color

```
public StyleColor color
```

## fit

```
public StyleBackgroundSize fit
```

## image

```
public StyleBackground image
```

## imageTintColor

```
public StyleColor imageTintColor
```

## repeat

```
public StyleBackgroundRepeat repeat
```

## slice

```
public StyleInt4 slice
```

## sliceScale

```
public StyleFloat sliceScale
```

## sliceType

```
public StyleEnum<SliceType> sliceType
```

## x

```
public StyleBackgroundPosition x
```

## y

```
public StyleBackgroundPosition y
```

## Equals(BackgroundStyle)

```
public bool Equals(BackgroundStyle other)
```

## BackgroundStyle(Color)

```
public static implicit operator BackgroundStyle(Color color)
```

## BackgroundStyle(StyleColor)

```
public static implicit operator BackgroundStyle(StyleColor color)
```

## BackgroundStyle(MaterialColor)

```
public static implicit operator BackgroundStyle(MaterialColor color)
```

## Apply(VisualElement)

```
public void Apply(VisualElement element)
```

## Merge(BackgroundStyle)

```
public void Merge(BackgroundStyle overrides)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
public override int GetHashCode()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```