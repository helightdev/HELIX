using System.Globalization;
using System.Linq;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class AsTableFunction : FunctionDefinition {
  internal AsTableFunction() : base("table", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    if (typed.Value is not MixinExpressionTable) {
      var table = new MixinExpressionTable();
      value = typed.Exists ? table.Put("0", typed) : table;
    }
    error = null;
    return true;
  }
}

internal abstract class TableMutationFunction(string name, int arguments)
  : FunctionDefinition(name, arguments, arguments) {
  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var property = (MixinExpressionProperty)invocation;
    var table = value as MixinExpressionTable ?? new MixinExpressionTable();
    value = Mutate(table, property, context);
    error = null;
    return true;
  }

  protected abstract MixinExpressionTable Mutate(
    MixinExpressionTable table, MixinExpressionProperty property, IMixinExpressionContext context
  );
}

internal sealed class PutFunction() : TableMutationFunction("put", 2) {
  protected override MixinExpressionTable Mutate(
    MixinExpressionTable table, MixinExpressionProperty property, IMixinExpressionContext context
  ) {
    return table.Put(
      MixinValue.From(property.Values[0], context).Render(),
      MixinValue.From(property.Values[1], context)
    );
  }
}

internal sealed class RemoveFunction() : TableMutationFunction("remove", 1) {
  protected override MixinExpressionTable Mutate(
    MixinExpressionTable table, MixinExpressionProperty property, IMixinExpressionContext context
  ) {
    return table.Remove(property.Argument);
  }
}

internal sealed class PushFunction() : TableMutationFunction("push", 1) {
  protected override MixinExpressionTable Mutate(
    MixinExpressionTable table, MixinExpressionProperty property, IMixinExpressionContext context
  ) {
    return table.Put(
      table.Count.ToString(CultureInfo.InvariantCulture),
      MixinValue.From(property.Values[0], context)
    );
  }
}

internal sealed class PopFunction() : TableMutationFunction("pop", 0) {
  protected override MixinExpressionTable Mutate(
    MixinExpressionTable table, MixinExpressionProperty property, IMixinExpressionContext context
  ) {
    return table.Remove((table.Count - 1).ToString(CultureInfo.InvariantCulture));
  }
}

internal abstract class TableJoinFunction(string name, int arguments)
  : FunctionDefinition(name, arguments, arguments) {
  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    if (value is not MixinExpressionTable table) {
      error = ":" + Name + " requires a table";
      return false;
    }
    value = MixinValue.From(Join(table, invocation));
    error = null;
    return true;
  }

  protected abstract string Join(MixinExpressionTable table, FunctionInvocation invocation);
}

internal sealed class JoinKeysFunction() : TableJoinFunction("joinKeys", 1) {
  protected override string Join(MixinExpressionTable table, FunctionInvocation invocation) {
    return string.Join(invocation.Argument, table.Entries.Select(item => item.Key));
  }
}

internal sealed class JoinValuesFunction() : TableJoinFunction("joinValues", 1) {
  protected override string Join(MixinExpressionTable table, FunctionInvocation invocation) {
    return string.Join(invocation.Argument, table.Entries.Select(item => item.Value.Render()));
  }
}

internal sealed class JoinEntriesFunction() : TableJoinFunction("join", 2) {
  protected override string Join(MixinExpressionTable table, FunctionInvocation invocation) {
    return string.Join(
      invocation.Arguments[1],
      table.Entries.Select(item => item.Key + invocation.Arguments[0] + item.Value.Render())
    );
  }
}

internal enum TableTransformKind { MapValues, Map, Filter }

internal abstract class TableTransformFunction(string name) : FunctionDefinition(name, 1, 1) {
  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    if (value is not MixinExpressionTable table) {
      error = ":" + Name + " requires a table";
      return false;
    }
    value = CreateRequest(table, invocation.Argument);
    error = null;
    return true;
  }

  protected abstract MixinTransformRequest CreateRequest(MixinExpressionTable table, string function);
}

internal sealed class MapValuesFunction() : TableTransformFunction("mapValues") {
  protected override MixinTransformRequest CreateRequest(MixinExpressionTable table, string function) {
    return new MixinTransformRequest(table, TableTransformKind.MapValues, function);
  }
}

internal sealed class MapFunction() : TableTransformFunction("map") {
  protected override MixinTransformRequest CreateRequest(MixinExpressionTable table, string function) {
    return new MixinTransformRequest(table, TableTransformKind.Map, function);
  }
}

internal sealed class FilterFunction() : TableTransformFunction("filter") {
  protected override MixinTransformRequest CreateRequest(MixinExpressionTable table, string function) {
    return new MixinTransformRequest(table, TableTransformKind.Filter, function);
  }
}