using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler;

internal class HixAstRewriter {
  internal T Rewrite<T>(T node) where T : HixAst => (T)RewriteNode(node);

  protected virtual HixAst RewriteNode(HixAst node) {
    if (node is null) return null;
    HixAst rewritten = node switch {
      ExpressionDeclarationAst value => new ExpressionDeclarationAst(value.IsPrelude, value.IsStrict, Rewrite(value.Body)),
      FunctionDeclarationAst value => new FunctionDeclarationAst(value.Name, value.IsPure, value.IsInline,
        value.IsNoinline, value.Signatures, Rewrite(value.Body), value.Metadata),
      BlockStatementAst value => new BlockStatementAst(value.Statements.Select(Rewrite).ToArray(), value.Label),
      AssignmentStatementAst value => new AssignmentStatementAst(value.Storage, value.Name, Rewrite(value.Value), value.IsCarried),
      InvocationStatementAst value => new InvocationStatementAst(Rewrite(value.Call)),
      ControlFlowStatementAst value => new ControlFlowStatementAst(value.Operation, value.Label,
        value.Values.Select(Rewrite).ToArray()),
      SelectionStatementAst value => new SelectionStatementAst(Rewrite(value.Selection)),
      StringExpressionAst value => new StringExpressionAst(value.Value),
      NumberExpressionAst value => new NumberExpressionAst(value.Value),
      BooleanExpressionAst value => new BooleanExpressionAst(value.Value),
      NullExpressionAst => new NullExpressionAst(),
      RootExpressionAst value => RewriteRoot(value),
      MemberExpressionAst value => new MemberExpressionAst(Rewrite(value.Receiver), value.Member),
      CallExpressionAst value => RewriteCall(value),
      InlineExpressionAst value => new InlineExpressionAst(Rewrite(value.Body), value.ResultLocal),
      LambdaExpressionAst value => new LambdaExpressionAst(Rewrite(value.Body)),
      UnaryExpressionAst value => new UnaryExpressionAst(value.Operation, Rewrite(value.Value)),
      FallbackExpressionAst value => new FallbackExpressionAst(Rewrite(value.Value), Rewrite(value.Fallback)),
      TupleExpressionAst value => new TupleExpressionAst(value.Values.Select(Rewrite).ToArray()),
      TableExpressionAst value => new TableExpressionAst(value.Entries.Select(entry =>
        new KeyValuePair<string, ExpressionAst>(entry.Key, Rewrite(entry.Value))).ToArray(), value.FieldMetadata),
      InterpolationExpressionAst value => new InterpolationExpressionAst(value.Parts.Select(Rewrite).ToArray()),
      SelectionBranchAst value => new SelectionBranchAst(value.Conditions.Select(Rewrite).ToArray(),
        RewriteNode(value.Result), value.IsTransformation),
      SelectionExpressionAst value => new SelectionExpressionAst(Rewrite(value.Selector),
        value.Branches.Select(Rewrite).ToArray(), RewriteNode(value.Fallback)),
      _ => node
    };
    return ReferenceEquals(rewritten, node) ? rewritten : CopyLocation(node, rewritten);
  }

  protected virtual ExpressionAst RewriteRoot(RootExpressionAst root) =>
    new RootExpressionAst(root.Name, root.IsSmart);

  protected virtual ExpressionAst RewriteCall(CallExpressionAst call) =>
    new CallExpressionAst(call.Name, call.Arguments.Select(Rewrite).ToArray(), call.CoerceBoolean);

  protected static T CopyLocation<T>(HixAst source, T target) where T : HixAst {
    target.SourceRange = source.SourceRange;
    target.Tokens = source.Tokens;
    target.Kind = source.Kind;
    return target;
  }
}
