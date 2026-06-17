# Border (/reference/HELIX.Types.Border)

# Border

```
public struct Border : IEquatable<Border>
```

## left

```
public BorderSide left
```

## top

```
public BorderSide top
```

## right

```
public BorderSide right
```

## bottom

```
public BorderSide bottom
```

## Border(BorderSide, BorderSide, BorderSide, BorderSide)

```
public Border(BorderSide left, BorderSide top, BorderSide right, BorderSide bottom)
```

## Apply(VisualElement)

```
public readonly void Apply(VisualElement element)
```

## Equals(Border)

```
public bool Equals(Border other)
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

## All(float, Color)

```
public static Border All(float width, Color color)
```

## All(BorderSide)

```
public static Border All(BorderSide side)
```

## Symmetric(BorderSide, BorderSide)

```
public static Border Symmetric(BorderSide horizontal, BorderSide vertical)
```

## Only(BorderSide?, BorderSide?, BorderSide?, BorderSide?)

```
public static Border Only(BorderSide? left = null, BorderSide? top = null, BorderSide? right = null, BorderSide? bottom = null)
```

## None

```
public static readonly Border None
```