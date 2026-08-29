using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HelixSourceGenerator.Language.Functions;

internal abstract class RegexFunction(string name) : FunctionDefinition(name, 2, 2) {
  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    try {
      var typed = MixinValue.From(value, context);
      if (!typed.TryGetText(out var text)) {
        error = "property ':" + Name + "' is not available for this value";
        return false;
      }
      value = MixinValue.From(Replace(text, invocation.Arguments[0], invocation.Arguments[1]));
      error = null;
      return true;
    } catch (ArgumentException exception) {
      error = "invalid regular expression: " + exception.Message;
      return false;
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

internal sealed class FloatTimeFunction : FunctionDefinition {
  private const float ZeroTolerance = 1e-6f;
  internal FloatTimeFunction() : base("floatTime", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref IMixinValue value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    var raw = typed.Unwrap().Value;
    var text = typed.TryGetText(out var comparable) ? comparable : typed.Render();
    if (!(TryConvertFloat(raw, out var seconds) ||
      TryParseFloatTime(raw is null ? null : Unwrap(text), out seconds))) {
      error = "cannot parse '" + text + "' as a float time";
      return false;
    }
    value = MixinValue.From(Format(seconds));
    error = null;
    return true;
  }

  private static string Unwrap(string value) {
    if (value.StartsWith("global::", StringComparison.Ordinal)) value = value.Substring(8);
    if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
      value = value.Substring(1, value.Length - 2);
    return value;
  }

  private static bool TryParseFloatTime(string value, out float seconds) {
    seconds = 0f;
    if (value is null) return true;
    var text = value.Trim();
    if (text.Length == 0 || string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "tick", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "ticks", StringComparison.OrdinalIgnoreCase)) return true;
    if (text[0] == '%') {
      if (!double.TryParse(
        text.Substring(1).Trim(), NumberStyles.Float,
        CultureInfo.InvariantCulture, out var frequency
      ) || frequency <= 0d) return false;
      return TryResult(1d / frequency, out seconds);
    }
    var match = Regex.Match(
      text,
      @"^(?<number>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)\s*(?<unit>ms|millis|s|second|seconds|t|tick|ticks|m|minute|minutes)?$",
      RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    );
    if (!match.Success || !double.TryParse(
      match.Groups["number"].Value, NumberStyles.Float,
      CultureInfo.InvariantCulture, out var number
    )) return false;
    switch (match.Groups["unit"].Value.ToLowerInvariant()) {
      case "t" or "tick" or "ticks" when number != Math.Truncate(number): return false;
      case "t" or "tick" or "ticks": number = -number; break;
      case "ms" or "millis": number /= 1000d; break;
      case "m" or "minute" or "minutes": number *= 60d; break;
    }
    return TryResult(number, out seconds);
  }

  private static bool TryConvertFloat(object value, out float result) {
    result = 0f;
    if (value is null) return false;
    switch (Type.GetTypeCode(value.GetType())) {
      case TypeCode.SByte:
      case TypeCode.Byte:
      case TypeCode.Int16:
      case TypeCode.UInt16:
      case TypeCode.Int32:
      case TypeCode.UInt32:
      case TypeCode.Int64:
      case TypeCode.UInt64:
      case TypeCode.Single:
      case TypeCode.Double:
      case TypeCode.Decimal:
        try {
          result = Convert.ToSingle(value, CultureInfo.InvariantCulture);
          return !float.IsNaN(result) && !float.IsInfinity(result);
        } catch (OverflowException) { return false; }
      default: return false;
    }
  }

  private static bool TryResult(double value, out float result) {
    result = (float)value;
    return !float.IsNaN(result) && !float.IsInfinity(result);
  }

  private static string Format(float seconds) {
    return Math.Abs(seconds) <= ZeroTolerance
      ? "-0f"
      : seconds.ToString("R", CultureInfo.InvariantCulture) + "f";
  }
}