# HGap (/reference/HELIX.Widgets.Universal.HGap)

# HGap

```
public class HGap : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate
```

A widget used to easily add spacing between other widgets.

## axis

```
public readonly Axis? axis
```

## size

```
public readonly StyleLength? size
```

## level

```
public readonly int level
```

## HGap(int, Axis?, Key, object[], IReadOnlyCollection<Modifier>)

```
public HGap(int level = 1, Axis? axis = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget to add spacing between other widgets using a theme-defined size.

## HGap(StyleLength?, Axis?, Key, object[], IReadOnlyCollection<Modifier>)

```
public HGap(StyleLength? size, Axis? axis = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget to add spacing between other widgets using a custom size.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.