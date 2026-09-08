using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler.Steps;

internal sealed class PreludeHoistingStep : HixCompilerStep {
  internal override HixCompilerSyntax Transform(HixCompilerSyntax input, MixinExpressionPreparedState globals) {
    var names = new HashSet<string>(input.Prelude.SelectMany(expression => expression.Body.Statements)
      .OfType<AssignmentStatementAst>().Where(statement => statement.IsCarried)
      .Select(statement => statement.Name), StringComparer.Ordinal);
    var generated = new List<StatementAst>();
    var functions = input.Functions.Concat(globals.Functions).GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    var rewriter = new HoistingRewriter(generated, names, functions);
    var late = input.Late.Select(expression => expression.IsStrict
      ? new HixAstRewriter().Rewrite(expression)
      : rewriter.RewriteLateExpression(expression)).ToArray();
    if (generated.Count == 0) return new HixCompilerSyntax(input.Prelude, late, input.Functions);
    var generatedBlock = new BlockStatementAst(generated);
    var generatedExpression = new ExpressionDeclarationAst(true, false, generatedBlock);
    return new HixCompilerSyntax(input.Prelude.Concat([generatedExpression]).ToArray(), late, input.Functions);
  }

  private sealed class HoistingRewriter(ICollection<StatementAst> generated, ISet<string> names,
    IReadOnlyDictionary<string, FunctionDeclarationAst[]> functions) : HixAstRewriter {
    private readonly Dictionary<string, string> carries = new(StringComparer.Ordinal);

    internal ExpressionDeclarationAst RewriteLateExpression(ExpressionDeclarationAst expression) =>
      CopyLocation(expression, new ExpressionDeclarationAst(false, expression.IsStrict, RewriteLateBlock(expression.Body)));

    private BlockStatementAst RewriteLateBlock(BlockStatementAst block) => CopyLocation(block,
      new BlockStatementAst(block.Statements.Select(RewriteLateStatement).ToArray(), block.Label));

    private StatementAst RewriteLateStatement(StatementAst statement) => statement switch {
      BlockStatementAst block => RewriteLateBlock(block),
      AssignmentStatementAst value => CopyLocation(value,
        new AssignmentStatementAst(value.Storage, value.Name, RewriteLateValue(value.Value), value.IsCarried)),
      InvocationStatementAst value => CopyLocation(value,
        new InvocationStatementAst(RewriteEffectCall(value.Call))),
      ControlFlowStatementAst value => CopyLocation(value,
        new ControlFlowStatementAst(value.Operation, value.Label, value.Values.Select(RewriteLateValue).ToArray())),
      SelectionStatementAst value => CopyLocation(value,
        new SelectionStatementAst((SelectionExpressionAst)RewriteLateSelection(value.Selection))),
      _ => Rewrite(statement)
    };

    private CallExpressionAst RewriteEffectCall(CallExpressionAst call) => CopyLocation(call,
      new CallExpressionAst(call.Name, call.Arguments.Select(RewriteLateValue).ToArray(), call.CoerceBoolean));

    private ExpressionAst RewriteLateValue(ExpressionAst expression) {
      if (expression is null) return null;
      if (DependsOnPrelude(expression, new HashSet<string>(StringComparer.Ordinal))) return Carry(expression);
      return expression switch {
        MemberExpressionAst value => CopyLocation(value, new MemberExpressionAst(RewriteLateValue(value.Receiver), value.Member)),
        CallExpressionAst value => CopyLocation(value, new CallExpressionAst(value.Name,
          value.Arguments.Select(RewriteLateValue).ToArray(), value.CoerceBoolean)),
        UnaryExpressionAst value => CopyLocation(value, new UnaryExpressionAst(value.Operation, RewriteLateValue(value.Value))),
        FallbackExpressionAst value => CopyLocation(value,
          new FallbackExpressionAst(RewriteLateValue(value.Value), RewriteLateValue(value.Fallback))),
        TupleExpressionAst value => CopyLocation(value, new TupleExpressionAst(value.Values.Select(RewriteLateValue).ToArray())),
        TableExpressionAst value => CopyLocation(value, new TableExpressionAst(value.Entries.Select(entry =>
          new KeyValuePair<string, ExpressionAst>(entry.Key, RewriteLateValue(entry.Value))).ToArray())),
        InterpolationExpressionAst value => CopyLocation(value,
          new InterpolationExpressionAst(value.Parts.Select(RewriteLateValue).ToArray())),
        SelectionExpressionAst value => RewriteLateSelection(value),
        _ => Rewrite(expression)
      };
    }

    private ExpressionAst RewriteLateSelection(SelectionExpressionAst selection) => CopyLocation(selection,
      new SelectionExpressionAst(RewriteLateValue(selection.Selector), selection.Branches.Select(branch =>
        CopyLocation(branch, new SelectionBranchAst(branch.Conditions.Select(RewriteLateValue).ToArray(),
          branch.Result is ExpressionAst expression ? RewriteLateValue(expression) :
          branch.Result is BlockStatementAst block ? RewriteLateBlock(block) : RewriteNode(branch.Result),
          branch.IsTransformation))).ToArray(),
        selection.Fallback is ExpressionAst fallback ? RewriteLateValue(fallback) :
        selection.Fallback is BlockStatementAst block ? RewriteLateBlock(block) : RewriteNode(selection.Fallback)));

    private ExpressionAst Carry(ExpressionAst expression) {
      var key = expression.Program?.Source is { } source && !expression.SourceRange.IsEmpty
        ? source.Substring(expression.SourceRange.Start, expression.SourceRange.Length)
        : expression.GetType().Name + ":" + expression.SourceRange.Start;
      if (!carries.TryGetValue(key, out var name)) {
        var index = carries.Count;
        do name = "__hoist_" + index++; while (names.Contains(name));
        names.Add(name);
        carries.Add(key, name);
        generated.Add(CopyLocation(expression,
          new AssignmentStatementAst(StorageSpace.Local, name, Rewrite(expression), true)));
      }
      return CopyLocation(expression,
        new MemberExpressionAst(new RootExpressionAst("local"), name));
    }

    private bool DependsOnPrelude(HixAst node, ISet<string> active) {
      if (node is null) return false;
      if (node is RootExpressionAst {IsSmart: false, Name: "this" or "target" or "attr"}) return true;
      if (node is CallExpressionAst call) {
        if (FunctionLibrary.Resolve(call.Name, call.Arguments.Count).Any(definition => definition.HasEffects)) return true;
        if (functions.TryGetValue(call.Name, out var candidates) && active.Add(call.Name)) {
          try {
            if (candidates.Any(function => DependsOnPrelude(function.Body, active))) return true;
          } finally { active.Remove(call.Name); }
        }
      }
      return node.SemanticChildren.Any(child => DependsOnPrelude(child, active));
    }
  }
}
