# Prose cleanup proposal

How the two layers stand today, where the hardcoding actually is, and the
smallest set of changes that makes the field, overlay and option paths
reusable.

## 1. What exists

### Compose (`Plugins/HELIX.Compose`, ~23k LOC)

Retained-mode UI over UIElements. The pieces that matter here:

- `Composable` = `delegate void(ref Composition cx)`. A composition is a tree
  of these, walked each recompose against a boundary that keeps the elements.
- **Specs** (`Specs/Specs.cs`) are the extension point: a `struct ISpec` is
  handed to `cx.Spec(in spec)`, and `SpecConfig` resolves a
  `ReadComposable<T>` for it from a context-scoped handler list. Replaceable
  per subtree, no hardcoding. This is the good part.
- **Controls** (`Specs/ControlSpec.cs`): `ControlSpec<T>` carries value +
  `IDatatype<T>` + change/commit callbacks, and `SpecConfig.Default`
  registers seven handlers (collection, composite, choice, text, int, float,
  bool) that pick the widget. **The per-type dispatch already lives here.**
- **Forms** (`Forms/`): `FormController` owns values keyed by `FormPath`;
  `HXFormField` is a boundary that registers a path and exposes
  `Lookup<HXFormField>()` to controls below it. Clean separation.
- **Overlays** (`Components/Overlay.cs`): `OverlayController` +
  `Overlay.Build(...)` fluent builder (anchor, placement, modality, focus,
  dismissal, timeout). Also fine.

### Prose (`Plugins/HELIX/Runtime/Scripts/Prose`, ~3.7k LOC)

A write-only semantic markup protocol: `IProseWriter` with
`Begin/End/Push/Write`, scopes (section, paragraph, list, table, property,
tree), modifiers (text style, level, alignment, …) and `IDatatype<T>` for
value formatting. Sinks: plain text, Markdown, Unity rich text, dictionary.
`ReducingProseWriter<TReduced>` folds frames into one intermediate type and
can delegate whole scopes to a nested writer.

This core is in good shape — generic, no UI knowledge, pooled frames.

### The bridge (`HELIX.Compose/Runtime/Scripts/Prose`, ~1.2k LOC)

`ComposeProseWriter : ReducingProseWriter<Composable>` reduces prose to a
composable tree; `NavTreeProseWriter` adds path scopes and a section tree.
`ComposeProseFieldHandler` delegates `IProseField` scopes to a field reducer.
`ComposeProseFieldFactories.Standard` turns a field into a control.

Baking happens **once** (`OptionsService.BuildOptionPages`), so the baked
closures are the steady state and the "free updates" claim holds here. The
camera-overlay writer (§2.4) is a different sink on purpose and does not bake
to `Composable` at all.

## 2. What is actually wrong

### 2.1 The field factory re-implements dispatch Compose already does

`ComposeProseFieldFactories` (636 lines) is seven near-identical methods —
`Text`, `Integer`, `Float`, `Checkbox`, `Choice`, `Composite`, `Collection` —
each of which:

1. bails on an empty path,
2. builds a `Composable` that looks up `HXFormField` and emits
   `new ControlSpec<T>(value, datatype, SetX, FinishEditing, …)`,
3. calls the shared `Field(...)`.

They differ only in `T` and in which of six identical `SetX` methods they
pass, all of which are `field.SetUserValue(value)`. And they are selected by
a hand-written `if (formatter is …)` chain in `Standard` — the same chain
`SpecConfig.Default`'s handlers already run on the spec.

Two consequences:

- Any field whose type is not one of those seven **silently disappears**
  (`return false` → the field contributes nothing, no diagnostic).
- Choice/composite/collection are forced through `ControlSpec<object>`, which
  needs the three `Untyped*Datatype` adapter classes at the bottom of the file
  (~150 lines) purely to present `IDatatype<object>`. One of them,
  `UntypedIDatatypeChoiceDatatype`, duplicates `UntypedChoiceDatatype` that
  already exists in `ControlSpec.cs`.

The typing is ceremony anyway: every control factory reads
`IControlSpec.Value` as `object` and casts.

