# HSliderStyle (/reference/HELIX.Widgets.Universal.Styles.HSliderStyle)

# HSliderStyle

```
public class HSliderStyle : DiagnosticableBase, IDiagnosticable
```

Defines the visual style of a <a data-furef-uid="HELIX.Widgets.Universal.HSlider">HSlider</a>.

<a data-furef-uid="HELIX.Widgets.WidgetState.Special1">WidgetState.Special1</a> and <a data-furef-uid="HELIX.Widgets.WidgetState.Special2">WidgetState.Special2</a> are used to differentiate
between horizontal and vertical sliders respectively.

## constraints

```
public WidgetStateProperty<BoxConstraints> constraints
```

Defines constraints applied using <a data-furef-uid="HELIX.Widgets.Modifiers.SizeModifier">SizeModifier</a>.

## progress

```
public SubstanceLayers progress
```

Controls the progress <a data-furef-uid="HELIX.Widgets.Universal.HSubstanceBox.substances">HSubstanceBox.substances</a>.

## thumb

```
public SubstanceLayers thumb
```

Controls the thumb <a data-furef-uid="HELIX.Widgets.Universal.HSubstanceBox.substances">HSubstanceBox.substances</a>.

## track

```
public SubstanceLayers track
```

Controls the track background <a data-furef-uid="HELIX.Widgets.Universal.HSubstanceBox.substances">HSubstanceBox.substances</a>.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## DefaultStyleOf(IThemeProvider, ColorTokenPalette, SurfaceColorPalette)

```
public static HSliderStyle DefaultStyleOf(IThemeProvider context, ColorTokenPalette palette = null, SurfaceColorPalette surfacePalette = null)
```

## DefaultScrollbarStyleOf(BuildContext)

```
public static HSliderStyle DefaultScrollbarStyleOf(BuildContext context)
```