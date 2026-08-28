using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HELIX.SourceGen.Expressions;

using static MixinExpressionInterpreter;
using static MixinExpressionCompiler;

internal static class MixinExpressionEvaluator {
  internal static bool TryResolveDirectiveArgument(
    string argument,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out string value,
    out string error
  ) {
    if (!TryResolveArgumentValue(
      argument, context, locals, variables, out var resolved, out error
    )) {
      value = null;
      return false;
    }
    value = RenderValue(resolved);
    return true;
  }

  private static bool TryResolveArgumentValue(
    string argument,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out object value,
    out string error
  ) {
    if (argument is not null && argument.Length >= 2 &&
      argument[0] == '(' && argument[argument.Length - 1] == ')') {
      var expression = argument.Substring(1, argument.Length - 2);
      if (!ValidateValueExpressionSyntax(expression, out error)) {
        value = null;
        return false;
      }
      return TryEvaluateExpression(
        MixinExpressionParser.ParseValueExpression(expression), context, locals, variables, out value, out error
      );
    }
    value = argument;
    error = null;
    return true;
  }

  internal static void TryOutputTarget(
    string argument,
    out MixinExpressionOutputTarget target,
    out string injectionTarget
  ) {
    injectionTarget = null;
    switch ((argument ?? "TARGET").ToUpperInvariant()) {
      case "TARGET":
        target = MixinExpressionOutputTarget.Target;
        return;
      case "CLASS":
        target = MixinExpressionOutputTarget.Class;
        return;
      case "FILE":
        target = MixinExpressionOutputTarget.File;
        return;
      case "IMPLEMENTS":
        target = MixinExpressionOutputTarget.Implements;
        return;
      case "ANNOTATION":
        target = MixinExpressionOutputTarget.Annotation;
        return;
      default:
        target = MixinExpressionOutputTarget.Injection;
        injectionTarget = argument;
        return;
    }
  }

