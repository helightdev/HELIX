# HELIX codegen, restructured: mixins as annotated dataclasses, lowered by Roslyn, then compiled

Converged design. Replaces Hix for HELIX's code generation.

## 0. Shape

- **A mixin is an annotated dataclass.** The attribute's own declaration — its parameters as properties — carries the weave logic as static methods on itself. One declaration site for the trigger, the arguments and the implementation.
- **Emission is strings.** Raw interpolated strings; same `{{hole}}` syntax as Hix's `@>` blocks, so templates port by copy-paste.
- **Bodies are plain C#.** Shape tests, branching, joins, LINQ — normal control flow. The only declarative part is the `[Mixin(Unit.…)]` application unit.
- **It's a higher-level DSL over Roslyn, not a replacement for it.** `TypeTarget`/`MethodTarget` expose the raw `INamedTypeSymbol` / `IMethodSymbol` / `Compilation`; the sugar is additive and never in the way.
- **The late pass is pure.** It sees only detached values — no attribute, no symbols, nothing analyzed-source-backed — because anything else is an incremental dependency. Lowering enforces this by analysis (§3).
- **Lowering never touches a method body.** It relocates declarations and generates wiring — nothing else. Body text is moved verbatim, so the transform is testable by comparing declarations rather than reasoning about semantics.
- **Sources are lowered, then compiled.** Roslyn analyses the authoring files, relocates their declarations into a build directory and generates the wiring around them; `*.lib.cs` files are copied through untouched. That output is what becomes the generator DLL.

## 1. Pipeline

```
src/HELIX.Weave.Mixins/            authoring project. References HELIX.Weave only.
  Context.cs  Compose.cs            No Unity, no HELIX runtime.
  Core.cs  Boot.cs  Odin.cs         Compiles as a sanity check; output never shipped.
  Derivation.lib.cs                 *.lib.cs → copied verbatim, no transform
        │
        │  HELIX.Weave.Lower   (console app: Roslyn analyse + relocate)
        ↓
obj/weave/*.g.cs                   relocated declarations + generated wiring + copied libs.
                                   references only Roslyn + HELIX.Weave.
        │  csc
        ↓
HELIX.Weave.Generator.dll          → Unity (replaces HixUnityGenerator.dll)

src/HELIX.Weave/                   host: emission buckets, injection weaving,
                                   diagnostics, incremental model, the target and
                                   emission API. Ported (§6).
```

Both the mixin project and the generator reference the *same* `HELIX.Weave`, so a body that compiles in the authoring project compiles identically after relocation. That's what makes verbatim body movement safe, and it's the constraint the whole design hangs off: **the authoring API is the runtime API.** Nothing in a body is sugar awaiting a rewrite — type names are strings, literals go through `Lit.Of`, and what you read is what runs.

## 2. One pass — the default

```csharp
[Mixin(Unit.Method)]
public sealed partial class EventHandlerAttribute : Attribute {
  public int Priority { get; init; }

  [Weave]
  static void Apply(MethodTarget t, EventHandlerAttribute a) {
    Shared.RequireHandler(t);

    if (t.Params is not [var p]) {
      t.Report(Diag.Shape, "expected exactly one event parameter");
      return;
    }
    if (t.IsComponent)
      t.Inject(Targets.ConfigureManaged, -90000,
        $"registration.RegisterHandlerBinding<{p.Type}>({Lit.Of(a.Priority)})");

    var register =
      p.ByValue && p.Type.Implements(Types.AsyncChainEvt) ? "RegisterAsync" :
      (p.ByValue || p.ByRef) && p.Type.Implements(Types.Evt) ? "Register" : null;

    if (register is null) {
      t.Report(Diag.Shape, "parameter must be an Evt or an IAsyncChainEvt");
      return;
    }
    t.Inject(Targets.Init, 1,
      $"eventHandlerList.{register}<{p.Type}>({t.Name}, {Lit.Of(a.Priority)});");
  }
}
```

A single `[Weave]` method generates a prelude-pass mixin and nothing else. The attribute's properties are its parameters, so the decoder (§5.3) is generated straight off this declaration — no second signature to keep in sync.

`Types.Evt` is a plain `const string` of a metadata name, and `Lit.Of` is written out. Both could have been sugar (`Implements<Evt>()`, bare `{a.Priority}`) desugared during lowering — that's the version of this design that rewrites bodies, and it isn't worth it. Body rewriting is the part that's hard to test, and the sugar it buys is two characters.

