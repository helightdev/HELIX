using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler.Steps;

internal sealed class FunctionBindingStep : HixCompilerStep {
  internal override HixCompilerSyntax Transform(HixCompilerSyntax input, MixinExpressionPreparedState globals) {
    var names = new HashSet<string>(input.Functions.Select(function => function.Name), StringComparer.Ordinal);
    foreach (var function in globals.Functions) names.Add(function.Name);
    var rewriter = new BindingRewriter(names);
    return new HixCompilerSyntax(input.Prelude.Select(rewriter.Rewrite).ToArray(),
      input.Late.Select(rewriter.Rewrite).ToArray(), input.Functions.Select(rewriter.Rewrite).ToArray());
  }

  private sealed class BindingRewriter(ISet<string> functions) : HixAstRewriter {
    protected override ExpressionAst RewriteCall(CallExpressionAst call) {
      if (!functions.Contains(call.Name) && FunctionLibrary.Resolve(call.Name, call.Arguments.Count).Count == 0)
        throw new ArgumentException("unknown function '" + call.Name + "'", nameof(call));
      return base.RewriteCall(call);
    }
  }
}
