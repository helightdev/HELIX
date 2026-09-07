using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mixins.Compiler;

namespace Mixins.Runtime;

internal sealed partial class LanguageExecution {
  // These delegates capture immutable operands and child operations, never an execution context.
  private delegate IMixinValue ValueOperation(LanguageExecution execution, bool check = false);

  internal static Action<LanguageExecution> LowerBlock(BlockStatementAst block,
    Dictionary<BlockStatementAst, Action<LanguageExecution>> blocks) {
    if (blocks.TryGetValue(block, out var existing)) return existing;
    var labels = new Dictionary<string, int>(StringComparer.Ordinal);
    var operations = new Action<LanguageExecution>[block.Statements.Count];
    for (var index = 0; index < operations.Length; index++) {
      var statement = block.Statements[index];
      var label = statement switch {
        ControlFlowStatementAst {Operation: ControlFlowKind.Label} marker => marker.Label,
        BlockStatementAst nested => nested.Label, _ => null
      };
      if (label != null) {
        if (labels.ContainsKey(label)) throw new ArgumentException("duplicate label '" + label + "'");
        labels.Add(label, index);
      }
      operations[index] = LowerStatement(statement, blocks);
    }
    Action<LanguageExecution> result = execution => {
      for (var pc = 0; pc < operations.Length; pc++) {
        try { operations[pc](execution); }
        catch (Flow flow) {
          if (flow.Kind == ControlFlowKind.Break) return;
          if (flow.Kind == ControlFlowKind.Continue) { pc = -1; continue; }
          if (flow.Kind == ControlFlowKind.Goto && labels.TryGetValue(flow.Label, out var target)) {
            pc = target - 1;
            continue;
          }
          throw;
        }
      }
    };
    blocks.Add(block, result);
    return result;
  }

  private static Action<LanguageExecution> LowerStatement(StatementAst statement,
    Dictionary<BlockStatementAst, Action<LanguageExecution>> blocks) {
    Action<LanguageExecution> operation;
    var line = statement.Line;
    switch (statement) {
      case BlockStatementAst block: operation = LowerBlock(block, blocks); break;
      case AssignmentStatementAst assignment: {
        var value = LowerValue(assignment.Value, blocks);
        var name = assignment.Name;
        var space = assignment.Storage;
        Func<LanguageExecution, Dictionary<string, IMixinValue>> storage = space switch {
          StorageSpace.Local => execution => execution.locals,
          StorageSpace.Variable => execution => execution.variables,
          StorageSpace.Target => execution => execution.targetVariables,
          _ => execution => execution.carries
        };
        operation = execution => {
          if (execution.pure && space != StorageSpace.Local)
            throw new Failure(execution.context.Error("pure functions cannot mutate shared storage"), line);
          if (space == StorageSpace.Carry && !execution.prelude)
            throw new Failure(execution.context.Error("carry assignments require the prelude pass"), line);
          storage(execution)[name] = value(execution);
        };
        break;
      }
      case InvocationStatementAst invocation: {
        var value = LowerValue(invocation.Call, blocks);
        operation = execution => { value(execution); };
        break;
      }
      case SelectionStatementAst selection: {
        var value = LowerValue(selection.Selection, blocks);
        operation = execution => { value(execution); };
        break;
      }
      case ControlFlowStatementAst {Operation: ControlFlowKind.Label}: operation = _ => { }; break;
      case ControlFlowStatementAst flow: {
        var values = flow.Values.Select(value => LowerValue(value, blocks)).ToArray();
        var kind = flow.Operation;
        var label = flow.Label;
        operation = execution => throw new Flow(kind, label, Pack(Values(values, execution)), line);
        break;
      }
      default: throw new ArgumentException("unknown statement at line " + line);
    }
    return execution => { execution.Tick(line); operation(execution); };
  }

  private static IMixinValue[] Values(ValueOperation[] operations, LanguageExecution execution) {
    var values = new IMixinValue[operations.Length];
    for (var index = 0; index < values.Length; index++) values[index] = operations[index](execution);
    return values;
  }

