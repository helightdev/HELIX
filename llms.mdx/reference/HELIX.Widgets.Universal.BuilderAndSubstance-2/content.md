# BuilderAndSubstance<TBuilder, T> (/reference/HELIX.Widgets.Universal.BuilderAndSubstance-2)

# BuilderAndSubstance<TBuilder, T>

```
public readonly struct BuilderAndSubstance<TBuilder, T> : ISubstanceBuilder<TBuilder> where TBuilder : ISubstanceBuilder<TBuilder> where T : Substance
```

## valueBuilder

```
public readonly TBuilder valueBuilder
```

## value

```
public readonly T value
```

## BuilderAndSubstance(TBuilder, T)

```
public BuilderAndSubstance(TBuilder valueBuilder, T value)
```

## Listening

```
public bool Listening { get; }
```

## Self

```
public TBuilder Self { get; }
```

## Append<TNext>(Func<IThemeProvider, TNext>)

```
public BuilderAndSubstance<TBuilder, TNext> Append<TNext>(Func<IThemeProvider, TNext> builder) where TNext : Substance
```

## T(BuilderAndSubstance<TBuilder, T>)

```
public static implicit operator T(BuilderAndSubstance<TBuilder, T> wrapper)
```

## TBuilder(BuilderAndSubstance<TBuilder, T>)

```
public static implicit operator TBuilder(BuilderAndSubstance<TBuilder, T> wrapper)
```