using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal sealed class NumberFunctionDefinition(string name, MixinValueKind result, params MixinValueKind[] parameters)
  : FunctionDefinition(name, parameters.Length, MixinValueKind.Number, result, parameters) {
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] args, int line) {
    var name = Name;
    var value = ((NumberMixinValue)args[0]).Value;

    var count = args.Length;
    if (name == "isInt" && count == 1) return Bool(!double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value));
    if (name == "compare" && count == 3 && args[1] is LiteralMixinValue && args[2] is NumberMixinValue compared) {
      var equal = Math.Abs(value - compared.Value) <= 1e-9 * Math.Max(1, Math.Max(Math.Abs(value), Math.Abs(compared.Value)));
      return execution.Text(args[1]) switch {
        "lt" => Bool(value < compared.Value && !equal), "gt" => Bool(value > compared.Value && !equal),
        "lte" => Bool(value <= compared.Value || equal), "gte" => Bool(value >= compared.Value || equal),
        "eq" => Bool(equal), "neq" => Bool(!equal), _ => execution.Context.Error("unknown comparison")
      };
    }
    if (args.Any(argument => argument is not NumberMixinValue)) return null;
    double? result = (name, count) switch {
      ("round", 1) => Math.Round(value), ("floor", 1) => Math.Floor(value), ("ceil", 1) => Math.Ceiling(value),
      ("abs", 1) => Math.Abs(value), ("sqrt", 1) => Math.Sqrt(value),
      ("min", 2) => Math.Min(value, ((NumberMixinValue)args[1]).Value),
      ("max", 2) => Math.Max(value, ((NumberMixinValue)args[1]).Value),
      ("pow", 2) => Math.Pow(value, ((NumberMixinValue)args[1]).Value),
      ("log", 2) => Math.Log(value, ((NumberMixinValue)args[1]).Value),
      ("minus", 2) => value - ((NumberMixinValue)args[1]).Value,
      ("plus", 2) => value + ((NumberMixinValue)args[1]).Value,
      ("mult", 2) => value * ((NumberMixinValue)args[1]).Value,
      ("div", 2) => value / ((NumberMixinValue)args[1]).Value,
      ("mod", 2) => value % ((NumberMixinValue)args[1]).Value,
      ("clamp", 3) => Math.Max(((NumberMixinValue)args[1]).Value, Math.Min(value, ((NumberMixinValue)args[2]).Value)),
      _ => null
    };
    return result == null ? null : double.IsNaN(result.Value) || double.IsInfinity(result.Value)
      ? execution.Context.Error("invalid numeric operation") : new NumberMixinValue(result.Value);
  
  }
}
