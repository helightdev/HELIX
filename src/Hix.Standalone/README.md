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

`IHixAnalyzerHost` is the optional host-symbol boundary used for integrations such as Rider's C#
type index. The source generator does not reference or contain `Hix.Standalone`; its dependency
direction remains independent. Rider currently links only the analyzer implementation sources
into its backend because the generator embeds Hix types. The same implementation is part of the
standalone assembly and can later be exposed through JSON-RPC/LSP without relocating analysis
back into an IDE plugin.

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

The entrypoint is the main worker on the calling OS thread, independently of the
background pool. `spawn(function, arguments = @[], useThreadPool = false)` creates
an independent coroutine/context and returns an opaque `symbol` task handle.
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
