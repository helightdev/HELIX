# OkLchColor (/reference/HELIX.Coloring.OkLchColor)

# OkLchColor

```
public struct OkLchColor : IEquatable<OkLchColor>
```

## l

```
public float l
```

## c

```
public float c
```

## h

```
public float h
```

## OkLchColor(float, float, float)

```
public OkLchColor(float l, float c, float h)
```

## OkLchColor(Color)

```
public OkLchColor(Color linear)
```

## ToGamma()

```
public Color ToGamma()
```

## ToString()

```
public override string ToString()
```

## Equals(OkLchColor)

```
public bool Equals(OkLchColor other)
```

## Equals(object)

```
public override bool Equals(object o)
```

## GetHashCode()

```
public override int GetHashCode()
```

## OkLabColor(OkLchColor)

```
public static explicit operator OkLabColor(OkLchColor color)
```

## Color(OkLchColor)

```
public static explicit operator Color(OkLchColor color)
```

## float3(OkLchColor)

```
public static implicit operator float3(OkLchColor color)
```

## OkLchColor(float3)

```
public static implicit operator OkLchColor(float3 components)
```