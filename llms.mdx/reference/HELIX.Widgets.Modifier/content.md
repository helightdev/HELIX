# Modifier (/reference/HELIX.Widgets.Modifier)

# Modifier

```
public abstract class Modifier : DiagnosticableBase, IDiagnosticable
```

<p>Modifiers provide a way to manipulate the underlying <a data-furef-uid="UnityEngine.UIElements.VisualElement">VisualElement</a> of a widget.</p>

<p>
Modifiers may alter appearance, layout, or other properties of the element.
They can also be used to register even callbacks, but are expected to behave mostly immutably.
</p>
<p>
When applying modifiers, deltas are computed and passed to the modifier using the
<a data-furef-uid="HELIX.Widgets.Modifier.Apply(HELIX.Widgets.Modifier%2cUnityEngine.UIElements.VisualElement)">Modifier.Apply</a> method. Once a modifier is not used anymore,
it is expected to call <a data-furef-uid="HELIX.Widgets.Modifier.Reset(UnityEngine.UIElements.VisualElement)">Modifier.Reset</a> to reset any alterations it may have made to
the underlying <a data-furef-uid="UnityEngine.UIElements.VisualElement">VisualElement</a>.
</p>
<p>
All modifiers of the same type must be equal to each other, changes are tracked using
<a data-furef-uid="HELIX.Widgets.Modifier.DeepEquals(HELIX.Widgets.Modifier)">Modifier.DeepEquals</a> and the respective <a data-furef-uid="HELIX.Widgets.Modifier.HasChanged(HELIX.Widgets.Modifier)">Modifier.HasChanged</a>
implementations.
</p>
<p>Mustn't expect any ordering guarantees as <a data-furef-uid="HELIX.Widgets.ModifierSet">ModifierSet</a> uses a hashset internally.</p>

## isFallback

```
public bool isFallback
```

## Apply(VisualElement)

```
public virtual void Apply(VisualElement element)
```

## Reset(VisualElement)

```
public virtual void Reset(VisualElement element)
```

## Apply(Modifier, VisualElement)

```
public virtual void Apply(Modifier prev, VisualElement element)
```

## HasChanged(Modifier)

```
public virtual bool HasChanged(Modifier previous)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## DeepEquals(Modifier)

```
public bool DeepEquals(Modifier other)
```

## GetHashCode()

```
public override int GetHashCode()
```

## ApplyDelta(Widget, Widget, VisualElement)

```
public static void ApplyDelta(Widget previous, Widget next, VisualElement element)
```

## ApplyDelta(ModifierSet, ModifierSet, VisualElement)

```
public static void ApplyDelta(ModifierSet previous, ModifierSet current, VisualElement element)
```

## ToStringShort()

```
public override string ToStringShort()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## FillModifierProperties(DiagnosticPropertiesBuilder)

```
public virtual void FillModifierProperties(DiagnosticPropertiesBuilder properties)
```

## FindConstantName()

```
protected virtual string FindConstantName()
```

## Append(HashSet<Modifier>, Modifier)

```
public static void Append(HashSet<Modifier> modifiers, Modifier modifier)
```

## Modifier(BoxConstraints)

```
public static implicit operator Modifier(BoxConstraints constraints)
```

## Modifier(BackgroundStyle)

```
public static implicit operator Modifier(BackgroundStyle style)
```

## Modifier(TextStyle)

```
public static implicit operator Modifier(TextStyle style)
```