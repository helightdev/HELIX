# HAL graph editor

From the repository root:

```sh
./gradlew runHalGraphEditor
# Or choose a different workspace:
./gradlew runHalGraphEditor -PHalGraphRoot=/path/to/workspace -PHalGraphPort=5178
```

Requires .NET 8, Node.js 20.19+ (or 22.12+), npm and the repository's Gradle/JDK setup.
The task builds `Hix.Toolchain`, installs the locked npm dependencies, builds the
React/XYFlow frontend and serves both frontend and API at http://localhost:5178.
The server working directory and file-browser root default to `src/Hix.GraphEditor/example`.
Stop it with Ctrl+C. `BuildConfiguration` selects the .NET build configuration.
`buildHalGraphFrontend` builds just the frontend.

Open `assets/scene.hal` for a small graph, or `assets/adventure.hal` for tagged
unions, enums, constraints, defaults, shared references, and multiple collection
shapes. See [the example guide](example/README.md). Select nodes to edit fields in the inspector.
Values are HAL expressions: strings can be quoted, numbers and booleans entered
literally, and objects/lists entered as `Position {x = 1, y = 2}` or `[]`.
Missing schema fields appear as editable ports, with optional/default indicators.
Drag a node's right-hand output to a field's left-hand input. The `+` list port
appends an item; existing list items have inspector removal buttons. Double-click
an edge to replace the receiving field with `null` (required fields then diagnose).
The palette creates typed sections, plain tables and lists. Tables have an Add field
control. Every graph node is a section with an anchor; wires always persist as
`ref("anchor")`. A list section has a list body:

```hal
--- table &scene
characters = ref(characters)

--- tuple &characters
[ref(hero)]

--- Character &hero
name = "Guide"
```

Opening an asset extracts existing inline tables and lists into sections in the
unsaved session. This conversion is undoable and is written to disk only on Save.
Graph/source edits also extract any newly introduced containers. `manifest.hal`
retains its single-section format. Editing a shared section updates all references
to it; connecting a wire never copies its value.

Drop an input wire onto empty space to choose a compatible **supplier**. Drop an
output wire onto empty space to choose a compatible **receiver field/list**. The
backend filters choices using Hix pattern relations. Choosing an entry creates and
connects its section atomically, at the drop position, as one undo step. Escape or
Cancel dismisses the menu without editing. Primitive list/table sections infer
inspector schemas from their receiving fields.

Required fields can remain unfinished during construction and produce diagnostics.
Connections reject impossible type relationships and cycles, without requiring an
otherwise complete document. The ordinary HAL evaluator reports value/default/
constraint errors. Deleting a referenced section requires disconnecting its wires.

Save writes HAL source with an external-change check and atomic replacement.
Undo/redo applies to source edits, including connections. Unsaved text stays in the
server session; it is not autosaved. The source view permits syntax recovery and edits
not represented by graph controls. Apply source changes before using graph actions.
`Refresh types` reloads workspace type information. Node positions are saved on each section as `%graphPosition(x=120, y=240)` metadata.
Dragging updates the session; Save writes positions for every node, including initial
layout positions. Reopening restores them. Moving a selection is one undo step. A browser reload starts a new UI session.

## Workspace and analysis

The service walks up from the asset's directory to the nearest `manifest.hal`, then
uses `HixProject.MainSources()` (including its resolved dependencies) and standalone
backend definitions. Without a manifest it uses sibling `.hix` files. Workspace
errors and HAL diagnostics are returned with every snapshot. No asset expression
executes arbitrary Hix functions: semantic validation uses HAL's data-only evaluator.
The web server binds localhost and confines browsing/open/save to `--root`; symlink
paths under that root are rejected. It is a local development editor, not a hosted
multi-user service.

## Analysis protocol

```sh
dotnet src/Hix.Toolchain/bin/Debug/net8.0/Hix.Toolchain.dll analysis-server
```

Stdio uses one JSON request/response per line (LSP-like lifecycle, **not** LSP
Content-Length framing). HTTP uses the same request at `POST /api`, with
`Content-Type: application/json`. The transport-neutral dispatcher and sessions live
in `Hix.Standalone.Analysis`, available to other editor adapters.

```json
{"id":1,"method":"hal/open","params":{"path":"/absolute/path/asset.hal"}}
```

Responses contain `id` and `result`, or `error: {code, message}`. Snapshots include
`session`, `path`, `workspace`, `revision`, `dirty`, `text`, `nodes`, `edges`, `types`,
`diagnostics`, `canUndo` and `canRedo`. Node and port IDs must be taken from the latest
snapshot. Every mutation includes `session` and the expected `revision`; stale
requests return `conflict` without changing the document. Revisions increase on
source edits and undo/redo; saving does not change source revision.

| Method | Additional parameters | Result |
|---|---|---|
| `hal/open` | `path` | New session snapshot |
| `hal/get` | `session` | Current snapshot and refreshed types |
| `hal/change` | `text` | Replace complete unsaved text |
| `hal/candidates` | `node`, `port`, `direction: "supplier" / "receiver"` | Compatible creation choices (also requires revision) |
| `hal/edit` | `edit` | Apply graph edit |
| `hal/save` | — | Save unless disk differs from opened/saved content |
| `hal/undo`, `hal/redo` | — | Apply source history |
| `hal/close` | — | Release session, discarding unsaved state |
| `files/list` (HTTP only) | optional `path` | Root, directory and HAL/subdirectory entries |

`edit` operations:

- `{operation:"set", node, port, value:"HAL expression"}` replaces a value or inserts a missing schema field/list item.
- `{operation:"connect", node, port, source:"source node ID"}` connects a node to a receiving field.
- `{operation:"disconnect", node, port}` writes `null`.
- `{operation:"move", node, x, y}` replaces that section's graphPosition metadata.
- `{operation:"layout", positions:[{node,x,y}]}` atomically updates multiple positions.
- `{operation:"addField", node, port:"new field name", value:"HAL expression"}` adds a table field.
- `{operation:"createConnected", node, port, direction, value:"type", receiverPort}` creates and connects a candidate in one revision. Optional `x`/`y` coordinates persist the drop location. For suppliers `receiverPort` is null; for receivers use the candidate's `port`.
- `{operation:"removeItem", node, port}` removes an existing list element.
- `{operation:"add", value:"WorkspaceTypeName"}` appends a section with a generated anchor; `table` and `tuple` create primitive collections.
- `{operation:"remove", node}` removes an unreferenced section.

Stdio also exposes `hix/synchronize` (`documents: [{path,text,revision}]`), `hix/get`
(`path`), `hix/remove` (`path`) and `hix/definitions`, backed by `HixAnalyzerService`.
Hix synchronization retains that service's directory snapshot semantics; send the
complete set of documents for each directory being synchronized.

To serve an already-built frontend directly:

```sh
dotnet src/Hix.Toolchain/bin/Debug/net8.0/Hix.Toolchain.dll graph-server \
  --root "$PWD" --web "$PWD/src/Hix.GraphEditor/dist" --port 5178
```

The web workflow uses the managed toolchain build. The frontend uses the standard
[XYFlow custom-node and handle APIs](https://reactflow.dev/learn/customization/handles).
