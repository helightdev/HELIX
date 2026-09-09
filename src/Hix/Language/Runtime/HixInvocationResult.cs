namespace Hix.Runtime;
public sealed record HixInvocationResult(IHixValue Value, HixExpressionResult Execution) {
  public bool Success => Execution.Success;
  public string Error => Execution.Error;
}
