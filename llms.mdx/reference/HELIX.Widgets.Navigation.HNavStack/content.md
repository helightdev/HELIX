# HNavStack (/reference/HELIX.Widgets.Navigation.HNavStack)

# HNavStack

```
public class HNavStack : SingleChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IEnumerable<Widget>, IEnumerable
```

A widget that manages a stack of <a data-furef-uid="HELIX.Widgets.Navigation.NavPageBase">NavPageBase</a>s.

## defaultTransition

```
public readonly PageTransition defaultTransition
```

## HNavStack(PageTransition, Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
public HNavStack(PageTransition defaultTransition = null, Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget that manages a stack of <a data-furef-uid="HELIX.Widgets.Navigation.NavPageBase">NavPageBase</a>s.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```