# Hix Roslyn backend

`HixRoslynBackend` is a non-sealed semantic host, not a C# transpiler. Create a context with
`backend.CreateContext(compilation, currentType, target, attribute)` and compile with that backend.
Semantic host roots require a prelude. The assembly references Hix and Roslyn, never the generator.

`collectAnnotatedTypes(<Namespace.AttributeType>)` returns a tuple of exactly annotated types in
the current compilation, including nested types. Referenced assemblies and derived attributes
are excluded. Partial declarations and repeated annotations do not duplicate entries. Results are
cached per compilation and ordered by fully qualified name. `namespace`, `accessible`, and `generic`
can filter the results. HELIX module discovery uses these functions in the Context mixin library.
