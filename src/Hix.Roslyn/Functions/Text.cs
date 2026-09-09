using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Hix.Roslyn;
using Hix.Runtime;

namespace Hix.Functions;

public sealed class IdentifierFunction() : RoslynFunctionDefinition("identifier", 0,
  HixValueKind.String, HixValueKind.String) {
  protected override IHixValue Apply(
    HixThread context, IHixValue value, IReadOnlyList<IHixValue> arguments
  ) {
    return new LiteralHixValue(
      HixString.Dynamic(
        GeneratorAnalysis.EscapeIdentifier(
          value.Render(context).Resolve(context.Strings)
        )
      )
    );
  }
}

public sealed class FloatTimeFunction() : RoslynFunctionDefinition("floatTime", 0,
  HixValueKind.Any, HixValueKind.String) {
  protected override IHixValue Apply(
    HixThread context, IHixValue value,
    IReadOnlyList<IHixValue> arguments
  ) {
    var text = context.Unwrap(value).Render(context).Resolve(context.Strings);
    text = value is NullHixValue ? "" : (text ?? "").Trim();
    double seconds;
    if (text.Length == 0 || string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "tick", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "ticks", StringComparison.OrdinalIgnoreCase)) seconds = 0;
    else if (text.StartsWith("%", StringComparison.Ordinal) &&
      double.TryParse(text.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out var frequency) &&
      frequency > 0)
      seconds = 1d / frequency;
    else {
      var match = Regex.Match(
        text,
        @"^(?<n>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)\s*(?<u>ms|millis|s|seconds?|t|ticks?|m|minutes?)?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
      );
      if (!match.Success || !double.TryParse(
        match.Groups["n"].Value, NumberStyles.Float,
        CultureInfo.InvariantCulture, out seconds
      )) return context.Error("cannot parse '" + text + "' as a float time");
      var unit = match.Groups["u"].Value.ToLowerInvariant();
      if (unit is "t" or "tick" or "ticks" && seconds != Math.Truncate(seconds))
        return context.Error("cannot parse '" + text + "' as a float time");
      switch (unit) {
        case "t":
        case "tick":
        case "ticks": seconds = -seconds; break;
        case "ms":
        case "millis": seconds /= 1000d; break;
        case "m":
        case "minute":
        case "minutes": seconds *= 60d; break;
      }
    }
    return new LiteralHixValue(
      HixString.Dynamic(
        Math.Abs(seconds) <= 1e-6
          ? "-0f"
          : ((float)seconds).ToString("R", CultureInfo.InvariantCulture) + "f"
      )
    );
  }
}
