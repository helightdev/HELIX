# ThemeComponent (/reference/HELIX.Widgets.Theming.ThemeComponent)

# ThemeComponent

```
[UxmlObject]
[RequireDerived]
public abstract class ThemeComponent : ICloneable
```

## lookupScope

```
protected IReadOnlyList<ThemeProperty> lookupScope
```

## Clone()

```
public virtual object Clone()
```

## Apply(IdentityDictionary<ThemeProperty, object>, bool)

```
public virtual void Apply(IdentityDictionary<ThemeProperty, object> dict, bool clearExisting = false)
```

## ApplyGlobal(bool)

```
public void ApplyGlobal(bool clearExisting = true)
```