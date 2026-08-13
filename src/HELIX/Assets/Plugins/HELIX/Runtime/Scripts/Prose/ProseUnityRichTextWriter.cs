using System;
using System.Text;

namespace HELIX.Prose {
  /// <summary>
  /// Plain-text writer variant that projects semantic markup to Unity rich-text tags. Tags are emitted in
  /// the result but remain zero-width for wrapping, truncation, table measurement, and alignment.
  /// </summary>
  public sealed class ProseUnityRichTextWriter : ProseTextWriter {
    private readonly string _codeColorTag;
    private readonly string _quoteColorTag;
    private readonly string _errorColorTag;
    private readonly StringBuilder _tagBuilder = new();

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
      configuration ?? ProsePlainTextConfigurations.UnityRichText
    ) {
      CodeColor = NormalizeColor(codeColor, nameof(codeColor));
      QuoteColor = NormalizeColor(quoteColor, nameof(quoteColor));
      ErrorColor = NormalizeColor(errorColor, nameof(errorColor));
      _codeColorTag = ColorStart(CodeColor);
      _quoteColorTag = ColorStart(QuoteColor);
      _errorColorTag = ColorStart(ErrorColor);
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
          DecorateRangeZeroWidth(start, end, _codeColorTag, "</color>");
          break;
        case ProseTextStyle.Quote:
          _tagBuilder.Clear();
          _tagBuilder.Append("<i>");
          _tagBuilder.Append(_quoteColorTag);
          DecorateRangeZeroWidth(start, end, _tagBuilder, "</color></i>");
          break;
        case ProseTextStyle.Error:
          _tagBuilder.Clear();
          _tagBuilder.Append("<b>");
          _tagBuilder.Append(_errorColorTag);
          DecorateRangeZeroWidth(start, end, _tagBuilder, "</color></b>");
          break;
        default:
          base.ApplyTextStyle(style, start, end, matching);
          break;
      }
    }

    protected override void ApplyLink(string target, int start, int end, TextMatching matching) {
      _tagBuilder.Clear();
      _tagBuilder.Append("<link=\"");
      AppendEscapedAttribute(_tagBuilder, target);
      _tagBuilder.Append("\"><u>");
      DecorateRangeZeroWidth(start, end, _tagBuilder, "</u></link>");
    }

    protected override void ApplyCodeBlock(
      ProseCodeBlock codeBlock, int start, int end, TextMatching matching
    ) {
      base.ApplyCodeBlock(codeBlock, start, end, matching);
      DecorateRangeZeroWidth(start, OutputLength, _codeColorTag, "</color>");
    }

    private static string ColorStart(string color) => "<color=" + color + ">";

    private static string NormalizeColor(string color, string parameterName) {
      if (string.IsNullOrWhiteSpace(color)) throw new ArgumentException("A color is required.", parameterName);
      if (color[0] == '#') return color;
      if (color.Length is not (3 or 4 or 6 or 8)) return color;
      for (var i = 0; i < color.Length; i++)
        if (!IsHexDigit(color[i])) return color;
      return "#" + color;
    }

    private static bool IsHexDigit(char value) =>
      value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

    private static void AppendEscapedAttribute(StringBuilder builder, string value) {
      if (value == null) return;
      for (var i = 0; i < value.Length; i++) {
        switch (value[i]) {
          case '&': builder.Append("&amp;"); break;
          case '"': builder.Append("&quot;"); break;
          case '<': builder.Append("&lt;"); break;
          case '>': builder.Append("&gt;"); break;
          default: builder.Append(value[i]); break;
        }
      }
    }
  }
}
