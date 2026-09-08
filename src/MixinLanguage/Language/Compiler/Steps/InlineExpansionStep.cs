using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mixins.Compiler.Steps;

internal sealed class InlineExpansionStep : HixCompilerStep {
  internal override HixCompilerSyntax Transform(HixCompilerSyntax input, MixinExpressionPreparedState globals) {
    var functions = input.Functions.Concat(globals.Functions)
      .GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    var rewriter = new InlineRewriter(functions);
    return new HixCompilerSyntax(input.Prelude.Select(rewriter.Rewrite).ToArray(),
      input.Late.Select(rewriter.Rewrite).ToArray(), input.Functions);
  }

  private sealed class InlineRewriter(IReadOnlyDictionary<string, FunctionDeclarationAst[]> functions)
    : HixAstRewriter {
    private readonly HashSet<string> active = new(StringComparer.Ordinal);
    private int sequence;

    protected override ExpressionAst RewriteCall(CallExpressionAst call) {
      var arguments = call.Arguments.Select(Rewrite).ToArray();
      var target = arguments.FirstOrDefault() as RootExpressionAst;
      var explicitInline = call.Name == "inline" && target is {IsSmart: false};
      var name = explicitInline ? target.Name : call.Name;
      var supplied = explicitInline ? arguments.Skip(1).ToArray() : arguments;
      if (!functions.TryGetValue(name, out var candidates))
        return CopyLocation(call, new CallExpressionAst(call.Name, arguments, call.CoerceBoolean));
      var function = candidates.FirstOrDefault(candidate => Accepts(candidate, supplied.Length) &&
        (explicitInline || candidate.IsInline));
      if (function is null)
        return CopyLocation(call, new CallExpressionAst(call.Name, arguments, call.CoerceBoolean));
      if (!active.Add(function.Name)) throw new ArgumentException("recursive inline function '" + function.Name + "'");
      try { return Expand(call, function, supplied); }
      finally { active.Remove(function.Name); }
    }

    private ExpressionAst Expand(CallExpressionAst call, FunctionDeclarationAst function,
      IReadOnlyList<ExpressionAst> arguments) {
      var suffix = "__inline_" + sequence++.ToString(CultureInfo.InvariantCulture);
      var argumentLocals = arguments.Select((_, index) => suffix + "_arg_" + index).ToArray();
      var resultLocal = suffix + "_result";
      var endLabel = suffix + "_end";
      var localNames = Descendants(function.Body).OfType<AssignmentStatementAst>()
        .Where(statement => statement.Storage == StorageSpace.Local).Select(statement => statement.Name)
        .Distinct(StringComparer.Ordinal).ToDictionary(name => name, name => name + suffix, StringComparer.Ordinal);
      var labels = Descendants(function.Body).Select(node => node switch {
          BlockStatementAst block => block.Label,
          ControlFlowStatementAst {Operation: ControlFlowKind.Label or ControlFlowKind.Goto} flow => flow.Label,
          _ => null
        }).Where(name => !string.IsNullOrEmpty(name)).Distinct(StringComparer.Ordinal)
        .ToDictionary(name => name, name => name + suffix, StringComparer.Ordinal);
      var signature = function.Signatures.FirstOrDefault(candidate => Accepts(candidate, arguments.Count));
      var named = signature?.Inputs?.Where(field => !field.Variadic).Select((field, index) => (field.Name, index))
        .ToDictionary(item => item.Name, item => item.index, StringComparer.Ordinal)
        ?? new Dictionary<string, int>(StringComparer.Ordinal);
      var bodyRewriter = new InlineBodyRewriter(this, argumentLocals, named, localNames, labels, resultLocal, endLabel);
      var statements = new List<StatementAst> {
        new AssignmentStatementAst(StorageSpace.Local, resultLocal, new NullExpressionAst())
      };
      for (var index = 0; index < arguments.Count; index++)
        statements.Add(new AssignmentStatementAst(StorageSpace.Local, argumentLocals[index], arguments[index]));
      statements.AddRange(function.Body.Statements.Select(bodyRewriter.Rewrite));
      statements.Add(new BlockStatementAst([], endLabel));
      ExpressionAst expanded = new InlineExpressionAst(new BlockStatementAst(statements), resultLocal);
      if (call.CoerceBoolean) expanded = new CallExpressionAst("bool", [expanded]);
      return CopyLocation(call, expanded);
    }

    private static bool Accepts(FunctionDeclarationAst function, int count) => function.Signatures.Count == 0 ||
      function.Signatures.Any(signature => Accepts(signature, count));

    private static bool Accepts(FunctionSignature signature, int count) => signature.Inputs == null ||
      (signature.Inputs.Any(field => field.Variadic)
        ? count >= signature.Inputs.TakeWhile(field => !field.Variadic).Count()
        : signature.Inputs.Count == count);

    private static IEnumerable<HixAst> Descendants(HixAst node) {
      yield return node;
      foreach (var child in node.SemanticChildren)
        foreach (var descendant in Descendants(child)) yield return descendant;
    }
  }

