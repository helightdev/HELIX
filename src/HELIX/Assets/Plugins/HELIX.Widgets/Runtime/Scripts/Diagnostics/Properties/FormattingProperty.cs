using System;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
  public class FormattingProperty<T> : DiagnosticsProperty<T> {
    private readonly Func<T, string> _formatter;

    public FormattingProperty(
      string name,
      T value,
      Func<T, string> formatter,
      string description = null,
      string ifNull = null,
      string ifEmpty = null,
      bool showName = true,
      bool showSeparator = true,
      object defaultValue = null,
      string tooltip = null,
      bool missingIfNull = false,
      string linePrefix = null,
      bool expandableValue = false,
      bool allowWrap = true,
      bool allowNameWrap = true,
      DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
      DiagnosticLevel level = DiagnosticLevel.Info
    ) : base(
      name,
      value,
      description,
      ifNull,
      ifEmpty,
      showName,
      showSeparator,
      defaultValue,
      tooltip,
      missingIfNull,
      linePrefix,
      expandableValue,
      allowWrap,
      allowNameWrap,
      style,
      level
    ) {
      _formatter = formatter;
    }

    public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
      return ValueTyped == null ? IfNull ?? "null" : _formatter(ValueTyped);
    }
  }
}