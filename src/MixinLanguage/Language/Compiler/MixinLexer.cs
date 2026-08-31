using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler;

/// <summary>
/// Canonical lossless tokenization for mixin source files.
/// </summary>
public static partial class MixinLexer {
  /// <summary>Lexes a complete source document into the shared lossless token stream.</summary>
  public static IReadOnlyList<MixinToken> Lex(string source) {
    source ??= "";
    var result = new List<MixinToken>();
    var position = 0;
    var lineStart = true;
    var expectIdentifier = false;
    var argumentDepth = 0;
    var referenceActive = false;
    var argumentReferences = new Stack<bool>();
    var enclosedReferenceArguments = new Stack<int>();
    var dynamicArguments = new Stack<int>();

    void Add(MixinTokenKind kind, int start, int end) {
      if (kind == MixinTokenKind.Text && result.Count > 0 &&
        result[result.Count - 1].Kind == kind && result[result.Count - 1].End == start) {
        result[result.Count - 1] = new MixinToken(kind, result[result.Count - 1].Start, end);
        return;
      }
      result.Add(new MixinToken(kind, start, end));
    }

    while (position < source.Length) {
      var start = position;
      var current = source[position];
      if (current is '\r' or '\n') {
        if (current == '\r' && position + 1 < source.Length && source[position + 1] == '\n') position++;
        position++;
        result.Add(new MixinToken(MixinTokenKind.NewLine, start, position));
        lineStart = true;
        expectIdentifier = false;
        referenceActive = false;
        enclosedReferenceArguments.Clear();
        continue;
      }
      if (current is ' ' or '\t') {
        while (position < source.Length && source[position] is ' ' or '\t') position++;
        Add(lineStart ? MixinTokenKind.Whitespace : MixinTokenKind.Text, start, position);
        if (argumentDepth == 0) {
          referenceActive = false;
        }
        continue;
      }
      if (lineStart && StartsWith(source, position, "@#")) {
        while (position < source.Length && source[position] is not ('\r' or '\n')) position++;
        result.Add(new MixinToken(MixinTokenKind.Comment, start, position));
        lineStart = false;
        continue;
      }
      if (lineStart && (StartsWith(source, position, "@\\") || StartsWith(source, position, "@+"))) {
        position += 2;
        result.Add(
          new MixinToken(
            source[start + 1] == '+'
              ? MixinTokenKind.DirectContinuation
              : MixinTokenKind.NewLineContinuation, start, position
          )
        );
        referenceActive = source[start + 1] == '+';
        lineStart = false;
        continue;
      }
      if (StartsWith(source, position, "@@")) {
        position += 2;
        result.Add(new MixinToken(MixinTokenKind.Escape, start, position));
        lineStart = false;
        continue;
      }
      if (StartsWith(source, position, "@(")) {
        result.Add(new MixinToken(MixinTokenKind.At, position, position + 1));
        position += 2;
        result.Add(new MixinToken(MixinTokenKind.OpenParenthesis, start + 1, position));
        enclosedReferenceArguments.Push(argumentDepth);
        expectIdentifier = true;
        referenceActive = true;
        lineStart = false;
        continue;
      }
      if (current == '@' && position + 1 < source.Length && source[position + 1] == ':') {
        position++;
        result.Add(new MixinToken(MixinTokenKind.At, start, position));
        referenceActive = true;
        lineStart = false;
        continue;
      }
      if (current == '(' && argumentDepth > 0 && result.Count > 0 &&
        result[result.Count - 1].Kind == MixinTokenKind.OpenArgument) {
        position++;
        result.Add(new MixinToken(MixinTokenKind.OpenParenthesis, start, position));
        dynamicArguments.Push(argumentDepth);
        lineStart = false;
        continue;
      }
      if (current == ')' && (enclosedReferenceArguments.Count > 0 &&
          enclosedReferenceArguments.Peek() == argumentDepth || dynamicArguments.Count > 0 &&
          dynamicArguments.Peek() == argumentDepth) ||
        current == '<' && (referenceActive || argumentDepth > 0) ||
        current == '>' && argumentDepth > 0) {
        position++;
        result.Add(
          new MixinToken(
            current == ')' ? MixinTokenKind.CloseParenthesis :
            current == '<' ? MixinTokenKind.OpenArgument : MixinTokenKind.CloseArgument,
            start, position
          )
        );
        if (current == '<') {
          argumentReferences.Push(referenceActive);
          argumentDepth++;
          referenceActive = false;
        } else if (current == '>' && argumentDepth > 0) {
          argumentDepth--;
          referenceActive = argumentReferences.Pop();
        } else if (current == ')') {
          if (dynamicArguments.Count > 0 && dynamicArguments.Peek() == argumentDepth)
            dynamicArguments.Pop();
          else enclosedReferenceArguments.Pop();
          referenceActive = false;
        }
        lineStart = false;
        continue;
      }
      if (referenceActive &&
        (StartsWith(source, position, ":!?") || StartsWith(source, position, ":?") || current == ':')) {
        position += StartsWith(source, position, ":!?") ? 3 : StartsWith(source, position, ":?") ? 2 : 1;
        result.Add(
          new MixinToken(
            position - start == 3
              ? MixinTokenKind.NegatedPredicateOperator
              : position - start == 2
                ? MixinTokenKind.PredicateOperator
                : MixinTokenKind.FunctionOperator,
            start, position
          )
        );
        expectIdentifier = true;
        lineStart = false;
        continue;
      }
      if (referenceActive && current == '#') {
        result.Add(new MixinToken(MixinTokenKind.Hash, position, position + 1));
        position++;
        var identifierStart = position;
        while (position < source.Length && IsName(source[position])) position++;
        if (position > identifierStart)
          result.Add(new MixinToken(MixinTokenKind.Identifier, identifierStart, position));
        lineStart = false;
        continue;
      }
      if (current == '@' && position + 1 < source.Length && IsName(source[position + 1])) {
        result.Add(new MixinToken(MixinTokenKind.At, position, position + 1));
        position++;
        var identifierStart = position++;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinToken(MixinTokenKind.Identifier, identifierStart, position));
        referenceActive = true;
        lineStart = false;
        continue;
      }
      if (expectIdentifier && IsName(current)) {
        position++;
        while (position < source.Length && IsName(source[position])) position++;
        result.Add(new MixinToken(MixinTokenKind.Identifier, start, position));
        expectIdentifier = false;
        lineStart = false;
        continue;
      }
      while (position < source.Length &&
        source[position] is not ('@' or '#' or ':' or '<' or '>' or ')' or '\r' or '\n' or ' ' or '\t')) position++;
      if (position == start) position++;
      Add(MixinTokenKind.Text, start, position);
      referenceActive = false;
      lineStart = false;
    }
    var located = new List<MixinToken>(result.Count);
    var scan = 0;
    var line = 1;
    var locationLineStart = 0;
    foreach (var token in result) {
      while (scan < token.Start) {
        if (source[scan] == '\r') {
          if (scan + 1 < source.Length && source[scan + 1] == '\n') scan++;
          line++;
          locationLineStart = scan + 1;
        } else if (source[scan] == '\n') {
          line++;
          locationLineStart = scan + 1;
        }
        scan++;
      }
      located.Add(
        token.WithTextAndLocation(
          source.Substring(token.Start, token.End - token.Start), line, token.Start - locationLineStart
        )
      );
    }
    return located.AsReadOnly();
  }

  internal static bool TryUnwrapDynamicArgument(string source, out string inner) {
    inner = null;
    if (source is not { Length: >= 2 } || source[0] != '(' || source[source.Length - 1] != ')') return false;
    inner = source.Substring(1, source.Length - 2);
    return true;
  }

  private static bool StartsWith(string source, int position, string value) =>
    position + value.Length <= source.Length &&
    string.CompareOrdinal(source, position, value, 0, value.Length) == 0;

  private static bool IsName(char value) => value == '_' || char.IsLetterOrDigit(value);
}

