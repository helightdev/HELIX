# TextStyle (/reference/HELIX.Widgets.Universal.Styles.TextStyle)

# TextStyle

```
public class TextStyle : DiagnosticableBase, IDiagnosticable, IEquatable<TextStyle>
```

## Default

```
public static readonly TextStyle Default
```

## AlignCenter

```
public static readonly TextStyle AlignCenter
```

## AlignLeft

```
public static readonly TextStyle AlignLeft
```

## AlignRight

```
public static readonly TextStyle AlignRight
```

## align

```
public StyleEnum<TextAnchor> align
```

## autoSize

```
public StyleTextAutoSize autoSize
```

## color

```
public StyleColor color
```

## font

```
public StyleFontDefinition font
```

## fontSize

```
public StyleLength fontSize
```

## generator

```
public StyleEnum<TextGeneratorType> generator
```

## letterSpacing

```
public StyleLength letterSpacing
```

## outlineColor

```
public StyleColor outlineColor
```

## outlineWidth

```
public StyleFloat outlineWidth
```

## overflow

```
public StyleEnum<TextOverflow> overflow
```

## overflowPosition

```
public StyleEnum<TextOverflowPosition> overflowPosition
```

## paragraphSpacing

```
public StyleLength paragraphSpacing
```

## shadow

```
public StyleTextShadow shadow
```

## style

```
public StyleEnum<FontStyle> style
```

## wordSpacing

```
public StyleLength wordSpacing
```

## wrap

```
public StyleEnum<WhiteSpace> wrap
```

## Equals(TextStyle)

```
public bool Equals(TextStyle other)
```

## Apply(VisualElement)

```
public void Apply(VisualElement element)
```

## Merge(TextStyle)

```
public void Merge(TextStyle overrides)
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