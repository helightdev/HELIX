using System;

namespace Hix.Compiler;

public sealed record HixToken {
  public HixToken(HixTokenKind kind, int start, int end)
    : this(kind, null, start, end) { }

  public HixToken(HixTokenKind kind, string text, int start, int end) {
    Kind = kind;
    Text = text;
    Line = 0;
    SourceRange = new HixSourceRange(start, end, 0, start);
    ContentRange = SourceRange;
  }

  public HixToken(
    HixTokenKind kind, string text, int line,
    HixSourceRange sourceRange, HixSourceRange contentRange
  ) {
    Kind = kind;
    Text = text;
    Line = line;
    SourceRange = sourceRange with { Line = line };
    ContentRange = contentRange with { Line = line };
  }

  public HixTokenKind Kind { get; }
  public string Text { get; }
  public int Line { get; }
  public HixSourceRange SourceRange { get; }
  public HixSourceRange ContentRange { get; }
  public int Start => SourceRange.Start;
  public int End => SourceRange.End;

  public HixToken Offset(int offset) => new(
    Kind, Text, Line,
    SourceRange with { Start = Start + offset, End = End + offset, Column = SourceRange.Column + offset },
    ContentRange with {
      Start = ContentRange.Start + offset, End = ContentRange.End + offset, Column = ContentRange.Column + offset
    }
  );

  public HixToken WithLocation(int line, int column) => new(
    Kind, Text, line,
    SourceRange with { Line = line, Column = column },
    ContentRange with { Line = line, Column = column + Math.Max(0, ContentRange.Start - SourceRange.Start) }
  );

  public HixToken WithTextAndLocation(string text, int line, int column) => new(
    Kind, text, line,
    SourceRange with { Line = line, Column = column },
    ContentRange with { Line = line, Column = column + Math.Max(0, ContentRange.Start - SourceRange.Start) }
  );

  public HixToken WithRange(HixSourceRange range) => new(
    Kind, Text, range.Line, range,
    range with {
      Start = range.Start + Math.Max(0, ContentRange.Start - SourceRange.Start),
      End = range.End - Math.Max(0, SourceRange.End - ContentRange.End),
      Column = range.Column + Math.Max(0, ContentRange.Start - SourceRange.Start)
    }
  );
}
