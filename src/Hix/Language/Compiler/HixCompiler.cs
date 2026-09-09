using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Compiler.Steps;
using Hix.Runtime;

namespace Hix.Compiler;

/// <summary>Catalog binding and preparation for the generated-ANTLR semantic model.</summary>
public static class HixCompiler {
  private static readonly IReadOnlyList<HixCompilerStep> Steps = [
    new LambdaLiftingStep(), new SignatureParameterBindingStep(), new InlineExpansionStep(),
    new PreludeHoistingStep(), new FunctionBindingStep()
  ];
  public static HixExpressionPreparedState PrepareGlobals(IEnumerable<string> sources, HixBackend backend = null) =>
    PrepareGlobals(sources.Select(source => AntlrSyntax.Parse(source, backend)), backend);

  public static HixExpressionPreparedState PrepareGlobals(IEnumerable<CompilationUnitAst> units, HixBackend backend = null) {
    backend ??= HixCoreBackend.Instance;
    var syntax = units.ToArray();
    var errors = syntax.SelectMany(unit => unit.Diagnostics).ToArray();
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
    return new HixExpressionPreparedState(strings.Freeze(), functions, derivations, backend);
  }

  /// <summary>Compile global functions directly to an executable bytecode image.</summary>
  public static HixExpressionExecutionProgram CompileFunctions(IEnumerable<string> sources, HixBackend backend = null) {
    var globals = PrepareGlobals(sources, backend);
    var syntax = new HixCompilerSyntax([], [], globals.Functions);
    foreach (var step in Steps) syntax = step.Transform(syntax, globals);
    var rewritten = new HixExpressionPreparedState(globals.StringPool, syntax.Functions, globals.Derivations, globals.Backend);
    return CreateProgram([], [], rewritten);
  }

  internal static HixExpressionExecutionProgram Prepare(MixinDeclarationAst declaration,
    HixExpressionPreparedState globals, bool? prelude) {
    var compiled = Compile(declaration, globals);
    var expressions = prelude == true ? compiled.Prelude : prelude == false ? compiled.Late
      : compiled.Prelude.Concat(compiled.Late).ToArray();
    return CreateProgram(expressions, compiled.Functions, globals);
  }

  public static HixExpressionExecutionProgram Compile(string source, string mixinName, HixBackend backend = null) =>
    Compile(AntlrSyntax.Parse(source, backend), mixinName, backend);

  public static HixExpressionExecutionProgram Compile(CompilationUnitAst unit, string mixinName, HixBackend backend = null) {
    if (unit == null) throw new ArgumentNullException(nameof(unit));
    if (unit.Diagnostics.Count != 0) throw new ArgumentException(unit.Diagnostics[0].Message);
    var declarations = unit.Declarations.OfType<MixinDeclarationAst>().Where(mixin => mixin.Name == mixinName).ToArray();
    if (declarations.Length != 1) throw new ArgumentException("expected exactly one mixin named '" + mixinName + "'");
    return Prepare(declarations[0], PrepareGlobals(new[] {unit}, backend), null);
  }

  internal static (HixExpressionExecutionProgram Prelude, HixExpressionExecutionProgram Late) PreparePrograms(
    MixinDeclarationAst declaration, HixExpressionPreparedState globals) {
    var compiled = Compile(declaration, globals);
    var program = CreateProgram(compiled.Prelude.Concat(compiled.Late).ToArray(), compiled.Functions, globals);
    return (program.ForPass(true), program.ForPass(false));
  }

  private static HixExpressionExecutionProgram CreateProgram(IReadOnlyList<ExpressionDeclarationAst> expressions,
    IReadOnlyList<FunctionDeclarationAst> functions, HixExpressionPreparedState globals) =>
    new HixBytecodeCompiler(globals.StringPool).Compile(expressions, functions, globals);

  private static HixCompilerSyntax Compile(MixinDeclarationAst declaration, HixExpressionPreparedState globals) {
    var expressions = declaration.Declarations.OfType<ExpressionDeclarationAst>().ToArray();
    var syntax = new HixCompilerSyntax(expressions.Where(expression => expression.IsPrelude).ToArray(),
      expressions.Where(expression => !expression.IsPrelude).ToArray(),
      declaration.Declarations.OfType<FunctionDeclarationAst>().ToArray());
    foreach (var step in Steps) syntax = step.Transform(syntax, globals);
    return syntax;
  }
}
