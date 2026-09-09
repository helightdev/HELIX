using System.Linq;
using Hix.Runtime;
using static Hix.Runtime.LanguageExecution;

namespace Hix.Functions;

internal sealed class CatchFunction() : FunctionDefinition("catch", [
  new(HixValueKind.Any, [HixValueKind.Any]),
  new(HixValueKind.Any, [HixValueKind.Any, HixValueKind.Function])
]) {
  public override bool AcceptsErrors => true;
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) { var execution = invocation.Execution; return arguments[0] is not ErrorHixValue ? arguments[0] : arguments.Length == 1
      ? NullHixValue.Instance : execution.Callback(arguments[1], [arguments[0]], line); }
}

internal sealed class CallbackFunction(string name) : FunctionDefinition(name, [
  new(HixValueKind.Any, [HixValueKind.Function]),
  new(HixValueKind.Any, [HixValueKind.Function, HixValueKind.Any])
]) {
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) { var execution = invocation.Execution; return execution.Callback(arguments[0], arguments.Length == 2 ? [arguments[1]] : [], line); }
}

internal sealed class MatchFunction() : FunctionDefinition("match", 0, HixValueKind.Any, HixValueKind.Null,
  [HixValueKind.Any], variadic: true) {
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) {
    var execution = invocation.Execution;
    if (!arguments.All(value => value.IsTruthy(execution.Context)))
      execution.Break(line);
    return NullHixValue.Instance;
  }
}

internal sealed class LogFunction(string name, bool dump) : FunctionDefinition(name, 1, HixValueKind.Any,
  dump ? HixValueKind.Any : HixValueKind.Null, [HixValueKind.Any]) {
  public override bool HasEffects => true;
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) {
    var execution = invocation.Execution;
    var rendered = execution.RenderText(arguments[0]);
    if (rendered is ErrorHixValue) return rendered;
    execution.Logs.Add(new HixExpressionLog(
      (dump ? arguments[0].Kind.ToString().ToLowerInvariant() + ": " : "") + execution.Text(rendered), line));
    return dump ? arguments[0] : NullHixValue.Instance;
  }
}

internal sealed class DeriveFunction() : FunctionDefinition("derive", 1, HixValueKind.Tuple,
  HixValueKind.Tuple, [HixValueKind.Tuple]) {
  public override bool HasEffects => true;
  public override bool RequiresPrelude => true;
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) { var execution = invocation.Execution; return execution.IsPrelude ? execution.Derive(arguments[0], line) : execution.Context.Error("derive requires the prelude pass"); }
}

internal sealed class CollectionTransformFunction(
  string name, CollectionFunctions.TransformKind operation, bool reduce = false
) : FunctionDefinition(name, reduce ? [
    new FunctionSignature(HixValueKind.Any, [HixValueKind.Tuple, HixValueKind.Function, HixValueKind.Any])
  ] : [
    new FunctionSignature(operation is CollectionFunctions.TransformKind.Any or CollectionFunctions.TransformKind.All
      ? HixValueKind.Bool : HixValueKind.Tuple, [HixValueKind.Tuple, HixValueKind.Function])
  ]) {
  public override IHixValue Execute(HixExecutionContext invocation, IHixValue[] arguments, int line) { var execution = invocation.Execution; return CollectionFunctions.Transform(execution, arguments, line, operation); }
}
