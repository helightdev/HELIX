using System.Linq;
using Hix.Compiler;
using Hix.Runtime;
using static Hix.Runtime.HixCoroutine;

namespace Hix.Functions;

public sealed class CatchFunction() : FunctionDefinition("catch") {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) { return arguments[0] is not ErrorHixValue ? arguments[0] : arguments.Length == 1
      ? NullHixValue.Instance : execution.Callback(arguments[1], [arguments[0]], line); }
}

public sealed class CallbackFunction(string name) : FunctionDefinition(name) {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) { return execution.Callback(arguments[0], arguments.Skip(1).ToArray(), line); }
}

public sealed class MatchFunction() : FunctionDefinition("match") {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) {
    if (!arguments.All(value => value.IsTruthy(execution)))
      execution.Break(line);
    return NullHixValue.Instance;
  }
}

public sealed class LogFunction(string name, bool dump) : FunctionDefinition(name) {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) {
    var rendered = execution.RenderText(arguments[0]);
    if (rendered is ErrorHixValue) return rendered;
    execution.Context.Logs.Add(new HixLog(
      dump ? HixString.Dynamic(arguments[0].Kind.ToString().ToLowerInvariant() + ": " + execution.ResolveText(rendered)) : execution.Text(rendered), line));
    return dump ? arguments[0] : NullHixValue.Instance;
  }
}

public sealed class DeriveFunction() : FunctionDefinition("derive") {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) {
    if (!HixExpressionContext.IsPrelude(execution)) return execution.Error("derive requires the prelude pass");
    if (arguments.Length != 1) return execution.Error("derive requires one tuple of records");
    if (arguments[0] is not TupleHixValue tuple) return execution.Error("derive requires a tuple of records");
    if (tuple.Values.Count == 0) return TupleHixValue.Empty;

    // One batch transaction; providers enter the ordinary heap-backed function interpreter.
    var checkpoint = execution.Context.CaptureCheckpoint(execution);
    var result = new IHixValue[tuple.Values.Count];
    for (var index = 0; index < tuple.Values.Count; index++) {
      if (tuple.Values[index] is not HixTableValue record)
        return Fail(execution, checkpoint, index, null, execution.Error("record must be a table"), line);
      if (execution.Backend.ValidateDerivationRecord(execution, record) is { } invalidRecord)
        return Fail(execution, checkpoint, index, null, invalidRecord, line);
      if (!record.TryGetValue(execution, execution.ResolveString("value"), out var current))
        return Fail(execution, checkpoint, index, null, execution.Error("record must contain value"), line);
      foreach (var provider in execution.Machine.Derivations) {
        // Existing frames already identify active providers; no per-thread recursion set is needed.
        foreach (var expression in provider.Expressions)
          if (execution.IsExecuting(expression.Function))
            return Fail(execution, checkpoint, index, provider, execution.Error("recursive derivation"), provider.Line);
        var providerLocals = HixThread.Current.RentLocals();
        try {
          foreach (var expression in provider.Expressions) {
            var parameter = CollectionFunctions.Put(execution, record, "value", current);
            var completion = execution.ExecuteFunction(expression.Function, provider.Scope, parameter, providerLocals);
            if (completion.Kind == BytecodeFlow.Error)
              return Fail(execution, checkpoint, index, provider, completion.Value, completion.Line);
            if (completion.Kind == BytecodeFlow.Normal)
              return Fail(execution, checkpoint, index, provider,
                execution.Error("derivation must explicitly return a value"), expression.Line);
            if (completion.Kind != BytecodeFlow.Return)
              return Fail(execution, checkpoint, index, provider,
                execution.Error("control flow cannot cross derivation boundaries: " + completion.Kind),
                completion.Line, true);
            current = completion.Value;
            if (current is ErrorHixValue)
              return Fail(execution, checkpoint, index, provider, current, expression.Line);
          }
        } finally {
          HixThread.Current.ReturnLocals(providerLocals);
        }
      }
      result[index] = CollectionFunctions.Put(execution, record, "value", current);
    }
    return new TupleHixValue(result);
  }

  private static IHixValue Fail(HixCoroutine execution, HixContextCheckpoint checkpoint, int index,
    BytecodeDerivation provider, IHixValue error, int line, bool control = false) {
    execution.Context.RollbackCheckpoint(execution, checkpoint, preserveLogs: true);
    return control ? error : execution.Error("derive entry " + index +
      (provider == null ? "" : " in '" + provider.Name + "'") + " at line " + line + ": " +
      execution.ResolveText(error));
  }
}

public sealed class CollectionTransformFunction(
  string name, CollectionFunctions.TransformKind operation
) : FunctionDefinition(name) {
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) { return CollectionFunctions.Transform(execution, arguments, line, operation); }
}
