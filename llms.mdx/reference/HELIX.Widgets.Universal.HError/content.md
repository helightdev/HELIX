# HError (/reference/HELIX.Widgets.Universal.HError)

# HError

```
public class HError : StatelessWidget<HError>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatelessWidget, IBuildable
```

## exception

```
public readonly Exception exception
```

## message

```
public readonly string message
```

## HError(string, Exception, Key, object[], IReadOnlyCollection<Modifier>)

```
public HError(string message, Exception exception = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## ReportIn(BuildContext)

```
public HError ReportIn(BuildContext context)
```

## Build(BuildContext)

```
public override Widget Build(BuildContext context)
```

Builds the widget in the given context.