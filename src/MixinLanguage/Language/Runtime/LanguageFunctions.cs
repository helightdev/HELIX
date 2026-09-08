using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mixins.Runtime;

internal sealed partial class LanguageExecution {
  private IMixinValue Call(string name, IMixinValue[] arguments, int line, IReadOnlyList<FunctionDefinition> bound = null) {
    if (scope.Contains(name)) return Invoke(name, arguments, line);
    var definitions = bound ?? FunctionLibrary.Resolve(name, arguments.Length);
    FunctionDefinition definition = null;
    IMixinValue[] convertedArguments = null;
    var conversions = int.MaxValue;
    foreach (var candidate in definitions) {
      if (!candidate.TryConvertValues(this, arguments, out var converted, out var count) || count >= conversions) continue;
      definition = candidate;
      convertedArguments = converted;
      conversions = count;
    }
    if (definition == null) return context.Error("no matching function '" + name + "' for " + arguments.Length + " arguments");
    if (pure && definition.HasEffects) return context.Error("pure functions cannot perform '" + name + "'");
    arguments = convertedArguments;
    if (!definition.AcceptsErrors && arguments.OfType<ErrorMixinValue>().FirstOrDefault() is {} failure)
      return failure with {IsChecked = false};
    try { return definition.Execute(this, arguments, line) ?? context.Error("invalid arguments for '" + name + "'"); }
    catch (ArgumentException exception) { return context.Error(exception.Message); }
    catch (OverflowException exception) { return context.Error(exception.Message); }
    catch (RegexMatchTimeoutException) { return context.Error("regular expression timed out"); }
  }

  internal IMixinValue Callback(IMixinValue function, IMixinValue[] arguments, int line) => function switch {
    NamedFunctionMixinValue {Name: ""} => NullMixinValue.Instance,
    NamedFunctionMixinValue named => Invoke(named.Name, arguments, line, named.Scope),
    _ => context.Error("expected a function value")
  };


}
