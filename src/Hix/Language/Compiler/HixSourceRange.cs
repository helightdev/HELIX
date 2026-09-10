using System.Collections.Generic;
using Antlr4.Runtime;

namespace Hix.Compiler;

/// <summary>A half-open source span, independent of syntax and execution representations.</summary>
public readonly record struct HixSourceRange(int Start, int End, int Line = 0, int Column = 0) {
  public int Length => End - Start;
  public bool IsEmpty => Length == 0;
  public static HixSourceRange FromToken(IToken token) =>
    new(token.StartIndex, token.StopIndex + 1, token.Line, token.Column);

  public static HixSourceRange Compose(IEnumerable<HixSourceRange> ranges) {
    if (ranges == null) return default;
    var result = default(HixSourceRange);
    var found = false;
    foreach (var range in ranges) {
      if (range.IsEmpty) continue;
      if (!found) { result = range; found = true; continue; }
      var end = System.Math.Max(result.End, range.End);
      result = range.Start < result.Start ? range with { End = end } : result with { End = end };
    }
    return result;
  }
}
