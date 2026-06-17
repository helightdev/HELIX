# Widget Catalog (/docs/widgets)



This page groups the full public widget surface by purpose. For constructor signatures and
parameter details, see the generated API reference under `/reference`. This page focuses on *what
each widget is for* and how they fit together.

***

## Base types for custom widgets [#base-types-for-custom-widgets]

<FREntitySymbolLink uid="Widget" /> is the root base class. Application-level widgets
should inherit from one of these specializations:

| Type                                           | When to use                                                                                  |
| ---------------------------------------------- | -------------------------------------------------------------------------------------------- |
| <FREntitySymbolLink uid="StatelessWidget-1" /> | Output depends only on constructor data and context. No mutable state.                       |
| <FREntitySymbolLink uid="StatefulWidget-1" />  | Owns local state, controllers, subscriptions, or disposables.                                |
| <FREntitySymbolLink uid="HStatefulBuilder" />  | Inline stateful build function that is handy for small examples and signal-driven fragments. |

Use <FREntitySymbolLink uid="WidgetHostElement" /> to insert any widget tree into
ordinary Unity UI Toolkit. It serves as the integration boundary between the two worlds.

***

## Layout [#layout]

### Axes and flow [#axes-and-flow]

<FREntitySymbolLink uid="HColumn" /> and
<FREntitySymbolLink uid="HRow" /> are the two most common layout widgets.
They lay children out vertically and horizontally respectively, and both support `mainAxisAlign`,
`crossAxisAlign`, `gap`, and `reverse`:

```csharp
new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
  new HText("Account settings"),
  new HRow(gap: 8) {
    new HButton(child: new HText("Cancel")),
    new HButton(HButtonVariant.Flat, child: new HText("Save"))
  }
}
```

Use <FREntitySymbolLink uid="HFlex" /> when the axis must be dynamic or
when flex wrap is needed. Use <FREntitySymbolLink uid="HStack" /> when
children should overlap with absolute positioning while still allowing non-positioned children to
flow normally.

### Supporting layout widgets [#supporting-layout-widgets]

| Widget                                  | Purpose                                                     |
| --------------------------------------- | ----------------------------------------------------------- |
| <FREntitySymbolLink uid="HBox" />       | Single child with background, border, radius, and alignment |
| <FREntitySymbolLink uid="HCenter" />    | Centers a child on both axes                                |
| <FREntitySymbolLink uid="HAlign" />     | Positions a child using an explicit `Alignment` value       |
| <FREntitySymbolLink uid="HGap" />       | Fixed-size spacing spacer                                   |
| <FREntitySymbolLink uid="HBoxShadow" /> | Applies box-shadow styling around a child                   |

***

## Text and icons [#text-and-icons]

### HText [#htext]

<FREntitySymbolLink uid="HText" /> wraps a UI Toolkit `Label` and adds
text-specific options: rich text, selectable text, language direction, and explicit <FREntitySymbolLink uid="TextStyle" />
values.

Apply theme-aware typography with extension methods:

```csharp
new HText("Section heading").Heading(context);
new HText("Body copy paragraph.").Body(context);
new HText("Small metadata label").Caption(context);
new HText("Hero display text").Display(context);
```

Each helper resolves the correct <FREntitySymbolLink uid="TextStyle" /> from the active <FREntitySymbolLink uid="PrimitiveTheme" />,
so the size, weight, and color update automatically when the theme changes.

### HIcon [#hicon]

<FREntitySymbolLink uid="HIcon" /> is a `Label` specialized for font-based
icon sets. Pass a Unicode glyph constant and a font definition:

```csharp
new HIcon(FaSolidIcons.MagnifyingGlass, FaSolidIcons.FontDefinition);
new HIcon(FaSolidIcons.FloppyDisk,      FaSolidIcons.FontDefinition);
```

The bundled Font Awesome solid icon constants are available through
<FREntitySymbolLink uid="FaSolidIcons" />.

***

## Buttons [#buttons]

<FREntitySymbolLink uid="HButton" /> is a themed, stateful button that
handles hover, press, focus, and disabled states automatically.

```csharp
new HButton(
  HButtonVariant.Soft,
  onClick: Save,
  child: new HRow(gap: 8) {
    new HIcon(FaSolidIcons.FloppyDisk, FaSolidIcons.FontDefinition),
    new HText("Save")
  }
);
```

Control the appearance with:

* **Variant:** <FREntitySymbolLink uid="HButtonVariant" />. Supported values are: `Flat`, `FlatTwoState`, `Soft`, `SoftTwoState`, `Outline`, `Ghost`, and `TwoState`.
* **Size:** <FREntitySymbolLink uid="HButtonSize" />, which controls padding and minimum height.
* **Radius:** <FREntitySymbolLink uid="HInputRadius" />, which controls corner rounding.

For external control, pass a <FREntitySymbolLink uid="ButtonController" />.
Without one, the button creates and manages one internally.

***

## Text fields [#text-fields]

<FREntitySymbolLink uid="HTextField" /> is the themed text-input widget.

**Uncontrolled:** provide an initial value and react to change or submit callbacks:

```csharp
new HTextField(
  initialValue: playerName,
  onChanged: value => playerName = value,
  onSubmitted: SubmitName
);
```

**Controlled:** bind a <FREntitySymbolLink uid="TextEditingController" /> to read or set the value programmatically.

