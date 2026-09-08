using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;

namespace Mixins.Compiler;

/// <summary>Catalog binding and preparation for the generated-ANTLR semantic model.</summary>
public static class HixCompiler {
  public static MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> sources) =>
    PrepareGlobals(sources.Select(AntlrSyntax.Parse));

  public static MixinExpressionPreparedState PrepareGlobals(IEnumerable<CompilationUnitAst> units) {
    var syntax = units.ToArray();
    var errors = syntax.SelectMany(unit => unit.Diagnostics).ToArray();
    if (errors.Length != 0) throw new ArgumentException(errors[0].Message);
    var declarations = syntax.SelectMany(unit => unit.Declarations).ToArray();
    var diagnostics = new List<HixParseDiagnostic>();
    LanguageValidation.Validate(declarations, diagnostics);
    if (diagnostics.Count != 0) throw new ArgumentException(diagnostics[0].Message);
    var strings = new MixinStringPoolBuilder();
    foreach (var unit in syntax)
      foreach (var token in unit.Tokens) strings.Intern(token.Text);
    return new MixinExpressionPreparedState(strings.Freeze(),
      declarations.OfType<FunctionDeclarationAst>().ToArray(),
      declarations.OfType<MixinDeclarationAst>().Where(mixin => mixin.IsDerivation).ToArray(),
      new LanguageProgramBindings(declarations));
  }

  internal static MixinExpressionExecutionProgram Prepare(MixinDeclarationAst declaration,
    MixinExpressionPreparedState globals, bool? prelude) => new(globals.StringPool,
      declaration.Declarations.OfType<ExpressionDeclarationAst>().Where(expression => prelude == null || expression.IsPrelude == prelude).ToArray(),
      globals.Functions, declaration.Declarations.OfType<FunctionDeclarationAst>().ToArray(), globals.Derivations,
      globals.GlobalScope, globals.DerivationScopes, globals.Bindings);
}