### 2.2 The factory hook is the wrong shape, so callers work around it

`ComposeProseFieldFactory` has no layout-slot parameter, but `Standard` grew
one as a trailing optional argument. So `OptionPageFieldFactory.Create`
(HELIX.UI) has to:

- copy `parts` into a new `List`,
- pull label/description/tooltip out by scanning for `ProseFieldPart` kinds,
- scan the copy **backwards** to delete the label part it just read,
- append a replacement label part,
- then call `Standard` with its slots.

That list surgery is the clearest symptom. The extension point should accept
decorations and slots as data instead of making callers rewrite the part list.

### 2.3 Hardcoded presentation constants in generic code

- `Field(...)` hardcodes `new Length(35f, LengthUnit.Percent)` as label width,
  duplicating the identical default inside `InspectorLayout`.
- `OptionPageFieldFactory` hardcodes the tooltip strings "Discard the pending
  change" / "Reset to the default value", a `ⓘ` glyph fallback, `-4f`
  anchor offset, `8f` radius, `320f` max width, `0.7f` opacity.
- `HXFormFieldStyle(Passthrough)` is passed to disable decoration because the
  factory has already built the layout itself — the decorator system is
  bypassed rather than used.

### 2.4 Camera overlays: right architecture, hand-rolled contents

`HELIX.UI/CameraOverlay/CameraOverlay.cs` (529 lines) is deliberately *not*
on the recomposition path. Overlays update per frame, which HELIX avoids
generally, so they are dynamic entries in a group driven by a scheduled tick
that writes straight into retained elements, and their `IProseWriter` is a
direct sink rather than a reduction to `Composable`. That is Prose used as
intended — a second sink, not bound to Compose — and the layering should stay.

What is wrong is the contents, not the shape:

- `CameraOverlayBoxElement` sets colors, radii, padding and min-width as raw
  literals, with no `ThemeData` or `ColorRoles` anywhere. Unlike the rest of
  the UI it cannot be restyled, and the values do not need to be read per
  frame — only when the theme changes.
- `OverlayPropertyElement` pre-creates **ten** `TextElement`s
  (`_value`, `_value2`, `_value3`, `_separator1`, `_separator2`, …) so it can
  render "at most three comma-separated components" — a `Vector3` renderer
  generalised one step and no further. A fourth component is silently dropped.
- `OverlayFieldModel<T>.SetText` is a `typeof(T) ==` chain over int / float /
  string / Vector3. The fast paths are the point and should stay, but the
  float branch reaches into `FloatDatatype`'s `Clamp`, `Scale`, `Compact` and
  `Format` fields directly, so only that one concrete datatype gets them.
- `CameraOverlayProseWriter` accepts 7 of the ~18 scopes and `Begin` throws on
  the rest, where the contract says `TryBegin` should refuse and the producer
  skip. It also drops every standard modifier except its own
  `CameraOverlayColor`, and reuses `ProsePropertyDescription` to mean "unit".
- Fields live in a `Dictionary<string, OverlayFieldModel>` keyed by label, so
  property order is hash order, and a field that stops being written is never
  removed from the box.
- `ScreenCameraOverlayController` and `WorldCameraOverlayController` are
  duplicates differing only in how a position is applied.
- `CameraOverlayElement` calls `DynamicCollectionHelper.Synchronize` by hand
  and then re-implements layout with raw `element.style.*` writes per entry
  per tick, instead of using the `DynamicFlexGroupBoundary` /
  `DynamicStackLayout` groups that already exist for this.
- Entries that fail their condition or face away from the camera are hidden
  with `style.display = None`, so they stay in the hierarchy and keep being
  positioned and styled every tick. `DynamicComposableController` already has
  `DetachEntry` / `ReattachEntry` and a detached-element pool for exactly
  this.

So "overlays" means two unrelated things: `OverlayController` (popups, modals,
menus, tooltips — Compose-native, retained, event-driven) and camera overlays
(HUD boxes — dynamic entries, per-frame, direct prose sink). Both are
legitimate; they just should not be confused for each other.

