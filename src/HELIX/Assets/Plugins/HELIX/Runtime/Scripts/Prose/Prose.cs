using System;

namespace HELIX.Prose {
  /// <summary>Low-boilerplate immediate-mode producers for the built-in semantic frames.</summary>
  public static class Prose {
    public static void WriteSpan(
      IProseWriter writer, string text, ProseTextStyle style = ProseTextStyle.None,
      string linkTarget = null
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginFrame(ProseSpan.Instance)) return;
      try {
        if (style != ProseTextStyle.None) writer.PushModifier(TextStyleMarker.For(style));
        if (linkTarget != null) writer.PushModifier(new LinkMarker(linkTarget));
        writer.Write(text);
      } finally {
        writer.PopFrame();
      }
    }

    public static void WriteSectionHeader(IProseWriter writer, string header) =>
      WriteTextFrame(writer, ProseSectionHeader.Instance, header);

    public static void WriteParagraph(IProseWriter writer, string content) =>
      WriteTextFrame(writer, ProseParagraph.Instance, content);

    public static void WriteCodeBlock(IProseWriter writer, string code, string language = null) =>
      WriteTextFrame(writer, new ProseCodeBlock(language), code);

    public static void WriteListItem(IProseWriter writer, string content) =>
      WriteTextFrame(writer, ProseListItem.Instance, content);

    public static void WriteListItem<T>(IProseWriter writer, T value, IProseFormatter<T> formatter) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      if (!writer.BeginFrame(ProseListItem.Instance)) return;
      try { writer.Write(value, formatter); } finally { writer.PopFrame(); }
    }

    public static void WriteTableCell(
      IProseWriter writer, string content, ProseTextAlignment alignment = ProseTextAlignment.Left
    ) => WriteTableCell(writer, content, ProseStringFormatter.Instance, alignment);

    public static void WriteTableCell<T>(
      IProseWriter writer, T value, IProseFormatter<T> formatter,
      ProseTextAlignment alignment = ProseTextAlignment.Left
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      if (!writer.BeginFrame(ProseTableCell.Instance)) return;
      try {
        if (alignment != ProseTextAlignment.Left)
          writer.PushModifier(TextAlignmentMarker.For(alignment));
        writer.Write(value, formatter);
      } finally {
        writer.PopFrame();
      }
    }

    public static void WriteName(IProseWriter writer, string name) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginFrame(ProseName.Instance)) return;
      try { writer.Write(name); } finally { writer.PopFrame(); }
    }

    public static void WriteProperty<T>(
      IProseWriter writer,
      string key,
      T value,
      IProseFormatter<T> formatter,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginFrame(ProseProperty.Instance)) return;
      try {
        writer.PushModifier(LevelMarker.For(level));
        if (hidden) writer.PushModifier(Hidden.Instance);
        if (noWrap) writer.PushModifier(NoWrap.Instance);

        if (writer.BeginFrame(ProsePropertyKey.Instance)) {
          try { writer.Write(key); } finally { writer.PopFrame(); }
        }

        if (writer.BeginFrame(ProsePropertyValue.Instance)) {
          try { writer.Write(value, formatter); } finally { writer.PopFrame(); }
        }
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteTextFrame(IProseWriter writer, IProseScope scope, string text) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginFrame(scope)) return;
      try { writer.Write(text); } finally { writer.PopFrame(); }
    }
  }
}
