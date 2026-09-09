# Hix

Hix is a host-independent language compiler and bytecode VM. `Hix.csproj` targets .NET Standard 2.0
and has no Roslyn, Unity, or source-generator dependencies. Public APIs use the `Hix` namespace.

The repository separates hosts into three projects:

- `Hix.Roslyn`: reusable semantic symbol services through `HixRoslynBackend`.
- `Hix.Standalone`: .NET 8 runtime and CLI through `HixStandaloneBackend`.
- `Hix.MixinGenerator`: HELIX Unity generation through `HixMixinBackend`.

Mixin declarations remain part of the language grammar. Their host semantics and available functions
come from the chosen backend. Pass the same backend configuration to compilation and execution.

## Backends

Derive from `HixBackend`, override `RegisterFunctions` and `RegisterRoots`, and call the base
registration methods to retain inherited definitions. Catalogs are initialized lazily and remain
immutable. Function definitions expose signatures, effects, prelude requirements, and optional
editor reference categories. `HasEffects` and `RequiresPrelude` are independent.

Implement `FunctionDefinition.Execute(HixExecutionContext, IHixValue[], int)` to expose a host
function. Use `context.Invoke` for Hix callbacks. Contexts own invocation state and must not be
shared between concurrent executions; backend definitions and loaded VMs can be shared. Host roots
are resolved by `HixBackend.ResolveRoot`, without adding VM instructions.

Backend-specific data import, rendering names, attributes, configuration, and target resolution
are backend services. Roslyn semantic caches belong to its invocation contexts or weakly owned
compilations. Core function implementations contain no Roslyn type checks.

## Build and deployment

`Hix.sln` includes the four projects and their four test suites. The root Gradle `solutions` DSL
registers it alongside Rider and Unity; regenerate the root solution with
`./gradlew generateMonorepoSolution`.

- `./gradlew buildHix` builds the solution.
- `./gradlew testHix` builds and runs all four test suites.
- `./gradlew copyHixMixinGenerator` builds the generator in Release and deploys its DLL to Unity.

`BuildConfiguration` selects the library/test configuration. `HixGeneratorConfiguration` overrides
the deployment configuration. DLL copying belongs exclusively to Gradle; ordinary MSBuild builds
never modify Unity assets.

The generator source-links the canonical core and Roslyn implementation into one
`HelixSourceGenerator.dll`. It excludes the core's disabled profiler and supplies its opt-in
profiler and compilation cache. Rider consumes this bundled backend to use the generator's exact
function catalog without loading duplicate Hix assemblies. Other .NET hosts can reference the
independent `Hix` and `Hix.Roslyn` libraries.

## Bytecode execution

Compile Hix with `HixCompiler.Compile(source, mixinName)`, then pass the returned
`HixExpressionExecutionProgram` to `HixVM.Execute(program, context, variables)`.
Parsing, AST transformations, label resolution, and lowering happen exclusively in the compiler.
The VM accepts no source or AST overloads. Function signatures, lexical scopes, derivations, and
entry points in the executable image contain runtime metadata only.

Instructions are byte-aligned and variable-length. A one-byte opcode is followed immediately
by its operands, with no flags or padding. Multibyte operands use little-endian encoding.
The `HixOpcode` enum documents each instruction's operands, stack effects, and control behavior.

| Instruction form | Operands | Total size |
| --- | --- | --- |
| Stack/control operation (`Pop`, `Return`, etc.) | None | 1 byte |
| Pool access, collection construction | u16 pool index or element count | 3 bytes |
| Branch/block reference | s16 displacement from the opcode address | 3 bytes |
| `Call` | u16 function-name string index, u16 argument count | 5 bytes |

Storage destinations and flow operations have distinct opcodes instead of flag operands.
Root reads uniformly use `LoadRoot` with a string-pool name followed by `Member` when needed;
there are no host-specific or smart-lookup opcodes. The compiler resolves smart references to
`param` (the packed parameter), `args` (positional arguments), or `local`.
Selections lower to generated `__selector_` locals, ordinary branches/blocks, and `Equal`.
Transformation conditions read the generated local; the VM has no selector state or opcodes.
Generated locals follow the same visibility and lifetime rules as other hoisted locals.
`LoadConst` and `LoadString` access the two pools. Null, booleans, and empty collections use
operandless `LoadNull`, `LoadTrue`, `LoadFalse`, `LoadTuple`, and `LoadTable` instructions.
`PackTuple` and `PackTable` construct collections, while `Pack` retains normal argument-packing
semantics. Conversion instructions use the `Cast` prefix (`CastBoolean`, `CastString`), and
`Throw` produces a runtime error through the explicit completion path. `Call` still pushes its
result; discarded results use `Pop`. Both VM-wide pools support at most 65,536 entries (indices 0–65,535).
All branches, checked-expression handlers, and block references use signed relative byte
displacements (-32,768–32,767); block ends are exclusive. The compiler rejects references or
operands outside their range, and loading rejects merged pools exceeding the u16 capacity.
Source-line mappings are stored separately, keyed by instruction byte address.

