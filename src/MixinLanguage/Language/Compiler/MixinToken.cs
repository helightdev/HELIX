using System;

namespace Mixins.Compiler;

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
    SourceRange = sourceRange with { Line = line };
    ContentRange = contentRange with { Line = line };
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
