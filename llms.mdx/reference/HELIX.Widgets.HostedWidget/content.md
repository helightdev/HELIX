# HostedWidget (/reference/HELIX.Widgets.HostedWidget)

# HostedWidget

```
public class HostedWidget : WrappingBaseWidget<HostedWidget, VisualElement>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IUserDataWidget<HostedWidget, VisualElement>
```

## element

```
public readonly VisualElement element
```

## HostedWidget(VisualElement, Key, object[], IReadOnlyCollection<Modifier>)

```
public HostedWidget(VisualElement element, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## Create()

```
public override VisualElement Create()
```

## Apply(HostedWidget, VisualElement)

```
public override void Apply(HostedWidget previous, VisualElement target)
```

## CanReconcile(HostedWidget, VisualElement)

```
public override bool CanReconcile(HostedWidget previous, VisualElement target)
```