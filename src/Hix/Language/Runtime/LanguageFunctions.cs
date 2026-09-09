using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hix.Runtime;

internal sealed partial class LanguageExecution {
  private IHixValue Call(string name, IHixValue[] arguments, int line, IReadOnlyList<FunctionDefinition> bound = null) {
    if (scope.Contains(name)) return Invoke(name, arguments, line);
    var definitions = bound ?? context.Backend.Functions.Resolve(name, arguments.Length);
    FunctionDefinition definition = null;
    IHixValue[] convertedArguments = null;
    var conversions = int.MaxValue;
    foreach (var candidate in definitions) {
      if (!candidate.TryConvertValues(this, arguments, out var converted, out var count) || count >= conversions) continue;
      definition = candidate;
      convertedArguments = converted;
      conversions = count;
      if (count == 0) break;
    }
    if (definition == null) return context.Error("no matching function '" + name + "' for " + arguments.Length + " arguments");
    if (pure && definition.HasEffects) return context.Error("pure functions cannot perform '" + name + "'");
    if (!prelude && definition.RequiresPrelude) return context.Error("function '" + name + "' requires the prelude pass");
    arguments = convertedArguments;
    if (!definition.AcceptsErrors && arguments.OfType<ErrorHixValue>().FirstOrDefault() is {} failure)
      return failure with {IsChecked = false};
    try { return definition.Execute(context, arguments, line) ?? context.Error("invalid arguments for '" + name + "'"); }
    catch (ArgumentException exception) { return context.Error(exception.Message); }
    catch (OverflowException exception) { return context.Error(exception.Message); }
    catch (RegexMatchTimeoutException) { return context.Error("regular expression timed out"); }
  }

  internal IHixValue Callback(IHixValue function, IHixValue[] arguments, int line) => function switch {
    NamedFunctionHixValue {Name: ""} => NullHixValue.Instance,
    NamedFunctionHixValue named => Invoke(named.Name, arguments, line, named.Scope),
    _ => context.Error("expected a function value")
  };


}