The explicit `else`-path reports matter: today this mixin falls off the end of its `when` cascade and **silently generates nothing** when the parameter isn't an `Evt`. In plain C# that's a visible unhandled branch instead of an invisible missing `when`.

`Odin.hix` in this form is two `[Weave]` methods of four `t.Class`/`t.Implements`/`t.Annotation` calls each.

## 3. Two passes — the carry record is the purity boundary

Where a mixin needs the late pass (after all preludes have run and type-scoped state has settled), declare a nested record: the prelude returns it, the late pass takes it. Both annotated.

**The late pass gets the carry and nothing else.** No attribute, no target symbol, no `Compilation` — those are analyzed-source-backed, and touching them from the late pass makes it an incremental dependency on the analyzed source. This is the same reason Hix has purity and detached values; the model here is to make the carry record the one channel across the boundary, and have lowering verify it.

```csharp
[Mixin(Unit.Method)]
public sealed partial class CommandAttribute : Attribute {
  public string? Name { get; init; }
  public string? Description { get; init; }
  public string? Parent { get; init; }

  public record Carry(string StructName, string Invoke, string Name, string Description, string Parent);

  [Weave.Prelude]
  static Carry Prelude(MethodTarget t, CommandAttribute a) {
    var props = t.DeriveParameters();
    var structName = $"{t.Name}_Args";
    Shared.EmitGeneratedStructure(t, structName, props, datatype: true);

    var args = Code.Join(props, p => $"{p.Modifier}args.{p.Name}", ", ");
    return new Carry(
      structName, $"args => {t.Name}({args})",
      Lit.Of(a.Name ?? t.Name), Lit.Of(a.Description), Lit.Of(a.Parent));
  }

  [Weave.Late]
  static void Late(LateTarget t, Carry c) {
    t.Using("HELIX.Boot");
    t.Inject(Targets.LoadManagedLate, 0,
      $$"""
      context.PublishBind(typeof(HELIX.UI.Console.Command), null,
        CommandBridge<{{c.StructName}}>.Create({{c.StructName}}.Datatype,
          {{c.Invoke}}, {{c.Name}}, {{c.Description}}, {{c.Parent}}));
      """);
    }
}
```

Compare `Boot.hix`'s version, which re-derives the parameter model in its late block (`%inline local StructHandle @= DeriveParameters(target);`) purely because there was no way to hand data across passes. The carry deletes that duplication and makes the dependency explicit and typed.

The rules, all checked by lowering:

- `[Weave.Prelude]` returning `void` → no late pass. Returning a record → a matching `[Weave.Late]` is required, and its second parameter must be that record type.
- `LateTarget` exposes **emission only**: `Class`/`File`/`Implements`/`Extends`/`Annotation`/`Using`/`Inject`/`Report`. No symbol queries, no `Compilation`, no attribute. The prelude's richer `MethodTarget`/`TypeTarget` aren't in scope.
- The carry's members must be **detached types**: primitives, `string`, enums, `PureTypeRef` (§3.1), and immutable collections or nested records of those. An `ITypeSymbol`, `AttributeData` or `MethodTarget` in a carry is a build error — that's the whole point of the check.
- The late pass may read **type-scoped slots** (§4). Those are part of the same per-type aggregation the host already collects before rendering, so they don't widen the dependency graph the way a symbol reference would.

### 3.1 Two type refs: backed in the prelude, pure downstream

Attributes commonly take a type — `BindAttribute(opaque? type)`, `ManagedAttribute(opaque? scope)`, `PropAttribute(Datatype)`. `System.Type` is wrong here: the authoring project doesn't reference the runtime types, and a type-valued attribute argument decodes to an `ITypeSymbol`.

The prelude keeps the symbol. Restricting it would be restricting the pass that has every right to ask the compiler things — so the prelude's type ref is **physically backed**, and detachment happens on the way out:

```csharp
// prelude only — holds the live symbol, full Roslyn access
public readonly struct TypeRef {
  public ITypeSymbol Symbol { get; }
  public string Qualified { get; }            // pooled handle, resolved once
  public bool Implements(string metadataName) => …;   // sugar over Symbol
  public PureTypeRef Pure() => …;             // precomputes the fact set
  public static implicit operator PureTypeRef(TypeRef r) => r.Pure();
}

// carry- and late-safe — no symbol, equatable, pooled strings
public readonly record struct PureTypeRef(StringHandle Qualified) {
  public StringHandle MetadataName { get; init; }
  public bool IsClass { get; init; }
  public bool IsEnum { get; init; }
  public bool IsPrimitive { get; init; }
  public bool ContainsPointer { get; init; }
  public ImmutableArray<PureTypeRef> TypeArguments { get; init; }
}
```

