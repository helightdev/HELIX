using System.Linq;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal sealed class CatchFunction() : FunctionDefinition("catch", [
  new(MixinValueKind.Any, [MixinValueKind.Any]),
  new(MixinValueKind.Any, [MixinValueKind.Any, MixinValueKind.Function])
]) {
  public override bool AcceptsErrors => true;
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) =>
    arguments[0] is not ErrorMixinValue ? arguments[0] : arguments.Length == 1
      ? NullMixinValue.Instance : execution.Callback(arguments[1], [arguments[0]], line);
}

internal sealed class CallbackFunction(string name) : FunctionDefinition(name, [
  new(MixinValueKind.Any, [MixinValueKind.Function]),
  new(MixinValueKind.Any, [MixinValueKind.Function, MixinValueKind.Any])
]) {
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) =>
    execution.Callback(arguments[0], arguments.Length == 2 ? [arguments[1]] : [], line);
}

internal sealed class MatchFunction() : FunctionDefinition("match", 0, MixinValueKind.Any, MixinValueKind.Null,
  [MixinValueKind.Any], variadic: true) {
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) {
    if (!arguments.All(value => value.IsTruthy(execution.Context)))
      throw new Flow(Compiler.ControlFlowKind.Break, null, NullMixinValue.Instance, line);
    return NullMixinValue.Instance;
  }
}

internal sealed class LogFunction(string name, bool dump) : FunctionDefinition(name, 1, MixinValueKind.Any,
  dump ? MixinValueKind.Any : MixinValueKind.Null, [MixinValueKind.Any]) {
  public override bool HasEffects => true;
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) {
    execution.Logs.Add(new MixinExpressionLog(
      dump ? arguments[0].Kind.ToString().ToLowerInvariant() + ": " + execution.Text(arguments[0]) : execution.Text(arguments[0]), line));
    return dump ? arguments[0] : NullMixinValue.Instance;
  }
}

internal sealed class DeriveFunction() : FunctionDefinition("derive", 1, MixinValueKind.Tuple,
  MixinValueKind.Tuple, [MixinValueKind.Tuple]) {
  public override bool HasEffects => true;
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) =>
    execution.IsPrelude ? execution.Derive(arguments[0], line) : execution.Context.Error("derive requires the prelude pass");
}

internal sealed class CollectionTransformFunction(
  string name, CollectionFunctions.TransformKind operation, bool reduce = false
) : FunctionDefinition(name, reduce ? [
    new FunctionSignature(MixinValueKind.Any, [MixinValueKind.Tuple, MixinValueKind.Function, MixinValueKind.Any])
  ] : [
    new FunctionSignature(operation is CollectionFunctions.TransformKind.Any or CollectionFunctions.TransformKind.All
      ? MixinValueKind.Bool : MixinValueKind.Tuple, [MixinValueKind.Tuple, MixinValueKind.Function])
  ]) {
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) =>
    CollectionFunctions.Transform(execution, arguments, line, operation);
}
