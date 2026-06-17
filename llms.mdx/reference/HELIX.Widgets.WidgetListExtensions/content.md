# WidgetListExtensions (/reference/HELIX.Widgets.WidgetListExtensions)

# WidgetListExtensions

```
public static class WidgetListExtensions
```

## If(IWidgetListCandidate, bool)

```
public static ConditionalCandidate If(this IWidgetListCandidate candidate, bool condition)
```

Conditionally adds the given candidate to the widget list.

## Spread(IEnumerable<IWidgetListCandidate>)

```
public static SpreadCandidate Spread(this IEnumerable<IWidgetListCandidate> candidates)
```

Spreads the given enumerable of candidates into the widget list.

## Spread(IEnumerable<IWidgetListCandidate>, BuildFunction)

```
public static SpreadCandidate Spread(this IEnumerable<IWidgetListCandidate> candidates, BuildFunction gap)
```

Spreads the given enumerable of candidates into the widget list, with the given gap between each.