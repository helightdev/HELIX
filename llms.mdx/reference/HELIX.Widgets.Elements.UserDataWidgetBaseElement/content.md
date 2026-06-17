# UserDataWidgetBaseElement (/reference/HELIX.Widgets.Elements.UserDataWidgetBaseElement)

# UserDataWidgetBaseElement

```
public abstract class UserDataWidgetBaseElement : IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider
```

## Element

```
public VisualElement Element { get; set; }
```

## Descriptor

```
public Widget Descriptor { get; set; }
```

## ParentContext

```
public BuildContext ParentContext { get; set; }
```

## HierarchyDepth

```
public int HierarchyDepth { get; set; }
```

## CanReconcile(Widget)

```
public abstract bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public abstract bool Reconcile(Widget updated)
```

## GetThemed<T>(BaseThemeProperty<T>, bool)

```
public T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true)
```

Resolves the theme value for the given property.

## TryGetThemed<S>(BaseThemeProperty<S>, out S, bool)

```
public bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true)
```

Tries to resolve the theme value for the given property.

## DebugDescribeChildren()

```
public virtual List<DiagnosticsNode> DebugDescribeChildren()
```

## ToStringDeep(string, string, DiagnosticLevel, int)

```
public virtual string ToStringDeep(string prefixLineOne = "", string prefixOtherLines = null, DiagnosticLevel minLevel = DiagnosticLevel.Debug, int wrapWidth = 65)
```

## ToStringShallow(string, DiagnosticLevel)

```
public virtual string ToStringShallow(string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug)
```

## ToDiagnosticsNode(string, DiagnosticsTreeStyle?)

```
public virtual DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null)
```

## ToStringShort()

```
public virtual string ToStringShort()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public virtual void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## ToString()

```
public override string ToString()
```