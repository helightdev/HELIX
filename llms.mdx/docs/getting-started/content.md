# Getting Started (/docs/getting-started)



HELIX Widgets is designed to live inside Unity UI Toolkit. The main integration boundary is
<FREntitySymbolLink uid="WidgetHostElement" />, which you can
place in UXML, subclass as a custom UXML element, or create from C# and attach to an existing
`VisualElement` tree.

***

## Packages [#packages]

| Package | ID                                   | Description                                                                    |
| ------- | ------------------------------------ | ------------------------------------------------------------------------------ |
| Core    | <FREntitySymbolLink uid="HELIX" />   | Shared UI primitives: colors, layout helpers, style values, painting utilities |
| Widgets | <FREntitySymbolLink uid="Widgets" /> | The widget framework (requires Core)                                           |

Both packages target Unity `6000.4` or later.

### Installing from Git [#installing-from-git]

Add the packages through **Window → Package Manager → + → Add package from Git URL**:

```text
# Core
https://github.com/helightdev/HELIX.git?path=src/HELIX/Assets/Plugins/HELIX

# Widgets
https://github.com/helightdev/HELIX.git?path=src/HELIX/Assets/Plugins/HELIX.Widgets
```

### Installing from the repository [#installing-from-the-repository]

If you are working directly inside this repository, the packages live at:

```text
src/HELIX/Assets/Plugins/HELIX
src/HELIX/Assets/Plugins/HELIX.Widgets
```

***

## Common namespaces [#common-namespaces]

Add these using directives to any file that works with HELIX Widgets:

```csharp
using HELIX.Types;                        // Colors, Axis, Align, Border, and other primitives
using HELIX.Widgets;                      // Widget, StatelessWidget, StatefulWidget, BuildContext
using HELIX.Widgets.Modifiers;            // Modifier types and fluent extension helpers
using HELIX.Widgets.Universal;            // HColumn, HRow, HText, HButton, HTextField, HSlider...
using HELIX.Widgets.Universal.Styles;     // HButtonVariant, HButtonSize, HInputRadius
using HELIX.Widgets.Universal.Theme;      // PrimitiveTheme, PrimitiveBaseTheme
```

***

## Hosting a widget tree [#hosting-a-widget-tree]

The preferred integration point is <FREntitySymbolLink uid="WidgetHostElement" />.
Subclass it and mark it with `[UxmlElement]` to make it available in UI Builder and UXML:

```csharp
using HELIX.Widgets;
using HELIX.Widgets.Universal;
using UnityEngine.UIElements;

[UxmlElement] // [!code focus:9]
public partial class InventoryPanel : WidgetHostElement {
  public InventoryPanel() {
    Buildable = new HColumn(gap: 8) {
      new HText("Inventory"),
      new HButton(child: new HText("Sort"))
    }.ToBuildable();
  }
}
```

You can also create a `WidgetHostElement` from C# and add it to any existing `VisualElement`:

```csharp
var host = new WidgetHostElement {
  Buildable = new InventoryList().ToBuildable()
};
rootVisualElement.Add(host);
```

Once the element attaches to a panel, HELIX schedules the first build and constructs the widget
subtree. After that it reconciles on `SetState` calls or theme changes, only touching the parts
of the `VisualElement` tree that actually changed.

***

## Building with collection initializers [#building-with-collection-initializers]

Container widgets implement `Add`, so HELIX code reads naturally with C# collection initializers:

```csharp
return new HRow(gap: 12) {
  new HText("Health"),
  new HBox(background: MaterialColors.Red).Expand(),
  new HText("82")
};
```

Single-child widgets also support initializer syntax. The first item added becomes the child:

```csharp
new HButton(onClick: Save) {
  new HText("Save")
}
```

### Conditional and projected children [#conditional-and-projected-children]

Use <FREntitySymbolLink uid="WidgetListExtensions" /> helpers to keep the list
readable even with conditional or dynamically projected items:

```csharp
new HColumn {
  new HText("Profile"),
  // [!code word:If]
  new HText("Admin").If(user.IsAdmin),      // included only when the predicate is true
  actions
    .Select(a => new HButton(child: new HText(a.Label)))
  // [!code word:Spread]
    .Spread(new HGap(8))                    // flattens a sequence into the parent list
};
```

***

## Applying modifiers [#applying-modifiers]

Modifiers are the normal way to size, space, clip, or decorate a widget after constructing it.
The fluent methods chain onto any widget and return the same widget for further chaining:

```csharp
new HBox(background: MaterialColors.Blue) {
  new HText("Selected")
}.Padding(12).Margin(8).Clip().Expand();
```

You can also pass an explicit `modifiers` array through the constructor when that reads better:

```csharp
new HColumn(
  modifiers: new ModifierSet { // [!code focus:5]
    PaddingModifier.Of(16),
    MarginModifier.Only(top: 8),
    ClipModifier.Clip
  }
) {
  new HText("Settings")
};
```

<Callout title="Modifiers are applied as deltas" type="tip">
  When a modifier disappears from a rebuild, HELIX automatically resets the styles or behavior that modifier owned.<br />
  See [Theming & Modifiers](/docs/theming-and-modifiers) for the full modifier reference
</Callout>

***

## Reading theme values [#reading-theme-values]

Every `Build` call receives a <FREntitySymbolLink uid="BuildContext" /> that resolves
theme tokens and auto-subscribes to theme changes:

```csharp
public override Widget Build(BuildContext context) {
  var surface = context.GetThemed(PrimitiveTheme.Surface);

  return new HBox(background: surface) {
    new HText("Theme aware").Body(context)
  }.Padding(16);
}
```

`HText` extension methods like `Heading`, `Body`, `Caption`, and `Display` are shorthand for
resolving typography from the active primitive theme and applying it to a label.

For more on providers, token overrides, and the substance system, see
[Theming & Modifiers](/docs/theming-and-modifiers).