The conversion is implicit one way only. A `TypeRef` assigned into a carry field becomes a `PureTypeRef` silently and correctly; there is no path back, so the purity boundary is enforced by the type system rather than by the lowering check — the check (§3) is then just a backstop for the cases types can't catch, like someone stashing an `AttributeData`.

Both render identically into emission (`{p.Type}` → qualified name), so moving a line of emission from the prelude to the late pass doesn't change its output.

The flip side is the usual one: whatever the late pass needs to know about a type must be a fact `PureTypeRef` carries. Start from what the current scripts query — `:class`, `:enum`, `:primitive`, `:pointer`, `:containsPointer`, type arguments — and grow it on demand. `DerivedProperty` (§7) is built from these, which is what makes it carry-safe.

## 4. State: three scopes, clearly separated

Hix had `var#` (per type) and `tar#` (per target) bags holding everything. Split by what the state is actually for:

| Scope | Mechanism | Example |
|---|---|---|
within one mixin, prelude → late | the **carry record** (§3) | `CommandAttribute.Carry` |
across mixins, per target type | `Slot<T>` + `t.Once(slot)` | `var#CompanionName`, `var#IsEventHandler` |
across mixins, per annotated target | `Slot<T>(Scope.Target)` | `tar#PropOptional` |

Slot values must be detached types for the same reason carries must be (§3); the late pass may read type-scoped slots but not target-scoped ones, since the latter are per-symbol.

The `RequireX()` family stays a plain function call in a `.lib.cs` file:

```csharp
// Shared.lib.cs — copied verbatim, no lowering
static readonly Slot<bool> HasHandler = new(Scope.Type);

internal static void RequireHandler(Target t) {
  if (!t.Once(HasHandler)) return;
  t.Using("System");
  t.Implements("IEventListener");
  t.Class("""
    [NonSerializedAttribute]
    protected readonly EventHandlerList eventHandlerList = EventHandlerList.Create();
    EventHandlerList IEventListener.HandlerList => eventHandlerList;
    """);
  t.Inject(Targets.Dispose, 1, "eventHandlerList.UnregisterAll();");
}
```

`.lib.cs` is the escape hatch generally: helpers, records, the derivation pipeline, anything that wants raw Roslyn. It bypasses the lowering pass entirely, so it must already be in runtime form (strings for type names, explicit `Lit.Of`) — which is exactly right for library code.

## 5. What lowering does

Declaration-level relocation plus generated wiring. **No body is read or rewritten** — a weave method's body is lifted as text and dropped into its new home.

| Authoring declaration | Relocated / generated as |
|---|---|
the dataclass itself | a plain `…Args` class in the generator: `: Attribute` dropped, weave methods moved out, properties kept as-is |
its properties | a generated `Decode(AttributeData)` — positional + named, C# defaults, enums, arrays, `typeof` |
type-valued property | `TypeRef?` — symbol-backed, since it is decoded for the prelude (§3.1) |
`[Weave]` method | moved into the generated mixin class, attribute stripped, **body verbatim** |
`[Weave.Prelude]` / `[Weave.Late]` pair | two-pass wiring; carry type checked across the pair |
nested carry record | moved as-is |
`[Mixin(Unit.…)]` | pipeline entry, unit filter, dispatch entry |
`[InjectionTarget(…)]` | descriptor constants (name, static, public, delegate type) |
a late pass, or a carry member, touching attached state | **build error** — the purity check (§3) |
`*.lib.cs` | **copied, untouched** |

Because the authoring project and the generator reference the same `HELIX.Weave`, a relocated body means exactly what it meant where it was written. The `goto Label` flow in `Context.hix`/`Compose.hix` becomes early returns by hand, once, during the port — not by any transform.

This is also what makes the pass testable: its unit of work is "did this declaration land in the right shape", which is a syntax-tree comparison, not a semantics question. The expensive, subtle alternative — desugaring inside bodies — buys a little syntax and costs a test surface nobody wants to own.

