using System;
using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Shared;

namespace HelixSourceGenerator.Language.Functions;

internal abstract class EvaluatedFunctionDefinition(string name, int minimumArguments, int maximumArguments,
  bool predicate = false
) : FunctionDefinition(name, minimumArguments, maximumArguments) {
  private readonly string _cacheKey = ":" + name;
  public override bool IsPredicate => predicate;

  public sealed override IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  ) {
    using var profile = MixinProfiler.MeasureFunction(Name);
    instance = context.Evaluate(instance);
    if (instance is ErrorMixinValue) return instance;
    var values = arguments.Count == 0 ? Array.Empty<IMixinValue>() : arguments.Select(context.Evaluate).ToArray();
    if (values.OfType<ErrorMixinValue>().FirstOrDefault() is { } error) return error;
    IMixinValue result;
    try {
      result = context is RoslynMixinContext roslyn && instance is RoslynMixinValue source
        ? roslyn.Derive(
          source, values.Length == 0
            ? _cacheKey
            : _cacheKey + "\u001f" +
            string.Join(
              "\u001f", values.Select(item => item.GetType().FullName + "=" +
                item.Render(context).Resolve(context.Strings)
              )
            ), () => Apply(context, instance, values)
        )
        : Apply(context, instance, values);
    } catch (ArgumentException exception) {
      return context.Error("invalid regular expression: " + exception.Message);
    }
    if (predicate) {
      var truth = result.IsTruthy(context);
      return truth != negated ? BooleanMixinValue.True : BooleanMixinValue.False;
    }
    return negated ? context.Error("value function ':" + Name + "' cannot be negated") : result;
  }

  protected abstract IMixinValue Apply(
    ExecutionContext context, IMixinValue value,
    IReadOnlyList<IMixinValue> arguments
  );
}

internal abstract class PredicateFunctionDefinition(string name, int minimumArguments, int maximumArguments)
  : EvaluatedFunctionDefinition(name, minimumArguments, maximumArguments, true) {
  protected static BooleanMixinValue Result(bool value) {
    return value ? BooleanMixinValue.True : BooleanMixinValue.False;
  }

  protected static string Comparable(IMixinValue value, ExecutionContext context) {
    var text = value.Render(context).Resolve(context.Strings) ?? "";
    if (text.StartsWith("global::", StringComparison.Ordinal)) text = text.Substring(8);
    return text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"'
      ? text.Substring(1, text.Length - 2)
      : text;
  }
}