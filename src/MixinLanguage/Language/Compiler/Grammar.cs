using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler;

public enum MixinSyntaxKind {
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

public enum MixinTokenKind {
  At,
  Identifier,
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

public readonly record struct MixinSourceRange(
  int Start, int End, int Line = 0, int Column = 0
) {
  public int Length => End - Start;
  public bool IsEmpty => Length == 0;

  public static MixinSourceRange Compose(IEnumerable<MixinSourceRange> ranges) {
    var values = (ranges ?? []).Where(range => !range.IsEmpty).ToArray();
    if (values.Length == 0) return default;
    var first = values.OrderBy(range => range.Start).First();
    return new MixinSourceRange(
      values.Min(range => range.Start), values.Max(range => range.End), first.Line, first.Column
    );
  }
}
