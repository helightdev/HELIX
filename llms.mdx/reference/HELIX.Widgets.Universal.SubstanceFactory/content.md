# SubstanceFactory (/reference/HELIX.Widgets.Universal.SubstanceFactory)

# SubstanceFactory

```
public sealed class SubstanceFactory : ISubstanceBuilder<SubstanceFactory>
```

## Instance

```
public static readonly SubstanceFactory Instance
```

## Listening

```
public bool Listening { get; }
```

## Self

```
public SubstanceFactory Self { get; }
```

## Append<T>(Func<IThemeProvider, T>)

```
public BuilderAndSubstance<SubstanceFactory, T> Append<T>(Func<IThemeProvider, T> builder) where T : Substance
```