  internal static bool TryEvaluateAll(
    IReadOnlyList<MixinExpressionReference> expression,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out bool result,
    out string error,
    out string failure
  ) {
    result = true;
    error = null;
    failure = null;
    var found = false;
    foreach (var reference in expression) {
      found = true;
      if (!TryEvaluate(reference, context, locals, variables, out var value, out error)) return false;
      result &= value;
      if (!value && failure is null) {
        failure = DescribeFailedCondition(
          SelectFailedCondition(reference, context, locals, variables)
        );
      }
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  private static MixinExpressionReference SelectFailedCondition(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables
  ) {
    var valueProperties = reference.Properties.Where(property => !IsBooleanProperty(property)).ToArray();
    foreach (var predicate in reference.Properties.Where(IsBooleanProperty)) {
      var properties = valueProperties.Concat(new[] { predicate }).ToArray();
      var candidate = new MixinExpressionReference(reference.Root, reference.Member, properties);
      if (TryEvaluate(candidate, context, locals, variables, out var value, out _) && !value) return candidate;
    }
    return reference;
  }

  private static bool IsBooleanProperty(MixinExpressionProperty property) {
    return FunctionLibrary.IsPredicate(property.Name);
  }

  private static string DescribeFailedCondition(MixinExpressionReference reference) {
    var subject = reference.Root switch {
      "var" => "Variable " + (reference.Member ?? "<unnamed>"),
      "local" => "Local variable " + (reference.Member ?? "<unnamed>"),
      "arg" => "Argument " + (reference.Member ?? "<unspecified>"),
      "this" => "Current type" + MemberSuffix(reference.Member),
      "target" => "Target" + MemberSuffix(reference.Member),
      "attr" => "Attribute" + MemberSuffix(reference.Member),
      _ => "Value @" + reference.Root + MemberSuffix(reference.Member)
    };
    var predicate = reference.Properties.FirstOrDefault(IsBooleanProperty);
    if (predicate is null) return subject + " is null, false or invalid";
    var expected = predicate.Argument ?? "";
    switch (predicate.Name) {
      case "eq":
        return subject + (predicate.Negated ? " is " : " is not ") +
          (expected.Length == 0 ? "the expected value" : expected);
      case "exists": return subject + (predicate.Negated ? " exists" : " does not exist");
      case "is": return subject + (predicate.Negated ? " is of type " : " is not of type ") + expected;
      case "has": return subject + (predicate.Negated ? " has member " : " does not have member ") + expected;
      case "isSelf": return subject + (predicate.Negated ? " is the current type" : " is not the current type");
      default:
        return subject + (predicate.Negated ? " is " : " is not ") + PredicateDescription(predicate.Name);
    }
  }

  private static string MemberSuffix(string member) {
    return string.IsNullOrEmpty(member) ? "" : " member " + member;
  }

  private static string PredicateDescription(string name) {
    return name switch {
      "ref" => "a ref parameter",
      "in" => "an in parameter",
      "out" => "an out parameter",
      "inout" => "an in or out parameter",
      "argument" => "a normal argument",
      "static" => "static",
      "public" => "public",
      "exposed" => "public or internal",
      "top" => "a top-level type",
      "concrete" => "concrete",
      "partial" => "partial",
      "generic" => "generic",
      "struct" => "a struct",
      "class" => "a class",
      _ => name
    };
  }

  internal static bool TryInterpolate(
    IReadOnlyList<ValueExpressionPart> expression,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out string result,
    out string error
  ) {
    error = null;
    var builder = new StringBuilder();
    foreach (var part in expression) {
      if (part.Reference is null) {
        builder.Append(part.Literal);
        continue;
      }
      if (!TryResolve(part.Reference, context, locals, variables, out var value, out error)) {
        result = null;
        return false;
      }
      builder.Append(RenderValue(value));
    }
    result = builder.ToString();
    return true;
  }

  internal static bool TryEvaluateExpression(
    IReadOnlyList<ValueExpressionPart> expression,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out object result,
    out string error
  ) {
    if (expression is { Count: 1 } && expression[0].Reference is not null)
      return TryResolve(expression[0].Reference, context, locals, variables, out result, out error);
    if (TryInterpolate(expression, context, locals, variables, out var text, out error)) {
      result = text;
      return true;
    }
    result = null;
    return false;
  }

  internal static void RestoreCallParameter(
    IDictionary<string, object> locals,
    CallFrame frame
  ) {
    if (frame.HadParameter) locals[ParameterLocalKey] = frame.Parameter;
    else locals.Remove(ParameterLocalKey);
  }

  private static bool TryResolve(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out object value,
    out string error
  ) {
    if (!TryPrepareReference(reference, context, locals, variables, out reference, out error) ||
      !TryReduceLogicalProperties(reference, context, locals, variables, out reference, out error)) {
      value = null;
      return false;
    }
    var tableOperationIndex = reference.Properties.ToList()
      .FindIndex(item => item.Name is "put" or "remove" or "push" or "pop");
    if (tableOperationIndex >= 0) {
      var prefix = new MixinExpressionReference(
        reference.Root, reference.Member,
        reference.Properties.Take(tableOperationIndex).ToArray()
      );
      if (!TryResolveCore(prefix, context, locals, variables, out value, out error)) return false;
      var operations = new MixinExpressionReference(
        "table", null, reference.Properties.Skip(tableOperationIndex).ToArray()
      );
      if (!TryApplyStringProperties(operations, context, null, ref value, out error)) return false;
      var predicate = operations.Properties.FirstOrDefault(IsBooleanProperty);
      if (predicate is null) return true;
      var matched = (predicate.Name == "exists" && value is not null) ||
        (predicate.Name == "eq" && RelaxedEquals(value, predicate.Values[0])) ||
        (predicate.Name == "matches" && RegexMatches(RenderValue(value), predicate.Argument, out error)) ||
        (predicate.Name == "has" && TableContainsValue(value, predicate.Values[0]));
      if (error is not null) return false;
      value = predicate.Negated ? !matched : matched;
      return true;
    }
    if (reference.Properties.Any(IsBooleanProperty)) {
      if (!TryEvaluateCore(reference, context, locals, variables, out var boolean, out error)) {
        value = null;
        return false;
      }
      value = boolean;
      return true;
    }
    return TryResolveCore(reference, context, locals, variables, out value, out error);
  }

  private static bool TryResolveCore(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out object value,
    out string error
  ) {
    if (reference.Root is "true" or "false" or "null" or "table" or "param") {
      value = reference.Root switch {
        "true" => true,
        "false" => false,
        "null" => null,
        "table" => new MixinExpressionTable(),
        "param" => locals.TryGetValue(ParameterLocalKey, out var parameter) ? parameter : null,
        _ => null
      };
      if (!string.IsNullOrEmpty(reference.Member)) {
        value = value is MixinExpressionTable table && table.TryGetValue(reference.Member, out var selected)
          ? selected
          : null;
      }
      return TryApplyStringProperties(reference, context, null, ref value, out error);
    }
    if (TryStored(reference, context, locals, variables, out value, out error)) return error is null;
    if (context.TryResolve(reference, out var resolved, out error)) {
      value = resolved;
      return true;
    }
    value = null;
    return false;
  }

  private static bool TryEvaluate(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out bool value,
    out string error
  ) {
    if (!TryPrepareReference(reference, context, locals, variables, out reference, out error) ||
      !TryReduceLogicalProperties(reference, context, locals, variables, out reference, out error)) {
      value = false;
      return false;
    }
    var tableOperationIndex = reference.Properties.ToList()
      .FindIndex(item => item.Name is "put" or "remove" or "push" or "pop"
      );
    if (tableOperationIndex >= 0) {
      var prefix = new MixinExpressionReference(
        reference.Root, reference.Member,
        reference.Properties.Take(tableOperationIndex).ToArray()
      );
      if (!TryResolveCore(prefix, context, locals, variables, out var tableValue, out error)) {
        value = false;
        return false;
      }
      var operations = new MixinExpressionReference(
        "table", null, reference.Properties.Skip(tableOperationIndex).ToArray()
      );
      if (!TryApplyStringProperties(operations, context, null, ref tableValue, out error)) {
        value = false;
        return false;
      }
      var predicate = operations.Properties.FirstOrDefault(IsBooleanProperty);
      if (predicate is null) {
        value = IsTruthyValue(tableValue);
        return true;
      }
      var matched = (predicate.Name == "exists" && tableValue is not null) ||
        (predicate.Name == "eq" && RelaxedEquals(tableValue, predicate.Values[0])) ||
        (predicate.Name == "matches" && RegexMatches(
          RenderValue(tableValue), predicate.Argument, out error
        )) || (predicate.Name == "has" && TableContainsValue(tableValue, predicate.Values[0]));
      if (error is not null) {
        value = false;
        return false;
      }
      value = predicate.Negated ? !matched : matched;
      return true;
    }
    var callablePredicateIndex = reference.Properties.ToList().FindIndex(item => item.Name is "signature" or "wireable"
    );
    if (callablePredicateIndex >= 0) {
      if (context is not IMixinExpressionSignatureContext signatureContext) {
        value = false;
        error = "the expression context does not support method signatures";
        return false;
      }
      var predicate = reference.Properties[callablePredicateIndex];
      value = false;
      if (predicate.Name == "wireable") {
        if (!signatureContext.TryWireable(
          predicate.Arguments[0], predicate.Arguments[1], out value, out error
        )) return false;
      } else {
        var prefix = new MixinExpressionReference(
          reference.Root, reference.Member,
          reference.Properties.Take(callablePredicateIndex).ToArray()
        );
        if (!TryResolveCore(prefix, context, locals, variables, out var first, out error) ||
          !signatureContext.TryHaveSameSignature(
            RenderValue(first), predicate.Argument, out value, out error
          )) return false;
      }
      if (predicate.Negated) value = !value;
      return true;
    }
    var propStructPredicateIndex = reference.Properties.ToList().FindIndex(item =>
      item.Name is "structHasEquality" or "structNoArgs" or "structAugment"
    );
    if (propStructPredicateIndex >= 0) {
      if (context is not IMixinExpressionPropStructContext propStructContext) {
        value = false;
        error = "the expression context does not support prop structs";
        return false;
      }
      var predicate = reference.Properties[propStructPredicateIndex];
      var prefix = new MixinExpressionReference(
        reference.Root,
        reference.Member,
        reference.Properties.Take(propStructPredicateIndex).ToArray()
      );
      if (!TryResolveCore(prefix, context, locals, variables, out var handle, out error) ||
        !propStructContext.TryApplyPropStructProperty(
          handle, predicate, out var predicateValue, out error
        )) {
        value = false;
        return false;
      }
      if (predicateValue is not bool boolean) {
        value = false;
        error = ":" + predicate.Name + " did not produce a boolean value";
        return false;
      }
      value = predicate.Negated ? !boolean : boolean;
      return true;
    }
    return TryEvaluateCore(reference, context, locals, variables, out value, out error);
  }

  private static bool TryEvaluateCore(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out bool value,
    out string error
  ) {
    switch (reference.Root) {
      case "true" or "false" or "null" or "table" or "param": {
        if (!TryResolveCore(reference, context, locals, variables, out var atom, out error)) {
          value = false;
          return false;
        }
        var predicates = reference.Properties.Where(IsBooleanProperty).ToArray();
        if (predicates.Length == 0) {
          value = IsTruthyValue(atom);
          return true;
        }
        value = true;
        foreach (var predicate in predicates) {
          var item = (predicate.Name == "exists" && atom is not null) ||
            (predicate.Name == "eq" && RelaxedEquals(atom, predicate.Values[0])) ||
            (predicate.Name == "matches" && RegexMatches(RenderValue(atom), predicate.Argument, out error)) ||
            (predicate.Name == "has" && TableContainsValue(atom, predicate.Values[0]));
          if (error is not null) return false;
          value &= predicate.Negated ? !item : item;
        }
        return true;
      }
      case "local":
      case "var": {
        TryStored(reference, context, locals, variables, out var stored, out error);
        var predicates = reference.Properties.Where(IsBooleanProperty).ToArray();
        if (error is not null) {
          if (predicates.Length == 0) {
            value = false;
            return false;
          }
          error = null;
          value = true;
          foreach (var predicate in predicates) {
            var item = predicate.Name == "eq" && RelaxedEquals(null, predicate.Argument);
            value &= predicate.Negated ? !item : item;
          }
          return true;
        }
        if (predicates.Length == 0) {
          value = IsTruthyValue(stored);
          return true;
        }
        value = true;
        foreach (var predicate in predicates) {
          bool item;
          if (predicate.Name is "structHasEquality" or "structNoArgs" or "structAugment") {
            if (context is not IMixinExpressionPropStructContext propStructContext ||
              !propStructContext.TryApplyPropStructProperty(
                stored, predicate, out var predicateValue, out error
              )) return false;
            item = predicateValue is true;
          } else {
            item = (predicate.Name == "exists" && stored is not null) ||
              (predicate.Name == "eq" && RelaxedEquals(stored, predicate.Values[0])) ||
              (predicate.Name == "matches" && RegexMatches(RenderValue(stored), predicate.Argument, out error)) ||
              (predicate.Name == "has" && TableContainsValue(stored, predicate.Values[0]));
          }
          if (error is not null) return false;
          value &= predicate.Negated ? !item : item;
        }
        return true;
      }
      default: return context.TryEvaluate(reference, out value, out error);
    }
  }

  private static bool TryPrepareReference(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out MixinExpressionReference prepared,
    out string error
  ) {
    var properties = new List<MixinExpressionProperty>(reference.Properties.Count);
    foreach (var property in reference.Properties) {
      var arguments = new List<string>(property.Arguments.Count);
      var values = new List<object>(property.Arguments.Count);
      foreach (var argument in property.Arguments) {
        if (property.Name is "and" or "or" && argument.Length >= 2 &&
          argument[0] == '(' && argument[argument.Length - 1] == ')') {
          var booleanExpression = argument.Substring(1, argument.Length - 2);
          arguments.Add(booleanExpression);
          values.Add(booleanExpression);
          continue;
        }
        if (!TryResolveArgumentValue(
          argument, context, locals, variables, out var resolvedValue, out error
        )) {
          prepared = null;
          return false;
        }
        arguments.Add(RenderValue(resolvedValue));
        values.Add(resolvedValue);
      }
      properties.Add(
        new MixinExpressionProperty(
          property.Name, arguments.AsReadOnly(), values.AsReadOnly(), property.Negated
        )
      );
    }
    prepared = new MixinExpressionReference(reference.Root, reference.Member, properties.AsReadOnly());
    error = null;
    return true;
  }

  private static bool TryReduceLogicalProperties(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out MixinExpressionReference reduced,
    out string error
  ) {
    var properties = reference.Properties.ToList();
    var root = reference.Root;
    var member = reference.Member;
    while (true) {
      var index = properties.FindIndex(item => item.Name is "and" or "or");
      if (index < 0) break;
      var operation = properties[index];
      if (operation.Arguments.Count == 0) {
        reduced = null;
        error = ":" + operation.Name + " requires at least one boolean expression";
        return false;
      }
      var prefix = new MixinExpressionReference(root, member, properties.Take(index).ToArray());
      if (!TryEvaluateCore(prefix, context, locals, variables, out var result, out error)) {
        reduced = null;
        return false;
      }
      foreach (var argument in operation.Arguments) {
        var parsed = MixinExpressionParser.ParseBooleanExpression(argument);
        if (!ValidateBooleanExpressionSyntax(argument, out error) ||
          !TryEvaluateAll(parsed, context, locals, variables, out var item, out error, out _)) {
          reduced = null;
          return false;
        }
        result = operation.Name == "and" ? result && item : result || item;
      }
      root = result ? "true" : "false";
      member = null;
      properties = properties.Skip(index + 1).ToList();
    }
    reduced = new MixinExpressionReference(root, member, properties.AsReadOnly());
    error = null;
    return true;
  }

  private static bool RegexMatches(string value, string pattern, out string error) {
    try {
      error = null;
      return Regex.IsMatch(value ?? "", pattern ?? "");
    } catch (ArgumentException exception) {
      error = "invalid regular expression: " + exception.Message;
      return false;
    }
  }

  private static bool RelaxedEquals(object actual, object expected) {
    var expectedIsNull = expected is null ||
      string.Equals(RenderValue(expected), "null", StringComparison.OrdinalIgnoreCase);
    if (actual is null) return expectedIsNull;
    if (Equals(actual, expected)) return true;
    return string.Equals(
      UnwrapComparable(RenderValue(actual)), UnwrapComparable(RenderValue(expected)),
      StringComparison.OrdinalIgnoreCase
    );
  }

  private static string UnwrapComparable(string value) {
    if (value.StartsWith("global::", StringComparison.Ordinal)) value = value.Substring(8);
    if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
      value = value.Substring(1, value.Length - 2);
    return value;
  }

  private static bool TryStored(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, object> locals,
    IReadOnlyDictionary<string, object> variables,
    out object value,
    out string error
  ) {
    value = null;
    error = null;
    if (reference.Root != "local" && reference.Root != "var") return false;
    if (string.IsNullOrEmpty(reference.Member)) {
      error = "@" + reference.Root + " requires a member name";
      return true;
    }
    var values = reference.Root == "local" ? locals : variables;
    if (!values.TryGetValue(reference.Member, out value)) {
      error = "unknown @" + reference.Root + " value '" + reference.Member + "'";
      return true;
    }
    TryApplyStringProperties(reference, context, reference.Member, ref value, out error);
    return true;
  }

  private static bool TryApplyStringProperties(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    string name,
    ref object value,
    out string error
  ) {
    error = null;
    foreach (var property in reference.Properties) {
      if (IsBooleanProperty(property) || property.Name is "and" or "or") continue;
      if (!FunctionLibrary.TryInvoke(property, context, reference.Root, name, ref value, out error)) return false;
    }
    return true;
  }

  internal static string Render(object value) {
    return RenderValue(value);
  }

  internal static string UnwrapValue(string value) {
    return UnwrapComparable(value);
  }

  internal static bool IsTruthy(object value) {
    return IsTruthyValue(value);
  }

  internal static bool TryParseFloatTime(string value, out float seconds) {
    seconds = 0f;
    if (value is null) return true;
    var text = value.Trim();
    if (text.Length == 0 || string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "tick", StringComparison.OrdinalIgnoreCase) ||
      string.Equals(text, "ticks", StringComparison.OrdinalIgnoreCase)) return true;

    if (text[0] == '%') {
      if (!double.TryParse(
        text.Substring(1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var frequency
      ) || frequency <= 0d) return false;
      return TryFloatTimeResult(1d / frequency, out seconds);
    }

    var match = Regex.Match(
      text,
      @"^(?<number>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)\s*(?<unit>ms|millis|s|second|seconds|t|tick|ticks|m|minute|minutes)?$",
      RegexOptions.IgnoreCase |
      RegexOptions.CultureInvariant
    );
    if (!match.Success || !double.TryParse(
      match.Groups["number"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number
    )) return false;

    var unit = match.Groups["unit"].Value.ToLowerInvariant();
    switch (unit) {
      case "t" or "tick" or "ticks" when number != Math.Truncate(number): return false;
      case "t" or "tick" or "ticks": number = -number; break;
      case "ms" or "millis": number /= 1000d; break;
      case "m" or "minute" or "minutes": number *= 60d; break;
    }
    return TryFloatTimeResult(number, out seconds);
  }

  internal static bool TryConvertFloat(object value, out float result) {
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
        } catch (OverflowException) {
          return false;
        }
      default:
        return false;
    }
  }

  private static bool TryFloatTimeResult(double value, out float result) {
    result = (float)value;
    return !float.IsNaN(result) && !float.IsInfinity(result);
  }

  internal static string FormatFloatTime(float seconds) {
    return Math.Abs(seconds) <= FloatTimeZeroTolerance
      ? "-0f"
      : seconds.ToString("R", CultureInfo.InvariantCulture) + "f";
  }

  private static bool IsTruthyValue(object value) {
    if (value is IMixinValue typed) return typed.IsTruthy;
    if (value is null) return false;
    if (value is bool boolean) return boolean;
    if (value is string text) {
      return text.Length != 0 &&
        !string.Equals(text, "false", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(text, "null", StringComparison.OrdinalIgnoreCase);
    }
    return true;
  }

  private static bool TableContainsValue(object value, object expected) {
    if (value is not MixinExpressionTable table) return false;
    return table.Values.Any(item => RelaxedEquals(item, expected));
  }

  internal static string RenderValue(object value) {
    return value switch {
      IMixinValue typed => typed.Render(),
      null => "null",
      bool boolean => boolean ? "true" : "false",
      string text => text,
      _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
    };
  }

  internal static bool TryReadReferenceNode(
    string text,
    ref int position,
    out MixinExpressionReference reference,
    out string error
  ) {
    reference = null;
    error = null;
    if (position >= text.Length || text[position] != '@') {
      error = "expected '@' expression reference";
      return false;
    }
    position++;
    var parenthesized = position < text.Length && text[position] == '(';
    if (parenthesized) position++;
    var rootStart = position;
    while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
    if (position == rootStart) {
      error = "expression reference has no root";
      return false;
    }
    var root = text.Substring(rootStart, position - rootStart);
    string member = null;
    if (position < text.Length && text[position] == '#') {
      position++;
      var memberStart = position;
      while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
      if (position == memberStart) {
        error = "member reference is empty";
        return false;
      }
      member = text.Substring(memberStart, position - memberStart);
    }

    var properties = new List<MixinExpressionProperty>();
    while (position < text.Length && (text[position] == ':' || text[position] == '#')) {
      if (text[position] == '#') {
        position++;
        var pathStart = position;
        while (position < text.Length &&
          (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
        if (position == pathStart) {
          error = "type argument path is empty";
          return false;
        }
        properties.Add(
          new MixinExpressionProperty(
            "path", text.Substring(pathStart, position - pathStart)
          )
        );
        continue;
      }
      position++;
      var negated = false;
      if (position < text.Length && text[position] == '!') {
        negated = true;
        position++;
      }
      if (position < text.Length && text[position] == '?') position++;
      var propertyStart = position;
      while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
      if (position == propertyStart) {
        error = "property name is empty";
        return false;
      }
      var name = text.Substring(propertyStart, position - propertyStart);
      var arguments = new List<string>();
      while (position < text.Length && text[position] == '<') {
        position++;
        var argumentStart = position;
        var depth = 1;
        while (position < text.Length && depth != 0) {
          if (text[position] == '<') depth++;
          else if (text[position] == '>') depth--;
          if (depth != 0) position++;
        }
        if (depth != 0) {
          error = "unterminated property argument";
          return false;
        }
        arguments.Add(text.Substring(argumentStart, position - argumentStart));
        position++;
      }
      properties.Add(new MixinExpressionProperty(name, arguments.AsReadOnly(), negated));
    }
    for (var index = 0; index < properties.Count; index++) {
      var property = properties[index];
      if (!ValidatePropertyArguments(property, out error)) return false;
      if (IsBooleanProperty(property) && index != properties.Count - 1) {
        error = "boolean operation ':" + property.Name + "' must be terminal";
        return false;
      }
    }
    if (parenthesized) {
      if (position >= text.Length || text[position] != ')') {
        error = "unterminated parenthesized reference";
        return false;
      }
      position++;
    }
    reference = new MixinExpressionReference(root, member, properties);
    return true;
  }

  private static bool ValidatePropertyArguments(
    MixinExpressionProperty property,
    out string error
  ) {
    if (FunctionLibrary.TryGet(property.Name, out var function))
      return function.Validate(property, out error);
    error = null;
    return true;
  }
}
