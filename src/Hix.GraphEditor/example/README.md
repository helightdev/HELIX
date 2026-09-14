# Example assets

Run from the repository root:

```sh
./gradlew runHalGraphEditor
```

Open `assets/scene.hal` for the small introductory graph, or `assets/adventure.hal`
for a quest with twelve positioned, section-backed nodes.

The adventure demonstrates:

- **Tagged unions:** `QuestObjective` accepts talk, collect, or explore objectives;
  `QuestReward` accepts currency or equipment. Their list sections contain mixed
  variants, connected by references. HAL supplies each variant's `$type` tag.
- **Enums:** difficulty, quest state, faction, item rarity, and reward currency.
  Enter their string values in the inspector, such as `hard`, `rangers`, or `rare`.
- **Constraints:** item counts and quantities must be at least one; currency amounts
  must be nonnegative. Try `count = 0` to see a diagnostic, then Undo.
- **Defaults:** omitted state, currency, quantity, and optional-objective flags use
  the defaults declared in `adventure.hix`.
- **Presence and nullability:** `subtitle = null` is explicitly nullable;
  `designerNote` is an omitted optional field that can be added through its inspector.
- **Collection shapes:** union-element lists, a string list, and a fixed two-number
  tuple for the level range.
- **Shared suppliers:** the warden supplies both the quest giver and talk objective;
  the watchtower supplies the quest destination, NPC home, and explore objective.
- **Saved layout:** every section includes `graphPosition` metadata.

Try dragging backwards from the objectives list's `+` port onto empty space.
The supplier menu offers objective variants. Drag an NPC output onto empty space
to create a compatible receiver, such as a talk objective's `speaker` field.
New sections may remain incomplete while you connect them; diagnostics identify
required fields that still need values.

`assets/external-assets.hal` demonstrates foreign references. `asset(path("adventure.hal"))`
loads the primary section, even though it is named `quest`. A second argument selects
another section: `asset(path("adventure.hal"), "warden")`.
`asset(uid("example.watchtower"))` resolves `shared-location.hal` through its primary
`%uid("example.watchtower")` metadata and survives moving that file within the workspace.
`ref("note")` still refers only to a section in the current file.
