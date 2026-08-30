namespace MixinLanguage.Compiler;

/// <summary>The syntax state passed between ordered compiler transformations.</summary>
public sealed record MixinCompilerSyntax(
  MixinProgramSyntax Prelude,
  MixinProgramSyntax Expression
);

/// <summary>An AST-to-AST transformation in the mixin compiler pipeline.</summary>
public abstract class MixinExpressionCompilerStep {
  public abstract bool TryTransform(
    MixinCompilerSyntax input,
    MixinExpressionPreparedState preparedState,
    out MixinCompilerSyntax output,
    out string error,
    out int errorLine
  );
}