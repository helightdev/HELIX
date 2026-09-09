using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hix.Runtime;
using static Hix.Runtime.LanguageExecution;

namespace Hix.Functions;

internal abstract class KindDefinition(HixValueKind valueKind, params HixValueKind[] implicitSources) {
  internal IReadOnlyCollection<HixValueKind> ImplicitSources { get; } = new HashSet<HixValueKind>(implicitSources);
  internal KindHixValue Kind { get; } = KindHixValue.Get(valueKind);
  internal HixValueKind ValueKind => valueKind;
  internal abstract IHixValue New(LanguageExecution execution);
  internal abstract IHixValue Convert(LanguageExecution execution, IHixValue value);
  internal bool Is(IHixValue value) => value.Kind == valueKind;
}

internal static class KindDefinitions {
  private static readonly KindDefinition[] All = [
    new NullKind(), new StringKind(), new BoolKind(), new NumberKind(), new TupleKind(), new TableKind(),
    new SymbolKind(), new FunctionKind(), new ErrorKind(), new KindKind()
  ];
  private static readonly IReadOnlyDictionary<HixValueKind, KindDefinition> ByKind =
    All.ToDictionary(definition => definition.ValueKind);
  internal static IEnumerable<KindDefinition> Enumerate() => All;
  internal static KindDefinition Get(KindHixValue kind) => ByKind[kind.ValueKind];
  internal static KindDefinition Get(HixValueKind kind) => ByKind[kind];
  internal static bool CanImplicitConvert(HixValueKind source, HixValueKind target) =>
    target == HixValueKind.Any || source == target || ByKind.TryGetValue(target, out var definition) &&
    definition.ImplicitSources.Contains(source);
  internal static bool TryImplicitConvert(LanguageExecution execution, IHixValue value,
    HixValueKind target, out IHixValue converted) {
    if (value.Kind == target || target == HixValueKind.Any) { converted = value; return true; }
    var definition = Get(target);
    if (!definition.ImplicitSources.Contains(value.Kind)) { converted = null; return false; }
    converted = definition.Convert(execution, value);
    return converted is not ErrorHixValue && converted.Kind == target;
  }

  private sealed class NullKind() : KindDefinition(HixValueKind.Null, HixValueKind.String) {
    internal override IHixValue New(LanguageExecution _) => NullHixValue.Instance;
    internal override IHixValue Convert(LanguageExecution _, IHixValue value) => NullHixValue.Instance;
  }
  private sealed class StringKind() : KindDefinition(HixValueKind.String,
    HixValueKind.Null, HixValueKind.Bool, HixValueKind.Number, HixValueKind.Symbol,
    HixValueKind.Function, HixValueKind.Error, HixValueKind.Kind) {
    internal override IHixValue New(LanguageExecution _) => String("");
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) => execution.RenderText(value);
  }
  private sealed class BoolKind() : KindDefinition(HixValueKind.Bool, HixValueKind.String) {
    internal override IHixValue New(LanguageExecution _) => BooleanHixValue.False;
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) => Bool(value.IsTruthy(execution.Context));
  }
  private sealed class NumberKind() : KindDefinition(HixValueKind.Number,
    HixValueKind.String, HixValueKind.Bool, HixValueKind.Null) {
    internal override IHixValue New(LanguageExecution _) => new NumberHixValue(0);
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) {
      if (value is NullHixValue) return New(execution);
      if (value is NumberHixValue) return value;
      if (value is BooleanHixValue boolean) return new NumberHixValue(boolean.Value ? 1 : 0);
      var rendered = execution.RenderText(value);
      if (rendered is ErrorHixValue) return rendered;
      return double.TryParse(execution.Text(rendered), NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
             !double.IsInfinity(number) && !double.IsNaN(number)
        ? new NumberHixValue(number) : execution.Context.Error("invalid number");
    }
  }
  private sealed class TupleKind() : KindDefinition(HixValueKind.Tuple,
    HixValueKind.String, HixValueKind.Null, HixValueKind.Table) {
    internal override IHixValue New(LanguageExecution _) => TupleHixValue.Empty;
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) => value switch {
      NullHixValue => New(execution), TupleHixValue => value,
      HixTableValue table => new TupleHixValue(table.Entries.Select(entry => (IHixValue)new TupleHixValue(
        [String(entry.Key.Resolve(execution.Context.Strings)), entry.Value])).ToArray()),
      _ => new TupleHixValue([value])
    };
  }
  private sealed class TableKind() : KindDefinition(HixValueKind.Table,
    HixValueKind.String, HixValueKind.Null, HixValueKind.Tuple) {
    internal override IHixValue New(LanguageExecution _) => HixTableValue.Empty;
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) {
      if (value is HixTableValue) return value;
      if (value is not TupleHixValue tuple) return New(execution);
      var result = HixTableValue.Empty;
      foreach (var entry in tuple.Values) {
        if (entry is not TupleHixValue {Values.Count: 2} pair || pair.Values[0] is not LiteralHixValue)
          return execution.Context.Error("table conversion requires string-keyed pairs");
        result = CollectionFunctions.Put(execution.Context, result, execution.Text(pair.Values[0]), pair.Values[1]);
      }
      return result;
    }
  }
  private sealed class SymbolKind() : KindDefinition(HixValueKind.Symbol) {
    internal override IHixValue New(LanguageExecution execution) => execution.Context.Error("not a symbol");
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) => Is(value) ? value : New(execution);
  }
  private sealed class FunctionKind() : KindDefinition(HixValueKind.Function, HixValueKind.String) {
    internal override IHixValue New(LanguageExecution _) => new NamedFunctionHixValue("");
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) {
      if (value is NamedFunctionHixValue) return value;
      var rendered = execution.RenderText(value);
      return rendered is ErrorHixValue ? rendered : (IHixValue)execution.BindFunction(execution.Text(rendered)) ?? execution.Context.Error("unknown function");
    }
  }
  private sealed class ErrorKind() : KindDefinition(HixValueKind.Error, HixValueKind.String) {
    internal override IHixValue New(LanguageExecution execution) => execution.Context.Error("unspecified error");
    internal override IHixValue Convert(LanguageExecution execution, IHixValue value) {
      var rendered = execution.RenderText(value);
      return rendered is ErrorHixValue ? rendered : execution.Context.Error(execution.Text(rendered));
    }
  }
  private sealed class KindKind() : KindDefinition(HixValueKind.Kind) {
    internal override IHixValue New(LanguageExecution _) => Kind;
    internal override IHixValue Convert(LanguageExecution _, IHixValue value) => KindHixValue.Get(value.Kind);
  }
}
