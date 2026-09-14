# Hix standalone

The standalone assembly targets .NET Standard 2.0 so it can be embedded into Unity and other
.NET Standard consumers. `Program.Run` retains the command-line host logic as a callable API;
a native or framework-specific executable host can invoke it without moving compiler or analyzer
logic out of this assembly.

```hix
func main {
  print(param)
}
```

The default entry is global `main`. Arguments are strings and use normal Hix packing: no arguments
produce null, one remains that value, and multiple arguments pack into a tuple. A tuple supplied
through the .NET API remains one argument. Return values are available through the API and are not
automatically printed. `print` writes to the backend's injectable `TextWriter`.

```csharp
var backend = new HixStandaloneBackend(Console.Out);
var program = HixCompiler.CompileFunctions(new[] { source }, backend);
var result = new HixVM(new[] { program }).Invoke(program, backend.CreateCoroutine());
```

Execution remains bytecode-only. `Program.Run` returns 0 for success, 1 for
compilation/runtime/I/O failure, and 2 for usage errors. Errors are written to the supplied error
writer. Roslyn functions and roots are unavailable in this backend.

Build the toolchain itself as a Release Native AOT executable with:

```sh
./gradlew buildHixToolchainAot
# Select a target runtime explicitly:
./gradlew buildHixToolchainAot -PHixToolchainRuntime=linux-x64
```

The task defaults to the current .NET runtime identifier and writes to
`build/hix-toolchain/aot/`. It always uses Release, independently of `BuildConfiguration`.
Publishing requires the .NET SDK and the Native AOT native compiler/linker prerequisites for
the target platform. Native AOT compatibility warnings must be reviewed before distribution.

The optional `Hix.Toolchain` executable exposes saved bytecode commands:

```sh
hix compile path/to/main.hix --output app.hixbc
hix compile path/to/project --output app.hixbc
hix run app.hixbc --entry main -- argument1 argument2
hix disassemble app.hixbc --output app.disassembly.txt
hix decompile app.hixbc --output recovered.hix
```

`compile` resolves the same imports, project sources, and dependencies as source execution and
writes an image without executing it. `--entry` overrides the entry stored in the image; otherwise
it stores the project's `applicationMain` or `main`. `run` uses the stored entry unless overridden,
passes arguments as strings, and resolves relative filesystem paths from the image's directory.
The original source files are not required to load or run an image.

`disassemble` and `decompile` write to standard output when `--output` is omitted. `-o` is an alias
for `--output`. Decompiled source can be compiled again with `compile`. `hix --help` lists commands.

Images use a versioned JSON container containing executable bytecode, string and constant pools,
source-line mappings, signatures, lexical scopes, patterns, parameters, markers, and derivations.
Loading checks the format version and validates bytecode through `HixDecompiler`; malformed or
unsupported images return an error. Loading binds host functions against the standalone backend.
`HixImageFile.Write` and `HixImageFile.Read` expose the same storage format to .NET consumers.

The standalone backend also provides JSON conversion through Newtonsoft.Json:

```hix
type Person = @{string name, %optional number age}

func read {
  return(parseJson(param, Person))
}

func save {
  return(writeJson(param, Person))
}
```

`parseJson(string)` and `writeJson(any)` perform untyped conversion. Their two-argument forms accept
either a pattern or a kind and reject values that do not match it. `generateJsonSchema(pattern)` emits
draft 2020-12 JSON Schema, while `loadJsonSchema(string)` imports a schema as a self-contained pattern.

Pattern schemas preserve nested definitions, enums, titles, descriptions, inclusive or exclusive
bounds, and collection-size bounds. The optional second argument to `%min` and `%max` makes the bound
exclusive, for example `%min(0, true) number`. On `%many`, the same constraints apply to item count.
Table fields may declare defaults either as metadata or with an assignment:

```hix
type Settings = %title<Settings> %description<Application settings> @{
  %enum<development><production> string mode = [<development>],
  %min(0, true) number workerCount = [1],
  %default<false> bool verbose
}
```

`parseJson` recursively inserts missing table-field defaults before validating the result. Generated
schemas emit these as `default`, and imported schemas retain and apply them in the same way.

## Imports and files

File imports belong in the metadata header and resolve relative to the file containing them. Imports
are recursive, de-duplicated by absolute path, and compiled before the importing file:

