using System;
using HELIX.Prose;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public interface IProseField : IProseScope {
    string Path { get; }
    string Name { get; }
    object Formatter { get; }

    /// <summary>
    /// The control for this field's value, resolved through <see cref="SpecConfig" /> rather than by a
    /// per-type factory. The composable reads and writes the enclosing <see cref="HXFormField" />.
    /// </summary>
    Composable CreateControl();

    /// <summary>The datatype's declared default, boxed, or false when it declares none.</summary>
    bool TryGetDefaultValue(out object value);
  }

  /// <summary>A typed field description whose current value is owned by the destination form.</summary>
  public sealed class ProseField<T> : IProseField {
    private static readonly CompositionAction<T> Setter = SetValue;
    private static readonly CompositionAction Committer = Commit;
    private Composable _control;

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

    public Composable CreateControl() => _control ??= ComposeControl;

    public bool TryGetDefaultValue(out object value) {
      if (Datatype is IDatatypeDefault<T> { HasDefaultValue: true } declared) {
        value = declared.DefaultValue;
        return true;
      }
      value = null;
      return false;
    }

    private void ComposeControl(ref Composition cx) {
      var field = cx.Lookup<HXFormField>();
      var data = field?.FieldData;
      cx.Spec(
        new ControlSpec<T>(
          field == null ? default : field.GetValue<T>(),
          Datatype,
          Setter,
          Committer,
          data?.HasFlag(FieldFlags.Disabled) != true,
          data?.HasFlag(FieldFlags.Error) == true
        )
      );
      cx.CURSOR.Flexible();
    }

    private static void SetValue(CompositionContext context, T value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);

    private static void Commit(CompositionContext context) =>
      context.Lookup<HXFormField>()?.MarkFinishedEditing();
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
    /// <summary>
    ///   Begins a field frame. The optional arguments cover the decorations a field usually wants, so a
    ///   field that needs nothing else does not need a block; keep the block for nested prose.
    /// </summary>
    public static ProseWriterScope Field<T>(
      this IProseWriter writer,
      string path,
      string name,
      IDatatype<T> datatype,
      string description = null,
      string tooltip = null,
      bool fullWidth = false,
      bool hideName = false,
      Length? labelWidth = null
    ) {
      var scope = writer.Scope(new ProseField<T>(path, name, datatype));
      if (fullWidth) writer.Push(ProseFields.FullWidth);
      if (hideName) writer.Push(ProseModifiers.HideName);
      if (labelWidth.HasValue) writer.Push(ProseFields.LabelWidth(labelWidth.Value));
      if (description != null) writer.FieldDescription(description);
      if (tooltip != null) writer.FieldTooltip(tooltip);
      return scope;
    }

    /// <summary>A field with no nested prose, written in one statement.</summary>
    public static void WriteField<T>(
      this IProseWriter writer,
      string path,
      string name,
      IDatatype<T> datatype,
      string description = null,
      string tooltip = null,
      bool fullWidth = false,
      bool hideName = false,
      Length? labelWidth = null
    ) {
      using (writer.Field(path, name, datatype, description, tooltip, fullWidth, hideName, labelWidth)) { }
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