  private sealed class InlineBodyRewriter(InlineRewriter owner, IReadOnlyList<string> arguments,
    IReadOnlyDictionary<string, int> named, IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> labels, string resultLocal, string endLabel) : HixAstRewriter {
    protected override HixAst RewriteNode(HixAst node) {
      if (node is MemberExpressionAst {Receiver: RootExpressionAst {IsSmart: false, Name: "local"}} member &&
        locals.TryGetValue(member.Member, out var localMember))
        return CopyLocation(member, Local(localMember));
      if (node is AssignmentStatementAst assignment && assignment.Storage == StorageSpace.Local &&
        locals.TryGetValue(assignment.Name, out var renamed))
        return CopyLocation(assignment,
          new AssignmentStatementAst(StorageSpace.Local, renamed, Rewrite(assignment.Value), assignment.IsCarried));
      if (node is ControlFlowStatementAst {Operation: ControlFlowKind.Return} returned) {
        ExpressionAst result = returned.Values.Count switch {
          0 => new NullExpressionAst(), 1 => Rewrite(returned.Values[0]),
          _ => new TupleExpressionAst(returned.Values.Select(Rewrite).ToArray())
        };
        return CopyLocation(returned, new BlockStatementAst([
          new AssignmentStatementAst(StorageSpace.Local, resultLocal, result),
          new ControlFlowStatementAst(ControlFlowKind.Goto, endLabel, [])
        ]));
      }
      if (node is ControlFlowStatementAst flow && flow.Operation is ControlFlowKind.Goto or ControlFlowKind.Label &&
        labels.TryGetValue(flow.Label, out var label))
        return CopyLocation(flow,
          new ControlFlowStatementAst(flow.Operation, label, flow.Values.Select(Rewrite).ToArray()));
      if (node is BlockStatementAst block && block.Label != null &&
        labels.TryGetValue(block.Label, out var blockLabel))
        return CopyLocation(block, new BlockStatementAst(block.Statements.Select(Rewrite).ToArray(), blockLabel));
      return base.RewriteNode(node);
    }

    protected override ExpressionAst RewriteRoot(RootExpressionAst root) {
      if (root.IsSmart) {
        if (locals.TryGetValue(root.Name, out var local)) return Local(local);
        if (int.TryParse(root.Name, out var position) && position > 0)
          return position <= arguments.Count ? Local(arguments[position - 1]) : new NullExpressionAst();
        if (named.TryGetValue(root.Name, out var index) && index < arguments.Count) return Local(arguments[index]);
      }
      if (!root.IsSmart && root.Name == "param") return arguments.Count switch {
        0 => new NullExpressionAst(), 1 => Local(arguments[0]),
        _ => new TupleExpressionAst(arguments.Select(Local).ToArray())
      };
      return base.RewriteRoot(root);
    }

    protected override ExpressionAst RewriteCall(CallExpressionAst call) => owner.Rewrite(base.RewriteCall(call));

    private static ExpressionAst Local(string name) => new MemberExpressionAst(new RootExpressionAst("local"), name);
  }
}
