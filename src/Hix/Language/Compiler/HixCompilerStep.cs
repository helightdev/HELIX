using System.Collections.Generic;

namespace Hix.Compiler;

public sealed record HixCompilerSyntax(
  IReadOnlyList<ExpressionDeclarationAst> Prelude,
  IReadOnlyList<ExpressionDeclarationAst> Late,
  IReadOnlyList<FunctionDeclarationAst> Functions
);

public abstract class HixCompilerStep {
  public abstract HixCompilerSyntax Transform(HixCompilerSyntax input, HixExpressionPreparedState globals);
}