public enum MixinTokenKind {
  At,
  Identifier,
  Hash,
  FunctionOperator,
  PredicateOperator,
  NegatedPredicateOperator,
  OpenArgument,
  CloseArgument,
  OpenParenthesis,
  CloseParenthesis,
  Escape,
  DirectContinuation,
  NewLineContinuation,
  Comment,
  Whitespace,
  NewLine,
  Text,
  Invalid
}

public sealed record MixinToken {
  public MixinToken(MixinTokenKind kind, int start, int end)
    : this(kind, null, start, end) { }

  public MixinToken(MixinTokenKind kind, string text, int start, int end) {
    Kind = kind;
    Text = text;
    Line = 0;
    SourceRange = new MixinSourceRange(start, end, 0, start);
    ContentRange = SourceRange;
  }

  internal MixinToken(
    MixinTokenKind kind, string text, int line,
    MixinSourceRange sourceRange, MixinSourceRange contentRange
  ) {
    Kind = kind;
    Text = text;
    Line = line;
    SourceRange = sourceRange with { Line = line, Column = sourceRange.Start };
    ContentRange = contentRange with { Line = line, Column = contentRange.Start };
  }

  public MixinTokenKind Kind { get; }
  public string Text { get; }
  public int Line { get; }
  public MixinSourceRange SourceRange { get; }
  public MixinSourceRange ContentRange { get; }
  public int Start => SourceRange.Start;
  public int End => SourceRange.End;

