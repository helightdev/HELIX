# BoxConstraints (/reference/HELIX.Types.BoxConstraints)

# BoxConstraints

```
public readonly struct BoxConstraints : IEquatable<BoxConstraints>
```

## preferred

```
public readonly StyleLength2 preferred
```

## min

```
public readonly StyleLength2 min
```

## max

```
public readonly StyleLength2 max
```

## BoxConstraints(StyleLength2, StyleLength2, StyleLength2)

```
public BoxConstraints(StyleLength2 preferred, StyleLength2 min, StyleLength2 max)
```

## Apply(VisualElement)

```
public void Apply(VisualElement element)
```

## Equals(BoxConstraints)

```
public bool Equals(BoxConstraints other)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```

## ToString()

```
public override string ToString()
```

## Only(StyleLength2?, StyleLength2?, StyleLength2?)

```
public static BoxConstraints Only(StyleLength2? preferred = null, StyleLength2? min = null, StyleLength2? max = null)
```

## Preferred(StyleLength2)

```
public static BoxConstraints Preferred(StyleLength2 preferred)
```

## Preferred(StyleLength, StyleLength)

```
public static BoxConstraints Preferred(StyleLength width, StyleLength height)
```

## Tight(StyleLength2)

```
public static BoxConstraints Tight(StyleLength2 size)
```

## Tight(StyleLength, StyleLength)

```
public static BoxConstraints Tight(StyleLength width, StyleLength height)
```

## Loose(StyleLength2)

```
public static BoxConstraints Loose(StyleLength2 max)
```

## Min(StyleLength2)

```
public static BoxConstraints Min(StyleLength2 min)
```

## Initial

```
public static readonly BoxConstraints Initial
```