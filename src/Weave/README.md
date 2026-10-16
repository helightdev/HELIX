# Weave

Standalone implementation of the codegen system drafted in [Weaver.md](../../Weaver.md).
Not wired into HELIX or `HELIX.sln` — it proves itself through its own tests first.

```
Helix.Weave/          host + authoring surface (netstandard2.0)
Helix.Weave.Lower/    the lowering pass + CLI (net8.0)
Helix.Weave.Tests/    unit tests + the end-to-end pipeline test (net8.0)
```

## The pipeline

```
Fixtures/*.cs  ──lower──▶  relocated declarations + wiring  ──csc──▶  generator  ──▶  user code
```

A mixin is an annotated dataclass. Lowering relocates its declarations and generates the
wiring around them; **method bodies are moved as text and never rewritten**, which is what
keeps the pass testable. `*.lib.cs` is copied through untouched.

```csharp
[Mixin(Unit.Method)]
public sealed class EventHandlerAttribute : Attribute {
  public EventHandlerAttribute(int priority = 0) => Priority = priority;
  public int Priority { get; init; }

  [Weave]
  static void Apply(MethodTarget t, EventHandlerAttribute a) {
    Shared.RequireHandler(t);
    var p = t.Param(0);
    t.Inject(Targets.Init, 1, $"eventHandlerList.Register<{p.Type}>({t.Name}, {Lit.Of(a.Priority)});");
  }
}
```

Two-pass mixins declare a nested carry record: `[Prelude]` returns it, `[Late]` takes it.
The late pass gets `(LateTarget, Carry)` and nothing else — no attribute, no symbols — so it
stays pure with respect to analyzed source. `TypeRef` (symbol-backed, prelude only) converts
implicitly, one way, to `PureTypeRef` (detached, pooled, equatable).

## Running it

```bash
dotnet test src/Weave/Helix.Weave.Tests/Helix.Weave.Tests.csproj
```

```bash
dotnet run --project src/Weave/Helix.Weave.Lower -- <mixin-dir> <output-dir> [refs...]
```

## Why there are no golden files

`PipelineTests` lowers the fixtures, compiles the lowered output into a real generator, runs it
over sample user code, and compiles the result. Wrong output fails to compile; wrong lowering
fails to compile. `DumpTests` writes both stages to `$TMPDIR/weave-dump` for eyeballing.

## Derivation

`Unit.Derivation` mixins are the property pipeline. They are *not* triggered by the attribute
pipeline — they are consulted from inside another mixin's prelude, by `t.DeriveFields()` or
`t.DeriveParameters()`, which seed a `DerivedProperty` per field/parameter and run each step
whose attribute the symbol carries, in `Order`.

```csharp
[Mixin(Unit.Derivation, Order = 10)]
public sealed class PropAttribute : Attribute {
  public string DefaultValue { get; init; }

  [Weave]
  static DerivedProperty Derive(DerivationTarget t, PropAttribute a, DerivedProperty p) =>
    p with { Required = false, Default = a.DefaultValue,
             Parameter = $"{p.Type.Qualified} {p.Name} = {a.DefaultValue}" };
}
```

`DerivedProperty` is fully detached (strings plus `PureTypeRef`), so it rides in a carry record.
`PropertySeed` builds the initial one — modifiers, parameter, argument, assignment, equality,
hash — and deliberately leaves `Datatype` empty: what a datatype means belongs to the mixins
that consume it, not to the core.

## The HELIX port

`Helix.Weave.Mixins/` is all five of HELIX's `.hix` files ported: `Core.cs`, `Context.cs`,
`Compose.cs`, `Boot.cs`, `Odin.cs`, plus `Shared.lib.cs` and `Types.lib.cs`. It references
neither Unity nor the HELIX runtime — every runtime type is named by metadata string.

`HelixPortTests` lowers that project, compiles the generator it produces, and runs it over
HELIX-shaped user code against the stub runtime in `HelixStubs.cs`. It checks the things that
are easy to get wrong: the recompose priority ladder's order, per-type target aliases
(`$Compose` is `PerformCompose` on a custom boundary and `MixinCompose` on a boundary mixin),
`RequireHandler` running once across several handlers, the datatype table, and shape mistakes
being reported rather than silently skipped.

Known differences from the Hix original, all deliberate:

- Shape mismatches report a diagnostic. `EventHandlerAttribute` used to fall off the end of its
  `when` cascade and emit nothing.
- `[Prop]`'s default-init handling is one enum comparison instead of five `:eq` branches, since
  the decoder now decodes enums properly.
- The datatype a property maps to is resolved where it is consumed (`Shared.DatatypeOf`), so the
  weave core keeps no opinion about what a datatype is.
- `Boot.CommandAttribute` carries its derived parameter model to the late pass instead of
  re-deriving it.

## Not done yet

- `Unit.Parameter` is declared but not dispatched (it reports `WEAVE001`).
- One session per containing type is rebuilt per run; per-type incremental caching is the next
  step up if generation time ever shows on a profile.
- Attribute decoding covers positional, named, enums, arrays and type arguments; nested attribute
  arrays of types are untested.

## Comparing against Hix on the real codebase

