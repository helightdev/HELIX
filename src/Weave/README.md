# Weave

The codegen system drafted in [Weaver.md](../../Weaver.md). It knows nothing about HELIX:
HELIX's own mixins live with the project, in `src/HELIX/Weave/` beside its `Assets/` folder, so
Unity never compiles them into the game.

```
src/Weave/                      the system — no HELIX knowledge
  Helix.Weave/                  host + authoring surface (netstandard2.0)
  Helix.Weave.Lower/            the lowering pass + CLI (net8.0)
  Helix.Weave.Tests/            unit tests + the end-to-end pipeline test (net8.0)

src/HELIX/Weave/                HELIX's own mixins, beside Assets/ (never compiled by Unity)
  HELIX.Weave.Mixins/           the ported .hix files
  HELIX.Weave.Generator/        gen/ + csproj producing HelixWeaveGenerator.dll
  HELIX.Weave.sln               open this to edit the mixins with full IDE support
```

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

To work on HELIX's mixins, open `src/HELIX/Weave/HELIX.Weave.sln`; it carries the mixin project,
the generator that bundles them, and the two weave projects they build against.

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

`src/HELIX/Weave/HELIX.Weave.Mixins/` is all five of HELIX's `.hix` files ported: `Core.cs`, `Context.cs`,
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

## How this was checked against Hix

While both generators existed, a comparison tool built a real Roslyn compilation from Unity's
own generated `.csproj` — same sources, references, defines and sibling-assembly references —
ran both over it, and compared their output semantically: base types and members as sets, method
bodies in order, ignoring layout and spellings like `int` versus `global::System.Int32`.

It ended at **64 of 77 generated types equivalent, none produced by only one generator, and no
weave errors**. Hix and the tool are both gone now; what follows is the record of what that
comparison established, because it is the evidence that the port is faithful.

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
- Module discovery walked referenced assemblies, so an application module re-registered
  components a referenced module already had — the one bug that only showed at runtime.
- A bare `[Ticker]` produced `FrameCountTicker(1)` instead of `(0)`, and enum properties went
  through `EqualityComparer<T>.Default` instead of `==`.

### The remaining differences

64 of 77 generated types are byte-for-byte equivalent. The 13 that differ fall into five kinds,
each checked by hand:

**Weave is right, Hix was wrong**

- **`[Prop(Equatable = false)]` is honoured** (`SliderStyle.box`). Hix read the flag only after
  it had already returned early for a `[Prop]` with no explicit `defaultInit`, so the flag did
  nothing. Weave excludes the property from `Equals`/`GetHashCode`. This changes generated
  behaviour and was kept deliberately: the attribute says what it means.
- **`registration.RegisterHandlerBinding` and the `managed.IsActive` ticker guard are emitted**
  on every `[Managed]` type. Hix gated both on a `var#IsComponent` flag that another mixin had
  to have set first, so whether they appeared depended on attribute evaluation order. Weave asks
  the type whether it carries `[Managed]`.

**Cosmetic**

- `UnloadManaged` is generated `public` where Hix emits it `private` on some types. Public is a
  superset and compiles; the asymmetry in Hix's own output looks incidental.
- The datatype array has no trailing comma before its closing brace.
- A resource's wire key is an escaped string rather than a verbatim one, which also escapes
  correctly if a path ever contains a backslash.

## Wired into the Unity project

The generator is installed at
`src/HELIX/Assets/Plugins/HELIX/Runtime/Scripts/HelixWeaveGenerator.dll` with a `RoslynAnalyzer`
meta, and is the only one: Hix and its generator are gone.

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

## Performance and incrementality

The pipeline is built around the prelude/late split. Each annotated symbol's prelude runs inside
its own `ForAttributeWithMetadataName` transform and produces a value-only `SymbolContribution`,
so Roslyn caches it; contributions are grouped per type and each type's output is registered
separately with its own equality, so an untouched type is not re-rendered. `IncrementalTests`
reads the driver's own step tracking rather than trusting the shape:

| edit | preludes re-run | types re-rendered |
|---|---|---|
| change a method body | 0 of 4 | 0 of 3 |
| whitespace only | 0 | 0 |
| add an annotated method | 1 of 5 | 1 |

Three things had to change to get there, and each is a rule worth keeping:

- **Nothing symbol-backed may survive a prelude.** `PureTypeRef` holds plain strings rather than
  handles into a pool that only lives for one run, and emissions, diagnostics and the type's
  shape are all value data. The carry was already detached, which is what made it a usable cache
  key in the first place.
- **Cross-symbol state is gone.** A cached prelude cannot know whether another symbol already
  emitted a shared block, so every symbol that needs one emits it under a key (`t.Shared`) and
  the merge keeps a single copy. `IsComponent` became a question about the type's attributes
  rather than a flag another mixin left behind — which also removes Hix's dependence on
  attribute evaluation order.
- **Injection-target aliases are declared, not accumulated.** A mixin's `[Aliases]` method says
  what its attribute implies for a type, and the host seeds them from the type's attributes
  before any prelude. That is what `defineTarget` always meant.

Unity's own per-assembly generator timings, heaviest assembly in a burst:

| | heaviest assembly | burst total |
|---|---|---|
| Hix (`HixUnityGenerator`) | 0.63 s | ~2.0 s |
| weave, first version | 1.75 s | — |
| weave, after caching symbol lookups | 0.45 s | ~0.65 s |
| weave, incremental | **0.16 s** | **~0.35 s** |

Measured side by side in one process weave looked worse than Hix on the largest assembly, but
that comparison flattered Hix: its `MixinCompilationCache` was process-wide, so it compiled its
`.hix` catalog once and reused it across all 20 projects, which is not what happens in a real
build. Unity's own numbers above are the ones that count.

## Building the generator DLL

```bash
./gradlew deployWeaveGenerator
```

That lowers the mixins, builds the generator and installs it into
`Assets/Plugins/HELIX/Runtime/Scripts`, verifying the copy byte for byte and checking that the
`.meta` still carries its `RoslynAnalyzer` label — without it Unity imports the DLL as a plain
plugin and silently never runs it. The three steps are separate tasks (`lowerWeaveMixins`,
`buildWeaveGenerator`, `deployWeaveGenerator`) in the `helix` group, each with declared inputs
and outputs, so editing a mixin re-lowers but skips the build when the lowered sources come out
unchanged.

After deploying a new generator, regenerate Unity's project files before trusting the IDE. Rider
and VS keep using the analyzer their `.csproj` files pointed at when they were written, so a
freshly deployed generator can leave the IDE reporting errors on generated members while Unity
itself compiles cleanly.

By hand, if you need the pieces:

```bash
dotnet run --project src/Weave/Helix.Weave.Lower -- \
  src/HELIX/Weave/HELIX.Weave.Mixins src/HELIX/Weave/HELIX.Weave.Generator/gen
dotnet build src/HELIX/Weave/HELIX.Weave.sln -c Release
```

The result is `HelixWeaveGenerator.dll` (~130 KB), a single analyzer assembly holding the host
and the lowered mixins — the same shape as `HixUnityGenerator.dll`, with no `.additionalfile`.
It is **not** installed into `src/HELIX/Assets` yet: both generators would emit the same partial
members and the project would not compile, so Hix has to come out in the same step.
