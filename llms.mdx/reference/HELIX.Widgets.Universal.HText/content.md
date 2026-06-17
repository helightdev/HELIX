# HText (/reference/HELIX.Widgets.Universal.HText)

# HText

```
public class HText : WrappingBaseWidget<HText, Label>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IUserDataWidget<HText, Label>
```

A wrapper widget around <a data-furef-uid="UnityEngine.UIElements.Label">Label</a> that allows for easy configuration of the textual content.

## doubleClickSelectsWords

```
public readonly bool doubleClickSelectsWords
```

## emojiFallbackSupport

```
public readonly bool emojiFallbackSupport
```

## enableRichText

```
public readonly bool enableRichText
```

## languageDirection

```
public readonly LanguageDirection languageDirection
```

## parseEscapeSequences

```
public readonly bool parseEscapeSequences
```

## selectable

```
public readonly bool selectable
```

## text

```
public readonly string text
```

## tripleClickSelectsLine

```
public readonly bool tripleClickSelectsLine
```

## style

```
public TextStyle style
```

## HText(string, bool, bool, bool, bool, bool, bool, LanguageDirection, TextStyle, Key, object[], IReadOnlyCollection<Modifier>)

```
public HText(string text, bool enableRichText = false, bool emojiFallbackSupport = true, bool parseEscapeSequences = false, bool selectable = false, bool doubleClickSelectsWords = true, bool tripleClickSelectsLine = true, LanguageDirection languageDirection = LanguageDirection.Inherit, TextStyle style = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a wrapper widget around <a data-furef-uid="UnityEngine.UIElements.Label">Label</a> that allows for easy configuration of the textual content.

## Create()

```
public override Label Create()
```

## Apply(HText, Label)

```
public override void Apply(HText previous, Label element)
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```