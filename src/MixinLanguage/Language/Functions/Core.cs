using System;
using System.Linq;
using Mixins.Runtime;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal sealed class CoreFunctionDefinition(string name, MixinValueKind result, bool effects, bool variadic,
  params MixinValueKind[] parameters)
  : FunctionDefinition(name, parameters.Length - (variadic ? 1 : 0), parameters.FirstOrDefault(), result,
    parameters, variadic: variadic) {
  public override bool HasEffects => effects;
  public override bool AcceptsErrors => Name is "catch" or "exists" or "not" or "is";
  internal override IMixinValue Execute(LanguageExecution execution, IMixinValue[] arguments, int line) {
    var name = Name;
    var count = arguments.Length;
    var value = count == 0 ? NullMixinValue.Instance : arguments[0];
      switch (name) {
        case "derive" when count == 1:
          return execution.IsPrelude ? execution.Derive(value, line) : execution.Context.Error("derive requires the prelude pass");
        case "resolveMixin" when count == 2 && value is LiteralMixinValue:
          return execution.IsPrelude ? execution.Context.ResolveMixin(value.Render(execution.Context), arguments[1])
            : execution.Context.Error("resolveMixin requires the prelude pass");
        case "defineTarget" when count == 2 && value is LiteralMixinValue && arguments[1] is LiteralMixinValue:
          return execution.IsPrelude ? execution.Context.DefineTarget(execution.Text(value), execution.Text(arguments[1])) : execution.Context.Error("defineTarget requires the prelude pass");
        case "config" when count == 2 && value is LiteralMixinValue:
          return execution.IsPrelude ? execution.Context.Configure(execution.Text(value), arguments[1]) : execution.Context.Error("config requires the prelude pass");
        case "kind" when count == 1: return KindMixinValue.Of(value);
        case "new" when count is 1 or 2 && value is KindMixinValue kind:
          return KindFunctionDefinition.Convert(execution, kind.Name, count == 2 ? arguments[1] : NullMixinValue.Instance, count == 1);
        case "as" when count == 2 && arguments[1] is KindMixinValue kind:
          return KindFunctionDefinition.Convert(execution, kind.Name, value, false);
        case "is" when count == 2 && arguments[1] is KindMixinValue kind: return Bool(MatchesKind(value, kind.Name));
        case "if" when count == 3 && arguments[1] is KindMixinValue kind:
          return MatchesKind(value, kind.Name) ? value : arguments[2];
        case "ifNot" when count == 3 && arguments[1] is KindMixinValue kind:
          return !MatchesKind(value, kind.Name) ? value : arguments[2];
        case "exists" when count == 1: return Bool(value is not (NullMixinValue or ErrorMixinValue));
        case "not" when count == 1: return Bool(!value.IsTruthy(execution.Context));
        case "eq" when count == 2: return Bool(execution.Equal(value, arguments[1]));
        case "neq" when count == 2: return Bool(!execution.Equal(value, arguments[1]));
        case "and" when count >= 2: return Bool(arguments.All(argument => argument.IsTruthy(execution.Context)));
        case "or" when count >= 2: return Bool(arguments.Any(argument => argument.IsTruthy(execution.Context)));
        case "catch" when count is 1 or 2:
          return value is not ErrorMixinValue ? value : count == 1 ? NullMixinValue.Instance : execution.Callback(arguments[1], [value], line);
        case "call" or "inline" when count is 1 or 2:
          return execution.Callback(value, count == 2 ? [arguments[1]] : [], line);
        case "assert":
          return arguments.All(argument => argument.IsTruthy(execution.Context)) ? NullMixinValue.Instance : execution.Context.Error("assertion failed");
        case "match":
          if (!arguments.All(argument => argument.IsTruthy(execution.Context)))
            throw new Flow(Compiler.ControlFlowKind.Break, null, NullMixinValue.Instance, line);
          return NullMixinValue.Instance;
        case "fail" when count <= 1: return execution.Context.Error(count == 1 ? execution.Text(value) : "execution failed");
        case "emit" when count is 1 or 2: {
          var target = count == 1 ? "TARGET" : execution.Text(value);
          if (Enum.TryParse<MixinExpressionOutputTarget>(target, true, out var outputTarget))
            execution.Outputs.Add(new MixinExpressionOutput(outputTarget, execution.Text(arguments[count - 1])));
          else execution.Outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Mixin,
            execution.Text(arguments[count - 1]), execution.Context.ResolveInjectionTarget(target)));
          return NullMixinValue.Instance;
        }
        case "using" when count == 1:
          execution.Outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Using, execution.Text(value)));
          return NullMixinValue.Instance;
        case "inject" when count is 2 or 3:
          if (count == 3 && arguments[1] is not NumberMixinValue) return execution.Context.Error("injection priority must be a number");
          execution.Outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Mixin, execution.Text(arguments[count - 1]),
            execution.Context.ResolveInjectionTarget(execution.Text(value)), count == 3 ? checked((int)((NumberMixinValue)arguments[1]).Value) : 0));
          return NullMixinValue.Instance;
        case "log" when count == 1:
          execution.Logs.Add(new MixinExpressionLog(execution.Text(value), line));
          return NullMixinValue.Instance;
        case "dump" when count == 1:
          execution.Logs.Add(new MixinExpressionLog(KindMixinValue.Of(value).Name + ": " + execution.Text(value), line));
          return value;
      }

    return execution.Context.Error("invalid arguments for '" + name + "'");
  }
}
