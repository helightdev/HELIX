# MaterialSwatch (/reference/HELIX.Coloring.Material.MaterialSwatch)

# MaterialSwatch

```
public abstract class MaterialSwatch
```

Base type for color swatches.
Supports a primary value, indexed shades, and conversion to Unity colors.

## name

```
public readonly string name
```

## MaterialSwatch(int, string)

```
protected MaterialSwatch(int primaryArgb, string name = null)
```

## MaterialSwatch(uint, string)

```
protected MaterialSwatch(uint primaryArgb, string name = null)
```

## PrimaryArgb

```
public int PrimaryArgb { get; }
```

Primary ARGB value of the swatch. For normal swatches this is typically shade 500.
For accent swatches this is typically shade 200.

## Value

```
public Color Value { get; }
```

## StyleValue

```
public StyleColor StyleValue { get; }
```

## Weights

```
public abstract ReadOnlySpan<int> Weights { get; }
```

Ordered list of valid shade weights for this swatch.

## this[int]

```
public abstract Color this[int weight] { get; }
```

Resolves a shade by weight.

## GetArgb(int)

```
public virtual int GetArgb(int weight)
```

Resolves a shade ARGB by weight.

## ToSwatch()

```
public Color[] ToSwatch()
```

Returns all shades in the natural order of this swatch type.

## ToSwatch(params int[])

```
public Color[] ToSwatch(params int[] weights)
```

Returns shades for arbitrary weights.

## ToString()

```
public override string ToString()
```

## Color(MaterialSwatch)

```
public static implicit operator Color(MaterialSwatch swatch)
```

## StyleColor(MaterialSwatch)

```
public static implicit operator StyleColor(MaterialSwatch swatch)
```