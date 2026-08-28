using System.Globalization;

namespace HELIX.SourceGen.Expressions;

internal enum TableFunctionKind { Put, Remove, Push, Pop }

internal sealed class TableFunction : FunctionDefinition {
  private readonly TableFunctionKind _kind;

  internal TableFunction(string name, int arguments, TableFunctionKind kind) : base(name, arguments, arguments) {
    _kind = kind;
  }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var property = (MixinExpressionProperty)invocation;
    var table = value as MixinExpressionTable ?? new MixinExpressionTable();
    switch (_kind) {
      case TableFunctionKind.Put:
        value = table.Put(MixinExpressionEvaluator.Render(property.Values[0]), property.Values[1]); break;
      case TableFunctionKind.Remove: value = table.Remove(invocation.Argument); break;
      case TableFunctionKind.Push:
        value = table.Put(table.Count.ToString(CultureInfo.InvariantCulture), property.Values[0]); break;
      case TableFunctionKind.Pop:
        value = table.Remove((table.Count - 1).ToString(CultureInfo.InvariantCulture)); break;
    }
    error = null;
    return true;
  }
}
