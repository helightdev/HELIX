# BuildContext (/reference/HELIX.Widgets.BuildContext)

# BuildContext

```
public interface BuildContext : IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider
```

<p>Represents the context in which a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a> is being built.</p>
<p>Provides access to the widget's parent, theme, and other contextual information.</p>

## Current

```
public static BuildContext Current
```

## ReconcilerCurrent

```
public static BuildContext ReconcilerCurrent
```

## Descriptor

```
Widget Descriptor { get; }
```

## ParentContext

```
BuildContext ParentContext { get; set; }
```

## IsUserWidget

```
bool IsUserWidget { get; }
```

## GetUserTarget(BuildContext, BuildContext)

```
public static BuildContext GetUserTarget(BuildContext start, BuildContext except = null)
```

## GetAncestorChain(BuildContext)

```
[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
public static IEnumerable<BuildContext> GetAncestorChain(BuildContext start)
```

## GetDirectParent(VisualElement)

```
[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
public static BuildContext GetDirectParent(VisualElement current)
```

## FindParent<T>(BuildContext)

```
public static BuildContext FindParent<T>(BuildContext context) where T : Widget
```