### 2.5 Authoring boilerplate

From `NWSystemExample.Prose.cs` / `Option<T>.ToProse`:

```csharp
using (writer.Field<bool>("audio.voice-chat", "Voice chat", new FlagDatatype(...))) { }
using (writer.Field("graphics.notice", "Display notice", new StringDatatype(...))) {
  writer.Push(ProseFields.FullWidth);
  writer.FieldDescription("Display changes may briefly blank the screen.");
}
```

- Empty `using` bodies for fields with no decoration.
- Modifiers are pushed *after* `Begin`, so every field is a block even when it
  is one line.
- `writer.Path("graphics")` establishes a nav path, and `NavTreeProseWriter`
  already tracks `CurrentPath`, yet field paths repeat the prefix
  (`"graphics.display-mode"` inside the `"graphics"` scope) because
  `writer.Field` ignores it.
- Every consumer repeats the same three-line setup:

```csharp
var fieldReducer = new ComposeProseFieldReducer().Add(OptionPageFieldFactory.Create);
var handlers = new ProseReducerChain<Composable>().Add(new ComposeProseFieldHandler(fieldReducer));
var writer = new NavTreeProseWriter(delegates: handlers);
```

## 3. Proposal

Five changes, independent, roughly in dependency order. Net effect is a
deletion.

### 3.1 Move control construction onto `ProseField<T>`

Add one member to `IProseField`:

```csharp
public interface IProseField : IProseScope {
  string Path { get; }
  string Name { get; }
  object Formatter { get; }
  Composable CreateControl();      // new
  bool TryGetDefaultValue(out object value);
}
```

and implement it **once**, generically, in `ProseField<T>`:

```csharp
public Composable CreateControl() => (ref Composition cx) => {
  var form = cx.Lookup<HXFormField>();
  cx.Spec(new ControlSpec<T>(
    form.GetValue<T>(), Datatype, SetValue, Commit,
    form.FieldData?.HasFlag(FieldFlags.Disabled) != true,
    form.FieldData?.HasFlag(FieldFlags.Error) == true));
  cx.CURSOR.Flexible();
};
```

`SpecConfig` then picks the widget, as it already does everywhere else.

**Deletes:** all seven factory methods, the six `SetX` methods, the
`Standard` dispatch chain, all three `Untyped*Datatype` classes — roughly 450
of the 636 lines of `ComposeProseFieldHandler.cs`.

**Fixes:** unknown field types reach the spec registry (where an app can
register a handler) instead of vanishing; choice/composite/collection keep
their real `T`; adding a field type means registering a spec handler and
nothing else.

Checkbox is the one behavioural difference today — it passes no commit
callback. Keep that by having `ControlSpec` treat a commit on a non-editing
control as a no-op, or by letting `IDatatype` opt out; either way it is one
flag, not a method.

### 3.2 Make the factory hook take data, not a part list

Replace the delegate with a decoration record the handler fills in and a
factory that only *adds* to it:

```csharp
public readonly struct ProseFieldDecoration {
  public Composable Label, Description, Tooltip, Prefix, Suffix, Before, Between, After;
  public InspectorLayoutSlots Slots;
  public Length? LabelWidth;
  public bool FullWidth, HideName;
}

public delegate bool ComposeProseFieldFactory(
  IProseField field, ref ProseFieldDecoration decoration, out Composable result);
```

The reducer does the part-kind → field mapping and the modifier scan once
(it already has both loops), so `OptionPageFieldFactory` becomes: read
`decoration.Description`/`Tooltip`, overwrite `decoration.Label` with the
tooltip-wrapped version, set `decoration.Slots.nameEnd` to the reset buttons,
return the wrapped field. No list copying, no backwards deletion.

Default-field assembly (`Field(...)`) stays in the bridge as one method and
takes the decoration struct.

### 3.3 Stop hardcoding presentation

- Drop the `35%` literal from the bridge; pass `LabelWidth` through as
  `Length?` and let `InspectorLayout` apply its own default.
