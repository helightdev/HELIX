using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal sealed class CollectionFunctionDefinition(string name, MixinValueKind result, params MixinValueKind[] parameters)
  : FunctionDefinition(name, parameters.Length, parameters[0], result, parameters) {
  internal static MixinTableValue Put(ExecutionContext context, MixinTableValue table, string key, IMixinValue value) {
    var found = false;
    var entries = table.Entries.Select(entry => {
      if (entry.Key.Resolve(context.Strings) != key) return entry;
      found = true;
      return new KeyValuePair<MixinString, IMixinValue>(entry.Key, value);
    }).ToList();
    if (!found) entries.Add(new KeyValuePair<MixinString, IMixinValue>(context.ResolveString(key), value));
    return new MixinTableValue(entries.ToArray());
  }


  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] args, int line) {
    var name = Name;
    var value = args[0];

    var count = args.Length;
    var table = value as MixinTableValue;
    var tuple = value as TupleMixinValue;
    var values = tuple?.Values ?? table.Entries.Select(entry => entry.Value).ToArray();
    switch (name) {
      case "length" when count == 1: return new NumberMixinValue(values.Count);
      case "contains" when count == 2: return Bool(values.Any(entry => execution.Equal(entry, args[1])));
      case "indexOf" when count == 2 && tuple != null:
        for (var index = 0; index < values.Count; index++) if (execution.Equal(values[index], args[1])) return new NumberMixinValue(index);
        return new NumberMixinValue(-1);
      case "push" when count == 2 && tuple != null: return new TupleMixinValue(values.Concat([args[1]]).ToArray());
      case "pop" when count == 1 && tuple != null: return new TupleMixinValue(values.Take(Math.Max(0, values.Count - 1)).ToArray());
      case "keys" when count == 1 && table != null:
        return new TupleMixinValue(table.Entries.Select(entry => (IMixinValue)String(entry.Key.Resolve(execution.Context.Strings))).ToArray());
      case "values" when count == 1 && table != null: return new TupleMixinValue(values);
      case "entries" when count == 1 && table != null: return KindFunctionDefinition.Convert(execution, "tuple", table, false);
      case "remove" when count == 2 && table != null && args[1] is LiteralMixinValue:
        return new MixinTableValue(table.Entries.Where(entry => entry.Key.Resolve(execution.Context.Strings) != execution.Text(args[1])).ToArray());
      case "put" when count == 3 && table != null && args[1] is LiteralMixinValue: return CollectionFunctionDefinition.Put(execution.Context, table, execution.Text(args[1]), args[2]);
      case "put" when count == 2 && table != null && args[1] is MixinTableValue other:
        foreach (var entry in other.Entries) table = CollectionFunctionDefinition.Put(execution.Context, table, entry.Key.Resolve(execution.Context.Strings), entry.Value);
        return table;
      case "get" or "has" when count is 2 or 3: {
        bool present;
        IMixinValue selected;
        if (table != null && args[1] is LiteralMixinValue) {
          var key = execution.Text(args[1]);
          present = table.Entries.Any(entry => entry.Key.Resolve(execution.Context.Strings) == key);
          selected = table.Select(execution.Context, execution.Context.ResolveString(key));
        } else if (tuple != null && args[1] is NumberMixinValue index) {
          present = index.Value == Math.Truncate(index.Value) && index.Value >= 0 && index.Value < values.Count;
          selected = present ? values[(int)index.Value] : NullMixinValue.Instance;
        } else return execution.Context.Error("invalid collection key kind");
        return name == "has" ? Bool(present) : present ? selected : count == 3 ? args[2] : NullMixinValue.Instance;
      }
      case "join" when count == 2 && tuple != null && args[1] is LiteralMixinValue:
        return String(string.Join(execution.Text(args[1]), values.Select(execution.Text)));
      case "join" when count == 3 && table != null && args[1] is LiteralMixinValue && args[2] is LiteralMixinValue:
        return String(string.Join(execution.Text(args[2]), table.Entries.Select(entry => entry.Key.Resolve(execution.Context.Strings) + execution.Text(args[1]) + execution.Text(entry.Value))));
      case "map" or "where" or "any" or "all" or "reduce" when count == (name == "reduce" ? 3 : 2): {
        var results = new List<IMixinValue>();
        var entries = new List<KeyValuePair<MixinString, IMixinValue>>();
        var accumulator = count == 3 ? args[2] : NullMixinValue.Instance;
        for (var index = 0; index < values.Count; index++) {
          var item = values[index];
          IMixinValue input = item;
          if (table != null || name == "reduce") {
            var fields = new List<KeyValuePair<MixinString, IMixinValue>>();
            if (name == "reduce") fields.Add(new KeyValuePair<MixinString, IMixinValue>(execution.Context.ResolveString("acc"), accumulator));
            if (table != null) fields.Add(new KeyValuePair<MixinString, IMixinValue>(execution.Context.ResolveString("key"), String(table.Entries[index].Key.Resolve(execution.Context.Strings))));
            fields.Add(new KeyValuePair<MixinString, IMixinValue>(execution.Context.ResolveString("value"), item));
            input = new MixinTableValue(fields.ToArray());
          }
          var result = execution.Callback(args[1], [input], line);
          if (result is ErrorMixinValue) return result;
          if (name == "any" && result.IsTruthy(execution.Context)) return BooleanMixinValue.True;
          if (name == "all" && !result.IsTruthy(execution.Context)) return BooleanMixinValue.False;
          if (name == "reduce") { accumulator = result; continue; }
          if (name == "where" && !result.IsTruthy(execution.Context)) continue;
          var mapped = name == "map" ? result : item;
          results.Add(mapped);
          if (table != null) entries.Add(new KeyValuePair<MixinString, IMixinValue>(table.Entries[index].Key, mapped));
        }
        return name switch {
          "any" => BooleanMixinValue.False, "all" => BooleanMixinValue.True, "reduce" => accumulator,
          _ => table == null ? new TupleMixinValue(results.ToArray()) : new MixinTableValue(entries.ToArray())
        };
      }
      default: return null;
    }
  }
}
