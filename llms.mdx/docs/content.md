# About HELIX (/docs)



HELIX Widgets is a **declarative widget framework** built on top of Unity UI Toolkit. Instead of
manipulating `VisualElement` trees directly, you describe your interface as a tree of
<FREntitySymbolLink uid="Widget" /> objects. HELIX creates the underlying
`VisualElement` tree, reconciles it efficiently on rebuild, and keeps modifiers, theme values, and
state in sync automatically.

## Where to start [#where-to-start]

<Cards>
  <Card title="Getting Started" description="Install HELIX and mount your first widget tree in a Unity UI Toolkit panel." href="/docs/getting-started" />

  <Card title="Widget Catalog" description="Browse layout, text, input, scrolling, navigation, and visual widgets." href="/docs/widgets" />

  <Card title="State & Reconciliation" description="How widgets update, preserve state across rebuilds, and track signal dependencies." href="/docs/state-and-reconciliation" />

  <Card title="Theming & Modifiers" description="Apply theme tokens, substance layers, and fluent modifier helpers." href="/docs/theming-and-modifiers" />

  <Card title="Navigation, Scrolling & Prompts" description="Page stacks, scroll controllers, virtualized lists, and input prompt glyphs." href="/docs/navigation-scrolling-prompts" />
</Cards>

***

## Mental model [#mental-model]

HELIX cleanly separates *configuration* from *runtime*:

| Concept                                     | Role                                                                                              |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| <FREntitySymbolLink uid="Widget" />         | The immutable-ish configuration object you construct in C#. Think of it as a blueprint.           |
| <FREntitySymbolLink uid="IWidgetElement" /> | The mounted runtime element that owns a Unity `VisualElement`. Created once and updated in place. |
| <FREntitySymbolLink uid="Reconciler" />     | Compares old and new widget trees and decides what to reuse, update, or recreate.                 |
| <FREntitySymbolLink uid="BuildContext" />   | Passed to every `Build` call. Gives access to the mounted element, parent context, and theme.     |
| <FREntitySymbolLink uid="Modifier" />       | Small, composable style and behavior deltas applied to the underlying `VisualElement`.            |

The mental model is intentionally similar to **Flutter-style UI**:

* **Stateless widgets:** subclasses of <FREntitySymbolLink uid="StatelessWidget-1" /> that rebuild from constructor inputs and context alone.
* **Stateful widgets:** subclasses of <FREntitySymbolLink uid="StatefulWidget-1" /> that own local state and expose a lifecycle (<FREntitySymbolLink uid="State-1.InitState" />, <FREntitySymbolLink uid="State-1.Build" />, <FREntitySymbolLink uid="State-1.DidUpdateWidget" />, and <FREntitySymbolLink uid="State-1.Dispose" />).
* **Keys:** preserve mounted element identity when children are reordered or inserted.
* **Theme providers:** flow values down the tree, resolved at build time through <FREntitySymbolLink uid="BuildContext" />.

***

## A complete minimal example [#a-complete-minimal-example]

The example below creates a counter label with a button. It covers stateful widgets, `SetState`,
the collection initializer syntax, and modifiers all in one place.

```csharp
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Universal;

// The widget is a lightweight configuration object. Its sole job is to produce a State.
public class CounterLabel : StatefulWidget<CounterLabel> {
  public override State<CounterLabel> CreateState() => new CounterLabelState();
}

// The state owns mutable data and implements Build.
public class CounterLabelState : State<CounterLabel> {
  private int _count;

  public override Widget Build(BuildContext context) {
    return new HColumn(gap: 8, crossAxisAlign: Align.FlexStart) {
      new HText($"Count: {_count}").Heading(context),
      new HButton(onClick: SetState(() => _count++)) {
        new HText("Increment")
      }
    }.Padding(16);
  }
}
```

### Mounting the widget [#mounting-the-widget]

Every HELIX widget tree is rooted in a <FREntitySymbolLink uid="WidgetHostElement" />.
Subclass it as a `[UxmlElement]` so you can drop it into UI Builder or UXML like any other element.

```csharp
[UxmlElement]
public partial class MyWidgetRoot : WidgetHostElement {
  public MyWidgetRoot() {
    // ToBuildable() wraps the widget in the IBuildable interface the host expects.
    Buildable = new CounterLabel().ToBuildable();
  }
}
```

Once the element attaches to a panel, HELIX schedules the first build automatically. After that, calls to `SetState()` trigger reconciled rebuilds where only the parts of the `VisualElement` tree that actually changed get touched.

***

## Key API reference [#key-api-reference]

The generated C# reference is available under `/reference`. These are the most useful anchor
points when reading the conceptual docs:

<Cards>
  <FREntityCardLink uid="Widget" />

  <FREntityCardLink uid="WidgetHostElement" />

  <FREntityCardLink uid="HButton" />

  <FREntityCardLink uid="HThemeProvider" />
</Cards>