```hix
%import<src/**>
---

func main {
  print(currentPath())
}
```

`%import<*>` imports the current directory, `%import<**>` includes every descendant, and a directory
prefix such as `%import<src/**>` scopes the recursive search. Only `.hix` files are selected. Embedders
can use `HixFileImports.Load(entryFile)` to obtain the same resolved source list as the CLI.

Standalone path and file functions use `/` in returned paths on every platform. Relative filesystem
paths resolve from the entry file's directory. The backend provides path joining, splitting,
normalization and inspection; working-directory and platform separator values; and file reading,
writing, creation, deletion, copying, renaming, existence checks, length queries, and directory listing.

## Analyzer service

`Hix.Standalone.Analysis.HixAnalyzerService` is the stateful, transport-neutral language-service
boundary used by IDE integrations. `Synchronize` accepts a complete directory workspace and
returns immutable snapshots containing declarations, references, diagnostics, lazy completion
sites, and type facts. It owns source-hash/revision caches and cross-file resolution.

`IHixAnalyzerHost` is the optional host-symbol boundary. Rider uses the configured toolchain's
stdio analysis server; its C# backend supplies only host declarations and C# symbol lookup.
The source generator remains independent of `Hix.Standalone`.

Coroutine execution uses `vm.CreateCoroutine(program, context)` and bounded
`coroutine.Resume(instructionBudget)` slices. Dispose abandoned coroutines to release
frames and the context lease. `Invoke` drains the same coroutine synchronously.
`InvokeAsync` only submits an event and returns a `HixTask<HixInvocationResult>`;
it does not use C# async/await.

`HixTask<T>` owns completion, failure, cancellation, and wakeups without .NET tasks.
Its awaiter posts continuations to the awaiting Hix worker's event queue. Use
`.AsTask()` or `.AsHixTask()` explicitly at .NET host boundaries.

By default all submissions enter `HixThread.Current`, the
current OS thread's event queue (or the worker currently executing a callback).
Pump this queue with `Run` or `Tick`; no synchronization-context dispatch or
implicit thread-pool scheduling is performed. Default submissions are pinned to
that worker and cannot be stolen. Unsupported contexts throw on pool submission.
Use `vm.InvokeAsync(worker, program)` to target an explicit single-thread event loop.
Opt into `useThreadPool: true` to use VM-owned dedicated `HixThread` event loops.
`workerCount: 0` (the default) creates no background pool; pool submission throws.
Positive worker counts eagerly start the configured threads, which sleep until notified.
Submissions enter a shared queue and wake an idle worker; busy workers pick them up
when free. Local runnable queues support work stealing. Dispose the VM to stop its
workers and cancel outstanding pooled work. Roslyn contexts
reject worker execution. Override `FunctionDefinition.ExecuteAsync` for a nonblocking
backend operation; waiting coroutines leave the runnable queues until completion.
Use `new HixThread().Enqueue(coroutine)` and `worker.Tick(budget)` for a manually
driven single-threaded queue. Synchronous C# callbacks and derivation coordination
still run to completion within a slice.

## Concurrency functions

Concurrency handles have compiler-only nominal types: `task<T>`, `channel<T>`, and `semaphore`.
`spawn` infers its result from the function/lambda and argument tuple; `awaitTask` preserves that
result type. `awaitAll` preserves known tuple positions, and `awaitAny` returns an index plus the
union of known result types. `channel()` produces `channel<any>`; host-created IR can provide an
explicit element type. All handles still use the single `opaque` runtime kind.

The entrypoint is the main worker on the calling OS thread, independently of the
background pool. `spawn(function, arguments = @[], useThreadPool = false)` creates
an independent coroutine/context and returns an opaque `task<T>` handle.
Functions and lambdas are supported; host-function references are not entrypoints.
The arguments tuple supplies positional arguments and is snapshotted at submission.
Lambdas do not capture the caller's locals: pass shared values explicitly.

```hix
func calculate(number value) -> number {
  yield()
  return(plus($value, 1))
}

func main {
  local first = spawn(calculate, @[41])
  local second = spawn(func => uppercase($0), @[<hello>])
  return(awaitAll(@[local#first, local#second]))
}
```

