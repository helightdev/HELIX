using System;
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
    var tuple = (TupleMixinValue)a[0];
    var values = tuple.Values;
    IMixinValue[] results = null;
    var resultCount = 0;
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
      if (kind is TransformKind.Any or TransformKind.All) continue;
      var keep = kind != TransformKind.Where || result.IsTruthy(e.Context);
      var mapped = kind == TransformKind.Map ? result : item;
      if (results == null && (!keep || !ReferenceEquals(mapped, item))) {
        results = new IMixinValue[values.Count];
        for (var previous = 0; previous < index; previous++) results[previous] = values[previous];
      }
      if (!keep) continue;
      if (results != null) results[resultCount] = mapped;
      resultCount++;
    }
    if (kind is TransformKind.Map or TransformKind.Where) {
      if (results == null) return tuple;
      if (resultCount == 0) return TupleMixinValue.Empty;
      if (resultCount != results.Length) Array.Resize(ref results, resultCount);
      return new TupleMixinValue(results);
    }
    return kind switch {
      TransformKind.Any => BooleanMixinValue.False,
      TransformKind.All => BooleanMixinValue.True,
      _ => accumulator
    };
  }
}
