using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Mixins.Runtime;

namespace Mixins.Compiler;

/// <summary>Executable blocks lowered once from canonical syntax; operations contain no invocation state.</summary>
internal sealed class LanguageProgramBindings {
  private readonly Dictionary<BlockStatementAst, Action<LanguageExecution>> blocks = new();

  internal LanguageProgramBindings(IEnumerable<HixAst> roots) {
    var visited = new HashSet<HixAst>();
    foreach (var root in roots) Visit(root, visited);
  }

  internal void Execute(BlockStatementAst block, LanguageExecution execution) => blocks[block](execution);

  internal string RenderExecutableIr(IEnumerable<ExpressionDeclarationAst> expressions) {
    var builder = new StringBuilder();
    foreach (var expression in expressions) {
      builder.Append("expression ").Append(expression.IsPrelude ? "prelude" : "late")
        .Append(" strict=").Append(expression.IsStrict ? "true" : "false").AppendLine();
      RenderBlock(builder, expression.Body, 1);
    }
    return builder.ToString();
  }

  private static void RenderBlock(StringBuilder builder, BlockStatementAst block, int depth) {
    Line(builder, depth, "block" + (block.Label == null ? "" : " label=" + Quote(block.Label)));
    foreach (var statement in block.Statements) RenderStatement(builder, statement, depth + 1);
    Line(builder, depth, "end");
  }

  private static void RenderStatement(StringBuilder builder, StatementAst statement, int depth) {
    switch (statement) {
      case BlockStatementAst block: RenderBlock(builder, block, depth); break;
      case AssignmentStatementAst assignment:
        Line(builder, depth, "store." + assignment.Storage.ToString().ToLowerInvariant() + " " + Quote(assignment.Name));
        RenderValue(builder, assignment.Value, depth + 1);
        break;
      case InvocationStatementAst invocation:
        RenderValue(builder, invocation.Call, depth, "invoke ");
        break;
      case SelectionStatementAst selection:
        Line(builder, depth, "evaluate");
        RenderValue(builder, selection.Selection, depth + 1);
        break;
      case ControlFlowStatementAst flow:
        Line(builder, depth, "flow." + flow.Operation.ToString().ToLowerInvariant() +
          (flow.Label == null ? "" : " label=" + Quote(flow.Label)));
        for (var index = 0; index < flow.Values.Count; index++) {
          Line(builder, depth + 1, "value " + index.ToString(CultureInfo.InvariantCulture));
          RenderValue(builder, flow.Values[index], depth + 2);
        }
        break;
      default: Line(builder, depth, "unknown " + statement.GetType().Name); break;
    }
  }

