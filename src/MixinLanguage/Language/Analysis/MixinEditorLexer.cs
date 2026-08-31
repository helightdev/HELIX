using System;
using System.Collections.Generic;

namespace MixinLanguage.Analysis;

public enum MixinEditorTokenKind {
  Directive,
  Value,
  Path,
  Function,
  ArgumentDelimiter,
  DirectiveArgumentDelimiter,
  ExpressionArgumentDelimiter,
  Argument,
  Operator,
  Parenthesis,
  EnclosedReferenceParenthesis,
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
    var expectEnclosedRoot = false;
    var argumentDepth = 0;
    var referenceActive = false;
    var directiveHeader = false;
    var argumentReferences = new Stack<bool>();
    var directiveArguments = new Stack<bool>();
    var enclosedReferenceDepth = 0;
    while (position < source.Length) {
      var start = position;
      var current = source[position];
      if (current is '\r' or '\n') {
        if (current == '\r' && position + 1 < source.Length && source[position + 1] == '\n') position++;
        position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.NewLine, start, position));
        lineStart = true;
        expectFunction = false;
        expectEnclosedRoot = false;
        referenceActive = false;
        directiveHeader = false;
        enclosedReferenceDepth = 0;
        continue;
      }
      if (current is ' ' or '\t') {
        while (position < source.Length && source[position] is ' ' or '\t') position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Whitespace, start, position));
        if (argumentDepth == 0) {
          referenceActive = false;
          directiveHeader = false;
        }
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
        referenceActive = false;
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
        position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Value, start, position));
        result.Add(new MixinEditorToken(MixinEditorTokenKind.EnclosedReferenceParenthesis, position, ++position));
        enclosedReferenceDepth++;
        expectEnclosedRoot = true;
        referenceActive = true;
        lineStart = false;
        continue;
      }
      if (current == ')' && enclosedReferenceDepth > 0 ||
          current == '<' && (referenceActive || directiveHeader) ||
          current == '>' && argumentDepth > 0) {
        position++;
        var directiveArgument = current == '<'
          ? directiveHeader && !referenceActive
          : current == '>' && directiveArguments.Peek();
        result.Add(new MixinEditorToken(
          current == ')' ? MixinEditorTokenKind.EnclosedReferenceParenthesis :
          directiveArgument ? MixinEditorTokenKind.DirectiveArgumentDelimiter :
          MixinEditorTokenKind.ExpressionArgumentDelimiter,
          start, position));
        if (current == '<') {
          argumentReferences.Push(referenceActive);
          directiveArguments.Push(directiveArgument);
          argumentDepth++;
          referenceActive = false;
        } else if (current == '>' && argumentDepth > 0) {
          argumentDepth--;
          referenceActive = argumentReferences.Pop();
          directiveArguments.Pop();
        } else if (current == ')') {
          enclosedReferenceDepth--;
          referenceActive = false;
        }
        lineStart = false;
        continue;
      }
      if (referenceActive &&
          (StartsWith(source, position, ":!?") || StartsWith(source, position, ":?") || current == ':')) {
        position += StartsWith(source, position, ":!?") ? 3 : StartsWith(source, position, ":?") ? 2 : 1;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Operator, start, position));
        expectFunction = true;
        expectEnclosedRoot = false;
        lineStart = false;
        continue;
      }
      if (referenceActive && current == '#') {
        position++;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Path, start, position));
        lineStart = false;
        continue;
      }
      if (current == '@' && position + 1 < source.Length && IsName(source[position + 1])) {
        var directive = lineStart;
        position += 2;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinEditorToken(
          directive ? MixinEditorTokenKind.Directive : MixinEditorTokenKind.Value, start, position));
        directiveHeader = directive;
        referenceActive = !directive;
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
      if (expectEnclosedRoot && IsName(current)) {
        position++;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinEditorToken(MixinEditorTokenKind.Value, start, position));
        expectEnclosedRoot = false;
        lineStart = false;
        continue;
      }
      while (position < source.Length && source[position] is not ('@' or '#' or ':' or '<' or '>' or ')' or '\r' or '\n' or ' ' or '\t')) position++;
      if (position == start) position++;
      result.Add(new MixinEditorToken(argumentDepth > 0 ? MixinEditorTokenKind.Argument : MixinEditorTokenKind.Text, start, position));
      referenceActive = false;
      lineStart = false;
    }
    return result.AsReadOnly();
  }

  private static bool StartsWith(string source, int position, string value) =>
    position + value.Length <= source.Length &&
    string.CompareOrdinal(source, position, value, 0, value.Length) == 0;

  private static bool IsName(char value) => value == '_' || char.IsLetterOrDigit(value);
}
