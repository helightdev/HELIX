using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Shared;

namespace HelixSourceGenerator.Language;

public static class MixinExpressionVirtualMachine {
  internal const string CarryLocalPrefix = "\0@carry:";

  public static MixinExpressionResult Execute(
    string expression, ExecutionContext context,
    IDictionary<string, object> variables = null
  ) {
    return Execute(
      expression, context, variables, (MixinExpressionPreparedState)null
    );
  }

  public static MixinExpressionResult Execute(
    string expression, ExecutionContext context,
    IDictionary<string, object> variables, MixinExpressionPreparedState prepared
  ) {
    var syntax = expression is null ? null : MixinExpressionParser.Parse(expression);
    return ExecuteCompiled(syntax, context, variables, prepared);
  }

  public static MixinExpressionResult Execute(
    string expression, ExecutionContext context,
    IDictionary<string, object> variables, IEnumerable<string> preparedExpressions
  ) {
    return Execute(expression, context, variables, MixinExpressionCompiler.PrepareGlobals(preparedExpressions));
  }

  internal static MixinExpressionResult ExecuteCompiled(
    MixinProgramSyntax syntax, ExecutionContext context,
    IDictionary<string, object> variables, MixinExpressionPreparedState prepared
  ) {
    if (!MixinExpressionCompiler.TryCompileExecution(syntax, prepared, out var program, out var error, out var line))
      return Failure(error, line);
    return Execute(program, context, variables);
  }

