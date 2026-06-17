# ScriptablePainter (/reference/HELIX.Widgets.Visual.ScriptablePainter)

# ScriptablePainter

```
[UxmlObject]
public class ScriptablePainter : ScriptablePaint
```

## ScriptablePainter()

```
public ScriptablePainter()
```

## ScriptablePainter(List<ScriptablePathBuilder>, List<ScriptablePathDrawer>, ScriptablePaint)

```
public ScriptablePainter(List<ScriptablePathBuilder> builders, List<ScriptablePathDrawer> drawers, ScriptablePaint then = null)
```

## ScriptablePainter(ScriptablePathBuilder, ScriptablePathDrawer, ScriptablePaint)

```
public ScriptablePainter(ScriptablePathBuilder builder, ScriptablePathDrawer drawer, ScriptablePaint then = null)
```

## PathBuilders

```
[UxmlObjectReference("builders")]
public List<ScriptablePathBuilder> PathBuilders { get; set; }
```

## PathDrawers

```
[UxmlObjectReference("drawers")]
public List<ScriptablePathDrawer> PathDrawers { get; set; }
```

## Then

```
[UxmlObjectReference("then")]
public ScriptablePaint Then { get; set; }
```

## Draw(PaintCanvas, Rect)

```
public override void Draw(PaintCanvas canvas, Rect bounds)
```