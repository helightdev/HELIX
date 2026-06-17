# GenericTextInput (/reference/HELIX.Widgets.Elements.GenericTextInput)

# GenericTextInput

```
[UxmlElement]
public class GenericTextInput : BaseElement, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IElement
```

## GenericTextInput()

```
public GenericTextInput()
```

## BackingTextField

```
public TextField BackingTextField { get; }
```

## TextInputStyle

```
[UxmlAttribute]
[Tooltip("The visual style of the text input, which determines the selection and cursor colors.")]
public GenericTextInputStyle TextInputStyle { get; set; }
```

## SelectionColor

```
[UxmlAttribute]
[Tooltip("Color of the text selection highlight. Only applies if TextInputStyle is set to Custom.")]
public Color SelectionColor { get; set; }
```

## CursorColor

```
[UxmlAttribute]
[Tooltip("Color of the text field's cursor. Only applies if TextInputStyle is set to Custom.")]
public Color CursorColor { get; set; }
```

## Expands

```
[UxmlAttribute]
[Tooltip("Whether the text field should stretch to fill available space. If false, the text field will size to its content.")]
public bool Expands { get; set; }
```

## Value

```
[Header("Delegated Properties")]
[UxmlAttribute]
public string Value { get; set; }
```

## Multiline

```
[UxmlAttribute]
public bool Multiline { get; set; }
```

## IsReadOnly

```
[UxmlAttribute]
public bool IsReadOnly { get; set; }
```

## MaxLength

```
[UxmlAttribute]
public int MaxLength { get; set; }
```

## IsPasswordField

```
[UxmlAttribute]
public bool IsPasswordField { get; set; }
```

## MaskChar

```
[UxmlAttribute]
public char MaskChar { get; set; }
```

## AutoCorrection

```
[UxmlAttribute]
public bool AutoCorrection { get; set; }
```

## HideMobileInput

```
[UxmlAttribute]
public bool HideMobileInput { get; set; }
```

## KeyboardType

```
[UxmlAttribute]
public TouchScreenKeyboardType KeyboardType { get; set; }
```

## IsDelayed

```
[UxmlAttribute]
public bool IsDelayed { get; set; }
```

## TextAlignment

```
public TextAnchor TextAlignment { get; set; }
```

## RequestEditingFocus()

```
public void RequestEditingFocus()
```

## OnBeginEditing

```
public event Action OnBeginEditing
```

## OnEndEditing

```
public event Action OnEndEditing
```

## OnValueChanged

```
public event Action<string> OnValueChanged
```

## OnSubmit

```
public event Action<string> OnSubmit
```

## OnCancel

```
public event Action OnCancel
```

## OnFocus

```
public event Action OnFocus
```

## OnBlur

```
public event Action OnBlur
```