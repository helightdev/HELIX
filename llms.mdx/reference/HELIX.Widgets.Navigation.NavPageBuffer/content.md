# NavPageBuffer (/reference/HELIX.Widgets.Navigation.NavPageBuffer)

# NavPageBuffer

```
public class NavPageBuffer : IEnumerable<NavPageBase>, IEnumerable
```

## Empty

```
public static readonly NavPageBuffer Empty
```

## pages

```
public readonly IReadOnlyList<NavPageBase> pages
```

## NavPageBuffer(List<NavPageBase>)

```
public NavPageBuffer(List<NavPageBase> pages)
```

## IsEmpty

```
public bool IsEmpty { get; }
```

## Count

```
public int Count { get; }
```

## GetEnumerator()

```
public IEnumerator<NavPageBase> GetEnumerator()
```

## Contains(NavPageBase)

```
public bool Contains(NavPageBase pageBase)
```