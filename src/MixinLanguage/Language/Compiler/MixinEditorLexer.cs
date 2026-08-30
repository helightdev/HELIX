using System;
using System.Collections.Generic;

namespace MixinLanguage.Compiler;

public enum MixinEditorTokenKind {
  Directive,
  Value,
  Path,
  Function,
  ArgumentDelimiter,
  Argument,
  Operator,
  Parenthesis,
  Escape,
  Continuation,
  Comment,
  Whitespace,
  NewLine,
  Text,
  Invalid
}

public readonly struct MixinEditorToken {
  public MixinEditorToken(MixinEditorTokenKind kind, int start, int end) {
    Kind = kind;
    Start = start;
    End = end;
  }

  public MixinEditorTokenKind Kind { get; }
  public int Start { get; }
  public int End { get; }
}

/// <summary>
/// Lossless editor-facing tokenization for mixin additional files. This lives beside the
/// compiler lexer so IDE hosts do not maintain a second approximation of the language.
/// </summary>
public static class MixinEditorLexer {
  public static IReadOnlyList<MixinEditorToken> Lex(string source) {
    source ??= "";
    var result = new List<MixinEditorToken>();
    var position = 0;
    var lineStart = true;
    var expectFunction = false;
    while (position < source.Length) {
      var start = position;
      var current = source[position];
      if (current is '\r' or '\n') {
        if (current == '\r' && position + 1 < source.Length && source[position + 1] == '\n') position++;
        position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.NewLine, start, position));
        lineStart = true;
        expectFunction = false;
        continue;
      }
      if (current is ' ' or '\t') {
        while (position < source.Length && source[position] is ' ' or '\t') position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Whitespace, start, position));
        continue;
      }
      if (lineStart && StartsWith(source, position, "@#")) {
        while (position < source.Length && source[position] is not ('\r' or '\n')) position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Comment, start, position));
        lineStart = false;
        continue;
      }
      if (lineStart && (StartsWith(source, position, "@\\") || StartsWith(source, position, "@+"))) {
        position += 2;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Continuation, start, position));
        lineStart = false;
        continue;
      }
      if (StartsWith(source, position, "@@")) {
        position += 2;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Escape, start, position));
        lineStart = false;
        continue;
      }
      if (StartsWith(source, position, "@(")) {
        position += 2;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Parenthesis, start, position));
        lineStart = false;
        continue;
      }
      if (current == ')' || current == '<' || current == '>') {
        position++;
        result.Add(new MixinEditorToken(
          current == ')' ? MixinEditorTokenKind.Parenthesis : MixinEditorTokenKind.ArgumentDelimiter,
          start, position));
        lineStart = false;
        continue;
      }
      if (StartsWith(source, position, ":!?") || StartsWith(source, position, ":?") || current == ':') {
        position += StartsWith(source, position, ":!?") ? 3 : StartsWith(source, position, ":?") ? 2 : 1;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Operator, start, position));
        expectFunction = true;
        lineStart = false;
        continue;
      }
      if (current == '#') {
        position++;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Path, start, position));
        lineStart = false;
        continue;
      }
      if (current == '@' && position + 1 < source.Length && IsName(source[position + 1])) {
        position += 2;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinEditorToken(
          lineStart ? MixinEditorTokenKind.Directive : MixinEditorTokenKind.Value, start, position));
        lineStart = false;
        continue;
      }
      if (expectFunction && IsName(current)) {
        position++;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Function, start, position));
        expectFunction = false;
        lineStart = false;
        continue;
      }
      while (position < source.Length && source[position] is not ('@' or '#' or ':' or '<' or '>' or ')' or '\r' or '\n' or ' ' or '\t')) position++;
      if (position == start) position++;
      result.Add(new MixinEditorToken(MixinEditorTokenKind.Text, start, position));
      lineStart = false;
    }
    return result.AsReadOnly();
  }

  private static bool StartsWith(string source, int position, string value) =>
    position + value.Length <= source.Length &&
    string.CompareOrdinal(source, position, value, 0, value.Length) == 0;

  private static bool IsName(char value) => value == '_' || char.IsLetterOrDigit(value);
}
