# SubstanceBuilder (/reference/HELIX.Widgets.Universal.SubstanceBuilder)

# SubstanceBuilder

```
public sealed class SubstanceBuilder : WidgetStateProperty<IReadOnlyList<Substance>>, IDiagnosticable, IReadOnlyList<Substance>, IReadOnlyCollection<Substance>, IEnumerable<Substance>, IEnumerable, ISubstanceBuilder<SubstanceBuilder>
```

## SubstanceBuilder(IThemeProvider, bool)

```
public SubstanceBuilder(IThemeProvider context, bool listening = false)
```

## GetEnumerator()

```
public IEnumerator<Substance> GetEnumerator()
```

## Count

```
public int Count { get; }
```

## this[int]

```
public Substance this[int index] { get; }
```

## Listening

```
public bool Listening { get; }
```

## Self

```
public SubstanceBuilder Self { get; }
```

## Append<T>(Func<IThemeProvider, T>)

```
public BuilderAndSubstance<SubstanceBuilder, T> Append<T>(Func<IThemeProvider, T> builder) where T : Substance
```

## Clear()

```
public void Clear()
```

## TryResolve(WidgetState, out IReadOnlyList<Substance>)

```
public override bool TryResolve(WidgetState state, out IReadOnlyList<Substance> value)
```