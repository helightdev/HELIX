# OkLabColor (/reference/HELIX.Coloring.OkLabColor)

# OkLabColor

```
[Serializable]
public struct OkLabColor : IEquatable<OkLabColor>
```

## l

```
public float l
```

## a

```
public float a
```

## b

```
public float b
```

## OkLabColor(float, float, float)

```
public OkLabColor(float l, float a, float b)
```

## OkLabColor(Color)

```
public OkLabColor(Color linear)
```

## ToGamma()

```
public Color ToGamma()
```

## Equals(OkLabColor)

```
public bool Equals(OkLabColor other)
```

## Equals(object)

```
public override bool Equals(object o)
```

## GetHashCode()

```
public override int GetHashCode()
```

## Lerp(OkLabColor, OkLabColor, float)

```
public static OkLabColor Lerp(OkLabColor a, OkLabColor b, float t)
```

## Color(OkLabColor)

```
public static explicit operator Color(OkLabColor color)
```

## OkLchColor(OkLabColor)

```
public static explicit operator OkLchColor(OkLabColor color)
```

## float3(OkLabColor)

```
public static implicit operator float3(OkLabColor color)
```

## OkLabColor(float3)

```
public static implicit operator OkLabColor(float3 components)
```