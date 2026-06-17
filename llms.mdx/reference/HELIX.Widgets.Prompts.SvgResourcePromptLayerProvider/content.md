# SvgResourcePromptLayerProvider (/reference/HELIX.Widgets.Prompts.SvgResourcePromptLayerProvider)

# SvgResourcePromptLayerProvider

```
public abstract class SvgResourcePromptLayerProvider : IPromptLayerProvider
```

## Mapping

```
public Dictionary<string, string> Mapping { get; protected set; }
```

## CollectionPath

```
public string CollectionPath { get; protected set; }
```

## Tint

```
public Color Tint { get; set; }
```

## SvgResourcePromptLayerProvider(string)

```
protected SvgResourcePromptLayerProvider(string collectionPath)
```

## TryGetImage(string, out VectorImage)

```
protected bool TryGetImage(string name, out VectorImage image)
```

## TryResolvePromptLayer(BuildContext, string, out SubstanceLayers)

```
public virtual bool TryResolvePromptLayer(BuildContext context, string bindingPath, out SubstanceLayers layers)
```

## BuildSubstanceLayersFromResource(BuildContext, string)

```
public virtual SubstanceLayers BuildSubstanceLayersFromResource(BuildContext context, string resourcePath)
```

## LoadBackgroundFromResource(string)

```
public virtual Background LoadBackgroundFromResource(string resourcePath)
```