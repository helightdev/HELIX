# HBox (/reference/HELIX.Widgets.Universal.HBox)

# HBox

```
public class HBox : SingleChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IEnumerable<Widget>, IEnumerable
```

A primitive widget that can be used to apply a background and border.

## alignment

```
public readonly Alignment alignment
```

## background

```
public readonly BackgroundStyle background
```

## border

```
public readonly Border border
```

## borderRadius

```
public readonly BorderRadius borderRadius
```

## HBox(Alignment?, BackgroundStyle, Border?, BorderRadius?, Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
public HBox(Alignment? alignment = null, BackgroundStyle background = null, Border? border = null, BorderRadius? borderRadius = null, Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a primitive widget that can be used to apply a background and border.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```