- Promote the option-page strings and the tooltip geometry into
  `OptionPagesOptions` (which already carries icons and the
  `showTooltipOnLabelHover` flags, so the slot exists).
- Make the tooltip surface read `ThemeProperties` instead of literal radius
  and width.
- Use `HXFormFieldStyle`'s decorators for prefix/suffix/description rather
  than `Passthrough` + a hand-built layout, so a form can restyle
  prose-authored fields the same way it restyles hand-written ones.

### 3.4 Clean up camera overlays without moving them onto Compose

Keep the dynamic-entry group, the scheduled tick and the direct prose sink.
Fix what is hardcoded inside them.

- **Theme the box through data, not recomposition.** Give the controller a
  `ThemeData` reference and resolve colors/radii/padding into a plain
  `CameraOverlayBoxStyle` struct once, re-resolved only when the theme signal
  fires. The box element applies that struct; it never touches `Composition`.
  This is what makes it restyleable without putting it on the recompose path.
- **Make the property row component-count driven.** Replace the ten fixed
  `TextElement`s with a grown-on-demand list of value elements plus
  separators, and drive the count from `ICompositeDatatype.ComponentCount` —
  which already describes exactly what `_value/_value2/_value3` open-codes
  for `Vector3`. Growth happens once per entry, so the steady state is still
  allocation-free, and four-component values stop being truncated.
- **Keep the `typeof(T)` fast paths, move their knobs onto the datatype.**
  Clamp / scale / compact / format belong behind an interface
  (`IDatatypeNumberFormat` or similar) so any float datatype gets them, not
  just `FloatDatatype`. The `typeof(T) == typeof(int)` style branches stay —
  avoiding the formatting allocation is the whole point of this path.
- **Order and remove fields.** Replace `Dictionary<string, …>` with an ordered
  list plus a write-generation stamp: fields render in authoring order, and
  one that stops being written in a tick is dropped instead of lingering.
- **Collapse the two controllers.** One generic
  `CameraOverlayController<TLayout>` with the position application supplied
  per entry; `Screen` and `World` differ only there.
- **Use the groups that exist.** Compose the entries through
  `DynamicFlexGroupBoundary` / the stack group rather than hand-calling
  `DynamicCollectionHelper.Synchronize` and writing `element.style.*` per
  entry per tick. World-space placement still needs the per-tick camera
  projection, but that becomes a position write on the entry's layout, not a
  reimplementation of the group.
- **Detach instead of hiding.** Entries failing their condition or behind the
  camera should `DetachEntry`, so they leave the hierarchy and stop costing
  per-tick positioning and styling. The controller already pools detached
  elements, so reattaching is cheap.
- **Fix the writer contract.** `TryBegin` returns false for scopes the sink
  does not model, so producers skip them, instead of `Begin` throwing. Stop
  overloading `ProsePropertyDescription` as "unit" — if a unit is wanted as
  prose, it is a modifier, and `IDatatypeUnit` already carries it otherwise.

Explicitly **not** proposed: expressing the box as an `ISpec`, reading
`ThemeData` through `cx`, or reducing overlay prose to `Composable`. All three
would drag a per-frame surface onto the recomposition path, which is the thing
this subsystem exists to avoid.

### 3.5 Trim the authoring surface

- Optional arguments on `Field`, so a decoration-free field is one line:
  ```csharp
  writer.Field("voice-chat", "Voice chat", new FlagDatatype(...),
               description: "Enable push-to-talk.", fullWidth: true);
  ```
  The `using` block stays for fields that need nested prose.
- Resolve field paths against `NavTreeProseWriter.CurrentPath` so
  `writer.Path("graphics")` actually prefixes, and the example's
  `"graphics.display-mode"` becomes `"display-mode"`. `FormContext.WithPrefix`
  already does the equivalent on the Compose side.
- One factory for the standard setup:
  ```csharp
  var writer = NavTreeProseWriter.ForFields(OptionPageFieldFactory.Create);
  ```
  replacing the three-line reducer/chain/writer dance in both call sites.
- `ComposeProseFieldHandler.TryCreate` allocates a fresh
  `ReducingProseWriter` per field scope. Pool it — the handler is already the
  natural owner, and `ProseFrameStack` shows the intended pattern.

