using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hix.Compiler.Steps;

public sealed class InlineExpansionStep : HixLoweringStep {
  public override HixModuleIr Lower(HixModuleIr input, HixCompilerCatalog globals) {
    var functions = input.Functions.Concat(globals.Functions)
      .GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    var rewriter = new InlineRewriter(functions);
    return new HixModuleIr(input.Prelude.Select(rewriter.Rewrite).ToArray(),
      input.Late.Select(rewriter.Rewrite).ToArray(), input.Functions, input.Parameters);
  }

  private sealed class InlineRewriter(IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions)
    : HixIrRewriter {
    private readonly HashSet<string> active = new(StringComparer.Ordinal);
    private int sequence;

    protected override ExpressionIr RewriteCall(CallExpressionIr call) {
      var arguments = call.EffectiveArguments.Select(Rewrite).ToArray();
      var target = arguments.FirstOrDefault() as RootExpressionIr;
      var explicitInline = call.Name == "inline" && target is {IsSmart: false};
      var name = explicitInline ? target.Name : call.Name;
      var supplied = explicitInline ? arguments.Skip(1).ToArray() : arguments;
      if (!functions.TryGetValue(name, out var candidates))
        return CopyLocation(call, new CallExpressionIr(call.Name, arguments, call.CoerceBoolean, call.Binding));
      var function = candidates.FirstOrDefault(candidate => Accepts(candidate, supplied.Length) &&
        (explicitInline || candidate.IsInline));
      if (function is null)
        return CopyLocation(call, new CallExpressionIr(call.Name, arguments, call.CoerceBoolean, call.Binding));
      if (!active.Add(function.Name)) throw new ArgumentException("recursive inline function '" + function.Name + "'");
      try { return Expand(call, function, supplied); }
      finally { active.Remove(function.Name); }
    }

    private ExpressionIr Expand(CallExpressionIr call, FunctionDeclarationIr function,
      IReadOnlyList<ExpressionIr> arguments) {
      var suffix = "__inline_" + sequence++.ToString(CultureInfo.InvariantCulture);
      var argumentLocals = arguments.Select((_, index) => suffix + "_arg_" + index).ToArray();
      var resultLocal = suffix + "_result";
      var endLabel = suffix + "_end";
      var localNames = Descendants(function.Body).OfType<AssignmentStatementIr>()
        .Where(statement => statement.Storage == StorageSpace.Local).Select(statement => statement.Name)
        .Distinct(StringComparer.Ordinal).ToDictionary(name => name, name => name + suffix, StringComparer.Ordinal);
      var labels = Descendants(function.Body).Select(node => node switch {
          BlockStatementIr block => block.Label,
          ControlFlowStatementIr {Operation: ControlFlowKind.Label or ControlFlowKind.Goto} flow => flow.Label,
          _ => null
        }).Where(name => !string.IsNullOrEmpty(name)).Distinct(StringComparer.Ordinal)
        .ToDictionary(name => name, name => name + suffix, StringComparer.Ordinal);
      var signature = function.Signatures.FirstOrDefault(candidate => Accepts(candidate, arguments.Count));
      var named = signature?.Inputs?.Where(field => !field.Variadic).Select((field, index) => (field.Name, index))
        .ToDictionary(item => item.Name, item => item.index, StringComparer.Ordinal)
        ?? new Dictionary<string, int>(StringComparer.Ordinal);
      var bodyRewriter = new InlineBodyRewriter(this, argumentLocals, named, localNames, labels, resultLocal, endLabel);
      var statements = new List<StatementIr> {
        new AssignmentStatementIr(StorageSpace.Local, resultLocal, new NullExpressionIr())
      };
      for (var index = 0; index < arguments.Count; index++)
        statements.Add(new AssignmentStatementIr(StorageSpace.Local, argumentLocals[index],
          arguments[index]));
      statements.AddRange(function.Body.Statements.Select(bodyRewriter.Rewrite));
      statements.Add(new BlockStatementIr([], endLabel));
      ExpressionIr expanded = new InlineExpressionIr(new BlockStatementIr(statements), resultLocal);
      if (call.CoerceBoolean) expanded = new CallExpressionIr("bool", [expanded]);
      return CopyLocation(call, expanded);
    }

    private static bool Accepts(FunctionDeclarationIr function, int count) => function.Signatures.Count == 0 ||
      function.Signatures.Any(signature => Accepts(signature, count));

    private static bool Accepts(FunctionSignature signature, int count) => signature.Inputs == null ||
      (signature.Inputs.Any(field => field.Variadic)
        ? count >= signature.Inputs.TakeWhile(field => !field.Variadic).Count()
        : signature.Inputs.Count == count);

    private static IEnumerable<HixIrNode> Descendants(HixIrNode node) {
      yield return node;
      foreach (var child in node.SemanticChildren)
        foreach (var descendant in Descendants(child)) yield return descendant;
    }
  }