  private static ValueOperation LowerValue(ExpressionAst expression,
    Dictionary<BlockStatementAst, Action<LanguageExecution>> blocks) {
    Func<LanguageExecution, IMixinValue> operation;
    var line = expression.Line;
    switch (expression) {
      case StringExpressionAst text: {
        var constant = String(text.Value);
        operation = _ => constant;
        break;
      }
      case RootExpressionAst root: {
        var name = root.Name;
        var smart = root.IsSmart;
        operation = execution => execution.Root(name, smart);
        break;
      }
      case MemberExpressionAst member: {
        var name = member.Member;
        if (member.Receiver is RootExpressionAst {IsSmart: false, Name: "this" or "target" or "attr"} root) {
          var host = root.Name switch {"this" => MixinExpressionRoot.This, "target" => MixinExpressionRoot.Target, _ => MixinExpressionRoot.Attribute};
          operation = execution => execution.pure ? execution.context.Error("pure functions cannot read host roots")
            : !execution.prelude ? execution.context.Error("host members require the prelude pass")
            : execution.context.Resolve(host, execution.context.ResolveString(name));
        } else if (member.Receiver is RootExpressionAst {IsSmart: false, Name: "local" or "var" or "tar" or "carry"} storage) {
          // Direct keyed reads must not materialize the entire storage table for every member access.
          var space = storage.Name;
          operation = execution => {
            execution.Tick(line);
            if (execution.pure && space != "local") return execution.context.Error("pure functions cannot read shared storage");
            if (space == "carry" && execution.prelude) return execution.context.Error("carry storage is write-only during the prelude pass");
            var values = space switch {"local" => execution.locals, "var" => execution.variables,
              "tar" => execution.targetVariables, _ => execution.carries};
            return values.TryGetValue(name, out var found) ? found : space == "carry"
              ? execution.context.Error("uninitialized carry '" + name + "'") : NullMixinValue.Instance;
          };
        } else {
          var receiver = LowerValue(member.Receiver, blocks);
          operation = execution => {
            var value = receiver(execution);
            if (value is LiteralMixinValue or NumberMixinValue or BooleanMixinValue or NullMixinValue or KindMixinValue)
              return execution.context.Error("member selection requires a container or host selector");
            return value.Select(execution.context, execution.context.ResolveString(name));
          };
        }
        break;
      }
      case InterpolationExpressionAst interpolation: {
        var parts = interpolation.Parts.Select(part => LowerValue(part, blocks)).ToArray();
        operation = execution => {
          var text = new StringBuilder();
          foreach (var part in parts) text.Append(execution.Text(part(execution)));
          return String(text.ToString());
        };
        break;
      }
      case TupleExpressionAst tuple: {
        var values = tuple.Values.Select(value => LowerValue(value, blocks)).ToArray();
        operation = execution => new TupleMixinValue(Values(values, execution));
        break;
      }
      case TableExpressionAst table: {
        var keys = table.Entries.Select(entry => entry.Key).ToArray();
        var values = table.Entries.Select(entry => LowerValue(entry.Value, blocks)).ToArray();
        var duplicate = keys.GroupBy(key => key, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1)?.Key;
        operation = execution => {
          if (duplicate != null) return execution.context.Error("duplicate table key '" + duplicate + "'");
          var entries = new KeyValuePair<MixinString, IMixinValue>[keys.Length];
          for (var index = 0; index < entries.Length; index++)
            entries[index] = new(execution.context.ResolveString(keys[index]), values[index](execution));
          return new MixinTableValue(entries);
        };
        break;
      }
      case UnaryExpressionAst unary: {
        var value = LowerValue(unary.Value, blocks);
        operation = unary.Operation == UnaryOperation.Check ? execution => value(execution, true)
          : execution => Bool(!value(execution).IsTruthy(execution.context));
        break;
      }
      case FallbackExpressionAst fallback: {
        var value = LowerValue(fallback.Value, blocks);
        var other = LowerValue(fallback.Fallback, blocks);
        operation = execution => { var result = value(execution); return result is NullMixinValue ? other(execution) : result; };
        break;
      }
      case CallExpressionAst call: {
        var name = call.Name;
        var values = call.Arguments.Select(argument => LowerValue(argument, blocks)).ToArray();
        var definitions = FunctionLibrary.Resolve(name, values.Length);
        var booleanResult = call.CoerceBoolean;
        operation = execution => {
          var result = execution.Call(name, Values(values, execution), line, definitions);
          return booleanResult && result is not ErrorMixinValue ? Bool(result.IsTruthy(execution.context)) : result;
        };
        break;
      }
      case SelectionExpressionAst selection: {
        var selected = selection.Selector == null ? null : LowerValue(selection.Selector, blocks);
        var branches = selection.Branches.Select(branch => (
          Conditions: branch.Conditions.Select(condition => LowerValue(condition, blocks)).ToArray(),
          Result: LowerResult(branch.Result, blocks), Truth: branch.IsTransformation || selected == null)).ToArray();
        var fallback = LowerResult(selection.Fallback, blocks);
        operation = execution => {
          var previous = execution.selector;
          execution.selector = selected == null ? BooleanMixinValue.True : selected(execution);
          try {
            foreach (var branch in branches) {
              var matches = true;
              foreach (var condition in branch.Conditions) {
                var value = condition(execution);
                if (branch.Truth ? !value.IsTruthy(execution.context) : !execution.Equal(execution.selector, value)) {
                  matches = false;
                  break;
                }
              }
              if (matches) return branch.Result(execution);
            }
            return fallback(execution);
          } finally { execution.selector = previous; }
        };
        break;
      }
      default: throw new ArgumentException("unknown value expression at line " + line);
    }
    return (execution, check) => {
      execution.Tick(line);
      try {
        var value = operation(execution);
        if (value is ErrorMixinValue {IsChecked: false} && !check) throw new Failure(value, line);
        return check && value is ErrorMixinValue error ? error with {IsChecked = true} : value;
      } catch (Failure failure) when (check) {
        return failure.Value is ErrorMixinValue error ? error with {IsChecked = true} : failure.Value;
      }
    };
  }

  private static Func<LanguageExecution, IMixinValue> LowerResult(LanguageAst result,
    Dictionary<BlockStatementAst, Action<LanguageExecution>> blocks) {
    if (result is ExpressionAst expression) {
      var value = LowerValue(expression, blocks);
      return execution => value(execution);
    }
    if (result is StatementAst statement) {
      var operation = LowerStatement(statement, blocks);
      return execution => { operation(execution); return NullMixinValue.Instance; };
    }
    return _ => NullMixinValue.Instance;
  }
}
