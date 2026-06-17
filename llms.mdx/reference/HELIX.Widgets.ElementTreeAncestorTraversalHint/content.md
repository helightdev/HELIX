# ElementTreeAncestorTraversalHint (/reference/HELIX.Widgets.ElementTreeAncestorTraversalHint)

# ElementTreeAncestorTraversalHint

```
public class ElementTreeAncestorTraversalHint : ITreeAncestorTraversalHint
```

Can be set as a <a data-furef-uid="UnityEngine.UIElements.VisualElement.userData">userData</a> to hint to the <a data-furef-uid="HELIX.Widgets.Reconciler">Reconciler</a> that the element
is a part of <a data-furef-uid="HELIX.Widgets.ElementTreeAncestorTraversalHint.Owner">ElementTreeAncestorTraversalHint.Owner</a>.

## ElementTreeAncestorTraversalHint(IWidgetElement)

```
public ElementTreeAncestorTraversalHint(IWidgetElement owner)
```

## Owner

```
public IWidgetElement Owner { get; }
```