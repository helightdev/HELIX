# Transition (/reference/HELIX.Types.Transition)

# Transition

```
public struct Transition : IEquatable<Transition>
```

## property

```
public readonly StylePropertyName property
```

## easing

```
public EasingFunction easing
```

## duration

```
public TimeValue duration
```

## delay

```
public TimeValue delay
```

## DefaultDuration

```
public const float DefaultDuration = 200
```

## Transition(StylePropertyName)

```
public Transition(StylePropertyName property)
```

## Equals(Transition)

```
public bool Equals(Transition other)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```

## ToString()

```
public override string ToString()
```

## Transition(StylePropertyName)

```
public static implicit operator Transition(StylePropertyName propertyName)
```