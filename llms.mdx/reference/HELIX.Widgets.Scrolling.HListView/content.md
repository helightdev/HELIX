# HListView (/reference/HELIX.Widgets.Scrolling.HListView)

# HListView

```
public class HListView : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate
```

A widget that efficiently renders a large number of children in a vertical list.

Wraps Unity's <a data-furef-uid="UnityEngine.UIElements.ListView">ListView</a>.

## builder

```
public readonly BuildFunction<int> builder
```

## count

```
public readonly int count
```

## fixedItemHeight

```
public readonly float fixedItemHeight
```

## scrollController

```
public readonly ScrollController scrollController
```

## HListView(BuildFunction<int>, int, float, ScrollController, Key, object[], IReadOnlyCollection<Modifier>)

```
public HListView(BuildFunction<int> builder, int count, float fixedItemHeight = -1, ScrollController scrollController = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget that efficiently renders a large number of children in a vertical list.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.