using System;
using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;

namespace HelixSourceGenerator.Language.Functions;

internal sealed class AsTableFunction() : EvaluatedFunctionDefinition("table", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return value is MixinTableValue
      ? value
      : new MixinTableValue(
        value is NullMixinValue
          ? Array.Empty<KeyValuePair<MixinString, IMixinValue>>()
          : [new KeyValuePair<MixinString, IMixinValue>(context.Intern("0"), value)]
      );
  }
}

internal abstract class TableMutationFunction(string name, int arguments)
  : EvaluatedFunctionDefinition(name, arguments, arguments) {
  protected sealed override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return Mutate(
      context,
      value as MixinTableValue ?? MixinTableValue.Empty, arguments
    );
  }

  protected abstract MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  );
}

internal sealed class PutFunction() : TableMutationFunction("put", 2) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return table.Put(arguments[0].Render(context), arguments[1]);
  }
}

internal sealed class RemoveFunction() : TableMutationFunction("remove", 1) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return table.Remove(arguments[0].Render(context));
  }
}

internal sealed class PushFunction() : TableMutationFunction("push", 1) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return table.Push(context, arguments[0]);
  }
}

internal sealed class PopFunction() : TableMutationFunction("pop", 0) {
  protected override MixinTableValue Mutate(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return table.Pop();
  }
}

internal abstract class TableJoinFunction(string name, int arguments)
  : EvaluatedFunctionDefinition(name, arguments, arguments) {
  protected sealed override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return value is MixinTableValue table
      ? new LiteralMixinValue(ExecutionContext.Dynamic(Join(context, table, arguments)))
      : context.Error(":" + Name + " requires a table");
  }

  protected abstract string Join(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  );
}

internal sealed class JoinKeysFunction() : TableJoinFunction("joinKeys", 1) {
  protected override string Join(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return string.Join(
      arguments[0].Render(context).Resolve(context.Strings),
      table.Entries.Select(item => item.Key.Resolve(context.Strings))
    );
  }
}

internal sealed class JoinValuesFunction() : TableJoinFunction("joinValues", 1) {
  protected override string Join(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return string.Join(
      arguments[0].Render(context).Resolve(context.Strings),
      table.Entries.Select(item => item.Value.Render(context).Resolve(context.Strings))
    );
  }
}

internal sealed class JoinEntriesFunction() : TableJoinFunction("join", 2) {
  protected override string Join(
    ExecutionContext context, MixinTableValue table,
    IReadOnlyList<IMixinValue> arguments
  ) {
    return string.Join(
      arguments[1].Render(context).Resolve(context.Strings),
      table.Entries.Select(item => item.Key.Resolve(context.Strings) +
        arguments[0].Render(context).Resolve(context.Strings) +
        item.Value.Render(context).Resolve(context.Strings)
      )
    );
  }
}

internal abstract class TableTransformFunction(string name, TableTransformKind kind)
  : EvaluatedFunctionDefinition(name, 1, 1) {
  protected sealed override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    if (value is not MixinTableValue table) return context.Error(":" + Name + " requires a table");
    return arguments[0] is ProgramFunctionMixinValue callback
      ? new TableTransformMixinValue(table, kind, callback)
      : context.Error(":" + Name + " requires a resolved function");
  }
}

internal sealed class MapValuesFunction() : TableTransformFunction("mapValues", TableTransformKind.MapValues);

internal sealed class MapFunction() : TableTransformFunction("map", TableTransformKind.Map);

internal sealed class FilterFunction() : TableTransformFunction("filter", TableTransformKind.Filter);