# SingleChildStatefulWidget<T> (/reference/HELIX.Widgets.SingleChildStatefulWidget-1)

# SingleChildStatefulWidget<T>

```
public abstract class SingleChildStatefulWidget<T> : StatefulWidget<T>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget, IEnumerable<Widget>, IEnumerable where T : SingleChildStatefulWidget<T>
```

## child

```
public Widget child
```

## SingleChildStatefulWidget(Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
protected SingleChildStatefulWidget(Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## GetEnumerator()

```
public IEnumerator<Widget> GetEnumerator()
```

## Add(Widget)

```
public void Add(Widget widget)
```