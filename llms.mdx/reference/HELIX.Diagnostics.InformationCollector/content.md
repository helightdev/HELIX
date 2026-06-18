# InformationCollector (/reference/HELIX.Diagnostics.InformationCollector)

# InformationCollector

```
public sealed class InformationCollector
```

## Add(string)

```
public InformationCollector Add(string message)
```

## Add(DiagnosticsNode)

```
public InformationCollector Add(DiagnosticsNode node)
```

## AddRange(IEnumerable<DiagnosticsNode>)

```
public InformationCollector AddRange(IEnumerable<DiagnosticsNode> nodes)
```

## AddRange(params DiagnosticsNode[])

```
public InformationCollector AddRange(params DiagnosticsNode[] nodes)
```

## AddBuilder(DiagnosticsNodeBuilder)

```
public InformationCollector AddBuilder(DiagnosticsNodeBuilder builder)
```

## Collect()

```
public List<DiagnosticsNode> Collect()
```