## 4. Order and blast radius

Only three files outside the bridge consume these APIs:
`OptionPageField.cs`, `OptionsService.cs`, `NWSystemExample.Prose.cs`.

| Step | Touches | Deletes |
|---|---|---|
| 3.1 control construction | `ComposeProseFieldHandler.cs`, `ProseFields.cs` | ~450 lines |
| 3.2 decoration struct | bridge + `OptionPageField.cs` | ~60 lines |
| 3.3 presentation | `OptionPageField.cs`, `OptionPages.cs` | — |
| 3.5 authoring | bridge + 3 call sites | ~30 lines |
| 3.4 camera overlays | `CameraOverlay.cs` | ~120 lines |

3.1 and 3.2 are the ones that unlock reuse; do them first and together, since
3.2's decoration struct is what 3.1's generic field needs. 3.4 is independent
of all of them and can run in parallel — it shares only `IDatatype`. 3.3 and
3.5 are cosmetic and can trail.

Worth a check after 3.1: a round-trip test that a `ProseField<TEnum>` and a
`ProseField<Vector3>` both reach a control, which is the case that silently
fails today.

## 5. Driving prose UI from weave-generated datatypes

The generated side is newer than the prose side, and the two meet at
`StructureDatatype<T>`. The plumbing is mostly there; what is missing is
**human-facing metadata**, and one real rendering bug.

### 5.1 What already works

- `Shared.BuildDatatype` emits a `StructureDatatype<T>` per
  `[Structure(datatype: true)]` type and per `[Command]` method (the
  `<name>_Args` struct), with a `StructurePropertyDatatype<T, TValue>` per
  property carrying name, typed datatype, ref-based getter/setter, `required`
  and `defaultValue`.
- `StructureDatatype<T>` implements `ICompositeDatatype<T>`, so it already
  satisfies `CompositeControlSpecHandler` and reaches a control without any
  new code.
- `ConfigurableStructureDatatype<T>` + `DerivedProperty.Configuration` give
  derivations a post-construction hook; `[NamedArg]` uses it to push a
  `NamedArgModifier` onto one property's `Modifiers`.
- `StructurePropertyDatatype` and `StructureDatatype` both carry an
  `IList<IProseModifier> Modifiers`, which is the right carrier for
  presentation metadata.

So "a prop struct is already describable to the UI" is true structurally.

### 5.2 Gap: there is no label or description channel at all

`DerivedProperty` has `Name`, `Type`, `Datatype`, `Required`, `Default`,
`Equality`, `Hash` — and nothing human-facing. Downstream:

- `[Prop]`'s only UI-relevant member is `Datatype`.
- `StructurePropertyDatatype` has `FieldName` and no description.
- `ICompositeDatatype.GetComponentName(i)` returns the raw C# identifier, and
  `ComposeComposite` renders it verbatim via `cx.Text(GetComponentName(i))`.

So generated UI can only ever show `maxHealth`, never "Max health", and never
a tooltip. The options path already papers over the first half by hand —
`Option<T>.SetInferredName` runs `Casing.Convert(..., TitleCase)` — which is
exactly the transform that belongs at the generated boundary rather than in
one consumer.

**Weave does not read doc comments anywhere.** No mixin touches
`GetDocumentationCommentXml`, so the `///` text that already documents these
fields is discarded. That is the natural source, and purity is not an
obstacle: the prelude has the symbol, extracts `<summary>` to a `string`, and
the carry record takes it from there — the same shape `[Command]` already uses.

Smallest version: add `Label` and `Description` to `DerivedProperty`, seed
them in the prelude from the doc comment with `[Prop(Label =, Description =)]`
as an explicit override, emit them as constructor arguments on
`StructurePropertyDatatype`, and expose them as
`ICompositeDatatype.GetComponentLabel/Description`. Everything in §3 then has
something to render.

### 5.3 Gap: generated command arguments lose their descriptions