  private static void RenderValue(StringBuilder builder, ExpressionAst expression, int depth, string prefix = "") {
    switch (expression) {
      case StringExpressionAst text: Line(builder, depth, prefix + "string " + Quote(text.Value)); break;
      case NumberExpressionAst number: Line(builder, depth, prefix + "number " + number.Value.ToString("R", CultureInfo.InvariantCulture)); break;
      case BooleanExpressionAst boolean: Line(builder, depth, prefix + "boolean " + (boolean.Value ? "true" : "false")); break;
      case NullExpressionAst: Line(builder, depth, prefix + "null"); break;
      case RootExpressionAst root:
        Line(builder, depth, prefix + (root.IsSmart ? "resolve.smart " : "resolve.root ") + Quote(root.Name)); break;
      case MemberExpressionAst {Receiver: RootExpressionAst {IsSmart: false, Name: "local" or "var" or "tar" or "carry"} storage} member:
        Line(builder, depth, prefix + "load." + storage.Name + " " + Quote(member.Member)); break;
      case MemberExpressionAst {Receiver: RootExpressionAst {IsSmart: false, Name: "this" or "target" or "attr"} host} member:
        Line(builder, depth, prefix + "load.host." + host.Name + " " + Quote(member.Member)); break;
      case MemberExpressionAst member:
        Line(builder, depth, prefix + "select " + Quote(member.Member));
        Line(builder, depth + 1, "receiver");
        RenderValue(builder, member.Receiver, depth + 2);
        break;
      case CallExpressionAst call:
        Line(builder, depth, prefix + "call " + Quote(call.Name) + (call.CoerceBoolean ? " -> boolean" : ""));
        for (var index = 0; index < call.Arguments.Count; index++) {
          RenderValue(builder, call.Arguments[index], depth + 1,
            "[" + index.ToString(CultureInfo.InvariantCulture) + "] ");
        }
        break;
      case UnaryExpressionAst unary:
        Line(builder, depth, prefix + unary.Operation.ToString().ToLowerInvariant());
        RenderValue(builder, unary.Value, depth + 1);
        break;
      case FallbackExpressionAst fallback:
        Line(builder, depth, prefix + "fallback");
        Line(builder, depth + 1, "value"); RenderValue(builder, fallback.Value, depth + 2);
        Line(builder, depth + 1, "otherwise"); RenderValue(builder, fallback.Fallback, depth + 2);
        break;
      case TupleExpressionAst tuple:
        Line(builder, depth, prefix + "tuple");
        for (var index = 0; index < tuple.Values.Count; index++) {
          RenderValue(builder, tuple.Values[index], depth + 1,
            "[" + index.ToString(CultureInfo.InvariantCulture) + "] ");
        }
        break;
      case TableExpressionAst table:
        Line(builder, depth, prefix + "table");
        foreach (var entry in table.Entries) {
          Line(builder, depth + 1, "entry " + Quote(entry.Key));
          RenderValue(builder, entry.Value, depth + 2);
        }
        break;
      case InterpolationExpressionAst interpolation:
        Line(builder, depth, prefix + "interpolate");
        for (var index = 0; index < interpolation.Parts.Count; index++) {
          RenderValue(builder, interpolation.Parts[index], depth + 1,
            "[" + index.ToString(CultureInfo.InvariantCulture) + "] ");
        }
        break;
      case SelectionExpressionAst selection: RenderSelection(builder, selection, depth, prefix); break;
      default: Line(builder, depth, prefix + "unknown " + expression.GetType().Name); break;
    }
  }

  private static void RenderSelection(StringBuilder builder, SelectionExpressionAst selection, int depth, string prefix = "") {
    Line(builder, depth, prefix + "selection");
    Line(builder, depth + 1, "selector");
    if (selection.Selector == null) Line(builder, depth + 2, "boolean true");
    else RenderValue(builder, selection.Selector, depth + 2);
    for (var branchIndex = 0; branchIndex < selection.Branches.Count; branchIndex++) {
      var branch = selection.Branches[branchIndex];
      Line(builder, depth + 1, "branch " + branchIndex.ToString(CultureInfo.InvariantCulture) +
        (branch.IsTransformation ? " transform" : " match"));
      for (var conditionIndex = 0; conditionIndex < branch.Conditions.Count; conditionIndex++) {
        Line(builder, depth + 2, "condition " + conditionIndex.ToString(CultureInfo.InvariantCulture));
        RenderValue(builder, branch.Conditions[conditionIndex], depth + 3);
      }
      Line(builder, depth + 2, "result");
      RenderResult(builder, branch.Result, depth + 3);
    }
    Line(builder, depth + 1, "fallback");
    RenderResult(builder, selection.Fallback, depth + 2);
  }

  private static void RenderResult(StringBuilder builder, HixAst result, int depth) {
    if (result == null) Line(builder, depth, "null");
    else if (result is ExpressionAst expression) RenderValue(builder, expression, depth);
    else if (result is BlockStatementAst block) RenderBlock(builder, block, depth);
    else if (result is StatementAst statement) RenderStatement(builder, statement, depth);
    else Line(builder, depth, "unknown " + result.GetType().Name);
  }

  private static void Line(StringBuilder builder, int depth, string value) =>
    builder.Append(' ', depth * 2).AppendLine(value);

  private static string Quote(string value) => "\"" + (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

  private void Visit(HixAst node, HashSet<HixAst> visited) {
    if (!visited.Add(node)) return;
    if (node is BlockStatementAst block) {
      LanguageExecution.LowerBlock(block, blocks);
      return;
    }
    foreach (var child in node.SemanticChildren) Visit(child, visited);
  }
}