`awaitTask(task)` returns its value or propagates its error. `awaitAll(tasks)` waits
for all tasks, preserving input order; failures propagate after every task finishes.
An empty tuple completes immediately. `awaitAny(tasks)` returns a table containing
the zero-based `index` and `value` of the first completion; it rejects empty input
and does not cancel the other tasks. `taskDone(task)` checks completion without waiting.

`yield()` yields to the current worker's event queue. `sleep(milliseconds)` uses a
timer to publish completion without blocking the worker; timers do not execute Hix
code. `workerId()` reports the executing OS thread ID. Explicit
`useThreadPool = true` requires a VM constructed with a positive `workerCount`;
otherwise `spawn` returns a Hix error. Main and default-spawned coroutines remain
on the calling worker even when the VM has a pool.

`channel(capacity = 0)` creates a FIFO channel. Zero capacity means rendezvous;
positive capacity buffers that many values. `send(channel, value)` and
`receive(channel)` suspend when no counterpart or buffer space is available.
Values are detached before transfer. `closeChannel(channel)` wakes blocked senders
and receivers with an error and preserves messages already buffered; receiving from
a closed, drained channel returns an error. Closing again returns false.

`semaphore(permits = 1, maximum = 2147483647)` creates a FIFO semaphore.
`acquire(semaphore)` suspends when no permit is available, while `release(semaphore)`
wakes the oldest waiter or increments the permit count. Exceeding the maximum is an
error. Use `semaphore(1, 1)` as a mutex, and explicitly release acquired permits on
all control-flow paths. Handles can be shared between same-worker and pooled tasks.

Synchronous `vm.Invoke(...)` keeps main on the calling thread and pumps its event
queue during waits. For queued entrypoints, use `vm.CurrentThread.RunUntil(vm.InvokeAsync(program))`
to run an event loop until completion, or manage `Run`/`Tick` yourself. Pool size zero
supports all single-threaded concurrency operations without creating background workers.
Pending waits inside synchronous C# host callbacks are rejected rather than blocking
or attempting to preserve a native C# stack. Completed waits remain usable there.

## Host-defined opaque types and generics

Use top-level `meta` declarations for annotation signatures. They are not runtime-callable functions
and do not require an execution handler. List the allowed targets after `for`, for example:

```hix
meta annotation(string text) for function type;
```

Targets are `pattern`, `field`, `file`, `type`, `function`, `mixin`, `statement`, and `variable`.
Targets may be separated by spaces or commas; a terminating semicolon is required.

Host functions declared `pure` have no effects. Ordinary unmarked functions are impure; their effects
are taken from the Hix declaration, not a C# handler-registration flag.

Declare nominal types in the embedded Hix library. Companion Hix declarations supply signatures,
generic parameters, defaults, and documentation; the handler only supplies execution behavior.

```csharp
functions.Add(new SimpleFunction("unbox", (execution, arguments) => ReadBoxPayload(arguments[0])));
functions.AddLibrary("""
  %opaque<my.backend.box>(1)
  type box = opaque;
  %description<Reads the value of a host-owned box.>
  %generic<T>
  pure func unbox(%box<T> value) -> T external;
  """);
```

Use a stable host-qualified identity; display names alone do not establish type identity. Generic
parameters are scoped to their declaring API and may have a bound pattern. Opaque type arguments
are invariant, and conflicting concrete inferences or bound violations are compilation errors.
Unknown values remain `any`; runtime implementations must still validate their opaque payloads.

Programmatic `FunctionDeclarationIr`/IR signatures can declare `TypeParameters`, and
`CallExpressionIr.TypeArguments` supplies explicit type arguments. Compile these through
`HixCompiler.CompileFunctions(IEnumerable<CompilationUnitIr>, backend)`. Generic bodies emit once
with bounds/`any` and opaque types erased; declaration identity metadata preserves static overload
binding when signatures erase identically. `%computedGeneric<T></invoke/function/arguments/result>`
infers a callback result using its named positional argument tuple.
`%computedGeneric<Results></arguments/tasks/*/typeArguments/0>` projects type argument zero
of each opaque handle into an ordered result tuple. A second declaration,
`%computedGeneric<T></generics/Results/union>`, selects their result union.
`/arguments/target/pattern` selects a represented pattern schema; adding `/data` selects trait
component data. Paths use JSON Pointer escapes (`~0` and `~1`) and support field, callable,
collection, and opaque type argument selectors. These compiler declarations replace the
specialized inference metadata; C# handlers only implement execution.
Opaque types have no JSON/JSON Schema representation.

