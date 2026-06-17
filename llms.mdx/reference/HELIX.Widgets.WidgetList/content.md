# WidgetList (/reference/HELIX.Widgets.WidgetList)

# WidgetList

```
public class WidgetList : IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable, IWidgetListCandidate
```

## widgets

```
public readonly List<Widget> widgets
```

## WidgetList(int)

```
public WidgetList(int capacity = 1)
```

## WidgetList(List<Widget>)

```
public WidgetList(List<Widget> widgets)
```

## GetEnumerator()

```
public IEnumerator<Widget> GetEnumerator()
```

## Count

```
public int Count { get; }
```

## this[int]

```
public Widget this[int index] { get; }
```

## Add(IWidgetListCandidate)

```
public void Add(IWidgetListCandidate widget)
```

## WidgetList(List<Widget>)

```
public static implicit operator WidgetList(List<Widget> widgets)
```