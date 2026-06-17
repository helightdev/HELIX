# ModePageTransition (/reference/HELIX.Widgets.Navigation.Transitions.ModePageTransition)

# ModePageTransition

```
[UxmlObject]
public class ModePageTransition : PageTransition
```

## DefaultTransition

```
[UxmlObjectReference("Default")]
public PageTransition DefaultTransition { get; set; }
```

## PushTransition

```
[UxmlObjectReference("Push")]
public PageTransition PushTransition { get; set; }
```

## PopTransition

```
[UxmlObjectReference("Pop")]
public PageTransition PopTransition { get; set; }
```

## ReplaceTransition

```
[UxmlObjectReference("Replace")]
public PageTransition ReplaceTransition { get; set; }
```

## Start(PageTransitionContext)

```
public override void Start(PageTransitionContext context)
```

## Finish(PageTransitionContext)

```
public override void Finish(PageTransitionContext context)
```