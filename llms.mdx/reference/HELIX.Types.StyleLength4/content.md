# StyleLength4 (/reference/HELIX.Types.StyleLength4)

# StyleLength4

```
[Serializable]
public struct StyleLength4 : IEquatable<StyleLength4>, IStyleLength4
```

## l

```
public StyleLength l
```

## t

```
public StyleLength t
```

## r

```
public StyleLength r
```

## b

```
public StyleLength b
```

## StyleLength4(StyleLength, StyleLength, StyleLength, StyleLength)

```
public StyleLength4(StyleLength l, StyleLength t, StyleLength r, StyleLength b)
```

## StyleLength4(StyleLength2, StyleLength2)

```
public StyleLength4(StyleLength2 xy, StyleLength2 zw)
```

## StyleLength4(StyleLength)

```
public StyleLength4(StyleLength v)
```

## StyleLength2(StyleLength4)

```
public static implicit operator StyleLength2(StyleLength4 sl4)
```

## StyleLength4(Vector4)

```
public static implicit operator StyleLength4(Vector4 v)
```

## StyleLength4(StyleLength2)

```
public static implicit operator StyleLength4(StyleLength2 xy)
```

## StyleLength4(StyleLength)

```
public static implicit operator StyleLength4(StyleLength v)
```

## StyleLength4(float)

```
public static implicit operator StyleLength4(float v)
```

## Equals(StyleLength4)

```
public bool Equals(StyleLength4 other)
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

## Only(StyleLength?, StyleLength?, StyleLength?, StyleLength?)

```
public static StyleLength4 Only(StyleLength? left = null, StyleLength? right = null, StyleLength? top = null, StyleLength? bottom = null)
```

## Symmetric(StyleLength?, StyleLength?)

```
public static StyleLength4 Symmetric(StyleLength? horizontal = null, StyleLength? vertical = null)
```

## All(StyleLength)

```
public static StyleLength4 All(StyleLength value)
```

## Zero

```
public static readonly StyleLength4 Zero
```

## Initial

```
public static readonly StyleLength4 Initial
```

## Auto

```
public static readonly StyleLength4 Auto
```