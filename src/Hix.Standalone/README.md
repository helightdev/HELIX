# Hix standalone

The .NET 8 executable accepts `hix <file> [--entry <name>] [-- <arguments...>]`.
Run from source with `dotnet run --project src/Hix.Standalone -- file.hix -- hello`.

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
var result = new HixVM(new[] { program }).Invoke(program, backend.CreateContext());
```

Execution remains bytecode-only. Exit codes are 0 for success, 1 for compilation/runtime/I/O
failure, and 2 for CLI usage errors. Errors are written to stderr. Roslyn functions and roots are
unavailable in this backend.