  public MixinToken Offset(int offset) => new(
    Kind, Text, Line,
    SourceRange with { Start = Start + offset, End = End + offset, Column = SourceRange.Column + offset },
    ContentRange with {
      Start = ContentRange.Start + offset, End = ContentRange.End + offset, Column = ContentRange.Column + offset
    }
  );

  public MixinToken WithLocation(int line, int column) => new(
    Kind, Text, line,
    SourceRange with { Line = line, Column = column },
    ContentRange with { Line = line, Column = column + Math.Max(0, ContentRange.Start - SourceRange.Start) }
  );

  public MixinToken WithTextAndLocation(string text, int line, int column) => new(
    Kind, text, line,
    SourceRange with { Line = line, Column = column },
    ContentRange with { Line = line, Column = column + Math.Max(0, ContentRange.Start - SourceRange.Start) }
  );

  public MixinToken WithRange(MixinSourceRange range) => new(
    Kind, Text, range.Line, range,
    range with {
      Start = range.Start + Math.Max(0, ContentRange.Start - SourceRange.Start),
      End = range.End - Math.Max(0, SourceRange.End - ContentRange.End),
      Column = range.Column + Math.Max(0, ContentRange.Start - SourceRange.Start)
    }
  );
}

internal sealed class MixinTokenStream(IReadOnlyList<MixinToken> tokens) {
  public IReadOnlyList<MixinToken> Tokens { get; } = tokens ?? [];
  public int Position { get; set; }
  public bool AtEnd => Position >= Tokens.Count;
  public MixinToken Current => AtEnd ? null : Tokens[Position];
  public MixinToken Previous => Position == 0 ? null : Tokens[Position - 1];

  public bool At(MixinTokenKind kind) => Current is { } token && token.Kind == kind;

  public bool Take(MixinTokenKind kind, out MixinToken token) {
    if (At(kind)) {
      token = Tokens[Position++];
      return true;
    }
    token = null;
    return false;
  }

  public MixinToken Consume() => AtEnd ? null : Tokens[Position++];

  public IReadOnlyList<MixinToken> Slice(int start, int end) =>
    Tokens.Skip(start).Take(Math.Max(0, end - start)).ToArray();
}