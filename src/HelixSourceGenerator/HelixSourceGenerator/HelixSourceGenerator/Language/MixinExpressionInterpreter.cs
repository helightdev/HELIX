using System;
using System.Collections;
using System.Collections.Generic;

namespace HELIX.SourceGen.Expressions;

/// <summary>Public facade for compiling and executing mixin programs.</summary>
public sealed class MixinExpressionInterpreter {
  internal const string ParameterLocalKey = "\0@param";
  internal const string CarryLocalPrefix = "\0@carry:";
  internal const float FloatTimeZeroTolerance = 1e-6f;
  private const int MaximumCachedPrograms = 512;
  private const int MaximumCachedCharacters = 1024 * 1024;
  private static readonly object ProgramCacheLock = new();
  private static readonly Dictionary<string, LinkedListNode<CachedProgram>> ProgramCache =
    new(StringComparer.Ordinal);
  private static readonly LinkedList<CachedProgram> ProgramCacheUsage = new();
  private static int _cachedCharacters;

  internal static MixinProgramSyntax GetProgram(string expression, bool eager) {
    MixinProgramSyntax program;
    if (expression.Length > MaximumCachedCharacters) program = new MixinProgramSyntax(expression);
    else {
      lock (ProgramCacheLock) {
        if (ProgramCache.TryGetValue(expression, out var cached)) {
          ProgramCacheUsage.Remove(cached);
          ProgramCacheUsage.AddFirst(cached);
          program = cached.Value.Program;
        } else {
          program = new MixinProgramSyntax(expression);
          var node = ProgramCacheUsage.AddFirst(new CachedProgram(expression, program));
          ProgramCache.Add(expression, node);
          _cachedCharacters += expression.Length;
          while (ProgramCache.Count > MaximumCachedPrograms ||
            _cachedCharacters > MaximumCachedCharacters) {
            var expired = ProgramCacheUsage.Last;
            ProgramCacheUsage.RemoveLast();
            ProgramCache.Remove(expired.Value.Expression);
            _cachedCharacters -= expired.Value.Expression.Length;
          }
        }
      }
    }
    if (eager) program.ParseAll();
    return program;
  }

  /// <summary>Fully parses and context-independently evaluates prepared global programs.</summary>
  public MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> expressions) {
    return MixinExpressionCompiler.PrepareGlobals(expressions);
  }

  public MixinExpressionValidationResult ValidateSyntax(string expression) {
    return MixinExpressionCompiler.ValidateSyntax(expression, false);
  }

  internal MixinExpressionValidationResult ValidateFunctionLibrary(string expression) {
    return MixinExpressionCompiler.ValidateSyntax(expression, true);
  }

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, object> variables = null
  ) {
    return Execute(expression, context, variables, (MixinExpressionPreparedState)null);
  }

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, object> variables,
    MixinExpressionPreparedState preparedState
  ) {
    return MixinExpressionVirtualMachine.Execute(expression, context, variables, preparedState);
  }

  internal MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, object> variables,
    MixinExpressionPreparedState preparedState,
    MixinExpressionPreludeSnapshot preludeSnapshot
  ) {
    return MixinExpressionVirtualMachine.Execute(
      expression, context, variables, preparedState, preludeSnapshot
    );
  }

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, object> variables,
    IEnumerable<string> preparedExpressions
  ) {
    return Execute(
      expression,
      context,
      variables,
      PrepareGlobals(preparedExpressions ?? Array.Empty<string>())
    );
  }

  public bool TryParseReference(
    string text,
    out MixinExpressionReference reference,
    out string error
  ) {
    reference = null;
    error = null;
    if (text is null) {
      error = "reference is null";
      return false;
    }
    var position = 0;
    if (MixinExpressionEvaluator.TryReadReferenceNode(
      text, ref position, out reference, out error
    ) && position == text.Length) return true;
    error ??= "unexpected text after expression reference";
    reference = null;
    return false;
  }

  internal static bool TryParseFloatTime(string value, out float seconds) {
    return MixinExpressionEvaluator.TryParseFloatTime(value, out seconds);
  }

  internal static bool TryConvertFloat(object value, out float result) {
    return MixinExpressionEvaluator.TryConvertFloat(value, out result);
  }

  internal static string FormatFloatTime(float seconds) {
    return MixinExpressionEvaluator.FormatFloatTime(seconds);
  }

  internal static void CommitVariables(
    IDictionary<string, object> destination,
    IReadOnlyDictionary<string, object> source
  ) {
    if (destination is null) return;
    destination.Clear();
    foreach (var item in source) destination[item.Key] = item.Value;
  }

  internal static MixinExpressionResult Success(
    IReadOnlyList<MixinExpressionOutput> outputs,
    IReadOnlyList<MixinExpressionLog> logs,
    IReadOnlyDictionary<string, object> variables = null
  ) {
    return new MixinExpressionResult(true, null, 0, outputs, logs, variables);
  }

  internal static MixinExpressionResult Failure(
    string error,
    int line,
    IReadOnlyList<MixinExpressionLog> logs = null
  ) {
    return new MixinExpressionResult(false, error, line, Array.Empty<MixinExpressionOutput>(), logs);
  }

  private static MixinExpressionValidationResult ValidationFailure(string error, int line) {
    return new MixinExpressionValidationResult(false, error, line);
  }

  private sealed record CachedProgram(string Expression, MixinProgramSyntax Program);

  internal sealed class InstructionSequence : IReadOnlyList<DirectiveInstruction> {
    private readonly IReadOnlyList<DirectiveInstruction> _prefix;
    private readonly MixinProgramSyntax _tail;

    internal InstructionSequence(IReadOnlyList<DirectiveInstruction> prefix, MixinProgramSyntax tail) {
      _prefix = prefix;
      _tail = tail;
    }

    public int Count => _prefix.Count + _tail.Count;
    public DirectiveInstruction this[int index] => index < _prefix.Count
      ? _prefix[index]
      : _tail.Get(index - _prefix.Count);

    public IEnumerator<DirectiveInstruction> GetEnumerator() {
      for (var index = 0; index < Count; index++) yield return this[index];
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }

  internal enum FrameContinuation { Call, StoreLocal, StoreVariable, EmitCode, Return }

  internal sealed class CallFrame {
    internal int ReturnAddress;
    internal bool HadParameter;
    internal object Parameter;
    internal string ReturnLocal;
    internal FrameContinuation Continuation;
    internal MixinTransformRequest Transform;
    internal KeyValuePair<string, IMixinValue>[] Inputs;
    internal int InputIndex;
    internal MixinExpressionTable Accumulator;
    internal int FunctionStart;
    internal string Destination;
    internal MixinExpressionOutputTarget OutputTarget;
    internal string InjectionTarget;
  }
}
