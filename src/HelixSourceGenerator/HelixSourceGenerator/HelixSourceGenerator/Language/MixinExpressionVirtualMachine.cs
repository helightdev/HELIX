using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace HELIX.SourceGen.Expressions;

using static MixinExpressionInterpreter;
using static MixinExpressionEvaluator;
using static MixinExpressionCompiler;

internal static class MixinExpressionVirtualMachine {
  internal static MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, object> variables,
    MixinExpressionPreparedState preparedState
  ) => Execute(expression is null ? null : GetProgram(expression, false), context, variables, preparedState);

  internal static MixinExpressionResult Execute(
    MixinProgramSyntax localProgram,
    IMixinExpressionContext context,
    IDictionary<string, object> variables,
    MixinExpressionPreparedState preparedState
  ) {
    var executionStartedAt = Stopwatch.GetTimestamp();
    if (context is null) throw new ArgumentNullException(nameof(context));
    if (localProgram is null) return Failure("the expression is null", 0);

    var preparedLines = preparedState?.Instructions ?? Array.Empty<DirectiveInstruction>();
    var preparedInitializers = preparedState?.Initializers ?? new HashSet<int>();
    // Attribute expressions are deliberately lazy: their instruction AST nodes are created
    // only when this execution's control-flow scan or program counter reaches the line.
    var preparedCount = preparedLines.Count;
    IReadOnlyList<DirectiveInstruction> lines = new InstructionSequence(preparedLines, localProgram);
    IDictionary<string, int> labels = preparedState is null
      ? new Dictionary<string, int>(StringComparer.Ordinal)
      : new MixinStringDictionary<int>(preparedState.Labels, preparedState.StringPool);
    var instructionScopes = preparedState is null
      ? new Dictionary<int, int>()
      : preparedState.InstructionScopes.ToDictionary(item => item.Key, item => item.Value);
    IDictionary<string, MixinExpressionCompiler.FunctionDefinition> functions = preparedState is null
      ? new Dictionary<string, MixinExpressionCompiler.FunctionDefinition>(StringComparer.Ordinal)
      : new MixinStringDictionary<MixinExpressionCompiler.FunctionDefinition>(
        preparedState.Functions, preparedState.StringPool
      );
    var functionStarts = preparedState is null
      ? new Dictionary<int, int>()
      : preparedState.FunctionStarts.ToDictionary(item => item.Key, item => item.Value);
    var functionEnds = preparedState is null
      ? new HashSet<int>()
      : new HashSet<int>(preparedState.FunctionEnds);
    var executionPool = preparedState?.StringPool;
    if (executionPool is null) {
      var poolBuilder = new MixinStringPoolBuilder();
      localProgram.CollectConstants(poolBuilder);
      executionPool = poolBuilder.Freeze();
    }
    var locals = new MixinValueDictionary(executionPool);
    var pendingVariables = new MixinValueDictionary(executionPool);
    if (preparedState is not null) {
      foreach (var item in preparedState.Variables)
        pendingVariables[item.Key] = item.Value;
    }
    if (variables is not null) {
      foreach (var item in variables)
        pendingVariables[item.Key] = item.Value;
    }
    var outputs = new List<MixinExpressionOutput>();
    var logs = new List<MixinExpressionLog>();
    var pc = 0;
    var steps = 0;
    var executedOperations = 0;
    var maximumSteps = Math.Max(1024, lines.Count * 64);
    var calls = new Stack<CallFrame>();
    var localSymbolsIndexed = false;

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
        frame.Transform, context, frame.Accumulator, out var completed, out finishError
      )) return false;
      switch (frame.Continuation) {
        case FrameContinuation.StoreLocal: locals[frame.Destination] = completed; break;
        case FrameContinuation.StoreVariable: pendingVariables[frame.Destination] = completed; break;
        case FrameContinuation.EmitCode:
          outputs.Add(new MixinExpressionOutput(
            frame.OutputTarget,
            new[] { MixinString.Dynamic(Render(completed)) }, executionPool,
            executionPool.Get(frame.InjectionTarget)
          ));
          break;
        case FrameContinuation.Return:
          return CompleteCall(completed, out finishError);
      }
      return true;
    }

    object TransformParameter(CallFrame frame) {
      var item = frame.Inputs[frame.InputIndex];
      if (frame.Transform.Kind == TableTransformKind.MapValues) return item.Value.BackingValue;
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
        ReturnAddress = pc,
        HadParameter = locals.TryGetValue(ParameterLocalKey, out var previous),
        Parameter = previous,
        Continuation = continuation,
        Transform = request,
        Inputs = request.Source.Entries.ToArray(),
        Accumulator = new MixinExpressionTable(),
        FunctionStart = callback.Start,
        Destination = destination,
        OutputTarget = outputTarget,
        InjectionTarget = injectionTarget
      };
      if (frame.Inputs.Length == 0) return FinishTransform(frame, out beginError);
      calls.Push(frame);
      locals[ParameterLocalKey] = TransformParameter(frame);
      pc = frame.FunctionStart;
      return true;
    }

    bool CompleteCall(object returned, out string completeError) {
      completeError = null;
      var frame = calls.Peek();
      if (frame.Transform is null) {
        calls.Pop();
        RestoreCallParameter(locals, frame);
        if (!string.IsNullOrEmpty(frame.ReturnLocal)) locals[frame.ReturnLocal] = returned;
        pc = frame.ReturnAddress;
        return true;
      }
      var input = frame.Inputs[frame.InputIndex];
      if (frame.Transform.Kind == TableTransformKind.Filter) {
        if (MixinValue.From(returned, context).IsTruthy)
          frame.Accumulator = frame.Accumulator.Put(input.Key, input.Value);
      } else frame.Accumulator = frame.Accumulator.Put(input.Key, returned);
      frame.InputIndex++;
      if (frame.InputIndex < frame.Inputs.Length) {
        locals[ParameterLocalKey] = TransformParameter(frame);
        pc = frame.FunctionStart;
        return true;
      }
      calls.Pop();
      RestoreCallParameter(locals, frame);
      pc = frame.ReturnAddress;
      return FinishTransform(frame, out completeError);
    }

    while (pc < lines.Count) {
      if (++steps > maximumSteps) return Failure("execution limit exceeded (possible GOTO loop)", pc + 1, logs);
      if (pc >= preparedCount && !localSymbolsIndexed && lines[pc].Command == "FUNC") {
        if (!TryIndexSymbols(
          lines, preparedCount, lines.Count, labels, instructionScopes,
          functions, functionStarts, functionEnds, out var symbolError,
          out var symbolLine
        )) return Failure(symbolError, symbolLine, logs);
        localSymbolsIndexed = true;
      }
      if (functionStarts.TryGetValue(pc, out var functionEnd)) {
        pc = functionEnd + 1;
        continue;
      }
      var lineNumber = pc + 1;
      var instruction = pc;
      var parsed = lines[pc++];
      if (parsed.Error is not null) return Failure(parsed.Error, lineNumber, logs);
      var command = parsed.Command;
      var argument = parsed.Argument;
      var operand = parsed.Operand;
      if (string.IsNullOrEmpty(command)) continue;
      if (preparedInitializers.Contains(instruction)) continue;
      executedOperations++;

      // The interpreter has already compiled source names to opcodes. From here on this is
      // deliberately a small stack VM; directive spelling and syntax do not leak into dispatch.
      switch (parsed.Opcode) {
        case DirectiveOpcode.Scope:
        case DirectiveOpcode.Function:
          break;
        case DirectiveOpcode.End:
          if (functionEnds.Contains(instruction) && calls.Count != 0) {
            if (!CompleteCall(null, out var endCallError))
              return Failure(endCallError, lineNumber, logs);
          }
          break;
        case DirectiveOpcode.Match:
          if (!TryEvaluateAll(
            parsed.BooleanExpression, context, locals, pendingVariables,
            out var matched, out var matchError, out var matchFailure
          )) return Failure(matchError, lineNumber, logs);
          if (!matched) {
            if (!string.IsNullOrEmpty(argument)) {
              var matchScope = instructionScopes.TryGetValue(instruction, out var indexedMatchScope)
                ? indexedMatchScope
                : -preparedCount - 1;
              var matchKey = ScopeLabelKey(matchScope, argument);
              if (!labels.ContainsKey(matchKey) && !localSymbolsIndexed) {
                if (!TryIndexSymbols(
                  lines, preparedCount, lines.Count, labels, instructionScopes,
                  functions, functionStarts, functionEnds,
                  out var symbolError, out var symbolLine
                )) return Failure(symbolError, symbolLine, logs);
                localSymbolsIndexed = true;
                matchKey = ScopeLabelKey(instructionScopes[instruction], argument);
              }
              if (!labels.TryGetValue(matchKey, out var matchDestination))
                return Failure("unknown scope label '" + argument + "'", lineNumber, logs);
              pc = matchDestination + 1;
              break;
            }
            if (!instructionScopes.ContainsKey(instruction)) {
              if (!TryIndexSymbols(
                lines, preparedCount, lines.Count, labels, instructionScopes,
                functions, functionStarts, functionEnds,
                out var symbolError, out var symbolLine
              )) return Failure(symbolError, symbolLine, logs);
              localSymbolsIndexed = true;
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
        case DirectiveOpcode.Assert:
          if (!TryEvaluateAll(
            parsed.BooleanExpression, context, locals, pendingVariables,
            out var asserted, out var assertError, out var assertFailure
          )) return Failure(assertError, lineNumber, logs);
          if (!asserted) return Failure(assertFailure, lineNumber, logs);
          break;
        case DirectiveOpcode.Code:
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
          outputs.Add(new MixinExpressionOutput(
            outputTarget, code, executionPool, executionPool.Get(injectionTarget)
          ));
          break;
        case DirectiveOpcode.Mixin:
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
        case DirectiveOpcode.ResolveMixin:
          if (context is not IMixinExpressionSignatureContext signatureContext)
            return Failure("the expression context does not support mixin resolution", lineNumber, logs);
          if (!TryResolveDirectiveArgument(
            parsed.Arguments[0], context, locals, pendingVariables, out var resolvedLocal,
            out var resolveLocalError
          )) return Failure(resolveLocalError, lineNumber, logs);
          if (string.IsNullOrEmpty(resolvedLocal))
            return Failure("RESOLVE_MIXIN local name is empty", lineNumber, logs);
          if (!TryInterpolate(
            parsed.ValueExpression, context, locals, pendingVariables, out var resolvedTarget,
            out var resolveTargetError
          )) return Failure(resolveTargetError, lineNumber, logs);
          if (string.IsNullOrWhiteSpace(resolvedTarget))
            return Failure("RESOLVE_MIXIN target is empty", lineNumber, logs);
          if (!signatureContext.TryResolveMixin(
            resolvedTarget, out var callable, out var resolveError
          )) return Failure(resolveError, lineNumber, logs);
          locals[resolvedLocal] = callable;
          break;
        case DirectiveOpcode.Using:
          if (!TryInterpolateSegments(
            parsed.ValueExpression, context, locals, pendingVariables, executionPool,
            out var usingDirective,
            out var usingError
          )) return Failure(usingError, lineNumber, logs);
          outputs.Add(new MixinExpressionOutput(
            MixinExpressionOutputTarget.Using, usingDirective, executionPool,
            MixinString.Dynamic(null)
          ));
          break;
        case DirectiveOpcode.Log:
          if (!TryInterpolate(
            parsed.ValueExpression, context, locals, pendingVariables, out var log,
            out var logError
          )) return Failure(logError, lineNumber, logs);
          logs.Add(new MixinExpressionLog(log, lineNumber));
          break;
        case DirectiveOpcode.Local:
        case DirectiveOpcode.Variable:
          if (string.IsNullOrEmpty(argument)) return Failure(command + " requires a name", lineNumber, logs);
          if (!TryEvaluateExpression(
            parsed.ValueExpression, context, locals, pendingVariables, out var stored, out var storeError
          )) return Failure(storeError, lineNumber, logs);
          if (stored is MixinTransformRequest storeTransform) {
            if (!BeginTransform(
              storeTransform,
              parsed.Opcode == DirectiveOpcode.Local
                ? FrameContinuation.StoreLocal
                : FrameContinuation.StoreVariable,
              argument, default, null, out var beginStoreError
            )) return Failure(beginStoreError, lineNumber, logs);
            break;
          }
          (parsed.Opcode == DirectiveOpcode.Local ? locals : pendingVariables)[argument] = stored;
          break;
        case DirectiveOpcode.Carry:
          if (string.IsNullOrEmpty(argument)) return Failure("CARRY requires a label", lineNumber, logs);
          if (TryEvaluateExpression(
            parsed.ValueExpression, context, locals, pendingVariables,
            out var carried, out var carryError
          )) pendingVariables[CarryLocalPrefix + argument] = MixinValue.Unlink(carried, context);
          else pendingVariables[CarryLocalPrefix + argument] = new FailedMixinValue(carryError);
          break;
        case DirectiveOpcode.PropStruct:
          if (context is not IMixinExpressionPropStructContext propStructContext)
            return Failure("the expression context does not support prop structs", lineNumber, logs);
          if (!TryResolveDirectiveArgument(
            parsed.Arguments[0], context, locals, pendingVariables, out var propStructName,
            out var propStructNameError
          )) return Failure(propStructNameError, lineNumber, logs);
          if (!TryResolveDirectiveArgument(
            parsed.Arguments[1], context, locals, pendingVariables, out var propStructLocal,
            out var propStructLocalError
          )) return Failure(propStructLocalError, lineNumber, logs);
          if (parsed.ValueExpression is not { Count: 1 } ||
            parsed.ValueExpression[0].Reference is null)
            return Failure("PROP_STRUCT syntax target must be a single reference", lineNumber, logs);
          var generatePropStructDatatype = false;
          var generatePropStructDeclaration = true;
          for (var flagIndex = 2; flagIndex < parsed.Arguments.Count; flagIndex++) {
            if (!TryResolveDirectiveArgument(
              parsed.Arguments[flagIndex], context, locals, pendingVariables, out var flag,
              out var flagError
            )) return Failure(flagError, lineNumber, logs);
            if (string.Equals(flag, "datatype", StringComparison.OrdinalIgnoreCase)) {
              if (generatePropStructDatatype)
                return Failure("PROP_STRUCT flag 'datatype' was specified more than once", lineNumber, logs);
              generatePropStructDatatype = true;
            } else if (string.Equals(flag, "noGenerate", StringComparison.OrdinalIgnoreCase)) {
              if (!generatePropStructDeclaration)
                return Failure("PROP_STRUCT flag 'noGenerate' was specified more than once", lineNumber, logs);
              generatePropStructDeclaration = false;
            } else return Failure("unknown PROP_STRUCT flag '" + flag + "'", lineNumber, logs);
          }
          object propStructHandle;
          string propStructDeclaration;
          string propStructError;
          if (propStructContext is IMixinExpressionConfigurablePropStructContext configurablePropStructContext) {
            if (!configurablePropStructContext.TryCreatePropStruct(
              propStructName, parsed.ValueExpression[0].Reference,
              generatePropStructDatatype && generatePropStructDeclaration,
              generatePropStructDeclaration,
              out propStructHandle, out propStructDeclaration, out propStructError
            )) return Failure(propStructError, lineNumber, logs);
          } else {
            if (generatePropStructDatatype || !generatePropStructDeclaration)
              return Failure("the expression context does not support configurable prop structs", lineNumber, logs);
            if (!propStructContext.TryCreatePropStruct(
              propStructName, parsed.ValueExpression[0].Reference,
              out propStructHandle, out propStructDeclaration, out propStructError
            )) return Failure(propStructError, lineNumber, logs);
          }
          locals[propStructLocal] = propStructHandle;
          if (generatePropStructDeclaration) {
            outputs.Add(
              new MixinExpressionOutput(
                MixinExpressionOutputTarget.Class, propStructDeclaration
              )
            );
          }
          break;
        case DirectiveOpcode.AugmentStruct:
          if (context is not IMixinExpressionStructAugmentationContext augmentStructContext)
            return Failure("the expression context does not support struct augmentation", lineNumber, logs);
          if (!TryResolveDirectiveArgument(
            parsed.Arguments[0], context, locals, pendingVariables, out var augmentStructLocal,
            out var augmentStructLocalError
          )) return Failure(augmentStructLocalError, lineNumber, logs);
          if (string.IsNullOrEmpty(augmentStructLocal))
            return Failure("AUGMENT_STRUCT local name is empty", lineNumber, logs);
          if (parsed.ValueExpression is not { Count: 1 } ||
            parsed.ValueExpression[0].Reference is null)
            return Failure("AUGMENT_STRUCT syntax target must be a single reference", lineNumber, logs);
          if (!augmentStructContext.TryAugmentPropStruct(
            parsed.ValueExpression[0].Reference,
            out var augmentStructHandle,
            out var augmentStructDeclaration,
            out var augmentStructError
          )) return Failure(augmentStructError, lineNumber, logs);
          locals[augmentStructLocal] = augmentStructHandle;
          outputs.Add(
            new MixinExpressionOutput(
              MixinExpressionOutputTarget.Class, augmentStructDeclaration
            )
          );
          break;
        case DirectiveOpcode.Put:
        case DirectiveOpcode.Push:
          if (!TryResolveDirectiveArgument(
            parsed.Arguments[0], context, locals, pendingVariables, out var tableLocal,
            out var tableLocalError
          )) return Failure(tableLocalError, lineNumber, logs);
          if (string.IsNullOrEmpty(tableLocal))
            return Failure(command + " local name is empty", lineNumber, logs);
          if (!TryEvaluateExpression(
            parsed.ValueExpression, context, locals, pendingVariables,
            out var tableItem, out var tableItemError
          )) return Failure(tableItemError, lineNumber, logs);
          var updatedTable = locals.TryGetValue(tableLocal, out var currentTable) &&
            currentTable is MixinExpressionTable existingTable
              ? existingTable
              : new MixinExpressionTable();
          string tableKey;
          if (parsed.Opcode == DirectiveOpcode.Put) {
            if (!TryResolveDirectiveArgument(
              parsed.Arguments[1], context, locals, pendingVariables, out tableKey,
              out var tableKeyError
            )) return Failure(tableKeyError, lineNumber, logs);
          } else tableKey = updatedTable.Count.ToString(CultureInfo.InvariantCulture);
          locals[tableLocal] = updatedTable.Put(tableKey, tableItem);
          break;
        case DirectiveOpcode.Return:
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
        case DirectiveOpcode.Call:
          var callFunctionLabel = parsed.Arguments.Count == 2 ? parsed.Arguments[1] : argument;
          var callReturnLocal = parsed.Arguments.Count == 2 ? argument : null;
          if (!functions.ContainsKey(callFunctionLabel ?? "") && !localSymbolsIndexed) {
            if (!TryIndexSymbols(
              lines, preparedCount, lines.Count, labels, instructionScopes,
              functions, functionStarts, functionEnds,
              out var symbolError, out var symbolLine
            )) return Failure(symbolError, symbolLine, logs);
            localSymbolsIndexed = true;
          }
          if (string.IsNullOrEmpty(callFunctionLabel) || !functions.TryGetValue(callFunctionLabel, out var function))
            return Failure("unknown function '" + (callFunctionLabel ?? "") + "'", lineNumber, logs);
          var hadParameter = locals.TryGetValue(ParameterLocalKey, out var previousParameter);
          object callParameter = null;
          if (!string.IsNullOrEmpty(operand) && !TryEvaluateExpression(
            parsed.ValueExpression, context, locals, pendingVariables,
            out callParameter, out var callParameterError
          )) return Failure(callParameterError, lineNumber, logs);
          calls.Push(new CallFrame {
            ReturnAddress = pc,
            HadParameter = hadParameter,
            Parameter = previousParameter,
            ReturnLocal = callReturnLocal,
            Continuation = FrameContinuation.Call
          });
          locals[ParameterLocalKey] = callParameter;
          pc = function.Start;
          break;
        case DirectiveOpcode.Inline:
          return Failure("INLINE must be expanded before evaluation", lineNumber, logs);
        case DirectiveOpcode.Goto:
          var gotoScope = instructionScopes.TryGetValue(instruction, out var indexedGotoScope)
            ? indexedGotoScope
            : -preparedCount - 1;
          var gotoKey = ScopeLabelKey(gotoScope, argument ?? "");
          if (!labels.ContainsKey(gotoKey) && !localSymbolsIndexed) {
            if (!TryIndexSymbols(
              lines, preparedCount, lines.Count, labels, instructionScopes,
              functions, functionStarts, functionEnds,
              out var symbolError, out var symbolLine
            )) return Failure(symbolError, symbolLine, logs);
            localSymbolsIndexed = true;
            gotoKey = ScopeLabelKey(instructionScopes[instruction], argument ?? "");
          }
          if (string.IsNullOrEmpty(argument) || !labels.TryGetValue(gotoKey, out var destination))
            return Failure("unknown scope label '" + (argument ?? "") + "'", lineNumber, logs);
          pc = destination + 1;
          break;
        case DirectiveOpcode.Skip:
          if (!instructionScopes.ContainsKey(instruction)) {
            if (!TryIndexSymbols(
              lines, preparedCount, lines.Count, labels, instructionScopes,
              functions, functionStarts, functionEnds,
              out var symbolError, out var symbolLine
            )) return Failure(symbolError, symbolLine, logs);
            localSymbolsIndexed = true;
          }
          var skip = FindNextScopeOrEnd(
            lines, pc, instructionScopes[instruction], instructionScopes,
            functionStarts, functionEnds
          );
          if (skip < 0) return Failure("SKIP has no following scope", lineNumber, logs);
          pc = skip;
          break;
        case DirectiveOpcode.Fail:
          if (string.IsNullOrEmpty(operand)) return Failure("expression requested failure", lineNumber, logs);
          if (!TryInterpolate(
            parsed.ValueExpression, context, locals, pendingVariables,
            out var failureMessage, out var failureError
          )) return Failure(failureError, lineNumber, logs);
          return Failure(failureMessage, lineNumber, logs);
        default:
          return Failure("unknown directive '@" + command + "'", lineNumber, logs);
      }
    }

    CommitVariables(variables, pendingVariables);
    return SuccessfulResult();
  }
}
