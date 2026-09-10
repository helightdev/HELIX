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
var result = new HixVM(new[] { program }).Invoke(program, backend.CreateThread());
```

Execution remains bytecode-only. `Program.Run` returns 0 for success, 1 for
compilation/runtime/I/O failure, and 2 for usage errors. Errors are written to the supplied error
writer. Roslyn functions and roots are unavailable in this backend.

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
