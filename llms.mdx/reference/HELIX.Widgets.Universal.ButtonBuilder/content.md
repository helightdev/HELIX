# ButtonBuilder (/reference/HELIX.Widgets.Universal.ButtonBuilder)

# ButtonBuilder

```
[Obsolete("Use HButton or the ButtonControllerModifier itself directly or create a custom element")]
public class ButtonBuilder : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate
```

## alignment

```
public Alignment alignment
```

## builder

```
public BuildFunction<WidgetState> builder
```

## enabled

```
public bool enabled
```

## onClick

```
public Action onClick
```

## selected

```
public bool selected
```

## ButtonBuilder()

```
public ButtonBuilder()
```

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```