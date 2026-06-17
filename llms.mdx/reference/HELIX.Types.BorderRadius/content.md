# BorderRadius (/reference/HELIX.Types.BorderRadius)

# BorderRadius

```
public struct BorderRadius : IEquatable<BorderRadius>, IStyleLength4
```

## topLeft

```
public StyleLength topLeft
```

## topRight

```
public StyleLength topRight
```

## bottomRight

```
public StyleLength bottomRight
```

## bottomLeft

```
public StyleLength bottomLeft
```

## BorderRadius(StyleLength, StyleLength, StyleLength, StyleLength)

```
public BorderRadius(StyleLength topLeft, StyleLength topRight, StyleLength bottomRight, StyleLength bottomLeft)
```

## Apply(VisualElement)

```
public readonly void Apply(VisualElement element)
```

## Equals(BorderRadius)

```
public bool Equals(BorderRadius other)
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

## ToStyleLength4()

```
public StyleLength4 ToStyleLength4()
```

## BorderRadius(StyleLength4)

```
public static implicit operator BorderRadius(StyleLength4 sl4)
```

## StyleLength4(BorderRadius)

```
public static implicit operator StyleLength4(BorderRadius br)
```

## BorderRadius(float)

```
public static implicit operator BorderRadius(float v)
```

## All(StyleLength)

```
public static BorderRadius All(StyleLength radius)
```

## Horizontal(StyleLength, StyleLength)

```
public static BorderRadius Horizontal(StyleLength left, StyleLength right)
```

## Vertical(StyleLength, StyleLength)

```
public static BorderRadius Vertical(StyleLength top, StyleLength bottom)
```

## Only(StyleLength?, StyleLength?, StyleLength?, StyleLength?)

```
public static BorderRadius Only(StyleLength? topLeft = null, StyleLength? topRight = null, StyleLength? bottomRight = null, StyleLength? bottomLeft = null)
```

## None

```
public static readonly BorderRadius None
```

## Initial

```
public static readonly BorderRadius Initial
```