`CommandProperty` has a `Description` and `ShortHelp` renders it, but
`CommandBridge<T>.PropertyBinding` constructs every `CommandProperty<TValue>`
without passing one — the parameter is simply omitted. And `[Command]` only
takes a method-level `description`; there is no attribute or any other channel
for a *parameter* description.

So `--help` on a generated command lists argument names with no text, while a
hand-written `CommandProperties.Int("count", "How many to spawn")` has it.
This is the same missing channel as §5.2 and falls out of fixing it: pass
`property.Description` through in `PropertyBinding`.

### 5.4 Gap: no structure-to-form expansion

A `StructureDatatype<T>` in a form resolves to `ComposeComposite`, which lays
its components out as **one horizontal row** of name + control. That is right
for a `Vector3` and useless for a twelve-field prop struct, and it is the only
thing that happens today.

What is missing is a walk from a structure datatype into nested
`HXFormField`s at sub-paths — which is the actual "auto-generate a form from a
prop struct" feature. Every piece it needs exists:
`FormContext.WithPrefix(segment)`, `FormPath` composition,
`NavTreeProseWriter.Path`, and `ProseField<T>` per component. It is a
producer that walks `ComponentCount` and emits `writer.Field(name, label,
componentDatatype)` inside a path scope, recursing when a component is itself
composite. Roughly one file, and it is the thing that makes options
auto-generation possible rather than the structure datatype itself.

Note this wants §3.1 first: today a nested component of an unsupported type
would silently vanish.

### 5.5 Gap: no options generation

`[RegisterOption]` registers a hand-written `Option<T>` field — explicit
datatype, explicit default, `SetInferredPath` / `SetInferredName` for the
rest. Nothing turns a `[Structure(datatype: true)]` type into a set of
options, even though the structure datatype has every property's type,
datatype, default and required flag.

Once §5.4 exists, this is a thin mixin: a `[Structure]` carrying an options
attribute emits one registration that walks the structure datatype and
produces `Option<T>` instances per property, pathed by the struct's option
root. The missing inputs are labels and descriptions (§5.2) and grouping
(§5.6) — not the walk.

### 5.6 Gap: no hidden / order / grouping metadata

A generated form cannot omit an internal field or sort its properties.
`ProseLevel`, `ProseHiddenModifier` and the `Modifiers` lists are all in
place, so the carrier exists; nothing populates it. `Shared.DatatypeProperty`
never passes `modifiers:` at all — every modifier today arrives through the
mutable `Configuration` post-hoc path, which can only *add*, has no ordering,
and runs lazily on first `Datatype` access.

Emitting `modifiers:` directly at construction for statically-known
presentation (hidden, level, group) would make it declarative and leave
`Configuration` for the genuinely dynamic cases.

### 5.7 Bug: a generated structure cannot be written to a camera overlay

`StructureDatatype<T>.ToProse` opens `writer.Tree()`.
`CameraOverlayProseWriter.TryBegin` does not model `ProseTree`, so it returns
false and `Begin` throws `NotSupportedException` — a structure written as a
scope into an overlay fails outright.

Written as a *value* it does not throw, but it renders wrong. The overlay
stores the value in an `OverlayFieldModel<T>`, and `SetText` falls through to
`datatype.ToProse(_fallbackWriter, value)` with `TextElementProseWriter`,
whose `Write(string)` is `Element.SetText(...)` — an overwrite. The structure
writes each property in turn, so **only the last property survives**. One
`Vector3` works solely because `SetText` special-cases it before reaching the
fallback.

Two fixes, both small and both already implied by §3.4: model `ProseTree` in
the overlay writer, and make the property row component-count driven off
`ICompositeDatatype` so a composite value renders its components instead of
being flattened into one text element.

### 5.8 Datatype refinement is a source string

`[Prop(Datatype = "new global::HELIX.IntDatatype(min: 0, max: 100)")]` is raw
C# pasted into the generated table. Nothing validates it, no mixin can read
the range back, and a typo surfaces as a generated-code compile error against
a `#line` in the mixin file.

