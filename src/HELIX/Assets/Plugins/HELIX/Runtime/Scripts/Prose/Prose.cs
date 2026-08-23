using System;
using HELIX.Datatypes;

namespace HELIX.Prose {
  /// <summary>Owns a frame begun through <see cref="ProseWriterExtensions.Scope"/>.</summary>
  public struct ProseWriterScope : IDisposable {
    private IProseWriter _writer;

    internal ProseWriterScope(IProseWriter writer) => _writer = writer;

    public void Dispose() {
      var writer = _writer;
      _writer = null;
      writer?.End();
    }
  }

  /// <summary>Low-boilerplate immediate-mode producers for the built-in semantic frames.</summary>
  public static class ProseWriterExtensions {
    /// <summary>
    /// Begins a frame that is ended automatically when the returned scope is disposed. Unlike the TryBegin
    /// helpers, this always retains a balanced frame, including frames ignored by the writer.
    /// </summary>
    public static ProseWriterScope Scope(this IProseWriter writer, IProseScope scope) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      writer.BeginFrame(scope);
      return new ProseWriterScope(writer);
    }

    public static ProseWriterScope Span(
      this IProseWriter writer, ProseTextStyle style = ProseTextStyle.None, string linkTarget = null
    ) {
      var scope = writer.Scope(ProseScopes.Span);
      if (style != ProseTextStyle.None) writer.PushModifier(ProseModifiers.TextStyle(style));
      if (linkTarget != null) writer.PushModifier(new ProseLinkModifier(linkTarget));
      return scope;
    }

    public static ProseWriterScope Section(this IProseWriter writer) => writer.Scope(ProseScopes.Section);
    public static ProseWriterScope SectionHeader(this IProseWriter writer) => writer.Scope(ProseScopes.SectionHeader);
    public static ProseWriterScope Paragraph(this IProseWriter writer) => writer.Scope(ProseScopes.Paragraph);
    public static ProseWriterScope CodeBlock(this IProseWriter writer, string language = null) =>
      writer.Scope(string.IsNullOrEmpty(language) ? ProseScopes.PlainCodeBlock : new ProseCodeBlock(language));

    public static ProseWriterScope List(
      this IProseWriter writer, ProseListKind kind = ProseListKind.Unordered, int start = 1
    ) => writer.Scope(
      start == 1
        ? kind == ProseListKind.Ordered ? ProseScopes.OrderedList : ProseScopes.UnorderedList
        : new ProseList(kind, start)
    );

    public static ProseWriterScope OrderedList(this IProseWriter writer, int start = 1) =>
      writer.List(ProseListKind.Ordered, start);

    public static ProseWriterScope UnorderedList(this IProseWriter writer) => writer.List();
    public static ProseWriterScope ListItem(this IProseWriter writer) => writer.Scope(ProseScopes.ListItem);
    public static ProseWriterScope Table(this IProseWriter writer) => writer.Scope(ProseScopes.Table);
    public static ProseWriterScope TableRow(this IProseWriter writer, bool header = false) =>
      writer.Scope(header ? ProseScopes.TableHeaderRow : ProseScopes.TableBodyRow);

    public static ProseWriterScope TableCell(
      this IProseWriter writer, ProseTextAlignment alignment = ProseTextAlignment.Left
    ) {
      var scope = writer.Scope(ProseScopes.TableCell);
      if (alignment != ProseTextAlignment.Left) writer.PushModifier(ProseModifiers.Alignment(alignment));
      return scope;
    }

    public static ProseWriterScope Property(
      this IProseWriter writer,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false,
      bool hideName = false,
      bool hideSeparator = false,
      bool isDefaultValue = false
    ) {
      var scope = writer.Scope(ProseScopes.Property);
      writer.PushModifier(ProseModifiers.Level(level));
      if (hidden) writer.PushModifier(ProseModifiers.Hidden);
      if (noWrap) writer.PushModifier(ProseModifiers.NoWrap);
      if (hideName) writer.PushModifier(ProseModifiers.HideName);
      if (hideSeparator) writer.PushModifier(ProseModifiers.HideSeparator);
      if (isDefaultValue) writer.PushModifier(ProseModifiers.DefaultValue);
      return scope;
    }

