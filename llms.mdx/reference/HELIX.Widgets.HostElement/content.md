# HostElement (/reference/HELIX.Widgets.HostElement)

# HostElement

```
public class HostElement : DiagnosticableBase, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider
```

## HostElement(VisualElement)

```
public HostElement(VisualElement element)
```

## Element

```
public VisualElement Element { get; }
```

## Descriptor

```
public Widget Descriptor { get; }
```

## ParentContext

```
public BuildContext ParentContext { get; set; }
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

## HierarchyDepth

```
public int HierarchyDepth { get; }
```

## CanReconcile(Widget)

```
public bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
public bool Reconcile(Widget updated)
```

## ToStringShort()

```
public override string ToStringShort()
```

## Equals(HostElement)

```
protected bool Equals(HostElement other)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```