using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HELIX.SourceGen.Expressions;

public enum MixinExpressionOutputTarget { Target, Class, File, Implements, Injection, Annotation, Using }

public sealed class MixinExpressionOutput {
  public MixinExpressionOutput(
    MixinExpressionOutputTarget target,
    string text,
    string injectionTarget = null
  ) {
    Target = target;
    Text = text ?? "";
    InjectionTarget = injectionTarget;
  }

  public MixinExpressionOutputTarget Target { get; }
  public string Text { get; }
  public string InjectionTarget { get; }
}

public sealed class MixinExpressionLog {
  internal MixinExpressionLog(string text, int line) {
    Text = text ?? "";
    Line = line;
  }

  public string Text { get; }
  public int Line { get; }
}

public sealed class MixinExpressionPreparedLog {
  internal MixinExpressionPreparedLog(string text, int line, int programIndex) {
    Text = text ?? "";
    Line = line;
    ProgramIndex = programIndex;
  }

  public string Text { get; }
  public int Line { get; }
  public int ProgramIndex { get; }
}

public sealed class MixinExpressionProperty {
  public MixinExpressionProperty(string name, string argument = null, bool negated = false) {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Argument = argument;
    Negated = negated;
  }

  public string Name { get; }
  public string Argument { get; }
  public bool Negated { get; }
}

public sealed class MixinExpressionReference {
  public MixinExpressionReference(
    string root,
    string member,
    IReadOnlyList<MixinExpressionProperty> properties
  ) {
    Root = root ?? throw new ArgumentNullException(nameof(root));
    Member = member;
    Properties = properties ?? Array.Empty<MixinExpressionProperty>();
  }

  public string Root { get; }
  public string Member { get; }
  public IReadOnlyList<MixinExpressionProperty> Properties { get; }
}

/// <summary>Resolves host-specific values and predicates used by a mixin expression.</summary>
public interface IMixinExpressionContext {
  bool TryResolve(
    MixinExpressionReference reference,
    out string value,
    out string error
  );

  bool TryEvaluate(
    MixinExpressionReference reference,
    out bool value,
    out string error
  );
}

public sealed class MixinExpressionResult {
  internal MixinExpressionResult(
    bool success,
    string error,
    int errorLine,
    IReadOnlyList<MixinExpressionOutput> outputs,
    IReadOnlyList<MixinExpressionLog> logs = null
  ) {
    Success = success;
    Error = error;
    ErrorLine = errorLine;
    Outputs = outputs ?? Array.Empty<MixinExpressionOutput>();
    Logs = logs ?? Array.Empty<MixinExpressionLog>();
  }

  public bool Success { get; }
  public string Error { get; }
  public int ErrorLine { get; }
  public IReadOnlyList<MixinExpressionOutput> Outputs { get; }
  public IReadOnlyList<MixinExpressionLog> Logs { get; }
}

public sealed class MixinExpressionValidationResult {
  internal MixinExpressionValidationResult(bool success, string error, int errorLine) {
    Success = success;
    Error = error;
    ErrorLine = errorLine;
  }

  public bool Success { get; }
  public string Error { get; }
  public int ErrorLine { get; }
}

/// <summary>
/// Immutable, context-free result of compiling and evaluating prepared mixins.  The generator
/// may safely retain this object in an incremental value and share it between target runs.
/// </summary>
public sealed class MixinExpressionPreparedState {
  internal MixinExpressionPreparedState(
    IReadOnlyList<MixinExpressionInterpreter.Program> programs,
    IReadOnlyDictionary<string, string> variables,
    IReadOnlyList<MixinExpressionInterpreter.Instruction> instructions,
    IReadOnlyDictionary<string, int> labels,
    IReadOnlyDictionary<int, int> instructionScopes,
    IReadOnlyDictionary<string, MixinExpressionInterpreter.FunctionDefinition> functions,
    IReadOnlyDictionary<int, int> functionStarts,
    ISet<int> functionEnds,
    ISet<int> initializers,
    IReadOnlyList<MixinExpressionPreparedLog> logs,
    int executedOperations
  ) {
    Programs = programs;
    Variables = variables;
    Instructions = instructions;
    Labels = labels;
    InstructionScopes = instructionScopes;
    Functions = functions;
    FunctionStarts = functionStarts;
    FunctionEnds = functionEnds;
    Initializers = initializers;
    Logs = logs;
    ExecutedOperations = executedOperations;
  }

  internal IReadOnlyList<MixinExpressionInterpreter.Program> Programs { get; }
  internal IReadOnlyDictionary<string, string> Variables { get; }
  internal IReadOnlyList<MixinExpressionInterpreter.Instruction> Instructions { get; }
  internal IReadOnlyDictionary<string, int> Labels { get; }
  internal IReadOnlyDictionary<int, int> InstructionScopes { get; }
  internal IReadOnlyDictionary<string, MixinExpressionInterpreter.FunctionDefinition> Functions { get; }
  internal IReadOnlyDictionary<int, int> FunctionStarts { get; }
  internal ISet<int> FunctionEnds { get; }
  internal ISet<int> Initializers { get; }
  public IReadOnlyList<MixinExpressionPreparedLog> Logs { get; }
  public int ExecutedOperations { get; }
}

/// <summary>
/// Parses and executes the line-oriented mixin expression language. The interpreter is
/// independent of Roslyn; callers provide symbol/value semantics through
/// <see cref="IMixinExpressionContext"/>.
/// </summary>
public sealed class MixinExpressionInterpreter {
  internal sealed class Program {
    private readonly string[] _lines;
    private readonly Instruction[] _instructions;

