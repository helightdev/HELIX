# HPrompt (/reference/HELIX.Widgets.Prompts.HPrompt)

# HPrompt

```
public class HPrompt : StatefulWidget<HPrompt>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget
```

## HPrompt(InputAction, HPromptStyle, Key, object[], IReadOnlyCollection<Modifier>)

```
public HPrompt(InputAction action, HPromptStyle style = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## HPrompt(string, HPromptStyle, Key, object[], IReadOnlyCollection<Modifier>)

```
public HPrompt(string actionName, HPromptStyle style = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## CreateState()

```
public override State<HPrompt> CreateState()
```