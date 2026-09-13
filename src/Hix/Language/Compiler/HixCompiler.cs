using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Compiler.Steps;
using Hix.Env;
using Hix.Runtime;

namespace Hix.Compiler;

/// <summary>Catalog binding and preparation for the generated-ANTLR semantic model.</summary>
public static class HixCompiler {
  public static IReadOnlyList<HixCompilerStep> DefaultSteps { get; } = Array.AsReadOnly<HixCompilerStep>([
    new LambdaLiftingStep(), new SignatureParameterBindingStep(), new PatternBindingStep(), new InlineExpansionStep(),
    new PreludeHoistingStep(), new PatternBindingStep(), new NameValidationStep()
  ]);
  public static HixCompilerCatalog PrepareGlobals(IEnumerable<string> sources, HixBackend backend = null) =>
    PrepareGlobals(sources.Select(source => AntlrSyntax.Parse(source, backend)), backend);

  public static HixCompilerCatalog PrepareGlobals(IEnumerable<CompilationUnitIr> units, HixBackend backend = null) {
    using var profile = HixCompilerProfiler.Measure("compiler.prepare_globals");
    backend ??= HixCoreBackend.Instance;
    var syntax = units.ToArray();
    var declaredPatterns = new HashSet<string>(backend.Functions.Types.Concat(DeclarationExports.Expand(syntax.SelectMany(unit => unit.Declarations)).OfType<TypeDeclarationIr>()).Select(type => type.Name), StringComparer.Ordinal);
    var errors = syntax.SelectMany(unit => unit.Diagnostics)
      .Where(diagnostic => !ResolvedPatternDiagnostic(diagnostic.Message, declaredPatterns)).ToArray();
    if (errors.Length != 0) throw new ArgumentException(errors[0].Message);
    var declarations = DeclarationExports.Expand(syntax.SelectMany(unit => unit.Declarations)).Select(declaration =>
      declaration is FunctionDeclarationIr function ? SignatureParameterBindingStep.Rewrite(function) : declaration).ToArray();
    var diagnostics = new List<HixParseDiagnostic>();
    LanguageValidation.Validate(declarations, diagnostics, backend);
    if (declarations.OfType<FunctionDeclarationIr>().Any(function => function.TypeParameters.Count != 0 || function.Signatures.Any(signature => signature.TypeParameters.Count != 0)))
      PatternBinding.Validate(declarations, declarations.OfType<TypeDeclarationIr>().ToDictionary(type => type.Name,
        type => type.Pattern, StringComparer.Ordinal), diagnostics, backend);
    if (diagnostics.Count != 0) throw new ArgumentException(diagnostics[0].Message);
    var strings = new HixStringPoolBuilder();
    foreach (var unit in syntax)
      foreach (var token in unit.Tokens) strings.Intern(token.Text);
    var functions = LambdaLiftingStep.RewriteFunctions(declarations.OfType<FunctionDeclarationIr>().ToArray())
      .Select(SignatureParameterBindingStep.Rewrite).ToArray();
    var derivations = declarations.OfType<MixinDeclarationIr>().Where(mixin => mixin.IsDerivation).ToArray();
    var patterns = backend.Functions.Types.Concat(declarations.OfType<TypeDeclarationIr>()).GroupBy(type => type.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.Last().Pattern, StringComparer.Ordinal);
    functions = functions.Concat(patterns.SelectMany(item =>
      PatternConstructorCompiler.Compile(item.Key, item.Value, patterns))).ToArray();
    // The functions are the module being prepared here. Supplying them as catalog imports as well
    // duplicates every overload and prevents unique static binding (notably named-argument ordering).
    var metadataDefinitions = declarations.OfType<MetadataDeclarationIr>().ToArray();
    var metadata = syntax.SelectMany(unit => unit.Metadata).ToArray();
    var types = declarations.OfType<TypeDeclarationIr>().ToArray();
    var prepared = new HixCompilerCatalog(strings.Freeze(), [], derivations, backend, patterns, metadataDefinitions, metadata, types);
    var compiled = new HixModuleIr([], [], functions);
    compiled = RunSteps(compiled, prepared);
    prepared = new HixCompilerCatalog(prepared.StringPool, compiled.Functions, derivations, backend, patterns, metadataDefinitions, metadata, types);
    var compiledDerivations = derivations.Select(derivation => {
      var body = PrepareIr(derivation, prepared);
      var declarations = body.Prelude.Cast<HixIrNode>().Concat(body.Late).Concat(body.Functions).ToArray();
      return new MixinDeclarationIr(derivation.Name, true, declarations, derivation.Metadata, derivation.Parameters) {
        SourceRange = derivation.SourceRange,
        Tokens = derivation.Tokens
      };
    }).ToArray();
    return new HixCompilerCatalog(prepared.StringPool, compiled.Functions, compiledDerivations, backend, patterns, metadataDefinitions, metadata, types);
  }