    internal Program(string expression, bool eager) {
      _lines = SplitLines(expression ?? "");
      _instructions = new Instruction[_lines.Length];
      if (eager)
        for (var i = 0; i < _lines.Length; i++)
          Get(i);
    }

    internal int Count => _lines.Length;

    internal Instruction Get(int index) {
      return _instructions[index] ?? (_instructions[index] = Instruction.Parse(_lines[index], index + 1));
    }

    internal IEnumerable<Instruction> AvailableInstructions() {
      return _instructions.Where(instruction => instruction is not null);
    }
  }

  private sealed class InstructionSequence : IReadOnlyList<Instruction> {
    private readonly IReadOnlyList<Instruction> _prefix;
    private readonly Program _tail;

    internal InstructionSequence(IReadOnlyList<Instruction> prefix, Program tail) {
      _prefix = prefix;
      _tail = tail;
    }

    public int Count => _prefix.Count + _tail.Count;
    public Instruction this[int index] => index < _prefix.Count
      ? _prefix[index]
      : _tail.Get(index - _prefix.Count);

    public IEnumerator<Instruction> GetEnumerator() {
      for (var index = 0; index < Count; index++) yield return this[index];
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }

  internal sealed class Instruction {
    private Instruction(
      int line, string command, string argument, string operand, string error
    ) {
      Line = line;
      Command = command;
      Argument = argument;
      Operand = operand;
      Error = error;
      if (error is null && command is "MATCH" or "ASSERT") BooleanExpression = ParseBooleanExpression(operand);
      if (error is null && command is "CODE" or "USING" or "LOG" or "LOCAL" or "VAR" or "FAIL")
        StringExpression = ParseStringExpression(operand);
    }

    internal int Line { get; }
    internal string Command { get; }
    internal string Argument { get; }
    internal string Operand { get; }
    internal string Error { get; }
    internal IReadOnlyList<MixinExpressionReference> BooleanExpression { get; }
    internal IReadOnlyList<StringPart> StringExpression { get; }

    internal static Instruction Parse(string text, int line) {
      if (string.IsNullOrWhiteSpace(text)) return new Instruction(line, "", null, "", null);
      if (!TryDirective(text, out var command, out var argument, out var operand))
        return new Instruction(line, null, null, null, "expected an expression directive");
      if (!IsKnownDirective(command))
        return new Instruction(line, command, argument, operand, "unknown directive '@" + command + "'");
      return ValidateDirectiveSyntax(command, argument, operand, out var error)
        ? new Instruction(line, command, argument, operand, null)
        : new Instruction(line, command, argument, operand, error);
    }
  }

  internal sealed class StringPart {
    internal StringPart(string literal, MixinExpressionReference reference) {
      Literal = literal;
      Reference = reference;
    }

    internal string Literal { get; }
    internal MixinExpressionReference Reference { get; }
  }

  private static IReadOnlyList<MixinExpressionReference> ParseBooleanExpression(string text) {
    var result = new List<MixinExpressionReference>();
    var position = 0;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      TryReadReference(text, ref position, out var reference, out _);
      result.Add(reference);
    }
    return result.AsReadOnly();
  }

  private static IReadOnlyList<StringPart> ParseStringExpression(string text) {
    var result = new List<StringPart>();
    var literal = new StringBuilder();
    var position = 0;
    while (position < text.Length) {
      if (text[position] != '@') {
        literal.Append(text[position++]);
        continue;
      }
      if (position + 1 < text.Length && text[position + 1] == '@') {
        literal.Append('@');
        position += 2;
        continue;
      }
      if (literal.Length != 0) {
        result.Add(new StringPart(literal.ToString(), null));
        literal.Clear();
      }
      TryReadReference(text, ref position, out var reference, out _);
      result.Add(new StringPart(null, reference));
    }
    if (literal.Length != 0 || result.Count == 0) result.Add(new StringPart(literal.ToString(), null));
    return result.AsReadOnly();
  }

