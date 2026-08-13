using System;

namespace HELIX.Prose {
  /// <summary>Low-boilerplate immediate-mode producers for the built-in semantic frames.</summary>
  public static class ProseWriterExtensions {
    public static bool BeginSpan(
      this IProseWriter writer, ProseTextStyle style = ProseTextStyle.None,
      string linkTarget = null
    ) {
      if (!BeginScope(writer, ProseSpan.Instance)) return false;
      if (style != ProseTextStyle.None) writer.PushModifier(TextStyleMarker.For(style));
      if (linkTarget != null) writer.PushModifier(new LinkMarker(linkTarget));
      return true;
    }

    public static bool BeginSection(this IProseWriter writer) =>
      BeginScope(writer, ProseSection.Instance);

    public static bool BeginSectionHeader(this IProseWriter writer) =>
      BeginScope(writer, ProseSectionHeader.Instance);

    public static bool BeginParagraph(this IProseWriter writer) =>
      BeginScope(writer, ProseParagraph.Instance);

    public static bool BeginCodeBlock(this IProseWriter writer, string language = null) =>
      BeginScope(writer, string.IsNullOrEmpty(language) ? ProseCodeBlock.Plain : new ProseCodeBlock(language));

    public static bool BeginList(
      this IProseWriter writer, ProseListKind kind = ProseListKind.Unordered, int start = 1
    ) => BeginScope(
      writer,
      start == 1
        ? kind == ProseListKind.Ordered ? ProseList.Ordered : ProseList.Unordered
        : new ProseList(kind, start)
    );

    public static bool BeginOrderedList(this IProseWriter writer, int start = 1) =>
      writer.BeginList(ProseListKind.Ordered, start);

    public static bool BeginUnorderedList(this IProseWriter writer) =>
      writer.BeginList();

    public static bool BeginListItem(this IProseWriter writer) =>
      BeginScope(writer, ProseListItem.Instance);

    public static bool BeginTable(this IProseWriter writer) =>
      BeginScope(writer, ProseTable.Instance);

    public static bool BeginTableRow(this IProseWriter writer, bool header = false) =>
      BeginScope(writer, header ? ProseTableRow.Header : ProseTableRow.Body);

    public static bool BeginTableCell(
      this IProseWriter writer, ProseTextAlignment alignment = ProseTextAlignment.Left
    ) {
      if (!BeginScope(writer, ProseTableCell.Instance)) return false;
      if (alignment != ProseTextAlignment.Left)
        writer.PushModifier(TextAlignmentMarker.For(alignment));
      return true;
    }

    public static bool BeginProperty(
      this IProseWriter writer,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false,
      bool hideName = false,
      bool hideSeparator = false,
      bool isDefaultValue = false
    ) {
      if (!BeginScope(writer, ProseProperty.Instance)) return false;
      writer.PushModifier(LevelMarker.For(level));
      if (hidden) writer.PushModifier(Hidden.Instance);
      if (noWrap) writer.PushModifier(NoWrap.Instance);
      if (hideName) writer.PushModifier(HideName.Instance);
      if (hideSeparator) writer.PushModifier(HideSeparator.Instance);
      if (isDefaultValue) writer.PushModifier(DefaultValue.Instance);
      return true;
    }

    public static bool BeginTree(this IProseWriter writer) =>
      BeginScope(writer, ProseTree.Instance);

    public static bool BeginName(this IProseWriter writer) =>
      BeginScope(writer, ProseName.Instance);

    public static bool BeginPropertyKey(this IProseWriter writer) =>
      BeginScope(writer, ProsePropertyKey.Instance);

    public static bool BeginPropertyValue(this IProseWriter writer) =>
      BeginScope(writer, ProsePropertyValue.Instance);

    public static bool BeginPropertyDescription(this IProseWriter writer) =>
      BeginScope(writer, ProsePropertyDescription.Instance);

    public static void WriteSpan(
      this IProseWriter writer, string text, ProseTextStyle style = ProseTextStyle.None,
      string linkTarget = null
    ) {
      if (!writer.BeginSpan(style, linkTarget)) return;
      try {
        writer.Write(text);
      } finally {
        writer.End();
      }
    }

    public static void WriteSectionHeader(this IProseWriter writer, string header) =>
      WriteTextFrame(writer, ProseSectionHeader.Instance, header);

    public static void WriteParagraph(this IProseWriter writer, string content) =>
      WriteTextFrame(writer, ProseParagraph.Instance, content);

    public static void WriteCodeBlock(this IProseWriter writer, string code, string language = null) =>
      WriteTextFrame(writer, new ProseCodeBlock(language), code);

    public static void WriteListItem(this IProseWriter writer, string content) =>
      WriteTextFrame(writer, ProseListItem.Instance, content);

    public static void WriteListItem<T>(
      this IProseWriter writer, T value, IProseFormatter<T> formatter
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      if (!writer.BeginListItem()) return;
      try { writer.Write(value, formatter); } finally { writer.End(); }
    }

    public static void WriteTableCell(
      this IProseWriter writer, string content,
      ProseTextAlignment alignment = ProseTextAlignment.Left
    ) => writer.WriteTableCell(content, ProseFormatters.String, alignment);

    public static void WriteTableCell<T>(
      this IProseWriter writer, T value, IProseFormatter<T> formatter,
      ProseTextAlignment alignment = ProseTextAlignment.Left
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      if (!writer.BeginTableCell(alignment)) return;
      try {
        writer.Write(value, formatter);
      } finally {
        writer.End();
      }
    }

    public static void Name(this IProseWriter writer, string name) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginName()) return;
      try { writer.Write(name); } finally { writer.End(); }
    }

    public static void Description(this IProseWriter writer, string description) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginPropertyDescription()) return;
      try { writer.Write(description); } finally { writer.End(); }
    }

    public static void Property<T>(
      this IProseWriter writer,
      string key,
      T value,
      IProseFormatter<T> formatter,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false,
      bool hideName = false,
      bool hideSeparator = false,
      string description = null,
      object defaultValue = null
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      var isDefaultValue = defaultValue != null && Equals(value, defaultValue);
      if (!writer.BeginProperty(level, hidden, noWrap, hideName, hideSeparator, isDefaultValue)) return;
      try {
        if (description != null) writer.PushModifier(new PropertyValueMarker(value));

        if (writer.BeginPropertyKey()) {
          try { writer.Write(key); } finally { writer.End(); }
        }

        if (description != null) {
          writer.Description(description);
        } else if (writer.BeginPropertyValue()) {
          try { writer.Write(value, formatter); } finally { writer.End(); }
        }
      } finally {
        writer.End();
      }
    }

    private static void WriteTextFrame(IProseWriter writer, IProseScope scope, string text) {
      if (!BeginScope(writer, scope)) return;
      try { writer.Write(text); } finally { writer.End(); }
    }

    private static bool BeginScope(IProseWriter writer, IProseScope scope) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      return writer.BeginFrame(scope);
    }
  }
}
