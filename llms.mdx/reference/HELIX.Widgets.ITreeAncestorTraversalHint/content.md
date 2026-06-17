# ITreeAncestorTraversalHint (/reference/HELIX.Widgets.ITreeAncestorTraversalHint)

# ITreeAncestorTraversalHint

```
public interface ITreeAncestorTraversalHint
```

Provides a hint to the <a data-furef-uid="HELIX.Widgets.Reconciler">Reconciler</a> that the target is a part of <a data-furef-uid="HELIX.Widgets.ITreeAncestorTraversalHint.Owner">ITreeAncestorTraversalHint.Owner</a>.

<p>
Some widgets and modifiers try to resolve their parent using <a data-furef-uid="HELIX.Widgets.BuildContext.GetDirectParent(UnityEngine.UIElements.VisualElement)">BuildContext.GetDirectParent</a> to access
metadata like <a data-furef-uid="HELIX.Widgets.Elements.IPreferExplicitFlex">IPreferExplicitFlex</a> or <a data-furef-uid="HELIX.Widgets.Elements.IPreferStacking">IPreferStacking</a>. In cases where
<a data-furef-uid="HELIX.Widgets.BuildContext.ParentContext">BuildContext.ParentContext</a> is not available, the <a data-furef-uid="HELIX.Widgets.Reconciler">Reconciler</a> will attempt to traverse
the tree upwards once and check if the element is a <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> or provides a traversal hint
via direct implementation or by setting a <a data-furef-uid="HELIX.Widgets.ElementTreeAncestorTraversalHint">ElementTreeAncestorTraversalHint</a> in its <a data-furef-uid="UnityEngine.UIElements.VisualElement.userData">userData</a>.
</p>

## Owner

```
IWidgetElement Owner { get; }
```