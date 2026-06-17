# Key (/reference/HELIX.Widgets.Key)

# Key

```
[SuppressMessage("ReSharper", "Unity.BurstAccessingManagedMethod")]
[SuppressMessage("ReSharper", "Unity.BurstLoadingManagedType")]
[SuppressMessage("ReSharper", "Unity.BurstFunctionSignatureContainsManagedTypes")]
public readonly struct Key : IEquatable<Key>
```

## value

```
public readonly ulong value
```

## details

```
public readonly BaseKey details
```

## None

```
public static readonly Key None
```

## Key(ulong)

```
public Key(ulong value)
```

## Key(ulong, BaseKey)

```
public Key(ulong value, BaseKey detailsKey)
```

## IsNone

```
public bool IsNone { get; }
```

## Equals(Key)

```
public bool Equals(Key other)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```

## operator ==(Key, Key)

```
public static bool operator ==(Key a, Key b)
```

## operator !=(Key, Key)

```
public static bool operator !=(Key a, Key b)
```

## ToString()

```
public override string ToString()
```

## Key(ulong)

```
public static implicit operator Key(ulong value)
```

## Key(uint)

```
public static implicit operator Key(uint value)
```

## Key(int)

```
public static implicit operator Key(int value)
```

## Key(string)

```
public static implicit operator Key(string value)
```

## Key(BaseKey)

```
public static implicit operator Key(BaseKey detailsKey)
```