### Metadata-based generic types

Use `external` instead of a body for signature-only declarations:

```hix
%generic<T>
func deferred(T value) -> T external;
func renamed(number value) -> number external hostOperation;
```

These declarations retain metadata and signature validation but emit no executable function.
The trailing semicolon is optional. Binding them to host implementations is a future integration;
an external declaration alone is not callable at runtime.
An optional identifier after `external` selects the host entry name. Without it, the declared
function name is used; the IR preserves the explicit override as `ExternalName`.

Generic definitions are compiler-only pattern constructors, not patterns themselves. Registered
opaque definitions automatically provide constructors; hosts may also register structural constructors
with `DefinePatternConstructor(name, arity, implementation, documentation)`.

```hix
%generic<T>
func identity(T value) -> T { return($value) }

%generic<T>(union(number, string))
func boundedIdentity(T value) -> T { return($value) }

%generic<T>
func taskResult(%task<T> task) -> T { return(awaitTask($task)) }

type TextList = %many<string>;
type NestedTextList = %many(many(string));
type OtherNestedTextList = %many(many<string>);
```

`%generic<T>` introduces `T` only within the annotated function's signatures and body. Repeated
declarations introduce parameters in metadata order. Bounds follow the name and must all hold;
`union(...)` expresses alternatives. Nested construction uses ordinary call expressions in metadata,
not nested angle brackets within a string argument. Calls infer type arguments; explicit runtime-call
type arguments remain programmatic IR only. Constructed types and generic declarations emit no
runtime type environments or specialized code.

## HAL graph editing

Run `./gradlew runHalGraphEditor` from the repository root to build the standalone
toolchain and serve the XYFlow editor from `src/Hix.GraphEditor/example` by default.
Use `-PHalGraphRoot=/path/to/workspace` to select another workspace. See [the editor guide](../Hix.GraphEditor/README.md)
for the example workspace, graph semantics, and revisioned HTTP/stdio analysis protocol.

### Persistent workspace asset database

`AssetDatabase.Open(assetOrWorkspacePath)` maintains `.hix/assets.duckdb` in the
uppermost associated workspace. Association follows ancestor manifests that own the
nearest workspace through modules or path dependencies; an unrelated nested manifest
keeps its own database. Types still come from the nearest workspace and its declared
dependencies. Without a manifest, the containing directory is the workspace.
The database uses `DuckDB.NET.Data.Full` (including the native full bindings), pinned
to 1.4.1 for .NET Standard 2.0 compatibility. `.hix` is disposable generated data and
is excluded from discovery and version control. Any schema version mismatch (older or
newer) clears the entire database and regenerates the cache. There are no migrations.

```hal
%uid("world.watchtower")
%tag("location")
%tag("quest")
--- Location &tower
name = "Watchtower"

// In another asset:
// location = asset(uid("world.watchtower"))
// location = asset(path("locations/watchtower.hal"), "tower")
```

`%uid("...")` is optional primary-section metadata. UIDs must be nonempty and unique
within the indexed workspace; duplicate declarations fail indexing. Declared UIDs
preserve identity across file moves. Assets without a UID receive a database-local
identity associated with their path. `asset(path("relative.hal"))` and
`asset(uid("..."))` select the primary section; an optional second argument selects
an anchor. Paths are relative to the referencing file, confined to the associated
workspace, and cannot traverse symbolic links. `ref("anchor")` stays local.

`%tag("...")` is repeatable primary-section metadata. Each occurrence contributes one
nonempty, case-sensitive tag; duplicates collapse to one membership. Tags are shared
rows in `tags`, with an indexed `asset_tags` join table. `Query(tag: "quest")` filters
assets in SQL; `Tags(assetId)` lists their tags. Updating valid source replaces the
memberships, and deleted assets disappear from tag queries.

