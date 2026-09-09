using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hix.Compiler.Steps;

public sealed class LambdaLiftingStep : HixCompilerStep {
  public override HixCompilerSyntax Transform(HixCompilerSyntax input, HixExpressionPreparedState globals) {
    var names = new HashSet<string>(input.Functions.Concat(globals.Functions).Select(function => function.Name),
      StringComparer.Ordinal);
    var lifted = new List<FunctionDeclarationAst>();
    var rewriter = new LambdaRewriter(names, lifted);
    var functions = input.Functions.Select(rewriter.Rewrite).ToList();
    var prelude = input.Prelude.Select(rewriter.Rewrite).ToArray();
    var late = input.Late.Select(rewriter.Rewrite).ToArray();
    functions.AddRange(lifted);
    return new HixCompilerSyntax(prelude, late, functions);
  }

  public static IReadOnlyList<FunctionDeclarationAst> RewriteFunctions(
    IReadOnlyList<FunctionDeclarationAst> functions
  ) {
    var names = new HashSet<string>(functions.Select(function => function.Name), StringComparer.Ordinal);
    var lifted = new List<FunctionDeclarationAst>();
    var rewriter = new LambdaRewriter(names, lifted);
    var rewritten = functions.Select(rewriter.Rewrite).ToList();
    rewritten.AddRange(lifted);
    return rewritten;
  }

  private sealed class LambdaRewriter(ISet<string> names, ICollection<FunctionDeclarationAst> lifted)
    : HixAstRewriter {
    private int sequence;

    protected override HixAst RewriteNode(HixAst node) {
      if (node is not LambdaExpressionAst lambda) return base.RewriteNode(node);
      string name;
      do name = "__lambda_" + sequence++.ToString(CultureInfo.InvariantCulture);
      while (!names.Add(name));
      var body = Rewrite(lambda.Body);
      lifted.Add(CopyLocation(lambda, new FunctionDeclarationAst(name, true, true, false, [], body)));
      return CopyLocation(lambda, new RootExpressionAst(name));
    }
  }
}
