using System;
using HELIX.Prose;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public interface IProseField : IProseScope {
    string Path { get; }
    string Name { get; }
    object Formatter { get; }
  }

  /// <summary>A typed field description whose current value is owned by the destination form.</summary>
  public sealed class ProseField<T> : IProseField {
    public ProseField(string path, string name, IDatatype<T> datatype) {
      Path = path ?? string.Empty;
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A field name is required.", nameof(name));
      Name = name;
      Datatype = datatype ?? throw new ArgumentNullException(nameof(datatype));
    }

    public IDatatype<T> Datatype { get; }
    public string Path { get; }
    public string Name { get; }
    object IProseField.Formatter => Datatype;
  }

  /// <summary>Semantic prose belonging to a field. Writers may ignore parts they do not support.</summary>
  public interface IProseFieldPart : IProseScope { }

  public enum ProseFieldPartKind : byte {
    Label,
    Description,
    Tooltip,
    Prefix,
    Suffix,
    Before,
    Between,
    After
  }

  public sealed class ProseFieldPart : IProseFieldPart {
    public ProseFieldPart(ProseFieldPartKind kind) {
      Kind = kind;
    }

    public ProseFieldPartKind Kind { get; }
  }

  public sealed class ProseFieldLabelWidthModifier : IProseModifier {
    public ProseFieldLabelWidthModifier(Length width) {
      Width = width;
    }

    public Length Width { get; }
  }

  public sealed class ProseFullWidthModifier : IProseModifier {
    public static readonly ProseFullWidthModifier Default = new();
    private ProseFullWidthModifier() { }
  }

  public static class ProseFields {
    public static readonly ProseFieldPart Label = new(ProseFieldPartKind.Label);
    public static readonly ProseFieldPart Description = new(ProseFieldPartKind.Description);
    public static readonly ProseFieldPart Tooltip = new(ProseFieldPartKind.Tooltip);
    public static readonly ProseFieldPart Prefix = new(ProseFieldPartKind.Prefix);
    public static readonly ProseFieldPart Suffix = new(ProseFieldPartKind.Suffix);
    public static readonly ProseFieldPart Before = new(ProseFieldPartKind.Before);
    public static readonly ProseFieldPart Between = new(ProseFieldPartKind.Between);
    public static readonly ProseFieldPart After = new(ProseFieldPartKind.After);
    public static readonly ProseFullWidthModifier FullWidth = ProseFullWidthModifier.Default;

    public static ProseFieldLabelWidthModifier LabelWidth(Length width) {
      return new ProseFieldLabelWidthModifier(width);
    }
  }

  public static class ProseFieldWriterExtensions {
    public static ProseWriterScope Field<T>(
      this IProseWriter writer,
      string path,
      string name,
      IDatatype<T> datatype
    ) {
      return writer.Scope(new ProseField<T>(path, name, datatype));
    }

    public static ProseWriterScope FieldPart(this IProseWriter writer, IProseFieldPart part) {
      return writer.Scope(part);
    }

    public static ProseWriterScope FieldLabel(this IProseWriter writer) {
      return writer.Scope(ProseFields.Label);
    }

    public static ProseWriterScope FieldDescription(this IProseWriter writer) {
      return writer.Scope(ProseFields.Description);
    }

    public static ProseWriterScope FieldTooltip(this IProseWriter writer) {
      return writer.Scope(ProseFields.Tooltip);
    }

    public static ProseWriterScope FieldPrefix(this IProseWriter writer) {
      return writer.Scope(ProseFields.Prefix);
    }

    public static ProseWriterScope FieldSuffix(this IProseWriter writer) {
      return writer.Scope(ProseFields.Suffix);
    }

    public static ProseWriterScope FieldBefore(this IProseWriter writer) {
      return writer.Scope(ProseFields.Before);
    }

    public static ProseWriterScope FieldBetween(this IProseWriter writer) {
      return writer.Scope(ProseFields.Between);
    }

    public static ProseWriterScope FieldAfter(this IProseWriter writer) {
      return writer.Scope(ProseFields.After);
    }

    public static void FieldLabel(this IProseWriter writer, string text) {
      WritePart(writer, ProseFields.Label, text);
    }

    public static void FieldDescription(this IProseWriter writer, string text) {
      WritePart(writer, ProseFields.Description, text);
    }

    public static void FieldTooltip(this IProseWriter writer, string text) {
      WritePart(writer, ProseFields.Tooltip, text);
    }

    private static void WritePart(IProseWriter writer, IProseScope part, string text) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      using (writer.Scope(part)) writer.Write(text);
    }
  }
}