# RingBuffer<T> (/reference/HELIX.Widgets.Utilities.RingBuffer-1)

# RingBuffer<T>

```
public class RingBuffer<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
```

## RingBuffer(int)

```
public RingBuffer(int capacity)
```

## Capacity

```
public int Capacity { get; }
```

## Count

```
public int Count { get; }
```

## GetEnumerator()

```
public IEnumerator<T> GetEnumerator()
```

## this[int]

```
public T this[int index] { get; }
```

## Add(T)

```
public void Add(T item)
```

## Get(int)

```
public T Get(int i)
```

## Clear()

```
public void Clear()
```