using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal abstract class KindDefinition(MixinValueKind valueKind) {
  internal KindMixinValue Kind { get; } = KindMixinValue.Get(valueKind);
  internal MixinValueKind ValueKind => valueKind;
  internal abstract IMixinValue New(LanguageExecution execution);
  internal abstract IMixinValue Convert(LanguageExecution execution, IMixinValue value);
  internal bool Is(IMixinValue value) => KindMixinValue.Of(value).ValueKind == valueKind;
}

internal static class KindDefinitions {
  private static readonly KindDefinition[] All = [
    new NullKind(), new StringKind(), new BoolKind(), new NumberKind(), new TupleKind(), new TableKind(),
    new SymbolKind(), new FunctionKind(), new ErrorKind(), new KindKind()
  ];
  private static readonly IReadOnlyDictionary<MixinValueKind, KindDefinition> ByKind =
    All.ToDictionary(definition => definition.ValueKind);
  internal static IEnumerable<KindDefinition> Enumerate() => All;
  internal static KindDefinition Get(KindMixinValue kind) => ByKind[kind.ValueKind];
  internal static KindDefinition Get(MixinValueKind kind) => ByKind[kind];

  private sealed class NullKind() : KindDefinition(MixinValueKind.Null) {
    internal override IMixinValue New(LanguageExecution _) => NullMixinValue.Instance;
    internal override IMixinValue Convert(LanguageExecution _, IMixinValue value) => NullMixinValue.Instance;
  }
  private sealed class StringKind() : KindDefinition(MixinValueKind.String) {
    internal override IMixinValue New(LanguageExecution _) => String("");
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) => String(execution.Text(value));
  }
  private sealed class BoolKind() : KindDefinition(MixinValueKind.Bool) {
    internal override IMixinValue New(LanguageExecution _) => BooleanMixinValue.False;
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) => Bool(value.IsTruthy(execution.Context));
  }
  private sealed class NumberKind() : KindDefinition(MixinValueKind.Number) {
    internal override IMixinValue New(LanguageExecution _) => new NumberMixinValue(0);
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) {
      if (value is NullMixinValue) return New(execution);
      if (value is NumberMixinValue) return value;
      if (value is BooleanMixinValue boolean) return new NumberMixinValue(boolean.Value ? 1 : 0);
      return double.TryParse(execution.Text(value), NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
             !double.IsInfinity(number) && !double.IsNaN(number)
        ? new NumberMixinValue(number) : execution.Context.Error("invalid number");
    }
  }
  private sealed class TupleKind() : KindDefinition(MixinValueKind.Tuple) {
    internal override IMixinValue New(LanguageExecution _) => TupleMixinValue.Empty;
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) => value switch {
      NullMixinValue => New(execution), TupleMixinValue => value,
      MixinTableValue table => new TupleMixinValue(table.Entries.Select(entry => (IMixinValue)new TupleMixinValue(
        [String(entry.Key.Resolve(execution.Context.Strings)), entry.Value])).ToArray()),
      _ => new TupleMixinValue([value])
    };
  }
  private sealed class TableKind() : KindDefinition(MixinValueKind.Table) {
    internal override IMixinValue New(LanguageExecution _) => MixinTableValue.Empty;
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) {
      if (value is MixinTableValue) return value;
      if (value is not TupleMixinValue tuple) return New(execution);
      var result = MixinTableValue.Empty;
      foreach (var entry in tuple.Values) {
        if (entry is not TupleMixinValue {Values.Count: 2} pair || pair.Values[0] is not LiteralMixinValue)
          return execution.Context.Error("table conversion requires string-keyed pairs");
        result = CollectionFunctions.Put(execution.Context, result, execution.Text(pair.Values[0]), pair.Values[1]);
      }
      return result;
    }
  }
  private sealed class SymbolKind() : KindDefinition(MixinValueKind.Symbol) {
    internal override IMixinValue New(LanguageExecution execution) => execution.Context.Error("not a symbol");
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) => Is(value) ? value : New(execution);
  }
  private sealed class FunctionKind() : KindDefinition(MixinValueKind.Function) {
    internal override IMixinValue New(LanguageExecution _) => new NamedFunctionMixinValue("");
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) => value is NamedFunctionMixinValue
      ? value : (IMixinValue)execution.BindFunction(execution.Text(value)) ?? execution.Context.Error("unknown function");
  }
  private sealed class ErrorKind() : KindDefinition(MixinValueKind.Error) {
    internal override IMixinValue New(LanguageExecution execution) => execution.Context.Error("unspecified error");
    internal override IMixinValue Convert(LanguageExecution execution, IMixinValue value) => execution.Context.Error(execution.Text(value));
  }
  private sealed class KindKind() : KindDefinition(MixinValueKind.Kind) {
    internal override IMixinValue New(LanguageExecution _) => Kind;
    internal override IMixinValue Convert(LanguageExecution _, IMixinValue value) => KindMixinValue.Of(value);
  }
}