  private sealed class InlineBodyRewriter(InlineRewriter owner, IReadOnlyList<string> arguments,
    IReadOnlyDictionary<string, int> named, IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> labels, string resultLocal, string endLabel) : HixIrRewriter {
    protected override HixIrNode RewriteNode(HixIrNode node) {
      if (node is MemberExpressionIr {Receiver: RootExpressionIr {IsSmart: false, Name: "local"}} member &&
        locals.TryGetValue(member.Member, out var localMember))
        return CopyLocation(member, Local(localMember));
      if (node is AssignmentStatementIr assignment && assignment.Storage == StorageSpace.Local &&
        locals.TryGetValue(assignment.Name, out var renamed))
        return CopyLocation(assignment,
          new AssignmentStatementIr(StorageSpace.Local, renamed,
            assignment.Value == null ? null : Rewrite(assignment.Value), assignment.IsCarried,
            assignment.IsDeclaration, assignment.DeclaredPattern));
      if (node is ControlFlowStatementIr {Operation: ControlFlowKind.Return} returned) {
        ExpressionIr result = returned.Values.Count switch {
          0 => new NullExpressionIr(), 1 => Rewrite(returned.Values[0]),
          _ => new TupleExpressionIr(returned.Values.Select(Rewrite).ToArray())
        };
        return CopyLocation(returned, new BlockStatementIr([
          new AssignmentStatementIr(StorageSpace.Local, resultLocal, result),
          new ControlFlowStatementIr(ControlFlowKind.Goto, endLabel, [])
        ]));
      }
      if (node is ControlFlowStatementIr flow && flow.Operation is ControlFlowKind.Goto or ControlFlowKind.Label &&
        labels.TryGetValue(flow.Label, out var label))
        return CopyLocation(flow,
          new ControlFlowStatementIr(flow.Operation, label, flow.Values.Select(Rewrite).ToArray()));
      if (node is BlockStatementIr block && block.Label != null &&
        labels.TryGetValue(block.Label, out var blockLabel))
        return CopyLocation(block, new BlockStatementIr(block.Statements.Select(Rewrite).ToArray(), blockLabel));
      return base.RewriteNode(node);
    }

    protected override ExpressionIr RewriteRoot(RootExpressionIr root) {
      if (root.IsSmart) {
        if (root.Name == "it") return Parameter();
        if (locals.TryGetValue(root.Name, out var local)) return Local(local);
        if (int.TryParse(root.Name, out var position) && position >= 0)
          return position < arguments.Count ? Local(arguments[position]) : new NullExpressionIr();
        if (named.TryGetValue(root.Name, out var index) && index < arguments.Count) return Local(arguments[index]);
      }
      if (!root.IsSmart && root.Name == "param") return Parameter();
      return base.RewriteRoot(root);
    }

    private ExpressionIr Parameter() => arguments.Count switch {
        0 => new NullExpressionIr(), 1 => Local(arguments[0]),
        _ => new TupleExpressionIr(arguments.Select(Local).ToArray())
      };

    protected override ExpressionIr RewriteCall(CallExpressionIr call) => owner.Rewrite(base.RewriteCall(call));

    private static ExpressionIr Local(string name) => new MemberExpressionIr(new RootExpressionIr("local"), name);
  }
}
