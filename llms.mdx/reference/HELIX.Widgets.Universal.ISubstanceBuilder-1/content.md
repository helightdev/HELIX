# ISubstanceBuilder<TBuilder> (/reference/HELIX.Widgets.Universal.ISubstanceBuilder-1)

# ISubstanceBuilder<TBuilder>

```
public interface ISubstanceBuilder<TBuilder> where TBuilder : ISubstanceBuilder<TBuilder>
```

## Listening

```
bool Listening { get; }
```

## Self

```
TBuilder Self { get; }
```

## Append<T>(Func<IThemeProvider, T>)

```
BuilderAndSubstance<TBuilder, T> Append<T>(Func<IThemeProvider, T> builder) where T : Substance
```