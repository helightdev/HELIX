using System;

namespace HELIX.Prose {
  /// <summary>
  /// Describes an editable or invokable value. The optional role is a semantic hint such as "number", "choice",
  /// or "action"; it does not prescribe a widget. Writers may reject or render the control as ordinary prose.
  /// </summary>
  public class ProseControl : IProseScope {
    public ProseControl(string role = null) => Role = role ?? string.Empty;
    public string Role { get; }
  }

  /// <summary>A conventional descriptive part of a control.</summary>
  public class ProseControlPart : IProseScope {
    public ProseControlPart(string role) {
      if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("A control part role is required.", nameof(role));
      Role = role;
    }

    public string Role { get; }
  }

  /// <summary>
  /// Named prose content that a supporting writer may place around a materialized control. Names describe intent,
  /// for example "prefix", "suffix", "before", or a domain-specific decorator location.
  /// </summary>
  public class ProseControlSlot : IProseScope {
    public ProseControlSlot(string name) {
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A control slot name is required.", nameof(name));
      Name = name;
    }

    public string Name { get; }
  }

  /// <summary>Open-ended control metadata. Its meaning and value type are agreed upon by producers and writers.</summary>
  public class ProseControlPropertyModifier : IProseModifier {
    public ProseControlPropertyModifier(string name, object value) {
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A control property name is required.", nameof(name));
      Name = name;
      Value = value;
    }

    public string Name { get; }
    public object Value { get; }
  }

  /// <summary>Optional comparable bounds. Null members mean that the corresponding constraint is unspecified.</summary>
  public sealed class ProseControlRangeModifier : IProseModifier {
    public ProseControlRangeModifier(object min = null, object max = null, object step = null) {
      Min = min;
      Max = max;
      Step = step;
    }

    public object Min { get; }
    public object Max { get; }
    public object Step { get; }
  }

  /// <summary>A pattern constraint expressed independently of any particular regular-expression engine.</summary>
  public sealed class ProseControlPatternModifier : IProseModifier {
    public ProseControlPatternModifier(string pattern) {
      if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("A control pattern is required.", nameof(pattern));
      Pattern = pattern;
    }

    public string Pattern { get; }
  }

  /// <summary>Shared standard control parts and factories for optional control metadata.</summary>
  public static class ProseControls {
    public static readonly ProseControl Default = new();
    public static readonly ProseControlPart Label = new("label");
    public static readonly ProseControlPart Value = new("value");
    public static readonly ProseControlPart Description = new("description");
    public static readonly ProseControlPart Placeholder = new("placeholder");
    public static readonly ProseControlPart Choice = new("choice");

    public static ProseControlPropertyModifier Property(string name, object value) => new(name, value);
    public static ProseControlPropertyModifier Required(bool value = true) => new("required", value);
    public static ProseControlPropertyModifier ReadOnly(bool value = true) => new("read-only", value);
    public static ProseControlPropertyModifier DefaultValue(object value) => new("default", value);
    public static ProseControlPropertyModifier Unit(string value) => new("unit", value);
    public static ProseControlRangeModifier Range(object min = null, object max = null, object step = null) =>
      new(min, max, step);
    public static ProseControlPatternModifier Pattern(string value) => new(value);
  }

  public static class ProseControlWriterExtensions {
    public static ProseWriterScope Control(this IProseWriter writer, string role = null) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      return writer.Scope(string.IsNullOrEmpty(role) ? ProseControls.Default : new ProseControl(role));
    }

    public static bool TryBeginControl(this IProseWriter writer, string role = null) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      return writer.TryBeginFrame(string.IsNullOrEmpty(role) ? ProseControls.Default : new ProseControl(role));
    }

    public static ProseWriterScope ControlPart(this IProseWriter writer, string role) =>
      writer.Scope(new ProseControlPart(role));
    public static ProseWriterScope ControlLabel(this IProseWriter writer) => writer.Scope(ProseControls.Label);
    public static ProseWriterScope ControlValue(this IProseWriter writer) => writer.Scope(ProseControls.Value);
    public static ProseWriterScope ControlDescription(this IProseWriter writer) =>
      writer.Scope(ProseControls.Description);
    public static ProseWriterScope ControlPlaceholder(this IProseWriter writer) =>
      writer.Scope(ProseControls.Placeholder);
    public static ProseWriterScope ControlChoice(this IProseWriter writer) => writer.Scope(ProseControls.Choice);
    public static ProseWriterScope ControlSlot(this IProseWriter writer, string name) =>
      writer.Scope(new ProseControlSlot(name));

    public static void ControlLabel(this IProseWriter writer, string text) =>
      WriteControlPart(writer, ProseControls.Label, text);
    public static void ControlDescription(this IProseWriter writer, string text) =>
      WriteControlPart(writer, ProseControls.Description, text);
    public static void ControlPlaceholder(this IProseWriter writer, string text) =>
      WriteControlPart(writer, ProseControls.Placeholder, text);
    public static void ControlChoice(this IProseWriter writer, string text) =>
      WriteControlPart(writer, ProseControls.Choice, text);

    private static void WriteControlPart(IProseWriter writer, IProseScope scope, string text) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      using (writer.Scope(scope)) writer.Write(text);
    }
  }
}
