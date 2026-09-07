# Generated Mixin grammar

These files are generated from the repository-root `MixinLexer.g4` and `MixinParser.g4` with ANTLR 4.13.2, matching the vendored runtime. Do not edit generated C# files.

From the repository root, regenerate with:

```sh
java -jar /path/to/antlr-4.13.2-complete.jar -Dlanguage=CSharp -package Mixins.Compiler.Generated -visitor -no-listener -o src/MixinLanguage/Language/Compiler/Generated MixinLexer.g4 MixinParser.g4
```

Generation is explicit: normal builds neither download tools nor require Java.
