# PaintingElement (/reference/HELIX.Widgets.Elements.PaintingElement)

# PaintingElement

```
public abstract class PaintingElement : BaseElement, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IElement
```

## PaintingElement()

```
protected PaintingElement()
```

## OnGenerateVisualContent(MeshGenerationContext)

```
protected virtual void OnGenerateVisualContent(MeshGenerationContext mgc)
```

## RegisterThemeValue<T>(ThemeValue<T>)

```
protected override ThemeValue<T> RegisterThemeValue<T>(ThemeValue<T> themeValue)
```

## Paint(PaintCanvas, Rect)

```
public abstract void Paint(PaintCanvas canvas, Rect bounds)
```