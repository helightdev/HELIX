# HThemeProvider (/reference/HELIX.Widgets.Universal.HThemeProvider)

# HThemeProvider

```
public class HThemeProvider : SingleChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IEnumerable<Widget>, IEnumerable
```

A widget that provides inheritable theme properties to its descendants.

## components

```
public readonly List<ThemeComponent> components
```

## properties

```
public readonly Dictionary<ThemeProperty, object> properties
```

## HThemeProvider(List<ThemeComponent>, Dictionary<ThemeProperty, object>, Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
public HThemeProvider(List<ThemeComponent> components = null, Dictionary<ThemeProperty, object> properties = null, Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget that provides inheritable theme properties to its descendants.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.