# NavStackElement (/reference/HELIX.Widgets.Navigation.NavStackElement)

# NavStackElement

```
[UxmlElement]
public class NavStackElement : SingleChildWidgetBaseElement<HNavStack>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IReconcileScheduler, IScheduledReconcileRunner, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, ISingleChildContainer
```

## NavStackElement()

```
public NavStackElement()
```

## Pages

```
public IReadOnlyList<NavPageBase> Pages { get; }
```

## CurrentPageBase

```
public NavPageBase CurrentPageBase { get; }
```

## contentContainer

```
public override VisualElement contentContainer { get; }
```

<p>
Logical container where child elements are added.
If a child is added to this element, the child is added to this element's content container instead.
</p>

## DefaultTransition

```
[UxmlObjectReference]
public PageTransition DefaultTransition { get; set; }
```

## OnAttached(AttachToPanelEvent)

```
protected override void OnAttached(AttachToPanelEvent evt)
```

## Pop(PageTransition)

```
public Awaitable<bool> Pop(PageTransition transition = null)
```

## Pop(NavPageBase, PageTransition)

```
public Awaitable<bool> Pop(NavPageBase pageBase, PageTransition transition = null)
```

## PopUntil(Func<NavPageBase, bool>, PageTransition)

```
public Awaitable<bool> PopUntil(Func<NavPageBase, bool> predicate, PageTransition transition = null)
```

## Push(NavPageBase, PageTransition)

```
public Awaitable Push(NavPageBase pageBase, PageTransition transition = null)
```

## PushReplacement(NavPageBase, PageTransition)

```
public Awaitable PushReplacement(NavPageBase pageBase, PageTransition transition = null)
```

## Refresh()

```
public void Refresh()
```

## Apply(HNavStack, HNavStack)

```
public override void Apply(HNavStack previous, HNavStack widget)
```

## Reconcile(Widget)

```
public override bool Reconcile(Widget updated)
```

## Get(VisualElement)

```
public static NavStackElement Get(VisualElement context)
```

## GetNamed(VisualElement, string)

```
public static NavStackElement GetNamed(VisualElement context, string name)
```