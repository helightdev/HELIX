# SlidePageTransition (/reference/HELIX.Widgets.Navigation.Transitions.SlidePageTransition)

# SlidePageTransition

```
[UxmlObject]
public class SlidePageTransition : PageTransition
```

## SlidePageTransition()

```
public SlidePageTransition()
```

## SlidePageTransition(Vector2, int, EasingMode, bool)

```
public SlidePageTransition(Vector2 slideDirection, int durationMs = 200, EasingMode easing = EasingMode.Linear, bool invertForPop = false)
```

## SlidePageTransition(Vector2, Vector2, int, EasingMode, bool)

```
public SlidePageTransition(Vector2 positiveSlideDirection, Vector2 negativeSlideDirection, int durationMs = 200, EasingMode easing = EasingMode.Linear, bool invertForPop = false)
```

## Duration

```
[UxmlAttribute]
public int Duration { get; set; }
```

## Easing

```
[UxmlAttribute]
public EasingMode Easing { get; set; }
```

## PositiveSlideDirection

```
[UxmlAttribute]
public Vector2 PositiveSlideDirection { get; set; }
```

## NegativeSlideDirection

```
[UxmlAttribute]
public Vector2 NegativeSlideDirection { get; set; }
```

## SlideDirection

```
public Vector2 SlideDirection { set; }
```

## InvertForPop

```
[UxmlAttribute]
public bool InvertForPop { get; set; }
```

## Start(PageTransitionContext)

```
public override void Start(PageTransitionContext context)
```

## Finish(PageTransitionContext)

```
public override void Finish(PageTransitionContext context)
```