  /// <summary>Fully parses and context-independently evaluates prepared global programs.</summary>
  public MixinExpressionPreparedState PrepareGlobals(IEnumerable<string> expressions) {
    var programs = new List<Program>();
    var variables = new Dictionary<string, string>(StringComparer.Ordinal);
    var logs = new List<MixinExpressionPreparedLog>();
    var programIndex = 0;
    var executedOperations = 0;
    foreach (var expression in expressions ?? Array.Empty<string>()) {
      var validation = ValidateSyntax(expression);
      if (!validation.Success) {
        throw new ArgumentException(
          "invalid prepared expression at line " + validation.ErrorLine + ": " + validation.Error,
          nameof(expressions)
        );
      }
      var program = new Program(expression, true);
      programs.Add(program);
      if (!TryEvaluatePreparedInitializers(
        program, programIndex, variables, logs, ref executedOperations,
        out var error, out var line
      )) {
        throw new ArgumentException(
          "invalid prepared expression at line " + line + ": " + error, nameof(expressions)
        );
      }
      programIndex++;
    }
    var instructions = programs.SelectMany(program =>
      Enumerable.Range(0, program.Count).Select(program.Get)
    ).ToArray();
    var labels = new Dictionary<string, int>(StringComparer.Ordinal);
    var instructionScopes = new Dictionary<int, int>();
    var functions = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal);
    var functionStarts = new Dictionary<int, int>();
    var functionEnds = new HashSet<int>();
    var instructionOffset = 0;
    foreach (var program in programs) {
      if (!TryIndexSymbols(
        instructions, instructionOffset, instructionOffset + program.Count,
        labels, instructionScopes, functions, functionStarts, functionEnds,
        out var symbolError, out var symbolLine
      )) {
        throw new ArgumentException(
          "invalid prepared expression at line " + symbolLine + ": " + symbolError,
          nameof(expressions)
        );
      }
      instructionOffset += program.Count;
    }
    var initializers = FindPreparedInitializers(instructions);
    CollectPreparedDumps(programs, instructions, variables, logs, executedOperations);
    return new MixinExpressionPreparedState(
      programs.AsReadOnly(),
      new Dictionary<string, string>(variables, StringComparer.Ordinal),
      instructions,
      new Dictionary<string, int>(labels, StringComparer.Ordinal),
      new Dictionary<int, int>(instructionScopes),
      new Dictionary<string, FunctionDefinition>(functions, StringComparer.Ordinal),
      new Dictionary<int, int>(functionStarts),
      new HashSet<int>(functionEnds),
      initializers,
      logs.AsReadOnly(),
      executedOperations
    );
  }

  private static ISet<int> FindPreparedInitializers(IReadOnlyList<Instruction> instructions) {
    var result = new HashSet<int>();
    var depth = 0;
    var scope = false;
    for (var index = 0; index < instructions.Count; index++) {
      var instruction = instructions[index];
      if (instruction.Command == "FUNC") {
        depth++;
        scope = false;
        continue;
      }
      if (depth != 0) {
        if (instruction.Command == "SCOPE") scope = true;
        else if (instruction.Command == "END") {
          if (scope) scope = false;
          else depth--;
        }
        continue;
      }
      if (instruction.Command is "VAR" or "LOG" or "DUMP") result.Add(index);
    }
    return result;
  }

  private static bool TryEvaluatePreparedInitializers(
    Program program,
    int programIndex,
    IDictionary<string, string> variables,
    ICollection<MixinExpressionPreparedLog> logs,
    ref int executedOperations,
    out string error,
    out int line
  ) {
    error = null;
    line = 0;
    var functionDepth = 0;
    var functionScope = false;
    for (var i = 0; i < program.Count; i++) {
      var instruction = program.Get(i);
      if (instruction.Error is not null) {
        error = instruction.Error;
        line = instruction.Line;
        return false;
      }
      if (instruction.Command == "FUNC") {
        functionDepth++;
        functionScope = false;
        continue;
      }
      if (functionDepth != 0) {
        if (instruction.Command == "SCOPE") functionScope = true;
        else if (instruction.Command == "END") {
          if (functionScope) functionScope = false;
          else functionDepth--;
        }
        continue;
      }
      if (instruction.Command == "VAR") {
        executedOperations++;
        if (!TryInterpolatePrepared(instruction.StringExpression, variables, out var value, out error)) {
          line = instruction.Line;
          return false;
        }
        variables[instruction.Argument] = value;
      } else if (instruction.Command == "LOG") {
        executedOperations++;
        if (!TryInterpolatePrepared(instruction.StringExpression, variables, out var value, out error)) {
          line = instruction.Line;
          return false;
        }
        logs.Add(new MixinExpressionPreparedLog(value, instruction.Line, programIndex));
      } else if (instruction.Command == "DUMP") executedOperations++;
    }
    return true;
  }

  private static bool TryInterpolatePrepared(
    IReadOnlyList<StringPart> expression,
    IDictionary<string, string> variables,
    out string result,
    out string error
  ) {
    var builder = new StringBuilder();
    error = null;
    foreach (var part in expression) {
      if (part.Reference is null) {
        builder.Append(part.Literal);
        continue;
      }
      var reference = part.Reference;
      if (reference.Root != "var" || string.IsNullOrEmpty(reference.Member) ||
        !variables.TryGetValue(reference.Member, out var value)) {
        result = null;
        error = "prepared global initializers may only reference an existing @var value";
        return false;
      }
      if (reference.Properties.Count != 0) {
        result = null;
        error = "prepared global initializer references cannot have properties";
        return false;
      }
      builder.Append(value);
    }
    result = builder.ToString();
    return true;
  }

  private static void CollectPreparedDumps(
    IReadOnlyList<Program> programs,
    IReadOnlyList<Instruction> globalInstructions,
    IReadOnlyDictionary<string, string> variables,
    ICollection<MixinExpressionPreparedLog> logs,
    int executedOperations
  ) {
    for (var programIndex = 0; programIndex < programs.Count; programIndex++) {
      var program = programs[programIndex];
      var functionDepth = 0;
      var functionScope = false;
      for (var index = 0; index < program.Count; index++) {
        var instruction = program.Get(index);
        if (instruction.Command == "FUNC") {
          functionDepth++;
          functionScope = false;
          continue;
        }
        if (functionDepth != 0) {
          if (instruction.Command == "SCOPE") functionScope = true;
          else if (instruction.Command == "END") {
            if (functionScope) functionScope = false;
            else functionDepth--;
          }
          continue;
        }
        if (instruction.Command != "DUMP") continue;
        string text;
        switch ((instruction.Argument ?? "").ToUpperInvariant()) {
          case "STATE":
            text = DumpState(
              instruction.Line, index + 1, 0,
              new Dictionary<string, string>(), variables,
              executedOperations, executedOperations
            );
            break;
          case "BUFFER":
            text = DumpBuffer(Array.Empty<MixinExpressionOutput>());
            break;
          case "AST":
            text = DumpAst(globalInstructions, null);
            break;
          default:
            continue;
        }
        logs.Add(new MixinExpressionPreparedLog(text, instruction.Line, programIndex));
      }
    }
  }

  public MixinExpressionValidationResult ValidateSyntax(string expression) {
    if (expression is null) return ValidationFailure("the expression is null", 0);
    var lines = SplitLines(expression);
    string activeFunction = null;
    var functionLine = 0;
    var functionScopeOpen = false;
    var functions = new HashSet<string>(StringComparer.Ordinal);
    for (var index = 0; index < lines.Length; index++) {
      var line = lines[index];
      if (string.IsNullOrWhiteSpace(line)) continue;
      if (!TryDirective(line, out var command, out var argument, out var operand))
        return ValidationFailure("expected an expression directive", index + 1);
      if (!IsKnownDirective(command)) return ValidationFailure("unknown directive '@" + command + "'", index + 1);
      if (!ValidateDirectiveSyntax(command, argument, operand, out var error))
        return ValidationFailure(error, index + 1);
      if (activeFunction is null) {
        if (command != "FUNC") continue;
        if (!functions.Add(argument)) return ValidationFailure("duplicate function '" + argument + "'", index + 1);
        activeFunction = argument;
        functionLine = index + 1;
        functionScopeOpen = false;
        continue;
      }
      if (command == "FUNC") return ValidationFailure("functions may not be nested", index + 1);
      if (command == "SCOPE") functionScopeOpen = true;
      if (command != "END") continue;
      if (functionScopeOpen) {
        functionScopeOpen = false;
        continue;
      }
      activeFunction = null;
    }
    return activeFunction is null
      ? new MixinExpressionValidationResult(true, null, 0)
      : ValidationFailure("unterminated function '" + activeFunction + "'", functionLine);
  }

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, string> variables = null
  ) {
    return Execute(expression, context, variables, (MixinExpressionPreparedState)null);
  }

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, string> variables,
    IEnumerable<string> preparedExpressions
  ) {
    return Execute(
      expression,
      context,
      variables,
      PrepareGlobals(preparedExpressions ?? Array.Empty<string>())
    );
  }

  public MixinExpressionResult Execute(
    string expression,
    IMixinExpressionContext context,
    IDictionary<string, string> variables,
    MixinExpressionPreparedState preparedState
  ) {
    if (context is null) throw new ArgumentNullException(nameof(context));
    if (expression is null) return Failure("the expression is null", 0);

    var preparedLines = preparedState?.Instructions ?? Array.Empty<Instruction>();
    var preparedInitializers = preparedState?.Initializers ?? new HashSet<int>();
    // Attribute expressions are deliberately lazy: their instruction AST nodes are created
    // only when this execution's control-flow scan or program counter reaches the line.
    var localProgram = new Program(expression, false);
    var preparedCount = preparedLines.Count;
    IReadOnlyList<Instruction> lines = new InstructionSequence(preparedLines, localProgram);
    var labels = preparedState is null
      ? new Dictionary<string, int>(StringComparer.Ordinal)
      : preparedState.Labels.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    var instructionScopes = preparedState is null
      ? new Dictionary<int, int>()
      : preparedState.InstructionScopes.ToDictionary(item => item.Key, item => item.Value);
    var functions = preparedState is null
      ? new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal)
      : preparedState.Functions.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    var functionStarts = preparedState is null
      ? new Dictionary<int, int>()
      : preparedState.FunctionStarts.ToDictionary(item => item.Key, item => item.Value);
    var functionEnds = preparedState is null
      ? new HashSet<int>()
      : new HashSet<int>(preparedState.FunctionEnds);
    var locals = new Dictionary<string, string>(StringComparer.Ordinal);
    var pendingVariables = new Dictionary<string, string>(StringComparer.Ordinal);
    if (preparedState is not null)
      foreach (var item in preparedState.Variables)
        pendingVariables[item.Key] = item.Value;
    if (variables is not null)
      foreach (var item in variables)
        pendingVariables[item.Key] = item.Value;
    var outputs = new List<MixinExpressionOutput>();
    var logs = new List<MixinExpressionLog>();
    var pc = 0;
    var steps = 0;
    var executedOperations = 0;
    var maximumSteps = Math.Max(1024, lines.Count * 64);
    var calls = new Stack<int>();
    var localSymbolsIndexed = false;
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

      switch (command) {
        case "SCOPE":
          break;
        case "FUNC":
          break;
        case "END":
          if (functionEnds.Contains(instruction) && calls.Count != 0) pc = calls.Pop();
          break;
        case "MATCH":
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
        case "ASSERT":
          if (!TryEvaluateAll(
            parsed.BooleanExpression, context, locals, pendingVariables,
            out var asserted, out var assertError, out var assertFailure
          )) return Failure(assertError, lineNumber, logs);
          if (!asserted) return Failure(assertFailure, lineNumber, logs);
          break;
        case "CODE":
          if (!TryInterpolate(
            parsed.StringExpression, context, locals, pendingVariables, out var code, out var codeError
          )) return Failure(codeError, lineNumber, logs);
          TryOutputTarget(argument, out var outputTarget, out var injectionTarget);
          outputs.Add(new MixinExpressionOutput(outputTarget, code, injectionTarget));
          break;
        case "USING":
          if (!TryInterpolate(
            parsed.StringExpression, context, locals, pendingVariables, out var usingDirective,
            out var usingError
          )) return Failure(usingError, lineNumber, logs);
          outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Using, usingDirective));
          break;
        case "LOG":
          if (!TryInterpolate(
            parsed.StringExpression, context, locals, pendingVariables, out var log,
            out var logError
          )) return Failure(logError, lineNumber, logs);
          logs.Add(new MixinExpressionLog(log, lineNumber));
          break;
        case "DUMP":
          switch ((argument ?? "").ToUpperInvariant()) {
            case "STATE":
              logs.Add(
                new MixinExpressionLog(
                  DumpState(
                    lineNumber, pc, calls.Count, locals, pendingVariables,
                    executedOperations, preparedState?.ExecutedOperations ?? 0
                  ), lineNumber
                )
              );
              break;
            case "BUFFER":
              logs.Add(new MixinExpressionLog(DumpBuffer(outputs), lineNumber));
              break;
            case "AST":
              logs.Add(
                new MixinExpressionLog(
                  DumpAst(preparedState?.Instructions, localProgram.AvailableInstructions()), lineNumber
                )
              );
              break;
            default:
              return Failure("DUMP requires STATE, BUFFER or AST", lineNumber, logs);
          }
          break;
        case "LOCAL":
        case "VAR":
          if (string.IsNullOrEmpty(argument)) return Failure(command + " requires a name", lineNumber, logs);
          if (!TryInterpolate(
            parsed.StringExpression, context, locals, pendingVariables, out var stored, out var storeError
          )) return Failure(storeError, lineNumber, logs);
          (command == "LOCAL" ? locals : pendingVariables)[argument] = stored;
          break;
        case "RETURN":
          if (calls.Count != 0) {
            pc = calls.Pop();
            break;
          }
          CommitVariables(variables, pendingVariables);
          return Success(outputs, logs);
        case "CALL":
          if (!functions.ContainsKey(argument ?? "") && !localSymbolsIndexed) {
            if (!TryIndexSymbols(
              lines, preparedCount, lines.Count, labels, instructionScopes,
              functions, functionStarts, functionEnds,
              out var symbolError, out var symbolLine
            )) return Failure(symbolError, symbolLine, logs);
            localSymbolsIndexed = true;
          }
          if (string.IsNullOrEmpty(argument) || !functions.TryGetValue(argument, out var function))
            return Failure("unknown function '" + (argument ?? "") + "'", lineNumber, logs);
          calls.Push(pc);
          pc = function.Start;
          break;
        case "GOTO":
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
        case "SKIP":
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
        case "FAIL":
          if (string.IsNullOrEmpty(operand)) return Failure("expression requested failure", lineNumber, logs);
          if (!TryInterpolate(
            parsed.StringExpression, context, locals, pendingVariables,
            out var failureMessage, out var failureError
          )) return Failure(failureError, lineNumber, logs);
          return Failure(failureMessage, lineNumber, logs);
        default:
          return Failure("unknown directive '@" + command + "'", lineNumber, logs);
      }
    }

    CommitVariables(variables, pendingVariables);
    return Success(outputs, logs);
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
    if (!TryReadReference(text, ref position, out reference, out error) || position != text.Length) {
      if (error is null) error = "unexpected text after expression reference";
      reference = null;
      return false;
    }
    return true;
  }

  private static string[] SplitLines(string expression) {
    return expression.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
  }

  private static bool IsKnownDirective(string command) {
    return command is
      "SCOPE" or "FUNC" or "CALL" or "END" or "MATCH" or "ASSERT" or "CODE" or
      "USING" or "LOG" or "DUMP" or "LOCAL" or "VAR" or "RETURN" or "GOTO" or
      "SKIP" or "FAIL";
  }

  private static bool ValidateDirectiveSyntax(
    string command,
    string argument,
    string operand,
    out string error
  ) {
    error = null;
    if (command is "FUNC" or "CALL" or "GOTO" or "LOCAL" or "VAR" &&
      string.IsNullOrEmpty(argument)) {
      error = command + " requires a name";
      return false;
    }
    if (command is "MATCH" or "ASSERT") return ValidateBooleanSyntax(operand, out error);
    if (command is "CODE" or "USING" or "LOG" or "LOCAL" or "VAR" or "FAIL")
      return ValidateStringSyntax(operand, out error);
    if (command == "DUMP" && !string.Equals(argument, "STATE", StringComparison.OrdinalIgnoreCase) &&
      !string.Equals(argument, "BUFFER", StringComparison.OrdinalIgnoreCase) &&
      !string.Equals(argument, "AST", StringComparison.OrdinalIgnoreCase)) {
      error = "DUMP requires STATE, BUFFER or AST";
      return false;
    }
    return true;
  }

  private static string DumpState(
    int line,
    int programCounter,
    int callDepth,
    IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> variables,
    int executedOperations,
    int preparedOperations
  ) {
    return "STATE line=" + line + " pc=" + programCounter + " callDepth=" + callDepth +
      " operations=" + executedOperations + " preparedOperations=" + preparedOperations +
      " locals=" + DumpValues(locals) + " variables=" + DumpValues(variables);
  }

  private static string DumpValues(IReadOnlyDictionary<string, string> values) {
    return values.Count == 0
      ? "{}"
      : "{" + string.Join(
        ", ", values.OrderBy(item => item.Key, StringComparer.Ordinal)
          .Select(item => item.Key + "=" + item.Value)
      ) + "}";
  }

  private static string DumpBuffer(IReadOnlyList<MixinExpressionOutput> outputs) {
    return outputs.Count == 0
      ? "BUFFER <empty>"
      : "BUFFER " + string.Join(
        "\n", outputs.Select(item =>
          item.Target + (string.IsNullOrEmpty(item.InjectionTarget)
            ? ""
            : "<" + item.InjectionTarget + ">") + ": " + item.Text
        )
      );
  }

  private static string DumpAst(
    IEnumerable<Instruction> global,
    IEnumerable<Instruction> local
  ) {
    var builder = new StringBuilder("AST GLOBAL [");
    AppendAst(builder, global);
    builder.Append("] | AST LOCAL [");
    AppendAst(builder, local);
    builder.Append(']');
    return builder.ToString();
  }

  private static void AppendAst(StringBuilder builder, IEnumerable<Instruction> instructions) {
    if (instructions is null) {
      builder.Append("<unavailable>");
      return;
    }
    var found = false;
    foreach (var instruction in instructions) {
      if (found) builder.Append("; ");
      found = true;
      builder.Append(instruction.Line).Append(": @")
        .Append(instruction.Command);
      if (instruction.Argument is not null) builder.Append('<').Append(instruction.Argument).Append('>');
      if (!string.IsNullOrEmpty(instruction.Operand)) builder.Append(' ').Append(instruction.Operand);
      if (instruction.Error is not null) builder.Append(" [invalid: ").Append(instruction.Error).Append(']');
    }
    if (!found) builder.Append("<empty>");
  }

  private static bool ValidateBooleanSyntax(string text, out string error) {
    error = null;
    var position = 0;
    var found = false;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      found = true;
      if (!TryReadReference(text, ref position, out _, out error)) return false;
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  private static bool ValidateStringSyntax(string text, out string error) {
    error = null;
    var position = 0;
    while (position < text.Length) {
      if (text[position] != '@') {
        position++;
        continue;
      }
      if (position + 1 < text.Length && text[position + 1] == '@') {
        position += 2;
        continue;
      }
      if (!TryReadReference(text, ref position, out _, out error)) return false;
    }
    return true;
  }

  private static void AddScopeLabel(
    string command,
    string argument,
    int index,
    int scope,
    IDictionary<string, int> labels,
    out string error
  ) {
    error = null;
    if (command != "SCOPE" || string.IsNullOrEmpty(argument)) return;
    var key = ScopeLabelKey(scope, argument);
    if (labels.ContainsKey(key)) {
      error = "duplicate scope label '" + argument + "'";
      return;
    }
    labels.Add(key, index);
  }

  private static string ScopeLabelKey(int scope, string label) {
    return scope + "\0" + label;
  }

  private static int FindNextScopeOrEnd(
    IReadOnlyList<Instruction> lines,
    int start,
    int scope,
    IReadOnlyDictionary<int, int> instructionScopes,
    IReadOnlyDictionary<int, int> functionStarts,
    ISet<int> functionEnds
  ) {
    for (var index = start; index < lines.Count; index++) {
      if (!instructionScopes.TryGetValue(index, out var candidateScope) || candidateScope != scope) return -1;
      if (functionStarts.TryGetValue(index, out var functionEnd)) {
        index = functionEnd;
        continue;
      }
      var command = lines[index].Command;
      if (string.IsNullOrEmpty(command)) continue;
      if (command == "SCOPE") return index;
      if (command == "END") return functionEnds.Contains(index) ? index : index + 1;
    }
    return -1;
  }

  private static bool TryIndexSymbols(
    IReadOnlyList<Instruction> lines,
    int start,
    int end,
    IDictionary<string, int> labels,
    IDictionary<int, int> instructionScopes,
    IDictionary<string, FunctionDefinition> functions,
    IDictionary<int, int> functionStarts,
    ISet<int> functionEnds,
    out string error,
    out int errorLine
  ) {
    error = null;
    errorLine = 0;
    string activeFunction = null;
    var functionStart = -1;
    var functionScopeOpen = false;
    for (var index = start; index < end; index++) {
      var instruction = lines[index];
      // Expression and function regions use disjoint ID ranges, including when both begin at 0.
      var scope = activeFunction is null ? -start - 1 : functionStart + 1;
      instructionScopes[index] = scope;
      if (instruction.Error is not null) {
        error = instruction.Error;
        errorLine = instruction.Line;
        return false;
      }
      var command = instruction.Command;
      var argument = instruction.Argument;
      if (string.IsNullOrEmpty(command)) continue;
      if (activeFunction is not null) {
        if (command == "FUNC") {
          error = "functions may not be nested";
          errorLine = instruction.Line;
          return false;
        }
        if (command == "SCOPE") functionScopeOpen = true;
        if (command != "END") {
          AddScopeLabel(command, argument, index, scope, labels, out error);
          if (error is not null) {
            errorLine = instruction.Line;
            return false;
          }
          continue;
        }
        if (functionScopeOpen) {
          functionScopeOpen = false;
          continue;
        }
        functions.Add(activeFunction, new FunctionDefinition(functionStart + 1, index));
        functionStarts.Add(functionStart, index);
        functionEnds.Add(index);
        activeFunction = null;
        continue;
      }
      if (command == "FUNC") {
        if (functions.ContainsKey(argument)) {
          error = "duplicate function '" + argument + "'";
          errorLine = instruction.Line;
          return false;
        }
        activeFunction = argument;
        functionStart = index;
        functionScopeOpen = false;
        continue;
      }
      AddScopeLabel(command, argument, index, scope, labels, out error);
      if (error is not null) {
        errorLine = instruction.Line;
        return false;
      }
    }
    if (activeFunction is null) return true;
    error = "unterminated function '" + activeFunction + "'";
    errorLine = lines[functionStart].Line;
    return false;
  }

  internal sealed class FunctionDefinition {
    internal FunctionDefinition(int start, int end) {
      Start = start;
      End = end;
    }

    internal int Start { get; }
    internal int End { get; }
  }

  private static bool TryDirective(
    string line,
    out string command,
    out string argument,
    out string operand
  ) {
    command = null;
    argument = null;
    operand = null;
    var position = 0;
    while (position < line.Length && char.IsWhiteSpace(line[position])) position++;
    if (position >= line.Length || line[position] != '@') return false;
    position++;
    var start = position;
    while (position < line.Length && char.IsLetter(line[position])) position++;
    if (position == start) return false;
    command = line.Substring(start, position - start).ToUpperInvariant();
    if (position < line.Length && line[position] == '<') {
      var close = line.IndexOf('>', position + 1);
      if (close < 0) return false;
      argument = line.Substring(position + 1, close - position - 1).Trim();
      position = close + 1;
    }
    while (position < line.Length && char.IsWhiteSpace(line[position])) position++;
    operand = position == line.Length ? "" : line.Substring(position);
    return true;
  }

  private static void TryOutputTarget(
    string argument,
    out MixinExpressionOutputTarget target,
    out string injectionTarget
  ) {
    injectionTarget = null;
    switch ((argument ?? "TARGET").ToUpperInvariant()) {
      case "TARGET":
        target = MixinExpressionOutputTarget.Target;
        return;
      case "CLASS":
        target = MixinExpressionOutputTarget.Class;
        return;
      case "FILE":
        target = MixinExpressionOutputTarget.File;
        return;
      case "IMPLEMENTS":
        target = MixinExpressionOutputTarget.Implements;
        return;
      case "ANNOTATION":
        target = MixinExpressionOutputTarget.Annotation;
        return;
      default:
        target = MixinExpressionOutputTarget.Injection;
        injectionTarget = argument;
        return;
    }
  }

  private static bool TryEvaluateAll(
    IReadOnlyList<MixinExpressionReference> expression,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> variables,
    out bool result,
    out string error,
    out string failure
  ) {
    result = true;
    error = null;
    failure = null;
    var found = false;
    foreach (var reference in expression) {
      found = true;
      if (!TryEvaluate(reference, context, locals, variables, out var value, out error)) return false;
      result &= value;
      if (!value && failure is null) {
        failure = DescribeFailedCondition(
          SelectFailedCondition(reference, context, locals, variables)
        );
      }
    }
    if (found) return true;
    error = "boolean expression is empty";
    return false;
  }

  private static MixinExpressionReference SelectFailedCondition(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> variables
  ) {
    var valueProperties = reference.Properties.Where(property => !IsBooleanProperty(property)).ToArray();
    foreach (var predicate in reference.Properties.Where(IsBooleanProperty)) {
      var properties = valueProperties.Concat(new[] { predicate }).ToArray();
      var candidate = new MixinExpressionReference(reference.Root, reference.Member, properties);
      if (TryEvaluate(candidate, context, locals, variables, out var value, out _) && !value) return candidate;
    }
    return reference;
  }

  private static bool IsBooleanProperty(MixinExpressionProperty property) {
    return property.Name is
      "eq" or "exists" or "is" or "has" or "isSelf" or "ref" or "in" or "out" or "inout" or
      "argument" or "static" or "public" or "exposed" or "top" or "concrete" or "partial" or
      "generic" or "struct" or "class";
  }

  private static string DescribeFailedCondition(MixinExpressionReference reference) {
    var subject = reference.Root switch {
      "var" => "Variable " + (reference.Member ?? "<unnamed>"),
      "local" => "Local variable " + (reference.Member ?? "<unnamed>"),
      "arg" => "Argument " + (reference.Member ?? "<unspecified>"),
      "this" => "Current type" + MemberSuffix(reference.Member),
      "target" => "Target" + MemberSuffix(reference.Member),
      "attr" => "Attribute" + MemberSuffix(reference.Member),
      _ => "Value @" + reference.Root + MemberSuffix(reference.Member)
    };
    var predicate = reference.Properties.FirstOrDefault(IsBooleanProperty);
    if (predicate is null) return subject + " is null, false or invalid";
    var expected = predicate.Argument ?? "";
    switch (predicate.Name) {
      case "eq":
        return subject + (predicate.Negated ? " is " : " is not ") +
          (expected.Length == 0 ? "the expected value" : expected);
      case "exists": return subject + (predicate.Negated ? " exists" : " does not exist");
      case "is": return subject + (predicate.Negated ? " is of type " : " is not of type ") + expected;
      case "has": return subject + (predicate.Negated ? " has member " : " does not have member ") + expected;
      case "isSelf": return subject + (predicate.Negated ? " is the current type" : " is not the current type");
      default:
        return subject + (predicate.Negated ? " is " : " is not ") + PredicateDescription(predicate.Name);
    }
  }

  private static string MemberSuffix(string member) {
    return string.IsNullOrEmpty(member) ? "" : " member " + member;
  }

  private static string PredicateDescription(string name) {
    return name switch {
      "ref" => "a ref parameter",
      "in" => "an in parameter",
      "out" => "an out parameter",
      "inout" => "an in or out parameter",
      "argument" => "a normal argument",
      "static" => "static",
      "public" => "public",
      "exposed" => "public or internal",
      "top" => "a top-level type",
      "concrete" => "concrete",
      "partial" => "partial",
      "generic" => "generic",
      "struct" => "a struct",
      "class" => "a class",
      _ => name
    };
  }

  private static bool TryInterpolate(
    IReadOnlyList<StringPart> expression,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> variables,
    out string result,
    out string error
  ) {
    error = null;
    var builder = new StringBuilder();
    foreach (var part in expression) {
      if (part.Reference is null) {
        builder.Append(part.Literal);
        continue;
      }
      if (!TryResolve(part.Reference, context, locals, variables, out var value, out error)) {
        result = null;
        return false;
      }
      builder.Append(value);
    }
    result = builder.ToString();
    return true;
  }

  private static bool TryResolve(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> variables,
    out string value,
    out string error
  ) {
    if (TryStored(reference, locals, variables, out value, out error)) return error is null;
    return context.TryResolve(reference, out value, out error);
  }

  private static bool TryEvaluate(
    MixinExpressionReference reference,
    IMixinExpressionContext context,
    IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> variables,
    out bool value,
    out string error
  ) {
    if (reference.Root == "local" || reference.Root == "var") {
      TryStored(reference, locals, variables, out var stored, out error);
      var predicates = reference.Properties.Where(item => item.Name is "eq" or "exists").ToArray();
      if (error is not null) {
        if (predicates.Length == 0) {
          value = false;
          return false;
        }
        error = null;
        value = true;
        foreach (var predicate in predicates) {
          var item = predicate.Name == "eq" && RelaxedEquals(null, predicate.Argument);
          value &= predicate.Negated ? !item : item;
        }
        return true;
      }
      if (predicates.Length == 0) {
        value = !string.Equals(stored, "false", StringComparison.OrdinalIgnoreCase);
        return true;
      }
      value = true;
      foreach (var predicate in predicates) {
        var item = predicate.Name == "exists" ||
          RelaxedEquals(stored, predicate.Argument);
        value &= predicate.Negated ? !item : item;
      }
      return true;
    }
    return context.TryEvaluate(reference, out value, out error);
  }

  private static bool RelaxedEquals(string actual, string expected) {
    var expectedIsNull = string.Equals(expected, "null", StringComparison.OrdinalIgnoreCase);
    if (actual is null) return expectedIsNull;
    if (string.Equals(actual, expected ?? "", StringComparison.Ordinal)) return true;
    return string.Equals(
      UnwrapComparable(actual), UnwrapComparable(expected ?? ""), StringComparison.OrdinalIgnoreCase
    );
  }

  private static string UnwrapComparable(string value) {
    if (value.StartsWith("global::", StringComparison.Ordinal)) value = value.Substring(8);
    if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
      value = value.Substring(1, value.Length - 2);
    return value;
  }

  private static bool TryStored(
    MixinExpressionReference reference,
    IReadOnlyDictionary<string, string> locals,
    IReadOnlyDictionary<string, string> variables,
    out string value,
    out string error
  ) {
    value = null;
    error = null;
    if (reference.Root != "local" && reference.Root != "var") return false;
    if (string.IsNullOrEmpty(reference.Member)) {
      error = "@" + reference.Root + " requires a member name";
      return true;
    }
    var values = reference.Root == "local" ? locals : variables;
    if (!values.TryGetValue(reference.Member, out value)) {
      error = "unknown @" + reference.Root + " value '" + reference.Member + "'";
      return true;
    }
    foreach (var property in reference.Properties) {
      if (property.Name is "eq" or "exists") continue;
      if (property.Name == "name") value = reference.Member;
      else {
        error = "property '" + property.Name + "' is not valid for @" + reference.Root;
        return true;
      }
    }
    return true;
  }

  private static bool TryReadReference(
    string text,
    ref int position,
    out MixinExpressionReference reference,
    out string error
  ) {
    reference = null;
    error = null;
    if (position >= text.Length || text[position] != '@') {
      error = "expected '@' expression reference";
      return false;
    }
    position++;
    var parenthesized = position < text.Length && text[position] == '(';
    if (parenthesized) position++;
    var rootStart = position;
    while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
    if (position == rootStart) {
      error = "expression reference has no root";
      return false;
    }
    var root = text.Substring(rootStart, position - rootStart);
    string member = null;
    if (position < text.Length && text[position] == '#') {
      position++;
      var memberStart = position;
      while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
      if (position == memberStart) {
        error = "member reference is empty";
        return false;
      }
      member = text.Substring(memberStart, position - memberStart);
    }

    var properties = new List<MixinExpressionProperty>();
    while (position < text.Length && (text[position] == ':' || text[position] == '#')) {
      if (text[position] == '#') {
        position++;
        var pathStart = position;
        while (position < text.Length &&
          (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
        if (position == pathStart) {
          error = "type argument path is empty";
          return false;
        }
        properties.Add(
          new MixinExpressionProperty(
            "path", text.Substring(pathStart, position - pathStart)
          )
        );
        continue;
      }
      position++;
      var negated = false;
      if (position < text.Length && text[position] == '!') {
        negated = true;
        position++;
      }
      if (position < text.Length && text[position] == '?') position++;
      var propertyStart = position;
      while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
      if (position == propertyStart) {
        error = "property name is empty";
        return false;
      }
      var name = text.Substring(propertyStart, position - propertyStart);
      string argument = null;
      if (position < text.Length && text[position] == '<') {
        position++;
        var argumentStart = position;
        var depth = 1;
        while (position < text.Length && depth != 0) {
          if (text[position] == '<') depth++;
          else if (text[position] == '>') depth--;
          if (depth != 0) position++;
        }
        if (depth != 0) {
          error = "unterminated property argument";
          return false;
        }
        argument = text.Substring(argumentStart, position - argumentStart);
        position++;
      }
      properties.Add(new MixinExpressionProperty(name, argument, negated));
    }
    if (parenthesized) {
      if (position >= text.Length || text[position] != ')') {
        error = "unterminated parenthesized reference";
        return false;
      }
      position++;
    }
    reference = new MixinExpressionReference(root, member, properties);
    return true;
  }

  private static void CommitVariables(
    IDictionary<string, string> destination,
    IReadOnlyDictionary<string, string> source
  ) {
    if (destination is null) return;
    destination.Clear();
    foreach (var item in source) destination[item.Key] = item.Value;
  }

  private static MixinExpressionResult Success(
    IReadOnlyList<MixinExpressionOutput> outputs,
    IReadOnlyList<MixinExpressionLog> logs
  ) {
    return new MixinExpressionResult(true, null, 0, outputs, logs);
  }

  private static MixinExpressionResult Failure(
    string error,
    int line,
    IReadOnlyList<MixinExpressionLog> logs = null
  ) {
    return new MixinExpressionResult(false, error, line, Array.Empty<MixinExpressionOutput>(), logs);
  }

  private static MixinExpressionValidationResult ValidationFailure(string error, int line) {
    return new MixinExpressionValidationResult(false, error, line);
  }
}