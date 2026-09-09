using System;
using Hix.Runtime;
using static Hix.Runtime.LanguageExecution;

namespace Hix.Functions;

internal static class CollectionFunctions {
  internal static HixTableValue Put(HixExecutionContext context, HixTableValue table, string key, IHixValue value) =>
    table.Put(context, HixExecutionContext.Dynamic(key), value);

  internal static IHixValue Has(LanguageExecution e, IHixValue[] a) => Bool(TrySelect(e, a[0], a[1], out _));

  internal static IHixValue Get(LanguageExecution e, IHixValue[] a) => TrySelect(e, a[0], a[1], out var value)
    ?
    value
    : a.Length == 3
      ? a[2]
      : NullHixValue.Instance;

  private static bool TrySelect(LanguageExecution e, IHixValue collection, IHixValue key, out IHixValue value) {
    if (collection is HixTableValue table) {
      return table.TryGetValue(e.Context, e.Context.ResolveString(e.Text(key)), out value);
    }
    var index = ((NumberHixValue)key).Value;
    var values = ((TupleHixValue)collection).Values;
    var present = index == Math.Truncate(index) && index >= 0 && index < values.Count;
    value = present ? values[(int)index] : NullHixValue.Instance;
    return present;
  }

  internal enum TransformKind { Map, Where, Any, All, Reduce }

  internal static IHixValue Transform(LanguageExecution e, IHixValue[] a, int line, TransformKind kind) {
    var tuple = (TupleHixValue)a[0];
    var values = tuple.Values;
    IHixValue[] results = null;
    var resultCount = 0;
    var accumulator = a.Length == 3 ? a[2] : NullHixValue.Instance;
    for (var index = 0; index < values.Count; index++) {
      var item = values[index];
      var result = e.Callback(a[1], kind == TransformKind.Reduce ? [accumulator, item] : [item], line);
      if (result is ErrorHixValue) return result;
      if (kind == TransformKind.Any && result.IsTruthy(e.Context)) return BooleanHixValue.True;
      if (kind == TransformKind.All && !result.IsTruthy(e.Context)) return BooleanHixValue.False;
      if (kind == TransformKind.Reduce) {
        accumulator = result;
        continue;
      }
      if (kind is TransformKind.Any or TransformKind.All) continue;
      var keep = kind != TransformKind.Where || result.IsTruthy(e.Context);
      var mapped = kind == TransformKind.Map ? result : item;
      if (results == null && (!keep || !ReferenceEquals(mapped, item))) {
        results = new IHixValue[values.Count];
        for (var previous = 0; previous < index; previous++) results[previous] = values[previous];
      }
      if (!keep) continue;
      if (results != null) results[resultCount] = mapped;
      resultCount++;
    }
    if (kind is TransformKind.Map or TransformKind.Where) {
      if (results == null) return tuple;
      if (resultCount == 0) return TupleHixValue.Empty;
      if (resultCount != results.Length) Array.Resize(ref results, resultCount);
      return new TupleHixValue(results);
    }
    return kind switch {
      TransformKind.Any => BooleanHixValue.False,
      TransformKind.All => BooleanHixValue.True,
      _ => accumulator
    };
  }
}
