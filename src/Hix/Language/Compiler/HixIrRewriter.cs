using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

public class HixIrRewriter {
  public T Rewrite<T>(T node) where T : HixIrNode => (T)RewriteNode(node);

  protected virtual HixIrNode RewriteNode(HixIrNode node) {
    if (node is null) return null;
    HixIrNode rewritten = node switch {
      MetadataIr value => new MetadataIr(value.Name, value.Values.Select(Rewrite).ToArray()),
      ExpressionDeclarationIr value => new ExpressionDeclarationIr(value.IsPrelude, value.IsStrict, Rewrite(value.Body)),
      FunctionDeclarationIr value => new FunctionDeclarationIr(value.Name, value.IsPure, value.IsInline,
        value.IsNoinline, value.Signatures, Rewrite(value.Body), value.Metadata.Select(Rewrite).ToArray()),
      BlockStatementIr value => new BlockStatementIr(value.Statements.Select(Rewrite).ToArray(), value.Label),
      AssignmentStatementIr value => new AssignmentStatementIr(value.Storage, value.Name, Rewrite(value.Value), value.IsCarried),
      InvocationStatementIr value => new InvocationStatementIr(Rewrite(value.Call)),
      ControlFlowStatementIr value => new ControlFlowStatementIr(value.Operation, value.Label,
        value.Values.Select(Rewrite).ToArray()),
      SelectionStatementIr value => new SelectionStatementIr(Rewrite(value.Selection)),
      StringExpressionIr value => new StringExpressionIr(value.Value),
      NumberExpressionIr value => new NumberExpressionIr(value.Value),
      BooleanExpressionIr value => new BooleanExpressionIr(value.Value),
      NullExpressionIr => new NullExpressionIr(),
      RootExpressionIr value => RewriteRoot(value),
      SelectorExpressionIr => new SelectorExpressionIr(),
      MemberExpressionIr value => new MemberExpressionIr(Rewrite(value.Receiver), value.Member),
      CallExpressionIr value => RewriteCall(value),
      InlineExpressionIr value => new InlineExpressionIr(Rewrite(value.Body), value.ResultLocal),
      LambdaExpressionIr value => new LambdaExpressionIr(Rewrite(value.Body)),
      UnaryExpressionIr value => new UnaryExpressionIr(value.Operation, Rewrite(value.Value)),
      FallbackExpressionIr value => new FallbackExpressionIr(Rewrite(value.Value), Rewrite(value.Fallback)),
      TupleExpressionIr value => new TupleExpressionIr(value.Values.Select(Rewrite).ToArray()),
      TableExpressionIr value => new TableExpressionIr(value.Entries.Select(entry =>
        new KeyValuePair<string, ExpressionIr>(entry.Key, Rewrite(entry.Value))).ToArray(), value.FieldMetadata.Select(entry => new KeyValuePair<string, IReadOnlyList<MetadataIr>>(entry.Key, entry.Value.Select(Rewrite).ToArray())).ToArray()),
      InterpolationExpressionIr value => new InterpolationExpressionIr(value.Parts.Select(Rewrite).ToArray()),
      SelectionBranchIr value => new SelectionBranchIr(value.Conditions.Select(Rewrite).ToArray(),
        RewriteNode(value.Result), value.IsTransformation),
      SelectionExpressionIr value => new SelectionExpressionIr(Rewrite(value.Selector),
        value.Branches.Select(Rewrite).ToArray(), RewriteNode(value.Fallback)),
      _ => node
    };
    return ReferenceEquals(rewritten, node) ? rewritten : CopyLocation(node, rewritten);
  }

  protected virtual ExpressionIr RewriteRoot(RootExpressionIr root) =>
    new RootExpressionIr(root.Name, root.IsSmart);

  protected virtual ExpressionIr RewriteCall(CallExpressionIr call) =>
    new CallExpressionIr(call.Name, call.Arguments.Select(Rewrite).ToArray(), call.CoerceBoolean, call.Binding);

  protected static T CopyLocation<T>(HixIrNode source, T target) where T : HixIrNode {
    target.SourceRange = source.SourceRange;
    target.Tokens = source.Tokens;
    return target;
  }
}
