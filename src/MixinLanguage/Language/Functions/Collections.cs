using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal static class CollectionFunctions {
  internal static MixinTableValue Put(ExecutionContext context, MixinTableValue table, string key, IMixinValue value) {
    var found = false;
    var entries = table.Entries.Select(entry => {
        if (entry.Key.Resolve(context.Strings) != key) return entry;
        found = true;
        return new KeyValuePair<MixinString, IMixinValue>(entry.Key, value);
      }
    ).ToList();
    if (!found) entries.Add(new KeyValuePair<MixinString, IMixinValue>(context.ResolveString(key), value));
    return new MixinTableValue(entries.ToArray());
  }

  private static IReadOnlyList<IMixinValue> Values(IMixinValue value) => value is TupleMixinValue tuple
    ? tuple.Values
    : ((MixinTableValue)value).Entries.Select(entry => entry.Value).ToArray();

  internal static IMixinValue Has(LanguageExecution e, IMixinValue[] a) => Bool(TrySelect(e, a[0], a[1], out _));

  internal static IMixinValue Get(LanguageExecution e, IMixinValue[] a) => TrySelect(e, a[0], a[1], out var value)
    ?
    value
    : a.Length == 3
      ? a[2]
      : NullMixinValue.Instance;

  private static bool TrySelect(LanguageExecution e, IMixinValue collection, IMixinValue key, out IMixinValue value) {
    if (collection is MixinTableValue table) {
      var name = e.Text(key);
      var found = table.Entries.Any(entry => entry.Key.Resolve(e.Context.Strings) == name);
      value = found ? table.Select(e.Context, e.Context.ResolveString(name)) : NullMixinValue.Instance;
      return found;
    }
    var index = ((NumberMixinValue)key).Value;
    var values = Values(collection);
    var present = index == Math.Truncate(index) && index >= 0 && index < values.Count;
    value = present ? values[(int)index] : NullMixinValue.Instance;
    return present;
  }

  internal enum TransformKind { Map, Where, Any, All, Reduce }

  internal static IMixinValue Transform(LanguageExecution e, IMixinValue[] a, int line, TransformKind kind) {
    var table = a[0] as MixinTableValue;
    var values = Values(a[0]);
    var results = new List<IMixinValue>();
    var entries = new List<KeyValuePair<MixinString, IMixinValue>>();
    var accumulator = a.Length == 3 ? a[2] : NullMixinValue.Instance;
    for (var index = 0; index < values.Count; index++) {
      var item = values[index];
      IMixinValue input = item;
      if (table != null || kind == TransformKind.Reduce) {
        var fields = new List<KeyValuePair<MixinString, IMixinValue>>();
        if (kind == TransformKind.Reduce) fields.Add(new(e.Context.ResolveString("acc"), accumulator));
        if (table != null)
          fields.Add(new(e.Context.ResolveString("key"), String(table.Entries[index].Key.Resolve(e.Context.Strings))));
        fields.Add(new(e.Context.ResolveString("value"), item));
        input = new MixinTableValue(fields.ToArray());
      }
      var result = e.Callback(a[1], [input], line);
      if (result is ErrorMixinValue) return result;
      if (kind == TransformKind.Any && result.IsTruthy(e.Context)) return BooleanMixinValue.True;
      if (kind == TransformKind.All && !result.IsTruthy(e.Context)) return BooleanMixinValue.False;
      if (kind == TransformKind.Reduce) {
        accumulator = result;
        continue;
      }
      if (kind == TransformKind.Where && !result.IsTruthy(e.Context)) continue;
      var mapped = kind == TransformKind.Map ? result : item;
      results.Add(mapped);
      if (table != null) entries.Add(new(table.Entries[index].Key, mapped));
    }
    return kind switch {
      TransformKind.Any => BooleanMixinValue.False,
      TransformKind.All => BooleanMixinValue.True,
      TransformKind.Reduce => accumulator,
      _ => table == null ? new TupleMixinValue(results.ToArray()) : new MixinTableValue(entries.ToArray())
    };
  }
}