# Persistent map smoke benchmark

This executable links the production `PersistentMap.cs` directly. It uses ordinary C# `int` and
`string` keys and `int` values, with no VM, Roslyn, or mixin-value overhead and no benchmark packages.

Run from the repository root:

```sh
DOTNET_TieredCompilation=0 dotnet run --project benchmarks/PersistentMaps -c Release
# Correctness checks without timings:
dotnet run --project benchmarks/PersistentMaps -c Release -- --verify-only
```

Before timing, the program checks randomized updates against `Dictionary`, old snapshots and forks,
full-hash collisions, shared low hash bits, deletion to empty, custom comparers, bulk construction,
builder isolation, and no-op updates. Failures terminate with a nonzero exit code.

Each timed workload gets two warmups and five measured samples. The output reports median
nanoseconds and allocated bytes per operation. Lookup/update setup is outside the timed region;
build workloads include builder construction and publication. Enumeration is reported per entry,
build per inserted entry, and remove+insert per pair. Results are consumed to prevent elimination.
Tiered compilation is disabled in the command above to reduce compilation changes between samples.
String hash seeds still vary across processes, and timings depend on the machine and GC state.
This is a small comparative benchmark, not a substitute for profiling real Hix programs.

Representations:

- `flat`: always use shared key arrays and copy value arrays on updates.
- `hamt`: always use the bitmap HAMT for nonempty maps.
- `hybrid`: flat through 16 entries, then promote to a HAMT. Nonempty maps do not demote.
- `ImmutableDictionary`: .NET's persistent dictionary; construction uses its builder.
- `Dictionary`: mutable dictionary lookup baseline.
- `Dictionary-copy`: copy the dictionary before every update, preserving previous versions.

Workloads cover hits, misses, replacement, removal/reinsertion, enumeration, and bulk construction
at 1, 4, 8, 16, 32, 128, and 1024 entries. Correctness tests cover collisions; benchmark timings use
normal key hashes. Allocations do not measure retained memory when many versions stay alive.

See `results.csv` and `RESULTS.md` for a local sample. The HAMT is a conventional bitmap-node
implementation, not a fully tuned CHAMP. Its leaf objects and arrays still have room for allocation
improvements. The threshold of 16 is an initial tradeoff, not a universal optimum.