Not urgent, and the escape hatch is worth keeping. But the common cases —
range, step, unit, pattern, read-only — are exactly the `IDatatype*`
capability interfaces, and they would be better as typed `[Prop]` members
(`Min`, `Max`, `Step`, `Unit`) that the mixin composes into the right
datatype, with the string left as the override.

### 5.9 Suggested order

| # | Change | Unlocks |
|---|---|---|
| 1 | `Label`/`Description` on `DerivedProperty`, from doc comments + `[Prop]` overrides, emitted onto `StructurePropertyDatatype` | §5.3, §5.4, §5.5, and anything readable in §3 |
| 2 | Pass `Description` through `CommandBridge.PropertyBinding` | generated command help |
| 3 | Structure → nested `HXFormField` producer | auto-generated forms |
| 4 | `ProseTree` + composite property rows in the overlay writer | generated structures in overlays |
| 5 | Options generation from a structure datatype | auto-generated options |
| 6 | `modifiers:` at construction; typed `[Prop]` range/unit members | hidden/order/group, validated refinement |

1 is the keystone — 3, 4 and 5 are each roughly a file, and all three are
currently blocked on having something to label things with.

## 6. What was implemented

Five commits on `composable-rework`. Verified with Unity's own compiler
(clean compile, 152/152 EditMode tests) plus 82 weave unit tests; the stale
`HELIX.Widgets.csproj` still references the deleted Hix generator, which is
unrelated and predates this work.

| § | Change | Status |
|---|---|---|
| 3.1 | Control construction on `ProseField<T>`, dispatch via `SpecConfig` | done — 636 → 270 lines |
| 3.2 | `ProseFieldDecoration` by ref instead of a part list | done |
| 3.3 | Label width, reset tooltips, tooltip glyph/width, theme radius and spacing | done, except the decorator point below |
| 3.5 | Optional `Field` arguments, `WriteField`, `ForFields`, pooled field writer | done, except path prefixing below |
| 3.4 | Theming, component rows, ordered fields, writer contract, shared controller base | done, except the two points below |
| 5.2 | `Label`/`Description` from doc comments, through to `StructurePropertyDatatype` | done |
| 5.3 | Generated command arguments keep their descriptions | done |
| 5.4 | `StructureProse`: structure → nested `HXFormField`s | done |
| 5.5 | `StructureOption<T>` | done |
| 5.6 | `modifiers:` at construction; hidden/order/group | not done |
| 5.8 | Typed `[Prop]` range/unit members | not done |

### Deliberately not done

- **§3.3, decoration through `HXDecorator`.** The field row already carries
  label, description and affixes, so handing the same decorators to the
  decorator layout renders them twice. Doing this properly means a
  `DecoratorLayout` built on `InspectorLayout` and is a change to
  `HXDecorator`, not to the bridge. `Passthrough` stays, with a comment
  saying why.
- **§3.5, resolving field paths against the nav path.** The example looked
  redundant, but `OptionsService` derives `groupPath` *from* the option path
  by splitting it, so absolute field paths are the design and prefixing would
  be a second, conflicting convention.
- **§3.4, composing entries through `DynamicFlexGroupBoundary`.** World
  overlays need a per-tick camera projection written onto each entry, which
  is what the hand-rolled `Synchronize` plus style writes are for.
  `DynamicCollectionHelper.Synchronize` is the public API for exactly this,
  so this is using the lower-level entry point, not bypassing the system.
- **§3.4, `DetachEntry` instead of `display = None`.** Visibility flips every
  time the camera turns, and detach/reattach churn could cost more than the
  toggle. The actual waste — positioning and styling an entry that was going
  to be hidden — is gone either way: the enabled and condition checks now run
  before any work, not after.
- **§5.6 and §5.8.** Both are refinement rather than capability, and neither
  blocks anything else. They are the obvious next piece.

### Corrections to the estimates above

§3.4 was estimated at about 120 deleted lines. `CameraOverlay.cs` went from
529 to 830: the ten fixed text elements and the `Vector3` special case did
go, but the resolved style struct, the ordered-field bookkeeping and the
accumulating text writer are all new. The deletion was real; it was just
smaller than what replaced it.
