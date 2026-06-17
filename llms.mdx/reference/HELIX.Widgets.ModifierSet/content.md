# ModifierSet (/reference/HELIX.Widgets.ModifierSet)

# ModifierSet

```
public class ModifierSet : DiagnosticableBase, IDiagnosticable, IReadOnlyCollection<Modifier>, IEnumerable<Modifier>, IEnumerable
```

## Empty

```
public static readonly ModifierSet Empty
```

## DefaultFlexFill

```
public static readonly ModifierSet DefaultFlexFill
```

## DefaultFlexTight

```
public static readonly ModifierSet DefaultFlexTight
```

## DefaultFlexFillAndStacking

```
public static readonly ModifierSet DefaultFlexFillAndStacking
```

## ModifierSet(int)

```
public ModifierSet(int capacity = 1)
```

## ModifierSet(IEnumerable<Modifier>)

```
public ModifierSet(IEnumerable<Modifier> modifiers)
```

## ReadOnly

```
public bool ReadOnly { get; }
```

## GetEnumerator()

```
public IEnumerator<Modifier> GetEnumerator()
```

## Count

```
public int Count { get; }
```

## Add(Modifier)

```
public bool Add(Modifier modifier)
```

## Add(IEnumerable<Modifier>)

```
public bool Add(IEnumerable<Modifier> modifiers)
```

## AddCollection(IReadOnlyCollection<Modifier>)

```
public void AddCollection(IReadOnlyCollection<Modifier> modifiers)
```

## Contains(Modifier)

```
public bool Contains(Modifier modifier)
```

## TryGetValue(Modifier, out Modifier)

```
public bool TryGetValue(Modifier modifier, out Modifier existing)
```

## AddThrowing(Modifier)

```
public void AddThrowing(Modifier modifier)
```

## AddThrowing(IEnumerable<Modifier>)

```
public void AddThrowing(IEnumerable<Modifier> modifiers)
```

## AddThrowingCollection(IReadOnlyCollection<Modifier>)

```
public void AddThrowingCollection(IReadOnlyCollection<Modifier> modifiers)
```

## Sealed()

```
public ModifierSet Sealed()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```