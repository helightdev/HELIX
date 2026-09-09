# Hix

Hix is a host-independent language compiler and bytecode VM. `Hix.csproj` targets .NET Standard 2.0
and has no Roslyn, Unity, or source-generator dependencies. Public APIs use `Hix`, `Hix.Compiler`, `Hix.Functions`, and `Hix.Runtime`.

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

Implement `FunctionDefinition.Execute(HixThread, IHixValue[], int)` to expose a host
function, or register a `SimpleFunction` with explicit signatures and an `InlineFunction` delegate.
Use `thread.Invoke` for Hix callbacks, `thread.BindFunction` for lexical function binding, and
`thread.Outputs` / `thread.Logs` for emissions. Threads own invocation state and must not be
shared between concurrent executions; backend definitions and loaded VMs can be shared. Host roots
are resolved by `HixBackend.ResolveRoot`, without adding VM instructions.

Backend-specific data import, rendering names, attributes, configuration, and target resolution
are backend services. Roslyn semantic caches belong to its host contexts or weakly owned
compilations. Core function implementations contain no Roslyn type checks.

Compiler extension APIs are public: derive from `HixAstRewriter` or `HixCompilerStep`, and override
`HixBackend.CompilerSteps` to configure ordered transforms. `HixCompiler.DefaultSteps` is the
immutable standard pipeline; include those steps when adding a transform to retain normal binding,
hoisting, and lowering behavior. The pipeline applies to globals, mixins, and derivations.
AST constructors and source-location setters, prepared catalogs, bytecode models and compilation,
and disassembly APIs are also available for consumers building their own tooling.

Roslyn extensions can construct `RoslynHixValue`, subclass `HixRoslynContext`, and reuse its semantic
services and explicit cache lookup/store methods. `HixThread` is public and sealed: supply a context with `new HixThread(context)` and retrieve it
inside host functions through `thread.Context`. Override `HixBackend.CreateContext()` to supply
custom data when callers use `backend.CreateThread()`. Services receive the thread explicitly
when they need runtime values or its pool; contexts retain no execution back-reference.
Target changes are committed as detached values so a context can be reused with a fresh thread
and program image. Use separate contexts for concurrent target work.
No friend-assembly access or source inclusion is needed. VM lifecycle coordination, local pooling, and prepared-call bookkeeping remain internal;
operand frames and completion state remain private to `HixThread`.

## Generic mixins and host results

`mixin` and `derivation mixin` are core language constructs. Core mixins can run without Roslyn:
`derive` transforms records with a `value` field and preserves their other fields. A host can add
record constraints through `HixBackend.ValidateDerivationRecord`; the generator requires a semantic
`symbol`, while the core VM does not.

`emit(value)` publishes an arbitrary `IHixValue`; `emit(destination, value)` adds an opaque destination name.
`HixOutput.Value` preserves the value type. `HixThread.Emit` detaches values and destination names at
emission time, so collections and storage views remain stable after mutation or thread reuse.
Backends can consume structured output directly; the mixin generator owns its text rendering policy.
`HixExecutionResult` contains execution status, `HixOutput` values, `HixLog` entries, storage snapshots,
and counters. Core destinations carry no C# interpretation, injection priority, or member metadata.
`Variables` and `Carries` preserve `HixString` keys and `IHixValue` values. Detached outputs retain Hix values and dynamic `HixString` destinations.
Logs and errors retain `HixString` handles; resolve them against the result's `Strings` pool only
at host boundaries. Pooled text fingerprints are cached per immutable pool. `ExportVariables()`
and `ExportCarries()` explicitly convert storage into host objects when needed.

The generator owns `Hix.Mixins.MixinOutput`, `MixinEmissionTarget`, injection/using/target functions,
late-target discovery, and buffered generator emissions. It overrides `RegisterEmissionFunctions`
to interpret destination names. Contributions store generator outputs directly rather than synthetic
VM results. `HixContext.BeginExecution`, `CaptureEffects`, and `RollbackEffects` let a host make its
buffered effects transactional across failed expressions, calls, and derivation batches without
putting its output model into the VM.

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
`HixProgramImage` to `HixVM.Execute(program, context, variables)`.
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
`FunctionReferenceHixValue` constants hold only a signature. Loading replaces these entries in
an independent VM pool with `ResolvedFunctionHixValue` values bound to language declarations or
backend overloads. `Call` indexes that pool directly; no call-site binding dictionary is retained.
Function references use distinct slots where lexical scopes may resolve equal signatures differently.
The VM uses operand stacks and interprets opcodes, with bytecode entry points for calls and blocks.
Each VM owns one prepared image and its immutable string and constant pools. `HixThread`
owns invocation state, call frames, locals, outputs, and logs.
Host functions receive the active thread directly. `HixThread.Strings` resolves through its
VM while loaded; runtime strings remain dynamic and never mutate either pool.
Create `HixVM(programs)` and pass a thread from `backend.CreateThread()` to `Run` or `Invoke`.
The static `Execute` convenience method caches VMs by prepared program identity. Different
threads can use a VM concurrently; restarting an active thread is rejected. Sequential thread
reuse resets invocation state. The subclassable `HixContext` owns persistent variables, carries, target storage, and
host-specific data; Roslyn and mixin context subclasses own their semantic services and caches. Prelude and late passes share compiled pools;
function values retain lexical bindings and cannot execute against another program image.
Context storage survives sequential invocations and fresh threads, with rollback on failed expressions
and calls. Values are detached before leaving their program pool. A context permits one active
execution at a time; concurrent threads use separate contexts. Prelude status and call-frame purity
remain on the thread. Carry membership comes directly from the carry dictionary, including null values.

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
(pool construction and bytecode relocation), `vm.run` (thread setup, execution, and export), and
`vm.execution` (language execution, including transactions and host calls). Disassembly uses
`bytecode.disassemble`, with pool formatting under `bytecode.disassemble_pools`.
`bytecode.identity` measures the lazily cached structural program fingerprint used for cache identity.
Normal generation and state keys do not disassemble programs; formatting is confined to explicit
disassembly requests and debug rendering. Timings are inclusive/nested and must not be added together.
No per-instruction tracing is added.
