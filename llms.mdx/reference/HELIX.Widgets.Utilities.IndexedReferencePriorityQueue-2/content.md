# IndexedReferencePriorityQueue<TElement, TPriority> (/reference/HELIX.Widgets.Utilities.IndexedReferencePriorityQueue-2)

# IndexedReferencePriorityQueue<TElement, TPriority>

```
public class IndexedReferencePriorityQueue<TElement, TPriority> where TElement : class where TPriority : IComparable<TPriority>
```

## Count

```
public int Count { get; }
```

## Contains(TElement)

```
public bool Contains(TElement element)
```

## Enqueue(TElement, TPriority)

```
public bool Enqueue(TElement element, TPriority priority)
```

## Dequeue()

```
public TElement Dequeue()
```

## TryDequeue(out TElement)

```
public bool TryDequeue(out TElement element)
```

## Remove(TElement)

```
public bool Remove(TElement element)
```