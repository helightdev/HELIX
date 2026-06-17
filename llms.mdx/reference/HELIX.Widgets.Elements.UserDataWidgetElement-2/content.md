# UserDataWidgetElement<W, T> (/reference/HELIX.Widgets.Elements.UserDataWidgetElement-2)

# UserDataWidgetElement<W, T>

```
public class UserDataWidgetElement<W, T> : UserDataWidgetBaseElement, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider where W : Widget, IUserDataWidget<W, T> where T : VisualElement
```

## TypedDescriptor

```
public W TypedDescriptor { get; set; }
```

## CanReconcile(Widget)

```
public override bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public override bool Reconcile(Widget updated)
```

## ToStringShort()

```
public override string ToStringShort()
```