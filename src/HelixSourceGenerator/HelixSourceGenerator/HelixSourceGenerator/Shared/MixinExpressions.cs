using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HELIX.SourceGen.Expressions {
  public enum MixinExpressionOutputTarget {
    Target,
    Class,
    File,
    Implements,
    Injection,
    Annotation,
    Using
  }

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
  /// Parses and executes the line-oriented mixin expression language. The interpreter is
  /// independent of Roslyn; callers provide symbol/value semantics through
  /// <see cref="IMixinExpressionContext"/>.
  /// </summary>
  public sealed class MixinExpressionInterpreter {
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
        if (!TryDirective(line, out var command, out var argument, out var operand)) {
          return ValidationFailure("expected an expression directive", index + 1);
        }
        if (!IsKnownDirective(command)) {
          return ValidationFailure("unknown directive '@" + command + "'", index + 1);
        }
        if (!ValidateDirectiveSyntax(command, argument, operand, out var error)) {
          return ValidationFailure(error, index + 1);
        }
        if (activeFunction is null) {
          if (command != "FUNC") continue;
          if (!functions.Add(argument)) {
            return ValidationFailure("duplicate function '" + argument + "'", index + 1);
          }
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
    ) => Execute(expression, context, variables, null);

    public MixinExpressionResult Execute(
      string expression,
      IMixinExpressionContext context,
      IDictionary<string, string> variables,
      IEnumerable<string> preparedExpressions
    ) {
      if (context is null) throw new ArgumentNullException(nameof(context));
      if (expression is null) return Failure("the expression is null", 0);

      var lines = new List<string>();
      if (preparedExpressions is not null) {
        foreach (var prepared in preparedExpressions) {
          if (prepared is null) continue;
          lines.AddRange(SplitLines(prepared));
        }
      }
      lines.AddRange(SplitLines(expression));
      var labels = new Dictionary<string, int>(StringComparer.Ordinal);
      var functions = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal);
      var functionStarts = new Dictionary<int, int>();
      var functionEnds = new HashSet<int>();
      string activeFunction = null;
      var functionStart = -1;
      var functionScopeOpen = false;
      for (var index = 0; index < lines.Count; index++) {
        if (!TryDirective(lines[index], out var command, out var argument, out _)) continue;
        if (activeFunction is not null) {
          if (command == "FUNC") return Failure("functions may not be nested", index + 1);
          if (command == "SCOPE") functionScopeOpen = true;
          if (command != "END") {
            AddScopeLabel(command, argument, index, labels, out var labelError);
            if (labelError is not null) return Failure(labelError, index + 1);
            continue;
          }
          if (functionScopeOpen) {
            functionScopeOpen = false;
            continue;
          }
          var definition = new FunctionDefinition(functionStart + 1, index);
          functions.Add(activeFunction, definition);
          functionStarts.Add(functionStart, index);
          functionEnds.Add(index);
          activeFunction = null;
          functionStart = -1;
          continue;
        }
        if (command == "FUNC") {
          if (string.IsNullOrEmpty(argument)) return Failure("FUNC requires a name", index + 1);
          if (functions.ContainsKey(argument)) {
            return Failure("duplicate function '" + argument + "'", index + 1);
          }
          activeFunction = argument;
          functionStart = index;
          functionScopeOpen = false;
          continue;
        }
        AddScopeLabel(command, argument, index, labels, out var error);
        if (error is not null) return Failure(error, index + 1);
      }
      if (activeFunction is not null) return Failure("unterminated function '" + activeFunction + "'", functionStart + 1);

      var locals = new Dictionary<string, string>(StringComparer.Ordinal);
      var pendingVariables = variables is null
        ? new Dictionary<string, string>(StringComparer.Ordinal)
        : new Dictionary<string, string>(variables, StringComparer.Ordinal);
      var outputs = new List<MixinExpressionOutput>();
      var logs = new List<MixinExpressionLog>();
      var pc = 0;
      var steps = 0;
      var maximumSteps = Math.Max(1024, lines.Count * 64);
      var calls = new Stack<int>();
      while (pc < lines.Count) {
        if (++steps > maximumSteps) {
          return Failure("execution limit exceeded (possible GOTO loop)", pc + 1, logs);
        }
        if (functionStarts.TryGetValue(pc, out var functionEnd)) {
          pc = functionEnd + 1;
          continue;
        }
        var lineNumber = pc + 1;
        var instruction = pc;
        var line = lines[pc++];
        if (string.IsNullOrWhiteSpace(line)) continue;
        if (!TryDirective(line, out var command, out var argument, out var operand)) {
          return Failure("expected an expression directive", lineNumber, logs);
        }

        switch (command) {
          case "SCOPE":
            break;
          case "FUNC":
            break;
          case "END":
            if (functionEnds.Contains(instruction) && calls.Count != 0) pc = calls.Pop();
            break;
          case "MATCH":
            if (!TryEvaluateAll(operand, context, locals, pendingVariables, out var matched, out var matchError)) {
              return Failure(matchError, lineNumber, logs);
            }
            if (!matched) {
              var next = FindNextScopeOrEnd(lines, pc, functionStarts, functionEnds);
              if (next < 0) return Failure(
                "MATCH did not match and there is no following scope", lineNumber, logs
              );
              pc = next;
            }
            break;
          case "ASSERT":
            if (!TryEvaluateAll(operand, context, locals, pendingVariables, out var asserted, out var assertError)) {
              return Failure(assertError, lineNumber, logs);
            }
            if (!asserted) return Failure("assertion failed", lineNumber, logs);
            break;
          case "CODE":
            if (!TryInterpolate(operand, context, locals, pendingVariables, out var code, out var codeError)) {
              return Failure(codeError, lineNumber, logs);
            }
            TryOutputTarget(argument, out var outputTarget, out var injectionTarget);
            outputs.Add(new MixinExpressionOutput(outputTarget, code, injectionTarget));
            break;
          case "USING":
            if (!TryInterpolate(operand, context, locals, pendingVariables, out var usingDirective,
                  out var usingError)) {
              return Failure(usingError, lineNumber, logs);
            }
            outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Using, usingDirective));
            break;
          case "LOG":
            if (!TryInterpolate(operand, context, locals, pendingVariables, out var log,
                  out var logError)) {
              return Failure(logError, lineNumber, logs);
            }
            logs.Add(new MixinExpressionLog(log, lineNumber));
            break;
          case "DUMP":
            switch ((argument ?? "").ToUpperInvariant()) {
              case "STATE":
                logs.Add(new MixinExpressionLog(
                  DumpState(lineNumber, pc, calls.Count, locals, pendingVariables), lineNumber
                ));
                break;
              case "BUFFER":
                logs.Add(new MixinExpressionLog(DumpBuffer(outputs), lineNumber));
                break;
              default:
                return Failure("DUMP requires STATE or BUFFER", lineNumber, logs);
            }
            break;
          case "LOCAL":
          case "VAR":
            if (string.IsNullOrEmpty(argument)) {
              return Failure(command + " requires a name", lineNumber, logs);
            }
            if (!TryInterpolate(operand, context, locals, pendingVariables, out var stored, out var storeError)) {
              return Failure(storeError, lineNumber, logs);
            }
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
            if (string.IsNullOrEmpty(argument) || !functions.TryGetValue(argument, out var function)) {
              return Failure("unknown function '" + (argument ?? "") + "'", lineNumber, logs);
            }
            calls.Push(pc);
            pc = function.Start;
            break;
          case "GOTO":
            if (string.IsNullOrEmpty(argument) || !labels.TryGetValue(argument, out var destination)) {
              return Failure("unknown scope label '" + (argument ?? "") + "'", lineNumber, logs);
            }
            pc = destination + 1;
            break;
          case "SKIP":
            var skip = FindNextScopeOrEnd(lines, pc, functionStarts, functionEnds);
            if (skip < 0) return Failure("SKIP has no following scope", lineNumber, logs);
            pc = skip;
            break;
          case "FAIL":
            return Failure("expression requested failure", lineNumber, logs);
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

    private static string[] SplitLines(string expression) =>
      expression.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    private static bool IsKnownDirective(string command) => command is
      "SCOPE" or "FUNC" or "CALL" or "END" or "MATCH" or "ASSERT" or "CODE" or
      "USING" or "LOG" or "DUMP" or "LOCAL" or "VAR" or "RETURN" or "GOTO" or
      "SKIP" or "FAIL";

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
      if (command is "CODE" or "USING" or "LOG" or "LOCAL" or "VAR") {
        return ValidateStringSyntax(operand, out error);
      }
      if (command == "DUMP" && !string.Equals(argument, "STATE", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(argument, "BUFFER", StringComparison.OrdinalIgnoreCase)) {
        error = "DUMP requires STATE or BUFFER";
        return false;
      }
      return true;
    }

    private static string DumpState(
      int line,
      int programCounter,
      int callDepth,
      IReadOnlyDictionary<string, string> locals,
      IReadOnlyDictionary<string, string> variables
    ) => "STATE line=" + line + " pc=" + programCounter + " callDepth=" + callDepth +
         " locals=" + DumpValues(locals) + " variables=" + DumpValues(variables);

    private static string DumpValues(IReadOnlyDictionary<string, string> values) =>
      values.Count == 0
        ? "{}"
        : "{" + string.Join(", ", values.OrderBy(item => item.Key, StringComparer.Ordinal)
          .Select(item => item.Key + "=" + item.Value)) + "}";

    private static string DumpBuffer(IReadOnlyList<MixinExpressionOutput> outputs) =>
      outputs.Count == 0
        ? "BUFFER <empty>"
        : "BUFFER " + string.Join("\n", outputs.Select(item =>
          item.Target + (string.IsNullOrEmpty(item.InjectionTarget)
            ? ""
            : "<" + item.InjectionTarget + ">") + ": " + item.Text
        ));

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
      IDictionary<string, int> labels,
      out string error
    ) {
      error = null;
      if (command != "SCOPE" || string.IsNullOrEmpty(argument)) return;
      if (labels.ContainsKey(argument)) {
        error = "duplicate scope label '" + argument + "'";
        return;
      }
      labels.Add(argument, index);
    }

    private static int FindNextScopeOrEnd(
      IReadOnlyList<string> lines,
      int start,
      IReadOnlyDictionary<int, int> functionStarts,
      ISet<int> functionEnds
    ) {
      for (var index = start; index < lines.Count; index++) {
        if (functionStarts.TryGetValue(index, out var functionEnd)) {
          index = functionEnd;
          continue;
        }
        if (!TryDirective(lines[index], out var command, out _, out _)) continue;
        if (command == "SCOPE") return index;
        if (command == "END") return functionEnds.Contains(index) ? index : index + 1;
      }
      return -1;
    }

    private sealed class FunctionDefinition {
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
        case "TARGET": target = MixinExpressionOutputTarget.Target; return;
        case "CLASS": target = MixinExpressionOutputTarget.Class; return;
        case "FILE": target = MixinExpressionOutputTarget.File; return;
        case "IMPLEMENTS": target = MixinExpressionOutputTarget.Implements; return;
        case "ANNOTATION": target = MixinExpressionOutputTarget.Annotation; return;
        default:
          target = MixinExpressionOutputTarget.Injection;
          injectionTarget = argument;
          return;
      }
    }

    private static bool TryEvaluateAll(
      string text,
      IMixinExpressionContext context,
      IReadOnlyDictionary<string, string> locals,
      IReadOnlyDictionary<string, string> variables,
      out bool result,
      out string error
    ) {
      result = true;
      error = null;
      var position = 0;
      var found = false;
      while (position < text.Length) {
        while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
        if (position == text.Length) break;
        found = true;
        if (!TryReadReference(text, ref position, out var reference, out error)) return false;
        if (!TryEvaluate(reference, context, locals, variables, out var value, out error)) return false;
        result &= value;
      }
      if (found) return true;
      error = "boolean expression is empty";
      return false;
    }

    private static bool TryInterpolate(
      string text,
      IMixinExpressionContext context,
      IReadOnlyDictionary<string, string> locals,
      IReadOnlyDictionary<string, string> variables,
      out string result,
      out string error
    ) {
      error = null;
      var builder = new StringBuilder();
      var position = 0;
      while (position < text.Length) {
        if (text[position] != '@') {
          builder.Append(text[position++]);
          continue;
        }
        if (position + 1 < text.Length && text[position + 1] == '@') {
          builder.Append('@');
          position += 2;
          continue;
        }
        if (!TryReadReference(text, ref position, out var reference, out error)) {
          result = null;
          return false;
        }
        if (!TryResolve(reference, context, locals, variables, out var value, out error)) {
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
          if (predicates.Length != 0 && predicates.All(item => item.Name != "exists")) {
            value = false;
            return false;
          }
          error = null;
          value = predicates.Length != 0 && predicates.All(item => item.Negated);
          return true;
        }
        if (predicates.Length == 0) {
          value = !string.Equals(stored, "false", StringComparison.OrdinalIgnoreCase);
          return true;
        }
        value = true;
        foreach (var predicate in predicates) {
          var item = predicate.Name == "exists" ||
                     string.Equals(stored, predicate.Argument ?? "", StringComparison.Ordinal);
          value &= predicate.Negated ? !item : item;
        }
        return true;
      }
      return context.TryEvaluate(reference, out value, out error);
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
          properties.Add(new MixinExpressionProperty(
            "path", text.Substring(pathStart, position - pathStart)
          ));
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
    ) => new(true, null, 0, outputs, logs);

    private static MixinExpressionResult Failure(
      string error,
      int line,
      IReadOnlyList<MixinExpressionLog> logs = null
    ) => new(false, error, line, Array.Empty<MixinExpressionOutput>(), logs);

    private static MixinExpressionValidationResult ValidationFailure(string error, int line) =>
      new(false, error, line);
  }
}
