using System.Collections.Generic;

namespace HelixSourceGenerator.Language.Compiler;

internal enum MixinTokenKind {
  At,
  Identifier,
  Argument,
  Operand,
  EndOfLine,
  OrphanContinuation,
  Invalid
}

internal sealed record MixinToken(MixinTokenKind Kind, string Text, int Line);

internal enum MixinExpressionTokenKind {
  Literal,
  At,
  OpenParenthesis,
  CloseParenthesis,
  NullRoot,
  Identifier,
  Member,
  Path,
  Property,
  Predicate,
  Negation,
  ArgumentStart,
  ArgumentLiteral,
  ArgumentExpressionStart,
  ArgumentExpressionEnd,
  ArgumentEnd,
  Invalid
}

internal sealed record MixinExpressionToken(
  MixinExpressionTokenKind Kind, string Text, int Start, int End
);

/// <summary>Turns source text into tokens without applying directive semantics.</summary>
internal static class MixinExpressionLexer {
  internal static IReadOnlyList<MixinExpressionToken> LexExpression(string source) {
    source ??= "";
    var tokens = new List<MixinExpressionToken>();
    var position = 0;
    var literalStart = 0;
    while (position < source.Length) {
      if (source[position] != '@') {
        position++;
        continue;
      }
      if (position + 1 < source.Length && source[position + 1] == '@') {
        AddLiteral(tokens, source, literalStart, position);
        tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.Literal, "@", position, position + 2));
        position += 2;
        literalStart = position;
        continue;
      }
      AddLiteral(tokens, source, literalStart, position);
      if (!LexReference(source, ref position, tokens)) {
        tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.Invalid, "", position, position));
        return tokens.AsReadOnly();
      }
      literalStart = position;
    }
    AddLiteral(tokens, source, literalStart, source.Length);
    return tokens.AsReadOnly();
  }

  internal static bool TryUnwrapDynamicArgument(string source, out string inner) {
    inner = null;
    if (source is not { Length: >= 2 } || source[0] != '(' || source[source.Length - 1] != ')') return false;
    inner = source.Substring(1, source.Length - 2);
    return true;
  }

  private static bool LexReference(
    string source, ref int position, ICollection<MixinExpressionToken> tokens
  ) {
    var start = position;
    tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.At, "@", position, ++position));
    var parenthesized = position < source.Length && source[position] == '(';
    if (parenthesized) {
      tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.OpenParenthesis, "(", position, position + 1));
      position++;
    }
    var nullRoot = position < source.Length && source[position] == ':';
    if (nullRoot) {
      tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.NullRoot, "", position, position + 1));
      position++;
    } else if (!LexIdentifier(source, ref position, MixinExpressionTokenKind.Identifier, tokens)) {
      position = start;
      return false;
    }
    if (!nullRoot && position < source.Length && source[position] == '#') {
      position++;
      if (!LexIdentifier(source, ref position, MixinExpressionTokenKind.Member, tokens)) return false;
    }
    if (nullRoot && position < source.Length &&
      (char.IsLetterOrDigit(source[position]) || source[position] == '_' || source[position] is '!' or '?')) {
      if (!LexProperty(source, ref position, tokens))
        return false;
    }
    while (position < source.Length && source[position] is ':' or '#') {
      var path = source[position++] == '#';
      if (path) {
        if (!LexIdentifier(source, ref position, MixinExpressionTokenKind.Path, tokens)) return false;
        continue;
      }
      if (!LexProperty(source, ref position, tokens)) return false;
    }
    if (!parenthesized) return true;
    if (position >= source.Length || source[position] != ')') return false;
    tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.CloseParenthesis, ")", position, position + 1));
    position++;
    return true;
  }

  private static bool LexProperty(
    string source, ref int position, ICollection<MixinExpressionToken> tokens
  ) {
    if (position < source.Length && source[position] == '!') {
      tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.Negation, "!", position, position + 1));
      position++;
    }
    if (position < source.Length && source[position] == '?') {
      tokens.Add(new MixinExpressionToken(MixinExpressionTokenKind.Predicate, "?", position, position + 1));
      position++;
    }
    if (!LexIdentifier(source, ref position, MixinExpressionTokenKind.Property, tokens)) return false;
    while (position < source.Length && source[position] == '<') {
      var argumentStart = ++position;
      var depth = 1;
      while (position < source.Length && depth != 0) {
        if (source[position] == '<') depth++;
        else if (source[position] == '>') depth--;
        position++;
      }
      if (depth != 0) return false;
      var argumentEnd = position - 1;
      tokens.Add(
        new MixinExpressionToken(
          MixinExpressionTokenKind.ArgumentStart, "<", argumentStart - 1, argumentStart
        )
      );
      if (argumentEnd - argumentStart >= 2 && source[argumentStart] == '(' &&
        source[argumentEnd - 1] == ')') {
        tokens.Add(
          new MixinExpressionToken(
            MixinExpressionTokenKind.ArgumentExpressionStart, "(", argumentStart, argumentStart + 1
          )
        );
        foreach (var token in LexExpression(
          source.Substring(
            argumentStart + 1, argumentEnd - argumentStart - 2
          )
        )) {
          tokens.Add(token with { Start = token.Start + argumentStart + 1, End = token.End + argumentStart + 1 });
        }
        tokens.Add(
          new MixinExpressionToken(
            MixinExpressionTokenKind.ArgumentExpressionEnd, ")", argumentEnd - 1, argumentEnd
          )
        );
      } else {
        tokens.Add(
          new MixinExpressionToken(
            MixinExpressionTokenKind.ArgumentLiteral,
            source.Substring(argumentStart, argumentEnd - argumentStart), argumentStart, argumentEnd
          )
        );
      }
      tokens.Add(
        new MixinExpressionToken(
          MixinExpressionTokenKind.ArgumentEnd, ">", argumentEnd, position
        )
      );
    }
    return true;
  }

  private static bool LexIdentifier(
    string source, ref int position, MixinExpressionTokenKind kind,
    ICollection<MixinExpressionToken> tokens
  ) {
    var start = position;
    while (position < source.Length && (char.IsLetterOrDigit(source[position]) || source[position] == '_')) position++;
    if (position == start) return false;
    tokens.Add(new MixinExpressionToken(kind, source.Substring(start, position - start), start, position));
    return true;
  }

  private static void AddLiteral(
    ICollection<MixinExpressionToken> tokens, string source, int start, int end
  ) {
    if (end > start) {
      tokens.Add(
        new MixinExpressionToken(MixinExpressionTokenKind.Literal, source.Substring(start, end - start), start, end)
      );
    }
  }

  internal static IReadOnlyList<MixinToken> Lex(string source) {
    var logical = SplitLogicalLines(source);
    var tokens = new List<MixinToken>();
    for (var index = 0; index < logical.Length; index++) LexLine(logical[index] ?? "", index + 1, tokens);
    return tokens.AsReadOnly();
  }

  internal static string[] SplitLogicalLines(string source) {
    var physical = (source ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    var logical = new string[physical.Length];
    var continuedLine = -1;
    for (var index = 0; index < physical.Length; index++) {
      var text = physical[index];
      var marker = SkipWhitespace(text, 0);
      if (marker + 1 < text.Length && text[marker] == '@') {
        var kind = text[marker + 1];
        if (kind == '#') {
          logical[index] = "";
          continuedLine = -1;
          continue;
        }
        if (kind is '\\' or '+') {
          if (continuedLine >= 0) {
            logical[continuedLine] += (kind == '\\' ? "\n" : "") + text.Substring(marker + 2);
            logical[index] = "";
          } else logical[index] = text;
          continue;
        }
      }
      logical[index] = text;
      continuedLine =
        marker + 1 < text.Length && text[marker] == '@' && (char.IsLetter(text[marker + 1]) || text[marker + 1] == '_')
          ? index
          : -1;
    }
    return logical;
  }

  internal static IReadOnlyList<MixinToken> LexLogicalLine(string text, int line) {
    var tokens = new List<MixinToken>();
    LexLine(text ?? "", line, tokens);
    return tokens.AsReadOnly();
  }

  private static void LexLine(string text, int line, ICollection<MixinToken> tokens) {
    var position = SkipWhitespace(text, 0);
    if (position == text.Length) {
      End(tokens, line);
      return;
    }
    if (text[position] != '@') {
      Invalid(tokens, text, line);
      return;
    }
    if (position + 1 < text.Length && text[position + 1] is '\\' or '+') {
      tokens.Add(new MixinToken(MixinTokenKind.OrphanContinuation, "", line));
      End(tokens, line);
      return;
    }
    tokens.Add(new MixinToken(MixinTokenKind.At, "@", line));
    var start = ++position;
    while (position < text.Length && (char.IsLetter(text[position]) || text[position] == '_')) position++;
    if (position == start) {
      Invalid(tokens, text, line);
      return;
    }
    tokens.Add(new MixinToken(MixinTokenKind.Identifier, text.Substring(start, position - start), line));
    while (position < text.Length && text[position] == '<') {
      var argumentStart = ++position;
      var depth = 1;
      while (position < text.Length && depth != 0) {
        if (text[position] == '<') depth++;
        else if (text[position] == '>') depth--;
        position++;
      }
      if (depth != 0) {
        Invalid(tokens, text, line);
        return;
      }
      tokens.Add(
        new MixinToken(
          MixinTokenKind.Argument, text.Substring(argumentStart, position - argumentStart - 1).Trim(), line
        )
      );
    }
    position = SkipWhitespace(text, position);
    tokens.Add(new MixinToken(MixinTokenKind.Operand, position == text.Length ? "" : text.Substring(position), line));
    End(tokens, line);
  }

  private static void Invalid(ICollection<MixinToken> tokens, string text, int line) {
    tokens.Add(new MixinToken(MixinTokenKind.Invalid, text, line));
    End(tokens, line);
  }

  private static void End(ICollection<MixinToken> tokens, int line) {
    tokens.Add(new MixinToken(MixinTokenKind.EndOfLine, "", line));
  }

  private static int SkipWhitespace(string text, int position) {
    while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
    return position;
  }
}