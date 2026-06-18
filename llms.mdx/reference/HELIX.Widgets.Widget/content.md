# Widget (/reference/HELIX.Widgets.Widget)

# Widget

```
public abstract class Widget : DiagnosticableTreeBase, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate
```

Represents the base class for all widgets.
Provides a foundation for defining user interface components, with support
for diagnostics, modifiers, state reconciliation, and defining widget-specific behavior.

## constants

```
public object[] constants
```

<p>An array of constant objects associated with the widget.</p>
<p>
The <code>constants</code> property is used to prevent reconciliation (rebuilding)
when updating the widget tree. If the array of constants in the current widget
matches the array in the previous widget during a reconciliation check,
the system assumes the widgets are equivalent, and no further updates are applied.
</p>
<p>
The exact same widget with the <b>same reference</b> is <b>always considered equivalent</b> and
therefore treated as constant, using a <code>constants</code> array here is not beneficial.
</p>
<p>
If the constants provided in two widgets do not match, the reconciliation system
performs a normal rebuild of the corresponding widget subtree.
</p>

## key

```
public Key key
```

<p>A unique identifier for the widget instance.</p>
<p>
The <code>key</code> property is used to determine whether a widget can be reused
or must be replaced during the reconciliation process. Keys also play a critical
role in preserving widget states when the widget tree is updated.
</p>
<p>
If two widgets with the same parent have keys that are equal, those widgets
are considered positionally equivalent and their states can be reused if their types match
and reconciliation checks are successful.
</p>
<p>
If no key is specified, parent widgets will attempt to match children based primarily on their actual position.
Insertions and removal in the middle of the widget list may lead to unintended rebuilds
and state recreation as possibly valid widgets are being discarded.
</p>
<p>
Keys may also be used to for clarity or persistent tracking of a widget's identity
using a <a data-furef-uid="HELIX.Widgets.GlobalKey">GlobalKey</a> which is guaranteed to be unique across multiple parents and holds a reference
to the underlying <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a>
</p>
<br>
<a data-furef-uid="HELIX.Widgets.Key">Key</a>

## modifiers

```
protected ModifierSet modifiers
```

<p>
An effectively immutable collection of modifications applied to a
widget's underlying <a data-furef-uid="UnityEngine.UIElements.VisualElement">VisualElement</a>.
They can manipulate the element's style, layout, properties, and behavior generically.
</p>
<p>
Modifiers may be added to a widget using the <a data-furef-uid="HELIX.Widgets.Widget.modifiers">Widget.modifiers</a> constructor parameter, the
<a data-furef-uid="HELIX.Widgets.Widget.AddModifier(HELIX.Widgets.Modifier)">Widget.AddModifier</a> or <a data-furef-uid="HELIX.Widgets.Widget.AddModifiers(System.Collections.Generic.IEnumerable%7bHELIX.Widgets.Modifier%7d)">Widget.AddModifiers</a> methods, or by using the <a data-furef-uid="HELIX.Widgets.ModifierExtensions">ModifierExtensions</a>.
</p>

## Widget(Key, object[], IReadOnlyCollection<Modifier>)

```
protected Widget(Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Base constructor for a widget. See <a data-furef-uid="HELIX.Widgets.Widget.key">Widget.key</a> and <a data-furef-uid="HELIX.Widgets.Widget.constants">Widget.constants</a> and <a data-furef-uid="HELIX.Widgets.Widget.modifiers">Widget.modifiers</a>
for more information on the constructor parameters.

## CreateElement()

```
public abstract IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.

## GetModifiers()

```
public ModifierSet GetModifiers()
```

Returns the <a data-furef-uid="HELIX.Widgets.ModifierSet">ModifierSet</a> that is applied to the widget.

## AddModifier(Modifier)

```
public void AddModifier(Modifier modifier)
```

Adds a modifier to the widget.

## AddModifiers(IEnumerable<Modifier>)

```
public void AddModifiers(IEnumerable<Modifier> additions)
```

Adds a sequence of modifiers to the widget.

## DefaultModifiers(ModifierSet, IReadOnlyCollection<Modifier>)

```
protected void DefaultModifiers(ModifierSet defaults, IReadOnlyCollection<Modifier> user)
```

Sets the default modifiers for the widget.

## ToStringShort()

```
public override string ToStringShort()
```

## GetWidgetName()

```
public virtual string GetWidgetName()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## ReconcileInto(IWidgetElement, ReconcileMode)

```
protected IWidgetElement ReconcileInto(IWidgetElement element, ReconcileMode mode = ReconcileMode.AfterParent)
```

Reconciles the widget into the given element once it has enough hierarchy context.

## BuildFunction(Widget)

```
public static implicit operator BuildFunction(Widget widget)
```

Converts the widget into a constant <a data-furef-uid="HELIX.Widgets.BuildFunction">BuildFunction</a>.

## BuildFunction<WidgetState>(Widget)

```
public static implicit operator BuildFunction<WidgetState>(Widget widget)
```

Converts the widget into a constant <a data-furef-uid="HELIX.Widgets.BuildFunction%601">BuildFunction</a> that always
returns the same widget for all widget states.

## Widget(VisualElement)

```
public static implicit operator Widget(VisualElement element)
```