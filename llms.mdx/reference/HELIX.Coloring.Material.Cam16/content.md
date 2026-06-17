# Cam16 (/reference/HELIX.Coloring.Material.Cam16)

# Cam16

```
public sealed class Cam16
```

CAM16, a color appearance model.

## Cam16(double, double, double, double, double, double, double, double, double)

```
public Cam16(double hue, double chroma, double j, double q, double m, double s, double jstar, double astar, double bstar)
```

## Hue

```
public double Hue { get; }
```

## Chroma

```
public double Chroma { get; }
```

## J

```
public double J { get; }
```

## Q

```
public double Q { get; }
```

## M

```
public double M { get; }
```

## S

```
public double S { get; }
```

## Jstar

```
public double Jstar { get; }
```

## Astar

```
public double Astar { get; }
```

## Bstar

```
public double Bstar { get; }
```

## Distance(Cam16)

```
public double Distance(Cam16 other)
```

## FromInt(int)

```
public static Cam16 FromInt(int argb)
```

## FromIntInViewingConditions(int, ViewingConditions)

```
public static Cam16 FromIntInViewingConditions(int argb, ViewingConditions viewingConditions)
```

## FromXyzInViewingConditions(double, double, double, ViewingConditions)

```
public static Cam16 FromXyzInViewingConditions(double x, double y, double z, ViewingConditions viewingConditions)
```

## FromJch(double, double, double)

```
public static Cam16 FromJch(double j, double c, double h)
```

## FromJchInViewingConditions(double, double, double, ViewingConditions)

```
public static Cam16 FromJchInViewingConditions(double j, double c, double h, ViewingConditions viewingConditions)
```

## FromUcs(double, double, double)

```
public static Cam16 FromUcs(double jstar, double astar, double bstar)
```

## FromUcsInViewingConditions(double, double, double, ViewingConditions)

```
public static Cam16 FromUcsInViewingConditions(double jstar, double astar, double bstar, ViewingConditions viewingConditions)
```

## ToInt()

```
public int ToInt()
```

## Viewed(ViewingConditions)

```
public int Viewed(ViewingConditions viewingConditions)
```

## XyzInViewingConditions(ViewingConditions)

```
public double3 XyzInViewingConditions(ViewingConditions viewingConditions)
```