using System.Collections.Generic;

namespace HelixSourceGenerator.Language.Compiler;

internal enum MixinTokenKind { At, Identifier, Argument, Operand, EndOfLine, Invalid }

internal sealed record MixinToken(MixinTokenKind Kind, string Text, int Line);

/// <summary>Turns source text into tokens without applying directive semantics.</summary>
internal static class MixinExpressionLexer {
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

  private static void End(ICollection<MixinToken> tokens, int line) =>
    tokens.Add(new MixinToken(MixinTokenKind.EndOfLine, "", line));

  private static int SkipWhitespace(string text, int position) {
    while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
    return position;
  }
}