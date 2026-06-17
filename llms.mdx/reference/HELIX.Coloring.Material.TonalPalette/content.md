# TonalPalette (/reference/HELIX.Coloring.Material.TonalPalette)

# TonalPalette

```
public sealed class TonalPalette : IEquatable<TonalPalette>
```

A convenience class for retrieving colors that are constant in hue and
chroma, but vary in tone.

## CommonTones

```
public static readonly int[] CommonTones
```

## CommonSize

```
public static int CommonSize { get; }
```

## Hue

```
public double Hue { get; }
```

## Chroma

```
public double Chroma { get; }
```

## KeyColor

```
public Hct KeyColor { get; }
```

## AsList

```
public List<int> AsList { get; }
```

## Equals(TonalPalette)

```
public bool Equals(TonalPalette other)
```

## Of(double, double)

```
public static TonalPalette Of(double hue, double chroma)
```

## FromHct(Hct)

```
public static TonalPalette FromHct(Hct hct)
```

## FromList(IReadOnlyList<int>)

```
public static TonalPalette FromList(IReadOnlyList<int> colors)
```

## Get(int)

```
public int Get(int tone)
```

Returns the ARGB representation of an HCT color at the given tone.

## GetHct(double)

```
public Hct GetHct(double tone)
```

Returns the HCT color at the given tone.

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