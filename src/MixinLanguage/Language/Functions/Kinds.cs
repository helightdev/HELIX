using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal sealed class KindFunctionDefinition(string name, int count)
  : FunctionDefinition(name, count, MixinValueKind.Any, (MixinValueKind)Enum.Parse(typeof(MixinValueKind), name, true)) {
  public override bool AcceptsErrors => Name is "string" or "bool" or "kind";
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] args, int line) =>
    Convert(execution, Name, args.Length == 0 ? NullMixinValue.Instance : args[0], args.Length == 0);

  internal static IMixinValue Convert(LanguageExecution execution, string kind, IMixinValue value, bool empty) {

    switch (kind) {
      case "null": return NullMixinValue.Instance;
      case "string": return String(empty ? "" : execution.Text(value));
      case "bool": return Bool(!empty && value.IsTruthy(execution.Context));
      case "number":
        if (empty || value is NullMixinValue) return new NumberMixinValue(0);
        if (value is NumberMixinValue) return value;
        if (value is BooleanMixinValue boolean) return new NumberMixinValue(boolean.Value ? 1 : 0);
        return double.TryParse(execution.Text(value), NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
          !double.IsInfinity(number) && !double.IsNaN(number) ? new NumberMixinValue(number) : execution.Context.Error("invalid number");
      case "tuple": return value switch {
        NullMixinValue => TupleMixinValue.Empty, TupleMixinValue => value,
        MixinTableValue table => new TupleMixinValue(table.Entries.Select(entry => (IMixinValue)new TupleMixinValue(
          [String(entry.Key.Resolve(execution.Context.Strings)), entry.Value])).ToArray()),
        _ => new TupleMixinValue([value])
      };
      case "table":
        if (value is MixinTableValue) return value;
        if (value is not TupleMixinValue tuple) return MixinTableValue.Empty;
        var result = MixinTableValue.Empty;
        foreach (var entry in tuple.Values) {
          if (entry is not TupleMixinValue {Values.Count: 2} pair || pair.Values[0] is not LiteralMixinValue)
            return execution.Context.Error("table conversion requires string-keyed pairs");
          result = CollectionFunctionDefinition.Put(execution.Context, result, execution.Text(pair.Values[0]), pair.Values[1]);
        }
        return result;
      case "error": return execution.Context.Error(empty ? "unspecified error" : execution.Text(value));
      case "kind": return KindMixinValue.Of(value);
      case "function":
        if (empty) return new NamedFunctionMixinValue("");
        if (value is NamedFunctionMixinValue) return value;
        return (IMixinValue)execution.BindFunction(execution.Text(value)) ?? execution.Context.Error("unknown function");
      case "symbol": return KindMixinValue.Of(value).Name == "symbol" ? value : execution.Context.Error("not a symbol");
      default: return execution.Context.Error("unknown kind '" + kind + "'");
    }
  
  }
}
