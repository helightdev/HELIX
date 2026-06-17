# NavStackModificationResult (/reference/HELIX.Widgets.Navigation.NavStackModificationResult)

# NavStackModificationResult

```
public class NavStackModificationResult
```

## NavStackModificationResult(NavPageBuffer, NavPageBuffer, NavModificationType)

```
public NavStackModificationResult(NavPageBuffer before, NavPageBuffer after, NavModificationType type = NavModificationType.Complex)
```

## Before

```
public NavPageBuffer Before { get; }
```

## After

```
public NavPageBuffer After { get; }
```

## AddedPages

```
public NavPageBuffer AddedPages { get; }
```

## RemovedPages

```
public NavPageBuffer RemovedPages { get; }
```

## MinMaxDeltaIndices

```
public int4 MinMaxDeltaIndices { get; }
```

## Trimmed

```
public (NavPageBuffer TrimmedBefore, NavPageBuffer TrimmedAfter) Trimmed { get; }
```

## TrimmedBefore

```
public NavPageBuffer TrimmedBefore { get; }
```

## TrimmedAfter

```
public NavPageBuffer TrimmedAfter { get; }
```

## Type

```
public NavModificationType Type { get; }
```