The immutable image exposes encoded `Bytecode`, a non-string `ConstantPool`, and `StringPool`.
String instructions refer to the string pool; non-string literals refer to the value pool.
The VM uses operand stacks and interprets opcodes, with bytecode entry points for calls and blocks.
The VM loads programs into immutable VM-wide string and value pools, relocating pool operands
once during loading. `HixExecutionContext.Strings` reads that VM pool and has no setter; neither
program entry nor function calls switch pools. Create `HixVM(programs)` and call
`Run` to reuse a VM across programs. The static `Execute` convenience method loads an invocation
and any imported function images into a VM before running it.
Runtime-created strings remain dynamic and never mutate either pool. Prelude and late passes
share the same compiled image and pools. Exported function values retain their compiled image,
so invoking them from another program preserves their lexical bindings and constant indices.

Shared runtime storages use the persistent map (flat for small maps, HAMT for large maps).
Transaction snapshots and rollback share and restore immutable roots. Local dictionaries
are mutable and rented from a synchronized VM-owned pool, cleared on return after expressions,
calls, and derivations. Captured local tables own an immutable snapshot and survive reuse.

Tuples retain contiguous storage for indexed reads and scans. Push/pop copy directly into
the result array; they remain O(n). Mapping, filtering, and host attachment/detachment reuse
unchanged tuples, allocating result storage only when needed. Any/all/reduce do not build
result tuples. Published tuple arrays are never pooled or mutated by these operations.

`program.Disassemble()` prints three aligned columns: byte address, instruction with operands,
and stack pseudocode with resolved names and literal values. Functions (including signatures),
entry points, derivations, and nested blocks have separate headers at their actual addresses.
Branch destinations have standalone `loc_XXXX:` labels referenced by the instructions; addresses
remain hexadecimal byte offsets. Source lines appear beside the pseudocode. Source-generator debug output prints the global string and constant pools once, followed by disassemblies using the relocated global pool indices.

Return, break, continue, goto, checked failures, and execution limits propagate as explicit
`VmCompletion` values. They do not throw managed exceptions or unwind the host stack. Block
handlers consume local jumps and loop transfers, while function and derivation boundaries
consume returns and report invalid transfers as error values.

Instruction accounting and the execution-budget check live in the dispatch loop. Value-producing
instructions propagate unchecked errors directly; `Check`/`EndCheck` delimit explicit `?` handlers.
No `TICK` or `VALIDATE` bookkeeping instructions are emitted.

Storage roots are lightweight table views backed by `HixValueDictionary`. Member selection reads the backing dictionary directly;
local reads fall back to carried values without copying either dictionary. Table consumers can
enumerate the view on demand. Assignment, return, selector capture, and argument/collection packing
materialize a snapshot so stored values do not retain mutable execution frames or contain themselves.

Tables are unordered keyed collections. `keys`, `values`, `entries`, rendering, and table `join`
use an unspecified enumeration order; callers needing an order must arrange the resulting tuples
explicitly. Table equality ignores entry order. Fingerprints traverse keys in ordinal text order
so construction history does not change cache identity. Tuples remain ordered.

Immutable tables now use `Collections/PersistentMap.cs`: shared flat key shapes and privately owned
value arrays through 16 entries, then a persistent bitmap HAMT. Updates preserve old versions and
share unchanged branches; no-op updates return the existing table. Bulk builders construct a fresh
trie without repeated persistent path copying. Enumeration order remains unspecified. Storage roots
remain live `HixValueDictionary` views until captured. Ordinary table constructors copy input and
normalize interned keys using the explicitly supplied string pool; runtime dynamic keys need no pool.
The generic C# correctness harness and comparative benchmark live in `benchmarks/PersistentMaps`.

Profiling separates `vm.execute.total` (static invocation, including loading), `vm.load`
(pool construction and bytecode relocation), `vm.run` (context setup, execution, and export), and
`vm.execution` (language execution, including transactions and host calls). Disassembly uses
`bytecode.disassemble`, with pool formatting under `bytecode.disassemble_pools`.
`bytecode.identity` measures the lazily cached structural program fingerprint used for cache identity.
Normal generation and state keys do not disassemble programs; formatting is confined to explicit
disassembly requests and debug rendering. Timings are inclusive/nested and must not be added together.
No per-instruction tracing is added.
