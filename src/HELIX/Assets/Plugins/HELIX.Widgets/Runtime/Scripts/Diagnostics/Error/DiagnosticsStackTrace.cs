using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Error {
  public class DiagnosticsStackTrace : DiagnosticsBlock {
    public DiagnosticsStackTrace(
      string name,
      string stackTrace,
      bool userReduced = false,
      DiagnosticsTreeStyle style = DiagnosticsTreeStyle.Flat,
      DiagnosticLevel level = DiagnosticLevel.Error
    )
      : base(
        name,
        style,
        level: level,
        children: BuildLines(stackTrace, userReduced)
      ) {
      StackTraceText = stackTrace ?? string.Empty;
    }

    public string StackTraceText { get; }

    private static List<DiagnosticsNode> BuildLines(string stackTrace, bool userReduced) {
      if (string.IsNullOrEmpty(stackTrace)) return new List<DiagnosticsNode>();

      var children = stackTrace
        .Replace("\r\n", "\n")
        .Replace('\r', '\n')
        .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => {
            var v = x.Trim();
            if (v.StartsWith("at")) v = v[2..].TrimStart();
            return v;
          }
        )
        .Where(x => !userReduced || !(x.StartsWith("UnityEditor.") || x.StartsWith("UnityEngine.UIElements.") ||
                                      x.StartsWith("HELIX.Widgets.") ||
                                      x.StartsWith("System.Environment.get_StackTrace"))
        )
        .Select((x, i) => '#' + i.ToString(CultureInfo.InvariantCulture).PadRight(2, ' ') + "  " + x)
        .Select(line => Message(
            line,
            DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel.Error,
            false
          )
        )
        .ToList();
      return children;
    }
  }
}