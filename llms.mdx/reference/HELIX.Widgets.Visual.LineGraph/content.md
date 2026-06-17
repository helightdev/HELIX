# LineGraph (/reference/HELIX.Widgets.Visual.LineGraph)

# LineGraph

```
[UxmlElement]
public class LineGraph : PaintingElement, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IElement
```

## datasource

```
public ILineGraphDataSource datasource
```

## oversampleFactor

```
[UxmlAttribute]
public float oversampleFactor
```

## smoothLines

```
[UxmlAttribute]
public bool smoothLines
```

## LineGraph()

```
public LineGraph()
```

## LineDrawer

```
[UxmlObjectReference("stroke")]
public ScriptablePathDrawer LineDrawer { get; set; }
```

## FillDrawer

```
[UxmlObjectReference("fill")]
public ScriptablePathDrawer FillDrawer { get; set; }
```

## Datasource

```
public ILineGraphDataSource Datasource { get; set; }
```

## Refresh()

```
public void Refresh()
```

## Paint(PaintCanvas, Rect)

```
public override void Paint(PaintCanvas canvas, Rect bounds)
```