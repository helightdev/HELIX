# KennyResourcePromptLayerProvider (/reference/HELIX.Widgets.Prompts.Kenny.KennyResourcePromptLayerProvider)

# KennyResourcePromptLayerProvider

```
public class KennyResourcePromptLayerProvider : SvgResourcePromptLayerProvider, IPromptLayerProvider
```

## Variant

```
public KennyVariant Variant { get; set; }
```

## Variants

```
public Dictionary<string, KennyVariant> Variants { get; }
```

## KennyResourcePromptLayerProvider(string, Dictionary<string, string>)

```
public KennyResourcePromptLayerProvider(string collectionPath, Dictionary<string, string> mapping)
```

## TryGetBestMatch(string, out string)

```
public bool TryGetBestMatch(string bindingPath, out string resourcePath)
```

## TryResolvePromptLayer(BuildContext, string, out SubstanceLayers)

```
public override bool TryResolvePromptLayer(BuildContext context, string bindingPath, out SubstanceLayers layers)
```