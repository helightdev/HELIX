# HButton (/reference/HELIX.Widgets.Universal.HButton)

# HButton

```
public class HButton : SingleChildStatefulWidget<HButton>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget, IEnumerable<Widget>, IEnumerable
```

A highly customizable button with various styling, size, and behavioral options.

## controller

```
public readonly ButtonController controller
```

## enabled

```
public readonly bool enabled
```

## focusKey

```
public readonly Key focusKey
```

## onClick

```
public readonly Action onClick
```

## palette

```
public readonly ColorTokenPalette palette
```

## radius

```
public readonly HInputRadius? radius
```

## selected

```
public readonly bool selected
```

## size

```
public readonly HButtonSize? size
```

## style

```
public readonly HButtonStyle style
```

## variant

```
public readonly HButtonVariant? variant
```

## HButton(HButtonVariant?, ButtonController, Key, bool, bool, Action, HButtonStyle, HInputRadius?, HButtonSize?, ColorTokenPalette, Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
public HButton(HButtonVariant? variant = null, ButtonController controller = null, Key focusKey = default, bool enabled = true, bool selected = false, Action onClick = null, HButtonStyle style = null, HInputRadius? radius = null, HButtonSize? size = null, ColorTokenPalette palette = null, Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a highly customizable button with various styling, size, and behavioral options.

## CreateState()

```
public override State<HButton> CreateState()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```