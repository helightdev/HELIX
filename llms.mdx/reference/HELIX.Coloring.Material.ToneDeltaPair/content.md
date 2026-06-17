# ToneDeltaPair (/reference/HELIX.Coloring.Material.ToneDeltaPair)

# ToneDeltaPair

```
public sealed class ToneDeltaPair
```

Documents a constraint between two DynamicColors, in which their tones must
have a certain distance from each other.

## ToneDeltaPair(DynamicColor, DynamicColor, double, TonePolarity, bool)

```
public ToneDeltaPair(DynamicColor roleA, DynamicColor roleB, double delta, TonePolarity polarity, bool stayTogether)
```

## RoleA

```
public DynamicColor RoleA { get; }
```

## RoleB

```
public DynamicColor RoleB { get; }
```

## Delta

```
public double Delta { get; }
```

## Polarity

```
public TonePolarity Polarity { get; }
```

## StayTogether

```
public bool StayTogether { get; }
```