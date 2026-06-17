# HButtonStyle (/reference/HELIX.Widgets.Universal.Styles.HButtonStyle)

# HButtonStyle

```
public class HButtonStyle : DiagnosticableBase, IDiagnosticable
```

Defines the visual appearance of a <a data-furef-uid="HELIX.Widgets.Universal.HButton">HButton</a>.

## Default

```
public static HButtonStyle Default
```

## alignment

```
public WidgetStateProperty<Alignment> alignment
```

Controls <a data-furef-uid="HELIX.Widgets.Universal.HSubstanceBox.alignment">HSubstanceBox.alignment</a>.

## layers

```
public SubstanceLayers layers
```

Controls the background <a data-furef-uid="HELIX.Widgets.Universal.HSubstanceBox.substances">HSubstanceBox.substances</a>.

## constraints

```
public WidgetStateProperty<BoxConstraints> constraints
```

Defines constraints applied using <a data-furef-uid="HELIX.Widgets.Modifiers.SizeModifier">SizeModifier</a>.

## opacity

```
public WidgetStateProperty<float> opacity
```

Defines the opacity of the button applied using <a data-furef-uid="HELIX.Widgets.Modifiers.OpacityModifier">OpacityModifier</a>.

## modifiers

```
public WidgetStateProperty<ModifierSet> modifiers
```

Defines additional modifiers that may override the button's default modifiers.

## padding

```
public WidgetStateProperty<StyleLength4> padding
```

Defines padding between the button's background and its content.

## textStyle

```
public WidgetStateProperty<TextStyle> textStyle
```

Define the default text applied using <a data-furef-uid="HELIX.Widgets.Modifiers.TextStyleModifier">TextStyleModifier</a>.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```