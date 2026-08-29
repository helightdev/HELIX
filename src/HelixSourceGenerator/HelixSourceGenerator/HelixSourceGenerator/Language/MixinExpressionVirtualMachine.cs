using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language;

using static MixinExpressionCompiler;
public static partial class MixinExpressionVirtualMachine {
  internal const string ParameterLocalKey = "\0@param";
  internal const string CarryLocalPrefix = "\0@carry:";

  public static MixinExpressionResult Execute(
    string expression, IMixinExpressionContext context, IDictionary<string, object> variables = null
  ) {
    return Execute(expression, context, variables, (MixinExpressionPreparedState)null);
  }

  public static MixinExpressionResult Execute(
    string expression, IMixinExpressionContext context, IDictionary<string, object> variables,
    MixinExpressionPreparedState preparedState
  ) {
    return CompileAndExecute(
      expression is null ? null : MixinExpressionParser.Parse(expression), context, variables, preparedState
    );
  }

  public static MixinExpressionResult Execute(
    string expression, IMixinExpressionContext context, IDictionary<string, object> variables,
    IEnumerable<string> preparedExpressions
  ) {
    return Execute(expression, context, variables, PrepareGlobals(preparedExpressions ?? []));
  }

  internal static MixinExpressionResult ExecuteCompiled(
    MixinProgramSyntax program, IMixinExpressionContext context, IDictionary<string, object> variables,
    MixinExpressionPreparedState preparedState
  ) {
    return CompileAndExecute(program, context, variables, preparedState);
  }

