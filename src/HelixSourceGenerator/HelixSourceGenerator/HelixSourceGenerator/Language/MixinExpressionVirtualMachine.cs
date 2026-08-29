using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language;

using static MixinExpressionInterpreter;
using static MixinExpressionEvaluator;
using static MixinExpressionCompiler;

internal static class MixinExpressionVirtualMachine {
  internal static MixinExpressionResult Execute(
    MixinExpressionExecutionProgram program,
    IMixinExpressionContext context,
    IDictionary<string, object> variables
  ) {
    var executionStartedAt = Stopwatch.GetTimestamp();
    if (context is null) throw new ArgumentNullException(nameof(context));
    var lines = program.Instructions;
    var preparedInitializers = program.Initializers;
    var labels = program.Labels;
    var instructionScopes = program.InstructionScopes;
    var functions = program.Functions;
    var functionStarts = program.FunctionStarts;
    var functionEnds = program.FunctionEnds;
    var executionPool = program.StringPool;
    var locals = new MixinValueDictionary(executionPool);
    var pendingVariables = new MixinValueDictionary(executionPool);
    foreach (var item in program.Variables) pendingVariables[item.Key] = item.Value;
    if (variables is not null) {
      foreach (var item in variables) pendingVariables[item.Key] = item.Value;
    }
    var outputs = new List<MixinExpressionOutput>();
    var logs = new List<MixinExpressionLog>();
    var pc = 0;
    var steps = 0;
    var executedOperations = 0;
    var maximumSteps = Math.Max(1024, lines.Count * 64);
    var calls = new Stack<CallFrame>();

    MixinExpressionResult SuccessfulResult() {
      var unlinkedVariables = pendingVariables.ToDictionary(
        item => item.Key,
        item => MixinValue.Unlink(item.Value, context),
        StringComparer.Ordinal
      );
      var elapsedMilliseconds =
        (Stopwatch.GetTimestamp() - executionStartedAt) * 1000d / Stopwatch.Frequency;
      return Success(outputs, logs, unlinkedVariables, executedOperations, elapsedMilliseconds);
    }

    bool FinishTransform(CallFrame frame, out string finishError) {
      if (!TryResumeTransform(
        frame.transform, context, frame.accumulator, out var completed, out finishError
      )) return false;
      switch (frame.continuation) {
        case FrameContinuation.StoreLocal: locals[frame.destination] = completed; break;
        case FrameContinuation.StoreVariable: pendingVariables[frame.destination] = completed; break;
        case FrameContinuation.EmitCode:
          outputs.Add(
            new MixinExpressionOutput(
              frame.outputTarget,
              [MixinString.Dynamic(Render(completed))], executionPool,
              executionPool.Get(frame.injectionTarget)
            )
          );
          break;
        case FrameContinuation.Return:
          return CompleteCall(completed, out finishError);
      }
      return true;
    }

    object TransformParameter(CallFrame frame) {
      var item = frame.inputs[frame.inputIndex];
      if (frame.transform.Kind == TableTransformKind.MapValues) return item.Value.Value;
      return new MixinExpressionTable().Put("k", item.Key).Put("v", item.Value);
    }

    bool BeginTransform(
      MixinTransformRequest request, FrameContinuation continuation, string destination,
      MixinExpressionOutputTarget outputTarget, string injectionTarget, out string beginError
    ) {
      beginError = null;
      if (!functions.TryGetValue(request.FunctionLabel ?? "", out var callback)) {
        beginError = "unknown function '" + (request.FunctionLabel ?? "") + "'";
        return false;
      }
      var frame = new CallFrame {
        returnAddress = pc, hadParameter = locals.TryGetValue(ParameterLocalKey, out var previous),
        parameter = previous, continuation = continuation, transform = request, inputs = [.. request.Source.Entries],
        accumulator = new MixinExpressionTable(), functionStart = callback.Start, destination = destination,
        outputTarget = outputTarget, injectionTarget = injectionTarget
      };
      if (frame.inputs.Length == 0) return FinishTransform(frame, out beginError);
      calls.Push(frame);
      locals[ParameterLocalKey] = TransformParameter(frame);
      pc = frame.functionStart;
      return true;
    }

    bool CompleteCall(object returned, out string completeError) {
      completeError = null;
      var frame = calls.Peek();
      if (frame.transform is null) {
        calls.Pop();
        RestoreCallParameter(locals, frame);
        if (!string.IsNullOrEmpty(frame.returnLocal)) locals[frame.returnLocal] = returned;
        pc = frame.returnAddress;
        return true;
      }
      var input = frame.inputs[frame.inputIndex];
      if (frame.transform.Kind == TableTransformKind.Filter) {
        if (MixinValue.From(returned, context).IsTruthy)
          frame.accumulator = frame.accumulator.Put(input.Key, input.Value);
      } else frame.accumulator = frame.accumulator.Put(input.Key, returned);
      frame.inputIndex++;
      if (frame.inputIndex < frame.inputs.Length) {
        locals[ParameterLocalKey] = TransformParameter(frame);
        pc = frame.functionStart;
        return true;
      }
      calls.Pop();
      RestoreCallParameter(locals, frame);
      pc = frame.returnAddress;
      return FinishTransform(frame, out completeError);
    }

    while (pc < lines.Count) {
      if (++steps > maximumSteps) return Failure("execution limit exceeded (possible GOTO loop)", pc + 1, logs);
      if (functionStarts.TryGetValue(pc, out var functionEnd)) {
        pc = functionEnd + 1;
        continue;
      }
      var lineNumber = pc + 1;
      var instruction = pc;
      var parsed = lines[pc++];
      var command = parsed.Command;
      var argument = parsed.Argument;
      var operand = parsed.Operand;
      if (string.IsNullOrEmpty(command)) continue;
      if (preparedInitializers.Contains(instruction)) continue;
      executedOperations++;

      if (parsed is DirectiveInvocationSyntax { Definition: DirectiveFunctionDefinition directiveFunction }) {
        var invocation = new DirectiveFunctionInvocation(
          parsed, context, locals, pendingVariables, outputs
        );
        if (!directiveFunction.Invoke(invocation, out var directiveFunctionError))
          return Failure(directiveFunctionError, lineNumber, logs);
        continue;
      }

      // Dispatch on AST node kinds. Directive spelling and parser details do not leak into execution.
      switch (parsed) {
        case ScopeDirectiveSyntax:
        case LabelDirectiveSyntax:
        case FunctionDirectiveSyntax:
          break;
        case EndDirectiveSyntax:
          if (functionEnds.Contains(instruction) && calls.Count != 0) {
            if (!CompleteCall(null, out var endCallError))
              return Failure(endCallError, lineNumber, logs);
          }
          break;
        case MatchDirectiveSyntax:
          if (!TryEvaluateAll(
            parsed.BooleanExpression, context, locals, pendingVariables,
            out var matched, out var matchError, out var matchFailure
          )) return Failure(matchError, lineNumber, logs);
          if (!matched) {
            if (!string.IsNullOrEmpty(argument)) {
              var matchScope = instructionScopes[instruction];
              var matchKey = ScopeLabelKey(matchScope, argument);
              if (!labels.TryGetValue(matchKey, out var matchDestination))
                return Failure("unknown scope label '" + argument + "'", lineNumber, logs);
              pc = matchDestination + 1;
              break;
            }
            var next = FindNextScopeOrEnd(
              lines, pc, instructionScopes[instruction], instructionScopes,
              functionStarts, functionEnds
            );
            if (next < 0) {
              return Failure(
                matchFailure + "; there is no following scope", lineNumber, logs
              );
            }
            pc = next;
          }
          break;
        case AssertDirectiveSyntax:
          if (!TryEvaluateAll(
            parsed.BooleanExpression, context, locals, pendingVariables,
            out var asserted, out var assertError, out var assertFailure
          )) return Failure(assertError, lineNumber, logs);
          if (!asserted) return Failure(assertFailure, lineNumber, logs);
          break;
        case CodeDirectiveSyntax:
          TryOutputTarget(argument, out var outputTarget, out var injectionTarget);
          if (parsed.ValueExpression is { Count: 1 } && parsed.ValueExpression[0].Reference is not null) {
            if (!TryEvaluateExpression(
              parsed.ValueExpression, context, locals, pendingVariables, out var codeValue, out var codeValueError
            )) return Failure(codeValueError, lineNumber, logs);
            if (codeValue is MixinTransformRequest codeTransform) {
              if (!BeginTransform(
                codeTransform, FrameContinuation.EmitCode, null, outputTarget, injectionTarget,
                out var beginCodeError
              )) return Failure(beginCodeError, lineNumber, logs);
              break;
            }
          }
          if (!TryInterpolateSegments(
            parsed.ValueExpression, context, locals, pendingVariables, executionPool,
            out var code, out var codeError
          )) return Failure(codeError, lineNumber, logs);
          outputs.Add(
            new MixinExpressionOutput(
              outputTarget, code, executionPool, executionPool.Get(injectionTarget)
            )
          );
          break;
        case MixinDirectiveSyntax:
          if (!TryInterpolateSegments(
            parsed.ValueExpression, context, locals, pendingVariables, executionPool, out var mixinCode,
            out var mixinCodeError
          )) return Failure(mixinCodeError, lineNumber, logs);
          if (!TryResolveDirectiveArgument(
            argument, context, locals, pendingVariables, out var mixinTarget,
            out var mixinArgumentError
          )) return Failure(mixinArgumentError, lineNumber, logs);
          if (string.IsNullOrEmpty(mixinTarget)) return Failure("MIXIN target is empty", lineNumber, logs);
          var mixinPriority = 0;
          if (parsed.Arguments.Count == 2) {
            if (!TryResolveDirectiveArgument(
              parsed.Arguments[1], context, locals, pendingVariables, out var mixinPriorityText,
              out var mixinPriorityArgumentError
            )) return Failure(mixinPriorityArgumentError, lineNumber, logs);
            if (!int.TryParse(
              mixinPriorityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out mixinPriority
            )) {
              return Failure(
                "MIXIN priority '" + (mixinPriorityText ?? "") +
                "' is not a valid 32-bit integer", lineNumber, logs
              );
            }
          }
          outputs.Add(
            new MixinExpressionOutput(
              MixinExpressionOutputTarget.Mixin, mixinCode, executionPool,
              executionPool.Get(mixinTarget), mixinPriority
            )
          );
          break;
        case UsingDirectiveSyntax:
          if (!TryInterpolateSegments(
            parsed.ValueExpression, context, locals, pendingVariables, executionPool,
            out var usingDirective,
            out var usingError
          )) return Failure(usingError, lineNumber, logs);
          outputs.Add(
            new MixinExpressionOutput(
              MixinExpressionOutputTarget.Using, usingDirective, executionPool,
              MixinString.Dynamic(null)
            )
          );
          break;
        case LogDirectiveSyntax:
          if (!TryInterpolate(
            parsed.ValueExpression, context, locals, pendingVariables, out var log,
            out var logError
          )) return Failure(logError, lineNumber, logs);
          logs.Add(new MixinExpressionLog(log, lineNumber));
          break;
        case LocalDirectiveSyntax:
        case VariableDirectiveSyntax:
          if (string.IsNullOrEmpty(argument)) return Failure(command + " requires a name", lineNumber, logs);
          if (!TryEvaluateExpression(
            parsed.ValueExpression, context, locals, pendingVariables, out var stored, out var storeError
          )) return Failure(storeError, lineNumber, logs);
          if (stored is MixinTransformRequest storeTransform) {
            if (!BeginTransform(
              storeTransform,
              parsed is LocalDirectiveSyntax
                ? FrameContinuation.StoreLocal
                : FrameContinuation.StoreVariable,
              argument, default, null, out var beginStoreError
            )) return Failure(beginStoreError, lineNumber, logs);
            break;
          }
          (parsed is LocalDirectiveSyntax ? locals : pendingVariables)[argument] = stored;
          break;
        case CarryDirectiveSyntax:
          if (string.IsNullOrEmpty(argument)) return Failure("CARRY requires a label", lineNumber, logs);
          if (TryEvaluateExpression(
            parsed.ValueExpression, context, locals, pendingVariables,
            out var carried, out var carryError
          )) pendingVariables[CarryLocalPrefix + argument] = MixinValue.Unlink(carried, context);
          else pendingVariables[CarryLocalPrefix + argument] = new FailedMixinValue(carryError);
          break;
        case ReturnDirectiveSyntax:
          if (calls.Count != 0) {
            object returnValue = null;
            if (!string.IsNullOrEmpty(operand) && !TryEvaluateExpression(
              parsed.ValueExpression, context, locals, pendingVariables,
              out returnValue, out var returnError
            )) return Failure(returnError, lineNumber, logs);
            if (returnValue is MixinTransformRequest returnTransform) {
              if (!BeginTransform(
                returnTransform, FrameContinuation.Return, null, default, null,
                out var beginReturnError
              )) return Failure(beginReturnError, lineNumber, logs);
              break;
            }
            if (!CompleteCall(returnValue, out var completeCallError))
              return Failure(completeCallError, lineNumber, logs);
            break;
          }
          CommitVariables(variables, pendingVariables);
          return SuccessfulResult();
        case CallDirectiveSyntax:
          var callFunctionLabel = parsed.Arguments.Count == 2 ? parsed.Arguments[1] : argument;
          var callReturnLocal = parsed.Arguments.Count == 2 ? argument : null;
          if (string.IsNullOrEmpty(callFunctionLabel) || !functions.TryGetValue(callFunctionLabel, out var function))
            return Failure("unknown function '" + (callFunctionLabel ?? "") + "'", lineNumber, logs);
          var hadParameter = locals.TryGetValue(ParameterLocalKey, out var previousParameter);
          object callParameter = null;
          if (!string.IsNullOrEmpty(operand) && !TryEvaluateExpression(
            parsed.ValueExpression, context, locals, pendingVariables,
            out callParameter, out var callParameterError
          )) return Failure(callParameterError, lineNumber, logs);
          calls.Push(
            new CallFrame {
              returnAddress = pc, hadParameter = hadParameter, parameter = previousParameter,
              returnLocal = callReturnLocal, continuation = FrameContinuation.Call
            }
          );
          locals[ParameterLocalKey] = callParameter;
          pc = function.Start;
          break;
        case InlineDirectiveSyntax:
          return Failure("INLINE must be expanded before evaluation", lineNumber, logs);
        case GotoDirectiveSyntax:
          var gotoScope = instructionScopes[instruction];
          var gotoKey = ScopeLabelKey(gotoScope, argument ?? "");
          if (string.IsNullOrEmpty(argument) || !labels.TryGetValue(gotoKey, out var destination))
            return Failure("unknown scope label '" + (argument ?? "") + "'", lineNumber, logs);
          pc = destination + 1;
          break;
        case SkipDirectiveSyntax:
          var skip = FindNextScopeOrEnd(
            lines, pc, instructionScopes[instruction], instructionScopes,
            functionStarts, functionEnds
          );
          if (skip < 0) return Failure("SKIP has no following scope", lineNumber, logs);
          pc = skip;
          break;
        case FailDirectiveSyntax:
          if (string.IsNullOrEmpty(operand)) return Failure("expression requested failure", lineNumber, logs);
          return Failure(!TryInterpolate(
            parsed.ValueExpression, context, locals, pendingVariables, out var failureMessage, out var failureError
          ) ? failureError : failureMessage, lineNumber, logs);
        default:
          return Failure("unknown directive '@" + command + "'", lineNumber, logs);
      }
    }

    CommitVariables(variables, pendingVariables);
    return SuccessfulResult();
  }
}
