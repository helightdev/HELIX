# HTextField (/reference/HELIX.Widgets.Universal.HTextField)

# HTextField

```
public class HTextField : StatefulWidget<HTextField>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget
```

A highly customizable text field widget that can be used for text input.

## autocorrect

```
public readonly bool autocorrect
```

## controller

```
public readonly TextEditingController controller
```

## enabled

```
public readonly bool enabled
```

## focusKey

```
public readonly Key focusKey
```

## hideMobileInput

```
public readonly bool hideMobileInput
```

## initialValue

```
public readonly string initialValue
```

## isDelayed

```
public readonly bool isDelayed
```

## isPasswordField

```
public readonly bool isPasswordField
```

## isReadOnly

```
public readonly bool isReadOnly
```

## keyboardType

```
public readonly TouchScreenKeyboardType keyboardType
```

## maskChar

```
public readonly char maskChar
```

## maxLength

```
public readonly int maxLength
```

## multiline

```
public readonly bool multiline
```

## onChanged

```
public readonly Action<string> onChanged
```

## onSubmitted

```
public readonly Action<string> onSubmitted
```

## style

```
public readonly HTextFieldStyle style
```

## HTextField(TextEditingController, Key, HTextFieldStyle, bool, bool, bool, bool, bool, bool, TouchScreenKeyboardType, char, int, bool, string, Action<string>, Action<string>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HTextField(TextEditingController controller = null, Key focusKey = default, HTextFieldStyle style = null, bool multiline = false, bool autocorrect = true, bool isReadOnly = false, bool isPasswordField = false, bool isDelayed = false, bool hideMobileInput = true, TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default, char maskChar = '*', int maxLength = -1, bool enabled = true, string initialValue = "", Action<string> onChanged = null, Action<string> onSubmitted = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a highly customizable text field widget that can be used for text input.

## CreateState()

```
public override State<HTextField> CreateState()
```