# TransformModifier (/reference/HELIX.Widgets.Modifiers.TransformModifier)

# TransformModifier

```
public class TransformModifier : Modifier, IDiagnosticable
```

## None

```
public static readonly TransformModifier None
```

## Identity

```
public static readonly TransformModifier Identity
```

## rotate

```
public readonly StyleRotate rotate
```

## scale

```
public readonly StyleScale scale
```

## translate

```
public readonly StyleTranslate translate
```

## TransformModifier(StyleTranslate, StyleRotate, StyleScale)

```
public TransformModifier(StyleTranslate translate, StyleRotate rotate, StyleScale scale)
```

## TransformModifier()

```
public TransformModifier()
```

## Apply(VisualElement)

```
public override void Apply(VisualElement element)
```

## Reset(VisualElement)

```
public override void Reset(VisualElement element)
```

## FillModifierProperties(DiagnosticPropertiesBuilder)

```
public override void FillModifierProperties(DiagnosticPropertiesBuilder properties)
```

## FindConstantName()

```
protected override string FindConstantName()
```

## Scale(StyleScale)

```
public static TransformModifier Scale(StyleScale scale)
```

## Rotate(StyleRotate)

```
public static TransformModifier Rotate(StyleRotate rotate)
```

## Translate(StyleTranslate)

```
public static TransformModifier Translate(StyleTranslate translate)
```

## Of(StyleTranslate?, StyleRotate?, StyleScale?)

```
public static TransformModifier Of(StyleTranslate? translate = null, StyleRotate? rotate = null, StyleScale? scale = null)
```