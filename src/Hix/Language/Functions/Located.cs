using System.Linq;
using Hix.Compiler;
using Hix.Runtime;
using static Hix.Runtime.HixCoroutine;

namespace Hix.Functions;

public sealed class CatchFunction() : FunctionDefinition("catch", [
  new(HixValueKind.Any, [HixValueKind.Any]),
  new(HixValueKind.Any, [HixValueKind.Any, HixValueKind.Function])
]) {
  public override string Documentation => "Returns a non-error value unchanged, or null for an error.";
  public override bool AcceptsErrors => true;
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) { return arguments[0] is not ErrorHixValue ? arguments[0] : arguments.Length == 1
      ? NullHixValue.Instance : execution.Callback(arguments[1], [arguments[0]], line); }
}

public sealed class CallbackFunction(string name) : FunctionDefinition(name, [
  new(HixValueKind.Any, [HixValueKind.Function]),
  new(HixValueKind.Any, [HixValueKind.Function, HixValueKind.Any])
]) {
  public override string Documentation => "Invokes a function value with an optional parameter.";
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) { return execution.Callback(arguments[0], arguments.Length == 2 ? [arguments[1]] : [], line); }
}

public sealed class MatchFunction() : FunctionDefinition("match", 0, HixValueKind.Any, HixValueKind.Null,
  [HixValueKind.Any], variadic: true) {
  public override string Documentation => "Leaves the current block if any supplied condition is false.";
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) {
    if (!arguments.All(value => value.IsTruthy(execution)))
      execution.Break(line);
    return NullHixValue.Instance;
  }
}

public sealed class LogFunction(string name, bool dump) : FunctionDefinition(name, 1, HixValueKind.Any,
  dump ? HixValueKind.Any : HixValueKind.Null, [HixValueKind.Any]) {
  public override string Documentation => dump ? "Logs the kind and rendered value, then returns the value." : "Appends a rendered value to the execution log.";
  public override bool HasEffects => true;
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) {
    var rendered = execution.RenderText(arguments[0]);
    if (rendered is ErrorHixValue) return rendered;
    execution.Context.Logs.Add(new HixLog(
      dump ? HixString.Dynamic(arguments[0].Kind.ToString().ToLowerInvariant() + ": " + execution.ResolveText(rendered)) : execution.Text(rendered), line));
    return dump ? arguments[0] : NullHixValue.Instance;
  }
}

public sealed class DeriveFunction() : FunctionDefinition("derive", 1, HixValueKind.Tuple,
  HixValueKind.Tuple, [HixValueKind.Tuple]) {
  public override string Documentation => "Applies derivation mixins to a tuple of records during the prelude and returns the resulting records.";
  public override bool HasEffects => true;
  public override bool RequiresPrelude => true;
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) {
    if (!HixExpressionContext.IsPrelude(execution)) return execution.Error("derive requires the prelude pass");
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
      foreach (var provider in execution.Program.Derivations) {
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
  string name, CollectionFunctions.TransformKind operation, bool reduce = false
) : FunctionDefinition(name, reduce ? [
    new FunctionSignature(HixValueKind.Any, [HixValueKind.Tuple, HixValueKind.Function, HixValueKind.Any])
  ] : [
    new FunctionSignature(operation is CollectionFunctions.TransformKind.Any or CollectionFunctions.TransformKind.All
      ? HixValueKind.Bool : HixValueKind.Tuple, [HixValueKind.Tuple, HixValueKind.Function])
  ]) {
  public override string Documentation => Name switch {
    "map" => "Invokes a callback for each tuple element and returns the resulting tuple.",
    "where" => "Returns tuple elements for which the callback is truthy.",
    "any" => "Tests whether any tuple element satisfies the callback.",
    "all" => "Tests whether every tuple element satisfies the callback.",
    "reduce" => "Combines tuple elements through a callback and accumulator.",
    _ => "Applies the callback to tuple elements."
  };
  public override IHixValue Execute(HixCoroutine execution, IHixValue[] arguments, int line) { return CollectionFunctions.Transform(execution, arguments, line, operation); }
}