    public static ProseWriterScope Tree(this IProseWriter writer) => writer.Scope(ProseScopes.Tree);
    public static ProseWriterScope Name(this IProseWriter writer) => writer.Scope(ProseScopes.Name);
    public static ProseWriterScope PropertyKey(this IProseWriter writer) => writer.Scope(ProseScopes.PropertyKey);
    public static ProseWriterScope PropertyValue(this IProseWriter writer) => writer.Scope(ProseScopes.PropertyValue);
    public static ProseWriterScope PropertyDescription(this IProseWriter writer) =>
      writer.Scope(ProseScopes.PropertyDescription);

    public static bool TryBeginProperty(
      this IProseWriter writer,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false,
      bool hideName = false,
      bool hideSeparator = false,
      bool isDefaultValue = false
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.TryBeginFrame(ProseScopes.Property)) return false;
      writer.PushModifier(ProseModifiers.Level(level));
      if (hidden) writer.PushModifier(ProseModifiers.Hidden);
      if (noWrap) writer.PushModifier(ProseModifiers.NoWrap);
      if (hideName) writer.PushModifier(ProseModifiers.HideName);
      if (hideSeparator) writer.PushModifier(ProseModifiers.HideSeparator);
      if (isDefaultValue) writer.PushModifier(ProseModifiers.DefaultValue);
      return true;
    }

    public static bool TryBeginTree(this IProseWriter writer) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      return writer.TryBeginFrame(ProseScopes.Tree);
    }

    public static void WriteSpan(
      this IProseWriter writer, string text, ProseTextStyle style = ProseTextStyle.None,
      string linkTarget = null
    ) {
      using (writer.Span(style, linkTarget)) {
        writer.Write(text);
      }
    }

    public static void WriteSectionHeader(this IProseWriter writer, string header) =>
      WriteTextFrame(writer, ProseScopes.SectionHeader, header);

    public static void WriteParagraph(this IProseWriter writer, string content) =>
      WriteTextFrame(writer, ProseScopes.Paragraph, content);

    public static void WriteCodeBlock(this IProseWriter writer, string code, string language = null) =>
      WriteTextFrame(writer, new ProseCodeBlock(language), code);

    public static void WriteListItem(this IProseWriter writer, string content) =>
      WriteTextFrame(writer, ProseScopes.ListItem, content);

    public static void WriteListItem<T>(
      this IProseWriter writer, T value, IProseDatatype<T> datatype
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      using (writer.ListItem()) writer.Write(value, datatype);
    }

    public static void WriteTableCell(
      this IProseWriter writer, string content,
      ProseTextAlignment alignment = ProseTextAlignment.Left
    ) => writer.WriteTableCell(content, ProseDatatypes.String, alignment);

    public static void WriteTableCell<T>(
      this IProseWriter writer, T value, IProseDatatype<T> datatype,
      ProseTextAlignment alignment = ProseTextAlignment.Left
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      using (writer.TableCell(alignment)) {
        writer.Write(value, datatype);
      }
    }

    public static void Name(this IProseWriter writer, string name) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      using (writer.Name()) writer.Write(name);
    }

    public static void Description(this IProseWriter writer, string description) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      using (writer.PropertyDescription()) writer.Write(description);
    }

    public static void Property<T>(
      this IProseWriter writer,
      string key,
      T value,
      IProseDatatype<T> datatype,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false,
      bool hideName = false,
      bool hideSeparator = false,
      string description = null,
      object defaultValue = null
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      var isDefaultValue = defaultValue != null && Equals(value, defaultValue);
      if (!writer.TryBeginProperty(level, hidden, noWrap, hideName, hideSeparator, isDefaultValue)) return;
      try {
        if (description != null) writer.PushModifier(new ProsePropertyValueModifier(value));

        using (writer.PropertyKey()) writer.Write(key);

        if (description != null) {
          writer.Description(description);
        } else using (writer.PropertyValue()) writer.Write(value, datatype);
      } finally {
        writer.End();
      }
    }

    private static void WriteTextFrame(IProseWriter writer, IProseScope scope, string text) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      using (writer.Scope(scope)) writer.Write(text);
    }
  }
}
