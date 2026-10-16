using UnityEngine.UIElements;

namespace HELIX.Prose {
  /// <summary>A vertically arranged collection of inspector or option fields.</summary>
  public sealed class ProseForm : IProseScope {
    public ProseForm(string role = null) => Role = role ?? string.Empty;
    public string Role { get; }
  }

  /// <summary>A descriptive inspector or option field. Its role hints at the desired control kind.</summary>
  public sealed class ProseField : IProseScope {
    public ProseField(string role = null) => Role = role ?? string.Empty;
    public string Role { get; }
  }

  /// <summary>The conventional horizontal label, value, and inline-decorator region of a field.</summary>
  public sealed class ProseFieldRow : IProseScope {
    public static readonly ProseFieldRow Default = new();
    private ProseFieldRow() { }
  }

  /// <summary>A field label column. Null width asks the writer to use its shared default.</summary>
  public sealed class ProseFieldLabel : IProseScope {
    public ProseFieldLabel(Length? width = null) => Width = width;
    public Length? Width { get; }
  }

  /// <summary>The flexible value column of a conventional field row.</summary>
  public sealed class ProseFieldValue : IProseScope {
    public static readonly ProseFieldValue Default = new();
    private ProseFieldValue() { }
  }

  /// <summary>Field content that deliberately bypasses the aligned two-column row.</summary>
  public sealed class ProseFieldFullWidth : IProseScope {
    public ProseFieldFullWidth(string role = null) => Role = role ?? string.Empty;
    public string Role { get; }
  }

  public static class ProseFormWriterExtensions {
    public static ProseWriterScope Form(this IProseWriter writer, string role = null) =>
      writer.Scope(new ProseForm(role));

    public static ProseWriterScope Field(this IProseWriter writer, string role = null) =>
      writer.Scope(new ProseField(role));

    public static ProseWriterScope FieldRow(this IProseWriter writer) => writer.Scope(ProseFieldRow.Default);

    public static ProseWriterScope FieldLabel(
      this IProseWriter writer, Length? width = null
    ) => writer.Scope(new ProseFieldLabel(width));

    public static void FieldLabel(
      this IProseWriter writer, string text, Length? width = null
    ) {
      using (writer.FieldLabel(width)) writer.Write(text);
    }

    public static ProseWriterScope FieldValue(this IProseWriter writer) => writer.Scope(ProseFieldValue.Default);

    public static ProseWriterScope FieldFullWidth(this IProseWriter writer, string role = null) =>
      writer.Scope(new ProseFieldFullWidth(role));

    public static void FieldDescription(this IProseWriter writer, string text) {
      using (writer.FieldFullWidth("description")) writer.Write(text);
    }
  }
}
