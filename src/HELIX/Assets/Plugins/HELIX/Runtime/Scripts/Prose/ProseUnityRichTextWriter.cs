using System;
using System.Text;

namespace HELIX.Prose {
  /// <summary>
  /// Plain-text writer variant that projects semantic markup to Unity rich-text tags. Tags are emitted in
  /// the result but remain zero-width for wrapping, truncation, table measurement, and alignment.
  /// </summary>
  public sealed class ProseUnityRichTextWriter : ProsePlainTextWriter {
    public ProseUnityRichTextWriter(
      int wrapWidth = 100,
      ProseLevel minimumLevel = ProseLevel.Debug,
      int maxTruncatableFrameLength = -1,
      int initialCapacity = 256,
      int initialFrameCapacity = 16,
      ProsePlainTextConfiguration configuration = null,
      string codeColor = "#DCDCAA",
      string quoteColor = "#A0A0A0",
      string errorColor = "#FF6B6B"
    ) : base(
      wrapWidth, minimumLevel, maxTruncatableFrameLength,
      initialCapacity, initialFrameCapacity,
      configuration ?? ProsePlainTextConfigurations.Plain
    ) {
      CodeColor = NormalizeColor(codeColor, nameof(codeColor));
      QuoteColor = NormalizeColor(quoteColor, nameof(quoteColor));
      ErrorColor = NormalizeColor(errorColor, nameof(errorColor));
    }

    public string CodeColor { get; }
    public string QuoteColor { get; }
    public string ErrorColor { get; }

    protected override void ApplyTextStyle(
      ProseTextStyle style, int start, int end, TextMatching matching
    ) {
      switch (style) {
        case ProseTextStyle.Emphasis:
          DecorateRangeZeroWidth(start, end, "<i>", "</i>");
          break;
        case ProseTextStyle.Strong:
          DecorateRangeZeroWidth(start, end, "<b>", "</b>");
          break;
        case ProseTextStyle.Code:
          DecorateRangeZeroWidth(start, end, ColorStart(CodeColor), "</color>");
          break;
        case ProseTextStyle.Quote:
          DecorateRangeZeroWidth(start, end, "<i>" + ColorStart(QuoteColor), "</color></i>");
          break;
        case ProseTextStyle.Error:
          DecorateRangeZeroWidth(start, end, "<b>" + ColorStart(ErrorColor), "</color></b>");
          break;
        default:
          base.ApplyTextStyle(style, start, end, matching);
          break;
      }
    }

    protected override void ApplyLink(string target, int start, int end, TextMatching matching) {
      DecorateRangeZeroWidth(
        start, end,
        "<link=\"" + EscapeAttribute(target) + "\"><u>",
        "</u></link>"
      );
    }

    protected override void ApplyCodeBlock(
      ProseCodeBlock codeBlock, int start, int end, TextMatching matching
    ) {
      ApplyFormat(Configuration.CodeBlock, start, end, matching);
      DecorateRangeZeroWidth(start, OutputLength, ColorStart(CodeColor), "</color>");
    }

    private static string ColorStart(string color) => "<color=" + color + ">";

    private static string NormalizeColor(string color, string parameterName) {
      if (string.IsNullOrWhiteSpace(color)) throw new ArgumentException("A color is required.", parameterName);
      if (color[0] == '#') return color;
      if (color.Length is not (3 or 4 or 6 or 8)) return color;
      for (var i = 0; i < color.Length; i++)
        if (!Uri.IsHexDigit(color[i])) return color;
      return "#" + color;
    }

    private static string EscapeAttribute(string value) {
      if (value == null) return string.Empty;
      var builder = new StringBuilder(value.Length);
      for (var i = 0; i < value.Length; i++) {
        switch (value[i]) {
          case '&': builder.Append("&amp;"); break;
          case '"': builder.Append("&quot;"); break;
          case '<': builder.Append("&lt;"); break;
          case '>': builder.Append("&gt;"); break;
          default: builder.Append(value[i]); break;
        }
      }
      return builder.ToString();
    }
  }
}