  private static bool ResolvedPatternDiagnostic(string message, ISet<string> patterns) {
    const string prefix = "unknown pattern '";
    if (message == null || !message.StartsWith(prefix, StringComparison.Ordinal) || !message.EndsWith("'", StringComparison.Ordinal))
      return false;
    return patterns.Contains(message.Substring(prefix.Length, message.Length - prefix.Length - 1));
  }

  /// <summary>Compile global functions directly to an executable bytecode image.</summary>
  public static HixProgramImage CompileFunctions(IEnumerable<string> sources, HixBackend backend = null) {
    var globals = PrepareGlobals(sources, backend);
    return CreateProgram([], [], globals);
  }

  /// <summary>Compiles host-constructed IR, including signatures not exposed by the Hix grammar.</summary>
  public static HixProgramImage CompileFunctions(IEnumerable<CompilationUnitIr> units, HixBackend backend = null) =>
    CreateProgram([], [], PrepareGlobals(units, backend));

  public static HixProgramImage Prepare(MixinDeclarationIr declaration,
    HixCompilerCatalog globals, bool? prelude) {
    var compiled = PrepareIr(declaration, globals);
    var expressions = prelude == true ? compiled.Prelude : prelude == false ? compiled.Late
      : compiled.Prelude.Concat(compiled.Late).ToArray();
    return CreateProgram(expressions, compiled.Functions, globals, compiled.Parameters, declaration.Metadata);
  }

  public static HixProgramImage Compile(string source, string mixinName, HixBackend backend = null) =>
    Compile(AntlrSyntax.Parse(source, backend), mixinName, backend);

  public static HixProgramImage Compile(CompilationUnitIr unit, string mixinName, HixBackend backend = null) {
    if (unit == null) throw new ArgumentNullException(nameof(unit));
    if (unit.Diagnostics.Count != 0) throw new ArgumentException(unit.Diagnostics[0].Message);
    var declarations = unit.Declarations.OfType<MixinDeclarationIr>().Where(mixin => mixin.Name == mixinName).ToArray();
    if (declarations.Length != 1) throw new ArgumentException("expected exactly one mixin named '" + mixinName + "'");
    return Prepare(declarations[0], PrepareGlobals(new[] {unit}, backend), null);
  }

  public static (HixProgramImage Prelude, HixProgramImage Late) PreparePrograms(
    MixinDeclarationIr declaration, HixCompilerCatalog globals) {
    using var profile = HixCompilerProfiler.Measure("compiler.prepare_programs");
    var compiled = PrepareIr(declaration, globals);
    var program = CreateProgram(compiled.Prelude.Concat(compiled.Late).ToArray(), compiled.Functions, globals, compiled.Parameters, declaration.Metadata);
    return (program.ForPass(true), program.ForPass(false));
  }

  private static HixProgramImage CreateProgram(IReadOnlyList<ExpressionDeclarationIr> expressions,
    IReadOnlyList<FunctionDeclarationIr> functions, HixCompilerCatalog globals,
    IReadOnlyList<SignatureField> parameters = null, IReadOnlyList<MetadataIr> mixinMetadata = null) {
    using var profile = HixCompilerProfiler.Measure("compiler.bytecode");
    return new HixBytecodeCompiler(globals.StringPool).Compile(expressions, functions, globals, parameters, mixinMetadata);
  }

  public static HixModuleIr PrepareIr(MixinDeclarationIr declaration, HixCompilerCatalog globals) {
    var exported = DeclarationExports.Expand(declaration.Declarations);
    var expressions = exported.OfType<ExpressionDeclarationIr>().ToArray();
    var syntax = new HixModuleIr(expressions.Where(expression => expression.IsPrelude).ToArray(),
      expressions.Where(expression => !expression.IsPrelude).ToArray(),
      exported.OfType<FunctionDeclarationIr>().ToArray(), declaration.Parameters);
    return RunSteps(syntax, globals);
  }

  private static HixModuleIr RunSteps(HixModuleIr module, HixCompilerCatalog catalog) {
    var compilation = new HixCompilation(module, catalog);
    foreach (var step in catalog.Backend.CompilerSteps) {
      using var stepProfile = HixCompilerProfiler.Measure("compiler.step." + step.GetType().Name);
      step.Run(compilation);
    }
    if (compilation.Diagnostics.Count != 0) throw new ArgumentException("line " + compilation.Diagnostics[0].Line + ": " + compilation.Diagnostics[0].Message);
    return compilation.Module;
  }
}
