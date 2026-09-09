using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hix.Runtime;
using static Hix.Runtime.HixThread;

namespace Hix.Functions;

public abstract class KindDefinition(HixValueKind valueKind, params HixValueKind[] implicitSources) {
  public IReadOnlyCollection<HixValueKind> ImplicitSources { get; } = new HashSet<HixValueKind>(implicitSources);
  public KindHixValue Kind { get; } = KindHixValue.Get(valueKind);
  public HixValueKind ValueKind => valueKind;
  public abstract IHixValue New(HixThread execution);
  public abstract IHixValue Convert(HixThread execution, IHixValue value);
  public bool Is(IHixValue value) => value.Kind == valueKind;
}

public static class KindDefinitions {
  private static readonly KindDefinition[] All = [
    new NullKind(), new StringKind(), new BoolKind(), new NumberKind(), new TupleKind(), new TableKind(),
    new SymbolKind(), new FunctionKind(), new ErrorKind(), new KindKind()
  ];
  private static readonly IReadOnlyDictionary<HixValueKind, KindDefinition> ByKind =
    All.ToDictionary(definition => definition.ValueKind);
  public static IEnumerable<KindDefinition> Enumerate() => All;
  public static KindDefinition Get(KindHixValue kind) => ByKind[kind.ValueKind];
  public static KindDefinition Get(HixValueKind kind) => ByKind[kind];
  public static bool CanImplicitConvert(HixValueKind source, HixValueKind target) =>
    target == HixValueKind.Any || source == target || ByKind.TryGetValue(target, out var definition) &&
    definition.ImplicitSources.Contains(source);
  public static bool TryImplicitConvert(HixThread execution, IHixValue value,
    HixValueKind target, out IHixValue converted) {
    if (value.Kind == target || target == HixValueKind.Any) { converted = value; return true; }
    var definition = Get(target);
    if (!definition.ImplicitSources.Contains(value.Kind)) { converted = null; return false; }
    converted = definition.Convert(execution, value);
    return converted is not ErrorHixValue && converted.Kind == target;
  }

  private sealed class NullKind() : KindDefinition(HixValueKind.Null, HixValueKind.String) {
    public override IHixValue New(HixThread _) => NullHixValue.Instance;
    public override IHixValue Convert(HixThread _, IHixValue value) => NullHixValue.Instance;
  }
  private sealed class StringKind() : KindDefinition(HixValueKind.String,
    HixValueKind.Null, HixValueKind.Bool, HixValueKind.Number, HixValueKind.Symbol,
    HixValueKind.Function, HixValueKind.Error, HixValueKind.Kind) {
    public override IHixValue New(HixThread _) => String("");
    public override IHixValue Convert(HixThread execution, IHixValue value) => execution.RenderText(value);
  }
  private sealed class BoolKind() : KindDefinition(HixValueKind.Bool, HixValueKind.String) {
    public override IHixValue New(HixThread _) => BooleanHixValue.False;
    public override IHixValue Convert(HixThread execution, IHixValue value) => Bool(value.IsTruthy(execution));
  }
  private sealed class NumberKind() : KindDefinition(HixValueKind.Number,
    HixValueKind.String, HixValueKind.Bool, HixValueKind.Null) {
    public override IHixValue New(HixThread _) => new NumberHixValue(0);
    public override IHixValue Convert(HixThread execution, IHixValue value) {
      if (value is NullHixValue) return New(execution);
      if (value is NumberHixValue) return value;
      if (value is BooleanHixValue boolean) return new NumberHixValue(boolean.Value ? 1 : 0);
      var rendered = execution.RenderText(value);
      if (rendered is ErrorHixValue) return rendered;
      return double.TryParse(execution.ResolveText(rendered), NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
             !double.IsInfinity(number) && !double.IsNaN(number)
        ? new NumberHixValue(number) : execution.Error("invalid number");
    }
  }
  private sealed class TupleKind() : KindDefinition(HixValueKind.Tuple,
    HixValueKind.String, HixValueKind.Null, HixValueKind.Table) {
    public override IHixValue New(HixThread _) => TupleHixValue.Empty;
    public override IHixValue Convert(HixThread execution, IHixValue value) => value switch {
      NullHixValue => New(execution), TupleHixValue => value,
      HixTableValue table => new TupleHixValue(table.Entries.Select(entry => (IHixValue)new TupleHixValue(
        [String(entry.Key.Resolve(execution.Strings)), entry.Value])).ToArray()),
      _ => new TupleHixValue([value])
    };
  }
  private sealed class TableKind() : KindDefinition(HixValueKind.Table,
    HixValueKind.String, HixValueKind.Null, HixValueKind.Tuple) {
    public override IHixValue New(HixThread _) => HixTableValue.Empty;
    public override IHixValue Convert(HixThread execution, IHixValue value) {
      if (value is HixTableValue) return value;
      if (value is not TupleHixValue tuple) return New(execution);
      var result = HixTableValue.Empty;
      foreach (var entry in tuple.Values) {
        if (entry is not TupleHixValue {Values.Count: 2} pair || pair.Values[0] is not LiteralHixValue)
          return execution.Error("table conversion requires string-keyed pairs");
        result = CollectionFunctions.Put(execution, result, execution.ResolveText(pair.Values[0]), pair.Values[1]);
      }
      return result;
    }
  }
  private sealed class SymbolKind() : KindDefinition(HixValueKind.Symbol) {
    public override IHixValue New(HixThread execution) => execution.Error("not a symbol");
    public override IHixValue Convert(HixThread execution, IHixValue value) => Is(value) ? value : New(execution);
  }
  private sealed class FunctionKind() : KindDefinition(HixValueKind.Function, HixValueKind.String) {
    public override IHixValue New(HixThread _) => new NamedFunctionHixValue(HixString.Empty);
    public override IHixValue Convert(HixThread execution, IHixValue value) {
      if (value is NamedFunctionHixValue) return value;
      var rendered = execution.RenderText(value);
      return rendered is ErrorHixValue ? rendered : (IHixValue)execution.BindFunction(execution.Text(rendered)) ?? execution.Error("unknown function");
    }
  }
  private sealed class ErrorKind() : KindDefinition(HixValueKind.Error, HixValueKind.String) {
    public override IHixValue New(HixThread execution) => execution.Error("unspecified error");
    public override IHixValue Convert(HixThread execution, IHixValue value) {
      var rendered = execution.RenderText(value);
      return rendered is ErrorHixValue ? rendered : execution.Error(execution.Text(rendered));
    }
  }
  private sealed class KindKind() : KindDefinition(HixValueKind.Kind) {
    public override IHixValue New(HixThread _) => Kind;
    public override IHixValue Convert(HixThread _, IHixValue value) => KindHixValue.Get(value.Kind);
  }
}
