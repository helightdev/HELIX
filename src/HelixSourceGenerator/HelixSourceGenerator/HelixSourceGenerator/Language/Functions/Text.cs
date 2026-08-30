using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HelixSourceGenerator.Shared;

namespace HelixSourceGenerator.Language.Functions;

internal abstract class RegexFunction(string name) : EvaluatedFunctionDefinition(name, 2, 2) {
  protected sealed override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    try {
      return new LiteralMixinValue(
        ExecutionContext.Dynamic(
          Replace(
            value.Render(context).Resolve(context.Strings), arguments[0].Render(context).Resolve(context.Strings),
            arguments[1].Render(context).Resolve(context.Strings)
          )
        )
      );
    } catch (ArgumentException exception) {
      return context.Error("invalid regular expression: " + exception.Message);
    }
  }

  protected abstract string Replace(string text, string pattern, string replacement);
}

internal sealed class ReplaceFunction() : RegexFunction("replace") {
  protected override string Replace(string text, string pattern, string replacement) {
    return Regex.Replace(text, pattern, replacement);
  }
}

internal sealed class ReplaceFirstFunction() : RegexFunction("replaceFirst") {
  protected override string Replace(string text, string pattern, string replacement) {
    return new Regex(pattern).Replace(text, replacement, 1);
  }
}

internal sealed class FormatFunction() : EvaluatedFunctionDefinition("format", 1, 2) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> arguments
  ) {
    try {
      return new LiteralMixinValue(ExecutionContext.Dynamic(string.Format(
        CultureInfo.InvariantCulture,
        value.Render(context).Resolve(context.Strings),
        arguments.Select(item => (object)item.Render(context).Resolve(context.Strings)).ToArray()
      )));
    } catch (FormatException exception) {
      return context.Error("invalid format string: " + exception.Message);
    }
  }
}

internal sealed class IdentifierFunction() : EvaluatedFunctionDefinition("identifier", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value, IReadOnlyList<IMixinValue> arguments
  ) {
    return new LiteralMixinValue(ExecutionContext.Dynamic(GeneratorAnalysis.EscapeIdentifier(
      value.Render(context).Resolve(context.Strings)
    )));
  }
}

internal sealed class FloatTimeFunction() : EvaluatedFunctionDefinition("floatTime", 0, 0) {
  protected override IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  ) {
    var text = context.Unwrap(value).Render(context).Resolve(context.Strings);
    text = value is NullMixinValue ? "" : (text ?? "").Trim();
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
    return new LiteralMixinValue(
      ExecutionContext.Dynamic(
        Math.Abs(seconds) <= 1e-6
          ? "-0f"
          : ((float)seconds).ToString("R", CultureInfo.InvariantCulture) + "f"
      )
    );
  }
}
