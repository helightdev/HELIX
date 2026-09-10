using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hix.Compiler.Steps;

public sealed class LambdaLiftingStep : HixLoweringStep {
  public override HixModuleIr Lower(HixModuleIr input, HixCompilerCatalog globals) {
    var names = new HashSet<string>(input.Functions.Concat(globals.Functions).Select(function => function.Name),
      StringComparer.Ordinal);
    var lifted = new List<FunctionDeclarationIr>();
    var rewriter = new LambdaRewriter(names, lifted);
    var functions = input.Functions.Select(rewriter.Rewrite).ToList();
    var prelude = input.Prelude.Select(rewriter.Rewrite).ToArray();
    var late = input.Late.Select(rewriter.Rewrite).ToArray();
    functions.AddRange(lifted);
    return new HixModuleIr(prelude, late, functions, input.Parameters);
  }

  public static IReadOnlyList<FunctionDeclarationIr> RewriteFunctions(
    IReadOnlyList<FunctionDeclarationIr> functions
  ) {
    var names = new HashSet<string>(functions.Select(function => function.Name), StringComparer.Ordinal);
    var lifted = new List<FunctionDeclarationIr>();
    var rewriter = new LambdaRewriter(names, lifted);
    var rewritten = functions.Select(rewriter.Rewrite).ToList();
    rewritten.AddRange(lifted);
    return rewritten;
  }

  private sealed class LambdaRewriter(ISet<string> names, ICollection<FunctionDeclarationIr> lifted)
    : HixIrRewriter {
    private int sequence;

    protected override HixIrNode RewriteNode(HixIrNode node) {
      if (node is not LambdaExpressionIr lambda) return base.RewriteNode(node);
      string name;
      do name = "__lambda_" + sequence++.ToString(CultureInfo.InvariantCulture);
      while (!names.Add(name));
      var body = Rewrite(lambda.Body);
      lifted.Add(CopyLocation(lambda, new FunctionDeclarationIr(name, true, true, false, [], body)));
      return CopyLocation(lambda, new RootExpressionIr(name));
    }
  }
}
