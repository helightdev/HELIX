using System;
using System.Collections.Generic;
using System.Text;

namespace HELIX.SourceGen.Expressions;

internal static class MixinExpressionParser {
  internal static string[] SplitLines(string expression) {
    var lines = expression.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    var continuedLine = -1;
    for (var index = 0; index < lines.Length; index++) {
      var line = lines[index];
      var marker = 0;
      while (marker < line.Length && (line[marker] == ' ' || line[marker] == '\t')) marker++;
      if (marker + 1 < line.Length && line[marker] == '@') {
        var kind = line[marker + 1];
        switch (kind) {
          case '#':
            lines[index] = "";
            continuedLine = -1;
            continue;
          case '\\' or '+': {
            if (continuedLine >= 0) {
              lines[continuedLine] += (kind == '\\' ? "\n" : "") + line.Substring(marker + 2);
              lines[index] = "";
            }
            continue;
          }
        }
      }
      continuedLine = marker + 1 < line.Length && line[marker] == '@' && (char.IsLetter(line[marker + 1]) || line[marker + 1] == '_') ? index : -1;
    }
    return lines;
  }

  internal static bool IsDynamicArgument(string value) {
    return value is { Length: >= 2 } && value[0] == '(' && value[value.Length - 1] == ')';
  }

  internal static DirectiveInstruction ParseDirective(string text, int line) {
    if (string.IsNullOrWhiteSpace(text)) return new DirectiveInstruction(line, null, null, "", null);
    var trimmed = text.TrimStart(' ', '\t');
    if (trimmed.StartsWith("@\\", StringComparison.Ordinal) || trimmed.StartsWith("@+", StringComparison.Ordinal))
      return new DirectiveInstruction(line, null, null, "", "continuation requires an immediately preceding directive");
    if (!TryReadDirective(text, out var name, out var arguments, out var operand))
      return new DirectiveInstruction(line, null, null, "", "expected an expression directive");
    if (!DirectiveLibrary.TryGet(name, out var directive))
      return new DirectiveInstruction(line, null, arguments, operand, "unknown directive '@" + name + "'");
    return directive.Validate(arguments, operand, out var error) && ValidateOperand(directive, operand, out error)
      ? new DirectiveInstruction(line, directive, arguments, operand, null)
      : new DirectiveInstruction(line, directive, arguments, operand, error);
  }

  private static bool ValidateOperand(DirectiveDefinition directive, string operand, out string error) {
    switch (directive.OperandKind) {
      case DirectiveOperandKind.Boolean: return MixinExpressionCompiler.ValidateBooleanExpressionSyntax(operand, out error);
      case DirectiveOperandKind.Value: return MixinExpressionCompiler.ValidateValueExpressionSyntax(operand, out error);
      case DirectiveOperandKind.None:
      default:
        error = null;
        return true;
    }
  }

  internal static IReadOnlyList<MixinExpressionReference> ParseBooleanExpression(string text) {
    var result = new List<MixinExpressionReference>();
    var position = 0;
    while (position < text.Length) {
      while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
      if (position == text.Length) break;
      if (!MixinExpressionEvaluator.TryReadReferenceNode(text, ref position, out var reference, out _)) {
        result.Add(null);
        break;
      }
      result.Add(reference);
    }
    return result.AsReadOnly();
  }

  internal static IReadOnlyList<ValueExpressionPart> ParseValueExpression(string text) {
    var result = new List<ValueExpressionPart>();
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
        result.Add(new ValueExpressionPart(literal.ToString(), null));
        literal.Clear();
      }
      if (!MixinExpressionEvaluator.TryReadReferenceNode(text, ref position, out var reference, out _)) break;
      result.Add(new ValueExpressionPart(null, reference));
    }
    if (literal.Length != 0 || result.Count == 0) result.Add(new ValueExpressionPart(literal.ToString(), null));
    return result.AsReadOnly();
  }

  internal static bool TryReadDirective(
    string line, out string command, out IReadOnlyList<string> arguments, out string operand
  ) {
    command = null;
    arguments = [];
    operand = null;
    var position = 0;
    while (position < line.Length && char.IsWhiteSpace(line[position])) position++;
    if (position >= line.Length || line[position] != '@') return false;
    position++;
    var start = position;
    while (position < line.Length && (char.IsLetter(line[position]) || line[position] == '_')) position++;
    if (position == start) return false;
    command = line.Substring(start, position - start).ToUpperInvariant();
    List<string> parsedArguments = null;
    while (position < line.Length && line[position] == '<') {
      var argumentStart = ++position;
      var depth = 1;
      while (position < line.Length && depth != 0) {
        switch (line[position]) {
          case '<': depth++; break;
          case '>': depth--; break;
        }
        position++;
      }
      if (depth != 0) return false;
      (parsedArguments ??= []).Add(line.Substring(argumentStart, position - argumentStart - 1).Trim());
    }
    if (parsedArguments is not null) arguments = parsedArguments.AsReadOnly();
    while (position < line.Length && char.IsWhiteSpace(line[position])) position++;
    operand = position == line.Length ? "" : line.Substring(position);
    return true;
  }
}