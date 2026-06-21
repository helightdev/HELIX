using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HELIX.Diagnostics;
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

      var diagnosticsStackTrace = exception.Diagnostics
        .OfType<DiagnosticsStackTrace>().FirstOrDefault()?.ToStringDeep();
      if (diagnosticsStackTrace != null) {
        UnityConsoleLogBridge.Log(level, presented, diagnosticsStackTrace);
      } else {
        switch (level) {
          case <= DiagnosticLevel.Info: Debug.Log(presented); break;
          case <= DiagnosticLevel.Warning: Debug.LogWarning(presented); break;
          case <= DiagnosticLevel.Error: Debug.LogError(presented); break;
        }
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

  public static class UnityConsoleLogBridge {
    private delegate void LogCompilerVariant(
      string message,
      string fileName,
      int lineNumber,
      int columnNumber
    );

    private static LogCompilerVariant _logCompilerError;
    private static LogCompilerVariant _logCompilerWarning;
    private static LogCompilerVariant _logCompilerInfo;

    private static object _defaultEntityId;

    static UnityConsoleLogBridge() {
      var warningMethod = typeof(Debug).GetMethod(
        "LogCompilerWarning",
        BindingFlags.Static | BindingFlags.NonPublic
      );
      if (warningMethod != null) {
        _logCompilerWarning = (LogCompilerVariant)Delegate.CreateDelegate(
          typeof(LogCompilerVariant),
          warningMethod,
          false
        );
      }

      var infoMethod = typeof(Debug).GetMethod(
        "LogInformation",
        BindingFlags.Static | BindingFlags.NonPublic
      );
      if (infoMethod != null) {
        _logCompilerInfo = (LogCompilerVariant)Delegate.CreateDelegate(
          typeof(LogCompilerVariant),
          infoMethod,
          false
        );
      }

      var errorMethod = typeof(Debug).GetMethod(
        "LogCompilerError",
        BindingFlags.Static | BindingFlags.NonPublic
      );
      if (errorMethod != null) {
        _logCompilerError = (LogCompilerVariant)Delegate.CreateDelegate(
          typeof(LogCompilerVariant),
          errorMethod,
          false
        );
      }
    }

    private static bool TryExtractLine(string stackTrace, out string fileName, out int lineNumber) {
      if (stackTrace == null) {
        fileName = null;
        lineNumber = 0;
        return false;
      }

      var match = Regex.Match(stackTrace, @"in\s+(.*?):+(\d+)");
      if (match.Success) {
        fileName = match.Groups[1].Value.Trim();
        lineNumber = int.Parse(match.Groups[2].Value);
        return true;
      }
      fileName = null;
      lineNumber = 0;
      return false;
    }

    public static void Log(DiagnosticLevel level, string message, string stackTrace) {
      TryExtractLine(stackTrace, out var fileName, out var lineNumber);
      switch (level) {
        case <= DiagnosticLevel.Info:
          _logCompilerInfo(message, fileName, lineNumber, 0);
          break;
        case <= DiagnosticLevel.Warning:
          _logCompilerWarning(message, fileName, lineNumber, 0);
          break;
        case <= DiagnosticLevel.Error:
          _logCompilerError(message, fileName, lineNumber, 0);
          break;
      }
    }
  }
}