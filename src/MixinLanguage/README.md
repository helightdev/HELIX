# HELIX Mixin Language

This project contains the reusable mixin parser, compiler, virtual machine, table/text functions, additional-file catalog, and Roslyn semantic execution support.

`Helix.MixinLanguage.csproj` is a normal .NET class library for hosts such as the Rider plugin. It is deliberately not referenced by the source-generator project. Instead, `HelixSourceGenerator.csproj` links every source file below this directory with a `Compile` item, so the deployed analyzer remains a single self-contained `HelixSourceGenerator.dll`.

When adding shared implementation files, place them below `src/MixinLanguage/Language` or `src/MixinLanguage/Roslyn`. Generator entry points and incremental-generator orchestration remain in the source-generator project's `Generators` directory.
