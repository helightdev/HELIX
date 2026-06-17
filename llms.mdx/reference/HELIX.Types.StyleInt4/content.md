# StyleInt4 (/reference/HELIX.Types.StyleInt4)

# StyleInt4

```
public struct StyleInt4 : IStyleValue<int4>, IEquatable<StyleInt4>
```

## value

```
public int4 value { readonly get; set; }
```

## keyword

```
public StyleKeyword keyword { readonly get; set; }
```

## StyleInt4(int4)

```
public StyleInt4(int4 value)
```

## StyleInt4(StyleKeyword)

```
public StyleInt4(StyleKeyword keyword)
```

## L

```
public StyleInt L { get; }
```

## T

```
public StyleInt T { get; }
```

## R

```
public StyleInt R { get; }
```

## B

```
public StyleInt B { get; }
```

## Equals(StyleInt4)

```
public bool Equals(StyleInt4 other)
```

## ToString()

```
public override string ToString()
```

## StyleInt4(StyleKeyword)

```
public static implicit operator StyleInt4(StyleKeyword k)
```

## StyleInt4(int4)

```
public static implicit operator StyleInt4(int4 v)
```