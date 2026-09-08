using System.Collections.Generic;

namespace Mixins.Compiler;

internal sealed record HixCompilerSyntax(
  IReadOnlyList<ExpressionDeclarationAst> Prelude,
  IReadOnlyList<ExpressionDeclarationAst> Late,
  IReadOnlyList<FunctionDeclarationAst> Functions
);

internal abstract class HixCompilerStep {
  internal abstract HixCompilerSyntax Transform(HixCompilerSyntax input, MixinExpressionPreparedState globals);
}
