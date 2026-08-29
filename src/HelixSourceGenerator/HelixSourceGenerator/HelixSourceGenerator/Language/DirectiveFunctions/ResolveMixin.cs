using System.Collections.Generic;

namespace HelixSourceGenerator.Language.DirectiveFunctions;

internal sealed class ResolveMixinDirectiveFunction()
  : DirectiveFunctionDefinition("RESOLVE_MIXIN", DirectiveOperandKind.Value, 0) {
  internal override bool Validate(IReadOnlyList<string> arguments, string operand, out string error) {
    if (arguments.Count == 1 && !string.IsNullOrEmpty(arguments[0])) {
      error = null;
      return true;
    }
    error = "RESOLVE_MIXIN requires a local name";
    return false;
  }

  internal override bool Invoke(DirectiveFunctionInvocation invocation, out string error) {
    if (invocation.Context is not IMixinExpressionSignatureContext context) {
      error = "the expression context does not support mixin resolution";
      return false;
    }
    if (!invocation.ResolveArgument(0, out var local, out error)) return false;
    if (string.IsNullOrEmpty(local)) {
      error = "RESOLVE_MIXIN local name is empty";
      return false;
    }
    if (!invocation.Interpolate(out var target, out error)) return false;
    if (string.IsNullOrWhiteSpace(target)) {
      error = "RESOLVE_MIXIN target is empty";
      return false;
    }
    if (!context.TryResolveMixin(target, out var callable, out error)) return false;
    invocation.Store(local, callable);
    return true;
  }
}