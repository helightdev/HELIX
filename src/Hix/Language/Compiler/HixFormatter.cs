using System;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Hix.Compiler.Generated;

namespace Hix.Compiler;

public static class HixFormatter {
  public static string Format(string source, int indentSize = 2) {
    source ??= string.Empty;
    if (indentSize < 0) throw new ArgumentOutOfRangeException(nameof(indentSize));
    var lexer = new HixLexer(new AntlrInputStream(source));
    var tokens = lexer.GetAllTokens();
    var result = new StringBuilder();
    var depth = 0; var pendingLines = 0; IToken previous = null;
    for (var tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++) {
      var token = tokens[tokenIndex];
      if (IsWhitespace(token)) {
        var lines = token.Text.Count(character => character == '\n' || character == '\r') -
          token.Text.Count(character => character == '\r') / 2;
        if (lines != 0) pendingLines = Math.Min(2, pendingLines + Math.Max(1, lines));
        continue;
      }
      if (IsClosing(token.Type)) depth = Math.Max(0, depth - 1);
      if (token.Type == HixLexer.SECTION_DELIMITER) depth = 0;
      if (token.Type == HixLexer.SECTION_DELIMITER && pendingLines > 0) pendingLines = 1;
      if (pendingLines != 0) {
        TrimEnd(result);
        result.Append('\n', pendingLines);
        result.Append(' ', depth * indentSize);
        pendingLines = 0; previous = null;
      } else if (result.Length == 0 || result[result.Length - 1] == '\n') result.Append(' ', depth * indentSize);
      if (token.Type is HixLexer.COMMENT or HixLexer.SLASH_COMMENT) {
        if (result.Length != 0 && result[result.Length - 1] is not (' ' or '\n')) result.Append(' ');
        result.Append(token.Text.TrimEnd()); pendingLines = 1; previous = token; continue;
      }
      if (NeedsSpace(previous, token, result)) result.Append(' ');
      if (IsComma(token.Type) && result.Length != 0 && result[result.Length - 1] == ' ') result.Length--;
      result.Append(token.Text);
      if (token.Type == HixLexer.BEGIN_CONTENT) {
        var end = tokenIndex + 1;
        while (end < tokens.Count && tokens[end].Type != HixLexer.TERMINATOR) end++;
        var stop = end < tokens.Count ? tokens[end].StopIndex + 1 : source.Length;
        var start = token.StopIndex + 1;
        if (stop > start) result.Append(source.Substring(start, stop - start));
        tokenIndex = end < tokens.Count ? end : tokens.Count - 1;
        pendingLines = 0; previous = null;
        continue;
      }
      if (IsOpening(token.Type)) depth++;
      previous = token;
    }
    TrimEnd(result);
    return result.Length == 0 ? string.Empty : result.Append('\n').ToString();
  }

  private static bool IsWhitespace(IToken token) => token.Type is HixLexer.OUTER_WHITESPACE or HixLexer.NEWLINE or
    HixLexer.METADATA_WHITESPACE or HixLexer.DERIVATION_WHITESPACE or HixLexer.VALUE_WHITESPACE;
  private static bool IsOpening(int type) => type is HixLexer.LC or HixLexer.BEGIN_TABLE or HixLexer.BEGIN_TUPLE or
    HixLexer.BEGIN_INLINE_BLOCK or HixLexer.BEGIN_LAMBDA_BLOCK or HixLexer.BEGIN_PARAMETERS or HixLexer.BEGIN_VALUE_INLINE or
    HixLexer.BEGIN_METADATA_VALUE;
  private static bool IsClosing(int type) => type is HixLexer.RC or HixLexer.END_PARAMETERS or
    HixLexer.VALUE_END_INLINE;
  private static bool IsComma(int type) => type is HixLexer.COMMA or HixLexer.VALUE_DELIMITER;
  private static bool NeedsSpace(IToken previous, IToken current, StringBuilder result) {
    if (previous == null || result.Length == 0 || result[result.Length - 1] is ' ' or '\n') return false;
    if (previous.Type is HixLexer.BEGIN_ARGUMENT or HixLexer.ARGUMENT_TEXT or HixLexer.ESCAPE or
        HixLexer.ESCAPE_MACRO or HixLexer.ESCAPE_LITERAL or HixLexer.ESCAPE_HEX or HixLexer.BEGIN_CONTENT or
        HixLexer.CONTENT_TEXT or HixLexer.CONTENT_WRAP or HixLexer.CONTENT_LINEBREAK ||
        current.Type is HixLexer.ARGUMENT_END or HixLexer.ARGUMENT_TEXT or HixLexer.ESCAPE_MACRO or
        HixLexer.ESCAPE_LITERAL or HixLexer.ESCAPE_HEX or HixLexer.CONTENT_TEXT or HixLexer.CONTENT_WRAP or
        HixLexer.CONTENT_LINEBREAK or HixLexer.TERMINATOR) return false;
    if (IsComma(current.Type) || IsClosing(current.Type) || current.Type is HixLexer.SEMICOLON or HixLexer.VALUE_END or
        HixLexer.QUESTION or HixLexer.TOPLEVEL_QUESTION or HixLexer.VALUE_MEMBER or
        HixLexer.VALUE_FUNCTION or HixLexer.VALUE_PREDICATE or HixLexer.LABEL_PREFIX) return false;
    if (IsOpening(previous.Type) || previous.Type is HixLexer.METADATA_PREFIX or HixLexer.VALUE_MEMBER or
        HixLexer.VALUE_FUNCTION or HixLexer.VALUE_PREDICATE or HixLexer.VALUE_SMART_ROOT or
        HixLexer.LABEL_PREFIX) return false;
    if (current.Type is HixLexer.BEGIN_PARAMETERS or HixLexer.EMPTY_PARAMETERS) return false;
    if (current.Type == HixLexer.BEGIN_ARGUMENT && (previous.Type is HixLexer.IDENTIFIER or
        HixLexer.NAMESPACE_IDENTIFIER or HixLexer.ROOT_IDENTIFIER or HixLexer.FUNCTION_IDENTIFIER or
        HixLexer.ARGUMENT_END)) return false;
    if (current.Type is HixLexer.ASSIGN or HixLexer.VALUE_ASSIGN or HixLexer.ARROW or HixLexer.FAT_ARROW ||
        previous.Type is HixLexer.ASSIGN or HixLexer.VALUE_ASSIGN or HixLexer.ARROW or HixLexer.FAT_ARROW) return true;
    if (IsComma(previous.Type)) return true;
    return true;
  }
  private static void TrimEnd(StringBuilder result) {
    while (result.Length != 0 && char.IsWhiteSpace(result[result.Length - 1])) result.Length--;
  }
}
