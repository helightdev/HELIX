# Alignment (/reference/HELIX.Types.Alignment)

# Alignment

```
[Serializable]
public struct Alignment
```

## x

```
[Range(-1, 1)]
public float x
```

## y

```
[Range(-1, 1)]
public float y
```

## Alignment(float, float)

```
public Alignment(float x, float y)
```

## GetOffsetCoefficients()

```
public Vector2 GetOffsetCoefficients()
```

## TopCenter

```
public static readonly Alignment TopCenter
```

## TopRight

```
public static readonly Alignment TopRight
```

## TopLeft

```
public static readonly Alignment TopLeft
```

## CenterLeft

```
public static readonly Alignment CenterLeft
```

## Center

```
public static readonly Alignment Center
```

## CenterRight

```
public static readonly Alignment CenterRight
```

## BottomLeft

```
public static readonly Alignment BottomLeft
```

## BottomCenter

```
public static readonly Alignment BottomCenter
```

## BottomRight

```
public static readonly Alignment BottomRight
```

## Alignment(Vector2)

```
public static implicit operator Alignment(Vector2 vec)
```

## Vector2(Alignment)

```
public static implicit operator Vector2(Alignment alignment)
```

## Alignment(TextAnchor)

```
public static implicit operator Alignment(TextAnchor anchor)
```

## Quantize()

```
public TextAnchor Quantize()
```

## IsLosslessQuantizable()

```
public bool IsLosslessQuantizable()
```

## AlignAsColumn(VisualElement)

```
public readonly void AlignAsColumn(VisualElement element)
```

## ToString()

```
public override string ToString()
```