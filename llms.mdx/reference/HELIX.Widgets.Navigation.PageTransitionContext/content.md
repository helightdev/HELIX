# PageTransitionContext (/reference/HELIX.Widgets.Navigation.PageTransitionContext)

# PageTransitionContext

```
public class PageTransitionContext
```

## handles

```
public readonly List<PageTransitionHandle> handles
```

## modificationResult

```
public readonly NavStackModificationResult modificationResult
```

## stack

```
public readonly NavStackElement stack
```

## transition

```
public readonly PageTransition transition
```

## PageTransitionContext(PageTransition, NavStackElement, NavStackModificationResult)

```
public PageTransitionContext(PageTransition transition, NavStackElement stack, NavStackModificationResult modificationResult)
```

## ScheduledTransition(Func<IVisualElementScheduler, IVisualElementScheduledItem>)

```
public void ScheduledTransition(Func<IVisualElementScheduler, IVisualElementScheduledItem> func)
```