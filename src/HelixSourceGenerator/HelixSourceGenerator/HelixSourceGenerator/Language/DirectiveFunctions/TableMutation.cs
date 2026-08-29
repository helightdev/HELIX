using System.Collections.Generic;
using System.Globalization;

namespace HelixSourceGenerator.Language.DirectiveFunctions;

internal abstract class TableMutationDirectiveFunction(string name, int arguments)
  : DirectiveFunctionDefinition(name, DirectiveOperandKind.Value) {
  protected override int MaximumArguments => arguments;

  internal override bool Validate(IReadOnlyList<string> values, string operand, out string error) {
    if (values.Count == arguments && !string.IsNullOrEmpty(values[0])) {
      error = null;
      return true;
    }
    error = Name + " requires " + (arguments == 1 ? "a local name" : "a local name and key");
    return false;
  }

  protected abstract bool TryGetKey(
    DirectiveFunctionInvocation invocation, MixinExpressionTable table,
    out string key, out string error
  );

  internal override bool Invoke(DirectiveFunctionInvocation invocation, out string error) {
    if (!invocation.ResolveArgument(0, out var local, out error)) return false;
    if (string.IsNullOrEmpty(local)) {
      error = Name + " local name is empty";
      return false;
    }
    if (!invocation.Evaluate(out var item, out error)) return false;
    var table = invocation.TryGetLocal(local, out var current) && current is MixinExpressionTable existing
      ? existing
      : new MixinExpressionTable();
    if (!TryGetKey(invocation, table, out var key, out error)) return false;
    invocation.Store(local, table.Put(key, item));
    return true;
  }
}

internal sealed class PutDirectiveFunction() : TableMutationDirectiveFunction("PUT", 2) {
  protected override bool TryGetKey(
    DirectiveFunctionInvocation invocation, MixinExpressionTable table,
    out string key, out string error
  ) {
    return invocation.ResolveArgument(1, out key, out error);
  }
}

internal sealed class PushDirectiveFunction() : TableMutationDirectiveFunction("PUSH", 1) {
  protected override bool TryGetKey(
    DirectiveFunctionInvocation invocation, MixinExpressionTable table,
    out string key, out string error
  ) {
    key = table.Count.ToString(CultureInfo.InvariantCulture);
    error = null;
    return true;
  }
}