  private static MixinExpressionResult CompileAndExecute(
    MixinProgramSyntax program, IMixinExpressionContext context, IDictionary<string, object> variables,
    MixinExpressionPreparedState preparedState
  ) {
    if (context is null) throw new ArgumentNullException(nameof(context));
    if (!TryCompileExecution(program, preparedState, out var compiled, out var error, out var line))
      return Failure(error, line);
    return Execute(compiled, context, variables);
  }

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
    if (variables is not null)
      foreach (var item in variables)
        pendingVariables[item.Key] = item.Value;
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
              [MixinString.Dynamic(completed.Render())], executionPool,
              executionPool.Get(frame.injectionTarget)
            )
          );
          break;
        case FrameContinuation.Return:
          return CompleteCall(completed, out finishError);
      }
      return true;
    }

    IMixinValue TransformParameter(CallFrame frame) {
      var item = frame.inputs[frame.inputIndex];
      if (frame.transform.Kind == TableTransformKind.MapValues) return item.Value;
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
        parameter = MixinValue.From(previous, context), continuation = continuation, transform = request,
        inputs = [.. request.Source.Entries], accumulator = new MixinExpressionTable(), functionStart = callback.Start,
        destination = destination, outputTarget = outputTarget, injectionTarget = injectionTarget
      };
      if (frame.inputs.Length == 0) return FinishTransform(frame, out beginError);
      calls.Push(frame);
      locals[ParameterLocalKey] = TransformParameter(frame);
      pc = frame.functionStart;
      return true;
    }

    bool CompleteCall(IMixinValue returned, out string completeError) {
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
        if (returned.IsTruthy)
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
      if (parsed is EmptyDirectiveSyntax) continue;
      if (preparedInitializers.Contains(instruction)) continue;
      executedOperations++;

      if (parsed is DirectiveInvocationSyntax invocationSyntax &&
        invocationSyntax.Definition is DirectiveFunctionDefinition directiveFunction) {
        var invocation = new DirectiveFunctionInvocation(
          invocationSyntax, context, locals, pendingVariables, outputs
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
            if (!CompleteCall(NullMixinValue.Instance, out var endCallError))
              return Failure(endCallError, lineNumber, logs);
          }
          break;
        case MatchDirectiveSyntax matchSyntax:
          if (!TryEvaluateAll(
            matchSyntax.Expression, context, locals, pendingVariables,
            out var matched, out var matchError, out var matchFailure
          )) return Failure(matchError, lineNumber, logs);
          if (!matched) {
            if (!string.IsNullOrEmpty(matchSyntax.FailureLabel)) {
              var matchScope = instructionScopes[instruction];
              var matchKey = ScopeLabelKey(matchScope, matchSyntax.FailureLabel);
              if (!labels.TryGetValue(matchKey, out var matchDestination))
                return Failure("unknown scope label '" + matchSyntax.FailureLabel + "'", lineNumber, logs);
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
        case AssertDirectiveSyntax assertSyntax:
          if (!TryEvaluateAll(
            assertSyntax.Expression, context, locals, pendingVariables,
            out var asserted, out var assertError, out var assertFailure
          )) return Failure(assertError, lineNumber, logs);
          if (!asserted) return Failure(assertFailure, lineNumber, logs);
          break;
        case CodeDirectiveSyntax codeSyntax:
          var outputTarget = codeSyntax.Target;
          var injectionTarget = codeSyntax.InjectionTarget;
          if (codeSyntax.Expression is { Count: 1 } && codeSyntax.Expression[0].Reference is not null) {
            if (!TryEvaluateExpression(
              codeSyntax.Expression, context, locals, pendingVariables, out var codeValue, out var codeValueError
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
            codeSyntax.Expression, context, locals, pendingVariables, executionPool,
            out var code, out var codeError
          )) return Failure(codeError, lineNumber, logs);
          outputs.Add(
            new MixinExpressionOutput(
              outputTarget, code, executionPool, executionPool.Get(injectionTarget)
            )
          );
          break;
        case MixinDirectiveSyntax mixinSyntax:
          if (!TryInterpolateSegments(
            mixinSyntax.Expression, context, locals, pendingVariables, executionPool, out var mixinCode,
            out var mixinCodeError
          )) return Failure(mixinCodeError, lineNumber, logs);
          if (!TryResolveDirectiveArgument(
            mixinSyntax.Target, context, locals, pendingVariables, out var mixinTarget,
            out var mixinArgumentError
          )) return Failure(mixinArgumentError, lineNumber, logs);
          if (string.IsNullOrEmpty(mixinTarget)) return Failure("MIXIN target is empty", lineNumber, logs);
          var mixinPriority = 0;
          if (mixinSyntax.Priority is not null) {
            if (!TryResolveDirectiveArgument(
              mixinSyntax.Priority, context, locals, pendingVariables, out var mixinPriorityText,
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
        case UsingDirectiveSyntax usingSyntax:
          if (!TryInterpolateSegments(
            usingSyntax.Expression, context, locals, pendingVariables, executionPool,
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
        case LogDirectiveSyntax logSyntax:
          if (!TryInterpolate(
            logSyntax.Expression, context, locals, pendingVariables, out var log,
            out var logError
          )) return Failure(logError, lineNumber, logs);
          logs.Add(new MixinExpressionLog(log, lineNumber));
          break;
        case LocalDirectiveSyntax:
        case VariableDirectiveSyntax:
          var storeName = parsed is LocalDirectiveSyntax localSyntax
            ? localSyntax.Name
            : ((VariableDirectiveSyntax)parsed).Name;
          if (!TryEvaluateExpression(
            ((ValueDirectiveSyntax)parsed).Expression, context, locals, pendingVariables, out var stored,
            out var storeError
          )) return Failure(storeError, lineNumber, logs);
          if (stored is MixinTransformRequest storeTransform) {
            if (!BeginTransform(
              storeTransform,
              parsed is LocalDirectiveSyntax
                ? FrameContinuation.StoreLocal
                : FrameContinuation.StoreVariable,
              storeName, default, null, out var beginStoreError
            )) return Failure(beginStoreError, lineNumber, logs);
            break;
          }
          (parsed is LocalDirectiveSyntax ? locals : pendingVariables)[storeName] = stored;
          break;
        case CarryDirectiveSyntax carrySyntax:
          if (TryEvaluateExpression(
            carrySyntax.Expression, context, locals, pendingVariables,
            out var carried, out var carryError
          )) pendingVariables[CarryLocalPrefix + carrySyntax.Label] = MixinValue.Unlink(carried, context);
          else pendingVariables[CarryLocalPrefix + carrySyntax.Label] = new FailedMixinValue(carryError);
          break;
        case ReturnDirectiveSyntax returnSyntax:
          if (calls.Count != 0) {
            IMixinValue returnValue = NullMixinValue.Instance;
            if (HasExpression(returnSyntax.Expression) && !TryEvaluateExpression(
              returnSyntax.Expression, context, locals, pendingVariables,
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
        case CallDirectiveSyntax callSyntax:
          var callFunctionLabel = callSyntax.Function;
          var callReturnLocal = callSyntax.ReturnLocal;
          if (string.IsNullOrEmpty(callFunctionLabel) || !functions.TryGetValue(callFunctionLabel, out var function))
            return Failure("unknown function '" + (callFunctionLabel ?? "") + "'", lineNumber, logs);
          var hadParameter = locals.TryGetValue(ParameterLocalKey, out var previousParameter);
          IMixinValue callParameter = NullMixinValue.Instance;
          if (HasExpression(callSyntax.Expression) && !TryEvaluateExpression(
            callSyntax.Expression, context, locals, pendingVariables,
            out callParameter, out var callParameterError
          )) return Failure(callParameterError, lineNumber, logs);
          calls.Push(
            new CallFrame {
              returnAddress = pc, hadParameter = hadParameter, parameter = MixinValue.From(previousParameter, context),
              returnLocal = callReturnLocal, continuation = FrameContinuation.Call
            }
          );
          locals[ParameterLocalKey] = callParameter;
          pc = function.Start;
          break;
        case InlineDirectiveSyntax:
          return Failure("INLINE must be expanded before evaluation", lineNumber, logs);
        case GotoDirectiveSyntax gotoSyntax:
          var gotoScope = instructionScopes[instruction];
          var gotoKey = ScopeLabelKey(gotoScope, gotoSyntax.Label ?? "");
          if (string.IsNullOrEmpty(gotoSyntax.Label) || !labels.TryGetValue(gotoKey, out var destination))
            return Failure("unknown scope label '" + (gotoSyntax.Label ?? "") + "'", lineNumber, logs);
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
        case FailDirectiveSyntax failSyntax:
          if (!HasExpression(failSyntax.Expression)) return Failure("expression requested failure", lineNumber, logs);
          return Failure(
            !TryInterpolate(
              failSyntax.Expression, context, locals, pendingVariables, out var failureMessage, out var failureError
            )
              ? failureError
              : failureMessage, lineNumber, logs
          );
        default:
          return Failure("invalid compiled instruction", lineNumber, logs);
      }
    }

    CommitVariables(variables, pendingVariables);
    return SuccessfulResult();
  }

  private static bool HasExpression(IReadOnlyList<ValueExpressionPart> expression) {
    return expression is { Count: > 1 } || expression is { Count: 1 } &&
      (expression[0].Reference is not null || !string.IsNullOrEmpty(expression[0].Literal));
  }

  private static void RestoreCallParameter(MixinValueDictionary locals, CallFrame frame) {
    if (frame.hadParameter) locals[ParameterLocalKey] = frame.parameter;
    else locals.Remove(ParameterLocalKey);
  }

  internal static void CommitVariables(
    IDictionary<string, object> destination, IReadOnlyDictionary<string, object> source
  ) {
    if (destination is null) return;
    destination.Clear();
    foreach (var item in source) destination[item.Key] = item.Value;
  }

  internal static MixinExpressionResult Success(
    IReadOnlyList<MixinExpressionOutput> outputs, IReadOnlyList<MixinExpressionLog> logs,
    IReadOnlyDictionary<string, object> variables = null, int executedOperations = 0,
    double executionMilliseconds = 0
  ) {
    return new MixinExpressionResult(
      true, null, 0, outputs, logs, variables, executedOperations, executionMilliseconds
    );
  }

  internal static MixinExpressionResult Failure(
    string error, int line, IReadOnlyList<MixinExpressionLog> logs = null
  ) {
    return new MixinExpressionResult(false, error, line, [], logs);
  }

  internal enum FrameContinuation { Call, StoreLocal, StoreVariable, EmitCode, Return }

  internal sealed class CallFrame {
    internal MixinExpressionTable accumulator;
    internal FrameContinuation continuation;
    internal string destination;
    internal int functionStart;
    internal bool hadParameter;
    internal string injectionTarget;
    internal int inputIndex;
    internal KeyValuePair<string, IMixinValue>[] inputs;
    internal MixinExpressionOutputTarget outputTarget;
    internal IMixinValue parameter;
    internal int returnAddress;
    internal string returnLocal;
    internal MixinTransformRequest transform;
  }
}
