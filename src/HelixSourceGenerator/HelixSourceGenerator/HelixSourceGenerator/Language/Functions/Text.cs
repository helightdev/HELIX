using System;
using System.Text.RegularExpressions;

namespace HelixSourceGenerator.Language.Functions;

internal abstract class RegexFunction(string name) : FunctionDefinition(name, 2, 2) {

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    try {
      var typed = MixinValue.From(value, context);
      if (!typed.TryGetText(out var text)) {
        error = "property ':" + Name + "' is not available for this value";
        return false;
      }
      value = Replace(text, invocation.Arguments[0], invocation.Arguments[1]);
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
  protected override string Replace(string text, string pattern, string replacement) =>
    Regex.Replace(text, pattern, replacement);
}

internal sealed class ReplaceFirstFunction() : RegexFunction("replaceFirst") {
  protected override string Replace(string text, string pattern, string replacement) =>
    new Regex(pattern).Replace(text, replacement, 1);
}

internal sealed class FloatTimeFunction : FunctionDefinition {
  internal FloatTimeFunction() : base("floatTime", 0, 0) { }

  internal override bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    var typed = MixinValue.From(value, context);
    var raw = typed.Unwrap();
    var text = typed.TryGetText(out var comparable) ? comparable : typed.Render();
    if (!(MixinExpressionEvaluator.TryConvertFloat(raw, out var seconds) ||
      MixinExpressionEvaluator.TryParseFloatTime(
        raw is null ? null : MixinExpressionEvaluator.UnwrapValue(text), out seconds
      ))) {
      error = "cannot parse '" + text + "' as a float time";
      return false;
    }
    value = MixinExpressionEvaluator.FormatFloatTime(seconds);
    error = null;
    return true;
  }
}