`#line` directives in the relocated output point back at the authoring sources, so compile errors and diagnostics land on the author's real line.

Generated wiring, beyond the per-mixin lowering:

1. **Dispatch** — attribute metadata name → mixin, closed `switch`. No reflection, no assembly scanning.
2. **Pipeline registration** — one `ForAttributeWithMetadataName("…", …)` per mixin. That API needs a literal at pipeline-construction time, which is exactly why the list wants generating rather than hand-maintaining in `MixinGenerator.Initialize`.
3. **Attribute decoders** — positional + named args, C# defaults, enums, arrays, `typeof`. Deletes the `$Prop#defaultValue:unwrap` / `:cast(opaque)` / `:eq(3)` vs `:eq<None>` vs `:eq<HELIX.PropInit.None>` stack in `Core.hix`'s `PropAttribute`, which exists only because Hix couldn't decode an attribute properly.
4. **Equatable symbol models** per unit kind — a fixed fact set feeding the incremental pipeline. Replaces the hand-written `HixOutputModelComparer` + `HixRenderFingerprint`.
5. **Diagnostic descriptors** with stable IDs, one per `t.Report` site.

## 6. Keep the host

`src/Hix.MixinGenerator` is ~3 200 lines and is not the language. Port roughly as-is:

- `MixinOutput` / `MixinEmissionTarget` / output accumulators.
- Injection weaving — `MixinGenerator.Methods.cs`: matching contributions to partial declarations, validating static/visibility, building declarations, base calls.
- Diagnostics plumbing.

