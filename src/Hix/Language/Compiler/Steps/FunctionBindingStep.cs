using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler.Steps;

public sealed class FunctionBindingStep : HixCompilerStep {
  public override HixCompilerSyntax Transform(HixCompilerSyntax input, HixExpressionPreparedState globals) {
    var names = new HashSet<string>(input.Functions.Select(function => function.Name), StringComparer.Ordinal);
    foreach (var function in globals.Functions) names.Add(function.Name);
    var rewriter = new BindingRewriter(names, globals.Patterns.Keys, globals.Backend);
    return new HixCompilerSyntax(input.Prelude.Select(rewriter.Rewrite).ToArray(),
      input.Late.Select(rewriter.Rewrite).ToArray(), input.Functions.Select(rewriter.Rewrite).ToArray());
  }

  private sealed class BindingRewriter(ISet<string> functions, IEnumerable<string> patterns, HixBackend backend) : HixAstRewriter {
    private readonly HashSet<string> _patterns = new(patterns, StringComparer.Ordinal);
    protected override ExpressionAst RewriteRoot(RootExpressionAst root) {
      if (!root.IsSmart && root.Name is not ("local" or "var" or "tar" or "args" or "param") &&
          !KindHixValue.TryGet(root.Name, out _) && !functions.Contains(root.Name) && !backend.Roots.ContainsKey(root.Name))
        throw new ArgumentException("unknown root '" + root.Name + "'");
      return base.RewriteRoot(root);
    }
    protected override ExpressionAst RewriteCall(CallExpressionAst call) {
      if (!_patterns.Contains(call.Name) && !functions.Contains(call.Name) &&
          backend.Functions.Resolve(call.Name, call.Arguments.Count).Count == 0)
        throw new ArgumentException("unknown function '" + call.Name + "'", nameof(call));
      return base.RewriteCall(call);
    }
  }
}
