# Source generation benchmark

Run from the repository root:

```sh
DOTNET_TieredCompilation=0 dotnet run --project benchmarks/SourceGeneration -c Release -- "$PWD"
```

Generates 71 structures with four fields each using the repository's Core mixins.
Each iteration creates a fresh compilation and generator driver. The first run includes
cold initialization; compare the remaining five runs. Checks generated member presence
and output compilation diagnostics outside the timed region. Allocations are measured
on the calling thread. This fixture does not represent every Unity generation path.

Set `HELIX_MIXIN_PROFILE_PATH` to a TSV path for diagnostic timings; leave profiling
disabled for comparisons. Profile snapshots are cumulative, so do not sum snapshots.

2026-09-08 local Release comparison, tiered compilation disabled, profiling disabled:

| VM image loading | Warm median | Approximate allocated bytes/run |
| --- | ---: | ---: |
| Reload each execution | 70.837 ms | 32,670,000 |
| Cache by bytecode and seed pool identity | 52.311 ms | 27,500,000 |

Baseline used a temporary source copy with only loaded-image cache selection bypassed.
Both versions include the argument conversion allocation optimization. Five warm runs
per version: approximately 26% less elapsed time and 16% fewer allocated bytes.
These numbers are illustrative local measurements, not a statistical performance guarantee.
