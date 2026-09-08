# ANTLR migration status

This migration is **in progress**, not a completed source-generator replacement. The root grammars were not changed.

## Implemented and tested

- ANTLR 4.13.2 generated C# and Java sources, with explicit regeneration instructions beside each generated target.
- Vendored C# runtime integration without duplicate assembly metadata. Upstream runtime analyzer suppressions are scoped to the vendored directory.
- `AntlrSyntax.Parse` builds a nested semantic declaration/statement/value tree and rejects execution of recovery trees with diagnostics. Its canonical token stream includes skipped source gaps and preserves complete UTF-16 source coverage.
- The `MixinVirtualMachine.Execute(CompilationUnitAst, ...)` entry point exercises the new typed execution implementation: tuples, tables, numbers, interpolation, signatures, private call locals, selection, bounded block control flow, explicit preludes, and checked callee rollback.
- Expression transactions preserve prior successful commits, discard failed writes/output, and connect target storage to the host context. Checked function failures also roll back invalid cross-function control flow. Checked errors retain their kind through snapshot export/import.
- Function values retain lexical declaration environments without caller-local capture. Mixin-local overloads shadow matching signatures while preserving other global overloads.
- The new AST execution entry point runs tuple-based derivation providers in declaration order, preserves additional record fields, requires explicit returns, shares locals between a provider's expression blocks, and rolls back failed derivations.
- The production additional-file loader, compiler, and VM now use the generated parser and canonical declaration tree. The handwritten C# lexer/parser, flat directive AST, opcode interpreter, and legacy lowering passes have been removed.
- All five HELIX libraries use the new grammar, including native `when` guards/value selection, content/value tails, and tuple/table literals. They parse successfully; this does not yet establish generated C# correctness in Unity.
- C# conformance tests cover the new parse and execution path. After production cutover, 18 parameterless execution tests pass through a standalone harness against the rebuilt generator, including lazy `when` selection and independent guards. The full suite currently does not compile because old fixtures still reference removed APIs; migrate their feature coverage rather than removing it.
- Rider uses generated Java recognition instead of the handwritten Kotlin syntax port. Token coverage preserves skipped whitespace and UTF-16 offsets. PSI construction, syntax diagnostics, highlighting, basic keyword/declaration completion, function navigation, and delimiter handling use the new frontend.
- Nine Rider frontend tests pass. This does not establish successful loading or behavior in a running Rider instance.
- The Rider C# backend now builds against `LanguageAnalysis`, a semantic index over the canonical ANTLR tree, with no legacy editor parser dependency.
- Generator-host caches cover unchanged additional-file content and multiple prepared catalogs. Global, local, and derivation function scopes and overload candidates are prepared once. Statements and expressions are lowered into immutable operation delegates shared across the catalog; execution no longer dispatches over AST node types. Dynamic overload selection and core builtin dispatch remain at runtime. This is not yet a measured resolution of the reported 400 ms latency.
- Lowered member access reads keyed storage directly instead of materializing entire storage tables. Target-storage keys survive execution across different prepared string pools. The existing 18 parameterless execution checks pass against the lowered runtime, and Unity compilation remains successful.
- The deployed new-language generator and migrated HELIX libraries compile successfully in the running Unity editor (`completed`, `failed: false`, `errors: []`).

## Required before the migration is complete

1. Migrate remaining old API consumers in tests and the Rider C# backend. Do not restore compatibility parsers or directive ASTs.
2. Audit remaining host signatures against their actual scalar/symbol inputs. Core, numeric, string, collection, kind, and host implementations now use function definitions; preparation binds overload sets, and execution uses their flat signatures and effect metadata. The inherited value-kind enum and separate predicate-function category are removed.
3. Implement dependency-preserving inlining/hoisting, strict-expression validation, persistent call-site carry identities, and prepared snapshot/cache integration. Prelude-closing failure must invalidate the entire prepared application. Current execution tests establish only a subset of these contracts.
4. Finish C# host integration: callable descriptors, transactional target aliases/configuration, and semantic detachment. Typed constant unwrapping, tuple-valued host collections, and production-catalog derivation binding are implemented but need broader production integration tests.
5. Verify migrated HELIX library semantics and generated C# against the new production entry points. Rewrite obsolete syntax and callback shapes rather than adding compatibility functions.
6. Finish Rider semantic services against the new language model: builtin/host completion metadata, scoped variable and label references, cross-file resolution, and new C# backend snapshots. The obsolete backend diagnostics/catalog are intentionally not used by the new frontend annotator/completion provider.
7. Keep Unity compilation green while completing compiler execution preparation and Rider semantic integration. The migrated generator is deployed and the latest editor compilation succeeds.

## Validation commands

```sh
DOTNET_CLI_HOME=/tmp/helix-dotnet dotnet test src/HelixSourceGenerator/HelixSourceGenerator/HelixSourceGenerator.Tests/HelixSourceGenerator.Tests.csproj --no-restore -p:SkipCopyDll=true
./gradlew --no-configuration-cache :riderPlugin:test --tests 'dev.helight.helix.mixin.*' -x :riderPlugin:compileDotNet
git diff --check
```

The Gradle command tests the frontend without rebuilding the old C# plugin backend. `SkipCopyDll=true` prevents deploying an unvalidated generator during compiler development.
