# InputConfiguration (/reference/HELIX.Widgets.Prompts.InputConfiguration)

# InputConfiguration

```
[Serializable]
public struct InputConfiguration : IEquatable<InputConfiguration>
```

## Default

```
public static readonly InputConfiguration Default
```

## deviceType

```
public InputDeviceType deviceType
```

## gamepadVariant

```
public GamepadVariant gamepadVariant
```

## flag

```
public int flag
```

## InputMask

```
public WidgetState InputMask { get; }
```

## InputConfiguration(InputDeviceType, GamepadVariant, int)

```
public InputConfiguration(InputDeviceType deviceType, GamepadVariant gamepadVariant, int flag = 0)
```

## Equals(InputConfiguration)

```
public bool Equals(InputConfiguration other)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```