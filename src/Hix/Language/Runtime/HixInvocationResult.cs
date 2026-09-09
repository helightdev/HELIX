namespace Hix.Runtime;
public readonly record struct HixInvocationResult(IHixValue Value, HixExecutionResult Execution) {
  public HixStringPool Strings => Execution.Strings;
  public bool Success => Execution.Success;
  public HixString Error => Execution.Error;
}