| Parameter                                            | Effect                                         |
| ---------------------------------------------------- | ---------------------------------------------- |
| <FREntitySymbolLink uid="HTextField.multiline" />    | Enables multi-line input                       |
| <FREntitySymbolLink uid="HTextField.isDelayed" />    | Fires `onChanged` only on submit or focus loss |
| <FREntitySymbolLink uid="HTextField.isReadOnly" />   | Disables editing                               |
| <FREntitySymbolLink uid="HTextField.isPassword" />   | Masks input characters                         |
| <FREntitySymbolLink uid="HTextField.keyboardType" /> | Sets the mobile keyboard type                  |
| <FREntitySymbolLink uid="HTextField.maxLength" />    | Caps character count                           |

***

## Sliders [#sliders]

<FREntitySymbolLink uid="HSlider" /> supports two modes:

**Value slider:** controlled via a <FREntitySymbolLink uid="SliderController" /> or initial-value callbacks:

```csharp
new HSlider(
  axis: Axis.Horizontal,
  initialValue: volume,
  onChanged: value => volume = value
);
```

**Scrollbar:** bind a <FREntitySymbolLink uid="ScrollController" />. HELIX will configure the range and thumb size automatically:

```csharp
new HSlider(_scroll)
```

When created from a scroll controller, the slider uses the scrollbar theme by default.

***

## Scrolling and virtualization [#scrolling-and-virtualization]

### HScrollView [#hscrollview]

<FREntitySymbolLink uid="HScrollView" /> creates a scrollable container.
Use it for forms, settings panels, and small-to-medium dynamic content where all children can be
mounted at once:

```csharp
new HScrollView(Axis.Vertical) {
  new HColumn(gap: 8) {
    rows.Select(row => new HText(row.Title)).Spread()
  }
}.Fill();
```

### HListView (virtualized) [#hlistview-virtualized]

<FREntitySymbolLink uid="HListView" /> is the correct choice for long
lists. It builds row widgets on demand as the visible range changes, keeping the `VisualElement`
count small regardless of how many items there are:

```csharp
new HListView(
  (context, index) => new HBox {
    new HText($"Item {index + 1}")
  }.Padding(12),
  count: 1000,
  fixedItemHeight: 48f
);
```

Set `fixedItemHeight` when all rows are the same height to allow O(1) scroll position calculations.
For variable-height content, use dynamic height mode, which performs extra measurement work in exchange for flexibility.

### HPanView [#hpanview]

<FREntitySymbolLink uid="HPanView" /> supports free two-dimensional
panning of a larger child surface. It is well-suited for map-like views, node editors, canvas
tools, and any content that is not naturally linear.

***

## Navigation [#navigation]

<FREntitySymbolLink uid="HNavStack" /> hosts a stack of pages and handles
push, pop, and replacement with optional transitions. Reach the mounted element through a
<FREntitySymbolLink uid="GlobalKey-1" /> stored as a state field:

```csharp
private readonly GlobalKey<NavStackElement> _nav = new();

new HNavStack(key: _nav).Fill();

_nav.Element.PushReplacement(new WidgetNavPage {
  Buildable = new SettingsPage().ToBuildable()
});
```

Built-in transitions include <FREntitySymbolLink uid="InstantPageTransition" />,
<FREntitySymbolLink uid="FadePageTransition" />, and
<FREntitySymbolLink uid="SlidePageTransition" />.
Implement <FREntitySymbolLink uid="PageTransition" /> to create custom ones.

<FREntitySymbolLink uid="HScaffold" /> is a higher-level page shell that
manages named layout areas such as a body slot, navigation surfaces, and common chrome. Use it when
pages share structural chrome that should not be rebuilt on each navigation.

***

## Prompts and input glyphs [#prompts-and-input-glyphs]

<FREntitySymbolLink uid="HPrompt" /> displays context-sensitive glyphs for
input bindings. Rendering is provider-based, meaning the same prompt widget automatically shows the correct glyph for
keyboard/mouse, Xbox, PlayStation, Nintendo Switch 2, Steam Deck, or Steam Controller, depending on the active device.

Key types:

| Type                                                        | Role                                         |
| ----------------------------------------------------------- | -------------------------------------------- |
| <FREntitySymbolLink uid="IPromptLayerProvider" />           | Defines a source of prompt layer images      |
| <FREntitySymbolLink uid="SvgResourcePromptLayerProvider" /> | Loads SVG prompt layers from Unity Resources |
| <FREntitySymbolLink uid="HelixInputController" />           | Tracks the active input device and variant   |
| <FREntitySymbolLink uid="InputConfiguration" />             | Describes the set of active prompt providers |
| <FREntitySymbolLink uid="GamepadVariant" />                 | Identifies a specific gamepad family         |
| <FREntitySymbolLink uid="InputDeviceType" />                | Keyboard/mouse or gamepad                    |

Prompt widgets are useful for action bars, tutorial hints, rebinding screens, and context-sensitive
control help.

***

## Visual and painting helpers [#visual-and-painting-helpers]

The <FREntitySymbolLink uid="Visual" /> namespace contains lower-level drawing helpers for custom UI visuals:

| Widget / Type                                     | Purpose                                                 |
| ------------------------------------------------- | ------------------------------------------------------- |
| <FREntitySymbolLink uid="PathPainter" />          | Paints vector paths onto a `VisualElement`              |
| <FREntitySymbolLink uid="ScriptablePainter" />    | Scriptable, data-driven custom paint pass               |
| <FREntitySymbolLink uid="SwatchVisualizer" />     | Renders a grid of palette/token swatches for inspection |
| <FREntitySymbolLink uid="MemoryUsageLineGraph" /> | Diagnostic line graph for memory usage data             |
| <FREntitySymbolLink uid="LineGraphStroke" />      | Stroke style for `MemoryUsageLineGraph`                 |
