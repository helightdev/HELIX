# SubstanceLayers (/reference/HELIX.Widgets.Universal.SubstanceLayers)

# SubstanceLayers

```
public readonly struct SubstanceLayers : IReadOnlyList<Substance>, IReadOnlyCollection<Substance>, IEnumerable<Substance>, IEnumerable
```

Represents a collection of <a data-furef-uid="HELIX.Widgets.Universal.Substance">Substance</a>s.

May be implicitly converted from <a data-furef-uid="HELIX.Widgets.Universal.SubstanceBuilder">SubstanceBuilder</a>, a collection of <a data-furef-uid="HELIX.Widgets.Universal.Substance">Substance</a>
or single <a data-furef-uid="HELIX.Widgets.Universal.Substance">Substance</a> instances for ease of use.

## SubstanceLayers(IReadOnlyList<Substance>)

```
public SubstanceLayers(IReadOnlyList<Substance> substances)
```

Creates a new <a data-furef-uid="HELIX.Widgets.Universal.SubstanceLayers">SubstanceLayers</a> instance from a collection of <a data-furef-uid="HELIX.Widgets.Universal.Substance">Substance</a>s.

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

## SubstanceLayers(SubstanceBuilder)

```
public static implicit operator SubstanceLayers(SubstanceBuilder builder)
```

## SubstanceLayers(List<Substance>)

```
public static implicit operator SubstanceLayers(List<Substance> substances)
```

## SubstanceLayers(Substance[])

```
public static implicit operator SubstanceLayers(Substance[] substances)
```

## SubstanceLayers(Substance)

```
public static implicit operator SubstanceLayers(Substance substance)
```