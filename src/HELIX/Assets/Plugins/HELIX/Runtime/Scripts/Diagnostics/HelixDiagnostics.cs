using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HELIX.Diagnostics.Error;
using UnityEngine;

namespace HELIX.Diagnostics {
  public static class HelixDiagnostics {
    public static Func<HelixDiagnosticException, string> PresentError = error => error.ToStringDeep();
    public static event Action<HelixDiagnosticException, DiagnosticLevel> OnErrorReported;

    // Fixed log content marker used to detect if a log message if from HELIX as to prevent recursion
    public static string LogSignature = "<size=0>HELIX-8a9f0aac-e33d-4246-b349-56a4a5cc6d66</size>";

    public static string FormatException(Exception exception, string summary = null) {
      if (exception is HelixDiagnosticException diagnosticsException) return PresentError(diagnosticsException);
      return PresentError(HelixDiagnosticException.FromException(exception, summary));
    }

    public static HelixDiagnosticException Build(
      string summary,
      string description = null,
      IEnumerable<DiagnosticsNode> details = null,
      Exception exception = null,
      string stackTrace = null,
      IEnumerable<DiagnosticsNode> hints = null
    ) {
      var parts = new List<DiagnosticsNode> { new ErrorSummary(summary) };

      if (!string.IsNullOrEmpty(description)) parts.Add(new ErrorDescription(description));

      parts.Add(new ErrorSpacer());

      if (details != null) parts.AddRange(details);

      if (hints != null) {
        if (parts.Last() is not ErrorSpacer) parts.Add(new ErrorSpacer());
        parts.AddRange(hints);
      }

      if (exception != null) {
        if (parts.Last() is not ErrorSpacer) parts.Add(new ErrorSpacer());
        parts.Add(new ExceptionThrownErrorProperty(exception));
        parts.Add(new ErrorSpacer());
        parts.Add(new DiagnosticsStackTrace("StackTrace", exception.StackTrace));
      } else if (!string.IsNullOrEmpty(stackTrace)) {
        if (parts.Last() is not ErrorSpacer) parts.Add(new ErrorSpacer());
        parts.Add(new DiagnosticsStackTrace("StackTrace", stackTrace));
      }

      return new HelixDiagnosticException(parts);
    }

    public static HelixDiagnosticException Build(
      string summary,
      Action<InformationCollector> collectInformation,
      Exception exception = null,
      string stackTrace = null
    ) {
      var collector = new InformationCollector();
      collectInformation?.Invoke(collector);

      var parts = new List<DiagnosticsNode> { new ErrorSummary(summary) };

      parts.AddRange(collector.Collect());

      if (exception != null) {
        if (parts.Last() is not ErrorSpacer) parts.Add(new ErrorSpacer());
        parts.Add(new ExceptionThrownErrorProperty(exception));
        parts.Add(new ErrorSpacer());
        parts.Add(new DiagnosticsStackTrace("StackTrace", exception.StackTrace));
      } else if (!string.IsNullOrEmpty(stackTrace)) {
        if (parts.Last() is not ErrorSpacer) parts.Add(new ErrorSpacer());
        parts.Add(new DiagnosticsStackTrace("StackTrace", stackTrace));
      }

      return new HelixDiagnosticException(parts);
    }

    public static void Throw(
      string summary,
      string description = null,
      IEnumerable<DiagnosticsNode> details = null,
      Exception exception = null,
      string stackTrace = null,
      IEnumerable<DiagnosticsNode> hints = null
    ) {
      throw Build(summary, description, details, exception, stackTrace, hints);
    }

    public static void Report(this HelixDiagnosticException exception, DiagnosticLevel level) {
      var requiredLevel = DiagnosticLevel.Error;
#if UNITY_EDITOR
      requiredLevel = DiagnosticLevel.Debug;
#elif DEVELOPMENT_BUILD
            requiredLevel = DiagnosticLevel.Info;
#endif
      if (level < requiredLevel) return;

      OnErrorReported?.Invoke(exception, level);
      var presented = PresentError(exception);
      presented += LogSignature;
      switch (level) {
        case <= DiagnosticLevel.Info: Debug.Log(presented); break;
        case <= DiagnosticLevel.Warning: Debug.LogWarning(presented); break;
        case <= DiagnosticLevel.Error: Debug.LogError(presented); break;
      }
    }

    public static string ShortHash(this object obj) {
      if (obj == null) return "00000";
      var v = unchecked((uint)obj.GetHashCode()) & 0xFFFFF;
      return v.ToString("x5", CultureInfo.InvariantCulture);
    }

    public static string DescribeIdentity(this object obj) {
      return (obj == null ? "<null>" : obj.GetType().Name) + "#" + obj.ShortHash();
    }

    public static string ToStringNullable(this object obj) {
      return obj == null ? "<null>" : obj.ToString();
    }
  }
}