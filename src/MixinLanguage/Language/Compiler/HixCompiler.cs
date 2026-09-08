using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler.Steps;
using Mixins.Runtime;

namespace Mixins.Compiler;

/// <summary>Catalog binding and preparation for the generated-ANTLR semantic model.</summary>
public static class HixCompiler {
  private static readonly IReadOnlyList<HixCompilerStep> Steps = [
    new LambdaLiftingStep(), new SignatureParameterBindingStep(), new InlineExpansionStep(),
    new PreludeHoistingStep(), new FunctionBindingStep()
  ];
  public static MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> sources) =>
    PrepareGlobals(sources.Select(AntlrSyntax.Parse));

  public static MixinExpressionPreparedState PrepareGlobals(IEnumerable<CompilationUnitAst> units) {
    var syntax = units.ToArray();
    var errors = syntax.SelectMany(unit => unit.Diagnostics).ToArray();
    if (errors.Length != 0) throw new ArgumentException(errors[0].Message);
    var declarations = syntax.SelectMany(unit => unit.Declarations).Select(declaration =>
      declaration is FunctionDeclarationAst function ? SignatureParameterBindingStep.Rewrite(function) : declaration).ToArray();
    var diagnostics = new List<HixParseDiagnostic>();
    LanguageValidation.Validate(declarations, diagnostics);
    if (diagnostics.Count != 0) throw new ArgumentException(diagnostics[0].Message);
    var strings = new MixinStringPoolBuilder();
    foreach (var unit in syntax)
      foreach (var token in unit.Tokens) strings.Intern(token.Text);
    var functions = LambdaLiftingStep.RewriteFunctions(declarations.OfType<FunctionDeclarationAst>().ToArray())
      .Select(SignatureParameterBindingStep.Rewrite).ToArray();
    var derivations = declarations.OfType<MixinDeclarationAst>().Where(mixin => mixin.IsDerivation).ToArray();
    return new MixinExpressionPreparedState(strings.Freeze(), functions, derivations,
      new LanguageProgramBindings(functions.Cast<HixAst>().Concat(derivations)));
  }

  internal static MixinExpressionExecutionProgram Prepare(MixinDeclarationAst declaration,
    MixinExpressionPreparedState globals, bool? prelude) {
    var compiled = Compile(declaration, globals);
    var expressions = prelude == true ? compiled.Prelude : prelude == false ? compiled.Late
      : compiled.Prelude.Concat(compiled.Late).ToArray();
    var bindings = new LanguageProgramBindings(globals.Functions.Cast<HixAst>()
      .Concat(compiled.Functions).Concat(expressions).Concat(globals.Derivations));
    return new MixinExpressionExecutionProgram(globals.StringPool, expressions,
      globals.Functions, compiled.Functions, globals.Derivations,
      globals.GlobalScope, globals.DerivationScopes, bindings);
  }

  internal static (MixinExpressionExecutionProgram Prelude, MixinExpressionExecutionProgram Late) PreparePrograms(
    MixinDeclarationAst declaration, MixinExpressionPreparedState globals
  ) {
    var compiled = Compile(declaration, globals);
    return (CreateProgram(compiled.Prelude, compiled.Functions, globals),
      CreateProgram(compiled.Late, compiled.Functions, globals));
  }

  private static MixinExpressionExecutionProgram CreateProgram(IReadOnlyList<ExpressionDeclarationAst> expressions,
    IReadOnlyList<FunctionDeclarationAst> functions, MixinExpressionPreparedState globals) {
    var bindings = new LanguageProgramBindings(globals.Functions.Cast<HixAst>()
      .Concat(functions).Concat(expressions).Concat(globals.Derivations));
    return new MixinExpressionExecutionProgram(globals.StringPool, expressions,
      globals.Functions, functions, globals.Derivations,
      globals.GlobalScope, globals.DerivationScopes, bindings);
  }

  private static HixCompilerSyntax Compile(MixinDeclarationAst declaration, MixinExpressionPreparedState globals) {
    var expressions = declaration.Declarations.OfType<ExpressionDeclarationAst>().ToArray();
    var syntax = new HixCompilerSyntax(expressions.Where(expression => expression.IsPrelude).ToArray(),
      expressions.Where(expression => !expression.IsPrelude).ToArray(),
      declaration.Declarations.OfType<FunctionDeclarationAst>().ToArray());
    foreach (var step in Steps) syntax = step.Transform(syntax, globals);
    return syntax;
  }
}