`Helix.Weave.Compare` builds a real Roslyn compilation from Unity's own generated `.csproj`
(same sources, references, defines and sibling-assembly references), runs both generators over
it, and compares their output semantically — base types and members as sets, method bodies in
order, ignoring layout and spellings like `int` vs `global::System.Int32`.

```bash
dotnet run --project src/Weave/Helix.Weave.Compare -- \
  src/HELIX src/Weave/Helix.Weave.Mixins \
  src/Weave/unity-backup/HixUnityGenerator.dll --out /tmp/weave-compare
```

Across the whole project: **51 of 77 generated types semantically identical, 0 produced by only
one generator, 0 weave errors.** The remaining 26 differ in ways checked by hand and judged
equivalent or deliberate (see below).

### Bugs the comparison found

- `InputStateListener` applies to types, not methods.
- The generated partial carried an accessibility modifier, which breaks on any `internal` type.
- File-level declarations (`_Companion` classes and their extension methods) belong outside the
  namespace, which is where Hix has always put them.
- `Slot<T>`'s id counter was a static on a *generic* type, so `Slot<bool>` and `Slot<string>`
  shared ids and collided.
- The decoder guessed one constructor; `BindAttribute` has three overloads, so positional
  arguments must be matched by the parameter names of the constructor actually invoked.
- Attribute members must be matched case-insensitively: HELIX names its parameters `scope`,
  `source`, `qualifier`, and exact matching silently dropped them.
- Attribute arguments bind to *fields* as well as properties (`PropAttribute.defaultValue`),
  and a relocated `readonly` field cannot be assigned by the decoder.
- A non-`partial` type must be skipped, not generated into.
- Module imports were qualified twice, producing `global::global::`.

### Deliberate differences from Hix

- **Equality is opt-in.** Hix's `equatableSelf` means the type declares `IEquatable<Self>`;
  weave matches that rather than emitting equality for every `[Structure]`.
- **`RegisterHandlerBinding` is always emitted** for an `[EventHandler]` on a `[Managed]` type.
  Hix emits it only when its `var#IsComponent` flag happened to be set first, which depends on
  attribute evaluation order; weave runs type-level mixins first, so the intent holds.
- **`UnloadManaged` is generated public**, where Hix emits it private on some types. Public is a
  superset and compiles; the asymmetry in Hix's own output looks incidental.
- Shape mismatches are reported as diagnostics instead of silently generating nothing.

## Wired into the Unity project

The generator is installed at
`src/HELIX/Assets/Plugins/HELIX/Runtime/Scripts/HelixWeaveGenerator.dll` with a `RoslynAnalyzer`
meta. The two generators cannot both run — they emit the same partial members — so
`HixUnityGenerator.dll` is renamed to `.dll.disabled` alongside it, and the originals are kept in
`src/Weave/unity-backup/`.

**Result: the project compiles with zero errors, and 152/152 EditMode tests pass**, including
the module-discovery and managed-container tests that exercise generated code directly. The only
warnings are `WEAVE003` on the discovery fixtures that are deliberately not `partial`.

Compiling is not the same as working: running the sample scene found a bug the comparison had
not, because it only surfaced at runtime — module discovery walked the whole compilation
including referenced assemblies, so an application module re-registered components that a
referenced module had already registered (`ComponentGraphException: Component type
HELIX.UI.GuiService is already registered`). Fixed by scanning
`Compilation.Assembly.GlobalNamespace`, which is what Hix's `collectAnnotatedTypes` always meant.
Registration order is now sorted by qualified name too, since load order uses entry order as a
tie-breaker.

## Performance

From Unity's own generator timings in `Logs/Editor.log`:

| | per-assembly generator time |
|---|---|
| Hix (`HixUnityGenerator`) | 1.47 – 1.59 s |
| weave (`HelixWeaveGenerator`) | 0.72 – 0.91 s |

Weave is about twice as fast, mostly because it has no `.hix` catalog to compile on every
compilation. Measured side by side in one process (`Helix.Weave.Compare` prints per-project
timings), weave looks *worse* than Hix on the largest assembly — 571 ms against 193 ms — but
that comparison flatters Hix: its `MixinCompilationCache` is process-wide, so it compiles the
catalog once and reuses it across all 20 projects, which is not what happens in a real build.

Caching metadata-name lookups and the annotated-type scan in `SymbolFacts` halved the worst case
(1323 ms → 571 ms cold, 243 ms warm). The remaining ceiling is incrementality: `Initialize`
collects every match and combines it with `CompilationProvider`, so any edit regenerates every
type in the assembly. Per-type caching is the next real win and is not done.

To go back to Hix: restore the two `HixUnityGenerator.dll*.disabled` files to their original
names and delete `HelixWeaveGenerator.dll*`.

## Building the generator DLL

```bash
dotnet run --project src/Weave/Helix.Weave.Lower -- \
  src/Weave/Helix.Weave.Mixins src/Weave/Helix.Weave.Generator/gen
dotnet build src/Weave/Helix.Weave.Generator -c Release
```

The result is `HelixWeaveGenerator.dll` (~130 KB), a single analyzer assembly holding the host
and the lowered mixins — the same shape as `HixUnityGenerator.dll`, with no `.additionalfile`.
It is **not** installed into `src/HELIX/Assets` yet: both generators would emit the same partial
members and the project would not compile, so Hix has to come out in the same step.
