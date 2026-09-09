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
    new PreludeHoistingStep(), new FunctionBindingStep()
  ]);
  public static HixExpressionPreparedState PrepareGlobals(IEnumerable<string> sources, HixBackend backend = null) =>
    PrepareGlobals(sources.Select(source => AntlrSyntax.Parse(source, backend)), backend);

  public static HixExpressionPreparedState PrepareGlobals(IEnumerable<CompilationUnitAst> units, HixBackend backend = null) {
    using var profile = HixCompilerProfiler.Measure("compiler.prepare_globals");
    backend ??= HixCoreBackend.Instance;
    var syntax = units.ToArray();
    var declaredPatterns = new HashSet<string>(syntax.SelectMany(unit => unit.Declarations).OfType<TypeDeclarationAst>()
      .Select(type => type.Name), StringComparer.Ordinal);
    var errors = syntax.SelectMany(unit => unit.Diagnostics)
      .Where(diagnostic => !ResolvedPatternDiagnostic(diagnostic.Message, declaredPatterns)).ToArray();
    if (errors.Length != 0) throw new ArgumentException(errors[0].Message);
    var declarations = syntax.SelectMany(unit => unit.Declarations).Select(declaration =>
      declaration is FunctionDeclarationAst function ? SignatureParameterBindingStep.Rewrite(function) : declaration).ToArray();
    var diagnostics = new List<HixParseDiagnostic>();
    LanguageValidation.Validate(declarations, diagnostics, backend);
    if (diagnostics.Count != 0) throw new ArgumentException(diagnostics[0].Message);
    var strings = new HixStringPoolBuilder();
    foreach (var unit in syntax)
      foreach (var token in unit.Tokens) strings.Intern(token.Text);
    var functions = LambdaLiftingStep.RewriteFunctions(declarations.OfType<FunctionDeclarationAst>().ToArray())
      .Select(SignatureParameterBindingStep.Rewrite).ToArray();
    var derivations = declarations.OfType<MixinDeclarationAst>().Where(mixin => mixin.IsDerivation).ToArray();
    var patterns = declarations.OfType<TypeDeclarationAst>().GroupBy(type => type.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.First().Pattern, StringComparer.Ordinal);
    var prepared = new HixExpressionPreparedState(strings.Freeze(), functions, derivations, backend, patterns);
    var compiled = new HixCompilerSyntax([], [], functions);
    foreach (var step in backend.CompilerSteps) {
      using var stepProfile = HixCompilerProfiler.Measure("compiler.step." + step.GetType().Name);
      compiled = step.Transform(compiled, prepared);
    }
    prepared = new HixExpressionPreparedState(prepared.StringPool, compiled.Functions, derivations, backend, patterns);
    var compiledDerivations = derivations.Select(derivation => {
      var body = PrepareSyntax(derivation, prepared);
      var declarations = body.Prelude.Cast<HixAst>().Concat(body.Late).Concat(body.Functions).ToArray();
      return new MixinDeclarationAst(derivation.Name, true, declarations, derivation.Metadata) {
        SourceRange = derivation.SourceRange,
        Tokens = derivation.Tokens
      };
    }).ToArray();
    return new HixExpressionPreparedState(prepared.StringPool, compiled.Functions, compiledDerivations, backend, patterns);
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

  public static HixProgramImage Prepare(MixinDeclarationAst declaration,
    HixExpressionPreparedState globals, bool? prelude) {
    var compiled = PrepareSyntax(declaration, globals);
    var expressions = prelude == true ? compiled.Prelude : prelude == false ? compiled.Late
      : compiled.Prelude.Concat(compiled.Late).ToArray();
    return CreateProgram(expressions, compiled.Functions, globals);
  }

  public static HixProgramImage Compile(string source, string mixinName, HixBackend backend = null) =>
    Compile(AntlrSyntax.Parse(source, backend), mixinName, backend);

  public static HixProgramImage Compile(CompilationUnitAst unit, string mixinName, HixBackend backend = null) {
    if (unit == null) throw new ArgumentNullException(nameof(unit));
    if (unit.Diagnostics.Count != 0) throw new ArgumentException(unit.Diagnostics[0].Message);
    var declarations = unit.Declarations.OfType<MixinDeclarationAst>().Where(mixin => mixin.Name == mixinName).ToArray();
    if (declarations.Length != 1) throw new ArgumentException("expected exactly one mixin named '" + mixinName + "'");
    return Prepare(declarations[0], PrepareGlobals(new[] {unit}, backend), null);
  }

  public static (HixProgramImage Prelude, HixProgramImage Late) PreparePrograms(
    MixinDeclarationAst declaration, HixExpressionPreparedState globals) {
    using var profile = HixCompilerProfiler.Measure("compiler.prepare_programs");
    var compiled = PrepareSyntax(declaration, globals);
    var program = CreateProgram(compiled.Prelude.Concat(compiled.Late).ToArray(), compiled.Functions, globals);
    return (program.ForPass(true), program.ForPass(false));
  }

  private static HixProgramImage CreateProgram(IReadOnlyList<ExpressionDeclarationAst> expressions,
    IReadOnlyList<FunctionDeclarationAst> functions, HixExpressionPreparedState globals) {
    using var profile = HixCompilerProfiler.Measure("compiler.bytecode");
    return new HixBytecodeCompiler(globals.StringPool).Compile(expressions, functions, globals);
  }

  public static HixCompilerSyntax PrepareSyntax(MixinDeclarationAst declaration, HixExpressionPreparedState globals) {
    var expressions = declaration.Declarations.OfType<ExpressionDeclarationAst>().ToArray();
    var syntax = new HixCompilerSyntax(expressions.Where(expression => expression.IsPrelude).ToArray(),
      expressions.Where(expression => !expression.IsPrelude).ToArray(),
      declaration.Declarations.OfType<FunctionDeclarationAst>().ToArray());
    foreach (var step in globals.Backend.CompilerSteps) {
      using var stepProfile = HixCompilerProfiler.Measure("compiler.step." + step.GetType().Name);
      syntax = step.Transform(syntax, globals);
    }
    return syntax;
  }
}
