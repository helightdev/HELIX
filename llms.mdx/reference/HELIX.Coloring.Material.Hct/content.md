# Hct (/reference/HELIX.Coloring.Material.Hct)

# Hct

```
public sealed class Hct : IEquatable<Hct>
```

HCT, hue, chroma, and tone.

## Hue

```
public double Hue { get; set; }
```

## Chroma

```
public double Chroma { get; set; }
```

## Tone

```
public double Tone { get; set; }
```

## Equals(Hct)

```
public bool Equals(Hct other)
```

## From(double, double, double)

```
public static Hct From(double hue, double chroma, double tone)
```

## FromInt(int)

```
public static Hct FromInt(int argb)
```

## ToInt()

```
public int ToInt()
```

## InViewingConditions(ViewingConditions)

```
public Hct InViewingConditions(ViewingConditions vc)
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