# WidgetState (/reference/HELIX.Widgets.WidgetState)

# WidgetState

```
[Flags]
public enum WidgetState : ushort
```

## None

```
None = 0
```

## Hovered

```
Hovered = 1
```

## Focused

```
Focused = 2
```

## Pressed

```
Pressed = 4
```

## Dragged

```
Dragged = 8
```

## Selected

```
Selected = 16
```

## Disabled

```
Disabled = 32
```

## Error

```
Error = 64
```

## Navigated

```
Navigated = 128
```

## Special1

```
Special1 = 256
```

## Special2

```
Special2 = 512
```

## InputKeyboardMouse

```
InputKeyboardMouse = 2048
```

## InputGamepad

```
InputGamepad = 4096
```

## InputTouch

```
InputTouch = 8192
```

## MetaState

```
MetaState = Hovered | Focused | Pressed | Dragged | Selected | Disabled | Error | Navigated | Special1 | Special2
```

## MetaInput

```
MetaInput = InputKeyboardMouse | InputGamepad | InputTouch
```

## MetaAll

```
MetaAll = MetaState | MetaInput
```

## ModNot

```
ModNot = 16384
```

## ModAny

```
ModAny = 32768
```