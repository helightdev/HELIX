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
      IReadOnlyList<MixinExpressionOutput> outputs
    ) {
      Success = success;
      Error = error;
      ErrorLine = errorLine;
      Outputs = outputs ?? Array.Empty<MixinExpressionOutput>();
    }

    public bool Success { get; }
    public string Error { get; }
    public int ErrorLine { get; }
    public IReadOnlyList<MixinExpressionOutput> Outputs { get; }
  }

  /// <summary>
  /// Parses and executes the line-oriented mixin expression language. The interpreter is
  /// independent of Roslyn; callers provide symbol/value semantics through
  /// <see cref="IMixinExpressionContext"/>.
  /// </summary>
  public sealed class MixinExpressionInterpreter {
    public MixinExpressionResult Execute(
      string expression,
      IMixinExpressionContext context,
      IDictionary<string, string> variables = null
    ) {
      if (context is null) throw new ArgumentNullException(nameof(context));
      if (expression is null) return Failure("the expression is null", 0);

      var lines = expression.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
      var labels = new Dictionary<string, int>(StringComparer.Ordinal);
      for (var index = 0; index < lines.Length; index++) {
        if (!TryDirective(lines[index], out var command, out var argument, out _)) continue;
        if (command != "SCOPE" || string.IsNullOrEmpty(argument)) continue;
        if (labels.ContainsKey(argument)) return Failure("duplicate scope label '" + argument + "'", index + 1);
        labels.Add(argument, index);
      }

      var locals = new Dictionary<string, string>(StringComparer.Ordinal);
      var pendingVariables = variables is null
        ? new Dictionary<string, string>(StringComparer.Ordinal)
        : new Dictionary<string, string>(variables, StringComparer.Ordinal);
      var outputs = new List<MixinExpressionOutput>();
      var pc = 0;
      var steps = 0;
      var maximumSteps = Math.Max(1024, lines.Length * 64);
      while (pc < lines.Length) {
        if (++steps > maximumSteps) return Failure("execution limit exceeded (possible GOTO loop)", pc + 1);
        var lineNumber = pc + 1;
        var line = lines[pc++];
        if (string.IsNullOrWhiteSpace(line)) continue;
        if (!TryDirective(line, out var command, out var argument, out var operand)) {
          return Failure("expected an expression directive", lineNumber);
        }

        switch (command) {
          case "SCOPE":
          case "END":
            break;
          case "MATCH":
            if (!TryEvaluateAll(operand, context, locals, pendingVariables, out var matched, out var matchError)) {
              return Failure(matchError, lineNumber);
            }
            if (!matched) {
              var next = FindNextScopeOrEnd(lines, pc);
              if (next < 0) return Failure("MATCH did not match and there is no following scope", lineNumber);
              pc = next;
            }
            break;
          case "ASSERT":
            if (!TryEvaluateAll(operand, context, locals, pendingVariables, out var asserted, out var assertError)) {
              return Failure(assertError, lineNumber);
            }
            if (!asserted) return Failure("assertion failed", lineNumber);
            break;
          case "CODE":
            if (!TryInterpolate(operand, context, locals, pendingVariables, out var code, out var codeError)) {
              return Failure(codeError, lineNumber);
            }
            TryOutputTarget(argument, out var outputTarget, out var injectionTarget);
            outputs.Add(new MixinExpressionOutput(outputTarget, code, injectionTarget));
            break;
          case "USING":
            if (!TryInterpolate(operand, context, locals, pendingVariables, out var usingDirective,
                  out var usingError)) {
              return Failure(usingError, lineNumber);
            }
            outputs.Add(new MixinExpressionOutput(MixinExpressionOutputTarget.Using, usingDirective));
            break;
          case "LOCAL":
          case "VAR":
            if (string.IsNullOrEmpty(argument)) return Failure(command + " requires a name", lineNumber);
            if (!TryInterpolate(operand, context, locals, pendingVariables, out var stored, out var storeError)) {
              return Failure(storeError, lineNumber);
            }
            (command == "LOCAL" ? locals : pendingVariables)[argument] = stored;
            break;
          case "RETURN":
            CommitVariables(variables, pendingVariables);
            return Success(outputs);
          case "GOTO":
            if (string.IsNullOrEmpty(argument) || !labels.TryGetValue(argument, out var destination)) {
              return Failure("unknown scope label '" + (argument ?? "") + "'", lineNumber);
            }
            pc = destination + 1;
            break;
          case "SKIP":
            var skip = FindNextScopeOrEnd(lines, pc);
            if (skip < 0) return Failure("SKIP has no following scope", lineNumber);
            pc = skip;
            break;
          case "FAIL":
            return Failure("expression requested failure", lineNumber);
          default:
            return Failure("unknown directive '@" + command + "'", lineNumber);
        }
      }

      CommitVariables(variables, pendingVariables);
      return Success(outputs);
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

    private static int FindNextScopeOrEnd(IReadOnlyList<string> lines, int start) {
      for (var index = start; index < lines.Count; index++) {
        if (!TryDirective(lines[index], out var command, out _, out _)) continue;
        if (command == "SCOPE") return index;
        if (command == "END") return index + 1;
      }
      return -1;
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

    private static MixinExpressionResult Success(IReadOnlyList<MixinExpressionOutput> outputs) =>
      new(true, null, 0, outputs);

    private static MixinExpressionResult Failure(string error, int line) =>
      new(false, error, line, Array.Empty<MixinExpressionOutput>());
  }
}
