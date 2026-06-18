# TextTreeRenderer (/reference/HELIX.Diagnostics.Formatting.TextTreeRenderer)

# TextTreeRenderer

```
public sealed class TextTreeRenderer
```

## TextTreeRenderer(DiagnosticLevel, int, int, int)

```
public TextTreeRenderer(DiagnosticLevel minLevel = DiagnosticLevel.Debug, int wrapWidth = 100, int wrapWidthProperties = 65, int maxDescendantsTruncatableNode = -1)
```

## Render(DiagnosticsNode, string, string, TextTreeConfiguration)

```
public string Render(DiagnosticsNode node, string prefixLineOne = "", string prefixOtherLines = null, TextTreeConfiguration parentConfiguration = null)
```