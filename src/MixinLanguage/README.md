# Hix Language

This project contains the reusable mixin parser, compiler, virtual machine, table/text functions, additional-file
catalog, and Roslyn semantic execution support.

Its public namespace root is `Mixins`; compiler syntax and lowering APIs use `Mixins.Compiler`. Only the
source-generator entry points retain the `HelixSourceGenerator` namespace.

`Helix.MixinLanguage.csproj` is a normal .NET Standard 2.0 class library for hosts such as the Rider plugin. It is
deliberately not referenced by the source-generator project. Instead, `HelixSourceGenerator.csproj` links every source
file below this directory with a `Compile` item, so the deployed analyzer remains a single self-contained
`HelixSourceGenerator.dll`. Keeping the standalone project on the same target framework also prevents shared code from
accidentally using APIs unavailable to the analyzer host; its `IsExternalInit` compatibility shim is therefore shared as
well.

When adding shared implementation files, place them below `src/MixinLanguage/Language` or `src/MixinLanguage/Roslyn`.
Generator entry points and incremental-generator orchestration remain in the source-generator project's `Generators`
directory.

## Environment boundary

Host-independent debug rendering lives in `Debug`. Host policy lives behind the internal environment contracts used for
profiling and compiled-library caching.

The standalone project compiles `Environment/Default`, whose implementations are deliberately safe defaults:

- profiling is disabled and never writes files;
- `CompileCached` performs a fresh deterministic compilation and leaves caching to its host.

The source-generator project excludes `Environment/Default/**/*.cs` from its linked files and supplies matching
implementations in its own `Environment` directory. Those implementations retain the process-wide incremental-generator
cache and the opt-in Unity profiling report.

A future host can follow the same pattern: exclude the default environment files when source-linking and provide classes
with the same internal contracts. A normal project reference uses the standalone defaults automatically.

`MixinLanguage.sln` is a minimal wrapper solution containing only this project. External builds such as the Rider
plugin's Gradle configuration can reference that solution without loading the source-generator projects.

## Bytecode execution

Compile Hix with `HixCompiler.Compile(source, mixinName)`, then pass the returned
`MixinExpressionExecutionProgram` to `MixinVirtualMachine.Execute(program, context, variables)`.
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
once during loading. `ExecutionContext.Strings` reads that VM pool and has no setter; neither
program entry nor function calls switch pools. Create `MixinVirtualMachine(programs)` and call
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

Storage roots are lightweight table views backed by `MixinValueDictionary`. Member selection reads the backing dictionary directly;
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
remain live `MixinValueDictionary` views until captured. Ordinary table constructors copy input and
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
