using System;
using System.Collections;
using System.Collections.Generic;

namespace HelixSourceGenerator.Language;

/// <summary>Public facade for compiling and executing mixin programs.</summary>
public sealed class MixinExpressionInterpreter {
  internal const string ParameterLocalKey = "\0@param";
  internal const string CarryLocalPrefix = "\0@carry:";
  internal const float FloatTimeZeroTolerance = 1e-6f;
  private const int _maximumCachedPrograms = 512;
  private const int _maximumCachedCharacters = 1024 * 1024;
  private static readonly object _programCacheLock = new();
  private static readonly Dictionary<string, LinkedListNode<CachedProgram>> _programCache =
    new(StringComparer.Ordinal);
  private static readonly LinkedList<CachedProgram> _programCacheUsage = new();
  private static int _cachedCharacters;

  internal static MixinProgramSyntax GetProgram(string expression, bool eager) {
    MixinProgramSyntax program;
    if (expression.Length > _maximumCachedCharacters) program = new MixinProgramSyntax(expression);
    else {
      lock (_programCacheLock) {
        if (_programCache.TryGetValue(expression, out var cached)) {
          _programCacheUsage.Remove(cached);
          _programCacheUsage.AddFirst(cached);
          program = cached.Value.Program;
        } else {
          program = new MixinProgramSyntax(expression);
          var node = _programCacheUsage.AddFirst(new CachedProgram(expression, program));
          _programCache.Add(expression, node);
          _cachedCharacters += expression.Length;
          while (_programCache.Count > _maximumCachedPrograms ||
            _cachedCharacters > _maximumCachedCharacters) {
            var expired = _programCacheUsage.Last;
            _programCacheUsage.RemoveLast();
            _programCache.Remove(expired.Value.Expression);
            _cachedCharacters -= expired.Value.Expression.Length;
          }
        }
      }
    }
    if (eager) program.ParseAll();
    return program;
  }

  /// <summary>Fully parses and context-independently evaluates prepared global programs.</summary>
  public MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> expressions) =>
    MixinExpressionCompiler.PrepareGlobals(expressions);

  public MixinExpressionValidationResult ValidateSyntax(string expression) =>
    MixinExpressionCompiler.ValidateSyntax(expression, false);

  internal MixinExpressionValidationResult ValidateFunctionLibrary(string expression) =>
    MixinExpressionCompiler.ValidateSyntax(expression, true);

  public MixinExpressionResult Execute(
    string expression, IMixinExpressionContext context, IDictionary<string, object> variables = null
  ) {
    return Execute(expression, context, variables, (MixinExpressionPreparedState)null);
  }

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context, IDictionary<string, object> variables, MixinExpressionPreparedState preparedState
  ) => MixinExpressionVirtualMachine.Execute(expression, context, variables, preparedState);

  internal MixinExpressionResult ExecuteCompiled(
    MixinProgramSyntax program,
    IMixinExpressionContext context, IDictionary<string, object> variables, MixinExpressionPreparedState preparedState
  ) => MixinExpressionVirtualMachine.Execute(program, context, variables, preparedState);

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context, IDictionary<string, object> variables, IEnumerable<string> preparedExpressions
  ) => Execute(expression, context, variables, PrepareGlobals(preparedExpressions ?? []));

  public static bool TryParseReference(string text, out MixinExpressionReference reference, out string error) {
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
    IReadOnlyDictionary<string, object> variables = null,
    int executedOperations = 0,
    double executionMilliseconds = 0
  ) {
    return new MixinExpressionResult(
      true, null, 0, outputs, logs, variables, executedOperations, executionMilliseconds
    );
  }

  internal static MixinExpressionResult Failure(
    string error,
    int line,
    IReadOnlyList<MixinExpressionLog> logs = null
  ) {
    return new MixinExpressionResult(false, error, line, [], logs);
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
    internal MixinExpressionTable accumulator;
    internal FrameContinuation continuation;
    internal string destination;
    internal int functionStart;
    internal bool hadParameter;
    internal string injectionTarget;
    internal int inputIndex;
    internal KeyValuePair<string, IMixinValue>[] inputs;
    internal MixinExpressionOutputTarget outputTarget;
    internal object parameter;
    internal int returnAddress;
    internal string returnLocal;
    internal MixinTransformRequest transform;
  }
}