Reusable verbatim, no Hix dependency: `Hix.Roslyn/Analysis/SharpStringBuilder.cs` (~218 lines, scoped C# text builder with list joining — serves as both the emission helper layer and what the lowering pass writes with), `Hix.Roslyn/Analysis/GeneratorApi.cs` (`GeneratorAnalysis`), and most of `Hix.Roslyn/Functions/*.cs` as the symbol-facts layer behind `TypeTarget`/`MethodTarget`.

Injection priorities stay exactly as they are, **including the insertion-order tie-break within equal priority** — `Compose.hix` leans on the `-1000 … 1000` ladder hard, and any change there churns output.

### 6.1 Keep the caching, all of it

The generator is fast because of work that has nothing to do with the language. Port it layer for layer:

| Today | Keeps doing |
|---|---|
`HixStringPool` / `HixString` handles | `MixinOutput` holds **pooled handles**, not strings; text resolves once at render. `StringHandle` in `PureTypeRef` (§3.1) means detachment allocates nothing and equality is a handle compare. |
`OutputFingerprintBuilder` (FNV-1a over outputs + length), `HixOutputCollection` | emission accumulates a rolling hash, so comparers short-circuit instead of deep-comparing output lists |
`HixRenderFingerprint`, `HixOutputModelComparer` | incremental-pipeline equality without deep compares — now generated per unit kind (§5) rather than hand-written |
`RoslynValueCache` (`Hix.Roslyn/Runtime/RoslynHostExpressionCache.cs`) | memoized per-symbol derived facts, keyed by reference, with hit/miss counters. Becomes the cache behind `TypeRef` and the target facades. |
`GeneratorAnalysis._attributeCache` (`ConditionalWeakTable<ISymbol, …>`) | unchanged |
`HixProfiler` / `HixCompilerProfiler` | unchanged, retargeted at the new stages |

`MixinCompilationCache`'s bounded LRU over compiled `.hix` scripts and library files is the one piece that dies with the language — there are no scripts to compile at generator runtime any more, which is itself the largest single speedup on the table.

The thing to be careful about: the detachment boundary (§3) is also a *caching* boundary. A `PureTypeRef` with pooled handles is a sound cache key; an `ITypeSymbol` is not, since it is tied to a `Compilation` that changes on every keystroke. Keeping the late pass pure is what lets its outputs be cached per type instead of recomputed per edit — the same reason Hix had purity in the first place.

Everything else — `src/Hix`, the VM glue, `Hix.Standalone`, `Hix.Toolchain`, `Hix.GraphEditor`, `Grammars`, `RiderPlugin`: **~75 000 lines** — goes, with the `.hix` files and the committed `.additionalfile` bytecode blob.

## 7. `Core.hix` becomes a `.lib.cs` plus derivation mixins

String-fragment building stays string-fragment building, with a record instead of a 16-slot untyped table:

```csharp
record DerivedProperty(
  string Name, PureTypeRef Type, RefKind Ref, string Modifier, string Parameter,
  string Argument, string CallArgument, string Assignment, string Datatype,
  bool Required, string Default, string Configuration, bool Equatable,
  bool Unsafe, string Equality, string Hash);
```

`:put(<equatable>, false)` → `p with { Equatable = false }`, compiler-checked. Every member is detached, so a `DerivedProperty` can ride in a carry record (§3) — which is what `Boot.hix`'s `CommandAttribute` needs and currently fakes by re-deriving. `SeedProp`, `DeriveFields`, `DeriveParameters`, `BuildDatatype`, `EmitGeneratedStructure` all live in `Derivation.lib.cs`. The `derivation mixin` declarations become dataclasses with `[Mixin(Unit.Derivation, Order = …)]` whose weave method takes and returns a `DerivedProperty`.

## 8. Open decisions

1. **Is the dataclass the runtime attribute too?** The declaration already holds everything the runtime attribute needs. Lowering could emit two outputs from one file: the plain attribute for the HELIX runtime assembly, and the generator code. That removes the duplicate declaration entirely and is the thing I'd aim for — but it couples the lowering step into the Unity assembly's build, so decide it deliberately. Fallback: declare the attribute twice and generate the authoring copy from runtime metadata.
2. **Is the lowered output committed?** Under `obj/` it's invisible; committed under `gen/` every lowering change becomes a reviewable diff of real C# — the best defence against a lowering bug becoming a silent codegen bug. Recommend committed, with a test that fails on drift.
3. **How the lowering pass loads the authoring project.** `MSBuildWorkspace` is heavy and version-fragile; a `*.cs` glob plus a generated reference list is lazier and enough.
4. **How much fact goes in `PureTypeRef` and the cached symbol model.** Start with the set the current scripts query; accept recomputation in the prelude for rare ones. Resist deriving the set from the mixin bodies — that's analysis creep, and the purity check (§3) already stops the failure mode it would be guarding against.
5. **`collectAnnotatedTypes<ManagedAttribute>`** (module discovery in `Context.hix`) is a whole-compilation query and the one thing that defeats per-symbol caching. Own pipeline branch, own comparer, so a body edit in one component doesn't re-run every module.

## 9. Order of work

1. **Golden harness first.** Snapshot every file today's generator produces for the Unity project; one test diffs against it. This is the entire safety net for everything below — build it before touching anything.
2. **Carve out the host** (§6) with Hix still driving it. Goldens byte-identical; no behaviour change.
3. **Authoring surface + lowering skeleton**, end-to-end on `Odin.hix`: two mixins, `Unit.Type`, one pass, no attribute args. Proves the pipeline, `.lib.cs` passthrough and the `#line` mapping.
4. **Attribute decoders + injection targets**, on `Context.hix`'s `AutoDispose`, `AlwaysEnabled`, `Inject`.
5. **Two-pass + carry**, on `Boot.hix`'s `CommandAttribute` — the one that currently re-derives its model in the late block.
6. **`Core.hix`** (§7). The hard one; everything depends on it.
7. **`Context.hix`** in full, then **`Compose.hix`** last — biggest, heaviest user of injection priorities and companion-type state.
8. **Delete.**

Steps 3–7 run with both dispatch paths live (Hix catalog + lowered C#, keyed on attribute name), so you port one attribute at a time with goldens green throughout.

## 10. Honest costs

- **Two compile stages to debug.** A lowering bug surfaces as a compile error in generated code or as wrong output downstream. `#line` mapping and committed lowered output (§8.2) are the mitigations; build both at step 3, not later.
- **Iteration loop gets worse before it gets better.** A `.hix` edit is an additionalfile change today; after, it's lower + compile + Unity DLL swap. Build a standalone generator host (console app pointed at the project) during step 3 — that's the real inner loop.
- **Rider tooling, the HAL graph editor and the LSP go away**, replaced by C# tooling you don't maintain. Good trade.
- **Emitted code is still unchecked strings.** The golden harness catches regressions; nothing type-checks the output text itself.
- **The lowering pass is new surface that can be wrong** — maybe 1k lines now that it only relocates declarations and emits decoders. `.lib.cs` passthrough and verbatim bodies keep it from having to understand most of the code it moves.
- **Mixins can run arbitrary code at compile time.** Hix couldn't. Single-team codebase, don't care.
