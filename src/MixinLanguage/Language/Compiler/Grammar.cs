using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler;

public enum HixSyntaxKind {
  Document,
  Declaration,
  Statement,
  Reference,
  ParenthesizedReference,
  Root,
  Member,
  Path,
  FunctionCall,
  LiteralArgument,
  ExpressionArgument,
  Comment,
  Continuation,
  Escape,
  Punctuation,
  Error
}

public enum HixTokenKind {
  At,
  Identifier,
  Number,
  Boolean,
  Null,
  Hash,
  FunctionOperator,
  BooleanCallOperator,
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

public readonly record struct HixSourceRange(
  int Start, int End, int Line = 0, int Column = 0
) {
  public int Length => End - Start;
  public bool IsEmpty => Length == 0;

  public static HixSourceRange Compose(IEnumerable<HixSourceRange> ranges) {
    var values = (ranges ?? []).Where(range => !range.IsEmpty).ToArray();
    if (values.Length == 0) return default;
    var first = values.OrderBy(range => range.Start).First();
    return new HixSourceRange(
      values.Min(range => range.Start), values.Max(range => range.End), first.Line, first.Column
    );
  }
}
