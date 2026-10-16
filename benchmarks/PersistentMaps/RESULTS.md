# Local benchmark sample

Recorded on 2026-09-08: .NET 8.0.30, openSUSE Tumbleweed, x64, Release, tiered compilation disabled.

The correctness harness passed before timing. See [results.csv](results.csv) for all results and [README.md](README.md) for commands and methodology.

## Representative string-key results

| Entries | Operation | Flat ns/op | HAMT ns/op | Hybrid ns/op | ImmutableDictionary ns/op |
| --- | --- | ---: | ---: | ---: | ---: |
| 16 | lookup | 33.5 | 14.4 | 33.6 | 16.8 |
| 16 | lookup-miss | 58.6 | 9.2 | 60.1 | 10.0 |
| 16 | update | 78.9 | 133.6 | 78.7 | 146.1 |
| 16 | remove+insert | 168.0 | 154.6 | 168.5 | 312.3 |
| 16 | enumerate | 3.8 | 16.9 | 3.7 | 65.3 |
| 16 | build | 28.1 | 98.8 | 28.5 | 89.3 |
| 1024 | lookup | 2077.4 | 29.4 | 30.0 | 51.5 |
| 1024 | lookup-miss | 2942.1 | 19.6 | 19.7 | 39.0 |
| 1024 | update | 2456.2 | 225.3 | 229.4 | 411.2 |
| 1024 | remove+insert | 5812.0 | 374.4 | 400.8 | 892.8 |
| 1024 | enumerate | 2.6 | 20.4 | 21.7 | 58.7 |
| 1024 | build | 20.3 | 118.1 | 130.4 | 218.0 |

## Allocation tradeoffs

| Entries | Operation | Flat B/op | HAMT B/op | ImmutableDictionary B/op |
| --- | --- | ---: | ---: | ---: |
| 16 | update | 144.0 | 315.0 | 255.9 |
| 16 | enumerate | 0.0 | 9.0 | 0.0 |
| 16 | build | 82.5 | 306.0 | 74.5 |
| 1024 | update | 4176.0 | 710.1 | 627.7 |
| 1024 | enumerate | 0.0 | 0.1 | 0.0 |
| 1024 | build | 112.0 | 359.7 | 64.2 |

Flat storage performs well for small updates, construction, and enumeration; hash lookup wins earlier for strings. The threshold of 16 favors compact small records rather than minimum lookup latency.

The HAMT is faster than ImmutableDictionary for these sampled large-map lookups and updates, but allocates more per update and substantially more during construction. Mutable Dictionary lookup remains faster; Dictionary-copy is a persistence baseline rather than an optimized immutable collection.

These are local microbenchmarks, not a claim of universal superiority or a measured Hix application speedup. Builder internals and leaf layout remain candidates for further allocation tuning.
