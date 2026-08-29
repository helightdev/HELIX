using System.Globalization;
using System.Linq;

namespace HELIX.SourceGen.Expressions;

internal enum TableFunctionKind { Put, Remove, Push, Pop }

internal sealed class AsTableFunction : FunctionDefinition {
  internal AsTableFunction() : base("table", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (typed.BackingValue is not MixinExpressionTable) {
      var table = new MixinExpressionTable();
      value = typed.Exists ? table.Put("0", typed) : table;
    }
    error = null;
    return true;
  }
}

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
        value = table.Put(
          MixinExpressionEvaluator.Render(property.Values[0]),
          MixinValue.From(property.Values[1], context)
        ); break;
      case TableFunctionKind.Remove: value = table.Remove(invocation.Argument); break;
      case TableFunctionKind.Push:
        value = table.Put(
          table.Count.ToString(CultureInfo.InvariantCulture),
          MixinValue.From(property.Values[0], context)
        ); break;
      case TableFunctionKind.Pop:
        value = table.Remove((table.Count - 1).ToString(CultureInfo.InvariantCulture)); break;
    }
    error = null;
    return true;
  }
}

internal enum TableJoinKind { Keys, Values, Entries }

internal sealed class TableJoinFunction : FunctionDefinition {
  private readonly TableJoinKind _kind;
  internal TableJoinFunction(string name, int arguments, TableJoinKind kind) : base(name, arguments, arguments) {
    _kind = kind;
  }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    if (value is not MixinExpressionTable table) {
      error = ":" + Name + " requires a table";
      return false;
    }
    value = _kind switch {
      TableJoinKind.Keys => string.Join(invocation.Argument, table.Entries.Select(item => item.Key)),
      TableJoinKind.Values => string.Join(invocation.Argument, table.Entries.Select(item => item.Value.Render())),
      TableJoinKind.Entries => string.Join(
        invocation.Arguments[1],
        table.Entries.Select(item => item.Key + invocation.Arguments[0] + item.Value.Render())
      ),
      _ => ""
    };
    error = null;
    return true;
  }
}

internal enum TableTransformKind { MapValues, Map, Filter }

internal sealed class TableTransformFunction : FunctionDefinition {
  private readonly TableTransformKind _kind;
  internal TableTransformFunction(string name, TableTransformKind kind) : base(name, 1, 1) { _kind = kind; }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    if (value is not MixinExpressionTable table) {
      error = ":" + Name + " requires a table";
      return false;
    }
    value = new MixinTransformRequest(table, _kind, invocation.Argument);
    error = null;
    return true;
  }
}
