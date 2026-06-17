# HIcon (/reference/HELIX.Widgets.Universal.HIcon)

# HIcon

```
public class HIcon : WrappingBaseWidget<HIcon, Label>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IUserDataWidget<HIcon, Label>
```

A wrapper widget around <a data-furef-uid="UnityEngine.UIElements.Label">Label</a> that is optimized for displaying font based icons.

## font

```
public StyleFontDefinition font
```

## fontSize

```
public StyleLength fontSize
```

## color

```
public StyleColor color
```

## align

```
public StyleEnum<TextAnchor> align
```

## text

```
public readonly string text
```

## HIcon(string, FontDefinition, StyleLength?, StyleColor?, TextAnchor?, Key, object[], IReadOnlyCollection<Modifier>)

```
public HIcon(string text, FontDefinition font, StyleLength? size = null, StyleColor? color = null, TextAnchor? align = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a wrapper widget around <a data-furef-uid="UnityEngine.UIElements.Label">Label</a> that is optimized for displaying font based icons.

## HIcon(char, FontDefinition, StyleLength?, StyleColor?, TextAnchor?, Key, object[], IReadOnlyCollection<Modifier>)

```
public HIcon(char icon, FontDefinition font, StyleLength? size = null, StyleColor? color = null, TextAnchor? align = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a wrapper widget around <a data-furef-uid="UnityEngine.UIElements.Label">Label</a> that is optimized for displaying font based icons.

## Create()

```
public override Label Create()
```

## Apply(HIcon, Label)

```
public override void Apply(HIcon previous, Label element)
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```