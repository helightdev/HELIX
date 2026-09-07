using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal sealed class StringFunctionDefinition(string name, MixinValueKind result, params MixinValueKind[] parameters)
  : FunctionDefinition(name, parameters.Length, MixinValueKind.String, result, parameters) {
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] args, int line) {
    var name = Name;
    var value = execution.Text(args[0]);

    var count = args.Length;
    if (name == "length" && count == 1) return new NumberMixinValue(value.Length);
    if (name == "substring" && count == 3 && args[1] is NumberMixinValue start && args[2] is NumberMixinValue end) {
      if (start.Value != Math.Truncate(start.Value) || end.Value != Math.Truncate(end.Value) ||
        start.Value < 0 || end.Value < start.Value || end.Value > value.Length) return execution.Context.Error("invalid substring range");
      return String(value.Substring((int)start.Value, (int)(end.Value - start.Value)));
    }
    if (args.Any(argument => argument is not LiteralMixinValue)) return null;
    var other = count >= 2 ? execution.Text(args[1]) : "";
    switch (name) {
      case "contains" when count == 2: return Bool(value.Contains(other));
      case "startsWith" when count == 2: return Bool(value.StartsWith(other, StringComparison.Ordinal));
      case "endsWith" when count == 2: return Bool(value.EndsWith(other, StringComparison.Ordinal));
      case "uppercase" when count == 1: return String(value.ToUpperInvariant());
      case "lowercase" when count == 1: return String(value.ToLowerInvariant());
      case "trim" when count == 1: return String(value.Trim());
      case "trimStart" when count == 1: return String(value.TrimStart());
      case "trimEnd" when count == 1: return String(value.TrimEnd());
      case "split" when count == 2:
        return new TupleMixinValue(value.Split([other], StringSplitOptions.None).Select(text => (IMixinValue)String(text)).ToArray());
      case "replaceAll" when count == 3: return String(value.Replace(other, execution.Text(args[2])));
      case "replaceFirst" or "replaceLast" when count == 3: {
        if (other.Length == 0) return execution.Context.Error("replacement search string cannot be empty");
        var index = name == "replaceFirst" ? value.IndexOf(other, StringComparison.Ordinal) : value.LastIndexOf(other, StringComparison.Ordinal);
        return String(index < 0 ? value : value.Substring(0, index) + execution.Text(args[2]) + value.Substring(index + other.Length));
      }
      case "matches" when count == 2: return Bool(new Regex(other, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).IsMatch(value));
      case "regexReplaceAll" or "regexReplaceFirst" when count == 3: {
        var regex = new Regex(other, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        return String(name == "regexReplaceAll" ? regex.Replace(value, execution.Text(args[2])) : regex.Replace(value, execution.Text(args[2]), 1));
      }
      default: return null;
    }
  
  }
}
