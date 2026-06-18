# HelixSpanAllocator (/reference/HELIX.HelixSpanAllocator)

# HelixSpanAllocator

```
public static class HelixSpanAllocator
```

## Lease<T, R>(ReferenceSpanAction<T, R>, int, ref R)

```
public static void Lease<T, R>(ReferenceSpanAction<T, R> action, int length, ref R reference) where T : unmanaged
```

## Lease<T, V>(ValueSpanAction<T, V>, int, V)

```
public static void Lease<T, V>(ValueSpanAction<T, V> action, int length, V value) where T : unmanaged
```