The database API separates `Refresh()` filesystem reconciliation, indexed/paginated
`Query(type, offset, limit, tag)`, `Find(pathOrId)`, `Analyze()`, `Document(path)`,
`Symbols(name)`, `Dependencies(path, reverse)`, and `Build(path, processorVersion)`.
Builds resolve references recursively, apply Hix defaults/tags, validate types, detect
cycles, and cache all section values. Fingerprints include source content, compiler
identity, workspace type inputs, processor version, and dependency fingerprints.
Successful values and dependency records publish in one transaction. Failed builds
retain the previous artifact internally but throw rather than serving it as current.
Deleted assets disappear from queries; invalid source retains last-known objects with
an exposed diagnostic. Query/dependency results describe the last indexed/built state;
call `Refresh` or `Build` to reconcile external changes. UID resolution reconciles the
index before lookup so moved files are found.

The SQL layout uses `workspaces(id, path, fingerprint)` and `files(id, path)` to store
paths once. `documents` relates each file to its analysis workspace. Symbols, references,
diagnostics, and type facts refer to document IDs; reference targets and asset dependency
edges refer to file IDs. Stable `asset_identities` anchor objects, builds, and tags even
when an asset changes its file binding or removes its declared UID.

Each pattern is a row in `patterns(workspace_id, name, kind, display, definition)`.
The definition column contains only that pattern's canonical AST, not a workspace-wide
object. Name and kind are indexed SQL columns. `Patterns(name, kind, offset, limit)`
queries the last indexed set without parsing sources or loading other patterns; call
`Analyze()` when filesystem changes need to be reconciled. Document snapshots are
reconstructed from their relational rows rather than stored as duplicate JSON blobs.

Canonical pattern definitions, source diagnostics, document type facts, declarations,
and symbol locations persist across process restarts. Warm analysis hashes inputs and
loads saved analysis without reparsing Hix. Type-input changes conservatively invalidate
the nearest workspace's analysis; unrelated asset edits do not. Queries are paginated
and indexed. Full reconciliation still walks files, and cold analysis still constructs
the workspace semantic model; this is not a claim of constant-time project loading.
Editor buffers remain revisioned in memory and never overwrite disk index entries
until saved. The graph editor uses the persistent analysis automatically.

The analysis server exposes `assets/index`, `assets/query`, `assets/build`,
`assets/dependencies`, `assets/analyze`, `assets/document`, `assets/symbols`,
`assets/patterns`, and `assets/tags` over
stdio and HTTP. Supply `path` (asset or workspace), with `type`, `offset`, `limit`,
`reverse`, `name`, `kind`, `tag`, `assetId`, or `processorVersion` as appropriate. `assets/build` returns
`assetId`, `fingerprint`, `cacheHit`, and serialized full section values.
`hix/get` falls back to persisted disk analysis when there is no open buffer.
Use one server process per database for writes; DuckDB's process lock prevents
concurrent independent writers. Dispose `AssetDatabase` on the opening thread.

Rider requires a Hix toolchain executable in HELIX project settings. Browse to it or use
**Detect from PATH** (Linux: `hix` / `Hix.Toolchain`; Windows: `hix.exe` / `Hix.Toolchain.exe`).
Analysis, completion, formatting, and run/test execution use that executable. There is no
bundled analyzer fallback. Each project owns a stateful `analysis-server` process; changing
settings restarts it and invalidates frontend snapshots. C# interop alone uses Rider's RD
backend: it exports host library declarations and resolves/completes C# types. The external
compiler analyzes those declarations; the plugin does not parse or compile Hix in its backend.
Saved workspace analysis creates the uppermost associated `.hix/assets.duckdb` cache during
`hix/synchronize`; unsaved text stays in memory.

`:riderPlugin:runIde` always depends on `buildHixToolchainAot`. On Linux,
`installDevelopmentToolchain` copies the executable as `~/.local/bin/hix` and its runtime
libraries beside it. Add `~/.local/bin` to PATH before starting Rider to use autodetection.
On Windows, select the executable under `build/hix-toolchain/aot`.

`:riderPlugin:testDotNet` builds the toolchain and runs the C# interop tests against it
(using the modern .NET test host on Linux). To select an existing executable manually:
`HIX_TOOLCHAIN=/absolute/path/to/hix dotnet test src/RiderPlugin/src/dotnet/HelixRider.Tests/HelixRider.Tests.csproj -p:RiderTestFramework=net8.0`.
The modern-host override excludes the Windows STA/PSI test environment; it does not replace
interactive Rider QA.
