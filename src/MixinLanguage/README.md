# HELIX Mixin Language

This project contains the reusable mixin parser, compiler, virtual machine, table/text functions, additional-file catalog, and Roslyn semantic execution support.

`Helix.MixinLanguage.csproj` is a normal .NET Standard 2.0 class library for hosts such as the Rider plugin. It is deliberately not referenced by the source-generator project. Instead, `HelixSourceGenerator.csproj` links every source file below this directory with a `Compile` item, so the deployed analyzer remains a single self-contained `HelixSourceGenerator.dll`. Keeping the standalone project on the same target framework also prevents shared code from accidentally using APIs unavailable to the analyzer host; its `IsExternalInit` compatibility shim is therefore shared as well.

When adding shared implementation files, place them below `src/MixinLanguage/Language` or `src/MixinLanguage/Roslyn`. Generator entry points and incremental-generator orchestration remain in the source-generator project's `Generators` directory.

## Environment boundary

Host-independent debug rendering lives in `Debug`. Host policy lives behind the internal environment contracts used for profiling and compiled-library caching.

The standalone project compiles `Environment/Default`, whose implementations are deliberately safe defaults:

- profiling is disabled and never writes files;
- `CompileCached` performs a fresh deterministic compilation and leaves caching to its host.

The source-generator project excludes `Environment/Default/**/*.cs` from its linked files and supplies matching implementations in its own `Environment` directory. Those implementations retain the process-wide incremental-generator cache and the opt-in Unity profiling report.

A future host can follow the same pattern: exclude the default environment files when source-linking and provide classes with the same internal contracts. A normal project reference uses the standalone defaults automatically.
