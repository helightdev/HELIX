using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal static class CollectionFunctions {
  internal static MixinTableValue Put(ExecutionContext context, MixinTableValue table, string key, IMixinValue value) =>
    table.Put(context, ExecutionContext.Dynamic(key), value);

  internal static IMixinValue Has(LanguageExecution e, IMixinValue[] a) => Bool(TrySelect(e, a[0], a[1], out _));

  internal static IMixinValue Get(LanguageExecution e, IMixinValue[] a) => TrySelect(e, a[0], a[1], out var value)
    ?
    value
    : a.Length == 3
      ? a[2]
      : NullMixinValue.Instance;

  private static bool TrySelect(LanguageExecution e, IMixinValue collection, IMixinValue key, out IMixinValue value) {
    if (collection is MixinTableValue table) {
      return table.TryGetValue(e.Context, e.Context.ResolveString(e.Text(key)), out value);
    }
    var index = ((NumberMixinValue)key).Value;
    var values = ((TupleMixinValue)collection).Values;
    var present = index == Math.Truncate(index) && index >= 0 && index < values.Count;
    value = present ? values[(int)index] : NullMixinValue.Instance;
    return present;
  }

  internal enum TransformKind { Map, Where, Any, All, Reduce }

  internal static IMixinValue Transform(LanguageExecution e, IMixinValue[] a, int line, TransformKind kind) {
    var values = ((TupleMixinValue)a[0]).Values;
    var results = new List<IMixinValue>();
    var accumulator = a.Length == 3 ? a[2] : NullMixinValue.Instance;
    for (var index = 0; index < values.Count; index++) {
      var item = values[index];
      var result = e.Callback(a[1], kind == TransformKind.Reduce ? [accumulator, item] : [item], line);
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
    }
    return kind switch {
      TransformKind.Any => BooleanMixinValue.False,
      TransformKind.All => BooleanMixinValue.True,
      TransformKind.Reduce => accumulator,
      _ => new TupleMixinValue(results.ToArray())
    };
  }
}
