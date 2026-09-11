using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler.Steps;

public sealed class PreludeHoistingStep : HixLoweringStep {
  public override HixModuleIr Lower(HixModuleIr input, HixCompilerCatalog globals) {
    var names = new HashSet<string>(input.Prelude.SelectMany(expression => expression.Body.Statements)
      .OfType<AssignmentStatementIr>().Where(statement => statement.IsCarried)
      .Select(statement => statement.Name), StringComparer.Ordinal);
    var generated = new List<StatementIr>();
    var functions = input.Functions.Concat(globals.Functions).GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    var rewriter = new HoistingRewriter(generated, names, functions, globals.Backend);
    var late = input.Late.Select(expression => expression.IsStrict
      ? new HixIrRewriter().Rewrite(expression)
      : rewriter.RewriteLateExpression(expression)).ToArray();
    if (generated.Count == 0) return new HixModuleIr(input.Prelude, late, input.Functions, input.Parameters);
    var generatedBlock = new BlockStatementIr(generated);
    var generatedExpression = new ExpressionDeclarationIr(true, false, generatedBlock);
    return new HixModuleIr(input.Prelude.Concat([generatedExpression]).ToArray(), late, input.Functions, input.Parameters);
  }

  private sealed class HoistingRewriter(ICollection<StatementIr> generated, ISet<string> names,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions, HixBackend backend) : HixIrRewriter {
    private readonly Dictionary<string, string> carries = new(StringComparer.Ordinal);

    public ExpressionDeclarationIr RewriteLateExpression(ExpressionDeclarationIr expression) =>
      CopyLocation(expression, new ExpressionDeclarationIr(false, expression.IsStrict, RewriteLateBlock(expression.Body)));

    private BlockStatementIr RewriteLateBlock(BlockStatementIr block) => CopyLocation(block,
      new BlockStatementIr(block.Statements.Select(RewriteLateStatement).ToArray(), block.Label));

    private StatementIr RewriteLateStatement(StatementIr statement) => PreserveMetadata(statement, statement switch {
      BlockStatementIr block => RewriteLateBlock(block),
      AssignmentStatementIr value => CopyLocation(value,
        new AssignmentStatementIr(value.Storage, value.Name,
          value.Value == null ? null : RewriteLateValue(value.Value), value.IsCarried,
          value.IsDeclaration, value.DeclaredPattern)),
      InvocationStatementIr value => CopyLocation(value,
        new InvocationStatementIr(RewriteEffectCall(value.Call))),
      ControlFlowStatementIr value => CopyLocation(value,
        new ControlFlowStatementIr(value.Operation, value.Label, value.Values.Select(RewriteLateValue).ToArray())),
      SelectionStatementIr value => CopyLocation(value,
        new SelectionStatementIr((SelectionExpressionIr)RewriteLateSelection(value.Selection))),
      _ => Rewrite(statement)
    });

    private StatementIr PreserveMetadata(StatementIr source, StatementIr target) {
      target.SetMetadata(source.Metadata.Select(Rewrite).ToArray());
      return target;
    }

    private CallExpressionIr RewriteEffectCall(CallExpressionIr call) => CopyLocation(call,
      new CallExpressionIr(call.Name, call.EffectiveArguments.Select(RewriteLateValue).ToArray(), call.CoerceBoolean, call.Binding));

    private ExpressionIr RewriteLateValue(ExpressionIr expression) {
      if (expression is null) return null;
      if (DependsOnPrelude(expression, new HashSet<string>(StringComparer.Ordinal))) return Carry(expression);
      return expression switch {
        MemberExpressionIr value => CopyLocation(value, new MemberExpressionIr(RewriteLateValue(value.Receiver), value.Member)),
        CallExpressionIr value => CopyLocation(value, new CallExpressionIr(value.Name,
          value.EffectiveArguments.Select(RewriteLateValue).ToArray(), value.CoerceBoolean, value.Binding)),
        UnaryExpressionIr value => CopyLocation(value, new UnaryExpressionIr(value.Operation, RewriteLateValue(value.Value))),
        FallbackExpressionIr value => CopyLocation(value,
          new FallbackExpressionIr(RewriteLateValue(value.Value), RewriteLateValue(value.Fallback))),
        TupleExpressionIr value => CopyLocation(value, new TupleExpressionIr(value.Values.Select(RewriteLateValue).ToArray())),
        TableExpressionIr value => CopyLocation(value, new TableExpressionIr(value.Entries.Select(entry =>
          new KeyValuePair<string, ExpressionIr>(entry.Key, RewriteLateValue(entry.Value))).ToArray(),
          value.FieldMetadata)),
        InterpolationExpressionIr value => CopyLocation(value,
          new InterpolationExpressionIr(value.Parts.Select(RewriteLateValue).ToArray())),
        SelectionExpressionIr value => RewriteLateSelection(value),
        _ => Rewrite(expression)
      };
    }

    private ExpressionIr RewriteLateSelection(SelectionExpressionIr selection) => CopyLocation(selection,
      new SelectionExpressionIr(RewriteLateValue(selection.Selector), selection.Branches.Select(branch =>
        CopyLocation(branch, new SelectionBranchIr(branch.Conditions.Select(RewriteLateValue).ToArray(),
          branch.Result is ExpressionIr expression ? RewriteLateValue(expression) :
          branch.Result is BlockStatementIr block ? RewriteLateBlock(block) : RewriteNode(branch.Result),
          branch.IsTransformation))).ToArray(),
        selection.Fallback is ExpressionIr fallback ? RewriteLateValue(fallback) :
        selection.Fallback is BlockStatementIr block ? RewriteLateBlock(block) : RewriteNode(selection.Fallback)));

    private ExpressionIr Carry(ExpressionIr expression) {
      var key = expression.Program?.Source is { } source && !expression.SourceRange.IsEmpty
        ? source.Substring(expression.SourceRange.Start, expression.SourceRange.Length)
        : expression.GetType().Name + ":" + expression.SourceRange.Start;
      if (!carries.TryGetValue(key, out var name)) {
        var index = carries.Count;
        do name = "__hoist_" + index++; while (names.Contains(name));
        names.Add(name);
        carries.Add(key, name);
        generated.Add(CopyLocation(expression,
          new AssignmentStatementIr(StorageSpace.Local, name, Rewrite(expression), true)));
      }
      return CopyLocation(expression,
        new MemberExpressionIr(new RootExpressionIr("local"), name));
    }

    private bool DependsOnPrelude(HixIrNode node, ISet<string> active, bool mixinParameters = true) {
      if (node is null) return false;
      // Mixin parameters are supplied by the host context. Late generator passes run on an
      // unlinked context, so a smart parameter access must be captured while that host is live.
      // Function bodies also use parameter bindings, but those are ordinary call arguments.
      if (mixinParameters && node is RootExpressionIr {Binding.Kind: HixReferenceKind.Parameter}) return true;
      if (node is RootExpressionIr {IsSmart: false} root && backend.Roots.TryGetValue(root.Name, out var definition) && definition.RequiresPrelude) return true;
      if (node is CallExpressionIr call) {
        if (backend.Functions.Resolve(call.Name, call.Arguments.Count).Any(definition => definition.RequiresPrelude)) return true;
        if (functions.TryGetValue(call.Name, out var candidates) && active.Add(call.Name)) {
          try {
            if (candidates.Any(function => DependsOnPrelude(function.Body, active, false))) return true;
          } finally { active.Remove(call.Name); }
        }
      }
      return node.SemanticChildren.Any(child => DependsOnPrelude(child, active, mixinParameters));
    }
  }
}
