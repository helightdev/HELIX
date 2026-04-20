using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
  public class IterableProperty<T> : DiagnosticsProperty<IEnumerable<T>> {
    public IterableProperty(
      string name,
      IEnumerable<T> value,
      object defaultValue = null,
      string ifNull = null,
      string ifEmpty = "[]",
      DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
      bool showName = true,
      bool showSeparator = true,
      bool identityOnly = false,
      DiagnosticLevel level = DiagnosticLevel.Info
    )
      : base(
        name,
        value,
        null,
        ifNull,
        ifEmpty,
        showName,
        showSeparator,
        defaultValue,
        null,
        false,
        null,
        false,
        true,
        true,
        style,
        level
      ) {
      IdentityOnly = identityOnly;
    }

    public bool IdentityOnly { get; set; }

    public override DiagnosticLevel Level {
      get {
        if (IfEmpty == null && ValueTyped != null && !ValueTyped.Any() && base.Level != DiagnosticLevel.Hidden)
          return DiagnosticLevel.Fine;
        return base.Level;
      }
    }

    public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
      if (ValueTyped == null) return "null";

      var list = ValueTyped.ToList();
      if (list.Count == 0) return IfEmpty ?? "[]";

      var formatted = list.Select(v => FormatItem(v, parentConfiguration));

      if (parentConfiguration != null && !parentConfiguration.LineBreakProperties)
        return "[" + string.Join(", ", formatted) + "]";

      return string.Join(IsSingleLine(Style) ? ", " : "\n", formatted);
    }

    protected virtual string FormatItem(T v, TextTreeConfiguration parentConfiguration) {
      if (IdentityOnly) return v.DescribeIdentity();
      return v is float d ? FloatProperty.DebugFormatFloat(d) : v?.ToString() ?? "null";
    }
  }
}