  internal static MixinExpressionResult Execute(
    MixinExpressionExecutionProgram program, ExecutionContext context,
    IDictionary<string, object> variables, bool importCarries = true
  ) {
    using var profile = MixinProfiler.Measure("vm.execute.total");
    var started = Stopwatch.GetTimestamp();
    var setupProfile = MixinProfiler.Measure("vm.execute.setup");
    context.Strings = program.StringPool;
    var previousInvoker = context.ProgramInvoker;
    context.ProgramInvoker = (function, parameter) =>
      RunProgramFunction(program, context, function.Entry, parameter);
    context.Locals.Clear();
    context.Variables.Clear();
    context.Carries.Clear();
    context.Parameter = NullMixinValue.Instance;
    context.Variables.StoreIsolatedRange(program.Variables);
    if (variables is not null) {
      foreach (var item in variables) {
        // Carries belong to one expression. Importing prior carries makes every subsequent prelude
        // materialize and detach an ever-growing semantic snapshot set.
        if (item.Key.StartsWith(CarryLocalPrefix, StringComparison.Ordinal)) {
          if (importCarries) {
            context.Carries.Store(
              context, context.ResolveString(item.Key.Substring(CarryLocalPrefix.Length)),
              FromObject(context, item.Value)
            );
          }
          continue;
        }
        context.Variables.Store(context, context.ResolveString(item.Key), FromObject(context, item.Value));
      }
    }
    var outputs = new List<MixinExpressionOutput>();
    var logs = new List<MixinExpressionLog>();
    var calls = new Stack<(int Return, MixinString Local, IMixinValue Parameter)>();
    var pc = 0;
    var operations = 0;
    var steps = 0;
    var limit = Math.Max(1024, program.Instructions.Count * 64);
    setupProfile.Dispose();

    MixinExpressionResult Error(IMixinValue value, MixinInstruction instruction) {
      return Failure(value.Render(context).Resolve(context.Strings), instruction.Location.Line, logs);
    }

    bool Evaluate(MixinInstruction instruction, out IMixinValue value) {
      value = EvaluateRuntime(program, context, instruction.Operand ?? NullMixinValue.Instance);
      return value is not ErrorMixinValue;
    }

    try {
      var executionProfile = MixinProfiler.Measure("vm.execute.instructions");
      while (pc < program.Instructions.Count) {
        if (++steps > limit) return Failure("execution limit exceeded", program.Instructions[pc].Location.Line, logs);
        var instruction = program.Instructions[pc++];
        if (instruction.Opcode == MixinOpcode.Empty) continue;
        operations++;
        switch (instruction.Opcode) {
          case MixinOpcode.Scope: break;
          case MixinOpcode.Function: pc = instruction.Destination; break;
          case MixinOpcode.End:
            if (instruction.SecondaryDestination != 0 && calls.Count != 0)
              CompleteReturn(NullMixinValue.Instance);
            break;
          case MixinOpcode.Match:
            if (!Evaluate(instruction, out var matched)) return Error(matched, instruction);
            if (!matched.IsTruthy(context)) {
              var destination = instruction.Destination >= 0
                ? instruction.Destination
                : instruction.SecondaryDestination;
              if (destination < 0) return Failure("MATCH has no following scope", instruction.Location.Line, logs);
              pc = destination;
            }
            break;
          case MixinOpcode.Assert:
            if (!Evaluate(instruction, out var asserted)) return Error(asserted, instruction);
            if (!asserted.IsTruthy(context)) {
              return Failure(
                instruction.Message.Resolve(context.Strings) ?? "assertion failed",
                instruction.Location.Line, logs
              );
            }
            break;
          case MixinOpcode.Emit:
          case MixinOpcode.Using:
            if (!Evaluate(instruction, out var emitted)) return Error(emitted, instruction);
            outputs.Add(
              new MixinExpressionOutput(
                instruction.OutputTarget == default && instruction.Opcode == MixinOpcode.Using
                  ? MixinExpressionOutputTarget.Using
                  : instruction.OutputTarget,
                [emitted.Render(context)], context.Strings, instruction.Name
              )
            );
            break;
          case MixinOpcode.Mixin:
            if (!Evaluate(instruction, out var mixinCode)) return Error(mixinCode, instruction);
            var mixinTarget = context.Evaluate(instruction.Arguments[0]);
            var priorityValue = context.Evaluate(instruction.Arguments[1]);
            if (mixinTarget is ErrorMixinValue) return Error(mixinTarget, instruction);
            if (priorityValue is ErrorMixinValue) return Error(priorityValue, instruction);
            var priorityText = priorityValue.Render(context).Resolve(context.Strings);
            int.TryParse(priorityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var priority);
            outputs.Add(
              new MixinExpressionOutput(
                MixinExpressionOutputTarget.Mixin,
                [mixinCode.Render(context)], context.Strings, mixinTarget.Render(context), priority
              )
            );
            break;
          case MixinOpcode.Log:
            if (!Evaluate(instruction, out var logged)) return Error(logged, instruction);
            logs.Add(
              new MixinExpressionLog(logged.Render(context).Resolve(context.Strings), instruction.Location.Line)
            );
            break;
          case MixinOpcode.StoreLocal:
          case MixinOpcode.StoreVariable:
          case MixinOpcode.Carry:
            if (!Evaluate(instruction, out var stored)) return Error(stored, instruction);
            if (instruction.Opcode == MixinOpcode.Carry)
              context.Carries.Store(context, instruction.Name, stored);
            else if (instruction.Opcode == MixinOpcode.StoreLocal)
              context.Locals.StoreIsolated(instruction.Name, stored);
            else context.Variables.Store(context, instruction.Name, stored);
            break;
          case MixinOpcode.Call:
            if (instruction.Destination < 0) return Failure("unknown function", instruction.Location.Line, logs);
            if (!Evaluate(instruction, out var parameter)) return Error(parameter, instruction);
            calls.Push((pc, instruction.Name, context.Parameter));
            context.Parameter = parameter;
            pc = instruction.Destination;
            break;
          case MixinOpcode.Return:
            if (!Evaluate(instruction, out var returned)) return Error(returned, instruction);
            if (calls.Count == 0) {
              pc = program.Instructions.Count;
              break;
            }
            CompleteReturn(returned);
            break;
          case MixinOpcode.Goto:
            if (instruction.Destination < 0) {
              return Failure(
                "unknown scope label '" + instruction.Name.Resolve(context.Strings) + "'",
                instruction.Location.Line, logs
              );
            }
            pc = instruction.Destination;
            break;
          case MixinOpcode.Skip:
            if (instruction.Destination < 0)
              return Failure("SKIP has no following scope", instruction.Location.Line, logs);
            pc = instruction.Destination;
            break;
          case MixinOpcode.Fail:
            if (!Evaluate(instruction, out var failed)) return Error(failed, instruction);
            var failureText = failed.Render(context).Resolve(context.Strings);
            return Failure(
              string.IsNullOrEmpty(failureText) ? "expression requested failure" : failureText,
              instruction.Location.Line, logs
            );
          case MixinOpcode.Directive:
            if (instruction.Directive is not DirectiveFunctionDefinition directive)
              return Failure("directive has no resolved runtime function", instruction.Location.Line, logs);
            var directiveValue = directive.Invoke(
              context, instruction.Arguments ?? [],
              instruction.Operand ?? NullMixinValue.Instance
            );
            if (directiveValue is ErrorMixinValue) return Error(directiveValue, instruction);
            if (directiveValue is DirectiveEffectMixinValue effect &&
              !string.IsNullOrEmpty(effect.ClassCode.Resolve(context.Strings))) {
              outputs.Add(
                new MixinExpressionOutput(
                  MixinExpressionOutputTarget.Class,
                  [effect.ClassCode], context.Strings
                )
              );
            }
            break;
          default: return Failure("unsupported compiled opcode", instruction.Location.Line, logs);
        }
      }

      executionProfile.Dispose();
      using (MixinProfiler.Measure("vm.execute.export")) {
        if (variables is not null) {
          variables.Clear();
          foreach (var item in context.Variables)
            variables[item.Key.Resolve(context.Strings)] = item.Value.Unlink(context);
          foreach (var item in context.Carries)
            variables[CarryLocalPrefix + item.Key.Resolve(context.Strings)] = item.Value.Unlink(context);
        }
      }
      var elapsed = (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency;
      return new MixinExpressionResult(
        true, null, 0, outputs, logs,
        variables?.ToDictionary(item => item.Key, item => item.Value) ?? new Dictionary<string, object>(), operations,
        elapsed
      );

      void CompleteReturn(IMixinValue value) {
        var frame = calls.Pop();
        context.Parameter = frame.Parameter;
        if (frame.Local.IsInterned || !string.IsNullOrEmpty(frame.Local.DynamicValue))
          context.Locals.StoreIsolated(frame.Local, value);
        pc = frame.Return;
      }
    } finally {
      context.ProgramInvoker = previousInvoker;
    }
  }

  private static IMixinValue EvaluateRuntime(
    MixinExpressionExecutionProgram program,
    ExecutionContext context, IMixinValue source
  ) {
    return context.Evaluate(source);
  }

  private static IMixinValue RunProgramFunction(
    MixinExpressionExecutionProgram program,
    ExecutionContext context, int entry, IMixinValue parameter
  ) {
    if (entry < 0 || entry >= program.Instructions.Count) return context.Error("invalid function entry");
    var previousParameter = context.Parameter;
    context.Parameter = parameter;
    try {
      var pc = entry;
      var steps = 0;
      while (pc < program.Instructions.Count && ++steps <= Math.Max(256, program.Instructions.Count * 16)) {
        var instruction = program.Instructions[pc++];

        IMixinValue Value() {
          return EvaluateRuntime(
            program, context,
            instruction.Operand ?? NullMixinValue.Instance
          );
        }

        switch (instruction.Opcode) {
          case MixinOpcode.Empty or MixinOpcode.Scope: continue;
          case MixinOpcode.Function:
            pc = instruction.Destination;
            continue;
          case MixinOpcode.End when instruction.SecondaryDestination != 0: return NullMixinValue.Instance;
          case MixinOpcode.End: continue;
          case MixinOpcode.Match: {
            var condition = Value();
            if (condition is ErrorMixinValue) return condition;
            if (!condition.IsTruthy(context)) {
              var destination = instruction.Destination >= 0
                ? instruction.Destination
                : instruction.SecondaryDestination;
              if (destination < 0) return context.Error("MATCH has no following scope");
              pc = destination;
            }
            continue;
          }
          case MixinOpcode.Assert: {
            var condition = Value();
            if (condition is ErrorMixinValue) return condition;
            if (!condition.IsTruthy(context)) return context.Error(instruction.Message.Resolve(context.Strings));
            continue;
          }
          case MixinOpcode.StoreLocal:
            context.Locals.StoreIsolated(instruction.Name, Value());
            continue;
          case MixinOpcode.StoreVariable:
            context.Variables.Store(context, instruction.Name, Value());
            continue;
          case MixinOpcode.Carry: {
            var carried = Value();
            context.Carries.Store(context, instruction.Name, carried);
            continue;
          }
          case MixinOpcode.Call: {
            var argument = Value();
            if (argument is ErrorMixinValue) return argument;
            var result = RunProgramFunction(program, context, instruction.Destination, argument);
            if (result is ErrorMixinValue) return result;
            if (instruction.Name.IsInterned || !string.IsNullOrEmpty(instruction.Name.DynamicValue))
              context.Locals.StoreIsolated(instruction.Name, result);
            continue;
          }
          case MixinOpcode.Return: return Value();
          case MixinOpcode.Goto:
            if (instruction.Destination < 0) return context.Error("unknown scope label");
            pc = instruction.Destination;
            continue;
          case MixinOpcode.Skip:
            if (instruction.Destination < 0) return context.Error("SKIP has no following scope");
            pc = instruction.Destination;
            continue;
          case MixinOpcode.Fail: return context.Error(Value().Render(context).Resolve(context.Strings));
          default: return context.Error("function callback contains unsupported opcode");
        }
      }
      return context.Error("function callback exceeded execution limit");
    } finally { context.Parameter = previousParameter; }
  }

  private static IMixinValue Resolve(IMixinValue value, ExecutionContext context) {
    return value switch {
      RootMixinValue root => context.Resolve(root.Root, root.Member),
      InvokeMixinValue invocation => context.Invoke(
        invocation.Function, invocation.Instance, invocation.Arguments, invocation.Negated
      ),
      _ => value
    };
  }

  private static IMixinValue FromObject(ExecutionContext context, object value) {
    return value switch {
      IMixinValue typed => typed, null => NullMixinValue.Instance,
      bool boolean => boolean ? BooleanMixinValue.True : BooleanMixinValue.False,
      string text => new LiteralMixinValue(context.ResolveString(text)),
      DetachedSemanticData detached => DetachedSemanticMixinValue.Materialize(detached),
      IReadOnlyDictionary<string, object> table => new MixinTableValue(
        [
          .. table.Select(item =>
            new KeyValuePair<MixinString, IMixinValue>(context.ResolveString(item.Key), FromObject(context, item.Value))
          )
        ]
      ),
      IDictionary<string, object> table => new MixinTableValue(
        [
          .. table.Select(item =>
            new KeyValuePair<MixinString, IMixinValue>(context.ResolveString(item.Key), FromObject(context, item.Value))
          )
        ]
      ),
      _ => new ObjectMixinValue(value)
    };
  }

  private static int FindNext(IReadOnlyList<MixinInstruction> instructions, int pc) {
    for (var i = pc; i < instructions.Count; i++)
      if (instructions[i].Opcode is MixinOpcode.Scope or MixinOpcode.End)
        return i;
    return instructions.Count;
  }

  private static MixinExpressionResult Failure(string error, int line, IReadOnlyList<MixinExpressionLog> logs = null) {
    return new MixinExpressionResult(false, error, line, [], logs ?? []);
  }
}
